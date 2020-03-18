using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WFQYDB.Communication
{
    public class TcpStreamSourceConfig : IStreamSourceConfig
    {
        private string host = "127.0.0.1";
        private int port = 6789;

        private int readTimeout = -1;
        private int writeTimeout = -1;

        public string ClassName => nameof(TcpStreamSource);

        public string Host 
        { 
            get => host;
            set
            {
                host = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Cfg));
            }
        }

        public int Port 
        { 
            get => port;
            set
            {
                port = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Cfg));
            }
        }

        public int ReadTimeout
        {
            get => readTimeout;
            set { readTimeout = value; NotifyPropertyChanged(); }
        }

        public int WriteTimeout
        {
            get => writeTimeout;
            set { writeTimeout = value; NotifyPropertyChanged(); }
        }

        public string Cfg
        {
            get => ToCfgString();
            set => FromCfgString(value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public IStreamSourceConfig FromCfgString(string connString)
        {
            var csParts = connString.Split(':');

            if (csParts.Length > 0)
            {
                host = csParts[0].Trim();
                if (csParts.Length > 1) 
                { 
                    if (int.TryParse(csParts[1].Trim(), out int p))
                        port = p;
                    else
                        throw new ArgumentException($"ip port {csParts[1]} is invalid");
                }
            }

            return this;
        }

        public string ToCfgString() => $"{host}:{port}";

        public override string ToString() => $"\"{ClassName}\"; {ToCfgString()}";

        public IStreamSource Get()
        {
            return new TcpStreamSource
            {
                Host = host,
                Port = port,
                ReadTimeout = readTimeout,
                WriteTimeout = writeTimeout,
            };
        }

    }
}
