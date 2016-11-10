// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
//#define DEBUG_HTTP_DATA

using System;
using System.Net;
using System.Net.Sockets;

using System.Text;

namespace More.Net
{
    public delegate void ByteAppender(ITextBuilder builder);
    public struct HttpContent
    {
        // Option 1
        public readonly Byte[] contentAsBytes;
        // Option 2
        public readonly String contentAsString;
        public readonly Encoder contentAsStringEncoder;
        // Option 3
        public readonly ByteAppender contentAppender;

        public readonly String contentType;

        public HttpContent(Byte[] content)
            : this(content, null)
        {
        }
        public HttpContent(Byte[] content, String contentType)
        {
            // Option 1
            this.contentAsBytes          = content;
            // Option 2
            this.contentAsString         = null;
            this.contentAsStringEncoder  = null;
            // Option 3
            this.contentAppender         = null;

            this.contentType             = contentType;
        }
        public HttpContent(String content)
            : this(content, Encoder.Utf8, null)
        {
        }
        public HttpContent(String content, String contentType)
            : this(content, Encoder.Utf8, contentType)
        {
        }
        public HttpContent(String content, Encoder encoding)
            : this(content, encoding, null)
        {
        }
        public HttpContent(String content, Encoder encoding, String contentType)
        {
            // Option 1
            this.contentAsBytes          = null;
            // Option 2
            this.contentAsString         = content;
            this.contentAsStringEncoder  = encoding;
            // Option 3
            this.contentAppender         = null;

            this.contentType             = contentType;
        }
        public HttpContent(ByteAppender contentAppender)
            : this(contentAppender, (String)null)
        {
        }
        public HttpContent(ByteAppender contentAppender, String contentType)
        {
            // Option 1
            this.contentAsBytes          = null;
            // Option 2
            this.contentAsString         = null;
            this.contentAsStringEncoder  = null;
            // Option 3
            this.contentAppender         = contentAppender;

            this.contentType             = contentType;
        }
    }
    // TODO:
    //
    // 1. Should I support including the endpoint in the resource?
    //    i.e. GET http://domain.com/index.html HTTP/1.1 (includeEndpointInResource?) instead of
    //         GET /index.html HTTP/1.1
    //
    // 
    //
    //
    // Note: Host header is required...if no host then the header should still be included by empty
    public struct HttpRequest
    {
        public static readonly Byte[] VersionPart = new Byte[]
        {
            (Byte)' ', (Byte)'H', (Byte)'T', (Byte)'T', (Byte)'P', (Byte)'/', (Byte)'1',
            (Byte)'.', (Byte)'1', (Byte)'\r', (Byte)'\n'
        };

        // Request Info
        public String method;

        public String resource;
        public ByteAppender resourceAppender; // Only valid if resource is null

        // Header Options
        public String overrideHostHeader;
        public Boolean forcePortInHostHeader;

        public Byte[] extraHeaders;
        public ByteAppender extraHeadersAppender;

        // Content
        public HttpContent content;

