// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Diagnostics;

namespace More
{
    public enum Utf8ExceptionType
    {
        StartedInsideCodePoint,
        MissingBytes,
        OutOfRange,
    }
    public class Utf8Exception : Exception
    {
        const String GenericMessage = "invalid utf8";
        const String StartedInsideCodePointMessage = "utf8 string started inside a utf8 code point";
        const String MissingBytesMessage = "utf8 encoding is missing some bytes";
        const String OutOfRangeMessage = "the utf8 code point is out of range";
        static String GetMessage(Utf8ExceptionType type)
        {
            switch(type)
            {
                case Utf8ExceptionType.StartedInsideCodePoint: return StartedInsideCodePointMessage;
                case Utf8ExceptionType.MissingBytes: return MissingBytesMessage;
                case Utf8ExceptionType.OutOfRange: return OutOfRangeMessage;
            }
            return GenericMessage;
        }
        public readonly Utf8ExceptionType type;
        public Utf8Exception(Utf8ExceptionType type) : base(GetMessage(type))
        {
            this.type = type;
        }
    }
    public static class Utf8
    {
        public static UInt32 Decode(Byte[] array, ref UInt32 offset, UInt32 limit)
        {
            if(offset >= limit) throw new ArgumentException("Cannot pass an empty data segment to Utf8.Decode");

            UInt32 c = array[offset];
            offset++;
            if(c <= 0x7F) {
                return c;
            }
            if((c & 0x40) == 0) {
                throw new Utf8Exception(Utf8ExceptionType.StartedInsideCodePoint);
            }

            if((c & 0x20) == 0) {
                if (offset >= limit) throw new Utf8Exception(Utf8ExceptionType.MissingBytes);
                return ((c << 6) & 0x7C0U) | (array[offset++] & 0x3FU);
            }

            if((c & 0x10) == 0) {
                offset++;
                if (offset >= limit) throw new Utf8Exception(Utf8ExceptionType.MissingBytes);
                return ((c << 12) & 0xF000U) | ((UInt32)(array[offset-1] << 6) & 0xFC0U) | (array[offset++] & 0x3FU);
            }

            if((c & 0x08) == 0) {
                offset += 2;
                if (offset >= limit) throw new Utf8Exception(Utf8ExceptionType.MissingBytes);
                return ((c << 18) & 0x1C0000U) | ((UInt32)(array[offset - 2] << 12) & 0x3F000U) |
                    ((UInt32)(array[offset - 1] << 6) & 0xFC0U) | (array[offset++] & 0x3FU);
            }

            throw new Utf8Exception(Utf8ExceptionType.OutOfRange);
        }
        public static UInt32 Decode(ref Utf8Pointer text, Utf8Pointer limit)
        {
            if (text >= limit) throw new ArgumentException("Cannot pass an empty data segment to Utf8.Decode");

            UInt32 c = text[0];
            if (c <= 0x7F)
            {
                return c;
            }
            if ((c & 0x40) == 0)
            {
                throw new Utf8Exception(Utf8ExceptionType.StartedInsideCodePoint);
            }

            text++;
            if ((c & 0x20) == 0)
            {
                if (text >= limit) throw new Utf8Exception(Utf8ExceptionType.MissingBytes);
                return ((c << 6) & 0x7C0U) | (text[0] & 0x3FU);
            }

            if ((c & 0x10) == 0)
            {
                if (text+1 >= limit) throw new Utf8Exception(Utf8ExceptionType.MissingBytes);
                return ((c << 12) & 0xF000U) | ((UInt32)(text[0] << 6) & 0xFC0U) | (text[1] & 0x3FU);
            }

            if ((c & 0x08) == 0)
            {
                if (text+2 >= limit) throw new Utf8Exception(Utf8ExceptionType.MissingBytes);
                return ((c << 18) & 0x1C0000U) | ((UInt32)(text[0] << 12) & 0x3F000U) |
                    ((UInt32)(text[1] << 6) & 0xFC0U) | (text[2] & 0x3FU);
            }

            throw new Utf8Exception(Utf8ExceptionType.OutOfRange);
        }

