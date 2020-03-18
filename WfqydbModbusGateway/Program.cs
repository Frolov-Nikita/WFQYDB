using ConsoleFramework;
using ConsoleFramework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WFQYDB;
using WFQYDB.Communication;
using WfqydbModbusGateway.Model;
using WfqydbModbusGateway.ViewModel;

namespace WfqydbModbusGateway
{
    class Program
    {
        static ArcController dbconrtoller;

        static void Main(string[] args)
        {
            Logger.Disabled = false;
            
            ThreadPool.SetMaxThreads(3, 3);

            var cfg = ConverterConfigFactory.Get();

            ConverterConfigFactory.Save(cfg);

            dbconrtoller = new ArcController(cfg.ArcFileName)
            {
                AllowMessageLogging = cfg.AllowMessageLogging,
                SavePeriod = cfg.SavePeriod,
                MaxDataLimit = cfg.MaxLimit,
                Hyst = cfg.Hyst
            };
            //dbconrtoller.Start();

            var cts = new CancellationTokenSource();
            var wCts = new CancellationTokenSource();
            
            var connnectionWfqydb = StreamSourceFactory.GetStreamSourceConfig(cfg.WFQYDBConnection).Get();

            var wFQYDBServer = new WFQYDBServer(
                connnectionWfqydb,
                new byte[] { 1, 0, 0, 0 },
                wCts.Token)
            {
                UpdatePeriod = new TimeSpan(0, 0, cfg.PeriodSec),
                CanA0 = cfg?.AllowedCommands.Contains<byte>(0xA0) ?? false,
                CanA1 = cfg?.AllowedCommands.Contains<byte>(0xA1) ?? false,
                CanA2 = cfg?.AllowedCommands.Contains<byte>(0xA2) ?? false,
                CanA3 = cfg?.AllowedCommands.Contains<byte>(0xA3) ?? false,
            };

            wFQYDBServer.PropertyChanged += WFQYDBServer_PropertyChanged;

            var modbusSvr = new ModbusSvr(
                cfg.ModbusSlaveTcpPort,
                cfg.ModbusSlaveRtuOverTcpPort,
                wFQYDBServer)
            {
                ArcController = dbconrtoller,
                //UpdatePeriod = new TimeSpan(cfg.PeriodSec * 10_000_000)
            };
            modbusSvr.Start();

            System.Threading.ThreadPool.GetMaxThreads(out int workerTh, out int completionTh);
            Logger.Info($"workerTh: {workerTh} completionTh: {completionTh}");

            if (args.Contains("TUI"))
            {
                Logger.Disabled = true;
                MainViewModel mvm = new MainViewModel();
                //mvm.ConnnectionWfqydb = connnectionWfqydb;
                mvm.WFQYDBServer = wFQYDBServer;
                mvm.ModbusSvr = modbusSvr;
                Window window = (Window)ConsoleApplication
                    .LoadFromXaml("WfqydbModbusGateway.View.MainWin.xml", mvm);
                WindowsHost windowsHost = new WindowsHost { Name = "temp" };
                windowsHost.Show(window);
                ConsoleApplication.Instance.Run(windowsHost);
            }
            else
            {
                int i = 0;

                while (!cts.Token.IsCancellationRequested)
                {
                    Thread.Sleep(200);
                    //if (wFQYDBServer.NeedToRestart)
                    //{
                    //    // restart services
                    //    Logger.Warn("Restarting services");
                    //    wCts.Cancel();
                    //    wCts = new CancellationTokenSource();
                    //    wFQYDBServer.PropertyChanged -= WFQYDBServer_PropertyChanged;
                    //    wFQYDBServer = new WFQYDBServer(connnectionWfqydb, new byte[] { 1, 0, 0, 0 }, cts.Token);
                    //    modbusSvr.WfqydbServer = wFQYDBServer;
                    //    GC.Collect();
                    //    wFQYDBServer.PropertyChanged += WFQYDBServer_PropertyChanged;
                    //}
                }
                wCts.Cancel();
                //Task.Delay(1000, cts.Token).Wait();
            }
            cts.Dispose();
            wCts.Dispose();
            dbconrtoller.Stop();

        }//Main

        private static void WFQYDBServer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var svr = (WFQYDBServer)sender;
            switch (e.PropertyName)
            {
                case nameof(WFQYDBServer.LastResponse):
                    if (svr.LastResponse.Dir == MessageStoryItem.Direction.Rx)
                        dbconrtoller?.NewData(svr.LastResponse);
                    break;
                default:
                    break;
            }
        }
    }
}
