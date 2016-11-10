// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using More;

namespace More.Net
{
    /// <summary>
    /// Note: the only code that interacts with this class should be the AccepHandlers and ReceiveHandlers
    /// NOTE: Maybe there should be a LockingSelectServer that allows other threads to interact with it.
    /// NOTE: Maybe there should be a select server that allows sockets specify timeouts.
    /// </summary>
    public class SelectServer<T> where T : SelectServer<T>
    {
        /// <summary>Callback for sockets controlled by a select server.</summary>
        /// <param name="handlerObject">An object passed to the handler.</param>
        /// <param name="socket">A socket that popped the select call.</param>
        public delegate void Handler(T handlerObject, Socket socket);
        
        readonly IDictionary<Socket, Handler> map = new Dictionary<Socket, Handler>(); // contains mapping from socket to handlers
        readonly List<Socket> listenSockets       = new List<Socket>();
        readonly List<Socket> receiveSockets      = new List<Socket>();

        readonly List<Socket> connectList;
        Boolean connectionError;

        Thread runThread;

        /// <summary>Create a SelectServer classs.</summary>
        /// <param name="supportConnectSockets">True if the server is going to support selecting on connecting sockets.</param>
        public SelectServer(Boolean supportConnectSockets)
        {
            if(supportConnectSockets)
            {
                this.connectList = new List<Socket>();
            }
            else
            {
                this.connectList = null;
            }
        }

        /// <summary>
        /// True if supports connect sockets
        /// </summary>
        public Boolean SupportsConnectSockets
        {
            get { return connectList != null; }
        }

        /// <summary>The object passed back to the handler functions.</summary>
        protected virtual T HandlerObject
        {
            get { return (T)this; }
        }

        /// <summary>
        /// The select loop.  Will call select until all sockets are closed/removed.
        /// </summary>
        public void Run()
        {
            if (runThread != null)
            {
                throw new InvalidOperationException("SelectServer is already running");
            }
            runThread = Thread.CurrentThread;

            List<Socket> selectReadSockets = new List<Socket>();
            List<Socket> selectWriteSockets, selectErrorSockets;
            if (connectList != null)
            {
                selectWriteSockets = new List<Socket>();
                selectErrorSockets = new List<Socket>();
            }
            else
            {
                selectWriteSockets = null;
                selectErrorSockets = null;
            }

            // The select Loop
            while(true)
            {
                selectReadSockets.Clear();
                selectReadSockets.AddRange(listenSockets);
                selectReadSockets.AddRange(receiveSockets);

                if (selectWriteSockets == null || connectList.Count == 0)
                {
                    if (selectReadSockets.Count == 0)
                    {
                        return;
                    }

                    Socket.Select(selectReadSockets, null, null, Int32.MaxValue);
                }
                else
                {
                    selectWriteSockets.Clear();
                    selectErrorSockets.Clear();

                    if (selectReadSockets.Count == 0 && connectList.Count == 0)
                    {
                        return;
                    }

                    selectWriteSockets.AddRange(connectList);
                    selectErrorSockets.AddRange(connectList);


                    //Console.WriteLine("[DEBUG] Selecting on {0} connect sockets", connectSockets.Count);
                    Socket.Select(selectReadSockets, selectWriteSockets, selectErrorSockets, Int32.MaxValue);

                    connectionError = true;
                    for (int i = 0; i < selectErrorSockets.Count; i++)
                    {
                        Socket socket = selectErrorSockets[i];
                        Handler handler = TryGetHandler(socket);
                        if (handler != null)
                        {
                            handler(HandlerObject, socket);
                        }
                    }
                    connectionError = false;
                    for (int i = 0; i < selectWriteSockets.Count; i++)
                    {
                        Socket socket = selectWriteSockets[i];
                        // Make sure you don't call the handler twice
                        if (!selectErrorSockets.Contains(socket))
                        {
                            Handler handler = TryGetHandler(socket);
                            if (handler != null)
                            {
                                handler(HandlerObject, socket);
                            }
                        }
                    }
                }

                for (int i = 0; i < selectReadSockets.Count; i++)
                {
                    Socket socket = selectReadSockets[i];
                    Handler handler = TryGetHandler(socket);
                    if (handler != null)
                    {
                        handler(HandlerObject, socket);
                    }
                }
            }
        }
        
