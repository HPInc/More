// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    /// <summary>
    /// Summary description for TestPopSocket
    /// </summary>
    [TestClass]
    public class TestPopSocketClass
    {
        /*
        [TestMethod]
        public void TestPopSocket()
        {
            using (Socket popReceiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                popReceiveSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));

                using (Socket datagramSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    EndPoint popReceiveEndPoint = popReceiveSocket.LocalEndPoint;

                    new Thread(() =>
                    {
                        Console.WriteLine("[SignalThread] Sleeping for 1 second...");
                        Thread.Sleep(1000);
                        Console.WriteLine("[SignalThread] Signaling popSocket");
                        datagramSendSocket.SendTo(new Byte[] { 0 }, popReceiveEndPoint);
                        Console.WriteLine("[SignalThread] Signal Thread Done");
                    }).Start();

                    Console.WriteLine("[SelectThread] Waiting for signal...");
                    Socket.Select(new SingleObjectList(popReceiveSocket), null, null, Int32.MaxValue);
                    Console.WriteLine("[SelectThread] Got signal");
                }
            }
        }
        */
        [TestMethod]
        public void TestPopSocket2()
        {
            using (Socket popSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                popSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                EndPoint popSocketEndPoint = popSocket.LocalEndPoint;

                new Thread(() =>
                {
                    Console.WriteLine("[SignalThread] Sleeping for 1 second...");
                    Thread.Sleep(1000);
                    Console.WriteLine("[SignalThread] Signaling popSocket");
                    popSocket.SendTo(new Byte[1], popSocketEndPoint);
                    Console.WriteLine("[SignalThread] Signal Thread Done");
                }).Start();

                Console.WriteLine("[SelectThread] Waiting for signal...");
                Socket.Select(new SingleObjectList(popSocket), null, null, Int32.MaxValue);
                Console.WriteLine("[SelectThread] Got signal");
            }
        }
    }
}
