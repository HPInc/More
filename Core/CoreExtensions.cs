// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Diagnostics;

#if WindowsCE
using ArrayCopier = System.MissingInCEArrayCopier;
#else
using ArrayCopier = System.Array;
#endif

namespace More
{
    /// <summary>
    /// Used to convert stopwatch ticks to and from other time units.
    /// </summary>
    public static class StopwatchExtensions
    {
        static readonly Double MillisecondsPerStopwatchTicksAsDouble = 1000.0 / Stopwatch.Frequency;
        static readonly Double MicrosecondsPerStopwatchTicksAsDouble = 1000000.0 / Stopwatch.Frequency;

        public static Int64 MillisToStopwatchTicks(this Int32 millis)
        {
            return Stopwatch.Frequency * (Int64)millis / 1000L;
        }
        public static Int64 MillisToStopwatchTicks(this UInt32 millis)
        {
            return Stopwatch.Frequency * (Int64)millis / 1000L;
        }
        public static Int64 StopwatchTicksAsMicroseconds(this Int64 stopwatchTicks)
        {
            return stopwatchTicks * 1000000L / Stopwatch.Frequency;
        }
        public static Double StopwatchTicksAsDoubleMicroseconds(this Int64 stopwatchTicks)
        {
            return (Double)(stopwatchTicks * 1000000L) / (Double)Stopwatch.Frequency;
        }
        public static Int32 StopwatchTicksAsInt32Milliseconds(this Int64 stopwatchTicks)
        {
            return (Int32)(stopwatchTicks * 1000 / Stopwatch.Frequency);
        }
        public static UInt32 StopwatchTicksAsUInt32Milliseconds(this Int64 stopwatchTicks)
        {
            return (UInt32)(stopwatchTicks * 1000 / Stopwatch.Frequency);
        }
   
        public static Int64 StopwatchTicksAsInt64Milliseconds(this Int64 stopwatchTicks)
        {
            return (Int64)(stopwatchTicks * 1000 / Stopwatch.Frequency);
        }
        public static Double StopwatchTicksAsDoubleMilliseconds(this Int64 stopwatchTicks)
        {
            return MillisecondsPerStopwatchTicksAsDouble * stopwatchTicks;
        }

        const Double OneThousand  = 1000;
        const Double OneMillion   = 1000000;
        const Double OneBillion   = 1000000000;

        public static String StopwatchTicksAsPrettyTime(this Int64 stopwatchTicks, Byte maxDecimalDigits)
        {
            Double microseconds = (Double)stopwatchTicks * MicrosecondsPerStopwatchTicksAsDouble;
            if (microseconds < 999.5D)
            {
                return microseconds.ToString("F" + maxDecimalDigits) + " microseconds";
            }
            if (microseconds < 999999.5D)
            {
                return (microseconds / OneThousand).ToString("F" + maxDecimalDigits) + " milliseconds";
            }
            return (microseconds / OneMillion).ToString("N" + maxDecimalDigits) + " seconds";
        }
    }
    public delegate T Parser<T>(String str);
    public static class StringExtensions
    {
        /// <summary>
        /// Peel the next non-whitespace substring from the front of the given string.
        /// </summary>
        /// <param name="str">The string to peel from</param>
        /// <param name="rest">The rest of the string after the peel</param>
        /// <returns>The peeled string</returns>
        public static String Peel(this String str, out String rest)
        {
            return Peel(str, 0, out rest);
        }