        /// <summary>
        /// Can use to check if your current thread is the same as the select server thread.
        /// Note: DO NOT SET, should only be set by the SelectServer.
        /// </summary>
        public Thread RunThread
        {
            get
            {
                return runThread;
            }
        }
        /// <summary>
        /// The total number of Receive/Listen/Connect sockets
        /// </summary>
        public UInt32 TotalSocketCount { get { return (uint)map.Count; } }

        /// <summary>
        /// A handler can use this to check whether or not a connect failed.
        /// </summary>
        public Boolean ConnectionError { get { return connectionError; } }

        /// <summary>Indicates that the control accepts connecting sockets.</summary>
        public Boolean SupportsConnect { get { return connectList != null; } }

        /// <summary>DO NOT MODIFY! Only use to access information.</summary>
        public ICollection<Socket> ListenSockets { get { return listenSockets; } }
        /// <summary>DO NOT MODIFY! Only use to access information.</summary>
        public ICollection<Socket> ReceiveSockets { get { return receiveSockets; } }
        /// <summary>DO NOT MODIFY! Only use to access information.</summary>
        public ICollection<Socket> ConnectSockets { get { return connectList; } }

        /// <summary>Returns true if there are currently any connect sockets.
        /// This should only be called if SupportsConnect is true.</summary>
        public Boolean HasConnectSockets
        {
            get
            {
                return connectList.Count > 0;
            }
        }

        /// <summary>
        /// Helper method for listen sockets.  Automatically adds the new socket
        /// to this class with the given handler.
        /// </summary>
        /// <param name="listenSocket"></param>
        /// <param name="handler"></param>
        public void PerformAccept(Socket listenSocket, Handler handler)
        {
            Socket newReceiveSocket = listenSocket.Accept();
            if (newReceiveSocket.Connected)
            {
                AddReceiveSocket(newReceiveSocket, handler);
            }
            else
            {
                newReceiveSocket.Close();
            }
        }

        /// <summary>
        /// Tries to retrieve the handler for the given listner, receiver, or connector socket.
        /// </summary>
        /// <param name="socket">A controlled socket</param>
        /// <returns>The handler if the socket is conrolled, otherwise returns null</returns>
        public Handler TryGetHandler(Socket socket)
        {
            Handler ret;
            return map.TryGetValue(socket, out ret) ? ret : null;
        }
        
