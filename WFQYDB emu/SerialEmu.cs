using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WFQYDB;
using Term;
using WFQYDB.Communication;

namespace WFQYDB_emu
{
    public class SerialEmu : IEmu
    {
        CancellationTokenSource cts;

        public byte[] Id { get; set; } = { 1, 2, 3, 4 };

        public IStreamSourceConfig StreamSourceCfg { get; set; } = new SerialPortStreamSourceConfig { PortName = "COM3" };

        WFQYDBemul emu;

        public string ClassName => "SerialEmu"; // Для сериализации

        public WFQYDBemul ProtoEmu => emu;

        ~SerialEmu()
        {
            Stop();
        }

        public void Start()
        {
            cts = new CancellationTokenSource();

            emu = new WFQYDBemul(StreamSourceCfg.Get(), Id, cts.Token);
            emu.Start();
        }

        public void Stop()
        {
            cts.Cancel();
            emu.Stop();
        }

        public TermView GetInfo()
        {
            var view = new TermView { FloatDirection = FloatDirection.Vertical }
                .Add($"Connection: {StreamSourceCfg.ToString()}")
                .Add(emu.GetInfo());

            return view;
        }

    }
}
