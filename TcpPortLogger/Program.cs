using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TcpPortLogger
{
    class Program
    {

        static void LogBuffer(byte[] buffer, int length)
        {
            var consoleForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(DateTime.Now.ToString("HH:mm:ss.ffff") + "\t");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(BitConverter.ToString(buffer, 0, length).Replace("-", " "));
            Console.ForegroundColor = consoleForegroundColor;
        }

        static async Task Listen(TcpListener listener, CancellationToken ct)
        {

            var buffer = new byte[1024];
            listener.Start();

            var cli = await listener.AcceptTcpClientAsync();
            Console.WriteLine($"Подключен {cli.Client.RemoteEndPoint}");

            while (!ct.IsCancellationRequested)
            {
                if (cli.Available > 0)
                {
                    await Task.Delay(0);

                    var length = cli.Available;

                    await cli.GetStream().ReadAsync(buffer, 0, length);

                    LogBuffer(buffer, length);
                }
                
                await Task.Delay(50);
            }
            cli.GetStream().Close();
            cli.Close();
            cli.Dispose();
        }

        static void Main(string[] args)
        {
            var port = 10001;
            
            if(args.Length > 0)
                if (int.TryParse(args[0], out int aport))
                    port = aport;

            var tcpListener = new TcpListener(IPAddress.Any, port);
            
            var cts = new CancellationTokenSource();

            var taskListener = Listen(tcpListener, cts.Token);

            Console.WriteLine($"МОНИТОРИЛКА v0");

            Console.WriteLine($"Ждем подключения на *:{port}");

            Console.WriteLine($"Жми esc для выхода.");

            Thread.Sleep(5);

            while (!cts.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var c = Console.ReadKey();
                    switch (c.Key)
                    {
                        case ConsoleKey.Insert:
                            Console.WriteLine("Введите строку байтов");
                            var str = Console.ReadLine();
                            break;
                        case ConsoleKey.Escape:
                            tcpListener.Stop();
                            cts.Cancel();                            
                            break;
                        default:
                            break;
                    }
                }
                //Console.WriteLine($"{taskListener.Status}");
                Thread.Sleep(50);
            }

        }
    }
}
