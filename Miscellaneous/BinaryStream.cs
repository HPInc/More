// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.IO;

namespace More
{
    public class BinaryStream
    {
        readonly byte[] eightByteBuffer = new byte[8];
        Stream stream;

        public BinaryStream(Stream stream)
        {
            this.stream = stream;
        }
        public void Skip(Int32 length)
        {
            stream.Position += length;
        }
        public Byte[] ReadFullSize(Int32 length)
        {
            Byte[] buffer = new Byte[length];
            stream.ReadFullSize(buffer, 0, length);
            return buffer;
        }
        public void ReadFullSize(Byte[] buffer, Int32 offset, Int32 length)
        {
            stream.ReadFullSize(buffer, offset, length);
        }
        public UInt16 LittleEndianReadUInt16()
        {
            ReadFullSize(eightByteBuffer, 0, 2);
            return eightByteBuffer.LittleEndianReadUInt16(0);
        }
        public UInt32 LittleEndianReadUInt32()
        {
            ReadFullSize(eightByteBuffer, 0, 4);
            return eightByteBuffer.LittleEndianReadUInt32(0);
        }
    }
}
