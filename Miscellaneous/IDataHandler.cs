// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.Sockets;

namespace More
{
    public delegate void DataHandler(Byte[] data, UInt32 offset, UInt32 length);
    public interface IDataHandler : IDisposable
    {
        void HandleData(Byte[] data, UInt32 offset, UInt32 length);
    }
    
    public interface IDataFilter
    {
        void FilterTo(DataHandler handler, Byte[] data, UInt32 offset, UInt32 length);
    }

    public class DataFilterHandler : IDataHandler
    {
        readonly IDataFilter filter;
        readonly DataHandler handler;
        readonly IDisposable disposable;
        public DataFilterHandler(IDataFilter filter, DataHandler dataHandler)
        {
            this.filter = filter;
            this.handler = dataHandler;
            this.disposable = null;
        }
        public DataFilterHandler(IDataFilter filter, IDataHandler iDataHandler)
        {
            this.filter = filter;
            this.handler = iDataHandler.HandleData;
            this.disposable = iDataHandler;
        }
        public void HandleData(Byte[] data, UInt32 offset, UInt32 length)
        {
            filter.FilterTo(handler, data, offset, length);
        }
        public void Dispose()
        {
            if (disposable != null) disposable.Dispose();
        }
    }

    public interface IDataHandlerChainFactory
    {
        IDataHandler CreateDataHandlerChain(IDataHandler passTo);
    }
    public class SocketSendDataHandler : IDataHandler
    {
        public readonly Socket socket;
        public SocketSendDataHandler(Socket socket)
        {
            this.socket = socket;
        }
        public void Dispose()
        {
            socket.ShutdownAndDispose();
        }
        public void HandleData(Byte[] data, UInt32 offset, UInt32 length)
        {
            socket.Send(data, (Int32)offset, (Int32)length, SocketFlags.None);
        }
    }
    public class SocketSendToDataHandler : IDataHandler
    {
        public readonly Socket socket;
        public readonly EndPoint to;
        public SocketSendToDataHandler(Socket socket, EndPoint to)
        {
            this.socket = socket;
            this.to = to;
        }
        public void Dispose()
        {
            socket.ShutdownAndDispose();
        }
        public void HandleData(Byte[] data, UInt32 offset, UInt32 length)
        {
            socket.SendTo(data, (Int32)offset, (Int32)length, SocketFlags.None, to);
        }
    }
}
