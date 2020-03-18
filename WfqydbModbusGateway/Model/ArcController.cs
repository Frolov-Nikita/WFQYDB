using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WFQYDB;

namespace WfqydbModbusGateway.Model
{
    /// <summary>
    /// Управление базой
    /// </summary>
    public class ArcController : ServiceAbstract
    {
        readonly string fileName;
        static readonly TimeSpan minSleepTime = new TimeSpan(1_000_000); // 100 msec
        ArcRecord lastArcRecord;

        public bool AllowMessageLogging { get; set; } = false;

        List<ArcRecord> arcRecords = new List<ArcRecord>(32);
        List<LogRecord> logRecords = new List<LogRecord>(16);

        public ArcController(string fileName = nameof(ArcData) + ".sqlite3")
        {
            this.fileName = fileName;
        }
        
        /// <summary>
        /// Период периодического архивирования
        /// </summary>
        public int PeriodSec { get; set; } = 60;

        /// <summary>
        /// Максимальное количество записей, которые должны храниться. 
        /// Более старые записи удаляютсяпериодически. 
        /// Кол-во записей иногда будет превышать этот лимит.
        /// 10_000_000 * 6 * 4 = 240_000_000;
        /// 240_000_000 / 1024 / 1024 = ~230 Мб
        /// 
        /// 2_545_928 байт 100_000 записей => 1 запись ~25.46 байт
        /// 2_545_928 * 10_000_000 / 100_000 = 254_592_800
        /// 254_592_800 / 1024 / 1024 = 242,8 Мб
        /// 10_000_000 / 1 / 60 / 24 = 19 лет , если 1 раз в минуту и без учета срабатываний по гистерезису.
        /// </summary>
        public int MaxDataLimit { get; set; } = 2_629_800; // ~5 лет, ~67 Мб

        public int MaxLogLimit { get; set; } = 4096; // ~5 лет, ~67 Мб

        /// <summary>
        /// Гистерезис значений в абсолютных единицах.
        /// </summary>
        public byte Hyst { get; set; } = 1;

        /// <summary>
        /// Период сохранения
        /// </summary>
        public TimeSpan SavePeriod { get; set; } = new TimeSpan(0, 0, 10);

        /// <summary>
        /// Обработка новых данных. Данные добавятся в БД, если они изменились, или истек период архивирования.
        /// </summary>
        /// <param name="msi"></param>
        /// <returns></returns>
        public void NewData(MessageStoryItem msi)
        {
            var newArcRecord = new ArcRecord(msi);

            bool outOfHyst(byte a, byte b) => (a != b) && ((a - b) >= Hyst) || ((b - a) >= Hyst);

            var needToArc = (lastArcRecord == null) ||
                             (lastArcRecord.Status != newArcRecord.Status) ||
                             outOfHyst(lastArcRecord.UpFreq, newArcRecord.UpFreq) ||
                             outOfHyst(lastArcRecord.DnFreq, newArcRecord.DnFreq) ||
                             outOfHyst(lastArcRecord.StokeRate, newArcRecord.StokeRate) ||
                             outOfHyst(lastArcRecord.StokeLength, newArcRecord.StokeLength);

            if (!needToArc)
            {
                var dt = newArcRecord.UnixTimestamp - lastArcRecord.UnixTimestamp;
                needToArc = (dt >= PeriodSec);
            }

            if (needToArc)
            {
                arcRecords.Add(newArcRecord);
                lastArcRecord = newArcRecord;
            }
        }

        /// <summary>
        /// Логирование события
        /// </summary>
        /// <param name="code">Код события</param>
        /// <param name="data">Дополнительные данные</param>
        /// <returns></returns>
        public void NewLogMessage(LogCode code, string data = "")
        {
            if (!AllowMessageLogging)
                return;

            logRecords.Add(new LogRecord
            {
                UnixTimestampMs = DateTime.Now.ToUnixTimestampMs(),
                Code = code,
                Data = data == default ? null : data
            });
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            var savedItems = 0;

            using (var db = new ArcData(fileName))
            {
                if(!db.LogCodes.Any())
                    db.UpsertLogCodes();
                db.Dispose();
            }
                
            while (!cancellationToken.IsCancellationRequested)
            {
                var tNext = DateTime.Now + SavePeriod;
                
                if ((logRecords.Count > 0) || (arcRecords.Count > 0))
                {
                    var db = new ArcData(fileName);

                    if (logRecords.Count > 0)
                    {
                        savedItems += logRecords.Count;
                        await db.Logs.AddRangeAsync(logRecords,cancellationToken).ConfigureAwait(false);
                    }
                    
                    if (arcRecords.Count > 0)
                    {
                        savedItems += arcRecords.Count;
                        await db.Data.AddRangeAsync(arcRecords, cancellationToken).ConfigureAwait(false);
                    }

                    db.SaveChanges();

                    // Обслуживание. очистка начала журнала и сжатие БД
                    if (savedItems > 512)
                    {
                        savedItems = 0;

                        var currDataCount = db.Data.Count();
                        var currLogCount = db.Logs.Count();
                        var saveAndClean = false;

                        if (currDataCount > MaxDataLimit)
                        {
                            var dataItemsToDelete = db.Data.OrderBy(d => d.UnixTimestamp).Take(MaxDataLimit - currDataCount);
                            db.Data.RemoveRange(dataItemsToDelete);
                            saveAndClean = true;
                        }

                        if (currLogCount > MaxLogLimit)
                        {
                            var logItemsToDelete = db.Data.OrderBy(d => d.UnixTimestamp).Take(MaxLogLimit - currLogCount);
                            db.Data.RemoveRange(logItemsToDelete);
                            saveAndClean = true;
                        }

                        if (saveAndClean)
                        {
                            // Тут cancellationToken не использован намеренно
                            await db.SaveChangesAsync().ConfigureAwait(false);
                            await db.Database.ExecuteSqlRawAsync("VACUUM ;").ConfigureAwait(false);
                        }
                    }
                    
                    db.Dispose();
                    logRecords.Clear();
                    arcRecords.Clear();
                }
                
                var sleepTime = tNext > DateTime.Now ? tNext - DateTime.Now : minSleepTime;
                await Task.Delay(sleepTime, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
