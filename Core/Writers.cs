// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.IO;

namespace More
{
    public interface IWriter
    {
        void Flush();

        unsafe void Write(Byte* buffer, UInt32 length);
        void Write(Byte[] byteString);
        void Write(Byte[] byteString, UInt32 offset, UInt32 length);

        void Write(String str);
        void Write(String str, UInt32 offset, UInt32 length);

        void WriteLine();

        unsafe void WriteLine(Byte* buffer, UInt32 length);
        void WriteLine(Byte[] byteString);
        void WriteLine(Byte[] byteString, UInt32 offset, UInt32 length);

        void WriteLine(String str);
        void WriteLine(String str, UInt32 offset, UInt32 length);

        void Write(Stream stream);
    }
    public interface IFormatWriter : IWriter
    {
        void Write(String format, Object a);
        void Write(String format, Object a, Object b);
        void Write(String format, Object a, Object b, Object c);
        void Write(String format, params Object[] obj);

        void WriteLine(String format, Object a);
        void WriteLine(String format, Object a, Object b);
        void WriteLine(String format, Object a, Object b, Object c);
        void WriteLine(String format, params Object[] obj);

    }
}