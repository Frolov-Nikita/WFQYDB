using ModbusBasic;
using System;
using System.Net;
using System.Net.Sockets;

namespace Multislave
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 502);
            TcpListener rtuListener = new TcpListener(IPAddress.Any, 503);
            
            tcpListener.Start();
            rtuListener.Start();

            var factory = new ModbusFactory();
            var slave = factory.CreateSlave(1);

            var hRegisters = slave.DataStore.HoldingRegisters;

            var networkTcp = factory.CreateSlaveNetwork(tcpListener);
            var networkRtu = factory.CreateRtuOverTcpSlaveNetwork(rtuListener);

            networkTcp.AddSlave(slave);
            networkRtu.AddSlave(slave);

            var tcpListenerTask = networkTcp.ListenAsync();
            var rtuListenerTask = networkRtu.ListenAsync();

            Console.WriteLine("press any key to die!");
            Console.ReadKey();
        }
    }
}
