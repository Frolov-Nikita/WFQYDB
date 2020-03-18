using ModbusBasic;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using WFQYDB;

namespace WfqydbModbusGateway.Model
{
    enum CmdState : ushort
    {
        Rdy = 0,
        Working = 1,
        CommonError = 100,
        NotImplementedCmd = 101,
    }

    enum Cmd : ushort
    {
        NoOp = 0x00,
        Reset = 0x0A,
        BroadcastQuery = 0xA0,
        AutoRun = 0xA2,
        Shutdown = 0xA3,
        ThrowException = 0xFA,
    }

    public class ModbusSvr : ServiceAbstract
    {
        public ModbusSvr(int tcpPort, int rtuOverTcpPort, WFQYDBServer wf)
        {
            TcpPort = tcpPort;
            RtuOverTcpPort = rtuOverTcpPort;
            WfqydbServer = wf;
            if(wf == default)
                throw new ArgumentException("Arg WFQYDBServer mast not be null.");
            //wf.PropertyChanged += Wf_PropertyChanged;
        }

        public ArcController ArcController { get; set; }

        public int TcpPort { get; private set; } = 502;

        public int RtuOverTcpPort { get; private set; } = 503;
        
        private readonly ushort[] currentData = new ushort[13];
        private readonly ushort[] sampledData = new ushort[500];

        private IPointSource<ushort> iRegisters;
        private IPointSource<ushort> hRegisters;

        public event ErrorEventHandler OnError;

        //public TimeSpan UpdatePeriod { get; set; } = new TimeSpan(0, 0, 20);

        public WFQYDBServer WfqydbServer { get; set; }

        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            ArcController?.NewLogMessage(LogCode.ModbusStarting);

            Logger.Info("begin");

            var tcpListener = new TcpListener(IPAddress.Any, TcpPort);
            var rtuListener = new TcpListener(IPAddress.Any, RtuOverTcpPort);

            Logger.Info("starting listener");

            tcpListener.Start();
            rtuListener.Start();

            Logger.Info("generating slave net");

            var factory = new ModbusFactory();

            var tcpNetwork = factory.CreateSlaveNetwork(tcpListener);
            var rtuNetwork = factory.CreateRtuOverTcpSlaveNetwork(rtuListener);

            var slaveTcp = factory.CreateSlave(1);
            var slaveRtu = factory.CreateSlave(1, slaveTcp.DataStore);

            tcpNetwork.AddSlave(slaveTcp);
            rtuNetwork.AddSlave(slaveRtu);

            iRegisters = slaveTcp.DataStore.InputRegisters;
            hRegisters = slaveTcp.DataStore.HoldingRegisters;
                       
            WfqydbServer.PropertyChanged += Wf_PropertyChanged;

            var timeOfStart = DateTime.Now.ToUnixTimestamp();
            ushort[] timeOfStartBuffer = {
                (ushort)(timeOfStart >> 16),
                (ushort)(timeOfStart ),
            };

            var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            ushort[] verBuffer = {
                (ushort)ver.Major,
                (ushort)ver.Minor,
                (ushort)ver.Build,
            };

            byte[] myAddress = { 0x01, 0x00, 0x00, 0x00, };

            iRegisters.WritePoints(0, verBuffer);
            iRegisters.WritePoints(3, timeOfStartBuffer);

            Logger.Info("starting slave net. step1");

            var tcpListenerTask = tcpNetwork.ListenAsync(cancellationToken);
            var rtuListenerTask = rtuNetwork.ListenAsync(cancellationToken);

            Logger.Info($"starting slave net. step2 {WfqydbServer.ToString()}");

            ArcController?.NewLogMessage(LogCode.ModbusStarted);
            ArcController?.NewLogMessage(LogCode.WFQYDBConnecting);

            //await WfqydbServer.PullBroadcastQuery().ConfigureAwait(false);
            //WfqydbServer.PullBroadcastQuery();
            ArcController?.NewLogMessage(LogCode.WFQYDBConnected);

            const int waitTime = 100; //ms
            const int faultRestTime = 10_000; //ms
            const int responseTime = 500; //ms
            var lastRequestTime = DateTime.Now;
            var lastResetTime = DateTime.Now;
            var resetInterval = TimeSpan.FromSeconds(30);

            var faultRestTimer = 0;

            Logger.Info("entering inf loop");
            // Главный цикл до отмены этой задачи.
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // состояние связи 
                    iRegisters.WritePoints(5, new ushort[] { (ushort)WfqydbServer.ConnectionState });
                    // Habdle

                    var buff = hRegisters.ReadPoints(10, 5);
                    var cmd = buff[0];
                    var cmdState = buff[1];

                    if ((buff[2] + buff[3]) == 0)
                    {
                        // Чтобы не было пустого места на элкамрм
                        hRegisters.WritePoints(12, new ushort[] { currentData[3], currentData[4], currentData[6] });
                    }

