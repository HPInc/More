// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace More
{
    public delegate void ThreadDone(Exception e);
    public class ReaderToWriterThread
    {
        public const Int32 DefaultBufferSize = 2048;

        private readonly ThreadDone callback;
        private readonly TextReader reader;
        private readonly TextWriter writer;
        private readonly Int32 bufferSize;

        private Boolean keepLooping;

        public ReaderToWriterThread(ThreadDone callback, TextReader reader, TextWriter writer)
            : this(callback, reader, writer, DefaultBufferSize)
        {
        }
        public ReaderToWriterThread(ThreadDone callback, TextReader reader, TextWriter writer, Int32 bufferSize)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if (writer == null) throw new ArgumentNullException("writer");

            this.callback = callback;
            this.reader = reader;
            this.writer = writer;
            this.bufferSize = bufferSize;
        }
        public void StopLooping()
        {
            keepLooping = false;
        }
        public void RunPrepare()
        {
            keepLooping = true;
        }
        public void Run()
        {
            Exception potentialException = null;

            try
            {
                Char[] buffer = new Char[bufferSize];

                while (keepLooping)
                {
                    Int32 bytesRead = reader.Read(buffer, 0, buffer.Length);
                    if (bytesRead <= 0) break;
                    writer.Write(buffer, 0, bytesRead);
                }
            }
            catch (Exception e)
            {
                potentialException = e;
            }
            finally
            {
                if (callback != null) callback(potentialException);
            }
        }
    }
}
