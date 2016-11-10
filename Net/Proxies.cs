// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using More;

#if WindowsCE
using IPParser = System.Net.MissingInCEIPParser;
using UInt16Parser = System.MissingInCEUInt16Parser;
#else
using IPParser = System.Net.IPAddress;
using UInt16Parser = System.UInt16;
#endif

namespace More.Net
{
    public abstract class Proxy
    {
        /// <summary>
        /// Parses a proxy specifier.  A proxy specifier is of the format:
        /// protocol:host:port
        /// The supported protocols are http, socks4, socks5, gateway, httpconnect.
        /// </summary>
        /// <param name="proxySpecifier">The proxy specifier to parser.</param>
        /// <param name="dnsPriorityQuery">The dns priority to resolve if a hostname is given for the proxy host.</param>
        /// <param name="proxyForProxy">A proxy to connect to the proxy through.</param>
        /// <returns>A proxy that can be used to connect sockets through.</returns>
        public static Proxy ParseProxy(String proxySpecifier, PriorityQuery<IPAddress> dnsPriorityQuery, Proxy proxyForProxy)
        {
            // format
            // http:<ip-or-host>:<port>
            // socks4:<ip-or-host>:<port>
            if (proxySpecifier == null || proxySpecifier.Length <= 0) return default(Proxy);

            String[] splitStrings = proxySpecifier.Split(':');
            if (splitStrings.Length != 3)
                throw new FormatException(String.Format("Invalid proxy '{0}', expected 'http:<host>:<port>', 'socks4:<host>:<port>' or 'socks5:<host>:<port>'", proxySpecifier));

            String proxyTypeString = splitStrings[0];
            String ipOrHost = splitStrings[1];
            String portString = splitStrings[2];

            UInt16 port;
            if (!UInt16Parser.TryParse(portString, out port))
            {
                throw new FormatException(String.Format("Invalid port '{0}'", portString));
            }
            InternetHost proxyHost = new InternetHost(ipOrHost, port, dnsPriorityQuery, proxyForProxy);

            if (proxyTypeString.Equals("socks4", StringComparison.CurrentCultureIgnoreCase))
            {
                return new Socks4Proxy(proxyHost, null);
            }
            else if (proxyTypeString.Equals("socks5", StringComparison.CurrentCultureIgnoreCase))
            {
                return new Socks5NoAuthenticationConnectSocket(proxyHost);
            }
            else if (proxyTypeString.Equals("http", StringComparison.CurrentCultureIgnoreCase))
            {
                return new HttpProxy(proxyHost);
            }
            else if (proxyTypeString.Equals("gateway", StringComparison.CurrentCultureIgnoreCase))
            {
                return new GatewayProxy(proxyHost);
            }
            else if (proxyTypeString.Equals("httpconnect", StringComparison.CurrentCultureIgnoreCase))
            {
                return new HttpConnectProxyProxy(proxyHost);
            }

            throw new FormatException(String.Format("Unexpected proxy type '{0}', expected 'gateway', 'http', 'httpconnect', 'socks4' or 'socks5'", proxyTypeString));
        }

        /// <summary>
        /// Strips proxies from connection specifier.
        /// Proxies are configured before the '%' sign, i.e. socks4:myproxy:1080%host.
        /// You can also configure multiple proxies, i.e. gateway:my-gateway-proxy:8080%socks4:myproxy:1080%host.
        /// </summary>
        /// <param name="connectorString">A connector string with potential proxies configured.</param>
        /// <param name="dnsPriorityQuery">Callback that determines priority to select an address from Dns record.
        ///   Use DnsPriority.(QueryName) for standard queries.</param>
        /// <param name="proxy">The proxy (or proxies) stripped from the connector string.</param>
        /// <returns>Connector string with proxies stripped.  Also returns the parsed proxies.</returns>
        public static String StripAndParseProxies(String connectorString, PriorityQuery<IPAddress> dnsPriorityQuery, out Proxy proxy)
        {
            proxy = null;

            while (true)
            {
                int percentIndex = connectorString.IndexOf('%');
                if (percentIndex < 0)
                {
                    return connectorString;
                }

                proxy = ParseProxy(connectorString.Remove(percentIndex), dnsPriorityQuery, proxy);
                connectorString = connectorString.Substring(percentIndex + 1);
            }
        }

