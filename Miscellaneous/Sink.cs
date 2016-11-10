// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Threading;

namespace More
{
    /// <summary>
    /// An output interface.
    /// </summary>
    public interface ISink : IDisposable
    {
        void Write(BytePtr ptr, UInt32 length);
        void Flush();
        BufferSlice GetAvailableBuffer();
    }
    public interface ITypedSink : ISink
    {
        void Write(Encoder encoder, String s, UInt32 offset, UInt32 length);
        void Write(Encoder encoder, CharPtr ptr, UInt32 length);
    }
    public abstract class TypedSink : ITypedSink
    {
        Boolean flushAtWriteLine;
        Encoder defaultEncoder;
        readonly Byte[] defaultEncoderNewline;
        public TypedSink(Boolean flushAtWriteLine, Encoder defaultEncoder)
        {
            this.flushAtWriteLine = flushAtWriteLine;
            this.defaultEncoder = (defaultEncoder == null) ? Encoder.Utf8 : null;

            this.defaultEncoderNewline = new Byte[defaultEncoder.GetEncodeLength(Environment.NewLine)];
            defaultEncoder.Encode(Environment.NewLine, this.defaultEncoderNewline, 0);
        }
        //
        // ISink Interface
        //
        public virtual void Dispose()
        {
        }
        public abstract void Write(BytePtr ptr, UInt32 length);
        public unsafe void Write(Byte[] message)
        {
            fixed (byte* ptr = message)
            {
                Write(ptr, (uint)message.Length);
            }
        }
        public unsafe void WriteLine(Byte[] message)
        {
            fixed (byte* ptr = message)
            {
                Write(ptr, (uint)message.Length);
            }
            Write(defaultEncoderNewline);
        }
        public unsafe void Write(Byte[] message, UInt32 offset, UInt32 length)
        {
            fixed (byte* ptr = message)
            {
                Write(ptr + offset, length);
            }
        }
        public unsafe void WriteLine(Byte[] message, UInt32 offset, UInt32 length)
        {
            fixed (byte* ptr = message)
            {
                Write(ptr + offset, length);
            }
            Write(defaultEncoderNewline);
        }
        public void WriteLine(BytePtr ptr, UInt32 length)
        {
            Write(ptr, length);
            Write(defaultEncoderNewline);
            if (flushAtWriteLine) Flush();
        }
        public abstract void Flush();
        public abstract BufferSlice GetAvailableBuffer();
        //
        // ITypedSink Interface
        //
        public abstract void Write(Encoder encoder, String s, UInt32 offset, UInt32 length);
        public abstract void Write(Encoder encoder, CharPtr ptr, UInt32 length);

        public void WriteLine()
        {
            Write(defaultEncoderNewline);
            if (flushAtWriteLine) Flush();
        }
        public void WriteLine(Encoder encoder)
        {
            Write(defaultEncoderNewline);
            if (flushAtWriteLine) Flush();
        }

        void Write(String s)
        {
            Write(defaultEncoder, s, 0, (uint)s.Length);
        }
        void Write(Encoder encoder, String s)
        {
            Write(encoder, s, 0, (uint)s.Length);
        }


        void WriteLine(String s)
        {
            Write(defaultEncoder, s, 0, (uint)s.Length);
            Write(defaultEncoderNewline);
            if (flushAtWriteLine) Flush();
        }
        void WriteLine(Encoder encoder, String s)
        {
            Write(encoder, s, 0, (uint)s.Length);
            Write(defaultEncoderNewline);
            if (flushAtWriteLine) Flush();
        }

        public static void DoNothingFlushHandler(BytePtr ptr, UInt32 length)
        {
        }
    }

    /*
    public class SinkHole : TypedSink
    {
        public SinkHole()
            : base(false, null)
        {
        }
        public override void Write(BytePtr ptr, uint length)
        {
        }
        public override void Flush()
        {
        }
        public override BufferSlice GetAvailableBuffer()
        {
            return new BufferSlice(IntPtr.Zero, 0);
        }
        public override void Write(Encoder encoder, string s, uint offset, uint length)
        {
        }
        public override void Write(Encoder encoder, CharPtr ptr, uint length)
        {
        }
    }
    */

    // Assumption: the buffer will be locked when this funtion is called
    public delegate void FlushHandler(BytePtr ptr, UInt32 length);