        public HttpRequest(String method, String resource)
            : this(method, resource, default(HttpContent))
        {
        }
        public HttpRequest(String method, String resource, HttpContent content)
        {
            this.method = method;

            this.resource = resource;
            this.resourceAppender = null;

            this.overrideHostHeader = null;
            this.forcePortInHostHeader = false;

            this.extraHeaders = null;
            this.extraHeadersAppender = null;

            this.content = content;
        }
        public HttpRequest(String method, ByteAppender resourceAppender)
            : this(method, resourceAppender, default(HttpContent))
        {
        }
        public HttpRequest(String method, ByteAppender resourceAppender, HttpContent content)
        {
            this.method = method;

            this.resource = null;
            this.resourceAppender = resourceAppender;

            this.overrideHostHeader = null;
            this.forcePortInHostHeader = false;

            this.extraHeaders = null;
            this.extraHeadersAppender = null;

            this.content = content;
        }
        // Returns the request offset into the builder buffer
        public UInt32 Build(ExtendedByteBuilder builder, HttpClient client, Boolean keepAlive)
        {
            if (method == null)
                throw new InvalidOperationException("The HttpRequest method must be set");
            //
            // <METHOD> <resource> HTTP/1.1\r\n
            //
            builder.AppendAscii(method); // Todo: Should I verify that method is ascii beforehand?
            builder.AppendAscii(' ');
            if (!String.IsNullOrEmpty(resource))
            {
                builder.AppendAscii(resource); // Todo: Should I verify that resource is ascii beforehand?
            }
            else if (resourceAppender != null)
            {
                resourceAppender(builder);
            }
            else
                throw new InvalidOperationException("The HttpRequest resource must be set");
            builder.Append(VersionPart);
            //
            // Host: <host>[:<port>]
            //
            builder.Append(Http.HostHeaderPrefix);
            if (overrideHostHeader != null)
            {
                builder.AppendAscii(overrideHostHeader);
            }
            else
            {
                builder.AppendAscii(client.IPOrHost);
                if (client.Port != 80 || forcePortInHostHeader)
                {
                    builder.AppendAscii(':');
                    builder.AppendNumber(client.Port, 10);
                }
            }
            builder.Append(Http.Newline);
            //
            // Header: Content-Length
            //
            // TODO: when do I not need a Content-Length?
            UInt32 contentLengthOffset = UInt32.MaxValue;
            {
                Boolean hasContent;
                UInt32 contentLength;
                if (content.contentAsBytes != null)
                {
                    hasContent = true;
                    contentLength = (UInt32)content.contentAsBytes.Length;
                }
                else if (content.contentAsString != null)
                {
                    hasContent = true;
                    contentLength = content.contentAsStringEncoder.GetEncodeLength(content.contentAsString);
                }
                else if (content.contentAppender != null)
                {
                    hasContent = true;
                    contentLength = UInt32.MaxValue; // Placeholder
                }
                else
                {
                    hasContent = false;
                    contentLength = 0;
                }
                if (hasContent)
                {
                    builder.Append(Http.ContentLengthHeaderPrefix);
                    contentLengthOffset = builder.contentLength;
                    builder.AppendNumber(contentLength);
                    builder.Append(Http.Newline);
                    if (content.contentType != null)
                    {
                        builder.Append(Http.ContentTypeHeaderPrefix);
                        builder.AppendAscii(content.contentType);
                        builder.Append(Http.Newline);
                    }
                }
            }
            //
            // Header: Connection
            //
            if (keepAlive)
            {
                builder.Append(Http.ConnectionHeaderPrefix);
                builder.Append(Http.ConnectionKeepAlive);
                builder.Append(Http.Newline);
            }
            else
            {
                builder.Append(Http.ConnectionHeaderPrefix);
                builder.Append(Http.ConnectionClose);
                builder.Append(Http.Newline);
            }
            //
            // Extra Headers
            //
            if (extraHeaders != null)
            {
                builder.Append(extraHeaders);
            }
            if (extraHeadersAppender != null)
            {
                extraHeadersAppender(builder);
            }
            //
            // End of Headers \r\n\r\n
            //
            builder.Append(Http.Newline);
            //
            // Content
            //
            if (content.contentAsBytes != null)
            {
                builder.Append(content.contentAsBytes);
            }
            else if (content.contentAsString != null)
            {
                builder.Append(content.contentAsStringEncoder, content.contentAsString);
            }
            else if (content.contentAppender != null)
            {
                //
                // Get the content
                //
                var contentStart = builder.contentLength;
                content.contentAppender(builder);
                UInt32 contentLength = builder.contentLength - contentStart;
                // Patch the request with the new content length
                UInt32 shift; // Shift everything before Content-Length: value to the right
                if (contentLength == 0)
                {
                    builder.bytes[contentLengthOffset + 9] = (Byte)'0';
                    shift = 9; // Shift everything
                }
                else
                {
                    shift = 9;
                    var temp = contentLength;
                    while(true)
                    {
                        builder.bytes[contentLengthOffset + shift] = (Byte)('0' + (temp % 10));
                        temp = temp / 10;
                        if (temp == 0)
                            break;
                        shift--;
                    }
                }
                // Shift the beginning of the request to compensate for a smaller Content-Length
                if (shift > 0)
                {
                    var offset = contentLengthOffset - 1;
                    while (true)
                    {
                        builder.bytes[offset + shift] = builder.bytes[offset];
                        if (offset == 0)
                            break;
                        offset--;
                    }
                }
                return shift;
            }
            return 0;
        }
    }
    /*
    public struct HttpResponse
    {
        public readonly UInt16 responseCode;
        public String responseMessage;

        public Byte[] extraHeaders;
        public ByteAppender extraHeadersAppender;

        // Content
        public HttpContent content;

        public HttpResponse(UInt16 responseCode)
            : this(responseCode, default(HttpContent))
        {
        }
        public HttpResponse(UInt16 responseCode, HttpContent content)
        {
            this.responseCode = responseCode;
            this.responseMessage = null;

            this.extraHeaders = null;
            this.extraHeadersAppender = null;

            this.content = content;
        }
        // Returns the request offset into the builder buffer
        public UInt32 Build(ITextBuilder builder, HttpClient client, Boolean keepAlive)
        {
            //
            // HTTP/1.1 <code> <message>\r\n
            //
            builder.AppendAscii(Http.VersionBytes);
            builder.AppendAscii(' ');
            builder.AppendNumber(responseCode);
            if (responseMessage == null)
            {
                Http.ResponseMessageMap.TryGetValue(responseCode, out responseMessage);
            }

            if (responseMessage != null)
            {
                builder.AppendAscii(' ');
                builder.AppendAscii(responseMessage);
            }

            builder.AppendAscii(Http.Newline);

            //
            // Header: Content-Length
            //
            // TODO: when do I not need a Content-Length?
            UInt32 contentLengthOffset = UInt32.MaxValue;
            {
                Boolean hasContent;
                UInt32 contentLength;
                if (content.contentAsBytes != null)
                {
                    hasContent = true;
                    contentLength = (UInt32)content.contentAsBytes.Length;
                }
                else if (content.contentAsString != null)
                {
                    hasContent = true;
                    contentLength = content.contentAsStringEncoder.GetEncodeLength(content.contentAsString);
                }
                else if (content.contentAppender != null)
                {
                    hasContent = true;
                    contentLength = UInt32.MaxValue; // Placeholder
                }
                else
                {
                    hasContent = false;
                    contentLength = 0;
                }
                if (hasContent)
                {
                    builder.AppendAscii(Http.ContentLengthHeaderPrefix);
                    contentLengthOffset = builder.Length;
                    builder.AppendNumber(contentLength);
                    builder.AppendAscii(Http.Newline);
                    if (content.contentType != null)
                    {
                        builder.AppendAscii(Http.ContentTypeHeaderPrefix);
                        builder.AppendAscii(content.contentType);
                        builder.AppendAscii(Http.Newline);
                    }
                }
            }
            //
            // Header: Connection
            //
            if (keepAlive)
            {
                builder.AppendAscii(Http.ConnectionHeaderPrefix);
                builder.AppendAscii(Http.ConnectionKeepAlive);
                builder.AppendAscii(Http.Newline);
            }
            else
            {
                builder.AppendAscii(Http.ConnectionHeaderPrefix);
                builder.AppendAscii(Http.ConnectionClose);
                builder.AppendAscii(Http.Newline);
            }
            //
            // Extra Headers
            //
            if (extraHeaders != null)
            {
                builder.AppendAscii(extraHeaders);
            }
            if (extraHeadersAppender != null)
            {
                extraHeadersAppender(builder);
            }
            //
            // End of Headers \r\n\r\n
            //
            builder.AppendAscii(Http.Newline);
            //
            // Content
            //
            if (content.contentAsBytes != null)
            {
                builder.AppendAscii(content.contentAsBytes);
            }
            else if (content.contentAsString != null)
            {
                builder.Append(content.contentAsStringEncoder, content.contentAsString);
            }
            else if (content.contentAppender != null)
            {
                throw new NotImplementedException();
                //
                // Get the content
                //
                var contentStart = builder.Length;
                content.contentAppender(builder);
                UInt32 contentLength = builder.Length - contentStart;
                // Patch the request with the new content length
                UInt32 shift; // Shift everything before Content-Length: value to the right
                if (contentLength == 0)
                {
                    builder.bytes[contentLengthOffset + 9] = (Byte)'0';
                    shift = 9; // Shift everything
                }
                else
                {
                    shift = 9;
                    var temp = contentLength;
                    while (true)
                    {
                        builder.bytes[contentLengthOffset + shift] = (Byte)('0' + (temp % 10));
                        temp = temp / 10;
                        if (temp == 0)
                            break;
                        shift--;
                    }
                }
                // Shift the beginning of the request to compensate for a smaller Content-Length
                if (shift > 0)
                {
                    var offset = contentLengthOffset - 1;
                    while (true)
                    {
                        builder.bytes[offset + shift] = builder.bytes[offset];
                        if (offset == 0)
                            break;
                        offset--;
                    }
                }
                return shift;
            }
            return 0;
        }
    }
    */
    public delegate void Callback(ByteBuilder builder, UInt32 offset);
    public class HttpClient
    {
        Socket socket;

