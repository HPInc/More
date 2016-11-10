// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

using More;

namespace More.Net
{
    public enum SocksCommand : byte
    {
        Connect      = 0x01,
        Bind         = 0x02,
        UdpAssociate = 0x03,
    };
    public enum SocksAddressType
    {
        IPv4       = 0x01,
        DomainName = 0x03,
        IPv6       = 0x04,
    };
    public static class Socks5
    {
        public const Int32 RequestLengthWithoutAddress = 6;
        public const Int32 ReplyLengthWithoutAddress = 6;

        public static Int32 MakeRequest(Byte[] packet, SocksCommand command, IPAddress ip, Int32 port)
        {
            Byte[] address = ip.GetAddressBytes();

            packet[0] = 0x05;          // Version 5
            packet[1] = (Byte)command;
            packet[2] = 0;             // Reserved

            Int32 offset;
            if (address.Length == 4)
            {
                packet[3] = (Byte)SocksAddressType.IPv4;
                packet[4] = address[0];
                packet[5] = address[1];
                packet[6] = address[2];
                packet[7] = address[3];
                offset = 8;
            }
            else if (address.Length == 16)
            {
                packet[3] = (Byte)SocksAddressType.IPv6;
                Array.Copy(address, 0, packet, 4, 16);
                offset = 20;
            }
            else
            {
                throw new InvalidOperationException(String.Format("IPAddress byte length is {0} bytes but only 4 and 16 are supported", address.Length));
            }

            packet[offset++] = (Byte)(port >> 8);
            packet[offset++] = (Byte)(port);

            return offset;
        }
        public static Int32 MakeRequest(Byte[] packet, SocksCommand command, Byte[] domainName, Int32 port)
        {
            packet[0] = 0x05;          // Version 5
            packet[1] = (Byte)command;
            packet[2] = 0;             // Reserved

            packet[3] = (Byte)SocksAddressType.DomainName;
            packet[4] = (Byte)domainName.Length;

            Int32 offset;
            Array.Copy(domainName, 0, packet, 5, domainName.Length);
            offset = 5 + domainName.Length;

            packet[offset++] = (Byte)(port >> 8);
            packet[offset++] = (Byte)(port);

            return offset;
        }
        public static Int32 MakeRequest(Byte[] packet, SocksCommand command, String asciiHostName, Int32 port)
        {
            packet[0] = 0x05;          // Version 5
            packet[1] = (Byte)command;
            packet[2] = 0;             // Reserved

            packet[3] = (Byte)SocksAddressType.DomainName;
            packet[4] = (Byte)asciiHostName.Length;

            Int32 offset = 5;
            for (int i = 0; i < asciiHostName.Length; i++)
            {
                packet[offset++] = (Byte)asciiHostName[i];
            }

            packet[offset++] = (Byte)(port >> 8);
            packet[offset++] = (Byte)(port);

            return offset;
        }
        public static void ReadAndIgnoreAddress(Socket socket, Byte[] buffer)
        {
            socket.ReadFullSize(buffer, 0, 5);

            if (buffer[0] != 5) throw new InvalidOperationException(String.Format(
                "The first byte of the proxy response was expected to be 5 (for SOCKS5 version) but it was {0}", buffer[0]));

            if (buffer[1] != 0) throw new Proxy5Exception(buffer[1]);

            Int32 addressLength;
            switch (buffer[3])
            {
                case (Byte)SocksAddressType.IPv4:
                    addressLength = 4;
                    break;
                case (Byte)SocksAddressType.DomainName:
                    addressLength = buffer[4];
                    break;
                case (Byte)SocksAddressType.IPv6:
                    addressLength = 16;
                    break;
                default:
                    throw new Proxy5Exception(String.Format("Expected Address type to be {0} ({1}),{2} ({3}), or {4} ({5}), but got {6}",
                        SocksAddressType.IPv4, (Int32)SocksAddressType.IPv4,
                        SocksAddressType.DomainName, (Int32)SocksAddressType.DomainName,
                        SocksAddressType.IPv6, (Int32)SocksAddressType.IPv6, buffer[3]));
            }
            socket.ReadFullSize(buffer, 5, addressLength + 1);
        }
        /*
        public static EndPoint ReadReply(Socket socket, Byte[] buffer)
        {
            socket.ReadFullSize(buffer, 0, 5);

            if (buffer[0] != 5) throw new InvalidOperationException(String.Format(
                "The first byte of the proxy response was expected to be 5 (for SOCKS5 version) but it was {0}", buffer[0]));

            if (buffer[1] != 0) throw new Proxy5Exception(buffer[1]);

            Int32 port;
            switch (buffer[3])
            {
                case (Byte)SocksAddressType.IPv4:
                    socket.ReadFullSize(buffer, 5, 5);

                    Byte[] ipv4Addr = new Byte[4];
                    ipv4Addr[0] = buffer[4];
                    ipv4Addr[1] = buffer[5];
                    ipv4Addr[2] = buffer[6];
                    ipv4Addr[3] = buffer[7];
                    port = (buffer[8] << 8 | buffer[9]);
                    return new IPEndPoint(new IPAddress(ipv4Addr), port);

                case (Byte)SocksAddressType.DomainName:
                    Int32 domainLength = buffer[4];
                    socket.ReadFullSize(buffer, 5, domainLength + 1);

                    String domainName = Encoding.UTF8.GetString(buffer, 5, domainLength);
                    port = (buffer[domainLength + 5] << 8 | buffer[domainLength + 6]);

                    return new DnsEndPoint(domainName, port, DnsPriority.IPv4ThenIPv6, true);

                case (Byte)SocksAddressType.IPv6:

                    Byte[] ipv6Addr = new Byte[16];
                    Array.Copy(buffer, 4, ipv6Addr, 0, 16);
                    port = (buffer[20] << 8 | buffer[21]);

                    return new IPEndPoint(new IPAddress(ipv6Addr), port);
                default:
                    throw new Proxy5Exception(String.Format("Expected Address type to be {0} ({1}),{2} ({3}), or {4} ({5}), but got {6}",
                        SocksAddressType.IPv4, (Int32)SocksAddressType.IPv4,
                        SocksAddressType.DomainName, (Int32)SocksAddressType.DomainName,
                        SocksAddressType.IPv6, (Int32)SocksAddressType.IPv6, buffer[3]));
            }
        }
        */
    }
    public class ProxyException : SystemException
    {
        public ProxyException(String message)
            : base(message)
        {
        }
    }
    public class Proxy4Exception : ProxyException
    {
        public static String Proxy4ResultCodeToMessage(Byte resultCode)
        {
            switch (resultCode)
            {
                case 90:
                    return "request granted";
                case 91:
                    return "request rejected or failed";
                case 92:
                    return "request rejected because SOCKS server cannot connect to identd on the client";
                case 93:
                    return "request rejected because the client program and identd report different user-ids";
                default:
                    return String.Format("Unknown Proxy4 Result Code {0}", resultCode);
            }
        }

