using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using WFQYDB.Communication;

namespace WFQYDB
{
    public enum ConnectionState
    {
        Unknown,
        NotConnected,
        Connecting,
        Connected,
        Fault,
    }

    public struct CommandConveyerItem
    {
        public Command cmd;
        public byte upFreq;
        public byte dnFreq;
        public byte stokeRate;
    }

    public class WFQYDBServer : INotifyPropertyChanged //TODO: ServiceAbstract
    {
        private readonly IStreamSource streamSource;
        private readonly CancellationToken cancellationToken;
        private MessageStoryItem lastResponse;
        private byte[] serverAddress = { 1, 0, 0, 0 };
        private byte[] destinationAddress = { 0, 0, 0, 1 };
        private ConnectionState connectionState = ConnectionState.Unknown;

        public bool CanA0 { get; set; } = true;
        public bool CanA1 { get; set; } = true;
        public bool CanA2 { get; set; } = true;
        public bool CanA3 { get; set; } = true;

        public WFQYDBServer(IStreamSource streamSource, byte[] SourceAddress, CancellationToken cancellationToken)
        {            
            this.streamSource = streamSource;
            this.ServerAddress = SourceAddress;
            this.cancellationToken = cancellationToken;

            Task.Run(Monitor, cancellationToken).ConfigureAwait(false);
                       
        }

        // TODO: убрать этот кастыль
        public bool NeedToRestart { get; private set; } = false;

        public byte[] ServerAddress
        {
            get => serverAddress;
            set
            {
                serverAddress = value;
                NotifyPropertyChanged();
            }
        }