        /// <summary>The maximum number of bytes it would take to encode a C# Char type</summary>
        public const UInt32 MaxCharEncodeLength = 3;
        public static Byte GetCharEncodeLength(Char c)
        {
            if (c <= 0x007F) return 1;
            if (c <= 0x07FF) return 2;
            return 3;
        }
        public static UInt32 GetEncodeLength(String str)
        {
            UInt32 byteCount = 0;
            for (int i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (c <= 0x007F)
                {
                    byteCount++;
                }
                else if (c <= 0x07FF)
                {
                    byteCount += 2;
                }
                else
                {
                    byteCount += 3;
                }
            }
            return byteCount;
        }
        // Returns the offset after encoding the character
        public static UInt32 EncodeChar(Char c, Byte[] buffer, UInt32 offset)
        {
            if (c <= 0x007F)
            {
                buffer[offset    ] = (Byte)c;
                return offset + 1;
            }
            if (c <= 0x07FF)
            {
                buffer[offset    ] = (Byte)(0xC0 | ((c >> 6)        )); // 110xxxxx
                buffer[offset + 1] = (Byte)(0x80 | ( c        & 0x3F)); // 10xxxxxx
                return offset + 2;
            }
            buffer[offset    ]     = (Byte)(0xE0 | ((c >> 12)       )); // 1110xxxx
            buffer[offset + 1]     = (Byte)(0x80 | ((c >>  6) & 0x3F)); // 10xxxxxx
            buffer[offset + 2]     = (Byte)(0x80 | ( c        & 0x3F)); // 10xxxxxx
            return offset + 3;
        }
        // Returns the offset after encoding the character
        // Note: it is assumed that the caller will have already calculated the encoded length, for
        //       that reason, this method does not return the offset after the encoding
        public static void Encode(String str, Byte[] buffer, UInt32 offset)
        {
            for (int i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (c <= 0x007F)
                {
                    buffer[offset++] = (Byte)c;
                }
                else if (c <= 0x07FF)
                {
                    buffer[offset++] = (Byte)(0xC0 | ((c >> 6)       )); // 110xxxxx
                    buffer[offset++] = (Byte)(0x80 | ( c       & 0x3F)); // 10xxxxxx
                }
                else
                {
                    buffer[offset++] = (Byte)(0xE0 | ((c >> 12)      )); // 1110xxxx
                    buffer[offset++] = (Byte)(0x80 | ((c >> 6) & 0x3F)); // 10xxxxxx
                    buffer[offset++] = (Byte)(0x80 | ( c       & 0x3F)); // 10xxxxxx
                }
            }
        }
        // TODO: implement this correctly later
        public static Boolean IsUpper(UInt32 c)
        {
            return
                (c >= 'A' && c <= 'Z');
        }
        // TODO: implement this correctly later
        public static Boolean IsLower(UInt32 c)
        {
            return
                (c >= 'a' && c <= 'z');
        }
        // TODO: implement this correctly later
        public static UInt32 ToUpper(UInt32 c)
        {
            if (c >= 'a' && c <= 'z') return c - 'a' + 'A';
            return c;
        }
        // TODO: implement this correctly later
        public static UInt32 ToLower(UInt32 c)
        {
            if (c >= 'A' && c <= 'Z') return c - 'A' + 'a';
            return c;
        }
        public static Boolean IsNormalWhiteSpace(UInt32 c)
        {
            return
                (c <= 0x000D && c >= 0x0009) || // TAB (U+0009)
                                                // LINE FEED (U+000A)
                                                // LINE TAB (U+000B)
                                                // FORM FEED (U+000C)
                                                // CARRIAGE RETURN (U+000D)
                (c == 0x0020) ;                 // SPACE (U+0020)
                /*
                (c == 0x0085) ||                // NEXT LINE (U+0085)
                (c == 0x00A0) ||                // NO-BREAK SPACE (U+00A0)
                (c == 0x1680) ||                // OGHAM SPACE MARK (U+1680)
                (c >= 0x2000 && c <= 0x200A) || // EN QUAD (U+2000)
                                                // EM QUAD (U+2001)
                                                // EN SPACE (U+2002)
                                                // EM SPACE (U+2003)
                                                // THREE-PER-EM SPACE (U+2004)
                                                // FOUR-PER-EM SPACE (U+2005)
                                                // SIX-PER-EM SPACE (U+2006)
                                                // FIGURE SPACE (U+2007)
                                                // PUNCTUATION SPACE (U+2008)
                                                // THIN SPACE (U+2009)
                                                // HAIR SPACE (U+200A)
                */
            /* 
             * 
             * OGHAM SPACE MARK (U+1680)

             * NARROW NO-BREAK SPACE (U+202F)
             * MEDIUM MATHEMATICAL SPACE (U+205F)
             * IDEOGRAPHIC SPACE (U+3000)*/
        }

