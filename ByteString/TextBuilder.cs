// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.IO;
using System.Text;

#if WindowsCE
using ArrayCopier = System.MissingInCEArrayCopier;
#else
using ArrayCopier = System.Array;
#endif

namespace More
{
    // AppendAscii = The data being appended is guaranteed to be valid ascii (0-127)
    //               Note: this does not include the extended ascii codes (128 - 255)
    // AppendUtf8  = If the data is a Char or String, it will be appended as utf8 encoded
    // AppendNumber = Append the number converted to a string
    public interface ITextBuilder
    {
        UInt32 Length { get; }
        void Clear();

        //void ShiftRight(UInt32 shiftAmount, UInt32 offset, UInt32 length);

        // The caller is guaranteeing that 0 <= c <= 127
        void AppendAscii(Byte c);
        // The caller is guaranteeing that 0 <= c <= 127
        void AppendAscii(Char c);

        /// <summary>
        /// The 'Ascii' part means the caller is guaranteeing that every char in str is between 0 and 127 (inclusive)
        /// </summary>
        /// <param name="str">The string to append to the text</param>
        void AppendAscii(String str);
        void AppendAscii(Byte[] str);
        void AppendAscii(Byte[] str, UInt32 offset, UInt32 length);

        //void AppendFormatAscii(String format, params Object[] obj);

        void AppendBoolean(Boolean value);

        void AppendNumber(UInt32 num);
        void AppendNumber(UInt32 num, Byte @base);
        void AppendNumber(Int32 num);
        void AppendNumber(Int32 num, Byte @base);
    }

    public struct StringDataBuilder : ITextBuilder
    {
        public readonly StringBuilder builder;
        public StringDataBuilder(StringBuilder builder)
        {
            this.builder = builder;
        }
        public UInt32 Length { get { return (uint)builder.Length; } }
        public void Clear()
        {
            this.builder.Length = 0;
        }
        // The caller is guaranteeing that 0 <= c <= 127
        public void AppendAscii(Byte c)
        {
            builder.Append((Char)c);
        }
        // The caller is guaranteeing that 0 <= c <= 127
        public void AppendAscii(Char c)
        {
            builder.Append(c);
        }
        // The caller is guaranteeing that every char in str is between 0 and 127 (inclusive)
        public void AppendAscii(String str)
        {
            builder.Append(str);
        }
        public void AppendAscii(Byte[] str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                builder.Append((char)str[i]);
            }
        }
        public void AppendAscii(Byte[] str, UInt32 offset, UInt32 length)
        {
            for (int i = 0; i < length; i++)
            {
                builder.Append((char)str[offset + i]);
            }
        }
        // The caller is guaranteeing that every char in str is between 0 and 127 (inclusive)
        public void AppendFormatAscii(String format, params Object[] obj)
        {
            builder.AppendFormat(format, obj);
        }

        public void AppendUtf8(Char c)
        {
            builder.Append(c);
        }
        public void AppendUtf8(String str)
        {
            builder.Append(str);
        }

        public void Append(Encoder encoder, String str)
        {
            builder.Append(str);
        }

        public void AppendBoolean(Boolean value)
        {
            builder.Append(value ? "true" : "false");
        }

