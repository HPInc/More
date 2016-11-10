// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using More;

#if WindowsCE
using UInt16Parser = System.MissingInCEUInt16Parser;
#else
using UInt16Parser = System.UInt16;
#endif

namespace More.Net
{
    /// <summary>
    /// NOTE: since this is a struct with non-readonly members, alway use 'ref' when passing it to functions
    /// and never make it a readonly member of a struct/class.
    /// </summary>
    public struct InternetHost
    {
        public static InternetHost FromIPOrHostWithOptionalPort(
            String ipOrHostOptionalPort, UInt16 defaultPort, PriorityQuery<IPAddress> dnsPriorityQuery, Proxy proxy)
        {
            Int32 colonIndex = ipOrHostOptionalPort.IndexOf(':');
            if (colonIndex >= 0)
            {
                // NOTE: I could parse this without creating another string
                String portString = ipOrHostOptionalPort.Substring(colonIndex + 1);
                if (!UInt16Parser.TryParse(portString, out defaultPort))
                {
                    throw new FormatException(String.Format("Port '{0}' could not be parsed as a 2 byte unsigned integer", portString));
                }
                ipOrHostOptionalPort = ipOrHostOptionalPort.Remove(colonIndex);
            }

            return new InternetHost(ipOrHostOptionalPort, defaultPort, dnsPriorityQuery, proxy);
        }
        public static InternetHost FromIPOrHostWithPort(String ipOrHostWithPort, PriorityQuery<IPAddress> dnsPriorityQuery, Proxy proxy)
        {
            Int32 colonIndex = ipOrHostWithPort.IndexOf(':');
            if (colonIndex < 0)
                throw new FormatException(String.Format("Missing colon to designate the port on '{0}'", ipOrHostWithPort));

            // NOTE: I could parse this without creating another string
            String portString = ipOrHostWithPort.Substring(colonIndex + 1);
            UInt16 port;
            if (!UInt16Parser.TryParse(portString, out port))
            {
                throw new FormatException(String.Format("Port '{0}' could not be parsed as a 2 byte unsigned integer", portString));
            }
            String ipOrHost = ipOrHostWithPort.Remove(colonIndex);

            return new InternetHost(ipOrHost, port, dnsPriorityQuery, proxy);
        }

        public StringEndPoint targetEndPoint; // Cannot be readonly because it is a struct with non-readonly parameters
        public readonly PriorityQuery<IPAddress> dnsPriorityQuery;
        public readonly Proxy proxy;

        public InternetHost(String ipOrHost, UInt16 port, PriorityQuery<IPAddress> dnsPriorityQuery, Proxy proxy)
        {
            this.targetEndPoint = new StringEndPoint(ipOrHost, port);
            this.dnsPriorityQuery = dnsPriorityQuery;
            this.proxy = proxy;
        }
        public InternetHost(InternetHost other, UInt16 port)
        {
            this.targetEndPoint = new StringEndPoint(other.targetEndPoint, port);
            this.dnsPriorityQuery = other.dnsPriorityQuery;
            this.proxy = other.proxy;
        }
        public InternetHost(InternetHost other, Proxy proxy)
        {
            this.targetEndPoint = other.targetEndPoint;
            this.dnsPriorityQuery = other.dnsPriorityQuery;
            this.proxy = proxy;
        }
        public AddressFamily GetAddressFamilyForTcp()
        {
            if (proxy == null)
            {
                targetEndPoint.ForceIPResolution(dnsPriorityQuery);
                return targetEndPoint.ipEndPoint.AddressFamily;
            }
            else
            {
                return proxy.host.GetAddressFamilyForTcp();
            }
        }
        public String CreateTargetString()
        {
            return String.Format("{0}:{1}", targetEndPoint.ipEndPoint, targetEndPoint.port);
        }
        public void Connect(Socket socket, PriorityQuery<IPAddress> dnsPriorityQuery, ProxyConnectOptions proxyOptions, ref BufStruct buf)
        {
            if (proxy == null)
            {
                targetEndPoint.ForceIPResolution(dnsPriorityQuery);
                socket.Connect(targetEndPoint.ipEndPoint);
            }
            else
            {
                proxy.ProxyConnectTcp(socket, ref targetEndPoint, proxyOptions, ref buf);
            }
        }

        /*
        /// <returns>trus if operation is pending, false if it completed</returns>
        public Boolean ConnectAsync(Socket socket, PriorityQuery<IPAddress> dnsPriorityQuery, ProxyConnectOptions proxyOptions, ref BufStruct buf)
        {
            if (proxy == null)
            {
                targetEndPoint.ForceIPResolution(dnsPriorityQuery);
                SocketAsyncEventArgs asyncArgs = new SocketAsyncEventArgs();
                asyncArgs.RemoteEndPoint = targetEndPoint.ipEndPoint;
                return socket.ConnectAsync(asyncArgs);
            }
            else
            {

                return proxy.ProxyConnectAsyncTcp(socket, ref targetEndPoint, proxyOptions, ref buf);
            }
        }
        /// <returns>trus if operation is pending, false if it completed</returns>
        public Boolean ConnectAsync(Socket socket, PriorityQuery<IPAddress> dnsPriorityQuery, ProxyConnectOptions proxyOptions, ref BufStruct buf)
        {
            if (proxy == null)
            {
                targetEndPoint.ForceIPResolution(dnsPriorityQuery);
                SocketAsyncEventArgs asyncArgs = new SocketAsyncEventArgs();
                asyncArgs.RemoteEndPoint = targetEndPoint.ipEndPoint;
                return socket.ConnectAsync(asyncArgs);
            }
            else
            {

                return proxy.ProxyConnectAsyncTcp(socket, ref targetEndPoint, proxyOptions, ref buf);
            }
        }
        */
    }
}