        /// <summary>Peel the first string surrounded by whitespace.</summary>
        /// <returns>The first string surrounded by whitespace BY LIMIT</returns>
        public static OffsetLimit Peel(ref SliceByLimit<Byte> segment)
        {
            Debug.Assert(segment.InValidState());
            var array = segment.array;
            var limit = segment.limit;

            if (segment.offset >= limit)
                return new OffsetLimit(segment.offset, limit);

            UInt32 c;
            UInt32 save;

            //
            // Skip beginning whitespace
            //
            while (true)
            {
                if (segment.offset >= limit)
                    return new OffsetLimit(segment.offset, limit);

                save = segment.offset;
                c = Decode(array, ref segment.offset, limit);
                if (!IsNormalWhiteSpace(c)) break;
            }

            UInt32 peelStart = save;

            //
            // Find next whitespace
            //
            while (true)
            {
                if (segment.offset >= limit)
                    return new OffsetLimit(peelStart, segment.offset);

                save = segment.offset;
                c = Decode(array, ref segment.offset, limit);
                if (IsNormalWhiteSpace(c)) break;
            }

            return new OffsetLimit(peelStart, save);

            /*
            UInt32 peelLimit = save;

            //
            // Remove whitespace till rest
            //
            while (true)
            {
                if (segment.offset >= limit)

                save = segment.offset;
                c = Decode(array, ref segment.offset, limit);
                if (!IsNormalWhiteSpace(c))
                {
                    segment.offset = save;
                    return new Segment(array, peelStart, peelLimit);
                }
            }
            */
        }
        public static Boolean EqualsString(Byte[] array, UInt32 offset, UInt32 limit, String compare, Boolean ignoreCase)
        {
            return EqualsString(array, offset, limit, compare, 0, (UInt32)compare.Length, ignoreCase);
        }
        public static Boolean EqualsString(Byte[] array, UInt32 offset, UInt32 limit,
            String compare, UInt32 compareOffset, UInt32 compareLimit, Boolean ignoreCase)
        {
            while(true)
            {
                if (offset >= limit)
                {
                    return compareOffset >= compareLimit;
                }
                if (compareOffset >= compareLimit)
                {
                    return false;
                }

                UInt32 c = Decode(array, ref offset, limit);
                if (c != (UInt32)compare[(int)compareOffset])
                {
                    if (!ignoreCase) return false;
                    if (Char.IsUpper(compare[(int)compareOffset]))
                    {
                        if (IsUpper(c)) return false;
                        if (ToUpper(c) != compare[(int)compareOffset]) return false;
                    }
                    else
                    {
                        if (IsLower(c)) return false;
                        if (ToLower(c) != (UInt32)compare[(int)compareOffset]) return false;
                    }

                }
                compareOffset++;
            }
        }
        
        public static String Decode(Utf8PointerLengthSlice text)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder((int)text.length);
            Utf8Pointer next = text.ptr;
            Utf8Pointer limit = text.LimitPointer;
            while (next < limit)
            {
                UInt32 c = Decode(ref next, limit);
                builder.Append((Char)c);
            }
            return builder.ToString();
        }
        public static String Decode(Utf8LengthSlice text)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder((int)text.length);
            UInt32 offset = text.offset;
            UInt32 limit = text.offset + text.length;
            while (true)
            {
                if (offset >= text.length)
                {
                    return builder.ToString();
                }

                UInt32 c = Decode(text.array, ref offset, limit);
                builder.Append((Char)c);
            }
        }
    }
}