        public InternetHost host; // Cannot be readonly because it is a struct with non-readonly fields
        protected Proxy(InternetHost host)
        {
            this.host = host;
        }

        public abstract ProxyType Type { get; }

        /*
        public String ConnectorString(StringEndPoint targetEndPoint)
        {
            return String.Format("{0}:{1}:{2}%{3}", Type,
                ipOrHost, endPoint.Port, targetEndPoint);
        }
        public override String ToString()
        {
            return String.Format("{0}:{1}:{2}", Type, ipOrHost, endPoint.Port);
        }
        */

        // Design Note: The ProxyConnect method could also connect the socket, however,
        //              in some cases the socket will already be connected, so, this api
        //              doesn't wrap connecting the socket.  Note that this means typically
        //              this call will always follow immediately after calling socket.Connect()
        /// <summary>
        /// Setup the proxy connection. The given socket must already be connected.
        /// The endpoint should have been retrieved from the proxy's CreateEndpoint method.
        /// 
        /// Once the method is done, any leftover data from the socket will be in the given buffer
        /// </summary>
        /// <param name="socket">A connected tcp socket</param>
        /// <param name="ipEndPoint">The final destination, retreived from calling proxy.CreateEndpoint(...).</param>
        public abstract void ProxyConnectTcp(Socket socket, ref StringEndPoint endPoint,
            ProxyConnectOptions options, ref BufStruct buf);


        public abstract Boolean ProxyConnectAsyncTcp(Socket socket, ref StringEndPoint endPoint,
            ProxyConnectOptions options, ref BufStruct buf); 

