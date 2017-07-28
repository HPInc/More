// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;

namespace More
{
    public static class Ascii
    {
        public static Boolean StartsWithAscii(this String value, Byte[] text, UInt32 offset, UInt32 length)
        {
            return StartsWithAscii(value, 0, text, offset, length);
        }
        public static Boolean StartsWithAscii(this String value, UInt32 stringOffset, Byte[] text, UInt32 offset, UInt32 length)
        {
            if (length > value.Length)
                return false;
            for (UInt32 i = 0; i < length; i++)
            {
                if (text[offset + i] != value[(int)(stringOffset + i)])
                    return false;
            }
            return true;
        }
        public static Boolean EqualsAscii(this String value, Byte[] text, UInt32 offset, UInt32 length)
        {
            return EqualsAscii(value, 0, text, offset, length);
        }
        public static Boolean EqualsAscii(this String value, UInt32 stringOffset, Byte[] text, UInt32 offset, UInt32 length)
        {
            if (length != value.Length)
                return false;
            for (UInt32 i = 0; i < length; i++)
            {
                if (text[offset + i] != value[(int)(stringOffset + i)])
                    return false;
            }
            return true;
        }
        /// <summary>The maximum number of bytes it would take to encode a C# Char type</summary>
        public const UInt32 MaxCharEncodeLength = 3;
        public static Byte GetCharEncodeLength(Char c)
        {
            return 1;
        }
        public static UInt32 GetEncodeLength(String str)
        {
            return (uint)str.Length;
        }
        // Returns the offset after encoding the character
        public static UInt32 EncodeChar(Char c, Byte[] buffer, UInt32 offset)
        {
            buffer[offset] = (Byte)c;
            return offset + 1;
        }
        // Returns the offset after encoding the character
        // Note: it is assumed that the caller will have already calculated the encoded length, for
        //       that reason, this method does not return the offset after the encoding
        public static UInt32 Encode(String str, Byte[] buffer, UInt32 offset)
        {
            for (int i = 0; i < str.Length; i++)
            {
                buffer[offset++] = (Byte)str[i];
            }
            return offset;
        }
        // Returns the offset after encoding the character
        // Note: it is assumed that the caller will have already calculated the encoded length, for
        //       that reason, this method does not return the offset after the encoding
        public static unsafe Byte* EncodeUnsafe(String str, Byte* buffer)
        {
            for (int i = 0; i < str.Length; i++)
            {
                *buffer = (Byte)str[i];
                buffer++;
            }
            return buffer;
        }
    }
    public static class ByteString
    {
        //
        // Number Parsing
        //
        static UInt32 ConsumeNum(this Byte[] array, UInt32 offset, UInt32 limit)
        {
            if (offset >= limit)
                return offset;
            if (array[offset] == '-')
                offset++;
            while (true)
            {
                if (offset >= limit)
                    return offset;
                var c = array[offset];
                if (c < '0' || c > '9')
                    return offset;
                offset++;
            }
        }
        // Returns the new offset of all the digits parsed.  Returns 0 to indicate overflow or invalid number.
        public static UInt32 TryParseUInt32(this Byte[] array, UInt32 offset, UInt32 limit, out UInt32 value)
        {
            if (offset >= limit) // Invalid number
            {
                value = 0;
                return 0;
            }
            UInt32 result;
            {
                Byte c = array[offset];
                if (c > '9' || c < '0')
                {
                    value = 0; // Invalid number
                    return 0;
                }
                result = (uint)(c - '0');
            }
            while (true)
            {
                offset++;
                if (offset >= limit)
                    break;
                var c = array[offset];
                if (c > '9' || c < '0')
                    break;

                UInt32 newResult = result * 10 + c - '0';
                if (newResult < result)
                {
                    value = 0;
                    return 0; // Overflow
                }
                result = newResult;
            }

            value = result;
            return offset;
        }
        // Returns the new offset of all the digits parsed.  Returns 0 to indicate overflow.
        public static UInt32 TryParseInt32(this Byte[] array, UInt32 offset, UInt32 limit, out Int32 value)
        {
            if (offset >= limit)
            {
                value = 0;
                return 0;
            }
            Boolean negative;
            {
                var c = array[offset];
                if (c == '-')
                {
                    negative = true;
                    offset++;
                }
                else
                {
                    negative = false;
                }
            }
            if (offset >= limit) // Invalid number
            {
                value = 0;
                return 0;
            }
            UInt32 result;
            {
                Byte c = array[offset];
                if (c > '9' || c < '0')
                {
                    value = 0; // Invalid number
                    return 0;
                }
                result = (uint)(c - '0');
            }
            while (true)
            {
                offset++;
                if (offset >= limit)
                    break;
                var c = array[offset];
                if (c > '9' || c < '0')
                    break;

                UInt32 newResult = result * 10 + c - '0';
                if (newResult < result)
                {
                    value = 0;
                    return 0; // Overflow
                }
                result = newResult;
            }

            if (negative)
            {
                if (result > ((UInt32)Int32.MaxValue) + 1)
                {
                    value = 0;
                    return 0; // Overflow
                }
                value = -(int)result;
            }
            else
            {
                if (result > (UInt32)Int32.MaxValue)
                {
                    value = 0;
                    return 0; // Overflow
                }
                value = (int)result;
            }
            value = negative ? -(Int32)result : (Int32)result;
            return offset;
        }
        public static UInt32 ParseByte(this Byte[] array, UInt32 offset, UInt32 limit, out Byte value)
        {
            UInt32 uint32;
            var newOffset = array.TryParseUInt32(offset, limit, out uint32);
            if (newOffset == 0 || uint32 > (UInt32)Byte.MaxValue)
                throw new OverflowException(String.Format("Overflow while parsing '{0}' as a Byte",
                    System.Text.Encoding.ASCII.GetString(array, (int)offset, (newOffset == 0) ? (int)ConsumeNum(array, offset, limit) : (int)newOffset)));
            value = (Byte)uint32;
            return newOffset;
        }
        public static UInt32 ParseUInt16(this Byte[] array, UInt32 offset, UInt32 limit, out UInt16 value)
        {
            UInt32 uint32;
            var newOffset = array.TryParseUInt32(offset, limit, out uint32);
            if (newOffset == 0 || uint32 > (UInt32)UInt16.MaxValue)
                throw new OverflowException(String.Format("Overflow while parsing '{0}' as a UInt16",
                    System.Text.Encoding.ASCII.GetString(array, (int)offset, (newOffset == 0) ? (int)ConsumeNum(array, offset, limit) : (int)newOffset)));
            value = (UInt16)uint32;
            return newOffset;
        }
        public static UInt32 ParseUInt32(this Byte[] array, UInt32 offset, UInt32 limit, out UInt32 value)
        {
            var newOffset = array.TryParseUInt32(offset, limit, out value);
            if (newOffset == 0)
                throw new OverflowException(String.Format("Overflow while parsing '{0}' as a UInt32",
                    System.Text.Encoding.ASCII.GetString(array, (int)offset, (newOffset == 0) ? (int)array.ConsumeNum(offset, limit) : (int)newOffset)));
            return newOffset;
        }
        public static UInt32 ParseInt32(this Byte[] array, UInt32 offset, UInt32 limit, out Int32 value)
        {
            var newOffset = array.TryParseInt32(offset, limit, out value);
            if (newOffset == 0)
                throw new OverflowException(String.Format("Overflow while parsing '{0}' as a Int32",
                    System.Text.Encoding.ASCII.GetString(array, (int)offset, (newOffset == 0) ? (int)array.ConsumeNum(offset, limit) : (int)newOffset)));
            return newOffset;
        }
    }
    public struct AsciiPartsBuilder
    {
        readonly String[] parts;
        readonly String[] inbetweens;
        public AsciiPartsBuilder(String[] parts, params String[] inbetweens)
        {
            if (inbetweens.Length + 1 != parts.Length)
            {
                throw new ArgumentException(String.Format(
                    "parts.Length ({0}) must be equal to inbetweens.Length + 1 ({1})",
                    parts.Length, inbetweens.Length + 1));
            }
            this.parts = parts;
            this.inbetweens = inbetweens;
        }
        public UInt32 PrecalculateLength()
        {
            UInt32 totalSize = 0;
            for (Int32 i = 0; i < parts.Length; i++)
            {
                totalSize += (UInt32)parts[i].Length;
            }
            for (Int32 i = 0; i < inbetweens.Length; i++)
            {
                totalSize += (UInt32)inbetweens[i].Length;
            }
            return totalSize;
        }
        public unsafe void BuildInto(Byte[] dst)
        {
            UInt32 offset = 0;
            for (Int32 i = 0; i < parts.Length - 1; i++)
            {
                offset = Ascii.Encode(parts[i], dst, offset);
                offset = Ascii.Encode(inbetweens[i], dst, offset);
            }
            Ascii.Encode(parts[parts.Length - 1], dst, offset);
        }
        public unsafe void BuildInto(Byte* dst)
        {
            for (Int32 i = 0; i < parts.Length - 1; i++)
            {
                dst = Ascii.EncodeUnsafe(parts[i], dst);
                dst = Ascii.EncodeUnsafe(inbetweens[i], dst);
            }
            Ascii.EncodeUnsafe(parts[parts.Length - 1], dst);
        }
    }
}
