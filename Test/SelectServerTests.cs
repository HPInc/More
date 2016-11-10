// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More.Net
{
    [TestClass]
    public class SelectServerTests
    {
        public class SharedArraySelectServer : SelectServer<SharedArraySelectServer>
        {
            public Byte[] sharedArray;
            public SharedArraySelectServer() : base(false)
            {
                this.sharedArray = new Byte[256];
            }
            protected override SharedArraySelectServer HandlerObject
            {
                get { return this; }
            }
        }
        [TestMethod]
        public void TestBasics()
        {
            UInt16 tcpPort, udpPort;
            Thread serverThread = StartSelectServer(out tcpPort, out udpPort);
            Console.WriteLine("Server running (tcp port {0}, udp port {1})", tcpPort, udpPort);

            IPEndPoint udpEndPoint = new IPEndPoint(IPAddress.Loopback, udpPort);
            IPEndPoint tcpEndPoint = new IPEndPoint(IPAddress.Loopback, tcpPort);

            Socket tcpSocket = new Socket(tcpEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Connect(tcpEndPoint);
            tcpSocket.Send(Encoding.ASCII.GetBytes("hello"));
            tcpSocket.Shutdown(SocketShutdown.Both);
            tcpSocket.Close();

            Socket udpSocket = new Socket(udpEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.SendTo(Encoding.ASCII.GetBytes("stop"), udpEndPoint);
            udpSocket.Close();

            Console.WriteLine("Waiting for server thread to finish...");
            serverThread.Join();
        }
        Thread StartSelectServer(out UInt16 tcpListenPort, out UInt16 udpReceivePort)
        {
            IPEndPoint listenEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Socket udpReceiveSock = new Socket(listenEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            udpReceiveSock.Bind(listenEndPoint);
            udpReceivePort = (UInt16)((IPEndPoint)udpReceiveSock.LocalEndPoint).Port;

            Socket tcpListenSock = new Socket(listenEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            tcpListenSock.Bind(listenEndPoint);
            tcpListenPort = (UInt16)((IPEndPoint)tcpListenSock.LocalEndPoint).Port;
            tcpListenSock.Listen(8);

            var server = new SharedArraySelectServer();
            server.AddReceiveSocket(udpReceiveSock, UdpReceiveHandler);
            server.AddListenSocket(tcpListenSock, TcpAcceptHandler);
            Thread serverThread = new Thread(() =>
            {
                server.Run();
            });
            serverThread.Name = "SelectServer";
            serverThread.Start();
            return serverThread;
        }
        void UdpReceiveHandler(SharedArraySelectServer server, Socket sock)
        {
            EndPoint from = new IPEndPoint(IPAddress.Any, 0);

            int received = sock.ReceiveFrom(server.sharedArray, ref from);
            if (received < 0)
            {
                throw new Exception("ReceiveFrom failed");
            }
            String command = Encoding.ASCII.GetString(server.sharedArray, 0, received);
            if (command.Equals("stop"))
            {
                while (server.ListenSockets.Count > 0)
                {
                    var enumerator = server.ListenSockets.GetEnumerator();
                    Assert.AreEqual(true, enumerator.MoveNext());
                    Console.WriteLine("[SelectServer] Closing listen socket...");
                    server.DisposeAndRemoveListenSocket(enumerator.Current);
                }
                Console.WriteLine("[SelectServer] Closing UDP receive socket...");
                Console.Out.Flush();
                server.DisposeAndRemoveReceiveSocket(sock);
            }
        }
        void TcpAcceptHandler(SharedArraySelectServer server, Socket sock)
        {
            Socket newSock = sock.Accept();
            Console.WriteLine("[SelectServer] Accepted new connection from '{0}'", newSock.RemoteEndPoint);
            server.AddReceiveSocket(newSock, TcpReceiveHandler);
        }
        void TcpReceiveHandler(SharedArraySelectServer server, Socket sock)
        {
            int received = sock.Receive(server.sharedArray);
            if (received <= 0)
            {
                server.DisposeAndRemoveReceiveSocket(sock);
            }
            else
            {
                Console.WriteLine("[SelectServer] Got {0} bytes", received);
            }
        }
    }
}