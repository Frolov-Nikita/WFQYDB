using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WFQYDB;
using Term;
using WFQYDB.Communication;

namespace WFQYDB_emu
{
    public class TcpEmu: IEmu
    {
        TcpServer tcpServer;

        CancellationTokenSource cts;
        List<WFQYDBemul> wFQYDBemuls = new List<WFQYDBemul>();

        public WFQYDBemul ProtoEmu => wFQYDBemuls.Count > 0 ? wFQYDBemuls[0] : null;

        public byte[] Id { get; set; } = { 1, 2, 3, 4 };
        
        public string ClassName => "TcpEmu"; // Для сериализации

        public int Port { get; set; } = 6789;

        ~TcpEmu()
        {
            Stop();
        }

        public void Start()
        {
            cts = new CancellationTokenSource();
            tcpServer = new TcpServer(Port);
            tcpServer.OnClientConnected += Ts_OnClientConnected;
            tcpServer.Start(cts.Token);
        }

        private void Ts_OnClientConnected(object sender, TcpServerStreamSource e)
        {
            var emu = new WFQYDBemul(e, new byte[] { 1, 2, 3, 4 }, cts.Token);
            emu.Start();
            wFQYDBemuls.Add(emu);
            emu.Dead += (sender, args) => wFQYDBemuls.Remove((WFQYDBemul)sender);
        }

        public void Stop()
        {
            cts.Cancel();
            tcpServer.Stop();
            foreach (var emu in wFQYDBemuls)
                emu.Stop();
            wFQYDBemuls.Clear();
        }

        public TermView GetInfo()
        {
            var view = new TermView { FloatDirection = FloatDirection.Vertical }
                .Add($"Connection: anyip:{Port}  clients: {wFQYDBemuls.Count}");
            var emulsView = new TermView { FloatDirection = FloatDirection.Horisontal };

            foreach (var emu in wFQYDBemuls)
                emulsView.Add(emu.GetInfo());

            view.Add(emulsView);
            return view;
        }
    }
}