        public readonly Int32 resultCode;

        public Proxy4Exception(byte resultCode)
            : base(Proxy4ResultCodeToMessage(resultCode))
        {
            this.resultCode = resultCode;
        }
    }
    public class Proxy5Exception : ProxyException
    {
        public static String Proxy5ResultCodeToMessage(byte resultCode)
        {
            switch (resultCode)
            {
                case 0:
                    return "request granted";
                case 1:
                    return "general failure";
                case 2:
                    return "connection not allowed by ruleset";
                case 3:
                    return "network unreachable";
                case 4:
                    return "host unreachable";
                case 5:
                    return "connection refused by destination host";
                case 6:
                    return "TTL expired";
                case 7:
                    return "command not supported / protocol error";
                case 8:
                    return "address type not supported";
                default:
                    return String.Format("Unknown Proxy5 Result Code {0}", resultCode);
            }
        }
        public Proxy5Exception(byte resultCode)
            : base(Proxy5ResultCodeToMessage(resultCode))
        {
        }
        public Proxy5Exception(String message)
            : base(message)
        {

        }
    }

    public static class SocksProxy
    {
        public const Byte ProxyVersion4 = 0x04;
        public const Byte ProxyVersion5 = 0x05;

        public const Byte ProxyVersion4ConnectFunction = 0x01;
        public const Byte ProxyVersion4BindFunction = 0x02;

