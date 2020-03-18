//#define DEBUG_SQL

using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;


namespace WfqydbModbusGateway.Model
{
    public class LogRecordCode
    {
        [Key]
        public int Id { get; set; }

        public int Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

#if DEBUG_SQL
    public class MyLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger();
        }

        public void Dispose() { }

        private class MyLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId,
                    TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                //File.AppendAllText("log.txt", formatter(state, exception));
                //Console.WriteLine(formatter(state, exception));
                System.Diagnostics.Debug.WriteLine(formatter(state, exception));
            }
        }
    }
#endif

    /// <summary>
    /// Контекст БД архива
    /// </summary>
    public class ArcData : DbContext
    {
        private readonly string fileName;

        public ArcData(string fileName = nameof(ArcData) + ".sqlite3")
        {
            this.fileName = fileName;
            Database.EnsureCreated();

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={fileName}");
#if DEBUG_SQL
            optionsBuilder.UseLoggerFactory(MyLoggerFactory);
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
                throw new ArgumentNullException(nameof(modelBuilder));

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LogRecord>()
                .Property(e => e.Code)
                .HasConversion(new ValueConverter<LogCode, int>(
                    v => (int)v,
                    v => (LogCode)v
                    ));

        }

        public DbSet<ArcRecord> Data { get; set; }

        public DbSet<LogRecord> Logs { get; set; }

        public DbSet<LogRecordCode> LogCodes { get; set; }

        public void UpsertLogCodes()
        {
            var t = typeof(LogCode);

            var enumItems = Enum.GetValues(t);

            var items = LogCodes.ToList();

            foreach (var i in enumItems)
            {
                var code = (int)i;

                var storedItem = items.FirstOrDefault(e=>e.Code == code);

                if (storedItem == default)
                {
                    storedItem = new LogRecordCode { Code = code };

                    LogCodes.Add(storedItem);
                }
                
                storedItem.Name = i.ToString();

                var item = t.GetMember(storedItem.Name)[0];
                var attrs = item.GetCustomAttributes(typeof(DescriptionAttribute), false);

                storedItem.Description = (attrs.Length > 0) ? ((DescriptionAttribute)attrs.First()).Description : "";
            }

           SaveChanges();
        }

#if DEBUG_SQL
        public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new MyLoggerProvider());    // указываем наш провайдер логгирования
        });
#endif
    }
}
