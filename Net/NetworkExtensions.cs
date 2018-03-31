// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace More
{
    public static class SocketExtensions
    {
#if WindowsCE
        public static void Connect(this Socket socket, String host, Int32 port)
        {
            var ip = EndPoints.ParseIPOrResolveHost(host, DnsPriority.IPv4ThenIPv6);
            socket.Connect(new IPEndPoint(ip, port));
        }
#endif

        public static void Connect(this Socket socket, ref StringEndPoint endpoint, PriorityQuery<IPAddress> dnsPriorityQuery)
        {
            if (endpoint.ipEndPoint == null)
            {
                endpoint.ipEndPoint = new IPEndPoint(EndPoints.DnsResolve(endpoint.ipOrHost, dnsPriorityQuery), endpoint.port);
            }
            socket.Connect(endpoint.ipEndPoint);
        }
        public static void Connect(this Socket socket, More.Net.InternetHost host, PriorityQuery<IPAddress> dnsPriorityQuery,
            More.Net.ProxyConnectOptions proxyOptions, ref BufStruct buf)
        {
            if (host.proxy == null)
            {
                host.targetEndPoint.ForceIPResolution(dnsPriorityQuery);
                socket.Connect(host.targetEndPoint.ipEndPoint);
            }
            else
            {
                host.proxy.ProxyConnectTcp(socket, ref host.targetEndPoint, proxyOptions, ref buf);
            }
        }
        /*
        public static void ConnectUdpSocketThroughProxy(this Socket socket, More.Net.HostWithOptionalProxy host)
        {
            if (host.proxy == null)
            {
                socket.Connect(host.endPoint);
            }
            else
            {
                socket.Connect(host.proxy.endPoint);
                host.proxy.ProxyConnectUdp(socket, host.endPoint);
            }
        }
        */
        public static String SafeLocalEndPointString(this Socket socket)
        {
            try { return socket.LocalEndPoint.ToString(); }
            catch (Exception) { return "<not-bound>"; }
        }
        public static String SafeRemoteEndPointString(this Socket socket)
        {
            try { return socket.RemoteEndPoint.ToString(); }
            catch (Exception) { return "<disconnected>";  }
        }
        public static void SendFullSize(this Socket socket, Byte[] buffer)
        {
            SendFullSize(socket, buffer, 0, buffer.Length);
        }
        public static void SendFullSize(this Socket socket, Byte[] buffer, Int32 offset, Int32 size)
        {
            if (size > 0)
            {
                while(true)
                {
                    int lastSent = socket.Send(buffer, offset, size, SocketFlags.None);
                    if (lastSent <= 0)
                    {
                        throw new IOException(String.Format("socket Send returned {0}", lastSent));
                    }
                    size -= lastSent;
                    if (size <= 0)
                    {
                        return;
                    }
                    offset += lastSent;
                }
            }
        }
        public static void ReadFullSize(this Socket socket, Byte[] buffer, Int32 offset, Int32 size)
        {
            int lastBytesRead;

            do
            {
                lastBytesRead = socket.Receive(buffer, offset, size, SocketFlags.None);
                size -= lastBytesRead;

                if (size <= 0) return;

                offset += lastBytesRead;
            } while (lastBytesRead > 0);

            throw new IOException(String.Format("reached end of stream: still needed {0} bytes", size));
        }
        public static void SendFile(this Socket socket, String filename, Byte[] transferBuffer)
        {
            using(FileStream fileStream = new FileStream(filename, FileMode.Open))
            {
                Int32 bytesRead;
                while ((bytesRead = fileStream.Read(transferBuffer, 0, transferBuffer.Length)) > 0)
                {
                    socket.Send(transferBuffer, 0, bytesRead, SocketFlags.None);
                }
            }
        }
        public static void ShutdownSafe(this Socket socket)
        {
            if (socket != null)
            {
                try
                {
                    if (socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        public static void ShutdownAndDispose(this Socket socket)
        {
            if (socket != null)
            {
                if (socket.Connected)
                {
                    try
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                    catch (SocketException)
                    {
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
                socket.Close();
            }
        }
        public static int ReceiveNoThrow(this Socket socket, Byte[] buffer, int offset, int length, SocketFlags flags)
        {
            try
            {
                return socket.Receive(buffer, offset, length, flags);
            }
            catch (SocketException)
            {
                return -1;
            }
        }
    }
}