        public static void RequestBind(String proxyHost, UInt16 proxyPort, IPAddress bindIP, UInt16 bindPort, byte[] userID)
        {
            if (userID == null) throw new ArgumentNullException("userID");

            byte[] buffer = new byte[9 + userID.Length];

            //
            // Initialize BIND Request Packet
            //
            buffer[0] = 4; // Version 4 of SOCKS protocol
            buffer[1] = 2; // Command 2 "BIND"

            // Insert ipAddress and port into connectRequest packet
            buffer[2] = (byte)(bindPort >> 8);
            buffer[3] = (byte)(bindPort);

            byte[] bindIPArray = bindIP.GetAddressBytes();
            buffer[4] = bindIPArray[0];
            buffer[5] = bindIPArray[1];
            buffer[6] = bindIPArray[2];
            buffer[7] = bindIPArray[3];

            Int32 offset = 8;
            for (int i = 0; i < userID.Length; i++)
            {
                buffer[offset++] = userID[i];
            }
            buffer[offset] = 0;

            Socket proxySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            proxySocket.Connect(proxyHost, proxyPort);
            proxySocket.Send(buffer);
            proxySocket.ReadFullSize(buffer, 0, 8);


            if (buffer[0] != 0)
            {
                Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 0 but it was {0}", buffer[0]);
            }

            if (buffer[1] != 90) throw new Proxy4Exception(buffer[1]);

            byte[] responseIPArray = new byte[4];
            responseIPArray[0] = buffer[4];
            responseIPArray[2] = buffer[6];
            responseIPArray[3] = buffer[7];
            responseIPArray[4] = buffer[8];

            IPAddress responseIP = new IPAddress(responseIPArray);
            UInt16 responsePort = (UInt16)((responseIPArray[2] << 8) | responseIPArray[3]);
            Console.WriteLine("Response IP={0}, Port={1}", responseIP, responsePort);
        }
    }


