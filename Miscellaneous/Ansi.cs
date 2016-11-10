// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace More
{
    public delegate void Escape();
    public class AnsiEscapeDecoder
    {
        public readonly DataHandler escapeHandler;
        public readonly DataHandler dataHandler;
        public AnsiEscapeDecoder(DataHandler escapeHandler, DataHandler dataHandler)
        {
            this.escapeHandler = escapeHandler;
            this.dataHandler = dataHandler;
        }
        // Returns the number of bytes processed
        public UInt32 Decode(Byte[] data, UInt32 offset, UInt32 bytesRead)
        {
            UInt32 byteCount;
            UInt32 start = offset;

            while(offset < bytesRead)
            {
                Byte b = data[offset];
                offset++;

                if (b == 0x1B)
                {
                    byteCount = offset - start;
                    if (byteCount >= 2)
                    {
                        dataHandler(data, start, byteCount - 1);
                    }
                    if (offset >= bytesRead)
                    {
                        return offset - 1; // Tell the caller to save the escape character
                    }

                    b = data[offset];
                    if (b == '[')
                    {
                        UInt32 saveOffset = offset;
                        while (true)
                        {
                            offset++;
                            if (offset >= bytesRead)
                            {
                                return saveOffset - 1; // Tell the caller to save the beginning part of this escape sequence
                            }
                            b = data[offset];
                            if (b >= '@' && b <= '~')
                            {
                                escapeHandler(data, saveOffset, offset + 1 - saveOffset);
                                offset++;
                                start = offset;
                                break;
                            }
                        }
                    }
                    else if(b >= '@' && b <= '_')
                    {
                        escapeHandler(data, offset, 1);
                        offset++;
                        start = offset;
                    }
                    else
                    {
                        throw new FormatException(String.Format("The AnsiEscapeDecoder does not recognize '{0}' (code={1}) as the second character in an escape sequence",
                            b, (UInt32)b));
                    }
                }
            }

            byteCount = bytesRead - start;
            if (byteCount > 0)
            {
                dataHandler(data, start, byteCount);
            }
            return bytesRead;
        }
    }
}