        /// <summary>
        /// Peel the next non-whitespace substring from the given offset of the given string.
        /// </summary>
        /// <param name="str">The string to peel from</param>
        /// <param name="offset">The offset into the string to start peeling from.</param>
        /// <param name="rest">The rest of the string after the peel</param>
        /// <returns>The peeled string</returns>
        public static String Peel(this String str, Int32 offset, out String rest)
        {
            if (str == null)
            {
                rest = null;
                return null;
            }

            Char c;

            //
            // Skip beginning whitespace
            //
            while (true)
            {
                if (offset >= str.Length)
                {
                    rest = null;
                    return null;
                }
                c = str[offset];
                if (!Char.IsWhiteSpace(c)) break;
                offset++;
            }

            Int32 startOffset = offset;

            //
            // Find next whitespace
            //
            while (true)
            {
                offset++;
                if (offset >= str.Length)
                {
                    rest = null;
                    return str.Substring(startOffset);
                }
                c = str[offset];
                if (Char.IsWhiteSpace(c)) break;
            }

            Int32 peelLimit = offset;

            //
            // Remove whitespace till rest
            //
            while (true)
            {
                offset++;
                if (offset >= str.Length)
                {
                    rest = null;
                }
                if (!Char.IsWhiteSpace(str[offset]))
                {
                    rest = str.Substring(offset);
                    break;
                }
            }
            return str.Substring(startOffset, peelLimit - startOffset);
        }
        /// <summary>
        /// Provides a substring comparison without having to create the substring.
        /// Functionally equivalent to str.Substring(offset, compare.Length).Equals(compare)
        /// </summary>
        public static Boolean SubstringEquals(this String str, Int32 offset, String compare)
        {
            return SubstringEquals(str, offset, compare, false);
        }
        public static Boolean SubstringEquals(this String str, Int32 offset, String compare, Boolean ignoreCase)
        {
            if (offset + compare.Length > str.Length)
            {
                return false;
            }
            for (int i = 0; i < compare.Length; i++)
            {
                if (str[offset] != compare[i])
                {
                    if (!ignoreCase)
                    {
                        return false;
                    }
                    if (Char.IsUpper(str[offset]))
                    {
                        if (Char.IsUpper(compare[i])) return false;
                        if (Char.ToUpper(compare[i]) != str[offset]) return false;
                    }
                    else
                    {
                        if (Char.IsLower(compare[i])) return false;
                        if (Char.ToLower(compare[i]) != str[offset]) return false;
                    }
                }
                offset++;
            }
            return true;
        }
    }
    public static class ByteExtensions
    {
        public static Byte HexDigitToValue(this Byte b)
        {
            if (b >= '0')
            {
                if (b <= '9') return (Byte)(b - '0');
                if (b >= 'A')
                {
                    if (b <= 'F') return (Byte)((b - 'A') + 10);
                    if (b >= 'a' && b <= 'f') return (Byte)((b - 'a') + 10);
                }
            }
            throw new FormatException(String.Format("Expected 0-9, A-F, or a-f but got '{0}' (charcode={1})", (Char)b, (UInt32)b));
        }
    }
    public static class CharExtensions
    {
        public static Int32 HexDigitToValue(this Char c)
        {
            if (c >= '0')
            {
                if (c <= '9') return (Byte)(c - '0');
                if (c >= 'A')
                {
                    if (c <= 'F') return (Byte)((c - 'A') + 10);
                    if (c >= 'a' && c <= 'f') return (Byte)((c - 'a') + 10);
                }
            }
            throw new FormatException(String.Format("Expected 0-9, A-F, or a-f but got '{0}' (charcode={1})", c, (UInt32)c));
        }
        static Char ToLowerAsciiOnly(this Char c)
        {
            return (c >= 'A' && c <= 'Z') ? (Char)(c - 'A' + 'a') : c;
        }
        static Char ToUpperAsciiOnly(this Char c)
        {
            return (c >= 'a' && c <= 'z') ? (Char)(c - 'a' + 'A') : c;
        }
    }
    public static class ByteArray
    {
        public static void BigEndianSetUInt32Subtype(this byte[] array, UInt32 offset, UInt32 value, Byte byteCount)
        {
            switch (byteCount)
            {
                case 1:
                    array[offset] = (Byte)value;
                    return;
                case 2:
                    array[offset    ] = (Byte)(value >> 8);
                    array[offset + 1] = (Byte)value;
                    return;
                case 3:
                    array[offset    ] = (Byte)(value >> 16);
                    array[offset + 1] = (Byte)(value >> 8);
                    array[offset + 2] = (Byte)value;
                    return;
                case 4:
                    array[offset    ] = (Byte)(value >> 24);
                    array[offset + 1] = (Byte)(value >> 16);
                    array[offset + 2] = (Byte)(value >> 8);
                    array[offset + 3] = (Byte)value;
                    return;
            }
            throw new InvalidOperationException(String.Format("Expected byteCount to be 1,2,3 or 4 but was {0}", byteCount));
        }
        public static UInt32 BigEndianReadUInt32Subtype(this byte[] bytes, UInt32 offset, Byte byteCount)
        {
            switch (byteCount)
            {
                case 1:
                    return (UInt32)(
                        (0x000000FFU & (bytes[offset])));
                case 2:
                    return (UInt32)(
                        (0x0000FF00U & (bytes[offset    ] << 8)) |
                        (0x000000FFU & (bytes[offset + 1])));
                case 3:
                    return (UInt32)(
                        (0x00FF0000U & (bytes[offset    ] << 16)) |
                        (0x0000FF00U & (bytes[offset + 1] <<  8)) |
                        (0x000000FFU & (bytes[offset + 2]      )) );
                case 4:
                    return (UInt32) (
                        (bytes[offset    ] << 24) |
                        (bytes[offset + 1] << 16) |
                        (bytes[offset + 2] <<  8) |
                        (bytes[offset + 3]      ) );
            }
            throw new InvalidOperationException(String.Format("Expected byteCount to be 1,2,3 or 4 but was {0}", byteCount));
        }