    // TODO: Modify connect to use buffer that is passed in
    public class Socks4Proxy : Proxy
    {
        readonly Byte[] userID;
        public Socks4Proxy(InternetHost host, byte[] userID)
            : base(host)
        {
            this.userID = userID;
        }
        public override ProxyType Type
        {
            get { return ProxyType.Socks4; }
        }
        public override void ProxyConnectUdp(Socket socket, ref StringEndPoint endPoint)
        {
            throw new NotImplementedException();
        }
        public override void ProxyConnectTcp(Socket socket, ref StringEndPoint endPoint,
            ProxyConnectOptions options, ref BufStruct buf)
        {
            BufStruct forwardBufStruct = default(BufStruct);
            host.Connect(socket, DnsPriority.IPv4ThenIPv6, ProxyConnectOptions.None, ref forwardBufStruct);
            if (forwardBufStruct.contentLength > 0)
            {
                throw new NotImplementedException();
            }

            if (endPoint.stringIsAnIP)
            {
                SetupProxyWithIP(socket, endPoint.ipEndPoint);
            }
            else
            {
                SetupProxyWithHost(socket, endPoint.ipOrHost, endPoint.port);
            }
        }
        public override Boolean ProxyConnectAsyncTcp(Socket socket, ref StringEndPoint endPoint,
            ProxyConnectOptions options, ref BufStruct buf)
        {
            throw new NotImplementedException();
        }
        public Socket ListenAndAccept(UInt16 port)
        {
            Byte[] bindRequest = new Byte[9];
            Byte[] replyBuffer = new Byte[8];

            bindRequest[0] = 4; // Version 4 of SOCKS protocol
            bindRequest[1] = 2; // Command 2 "BIND"

            bindRequest[2] = (byte)(port >> 8);
            bindRequest[3] = (byte)(port);

            bindRequest[4] = 174;
            bindRequest[5] = 34;
            bindRequest[6] = 174;
            bindRequest[7] = 4;

            bindRequest[8] = 0; // User ID

            //
            // Connect to the proxy server
            //
            Socket socket = new Socket(host.GetAddressFamilyForTcp(), SocketType.Stream, ProtocolType.Tcp);

            BufStruct dataLeftOverFromProxyReceive = default(BufStruct);
            host.Connect(socket, host.dnsPriorityQuery, ProxyConnectOptions.None, ref dataLeftOverFromProxyReceive);
            if (dataLeftOverFromProxyReceive.contentLength > 0)
            {
                throw new NotImplementedException("Data left over from proxy negotiation not implemented");
            }

            socket.Send(bindRequest);
            socket.ReadFullSize(replyBuffer, 0, 8);

            if (replyBuffer[0] != 0)
            {
                Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 0 but it was {0}", replyBuffer[0]);
            }

            if (replyBuffer[1] != 90) throw new Proxy4Exception(replyBuffer[1]);

            UInt16 listenPort = (UInt16)(
                (0xFF00 & (replyBuffer[2] << 8)) |
                (0xFF & (replyBuffer[3]))
                );
            Byte[] ip = new Byte[4];
            ip[0] = replyBuffer[4];
            ip[1] = replyBuffer[5];
            ip[2] = replyBuffer[6];
            ip[3] = replyBuffer[7];
            Console.WriteLine("EndPoint {0}:{1}", new IPAddress(ip).ToString(), listenPort);

            return socket;
        }
        public void SetupProxyWithHost(Socket socket, String asciiHostName, UInt16 port)
        {
            Byte[] replyBuffer = new Byte[8];
            Byte[] connectRequest = new Byte[10 + ((userID == null) ? 0 : userID.Length) + asciiHostName.Length];

            connectRequest[0] = 4; // Version 4 of SOCKS protocol
            connectRequest[1] = 1; // Command 1 "CONNECT"

            connectRequest[2] = (Byte)(port >> 8);
            connectRequest[3] = (Byte)(port);

            connectRequest[4] = 0;
            connectRequest[5] = 0;
            connectRequest[6] = 0;
            connectRequest[7] = 1;

            Int32 offset = 8;
            if (userID != null)
            {
                for (int i = 0; i < userID.Length; i++)
                {
                    connectRequest[offset++] = userID[i];
                }
            }
            connectRequest[offset++] = 0;

            //Console.WriteLine("[DEBUG] Setting up host name '{0}'", asciiHostName);
            for (int i = 0; i < asciiHostName.Length; i++)
            {
                //Console.WriteLine("[DEBUG] [{0}] '{1}' {2}", i, asciiHostName[i], (Byte)asciiHostName[i]);
                connectRequest[offset++] = (Byte)asciiHostName[i];
            }
            connectRequest[offset] = 0;

            //
            // send CONNECT, and read response
            //
            socket.Send(connectRequest);
            socket.ReadFullSize(replyBuffer, 0, 8);

            //if (replyBuffer[0] != 0)
            //{
            //    Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 0 but it was {0}", replyBuffer[0]);
            //}

            if (replyBuffer[1] != 90) throw new Proxy4Exception(replyBuffer[1]);
        }
        public void SetupProxyWithIP(Socket socket, IPEndPoint ipEndPoint)
        {
            Byte[] replyBuffer = new Byte[8];
            Byte[] connectRequest = new Byte[9 + ((userID == null) ? 0 : userID.Length)];

            connectRequest[0] = 4;                 // Version 4 of SOCKS protocol
            connectRequest[1] = 1;                 // Command 1 "CONNECT"            
            connectRequest[2] = (Byte)(ipEndPoint.Port >> 8);
            connectRequest[3] = (Byte)(ipEndPoint.Port);
            byte[] hostIPArray = ipEndPoint.Address.GetAddressBytes();
            connectRequest[4] = hostIPArray[0];
            connectRequest[5] = hostIPArray[1];
            connectRequest[6] = hostIPArray[2];
            connectRequest[7] = hostIPArray[3];
            Int32 offset = 8;
            if (userID != null)
            {
                for (int i = 0; i < userID.Length; i++)
                {
                    connectRequest[offset++] = userID[i];
                }
            }
            connectRequest[offset] = 0;

            //
            // send CONNECT, and read response
            //
            socket.Send(connectRequest);
            socket.ReadFullSize(replyBuffer, 0, 8);

            //if (replyBuffer[0] != 0)
            //{
            //    Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 0 but it was {0}", replyBuffer[0]);
            //}

            if (replyBuffer[1] != 90) throw new Proxy4Exception(replyBuffer[1]);
        }
    }
    // TODO: Modify connect to use buffer that is passed in
    public class Socks5NoAuthenticationConnectSocket : Proxy
    {
        private readonly Byte[] buffer;

