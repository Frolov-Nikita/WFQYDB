using System;

namespace WfqydbModbusGateway.Model
{
    public class ConverterConfig
    {
        // SerialPortStreamSource; COM1; 9600; 8; none; 1
        // TcpClientStreamSource; 127.0.0.1:6789
        public string WFQYDBConnection { get; set; } = "TcpClientStreamSource; 127.0.0.1:6789";

        public int ModbusSlaveTcpPort { get; set; } = 502;

        public int ModbusSlaveRtuOverTcpPort { get; set; } = 503;

        public string ArcFileName { get; set; } = "ArcData.sqlite3";

        // 0 - listen only
        public int PeriodSec { get; set; } = 600;

        // Разрешенные команды. команда 0xA4 разрешена всегда (конвертер её только принимает).
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Свойства не должны возвращать массивы", Justification = "<Ожидание>")]
        public byte[] AllowedCommands { get; set; } = { 0xA0, 0xA1, 0xA2, 0xA3 };

        // maximum count of records in arch db
        public int MaxLimit { get; set; } = 10_000_000;

        public byte Hyst { get; set; } = 1;

        public TimeSpan SavePeriod { get; set; } = new TimeSpan(0, 1, 0);

        public bool AllowMessageLogging { get; set; } = false;
    }
}
