// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More
{
    public class OffsetLineParser
    {
        readonly ByteArrayReference buffer;
        Int32 count;
        Int32 nextLineStart;

        public OffsetLineParser(ByteArrayReference buffer)
        {
            this.buffer = buffer;
            this.count = 0;
            this.nextLineStart = 0;
        }
        public void Add(Byte[] bytes, Int32 length)
        {
            buffer.EnsureCapacityCopyAllData((UInt32)(count + length));
            Array.Copy(bytes, 0, buffer.array, count, length);
            count += length;
        }

        // returns null when no more lines are available
        public Byte[] GetLine(ref Int32 lineOffset, ref Int32 lineLength)
        {
            if (count <= 0) return null;

            Int32 offset = nextLineStart;

            while (true)
            {
                if (offset >= count)
                {
                    // Move extra bytes to the beginning of the line
                    if (nextLineStart > 0 && offset > nextLineStart)
                    {
                        Array.Copy(buffer.array, nextLineStart, buffer.array, 0, nextLineStart - offset);
                        nextLineStart = 0;
                    }
                    return null;
                }

                if (buffer.array[offset] == '\n')
                {
                    lineOffset = nextLineStart;
                    lineLength = offset - nextLineStart;

                    if(lineLength <= 0)
                    {
                        nextLineStart++;
                        return buffer.array;
                    }

                    nextLineStart += lineLength + 1;
                    
                    // Get rid of '\r'
                    if (buffer.array[lineOffset + lineLength - 1] == '\r')
                    {
                        lineLength--;
                    }

                    return buffer.array;
                }

                offset++;
            }
        }
    }
}