        StringEndPoint serverEndPoint;
        Proxy proxy;

        Callback requestLogger;
        public Callback RequestLogger { set { this.requestLogger = value; } }

        public String IPOrHost { get { return serverEndPoint.ipOrHost; } }
        public UInt16 Port { get { return serverEndPoint.port; } }

        PriorityQuery<IPAddress> dnsPriorityQuery = DnsPriority.IPv4ThenIPv6;
        public PriorityQuery<IPAddress> DnsPriorityQuery
        {
            get { return dnsPriorityQuery; }
            set { dnsPriorityQuery = value; }
        }
        public HttpClient(String ipOrHost, UInt16 port)
        {
            this.serverEndPoint = new StringEndPoint(ipOrHost, port);
        }
        public HttpClient(StringEndPoint serverEndPoint)
        {
            this.serverEndPoint = serverEndPoint;
        }
        public Proxy Proxy
        {
            set
            {
                if (socket != null && socket.Connected)
                {
                    throw new InvalidOperationException("Cannot set Proxy while the HttpClient is connected");
                }
                proxy = value;
            }
        }
        void Open(BufStruct buf)
        {
            if (socket == null || !socket.Connected)
            {
                if (proxy == null)
                {
                    serverEndPoint.ForceIPResolution(DnsPriority.IPv4ThenIPv6);
                    socket = new Socket(serverEndPoint.ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(serverEndPoint.ipEndPoint);
                }
                else
                {
                    socket = new Socket(proxy.host.GetAddressFamilyForTcp(), SocketType.Stream, ProtocolType.Tcp);
                    proxy.ProxyConnectTcp(socket, ref serverEndPoint, ProxyConnectOptions.ContentIsRawHttp, ref buf);
                }
            }
        }
        static readonly String HttpResponseEndedMessage = "The HTTP response ended prematurely";
        public static UInt16 ParseReturnCode(Byte[] bytes, UInt32 offset, UInt32 limit)
        {
            while (true)
            {
                if (offset >= limit)
                    throw new FormatException(HttpResponseEndedMessage);

                var c = bytes[offset];
                if (c == ' ')
                {
                    var start = offset;
                    while (true)
                    {
                        offset++;
                        if (offset >= limit)
                            throw new FormatException(HttpResponseEndedMessage);
                        c = bytes[offset];
                        if (c == ' ')
                            break;
                    }
                    return UInt16.Parse(Encoding.ASCII.GetString(bytes, (Int32)start, (Int32)(offset - start)));
                }
                offset++;
            }
        }
        // Returns offset into response content
        public UInt32 Request(ref HttpRequest request, Boolean keepAlive, ExtendedByteBuilder builder)
        {
            builder.Clear();
            UInt32 requestOffset = request.Build(builder, this, keepAlive);

            if(requestLogger != null)
                requestLogger(builder, requestOffset);
            UInt32 retryCount = 0;

        RETRY:
            Open(default(BufStruct));

            Boolean clearedRequest = false;
            UInt32 responseContentOffset;
            try
            {
                socket.Send(builder.bytes, (int)requestOffset, (int)(builder.contentLength - requestOffset), 0);

                builder.Clear(); // clear the request for using it as the receive buffer
                clearedRequest = true;

                responseContentOffset = ReadResponse(keepAlive, builder);
            }
            catch (SocketException)
            {
                if (retryCount > 0)
                    throw;
                retryCount++;
                if (clearedRequest)
                    requestOffset = request.Build(builder, this, keepAlive); // Must rebuild the request
                goto RETRY;
            }
            return responseContentOffset;
        }
        // These number could be tweeked
        const UInt32 MinimumReceiveBuffer = 64;
        const UInt32 MinimumExpandReceiveBuffer = 256;
        // Returns offset into response content
        UInt32 ReadResponse(Boolean keepAlive, ByteBuilder builder)
        {
            UInt32 contentOffset;

            while (true)
            {
                Int32 bytesReceived = socket.Receive(builder.bytes, (int)builder.contentLength,
                    (int)(builder.bytes.Length - builder.contentLength), 0);
                if (bytesReceived <= 0)
                    throw new FormatException(String.Format("Socket.Receive returned {0} while reading response. Response so far is '{1}'",
                        bytesReceived, Encoding.UTF8.GetString(builder.bytes, 0, (int)builder.contentLength)));

                UInt32 checkOffset = builder.contentLength;
                builder.contentLength += (UInt32)bytesReceived;

                // Search for the ending \r\n\r\n
                Byte[] checkBuffer = builder.bytes;
                if (checkOffset >= 3)
                    checkOffset -= 3; // Reverse 3 chars in case the last request had the partial end of double newline
                for (; checkOffset + 3 < builder.contentLength; checkOffset++)
                {
                    if (checkBuffer[checkOffset    ] == (Byte)'\r' &&
                        checkBuffer[checkOffset + 1] == (Byte)'\n' &&
                        checkBuffer[checkOffset + 2] == (Byte)'\r' &&
                        checkBuffer[checkOffset + 3] == (Byte)'\n')
                    {
                        contentOffset = checkOffset + 4;
                        goto DONE_WITH_HEADERS;
                    }
                }

                // Expand the receive buffer if necessary
                if (builder.bytes.Length - builder.contentLength < MinimumReceiveBuffer)
                {
                    builder.EnsureTotalCapacity(builder.contentLength + MinimumExpandReceiveBuffer);
                }
            }
        DONE_WITH_HEADERS:

            //
            // Get Content Length
            //
            // TODO: verify this works
            UInt32 contentLengthFromHeader = Http.GetContentLength(builder.bytes, 0, builder.contentLength);


            //
            // TODO: Determine Message Length properly
            // 1. If the response cannot include a message body then it MUST NOT have any content (such
            //    as 1xx, 204, 304, or any HEAD response)
            // 2. If a Transfer-Encoding header field is present with any other value besides "identity", then
            //    the transfer-length is defined by use of the "chunked" transfer-coding unless the message
            //    is terminated by closing the connection
            // 3. Use the Content-Length header
            // 4. Using Multipart/byteranges....???
            // 5. Server closes the connection


            //
            // Read the rest of the response
            //
            if (contentLengthFromHeader == UInt32.MaxValue)
            {
                if (keepAlive)
                {
                    // TODO: check the response code
                    // The server did not include a Content-Length HTTP header
                    // This is problematic if the socket connection is Keep-Alive
                    //     Note: I am currently assuming keepAlive from the client request...I should check the server header
                    //           for CloseConnection
                    throw new FormatException("The server did not include a Content-Length HTTP header but the socket connection is Keep-Alive");
                }
                else
                {
                    while (true)
                    {
                        Int32 bytesReceived = socket.Receive(builder.bytes, (int)builder.contentLength,
                            (int)(builder.bytes.Length - builder.contentLength), 0);
                        if (bytesReceived <= 0)
                            throw new FormatException(String.Format("Socket.Receive returned {0} while reading response. Response so far is '{1}'",
                                bytesReceived, Encoding.UTF8.GetString(builder.bytes, 0, (int)builder.contentLength)));

                        builder.contentLength += (UInt32)bytesReceived;

                        // Expand the receive buffer if necessary
                        if (builder.bytes.Length - builder.contentLength < MinimumReceiveBuffer)
                        {
                            builder.EnsureTotalCapacity(builder.contentLength + MinimumExpandReceiveBuffer);
                        }
                    }
                }
            }
            else if (contentLengthFromHeader > 0)
            {
                UInt32 contentSoFar = builder.contentLength - contentOffset;
                Int32 contentLeft = (Int32)(contentLengthFromHeader - contentSoFar);
                if (contentLeft > 0)
                {
                    builder.EnsureTotalCapacity(builder.contentLength + (UInt32)contentLeft);
                    do
                    {
                        Int32 bytesReceived = socket.Receive(builder.bytes, (int)builder.contentLength,
                            (int)(builder.bytes.Length - builder.contentLength), 0);
                        if (bytesReceived <= 0)
                            throw new FormatException(String.Format("Socket.Receive returned {0} while reading response. Response so far is '{1}'",
                                bytesReceived, Encoding.UTF8.GetString(builder.bytes, 0, (int)builder.contentLength)));

                        builder.contentLength += (UInt32)bytesReceived;

                        // Expand the receive buffer if necessary
                        if (builder.bytes.Length - builder.contentLength < MinimumReceiveBuffer)
                        {
                            builder.EnsureTotalCapacity(builder.contentLength + MinimumExpandReceiveBuffer);
                        }
                        contentLeft -= bytesReceived;
                    } while (contentLeft > 0);
                }
            }

            if (!keepAlive)
            {
                socket.ShutdownAndDispose();
                socket = null;
            }

            return contentOffset;
        }
    }
}
