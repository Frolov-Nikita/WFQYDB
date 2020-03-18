using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WFQYDB.Communication
{
    public delegate void TcpClientConnectedHandler(object sender, TcpServerStreamSource e);

    public class TcpServer
    {
        TcpListener listener;

        List<TcpClient> clients = new List<TcpClient>();

        Task taskClientConnector;
        private readonly int port = 6789;

        public bool IsOpen => listener != default;

        public int Port => port;


        public int ClientsCount => clients.Count;

        public TcpServer(int port)
        {
            this.port = port;
        }

        async Task ClientConnector(CancellationToken cancellationToken)
        {
            EnshureStarted();
            while (!cancellationToken.IsCancellationRequested)
            {
                var cli = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                FireOnClientConnected(cli);
                clients.Add(cli);
            }

            // закрываем все соединения
            Stop();
        }

        void EnshureStarted()
        {
            if (!IsOpen)
            {
                listener = new TcpListener(IPAddress.Any, Port);
                listener.Start();
            }
        }

        public void Start(CancellationToken cancellationToken)
        {
            //EnshureStarted();
            taskClientConnector = ClientConnector(cancellationToken);
        }

        public void Stop()
        {
            listener.Stop();
            clients.ForEach(c =>
            {
                c.GetStream().Close();
                c.Close();
                c.Dispose();
            });
            clients.Clear();

        }


        public event TcpClientConnectedHandler OnClientConnected;

        private void FireOnClientConnected(TcpClient cli)
        {
            if (cli != null)
                OnClientConnected?.Invoke(this, new TcpServerStreamSource(cli));
        }

    }// TcpServer

    public class TcpServerStreamSource : IStreamSource
    {
        TcpClient client;

        public TcpServerStreamSource(TcpClient client)
        {
            this.client = client;
        }

        ~TcpServerStreamSource()
        {
            Close();
        }

        public int Aviable => client.Available;

        public bool IsOpen => client.Connected;

        public int ReadTimeout { 
            get => client.ReceiveTimeout; 
            set => client.ReceiveTimeout = value; 
        }

        public int WriteTimeout { 
            get => client.SendTimeout;
            set => client.SendTimeout = value;
        }

        public async Task<Stream> GetStreamAsync() =>
            await Task.Run(() => client.GetStream());


        public async Task<Stream> GetStreamAsync(CancellationToken cancellationToken)=>
            await Task.Run(() => client.GetStream(), cancellationToken);

        private void Close()
        {
            client.GetStream().Close();
            client.Close();
            client.Dispose();
        }

    }
}
