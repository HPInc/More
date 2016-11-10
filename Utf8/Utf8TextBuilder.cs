// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;

namespace More
{
    public interface IUtf8TextBuilder : ITextBuilder
    {
        void AppendUtf8(Char c);
        void AppendUtf8(String str);
        void Append(Encoder encoder, String str);
    }
    public class ExtendedByteBuilder : ByteBuilder, IUtf8TextBuilder
    {
        public ExtendedByteBuilder()
            : base()
        {
        }
        public ExtendedByteBuilder(UInt32 initialLength)
            : base(initialLength)
        {
        }
        public ExtendedByteBuilder(Byte[] bytes)
            : base(bytes)
        {
        }
        public void AppendUtf8(Char c)
        {
            EnsureTotalCapacity(contentLength + Utf8.MaxCharEncodeLength);
            contentLength = Utf8.EncodeChar(c, bytes, contentLength);
        }
        public void AppendUtf8(String str)
        {
            UInt32 encodeLength = Utf8.GetEncodeLength(str);
            EnsureTotalCapacity(contentLength + encodeLength);
            Utf8.Encode(str, bytes, contentLength);
            contentLength += encodeLength;
        }

        public void Append(Encoder encoder, String str)
        {
            var encodeLength = encoder.GetEncodeLength(str);
            EnsureTotalCapacity(contentLength + encodeLength);
            encoder.Encode(str, bytes, contentLength);
            contentLength += encodeLength;
        }
    }
}