        public Socks5NoAuthenticationConnectSocket(InternetHost host)
            : base(host)
        {
            //int maxAuthenticationBuffer = 3 + usernameBytes.Length + passwordBytes.Length;
            this.buffer = new Byte[21];
        }
        public override ProxyType Type { get { return ProxyType.Socks5; } }
        public override void ProxyConnectTcp(Socket socket, ref StringEndPoint endPoint,
            ProxyConnectOptions options, ref BufStruct buf)
        {
            BufStruct forwardBufStruct = default(BufStruct);
            host.Connect(socket, DnsPriority.IPv4ThenIPv6, ProxyConnectOptions.None, ref forwardBufStruct);
            if (forwardBufStruct.contentLength > 0)
            {
                throw new NotImplementedException();
            }

            if (endPoint.stringIsAnIP)
            {
                ProxyConnectTcpUsingIP(socket, endPoint.ipEndPoint.Address, endPoint.port);
            }
            else
            {
                ProxyConnectTcpUsingHost(socket, endPoint.ipOrHost, endPoint.port);
            }
        }
        public void ProxyConnectTcpUsingHost(Socket socket, String asciiHostName, UInt16 port)
        {
            Byte[] buffer = new Byte[10 + asciiHostName.Length];

            //
            // 1. Send Initial Greeting
            //
            buffer[0] = 5; // Version 5 of the SOCKS protocol
            buffer[1] = 1; // This class only supports 1 authentication protocol
            buffer[2] = 0; // The 'No Authentication' protocol
            socket.Send(buffer, 0, 3, SocketFlags.None);

            //
            // 2. Receive Initial Response
            //
            socket.ReadFullSize(buffer, 0, 2);
            if (buffer[0] != 5) throw new InvalidOperationException(String.Format(
                "The first byte of the proxy response was expected to be 5 (for SOCKS5 version) but it was {0}", buffer[0]));
            if (buffer[1] != 0) throw new Proxy5Exception(String.Format("Expected server's response to be 0 (Means no authenticaion), but it was {0}", buffer[1]));

            //
            // 3. Issue a CONNECT command
            //
            Int32 size = Socks5.MakeRequest(buffer, SocksCommand.Connect, asciiHostName, port);
            socket.Send(buffer, 0, size, SocketFlags.None);

            //
            // 4. Get Response
            //
            Socks5.ReadAndIgnoreAddress(socket, buffer);
        }
        void ProxyConnectTcpUsingIP(Socket socket, IPAddress ip, UInt16 port)
        {
            //
            // 1. Send Initial Greeting
            //
            buffer[0] = 5; // Version 5 of the SOCKS protocol
            buffer[1] = 1; // This class only supports 1 authentication protocol
            buffer[2] = 0; // The 'No Authentication' protocol
            socket.Send(buffer, 0, 3, SocketFlags.None);

            //
            // 2. Receive Initial Response
            //
            socket.ReadFullSize(buffer, 0, 2);
            if (buffer[0] != 5) throw new InvalidOperationException(String.Format(
                "The first byte of the proxy response was expected to be 5 (for SOCKS5 version) but it was {0}", buffer[0]));
            if (buffer[1] != 0) throw new Proxy5Exception(String.Format("Expected server's response to be 0 (Means no authenticaion), but it was {0}", buffer[1]));

            //
            // 3. Issue a CONNECT command
            //
            Int32 size = Socks5.MakeRequest(buffer, SocksCommand.Connect, ip, port);
            socket.Send(buffer, 0, size, SocketFlags.None);

            //
            // 4. Get Response
            //
            Socks5.ReadAndIgnoreAddress(socket, buffer);
        }