        public void AppendNumber(UInt32 num)
        {
            builder.Append(num);
        }
        public void AppendNumber(UInt32 num, Byte @base)
        {
            builder.Append(num.ToString("X"));
        }
        public void AppendNumber(Int32 num)
        {
            builder.Append(num);
        }
        public void AppendNumber(Int32 num, Byte @base)
        {
            builder.Append(num.ToString("X"));
        }
    }
    public class ByteBuilder : ITextBuilder
    {
        const UInt32 DefaultInitialLength = 16;

        public Byte[] bytes;
        public UInt32 contentLength;
        public ByteBuilder()
            : this(DefaultInitialLength)
        {
        }
        public ByteBuilder(UInt32 initialLength)
        {
            this.bytes = new Byte[initialLength];
            this.contentLength = 0;
        }
        public ByteBuilder(Byte[] bytes)
        {
            this.bytes = bytes;
            this.contentLength = 0;
        }
        public UInt32 Length { get { return contentLength; } }
        public void Clear()
        {
            this.contentLength = 0;
        }

        /*
        public void ShiftRight(UInt32 shiftAmount, UInt32 offset, UInt32 length)
        {
            // Shift the beginning of the request to compensate for a smaller Content-Length
            if (shift > 0)
            {
                while (offset < length)
                {
                    bytes[
                }
                var offset = contentLengthOffset - 1;
                while (true)
                {
                    builder.bytes[offset + shift] = builder.bytes[offset];
                    if (offset == 0)
                        break;
                    offset--;
                }
            }
            return shift;
        }
        */

        public void EnsureTotalCapacity(UInt32 capacity)
        {
            if (bytes.Length < capacity)
            {
                UInt32 newLength = (UInt32)bytes.Length * 2U;
                if (newLength < capacity)
                {
                    newLength = capacity;
                }
                var newBytes = new Byte[newLength];
                ArrayCopier.Copy(bytes, newBytes, contentLength);
                bytes = newBytes;
            }
        }
        public void ReadUntilClosed(Stream stream, UInt32 minimumReadBuffer)
        {
            while (true)
            {
                EnsureTotalCapacity(contentLength + minimumReadBuffer);
                Int32 bytesReceived = stream.Read(bytes, (int)contentLength, (int)(bytes.Length - contentLength));
                if (bytesReceived <= 0)
                    return;
                contentLength += (UInt32)bytesReceived;
            }
        }

        // The caller is guaranteeing that 0 <= c <= 127
        public void AppendAscii(Byte c)
        {
            EnsureTotalCapacity(contentLength + 1);
            bytes[contentLength++] = c;
        }
        // The caller is guaranteeing that 0 <= c <= 127
        public void AppendAscii(Char c)
        {
            EnsureTotalCapacity(contentLength + 1);
            bytes[contentLength++] = (Byte)c;
        }
        // The caller is guaranteeing that every char in str is between 0 and 127 (inclusive)
        public void AppendAscii(String str)
        {
            EnsureTotalCapacity(contentLength + (uint)str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                bytes[contentLength + i] = (Byte)str[i]; // Can do since this must be an Ascii string
            }
            contentLength += (uint)str.Length;
        }
        public void AppendAscii(Byte[] str)
        {
            EnsureTotalCapacity(contentLength + (uint)str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                bytes[contentLength + i] = str[i];
            }
            contentLength += (uint)str.Length;
        }
        public void AppendAscii(Byte[] str, UInt32 offset, UInt32 length)
        {
            EnsureTotalCapacity(contentLength + (uint)str.Length);
            for (int i = 0; i < length; i++)
            {
                bytes[contentLength + i] = str[offset + i];
            }
            contentLength += (uint)str.Length;
        }
        // The caller is guaranteeing that every char in str is between 0 and 127 (inclusive)
        //public void AppendFormatAscii(String format, params Object[] obj)
        //{
        //    String.Format(
        //    builder.AppendFormat(format, obj);
        //}

        public void Append(Byte[] content)
        {
            EnsureTotalCapacity(contentLength + (UInt32)content.Length);
            ArrayCopier.Copy(content, 0, bytes, contentLength, content.Length);
            contentLength += (UInt32)content.Length;
        }
        public void Append(Byte[] content, UInt32 offset, UInt32 length)
        {
            EnsureTotalCapacity(contentLength + length);
            ArrayCopier.Copy(content, offset, bytes, contentLength, length);
            contentLength += length;
        }

        public void AppendBoolean(Boolean value)
        {
            AppendAscii(value ? "true" : "false");
        }

        public void AppendNumber(Int32 num)
        {
            AppendNumber(num, 10);
        }
        public void AppendNumber(Int32 num, Byte @base)
        {
            // Enusure Capacity for max value
            if (@base >= 10)
            {
                EnsureTotalCapacity(contentLength + 11); // 11 = -2147483648 (also works with larger bases)
            }
            else
            {
                EnsureTotalCapacity(contentLength + 33); // 32 = '-' 11111111 11111111 1111111 11111111
            }
            if (num < 0)
            {
                bytes[contentLength++] = (Byte)'-';
                AppendNumber((UInt32)(-num), @base);
            }
            else
            {
                AppendNumber((UInt32)num, @base);
            }
        }
        public const String Chars = "0123456789ABCDEF";
        public void AppendNumber(UInt32 num)
        {
            AppendNumber(num, 10);
        }
        public void AppendNumber(UInt32 num, Byte @base)
        {
            if (@base > Chars.Length)
                throw new ArgumentOutOfRangeException("base", String.Format("base cannot be greater than {0}", Chars.Length));

            // Enusure Capacity for max value
            if (@base >= 10)
            {
                EnsureTotalCapacity(contentLength + 10); // 10 = 4294967295 (also works with larger bases)
            }
            else
            {
                EnsureTotalCapacity(contentLength + 32); // 32 = 11111111 11111111 1111111 11111111
            }

            if (num == 0)
            {
                bytes[contentLength++] = (Byte)'0';
            }
            else
            {
                var start = contentLength;
                do
                {
                    bytes[contentLength++] = (Byte)Chars[(int)(num % @base)];
                    num = num / @base;
                } while (num != 0);

                // reverse the string
                UInt32 limit = ((contentLength - start) / 2);
                for (UInt32 i = 0; i < limit; i++)
                {
                    var temp = bytes[start + i];
                    bytes[start + i] = bytes[contentLength - 1 - i];
                    bytes[contentLength - 1 - i] = temp;
                }
            }
        }
        /*
        public String Decode(Encoding encoding)
        {
            return encoding.GetString(bytes, 0, (Int32)contentLength);
        }
        */
    }
}