        /// <summary>
        /// Setup a UDP proxy.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="endpoint"></param>
        public abstract void ProxyConnectUdp(Socket socket, ref StringEndPoint endPoint);
    }
    public enum ProxyType
    {
        Gateway,     // No special data is sent to the proxy, the client just talks to the proxy like it is end intended target.
                     // Protocols can use this type of proxy if the protocol itself contains the intended host name.
                     // Applicable Protocols: HTTP
                     // Note that a typical gateway proxy will also support HttpConnect, in which case you have a full Http proxy.
        HttpConnect, // Initiates connection to the target by using an HTTP CONNECT method
                     // Typically used for HTTPS proxy connections, but could be used for anything.
        Http,        // Gateway for unencrypted HTTP traffic, and HttpConnect for everything else
        Socks4,
        Socks5,
    }
    [Flags]
    public enum ProxyConnectOptions
    {
        None = 0x00,
        ContentIsRawHttp = 0x01,
    }
    /// <summary>
    /// A Gateway proxy is just a proxy that works with no handshake.  Typically it only
    /// works with protocols that include the destination endpoint, for example, HTTP's Host header.
    /// </summary>
    public class GatewayProxy : Proxy
    {
        public GatewayProxy(InternetHost host)
            : base(host)
        {
        }
        public override ProxyType Type
        {
            get { return ProxyType.Gateway; }
        }
        public override void ProxyConnectTcp(Socket socket, ref StringEndPoint endPoint,
            ProxyConnectOptions options, ref BufStruct buf)
        {
            host.Connect(socket, DnsPriority.IPv4ThenIPv6, options, ref buf);
        }
        public override Boolean ProxyConnectAsyncTcp(Socket socket, ref StringEndPoint endPoint,
            ProxyConnectOptions options, ref BufStruct buf)
        {
            throw new NotImplementedException();
        }
        public override void ProxyConnectUdp(Socket socket, ref StringEndPoint endPoint)
        {
        }
    }
    public class HttpConnectProxyProxy : Proxy
    {
        public HttpConnectProxyProxy(InternetHost host)
            : base(host)
        {
        }
        public override ProxyType Type
        {
            get { return ProxyType.HttpConnect; }
        }
        public override void ProxyConnectUdp(Socket socket, ref StringEndPoint endPoint)
        {
            throw new NotSupportedException("The Http Connect protocol does not support Udp (as far as I know)");
        }
        public override void ProxyConnectTcp(Socket socket, ref StringEndPoint endPoint,
            ProxyConnectOptions options, ref BufStruct buf)
        {
            throw new NotImplementedException();
            /*
            //
            // Check if the proxy end point string is valid
            //
            socket.Send(Encoding.UTF8.GetBytes(String.Format(
                "CONNECT {0} HTTP/1.1\r\nHost: {0}:{1}\r\n\r\n",
                endpoint.unparsedIPOrHost, endpoint.port)));

            NetworkStream stream = new NetworkStream(socket);

            //
            // Process first line
            //
            for (int i = 0; i < 9; i++)
            {
                Int32 nextByte = stream.ReadByte();
                if ((nextByte & 0xFF) != nextByte) throw new SocketException();
            }

            //
            // Get response code
            //
            Char[] responseCodeChars = new Char[3];
            responseCodeChars[0] = (Char)stream.ReadByte();
            responseCodeChars[1] = (Char)stream.ReadByte();
            responseCodeChars[2] = (Char)stream.ReadByte();
            String responseCodeString = new String(responseCodeChars);

            Int32 responseCode;
            if (!Int32.TryParse(responseCodeString, out responseCode))
                throw new InvalidOperationException(String.Format("First line of HTTP Connect response was not formatted correctly (Expected response code but got '{0}')", responseCodeString));

            if (responseCode != 200) throw new InvalidOperationException(String.Format("Expected response code 200 but got {0}", responseCode));

            //
            // Read until end of response
            //
            Int32 lineLength = 12;

            while (true)
            {
                Int32 nextByte = stream.ReadByte();
                //Console.WriteLine("[HttpsProxyDebug] Got Char '{0}'", (Char)nextByte);
                if ((nextByte & 0xFF) != nextByte) throw new SocketException();

                if (nextByte != '\r')
                {
                    lineLength++;
                }
                else
                {
                    nextByte = stream.ReadByte();
                    if ((nextByte & 0xFF) != nextByte) throw new SocketException();
                    if (nextByte != '\n') throw new InvalidOperationException(String.Format(
                         "Received '\\r' and expected '\\n' next but got (Char)'{0}' (Int32)'{1}'",
                         (Char)nextByte, nextByte));


                    if (lineLength <= 0) break;

                    lineLength = 0;
                }
            }
            */
        }
        public override Boolean ProxyConnectAsyncTcp(Socket socket, ref StringEndPoint endPoint,
            ProxyConnectOptions options, ref BufStruct buf)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// A Gateway proxy is just a proxy that works with no handshake.  Typically it only
    /// works with protocols that include the destination endpoint, for example, HTTP's Host header.
    /// </summary>
    public class HttpProxy : Proxy
    {
        //Buf buf;
        public HttpProxy(InternetHost host)
            : base(host)
        {
        }
        public override ProxyType Type
        {
            get { return ProxyType.Http; }
        }
        public override void ProxyConnectUdp(Socket socket, ref StringEndPoint endPoint)
        {
            throw new NotSupportedException("The Http protocol does not support Udp (as far as I know)");
        }
        public override void ProxyConnectTcp(Socket socket, ref StringEndPoint endPoint,
            ProxyConnectOptions options, ref BufStruct buf)
        {
            if ((options & ProxyConnectOptions.ContentIsRawHttp) == 0)
            {
                throw new NotImplementedException();
            }
        }
        public override Boolean ProxyConnectAsyncTcp(Socket socket, ref StringEndPoint endPoint,
            ProxyConnectOptions options, ref BufStruct buf)
        {
            throw new NotImplementedException();
        }
    }
}