        public override void ProxyConnectUdp(Socket socket, ref StringEndPoint endPoint)
        {
            if (endPoint.stringIsAnIP)
            {
                throw new NotImplementedException();
            }
            else
            {
                ProxyConnectUdpUsingHost(socket, endPoint.ipOrHost, endPoint.port);
            }
        }
        public void ProxyConnectUdpUsingHost(Socket socket, String asciiHostName, Int32 port)
        {
            throw new NotImplementedException();

            /*
            Socket proxySocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            proxySocket.Connect(endPoint);

            Byte[] buffer = new Byte[10 + asciiHostName.Length];

            //
            // 1. Send Initial Greeting
            //
            buffer[0] = 5; // Version 5 of the SOCKS protocol
            buffer[1] = 1; // This class only supports 1 authentication protocol
            buffer[2] = 0; // The 'No Authentication' protocol
            proxySocket.Send(buffer, 0, 3, SocketFlags.None);

            //
            // 2. Receive Initial Response
            //
            proxySocket.ReadFullSize(buffer, 0, 2);
            if (buffer[0] != 5) throw new InvalidOperationException(String.Format(
                "The first byte of the proxy response was expected to be 5 (for SOCKS5 version) but it was {0}", buffer[0]));
            if (buffer[1] != 0) throw new Proxy5Exception(String.Format("Expected server's response to be 0 (Means no authenticaion), but it was {0}", buffer[1]));

            //
            // 3. Issue a CONNECT command
            //
            Int32 size = Socks5.MakeRequest(buffer, SocksCommand.Connect, asciiHostName, port);
            proxySocket.Send(buffer, 0, size, SocketFlags.None);

            //
            // 4. Get Response
            //
            EndPoint udpEndPoint = Socks5.ReadReply(proxySocket, buffer);

            Console.WriteLine("[Socks5Debug] Udp Associate '{0}'", udpEndPoint);
            socket.Connect(udpEndPoint);
            */
        }
        public override Boolean ProxyConnectAsyncTcp(Socket socket, ref StringEndPoint endPoint,
            ProxyConnectOptions options, ref BufStruct buf)
        {
            throw new NotImplementedException();
        }
    }