    /// <summary>
    /// An output interface that buffers it's input between writes.
    /// </summary>
    public class BufferedSink : TypedSink
    {
        /// <summary>
        /// There is a minimum buffer size so that we can guarantee there will be enough space in certain cases
        /// </summary>
        public const UInt32 MinimumBufferSize = 64;

        // If a write it called on a byte array that is greater than or equal to this,
        // then the buffer will be flushed and the new byte array will not be coped to the buffer but
        // will simply be passed to the FlushHandler
        const UInt32 DontCopyToBufferThreshold = 32;

        protected readonly BytePtr buffer;
        protected readonly BytePtr bufferLimit;
        protected readonly Boolean sync;
        protected readonly FlushHandler FlushHandler;

        protected BytePtr next; // Assumption: buffer <= next <= bufferLimit
        public BufferedSink(BytePtr buffer, BytePtr bufferLimit, Boolean flushAtWriteLine,
            Encoder defaultEncoder, FlushHandler flushHandler, Boolean sync)
            : base(flushAtWriteLine, defaultEncoder)
        {
            if (buffer >= bufferLimit)
            {
                throw new ArgumentException(String.Format("buffer {0} must be < bufferLimit {1}", buffer, bufferLimit));
            }
            if (bufferLimit.DiffWithSmallerNumber(buffer) < MinimumBufferSize)
            {
                throw new ArgumentException(String.Format("buffer length {0} is too small (minimum is {1})",
                    bufferLimit.DiffWithSmallerNumber(buffer), MinimumBufferSize));
            }
            if (flushHandler == null)
            {
                throw new ArgumentNullException("flushHandler");
            }

            this.buffer = buffer;
            this.bufferLimit = bufferLimit;
            this.sync = sync;
            this.FlushHandler = flushHandler;
            this.next = buffer;
        }
        public override void Write(BytePtr ptr, UInt32 length)
        {
            try
            {
                if (sync) Monitor.Enter(this);

                FlushHandler(buffer, next.DiffWithSmallerNumber(buffer));
                next = buffer;
                FlushHandler(ptr, length);
            }
            finally
            {
                if (sync) Monitor.Exit(this);
            }
        }
        public override void Flush()
        {
            try
            {
                if (sync) Monitor.Enter(this);
                FlushHandler(buffer, next.DiffWithSmallerNumber(buffer));
                next = buffer;
            }
            finally
            {
                if (sync) Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Note: if you would like to use this in a multithreaded application, you should
        /// lock the object while using the buffer if you want to prevent another thread from using it
        /// Example:
        ///   lock(buffer)
        ///   {
        ///       BufferSice slice = buffer.GetAvailableBuffer();
        ///       // fill in buffer
        ///       // ... need more buffer?
        ///       buffer.Flush();
        ///       slice = buffer.GetAvailableBuffer();
        ///       // ...
        ///   }
        /// </summary>
        /// <returns></returns>
        public override BufferSlice GetAvailableBuffer()
        {
            return new BufferSlice(next, bufferLimit.DiffWithSmallerNumber(next));
        }
        public override void Write(Encoder encoder, String s, UInt32 offset, UInt32 length)
        {
            throw new NotImplementedException();
        }
        public override void Write(Encoder encoder, CharPtr ptr, UInt32 length)
        {
            throw new NotImplementedException();
        }

        /*
        // Handle bigger writes
        if (length >= DontCopyToBufferThreshold)
        {
            // Flush if needed
            if (next > buffer)
            {
                FlushHandler(buffer, next.DiffWithSmallerNumber(buffer));
                next = buffer;
            }
            FlushHandler(ptr, length);
            return;
        }

        UInt32 bufferLeft = bufferLimit.DiffWithSmallerNumber(next);


        if (length > bufferLeft)
        {
            ptr.CopyTo(next, bufferLeft);
            FlushHandler(buffer, next.DiffWithSmallerNumber(buffer));
            next = buffer;

            length -= bufferLeft;
            ptr += bufferLeft;


            bufferLeft = bufferLimit.DiffWithSmallerNumber(buffer);





            while (length > bufferLeft)
            {
                //ptr.CopyTo(buffer, bufferLeft);
                //SynchronizedFlush();
                FlushHandler(ptr, buffer);
                length -= bufferLeft;
                ptr += bufferLeft;
            }
        }

        if (length > 0)
        {
            ptr.CopyTo(next, length);
            next += length;
        }
        */
    }
}