using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RJCP.IO.Ports;

namespace WFQYDB.Communication
{
    public class SerialPortStreamSource : IStreamSource
    {
        SerialPortStream stream;

        public string PortName { get; set; } = "COM1";

        public int BaudRate { get; set; } = 9600;

        public int DataBits { get; set; } = 8;

        public Parity Parity { get; set; } = Parity.None;

        public StopBits StopBits { get; set; } = StopBits.One;

        public int Aviable => stream?.BytesToRead ?? 0;

        public bool IsOpen => stream?.IsOpen ?? false;

        public int ReadTimeout { get; set; } = -1;

        public int WriteTimeout { get; set; } = -1;

        public async Task<Stream> GetStreamAsync()
        {
            return await Task.Run<Stream>(() =>
            {
                if ((stream == null) || (!stream.IsOpen))
                {
                    stream?.Close();
                    stream?.Dispose();
                    Logger.Info($"Creating SerialPortStream {PortName} {BaudRate}");
                    stream = new SerialPortStream(PortName, BaudRate, DataBits, Parity, StopBits);
                    //{ ReadTimeout = ReadTimeout, WriteTimeout = WriteTimeout };
                }
                if (!stream.IsOpen)
                    stream.Open();
                return stream;
            });
        }

        public async Task<Stream> GetStreamAsync(CancellationToken cancellationToken)
        {
            return await Task.Run<Stream>(() =>
            {
                if (stream == null)
                    stream = new SerialPortStream(PortName, BaudRate, DataBits, Parity, StopBits)
                    { ReadTimeout = ReadTimeout, WriteTimeout = WriteTimeout };
                if (!stream.IsOpen)
                    stream.Open();
                return stream;
            }, cancellationToken);
        }


        public void Close()
        {
            stream.Close();
            stream.Dispose();
            stream = null;
        }

    }
}