        //
        // Set 1-byte types
        //
        public static void SetByteEnum<EnumType>(this Byte[] bytes, UInt32 offset, EnumType value)
        {
            bytes[offset] = Convert.ToByte(value);
        }
        //
        // Read 1-byte types
        //
        public static EnumType ReadByteEnum<EnumType>(this Byte[] bytes, UInt32 offset)
        {
            return (EnumType)Enum.ToObject(typeof(EnumType), bytes[offset]);
        }
        //
        // Set 2-byte types
        //
        public static void BigEndianSetUInt16(this Byte[] bytes, UInt32 offset, UInt16 value)
        {
            bytes[offset    ] = (Byte)(value >> 8);
            bytes[offset + 1] = (Byte)value;
        }
        public static void BigEndianSetInt16(this Byte[] bytes, UInt32 offset, Int16 value)
        {
            bytes[offset    ] = (Byte)(value >> 8);
            bytes[offset + 1] = (Byte)value;
        }
        public static void BigEndianSetUInt16Enum<EnumType>(this Byte[] bytes, UInt32 offset, EnumType value)
        {
            UInt16 valueAsUInt16 = Convert.ToUInt16(value);
            bytes[offset    ] = (Byte)(valueAsUInt16 >> 8);
            bytes[offset + 1] = (Byte)valueAsUInt16;
        }
        //
        // Read 2-byte types
        //
        public static UInt16 BigEndianReadUInt16(this Byte[] bytes, UInt32 offset)
        {
            return (UInt16)(bytes[offset] << 8 | bytes[offset + 1]);
        }
        public static Int16 BigEndianReadInt16(this Byte[] bytes, UInt32 offset)
        {
            return (Int16)(bytes[offset] << 8 | bytes[offset + 1]);
        }
        public static EnumType BigEndianReadUInt16Enum<EnumType>(this Byte[] bytes, UInt32 offset)
        {
            return (EnumType)Enum.ToObject(typeof(EnumType), (UInt16)(bytes[offset] << 8 | bytes[offset + 1]));
        }
        //
        // Set 3-byte types
        //
        public static void BigEndianSetUInt24(this Byte[] bytes, UInt32 offset, UInt32 value)
        {
            bytes[offset    ] = (Byte)(value >> 16);
            bytes[offset + 1] = (Byte)(value >> 8);
            bytes[offset + 2] = (Byte)value;
        }
        public static void BigEndianSetUInt24Enum<EnumType>(this Byte[] bytes, UInt32 offset, EnumType value)
        {
            UInt32 valueAsUInt32 = Convert.ToUInt32(value);
            bytes[offset    ] = (Byte)(valueAsUInt32 >> 16);
            bytes[offset + 1] = (Byte)(valueAsUInt32 >> 8);
            bytes[offset + 2] = (Byte)valueAsUInt32;
        }
        public static void BigEndianSetInt24(this Byte[] bytes, UInt32 offset, Int32 value)
        {
            bytes[offset    ] = (Byte)(value >> 16);
            bytes[offset + 1] = (Byte)(value >> 8);
            bytes[offset + 2] = (Byte)value;
        }
        //
        // Read 3-byte types
        //
        public static UInt32 BigEndianReadUInt24(this Byte[] bytes, UInt32 offset)
        {
            return (UInt32)(
                bytes[offset    ] << 16 |
                bytes[offset + 1] <<  8 |
                bytes[offset + 2]       );
        }
        public static EnumType BigEndianReadUInt24Enum<EnumType>(this Byte[] bytes, UInt32 offset)
        {
            return (EnumType)Enum.ToObject(typeof(EnumType), (UInt32)(
                bytes[offset    ] << 16 |
                bytes[offset + 1] <<  8 |
                bytes[offset + 2]       ));
        }
        public static Int32 BigEndianReadInt24(this Byte[] bytes, UInt32 offset)
        {
            return (Int32)(
                ( ((bytes[offset] & 0x80) == 0x80) ? unchecked((Int32)0xFF000000) : 0) | // Sign Extend
                bytes[offset    ] << 16 |
                bytes[offset + 1] <<  8 |
                bytes[offset + 2]       );
        }
        //
        // Set 4-byte types
        //
        public static void BigEndianSetUInt32(this Byte[] bytes, UInt32 offset, UInt32 value)
        {
            bytes[offset    ] = (Byte)(value >> 24);
            bytes[offset + 1] = (Byte)(value >> 16);
            bytes[offset + 2] = (Byte)(value >> 8);
            bytes[offset + 3] = (Byte)value;
        }
        public static void BigEndianSetUInt32Enum<EnumType>(this Byte[] bytes, UInt32 offset, EnumType value)
        {
            UInt32 valueAsUInt32 = Convert.ToUInt32(value);
            bytes[offset    ] = (Byte)(valueAsUInt32 >> 24);
            bytes[offset + 1] = (Byte)(valueAsUInt32 >> 16);
            bytes[offset + 2] = (Byte)(valueAsUInt32 >> 8);
            bytes[offset + 3] = (Byte)valueAsUInt32;
        }
        public static void BigEndianSetInt32(this Byte[] bytes, UInt32 offset, Int32 value)
        {
            bytes[offset    ] = (Byte)(value >> 24);
            bytes[offset + 1] = (Byte)(value >> 16);
            bytes[offset + 2] = (Byte)(value >> 8);
            bytes[offset + 3] = (Byte)value;
        }
        //
        // Read 4-byte types
        //
        public static UInt32 BigEndianReadUInt32(this Byte[] bytes, UInt32 offset)
        {
            return (UInt32) (
                (bytes[offset    ] << 24) |
                (bytes[offset + 1] << 16) |
                (bytes[offset + 2] <<  8) |
                (bytes[offset + 3]      ) );
        }
        public static EnumType BigEndianReadUInt32Enum<EnumType>(this Byte[] bytes, UInt32 offset)
        {
            return (EnumType)Enum.ToObject(typeof(EnumType), (UInt32) (
                (bytes[offset    ] << 24) |
                (bytes[offset + 1] << 16) |
                (bytes[offset + 2] <<  8) |
                (bytes[offset + 3]      ) ));
        }
        public static Int32 BigEndianReadInt32(this Byte[] bytes, UInt32 offset)
        {
            return (Int32)(
                bytes[offset    ] << 24 |
                bytes[offset + 1] << 16 |
                bytes[offset + 2] <<  8 |
                bytes[offset + 3]       );
        }
        //
        // Set 8-byte types
        //
        public static void BigEndianSetUInt64(this Byte[] bytes, UInt32 offset, UInt64 value)
        {
            bytes[offset    ] = (Byte)(value >> 56);
            bytes[offset + 1] = (Byte)(value >> 48);
            bytes[offset + 2] = (Byte)(value >> 40);
            bytes[offset + 3] = (Byte)(value >> 32);
            bytes[offset + 4] = (Byte)(value >> 24);
            bytes[offset + 5] = (Byte)(value >> 16);
            bytes[offset + 6] = (Byte)(value >>  8);
            bytes[offset + 7] = (Byte)value;
        }
        /*
        public static void BigEndianSetUInt32Enum<EnumType>(this Byte[] bytes, Int32 offset, EnumType value)
        {
            UInt32 valueAsUInt32 = Convert.ToUInt32(value);
            bytes[offset    ] = (Byte)(valueAsUInt32 >> 24);
            bytes[offset + 1] = (Byte)(valueAsUInt32 >> 16);
            bytes[offset + 2] = (Byte)(valueAsUInt32 >> 8);
            bytes[offset + 3] = (Byte)valueAsUInt32;
        }
        public static void BigEndianSetInt32(this Byte[] bytes, Int32 offset, Int32 value)
        {
            bytes[offset    ] = (Byte)(value >> 24);
            bytes[offset + 1] = (Byte)(value >> 16);
            bytes[offset + 2] = (Byte)(value >> 8);
            bytes[offset + 3] = (Byte)value;
        }
        */
        //
        // Read 8-byte types
        //
        public static UInt64 BigEndianReadUInt64(this Byte[] bytes, UInt32 offset)
        {
            return
                (((UInt64)bytes[offset    ]) << 56) |
                (((UInt64)bytes[offset + 1]) << 48) |
                (((UInt64)bytes[offset + 2]) << 40) |
                (((UInt64)bytes[offset + 3]) << 32) |
                (((UInt64)bytes[offset + 4]) << 24) |
                (((UInt64)bytes[offset + 5]) << 16) |
                (((UInt64)bytes[offset + 6]) <<  8) |
                (((UInt64)bytes[offset + 7])      ) ;
        }
        public static void LittleEndianSetUInt16(this Byte[] array, UInt32 offset, UInt16 value)
        {
            array[offset + 1] = (Byte)(value >> 8);
            array[offset    ] = (Byte)(value     );
        }
        public static UInt16 LittleEndianReadUInt16(this Byte[] bytes, UInt32 offset)
        {
            return (UInt16)(
                (0xFF00 & (bytes[offset + 1] << 8)) |
                (0x00FF & (bytes[offset])));
        }
        public static void LittleEndianSetUInt32(this Byte[] array, UInt32 offset, UInt32 value)
        {
            array[offset + 3] = (Byte)(value >> 24);
            array[offset + 2] = (Byte)(value >> 16);
            array[offset + 1] = (Byte)(value >>  8);
            array[offset    ] = (Byte)(value      );
        }
        public static void LittleEndianSetUInt64(this Byte[] array, UInt32 offset, UInt64 value)
        {
            array[offset + 7] = (Byte)(value >> 56);
            array[offset + 6] = (Byte)(value >> 48);
            array[offset + 5] = (Byte)(value >> 40);
            array[offset + 4] = (Byte)(value >> 32);
            array[offset + 3] = (Byte)(value >> 24);
            array[offset + 2] = (Byte)(value >> 16);
            array[offset + 1] = (Byte)(value >>  8);
            array[offset    ] = (Byte)(value      );
        }
        public static UInt32 LittleEndianReadUInt32(this Byte[] bytes, UInt32 offset)
        {
            return (UInt32)(
                (0xFF000000U & (bytes[offset + 3] << 24)) |
                (0x00FF0000U & (bytes[offset + 2] << 16)) |
                (0x0000FF00U & (bytes[offset + 1] << 8)) |
                (0x000000FFU & (bytes[offset])));
        }
        public static String ToHexString(this Byte[] bytes, Int32 offset, Int32 length)
        {
            Char[] hexBuffer = new Char[length * 2];

            Int32 hexOffset = 0;
            Int32 offsetLimit = offset + length;

            while (offset < offsetLimit)
            {
                String hex = bytes[offset].ToString("X2");
                hexBuffer[hexOffset] = hex[0];
                hexBuffer[hexOffset + 1] = hex[1];
                offset++;
                hexOffset += 2;
            }
            return new String(hexBuffer);
        }
        public static String ToHexString(this Char[] chars, Int32 offset, Int32 length)
        {
            Char[] hexBuffer = new Char[length * 2];

            Int32 hexOffset = 0;
            Int32 offsetLimit = offset + length;

            while (offset < offsetLimit)
            {
                String hex = ((Byte)chars[offset]).ToString("X2");
                hexBuffer[hexOffset] = hex[0];
                hexBuffer[hexOffset + 1] = hex[1];
                offset++;
                hexOffset += 2;
            }
            return new String(hexBuffer);
        }
        public static void ParseHex(this Byte[] bytes, Int32 offset, String hexString, Int32 hexStringOffset, Int32 hexStringLength)
        {
            if (hexStringLength % 2 == 1) throw new InvalidOperationException(String.Format(
                 "The input hex string length must be even but you provided an odd length {0}", hexStringLength));
            Int32 hexStringLimit = hexStringOffset + hexStringLength;
            while (hexStringOffset < hexStringLimit)
            {
                bytes[offset++] = (Byte)(
                    (hexString[hexStringOffset    ].HexDigitToValue() << 4) |
                     hexString[hexStringOffset + 1].HexDigitToValue()       );
                hexStringOffset += 2;
            }
        }
        public static Byte[] CreateSubArray(this Byte[] bytes, UInt32 offset, UInt32 length)
        {
            Byte[] subArray = new Byte[length];
            ArrayCopier.Copy(bytes, offset, subArray, 0, length);
            return subArray;
        }
        public static SByte[] CreateSubSByteArray(this Byte[] bytes, UInt32 offset, UInt32 length)
        {
            SByte[] subArray = new SByte[length];
            ArrayCopier.Copy(bytes, offset, subArray, 0, length);
            return subArray;
        }
        //
        // TODO: Add ParseSingle and ParseDouble
        //

