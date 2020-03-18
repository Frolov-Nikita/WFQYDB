using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WfqydbModbusGateway.Model
{
    public enum LogCode
    {
        [Description("Сервер Modbus: Запуск")]
        ModbusStarting,
        [Description("Cервер Modbus: Запущен")]
        ModbusStarted,
        [Description("Cервер Modbus: Сбой")]
        ModbusFailed,
        [Description("Cервер Modbus: Подключен новый клиент")]
        ModbusClientConnected,

        [Description("Клиент WFQYDB: Подключение")]
        WFQYDBConnecting,
        [Description("Клиент WFQYDB: Подключено")]
        WFQYDBConnected,

        [Description("Клиент WFQYDB: Команда на чтение")]
        WFQYDBCmdRead,
        [Description("Клиент WFQYDB: Команда на запуск")]
        WFQYDBCmdCtart,
        [Description("Клиент WFQYDB: Команда на останов")]
        WFQYDBCmdCtop,
    }

    public class LogRecord
    {
        [Key]
        public int Id { get; set; }
        public Int64 UnixTimestampMs { get; set; }

        public LogCode Code { get; set; }

        public string Data { get; set; }

    }
}