        public ConnectionState ConnectionState
        {
            get => connectionState;
            private set
            {
                if (connectionState != value)
                {
                    connectionState = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public byte[] DestinationAddress
        {
            get => destinationAddress;
            set
            {
                destinationAddress = value;
                NotifyPropertyChanged();
            }
        }

        public event ErrorEventHandler OnError;

        public event PropertyChangedEventHandler PropertyChanged;

        public LimitedObservableCollection<MessageStoryItem> Story { get; } = new LimitedObservableCollection<MessageStoryItem>();

        public void ResetConnection()
        {
            if (streamSource is TcpStreamSource)
                ((TcpStreamSource)streamSource).Close();

            ConnectionState = ConnectionState.NotConnected;            
            Logger.Warn("ResettingConnection");
            NeedToRestart = true;
        }

        public MessageStoryItem LastResponse
        {
            get => lastResponse; private set
            {
                lastResponse = value;
                NotifyPropertyChanged();
            }
        }

        public TimeSpan UpdatePeriod { get; set; } = new TimeSpan(0, 1, 0);

        private Queue<CommandConveyerItem> Commands = new Queue<CommandConveyerItem>();

        async Task Monitor()
        {
            const int responseTime = 500; //ms
            var lastRequestTime = DateTime.Now;
            const int waitTime = 100; //ms

            Logger.Info("start monitoring");
            PullBroadcastQuery();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    ConnectionState = streamSource.IsOpen ? ConnectionState.Connected : ConnectionState.Connecting;
                    var stream = await streamSource.GetStreamAsync().ConfigureAwait(false);
                    ConnectionState = streamSource.IsOpen ? ConnectionState.Connected : ConnectionState.NotConnected;

                    if (streamSource.Aviable <= 12)
                    {
                        // перезагрузка во время простоя
                        if (UpdatePeriod.TotalSeconds > 0)
                        {
                            if ((LastResponse == null) || ((LastResponse.DateTime + UpdatePeriod < DateTime.Now)))
                            {
                                if (lastRequestTime + UpdatePeriod < DateTime.Now)
                                {
                                    ResetConnection();
                                    await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
                                    //await WfqydbServer.PullBroadcastQuery().ConfigureAwait(false);
                                    PullBroadcastQuery();

                                    lastRequestTime = DateTime.Now;
                                    await Task.Delay(responseTime, cancellationToken).ConfigureAwait(false);
                                }
                            }
                        }
                        

                        // обработка команд
                        if (Commands.Count > 0)
                        {
                            var c = Commands.Dequeue();
                            switch (c.cmd)
                            {
                                case Command.BroadcastQuery:
                                    await Pulla0a1a3Query(Command.BroadcastQuery);
                                    break;
                                case Command.IndividualQuery:
                                    await Pulla0a1a3Query(Command.IndividualQuery);
                                    break;
                                case Command.AutoRun:
                                    await PullAutoRunLocal(c.upFreq, c.dnFreq, c.stokeRate);
                                    break;
                                case Command.Shutdown:
                                    await Pulla0a1a3Query(Command.Shutdown);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                            await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    NeedToRestart = false;

                    //Logger.Info($"reading response header");

                    var response = new Message();
                    await stream.ReadAsync(response.Buffer, 0, 12, cancellationToken).ConfigureAwait(false);
                    response.AutoFit();

                    Logger.Info($"Got response header cmd:{response.Command}");

                    if ((response.IsHeaderValid) && (!cancellationToken.IsCancellationRequested))
                    {
                        Logger.Info($"Got response body {response.DataLength}");

                        int cnt = 20;
                        while (streamSource.Aviable <= response.DataLength)
                        {
                            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                            if (cnt-- == 0)
                            {
                                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                                Logger.Warn($"Can`t read body {response.DataLength}");
                                ResetConnection();
                                break;
                            }
                        }

                        await stream.ReadAsync(response.Buffer, 12, response.DataLength + 1, cancellationToken).ConfigureAwait(false);
                        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

                        if (cancellationToken.IsCancellationRequested)
                            return;
                        
                        LastResponse = new MessageStoryItem(MessageStoryItem.Direction.Rx, response);
                        Story.Add(LastResponse);
                        Logger.Info($"parse response body {response.DataLength} complete.");
                    }
                    else
                        Logger.Info($"Header is not valid.");

                    await Task.Delay(50, cancellationToken).ConfigureAwait(false);

                }
                catch (SocketException ex)
                {
                    ConnectionState = ConnectionState.Fault;
                    FireOnError(ex);
                    Logger.Alarm("Sockets" + ex.Message);
                    await Task.Delay(1000, cancellationToken);
                }
                catch (Exception ex)
                {
                    FireOnError(ex);
                    Logger.Alarm(ex.Message + "\r\n" + ex.StackTrace);
                    await Task.Delay(300, cancellationToken);
                }
            }
            ConnectionState = ConnectionState.NotConnected;
        }

        async Task Pulla0a1a3Query(Command cmd)
        {
            switch (cmd)
            {
                case Command.BroadcastQuery:
                    if (!CanA0) 
                        return;
                    break;
                case Command.IndividualQuery:
                    if (!CanA1) 
                        return;
                    break;                    
                case Command.Shutdown:
                    if (!CanA3) 
                        return;
                    break;
            }

            Logger.Info($"try to pool {cmd} to {streamSource.ToString()}");
            try
            {
                var request = new Message
                {
                    Source = ServerAddress,
                    Command = cmd,
                    Destination = DestinationAddress,
                    Data = new byte[0],
                };

                ConnectionState = streamSource.IsOpen ? ConnectionState.Connected : ConnectionState.Connecting;
                var stream = await streamSource.GetStreamAsync().ConfigureAwait(false);
                ConnectionState = streamSource.IsOpen ? ConnectionState.Connected : ConnectionState.NotConnected;

                Logger.Info($"try to write {cmd} to {streamSource.ToString()}");

                await stream.WriteAsync(request.Buffer, 0, request.FullLengthWithCrc, cancellationToken).ConfigureAwait(false);

                Story.Add(new MessageStoryItem(MessageStoryItem.Direction.Tx, request));
            }
            catch (SocketException ex)
            {
                ConnectionState = ConnectionState.Fault;
                FireOnError(ex);
                Logger.Alarm("Sockets" + ex.Message);
                await Task.Delay(1000, cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                FireOnError(ex);
                Logger.Alarm(ex.Message);
                throw;
            }
        }

        public void PullBroadcastQuery()
        {
            if (!CanA0)
                return;
            Commands.Enqueue(new CommandConveyerItem
            {
                cmd = Command.BroadcastQuery
            });
        }

        public void PullIndividualQuery()
        {
            if (!CanA1)
                return;
            Commands.Enqueue(new CommandConveyerItem
            {
                cmd = Command.IndividualQuery
            });
        }

        public void PullShutdown()
        {
            if (!CanA3)
                return;
            Commands.Enqueue(new CommandConveyerItem
            {
                cmd = Command.Shutdown
            });
        }

        public void PullAutoRun(byte upFreq, byte dnFreq, byte stokeRate)
        {
            if (!CanA2)
                return;
            Commands.Enqueue(new CommandConveyerItem
            {
                cmd = Command.AutoRun,
                dnFreq = dnFreq,
                upFreq = upFreq,
                stokeRate = stokeRate
            });
        }

        private async Task PullAutoRunLocal(byte upFreq, byte dnFreq, byte stokeRate)
        {
            if (!CanA2) 
                return;
            try
            {
                var request = new Message
                {
                    Source = ServerAddress,
                    Command = Command.AutoRun,
                    Destination = DestinationAddress,
                    Data = new byte[] { upFreq, dnFreq, stokeRate },
                };

                ConnectionState = streamSource.IsOpen ? ConnectionState.Connected : ConnectionState.Connecting;
                var stream = await streamSource.GetStreamAsync().ConfigureAwait(false);
                ConnectionState = streamSource.IsOpen ? ConnectionState.Connected : ConnectionState.NotConnected;

                //await Task.Run(() => { stream.Write(request.Buffer, 0, request.FullLengthWithCrc); });
                await stream.WriteAsync(request.Buffer, 0, request.FullLengthWithCrc, cancellationToken).ConfigureAwait(false);
                Story.Add(new MessageStoryItem(MessageStoryItem.Direction.Tx, request));
            }
            catch (SocketException ex)
            {
                ConnectionState = ConnectionState.Fault;
                FireOnError(ex);
                Logger.Alarm("Sockets" + ex.Message);
                await Task.Delay(1000, cancellationToken);
            }
            catch (Exception ex)
            {
                FireOnError(ex);
                Logger.Alarm(ex.Message);
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void FireOnError(Exception ex) =>
            OnError?.Invoke(this, new ErrorEventArgs(ex));

        public override string ToString() =>
            $"WFQYDBServer: {ConnectionState}";
    }
}