        // Assume compareOffset + length <= compare.Length
        public static Boolean EqualsAt(this Byte[] array, UInt32 offset, Byte[] compare, UInt32 compareOffset, UInt32 length)
        {
            if (offset + length > array.Length)
                return false;
            for (int i = 0; i < length; i++)
            {
                if (array[offset + i] != compare[compareOffset + i])
                    return false;
            }
            return true;
        }
        public static Int32 IndexOf(this Byte[] array, UInt32 offset, UInt32 limit, Byte[] b)
        {
            while (offset + b.Length <= limit)
            {
                int matchLength = 0;
                while (true)
                {
                    if (matchLength >= b.Length)
                        return (int)offset;
                    if (array[offset + matchLength] != b[matchLength])
                        break;
                    matchLength++;
                }
                offset++;
            }
            return -1;
        }
        public static Int32 IndexOf(this Byte[] array, UInt32 offset, UInt32 limit, Byte b)
        {
            while (offset < limit)
            {
                if (array[offset] == b)
                    return (int)offset;
                offset++;
            }
            return -1;
        }
        // Returns: UInt32.MaxValue on error
        public static UInt32 IndexOfUInt32(this Byte[] array, UInt32 offset, UInt32 limit, Byte b)
        {
            while (offset < limit)
            {
                if (array[offset] == b)
                    return offset;
                offset++;
            }
            return UInt32.MaxValue;
        }
        public static Int32 LastIndexOf(this Byte[] array, UInt32 offset, UInt32 limit, Byte b)
        {
            if (limit <= offset)
                return -1;
            do
            {
                limit--;
                if (array[limit] == b)
                    return (int)limit;
            } while (limit > offset);
            return -1;
        }
    }
}