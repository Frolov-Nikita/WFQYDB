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
    public class TcpStreamSource : IStreamSource
    {
        object locker = new object();

        TcpClient client;

        public string Host { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 6789;

        public int Aviable => client?.Available ?? 0;

        public int ReadTimeout { get; set; } = 5000;

        public int WriteTimeout { get; set; } = 5000;

        public bool IsOpen => client?.Connected ?? false;

        public async Task<Stream> GetStreamAsync() =>
            await Task.Run(() => {
                lock (locker)
                {
                    if ((client == default)||(!client.Connected))
                    {                        
                        client?.Close();
                        client?.Dispose();

                        client = null;

                        GC.Collect();
                        Logger.Info($"Creating TcpClient {Host}:{Port}");
                        //client = new TcpClient(Host, Port) { ReceiveTimeout = ReadTimeout, SendTimeout = WriteTimeout};
                        client = new TcpClient();// { ReceiveTimeout = ReadTimeout, SendTimeout = WriteTimeout };

                        // вариант 0 работает, но подглючивает
                        //client.Connect(Host, Port);

                        // вариант 1 не работает                        
                        //var r = client.BeginConnect(Host, Port, null, null);
                        //var success = r.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));
                        //if ((!success) || (!client.Connected))
                        //{
                        //    Logger.Warn($"Can't connect to {Host}:{Port}");
                        //    throw new Exception("Failed to connect.");
                        //}


                        // вариант 3 плохо работает
                        if (!client.ConnectAsync(Host, Port).Wait(5000))
                        {
                            Logger.Warn($"Can't connect to {Host}:{Port}");

                            throw new Exception("Failed to connect.");
                        }

                        client.NoDelay = false;
                        client.GetStream().ReadTimeout = ReadTimeout;
                        client.GetStream().WriteTimeout = WriteTimeout;
                        //Logger.Info($"Connected? {client.Connected} SendTimeout {client.SendTimeout} ReceiveTimeout {client.ReceiveTimeout} NoDelay{client.NoDelay} CanTimeout{client.GetStream().CanTimeout}");
                    }
                }

                if (client == default)
                    Logger.Alarm("Fail to create Tcp client");

                var stream = client.GetStream();
                
                stream.ReadTimeout = ReadTimeout;
                stream.WriteTimeout = WriteTimeout;
                
                return stream;
            });

        public void Close()
        {
            client?.GetStream()?.Close();
            client?.Close();
            client?.Dispose();
            client = null;
            GC.Collect();
        }

        public override string ToString() =>
            $"Tcp {Host}:{Port} Connected?{client?.Connected??false}";

    }
}