    /*
    public class Socks5UsernamePasswordConnectSocket : Proxy
    {
        readonly EndPoint proxyEndPoint;
        public readonly byte[] usernameBytes, passwordBytes;

        public readonly byte[] buffer;

        public Socks5UsernamePasswordConnectSocket(EndPoint proxyEndPoint, ProtocolType protocolType,
            String username, String password)
            : base(ProxyType.Socks5)
        {
            if (username == null || username.Length > 255) throw new ArgumentException("username must be a string, with Length <= 255", "username");
            if (password == null || password.Length > 255) throw new ArgumentException("password must be a string, with Length <= 255", "password");

            this.proxyEndPoint = proxyEndPoint;

            this.usernameBytes = Encoding.UTF8.GetBytes(username);
            this.passwordBytes = Encoding.UTF8.GetBytes(password);

            int maxAuthenticationBuffer = 3 + usernameBytes.Length + passwordBytes.Length;

            this.buffer = new byte[(maxAuthenticationBuffer > 21) ? maxAuthenticationBuffer : 21];
        }
        public override void Connect(Socket socket, EndPoint endPoint)
        {
            IPEndPoint ipEndPoint = (IPEndPoint)endPoint;
            ConnectUsingIP(socket, ipEndPoint.Address, ipEndPoint.Port);
        }
        public override void Connect(Socket socket, IPEndPoint endPoint)
        {
            ConnectUsingIP(socket, endPoint.Address, endPoint.Port);
        }
        public override void Connect(Socket socket, String ipOrHost, Int32 port)
        {
            IPAddress address;
            if (IPAddress.TryParse(ipOrHost, out address))
            {
                ConnectUsingIP(socket, address, port);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public void ConnectUsingIP(Socket socket, IPAddress ip, Int32 port)
        {
            Int32 offset;

            socket.Connect(proxyEndPoint);

            //
            // 1. Send Initial Greeting
            //
            buffer[0] = 5; // Version 5 of the SOCKS protocol
            buffer[1] = 1; // This class only supports 1 authentication protocol
            buffer[2] = 2; // Username/Password authenticaion protocol

            socket.Send(buffer, 0, 3, SocketFlags.None);
            
            //
            // 2. Receive Initial Response
            //
            GenericUtilities.ReadFullSize(socket, buffer, 0, 2);
            if (buffer[0] != 5)
            {
                Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 5 (for SOCKS5 version) but it was {0}", buffer[0]);
            }
            if (buffer[1] != 2) throw new Proxy5Exception(String.Format("Expected server's response to be 2 (Means to use Username/Password authenticaion), but it was {0}", buffer[1]));

            //
            // 3. Send Username/Password
            //
            buffer[0] = 1;
            buffer[1] = (byte)usernameBytes.Length;
            offset = 2;
            for (int i = 0; i < usernameBytes.Length; i++)
            {
                buffer[offset++] = usernameBytes[i]; 
            }
            buffer[offset++] = (byte)passwordBytes.Length;
            for (int i = 0; i < passwordBytes.Length; i++)
            {
                buffer[offset++] = passwordBytes[i];
            }
            socket.Send(buffer, 0, offset, SocketFlags.None);

            //
            // 4. Get Response
            //
            GenericUtilities.ReadFullSize(socket, buffer, 0, 2);
            if (buffer[0] != 5)
            {
                Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 5 (for SOCKS5 version) but it was {0}", buffer[0]);
            }
            if (buffer[1] != 0) throw new Proxy5Exception(String.Format("Expected server's response to be 0 (Means authentication succeeded), but it was {0}", buffer[1]));

            //
            // 5. Issue a CONNECT command
            //
            buffer[0] = 5; // Version 5 of the SOCKS protocol
            buffer[1] = 1; // The CONNECT Command
            buffer[2] = 0; // Reserved
            buffer[3] = (Byte)SocksAddressType.IPv4; // 1 = IPv4 Address (3 = DomainName, 4 = IPv6 Address)
            byte[] hostIPArray = ip.GetAddressBytes();
            buffer[4] = hostIPArray[0];
            buffer[5] = hostIPArray[1];
            buffer[6] = hostIPArray[2];
            buffer[7] = hostIPArray[3];
            buffer[8] = (byte)(port >> 8);
            buffer[9] = (byte)(port);
            socket.Send(buffer, 0, 10, SocketFlags.None);

            //
            // 5. Get Response
            //
            GenericUtilities.ReadFullSize(socket, buffer, 0, 7);
            if (buffer[0] != 5) Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 5 (for SOCKS5 version) but it was {0}", buffer[0]);
            if (buffer[1] != 0) throw new Proxy5Exception(buffer[1]);
            if (buffer[2] != 0) Console.WriteLine("WARNING: The third byte of the proxy response was expected to be 0 (It is RESERVED) but it was {0}", buffer[2]);

            switch(buffer[3])
            {
                case (Byte)SocksAddressType.IPv4:
                    GenericUtilities.ReadFullSize(socket, buffer, 7, 3);
                    
                    //port = (UInt16) ( (buffer[8] << 8) | buffer[9] );
                    break;
                case (Byte)SocksAddressType.DomainName:
                    byte[] domainNameArray = new byte[buffer[4]]; 
                    Int32 bytesLeft = domainNameArray.Length - 2;
                    if(bytesLeft > 0)
                    {
                        GenericUtilities.ReadFullSize(socket, buffer, 7, bytesLeft);
                    }
                    offset = 5;
                    for(int i = 0; i < domainNameArray.Length; i++)
                    {
                        domainNameArray[i] = buffer[offset++];
                    }
                    String domainName = Encoding.UTF8.GetString(domainNameArray);

                    //port = (UInt16) ( (buffer[offset] << 8) | buffer[offset + 1] );
                    break;
                case (Byte)SocksAddressType.IPv6:
                    GenericUtilities.ReadFullSize(socket, buffer, 7, 15);
                    
                    //port = (UInt16) ( (buffer[20] << 8) | buffer[21] );
                    break;
                default:
                    throw new Proxy5Exception(String.Format("Expected Address type to be 1, 3, or 4, but got {0}", buffer[3]));
            }
        }
    }
    */
}
