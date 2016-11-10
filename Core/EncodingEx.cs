// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;

namespace More
{
    /// <summary>
    /// Contains extensions to the systems encoding functionality.
    /// </summary>
    public static class EncodingEx
    {
        /// <summary>
        /// Encodes the given character array into the given destination.
        /// </summary>
        /// <param name="outEncodedTo">Places the number of characters encoded in this variable.</param>
        /// <returns>Pointer to the address after the last encoded character.</returns>
        public static unsafe byte* EncodeMaxUtf8(out char* outEncodedTo,
            char* str, char* strLimit, byte* dest, byte* destLimit)
        {
            while(str < strLimit)
            {
                char c = *str;
                if(c <= 0x7F)
                {
                    if(dest >= destLimit)
                    {
                        break;
                    }
                    *dest = (byte)c;
                    dest++;
                }
                else if(c <= 0x7FF)
                {
                    if (dest + 1 >= destLimit)
                    {
                        break;
                    }
                    *(dest    ) = (byte)(0xC0 | (c >>   6));
                    *(dest + 1) = (byte)(0x80 | (c & 0x3F));
                    dest += 2;
                }
                else if(c <= 0xFFFF)
                {
                    if (dest + 2 >= destLimit)
                    {
                        break;
                    }
                    *(dest    ) = (byte)(0xE0 | (c >> 12));
                    *(dest + 1) = (byte)(0x80 | ((c >> 6) & 0x3F));
                    *(dest + 2) = (byte)(0x80 | (c & 0x3F));
                    dest += 3;
                }
                else
                {
                    // TODO: implement larger characters
                    throw new NotImplementedException("large utf8 chars");
                }
                str++;
            }

            outEncodedTo = str;
            return dest;
        }
        public static unsafe byte* EncodeMaxUtf8(out uint outCharsEncoded, String str,
            UInt32 offset, UInt32 charLength, Byte* dest, Byte* destLimit)
        {
            // NOTE: can't do the addition (str + offset) in the fixed statement, because
            //       it doesn't do pointer arithmetic correctly
            fixed(char* originalStringPtr = str)
            {
                char* strPtr = originalStringPtr + offset;
                char* encodedTo;
                byte* writtenTo = EncodeMaxUtf8(out encodedTo, strPtr, strPtr + charLength, dest, destLimit);
                outCharsEncoded = (uint)(encodedTo - strPtr);
                return writtenTo;
            }
        }
    }
}