        /// <summary>
        /// Copies all connect sockets to the given list.  Note: this function assumes
        /// that SupportsConnect is true.
        /// Typically used by the SelectServer.
        /// </summary>
        public void CopyConnectSocketsTo(List<Socket> sockets)
        {
            sockets.AddRange(connectList);
        }
        /// <summary>
        /// Update the handler for the given socket.
        /// </summary>
        /// <param name="socket">The socket to update the handler for.</param>
        /// <param name="handler">The new handler.</param>
        public void UpdateHandler(Socket socket, Handler handler)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            map[socket] = handler;
        }
        /// <summary>
        /// Move a socket from the connect list, to the receive list.
        /// </summary>
        /// <param name="socket">The socket to update.</param>
        /// <param name="handler">The new handler.</param>
        public void UpdateConnectorToReceiver(Socket socket, Handler handler)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            connectList.Remove(socket);
            receiveSockets.Add(socket);
            map[socket] = handler;
        }

        const String BadThreadMessage = "This thread may only be called on the SelectServer thread, or before the SelectServer has been started";

        /// <summary>
        /// Add a listen socket to the listen set.
        /// </summary>
        /// <param name="socket">The socket to add.</param>
        /// <param name="handler">The handler to call when it pops.</param>
        public void AddListenSocket(Socket socket, Handler handler)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            // Can only call this function if select server is not running or 
            // you are calling it from the select server thread
            Debug.Assert(runThread == null || runThread == Thread.CurrentThread, BadThreadMessage);

            listenSockets.Add(socket);
            map.Add(socket, handler);
        }
        /// <summary>
        /// Add a receive socket to the receive set.
        /// </summary>
        /// <param name="socket">The socket to add.</param>
        /// <param name="handler">The handler to call when it pops.</param>
        public void AddReceiveSocket(Socket socket, Handler handler)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            // Can only call this function if select server is not running or 
            // you are calling it from the select server thread
            Debug.Assert(runThread == null || runThread == Thread.CurrentThread, BadThreadMessage);

            receiveSockets.Add(socket);
            map.Add(socket, handler);
        }
        /// <summary>
        /// Remove a socket from the receive list.
        /// </summary>
        /// <param name="socket">The socket to remove.</param>
        public void RemoveReceiveSocket(Socket socket)
        {
            // Can only call this function if select server is not running or 
            // you are calling it from the select server thread
            Debug.Assert(runThread == null || runThread == Thread.CurrentThread, BadThreadMessage);

            receiveSockets.Remove(socket);
            map.Remove(socket);
        }
        /// <summary>
        /// Add a connect socket to the connect list.
        /// </summary>
        /// <param name="socket">The socket to add.</param>
        /// <param name="handler">The handler to call when it pops.</param>
        public void AddConnectSocket(Socket socket, Handler handler)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            // Can only call this function if select server is not running or 
            // you are calling it from the select server thread
            Debug.Assert(runThread == null || runThread == Thread.CurrentThread, BadThreadMessage);

            connectList.Add(socket);
            map.Add(socket, handler);
        }
        /// <summary>
        /// Remove the socket from the listen set and close it.
        /// </summary>
        /// <param name="socket">The socket to remove and close.</param>
        public void DisposeAndRemoveListenSocket(Socket socket)
        {
            // Can only call this function if select server is not running or 
            // you are calling it from the select server thread
            Debug.Assert(runThread == null || runThread == Thread.CurrentThread, BadThreadMessage);

            listenSockets.Remove(socket);
            map.Remove(socket);
            socket.Close();
        }
        /// <summary>
        /// Remove the socket from the receive set and close it.
        /// NOTE: does not call shutdown.
        /// </summary>
        /// <param name="socket">The socket to remove and close.</param>
        public void DisposeAndRemoveReceiveSocket(Socket socket)
        {
            // Can only call this function if select server is not running or 
            // you are calling it from the select server thread
            Debug.Assert(runThread == null || runThread == Thread.CurrentThread, BadThreadMessage);

            receiveSockets.Remove(socket);
            map.Remove(socket);
            socket.Close();
        }
        /// <summary>
        /// Remove the socket from the connect set and close it.
        /// </summary>
        /// <param name="socket">The socket to remove and close.</param>
        public void DisposeAndRemoveConnectSocket(Socket socket)
        {
            // Can only call this function if select server is not running or 
            // you are calling it from the select server thread
            Debug.Assert(runThread == null || runThread == Thread.CurrentThread, BadThreadMessage);

            connectList.Remove(socket);
            map.Remove(socket);
            socket.Close();
        }

        /// <summary>
        /// Shutdown, close, and remove receive socket.
        /// </summary>
        public void ShutdownDisposeAndRemoveReceiveSocket(Socket socket)
        {
            // Can only call this function if select server is not running or 
            // you are calling it from the select server thread
            Debug.Assert(runThread == null || runThread == Thread.CurrentThread, BadThreadMessage);

            receiveSockets.Remove(socket);
            map.Remove(socket);
            socket.ShutdownAndDispose();
        }

        /// <summary>
        /// Shutdown and close all sockets.
        /// If this is called from the RunThread of the SelectServer, then
        /// it will also remove all the sockets from the control.
        /// </summary>
        public void ShutdownAndCloseSockets()
        {
            for (int i = 0; i < listenSockets.Count; i++)
            {
                listenSockets[i].Close();
            }
            for (int i = 0; i < receiveSockets.Count; i++)
            {
                receiveSockets[i].ShutdownAndDispose();
            }
            if (connectList != null)
            {
                for (int i = 0; i < connectList.Count; i++)
                {
                    connectList[i].ShutdownAndDispose();
                }
                connectList.Clear();
            }
            if (Thread.CurrentThread == runThread)
            {
                listenSockets.Clear();
                receiveSockets.Clear();
                map.Clear();
                runThread = null;
            }
        }
    }
    public class SelectServerSharedBuffer : SelectServer<SelectServerSharedBuffer>
    {
        public Byte[] sharedBuffer;
        public SelectServerSharedBuffer(Boolean supportsConnectSockets, Byte[] sharedBuffer)
            : base(supportsConnectSockets)
        {
            this.sharedBuffer = sharedBuffer;
        }
        protected override SelectServerSharedBuffer HandlerObject
        {
            get { return this; }
        }
    }
}