                    // обработка команд по модбас
                    if ((cmd > 0) && (cmdState == (ushort)CmdState.Rdy))
                    {
                        var upFreq = (byte)buff[2];
                        var dnFreq = (byte)buff[3];
                        var stokeRate = (byte)buff[4];

                        hRegisters.WritePoints(10, new ushort[] { cmd, (ushort)CmdState.Working });

                        try
                        {
                            switch ((Cmd)cmd)
                            {
                                case Cmd.Reset:
                                    cmd = 0;
                                    cmdState = 0;
                                    hRegisters.WritePoints(10, new ushort[] { (ushort)Cmd.NoOp, (ushort)CmdState.Rdy });
                                    break;
                                case Cmd.BroadcastQuery:
                                    lastRequestTime = DateTime.Now;
                                    //await WfqydbServer.PullBroadcastQuery().ConfigureAwait(false);
                                    WfqydbServer.PullBroadcastQuery();
                                    hRegisters.WritePoints(10, new ushort[] { (ushort)Cmd.NoOp, (ushort)CmdState.Rdy });

                                    ArcController?.NewLogMessage(LogCode.WFQYDBCmdRead);
                                    break;
                                case Cmd.AutoRun:
                                    lastRequestTime = DateTime.Now;
                                    //await WfqydbServer.PullAutoRun(upFreq, dnFreq, stokeRate).ConfigureAwait(false);
                                    WfqydbServer.PullAutoRun(upFreq, dnFreq, stokeRate);
                                    hRegisters.WritePoints(10, new ushort[] { (ushort)Cmd.NoOp, (ushort)CmdState.Rdy });

                                    ArcController?.NewLogMessage(LogCode.WFQYDBCmdCtart, $"u:{upFreq}, d:{dnFreq}, sr:{stokeRate}");
                                    break;
                                case Cmd.Shutdown:
                                    lastRequestTime = DateTime.Now;
                                    //await WfqydbServer.PullShutdown().ConfigureAwait(false);
                                    WfqydbServer.PullShutdown();
                                    hRegisters.WritePoints(10, new ushort[] { (ushort)Cmd.NoOp, (ushort)CmdState.Rdy });

                                    ArcController?.NewLogMessage(LogCode.WFQYDBCmdCtop);
                                    break;
                                case Cmd.ThrowException:
                                    throw new Exception("Test exception");

                                default:
                                    hRegisters.WritePoints(10, new ushort[] { cmd, (ushort)CmdState.NotImplementedCmd });
                                    faultRestTimer = faultRestTime;
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            hRegisters.WritePoints(10, new ushort[] { cmd, (ushort)CmdState.CommonError });
                            faultRestTimer = faultRestTime;
                            FireOnError(ex);

                            Logger.Alarm(ex.Message);
                            ArcController?.NewLogMessage(LogCode.ModbusFailed);
                        }
                    }

                    if (cmdState != (ushort)CmdState.Rdy)
                        if ((faultRestTimer -= waitTime) <= 0)
                            hRegisters.WritePoints(10, new ushort[] { (ushort)Cmd.NoOp, (ushort)CmdState.Rdy });

                    await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);

                    // передернем сеть rtu. Так как она периодически подвисает.
                    //var lastResetTime = DateTime.Now;
                    //var resetInterval = TimeSpan.FromSeconds(30);
                    if((lastResetTime + resetInterval) < DateTime.Now)
                    {
                        lastResetTime = DateTime.Now;
                        Logger.Info("restarting rtu");
                        //var rtuListener = new TcpListener(IPAddress.Any, RtuOverTcpPort);
                        rtuListener.Stop();
                        rtuListener.Start();
                        // var rtuNetwork = factory.CreateRtuOverTcpSlaveNetwork(rtuListener);
                        // var slaveRtu = factory.CreateSlave(1, slaveTcp.DataStore);
                        // rtuNetwork.AddSlave(slaveRtu);
                        // var rtuListenerTask = rtuNetwork.ListenAsync(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    FireOnError(ex);

                    Logger.Alarm(ex.Message);

                    ArcController?.NewLogMessage(LogCode.ModbusFailed);

                    await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
                }
            }// while

            tcpListener.Stop();
            rtuListener.Stop();

            tcpNetwork.Dispose();
            rtuNetwork.Dispose();

            await tcpListenerTask.ConfigureAwait(true);
            await rtuListenerTask.ConfigureAwait(true);
        }

        private void NewResponseHandle(MessageStoryItem response)
        {
            if ((response == null) && response.IsHeaderValid)
                return;

            var responseTimeStamp = response.DateTime.ToUnixTimestamp();

            var i = 0;
            currentData[i++] = (ushort)(responseTimeStamp >> 16);
            currentData[i++] = (ushort)(responseTimeStamp);
            currentData[i++] = (ushort)response.Command;

            // Если есть текущие данные
            if (response.DataLength >= 5)
                for (var j = 0; j < 5; j++)
                    currentData[i++] = response.Data[j];
            else
                for (var j = 0; j < 5; j++)
                    currentData[i++] = 0;

            iRegisters.WritePoints(10, currentData);
            
            // Если есть sampledData
            if (response.DataLength > 12)
            {
                var k = 0;
                sampledData[k++] = (ushort)(responseTimeStamp >> 16);
                sampledData[k++] = (ushort)(responseTimeStamp);
                sampledData[k++] = (ushort)(response.DataLength - 12);

                for (var j = 12; j < response.DataLength; j++)
                    sampledData[k++] = response.Data[j];

                for (; k < sampledData.Length;)
                    sampledData[k++] = 0;

                iRegisters.WritePoints(18, sampledData);
            }
        }

        private void Wf_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var wf = (WFQYDBServer)sender;
            switch (e.PropertyName)
            {
                case nameof(WFQYDBServer.LastResponse):
                    NewResponseHandle(wf.LastResponse);
                    break;
                default:
                    break;
            }
        }

        private void FireOnError(Exception ex) =>
            OnError?.Invoke(this, new ErrorEventArgs(ex));
    }
}
