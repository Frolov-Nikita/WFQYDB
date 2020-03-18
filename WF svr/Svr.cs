using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using WFQYDB;
using WFQYDB.Communication;

namespace WF_svr
{
    public class Svr : INotifyPropertyChanged
    {
        private IStreamSource streamSource;

        public IStreamSourceConfig StreamSourceCfg {get;set;}

        CancellationTokenSource cancellationTokenSource;


        private byte[] serverAddress = { 7, 7, 7, 7 };

        private byte[] destenationAddress = { 1, 2, 3, 4 };

        public byte[] ServerAddress
        {
            get => Server?.ServerAddress ?? serverAddress;
            set
            {
                serverAddress = value;
                if (Server != null)
                    Server.ServerAddress = serverAddress;
            }
        }

        public byte[] DestenationAddress
        {
            get => Server?.DestinationAddress ?? destenationAddress;
            set 
            { 
                destenationAddress = value;
                if (Server != null)
                    Server.DestinationAddress = destenationAddress;
            }
        }
                
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public WFQYDBServer Server { get; private set; }

        [JsonIgnore]
        public bool IsConnected => streamSource?.IsOpen ?? false;

        public void Connect()
        {
            if (streamSource == default)
                streamSource = StreamSourceCfg.Get();

            if (IsConnected)
                return;

            cancellationTokenSource = new CancellationTokenSource();

            Server = new WFQYDBServer(
                streamSource,
                serverAddress,
                cancellationTokenSource.Token);

            Server.DestinationAddress = destenationAddress;

            NotifyPropertyChanged("IsConnected");
        }

        public void Disconnect()
        {
            if (!IsConnected)
                return;

            cancellationTokenSource.Cancel();
            Task.Delay(100).Wait();

            streamSource.Close();
            Server = null;

            NotifyPropertyChanged("IsConnected");
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
