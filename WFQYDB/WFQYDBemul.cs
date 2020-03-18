using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WFQYDB.Communication;

namespace WFQYDB
{
    public class WFQYDBemul : ServiceAbstract
    {

        IStreamSource streamSource;

        CancellationToken ct;

        public byte[] MyAddress { get; set; } = { 1, 2, 3, 4 };

        public byte UpFreq { get; set; } = 0;

        public byte DnFreq { get; set; } = 0;

        public byte StokeLength { get; set; } = 0;

        public byte StokeRate { get; set; } = 0;

        public StatusByte Status { get; set; } = new StatusByte { Byte = 0 };
        
        Task A4;

        public LimitedObservableCollection<MessageStoryItem> Story { get; } = new LimitedObservableCollection<MessageStoryItem>();

        public WFQYDBemul(IStreamSource streamSource, byte[] myAddress, CancellationToken ct = default)
        {
            this.streamSource = streamSource;
            this.MyAddress = myAddress;
            this.ct = ct;
        }

        async Task MakeResponseAndSend(Message request, CancellationToken cancellationToken)
        {
            var response = new Message
            {
                Destination = request.Source,
                Source = MyAddress,
                Command = request.Command,
                Data = new byte[] { UpFreq, DnFreq, StokeLength, StokeRate, Status.Byte },
            };
            Story.Add(new MessageStoryItem(MessageStoryItem.Direction.Tx, response));
            var stream = await streamSource.GetStreamAsync();
            await stream.WriteAsync(response.Buffer, 0, response.FullLengthWithCrc, cancellationToken);
        }

        async Task A4PeriodicSend(byte[] source, CancellationToken cancellationToken)
        {
            byte[] srcAddr = new byte[4];
            source.CopyTo(srcAddr, 0);

            while (Status.Start && (!cancellationToken.IsCancellationRequested))
            {
                var response = new Message
                {
                    Destination = srcAddr,
                    Source = MyAddress,
                    Command = Command.RealtimeData,
                    Data = new byte[] { UpFreq, DnFreq, StokeLength, StokeRate, Status.Byte },
                };

                Story.Add(new MessageStoryItem(MessageStoryItem.Direction.Tx, response));
                var stream = await streamSource.GetStreamAsync();
                await stream.WriteAsync(response.Buffer, 0, response.FullLengthWithCrc, cancellationToken);

                await Task.Delay(5_000, cancellationToken);
            }
        }

        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[525];

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if(streamSource.Aviable < 12)
                    {
                        await Task.Delay(10, cancellationToken);
                        continue;
                    }

                    var stream = await streamSource.GetStreamAsync();
                    var i = await stream.ReadAsync(buffer, 0, 12, cancellationToken);
                    Message request = new Message(buffer);
                    if (!request.IsHeaderValid)
                    {
                        await stream.FlushAsync(cancellationToken);
                        continue;
                    }
                    i = await stream.ReadAsync(buffer, 12, request.DataLength + 1, cancellationToken);
                    request = new Message(buffer);
                    Story.Add(new MessageStoryItem(MessageStoryItem.Direction.Rx, request));

                    switch (request.Command)
                    {
                        case Command.BroadcastQuery:
                            await MakeResponseAndSend(request, cancellationToken);
                            break;
                        case Command.IndividualQuery:
                            await MakeResponseAndSend(request, cancellationToken);
                            break;
                        case Command.AutoRun:
                            Status.Start = true;
                            A4 = A4PeriodicSend(request.Source, cancellationToken);
                            // await MakeResponseAndSend(request, cancellationToken);
                            break;
                        case Command.Shutdown:
                            Status.Start = false;
                            await MakeResponseAndSend(request, cancellationToken);
                            break;
                        //case Command.RealtimeData:                    
                        default:
                            // ignore
                            stream.Flush();
                            break;
                    }
                }catch(Exception ex)
                {
                    await OnErrorAsync(ex, cancellationToken);
                    break;
                }

                await Task.Delay(100, cancellationToken);
            }
            Status.Start = false;
            Dead?.Invoke(this, null);
        }//ServiceTaskAsync()

        public event EventHandler Dead;
    }
}
