using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using WFQYDB;
using Term;

namespace WFQYDB_emu
{
    class Program
    {
        static CancellationTokenSource cts;

        static IEmu emu;

        static void Main(string[] args)
        {
            var cfgFileName = args.Length > 0 ? args[0] : "EmuCfg.json";
            emu = FactoryEmu.Get(cfgFileName);
            FactoryEmu.Save(emu, cfgFileName);
            emu.Start();

            cts = new CancellationTokenSource();

            var term = Term.Term.Instance;
            term.Title = "Эмулятор  WFQYDB";
            term.OnKeyPressed += Term_OnKeyPressed;
            term.CancelKeyPress += Term_CancelKeyPress;

            Task.Run(async () => 
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    //term.RenderFullFull(emu.GetInfo());
                    //term.RenderFull(emu.GetInfo());
                    term.Render(emu.GetInfo());
                    await Task.Delay(1_000, cts.Token);
                }
            }, cts.Token).Wait();
        }

        private static void Term_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            cts?.Cancel();
        }

        private static void Term_OnKeyPressed(object sender, ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.KeyChar)
            {
                case 'z':
                    emu.ProtoEmu.Story.Clear();
                    break;
                case 'q':
                    emu.ProtoEmu.Status.Start = !emu.ProtoEmu.Status.Start;
                    break;
                case 'w':
                    emu.ProtoEmu.Status.ShortCircuit = !emu.ProtoEmu.Status.ShortCircuit;
                    break;
                case 'e':
                    emu.ProtoEmu.Status.OverTemperature = !emu.ProtoEmu.Status.OverTemperature;
                    break;
                case 'r':
                    emu.ProtoEmu.Status.OverLoad = !emu.ProtoEmu.Status.OverLoad;
                    break;

                case 'u':
                    var uf = emu.ProtoEmu.UpFreq + 1;
                    uf = uf > 15 ? 6 : uf;
                    emu.ProtoEmu.UpFreq = (byte)uf;
                    break;
                case 'd':
                    var df = emu.ProtoEmu.DnFreq + 1;
                    df = df > 24 ? 15 : df;
                    emu.ProtoEmu.UpFreq = (byte)df;
                    break;
                case 's':
                    var sl = emu.ProtoEmu.StokeLength + 10;
                    sl = sl > 123 ? 10 : sl;
                    emu.ProtoEmu.StokeLength = (byte)sl;
                    break;
                case 'f':
                    var sr = emu.ProtoEmu.StokeRate + 10;
                    sr = sr > 60 ? 10 : sr;
                    emu.ProtoEmu.StokeRate = (byte)sr;
                    break;
                default:
                    break;
            }
        }
    }
}
