// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

#if WindowsCE
using ByteParser = System.MissingInCEByteParser;
#else
using ByteParser = System.Byte;
#endif

namespace More
{
    /*
    public static class GenericExtensions
    {
            public static void DataString<T>(this T value, StringBuilder builder)
            {
                builder.Append(value.ToString());
            }
    }
    */
#if !WindowsCE
    public static class GCExtensions
    {
        static Int32[] lastGenCount;

        static void VerifyLastGenCountSize(Int32 generation)
        {
            Int32 size = (generation > 2) ? generation + 1 : 3;

            if(lastGenCount == null)
            {
                lastGenCount = new Int32[size];
            }
            else
            {
                if(lastGenCount.Length < size)
                {
                    Int32[] newLastGenCount = new Int32[size];
                    Array.Copy(lastGenCount, newLastGenCount, lastGenCount.Length);
                    lastGenCount = newLastGenCount;
                }
            }
        }
        public static Int32 CountDiff(Int32 generation)
        {
            Int32 currentCount = GC.CollectionCount(generation);

            lock(typeof(GCExtensions))
            {
                VerifyLastGenCountSize(generation);

                Int32 diff = currentCount - lastGenCount[generation];
                lastGenCount[generation] = currentCount;
                return diff;
            }
        }
        public static void SaveCountsUpToGeneration(Int32 generation)
        {
            lock (typeof(GCExtensions))
            {
                VerifyLastGenCountSize(generation);

                for (int i = 0; i <= generation; i++)
                {
                    lastGenCount[i] = GC.CollectionCount(i);
                }
            }
        }
    }
#endif

    public static class ArrayExtensions
    {
        // Returns the index of the first element that is not equal, if they match,returns -1
        public static Int32 DiffIndex<T>(this T[] a, T[] b)
        {
            if (a == null)
            {
                if (b != null && b.Length != 0) return 0;
            }
            
            if (b == null)
            {
                return (a.Length == 0) ? -1 : 0;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (i >= b.Length || !a[i].Equals(b[i])) return i;
            }

            return -1;
        }
    }
    public enum EncodingType
    {
        Default = 0,
        Ascii   = 1,
        Utf7    = 2,
        Utf8    = 3,
        Utf32   = 4,
        Unicode = 5,
    }
    public static class EncodingParser
    {
        public static Encoding GetEncoding(this EncodingType encodingType)
        {
            switch (encodingType)
            {
                case EncodingType.Default: return Encoding.Default;
                case EncodingType.Ascii: return Encoding.ASCII;
                case EncodingType.Utf7: return Encoding.UTF7;
                case EncodingType.Utf8: return Encoding.UTF8;
#if !WindowsCE
                case EncodingType.Utf32: return Encoding.UTF32;
#endif
                case EncodingType.Unicode: return Encoding.Unicode;
            }
            throw new InvalidOperationException(String.Format("Unknown EncodingType '{0}'", encodingType));
        }
    }
    public static class StringBuilderExtensions
    {
        public static void AppendUtf8(this StringBuilder builder, Byte[] bytes)
        {
            builder.AppendUtf8(bytes, 0, (uint)bytes.Length);
        }
        public static void AppendUtf8(this StringBuilder builder, Byte[] bytes, UInt32 offset, UInt32 limit)
        {
            // Small optimization to prevent multiple resizes
            builder.EnsureCapacity((int)(builder.Length + (limit - offset)));

            while (true)
            {
                if (offset >= limit)
                    return;
                var c = Utf8.Decode(bytes, ref offset, limit);
                builder.Append((Char)c);
            }
        }
    }
    public static class MiscStringExtensions
    {
        public static Boolean EqualsIgnoreCase(this String str, String other)
        {
            return str.Equals(other, StringComparison.OrdinalIgnoreCase);
        }
        public static String JsonEncode(this String str)
        {
            if (str == null) return "null";

            Boolean changed = false;
            Int32 encodeLength = 2;
            for (int i = 0; i < str.Length; i++)
            {
                Char c = str[i];
                if (c == '\\' || c == '"' || c == '\r' || c == '\n' || c == '\t')
                {
                    changed = true;
                    encodeLength += 2;
                }
                else
                {
                    encodeLength++;
                }
            }

            if (!changed) return '"' + str + '"';

            Char[] newString = new Char[encodeLength];
            newString[0] = '"';
            Int32 index = 1;
            for (int i = 0; i < str.Length; i++)
            {
                Char c = str[i];
                switch(c)
                {
                    case '\\':
                        newString[index++] = '\\';
                        newString[index++] = '\\';
                        break;
                    case '"':
                        newString[index++] = '\\';
                        newString[index++] = '"';
                        break;
                    case '\r':
                        newString[index++] = '\\';
                        newString[index++] = 'r';
                        break;
                    case '\n':
                        newString[index++] = '\\';
                        newString[index++] = 'n';
                        break;
                    case '\t':
                        newString[index++] = '\\';
                        newString[index++] = 't';
                        break;
                    default:
                        newString[index++] = c;
                    break;
                }
            }
            newString[index++] = '"';

            Debug.Assert(index == encodeLength);
            return new String(newString);
        }
        /*
        public static Boolean SubstringEquals(this String str, Int32 offset, String compare)
        {
            if (offset + compare.Length > str.Length)
            {
                return false;
            }
            return str.Match(offset, compare) == compare.Length;
        }
        [System.Runtime.ConstrainedExecution.ReliabilityContract(
            System.Runtime.ConstrainedExecution.Consistency.WillNotCorruptState,
            System.Runtime.ConstrainedExecution.Cer.MayFail)]
        public static unsafe Int32 Match(this String haystack, Int32 offset, String needle)
        {
            if (haystack == null || needle == null) throw new ArgumentNullException();

            int originalLength = haystack.Length - offset;
            if (originalLength <= 0) throw new ArgumentOutOfRangeException("offset");
            if (needle.Length < originalLength)
            {
                originalLength = needle.Length;
            }


            fixed (char* fixedHaystackPtr = haystack)
            {
                fixed (char* fixedNeedlePtr = needle)
                {
                    char* haystackPtr = fixedHaystackPtr + offset;
                    char* needlePtr = fixedNeedlePtr;

                    //Console.WriteLine("Match('{0}', {1}, '{2}') haystackPtr = {3}, needlePtr = {4}",
                    //    haystack, offset, needle, (long)haystackPtr, (long)needlePtr);

                    for(int i = 0; i < originalLength; i++)
                    {
                        if(*haystackPtr != *needlePtr) return i;
                        haystackPtr++;
                        needlePtr++;
                        //Console.WriteLine("Match('{0}', {1}, '{2}') haystackPtr = {3}, needlePtr = {4}",
                        //    haystack, offset, needle, (long)haystackPtr, (long)needlePtr);
                    }

                    return originalLength;
                }
            }
        }
        */
        public static String[] SplitCorrectly(this String str, Char seperator)
        {
            if (str == null || str.Length == 0) return null;

            if (str[0] == seperator) throw new FormatException(String.Format("In the string '{0}', the first character can't be a seperator '{1}'",
                str, seperator));
            if (str[str.Length - 1] == seperator) throw new FormatException(String.Format("In the string '{0}', the last character can't be a seperator '{1}'",
                str, seperator));

            Int32 seperatorCount = 0;
            for (int i = 1; i < str.Length - 1; i++)
            {
                if (str[i] == seperator)
                {
                    if (str[i - 1] == seperator)
                    {
                        throw new FormatException(String.Format("In the string '{0}', expected something in between the seperator '{1}'",
                            str, seperator));
                    }
                    seperatorCount++;
                }
            }

            String[] splitStrings = new String[seperatorCount + 1];
            Int32 splitOffset = 0;

            Int32 lastOffset = 0;
            Int32 currentOffset = 1;
            while (currentOffset < str.Length)
            {
                if (str[currentOffset] == seperator)
                {
                    splitStrings[splitOffset++] = str.Substring(lastOffset, currentOffset - lastOffset);
                    lastOffset = currentOffset + 1;
                    currentOffset += 2;
                }
                else
                {
                    currentOffset++;
                }

            }

            splitStrings[splitOffset++] = str.Substring(lastOffset, currentOffset - lastOffset);

            return splitStrings;
        }
        /*
        Escape  Character Name              Unicode encoding
        ======  ==============              ================
        \\      Backslash                   0x005C
        \0      Null                        0x0000
        \a      Alert                       0x0007
        \b      Backspace                   0x0008
        \f      Form feed                   0x000C
        \n      New line                    0x000A
        \r      Carriage return             0x000D
        \t      Horizontal tab              0x0009
        \v      Vertical tab                0x000B
        \x      Hexadecimal Byte            \x41 = "A" = 0x41
        */
        public static Byte[] ParseStringLiteral(this String literal, Int32 offset, out Int32 outLength)
        {
            Int32 length = 0;
            Byte[] buffer = new Byte[literal.Length];

            Int32 save;

            while (true)
            {
                if (offset >= literal.Length)
                {
                    outLength = length;
                    return buffer;
                    //return builder.ToString();
                }

                save = offset;
                while (true)
                {
                    if (literal[offset] == '\\') break;
                    offset++;
                    if (offset >= literal.Length)
                    {
                        do
                        {
                            buffer[length++] = (byte)literal[save++]; // do I need to do an Encoding?
                        } while (save < literal.Length);
                        outLength = length;
                        return buffer;
                    }
                }

                // the character at i is '\'
                while (save < offset)
                {
                    buffer[length++] = (byte)literal[save++]; // do I need to do an Encoding?
                }
                offset++;
                if (offset >= literal.Length) throw new FormatException("Your literal string ended with '\'");

                char escapeChar = literal[offset];
                if (escapeChar == 'n') buffer[length++] = (byte)'\n';
                else if (escapeChar == '\\') buffer[length++] = (byte)'\\';
                else if (escapeChar == '0') buffer[length++] = (byte)'\0';
                else if (escapeChar == 'a') buffer[length++] = (byte)'\a';
                else if (escapeChar == 'r') buffer[length++] = (byte)'\r';
                else if (escapeChar == 't') buffer[length++] = (byte)'\t';
                else if (escapeChar == 'v') buffer[length++] = (byte)'\v';
                else if (escapeChar == 'x')
                {
                    offset++;
                    if (offset + 1 >= literal.Length) throw new FormatException("The escape character 'x' needs at least 2 digits");

                    Byte output;
                    String sequence = literal.Substring(offset, 2);
                    if (!ByteParser.TryParse(sequence, System.Globalization.NumberStyles.HexNumber, null, out output))
                    {
                        throw new FormatException(String.Format("Could not parse the hexadecimal escape sequence '\\x{0}' as a hexadecimal byte", sequence));
                    }
                    Console.WriteLine("Parsed '\\x{0}' as '{1}' (0x{2:X2}) ((char)0x{3:X2})", sequence, (char)output, output, (byte)(char)output);
                    buffer[length++] = output;
                    offset++;
                }
                else throw new FormatException(String.Format("Unrecognized escape sequence '\\{0}'", escapeChar));

                offset++;
            }
        }
        public static String UnderscoreToCamelCase(this String underscoreString)
        {
            if (String.IsNullOrEmpty(underscoreString)) return underscoreString;

            Char[] buffer = new Char[underscoreString.Length];
            Int32 bufferIndex = 0;

            Int32 offset = 0;

            while (true)
            {
                if (underscoreString[offset] != '_') break;
                offset++;
                if (offset >= underscoreString.Length) return "";
            }

            buffer[bufferIndex++] = Char.ToUpper(underscoreString[offset]);
            offset++;

            while (offset < underscoreString.Length)
            {
                Char c = underscoreString[offset];
                if (c != '_')
                {
                    buffer[bufferIndex++] = Char.ToLower(c);
                }
                else
                {
                    while (true)
                    {
                        offset++;
                        if (offset >= underscoreString.Length) goto DONE;
                        c = underscoreString[offset];
                        if (c != '_') break;
                    }
                    buffer[bufferIndex++] = Char.ToUpper(c);
                }
                offset++;
            }
            DONE:
            return new String(buffer, 0, bufferIndex);
        }
        public static String CamelToUpperUnderscoreCase(this String camelString)
        {
            if (camelString.Length <= 0) return camelString;
            Char c = camelString[0];

            Boolean atUpper = Char.IsUpper(c);
            Int32 nextIndex = 1;
            UInt32 underscoreCount = 0;

            while (true)
            {
                if (nextIndex >= camelString.Length) break;
                c = camelString[nextIndex++];
                if (atUpper)
                {
                    atUpper = Char.IsUpper(c);
                }
                else
                {
                    atUpper = Char.IsUpper(c);
                    if(atUpper)
                    {
                        underscoreCount++;
                    }
                }
            }
            
            Char[] underscoreString = new Char[camelString.Length + underscoreCount];

            c = camelString[0];
            atUpper = Char.IsUpper(c);
            nextIndex = 1;

            if (atUpper)
            {
                underscoreString[0] = c;
            }
            else
            {
                underscoreString[0] = Char.ToUpper(c);
            }
            Int32 underscoreIndex = 1;

            while (true)
            {
                if (nextIndex >= camelString.Length) break;
                c = camelString[nextIndex++];
                if (atUpper)
                {
                    atUpper = Char.IsUpper(c);
                    if (atUpper)
                    {
                        underscoreString[underscoreIndex++] = c;
                    }
                    else
                    {
                        underscoreString[underscoreIndex++] = Char.ToUpper(c);
                    }
                }
                else
                {
                    atUpper = Char.IsUpper(c);
                    if (atUpper)
                    {
                        underscoreString[underscoreIndex++] = '_';
                        underscoreString[underscoreIndex++] = c;
                    }
                    else
                    {
                        underscoreString[underscoreIndex++] = Char.ToUpper(c);
                    }
                }
            }


            if (underscoreIndex != underscoreString.Length) throw new InvalidOperationException(String.Format(
                "CodeBug: CamelToUpperUnderscoreCase function has a bug, expected string to be {0} but was {1}", underscoreString.Length, underscoreIndex));

            return new String(underscoreString);
        }
    }
    public static class Int24
    {
        public const Int32 MinValue = -0x800000;
        public const Int32 MaxValue =  0x7FFFFF;
    }
    public static class UInt24
    {
        public const Int32 MinValue = 0;
        public const Int32 MaxValue = 0xFFFFFF;
    }
    public static class ByteArrayExtensions
    {
        //
        // http://www.dlugosz.com/ZIP2/VLI.html
        //
        public static UInt32 SetVarUInt32(this Byte[] bytes, UInt32 offset, UInt32 value)
        {
            if (value <= 127)
            {
                bytes[offset] = (Byte)value;
                return offset + 1;
            }
            if (value <= 16383)
            {
                bytes[offset    ] = (Byte)(0xC0 | (value >> 8));
                bytes[offset + 1] = (Byte)(value);
                return offset + 2;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public static UInt32 ReadVarUInt32(this Byte[] bytes, UInt32 offset, out UInt32 value)
        {
            Byte firstByte = bytes[0];

            if (firstByte <= 127)
            {
                value = firstByte;
                return offset + 1;
            }

            if ((firstByte & 0xC0) == 0xC0)
            {
                value = (UInt32)(firstByte << 8 | bytes[offset + 1]);
                return offset = 2;
            }

            throw new NotImplementedException();
        }
        public static Boolean IsInRange(UInt32 value, Byte byteCount)
        {
            switch (byteCount)
            {
                case 1:
                    return ((value & 0xFF) == value);
                case 2:
                    return ((value & 0xFFFF) == value);
                case 3:
                    return ((value & 0xFFFFFF) == value);
                case 4:
                    return ((value & 0xFFFFFFFF) == value);
            }
            throw new InvalidOperationException(String.Format("Expected byteCount to be 1,2,3 or 4 but was {0}", byteCount));
        }
    }
    public static class ListExtensions
    {
        public static String ToDataString<T>(this List<T> list)
        {
            StringBuilder builder = new StringBuilder();
            ToDataString<T>(list, builder);
            return builder.ToString();
        }
        public static void ToDataString<T>(this List<T> list, StringBuilder builder)
        {
            if (list == null)
            {
                builder.Append("null");
                return;
            }

            builder.Append('[');
            Boolean atFirst = true;
            for (int i = 0; i < list.Count; i++)
            {
                if (atFirst) { atFirst = false; } else { builder.Append(','); }
                builder.Append(list[i]);
            }
            builder.Append("]");
        }
    }

    public static class IListExtensions
    {
        public static String ListString<T>(this IList<T> list)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) builder.Append(',');
                T item = list[i];
                if(item == null) builder.Append("null");
                else builder.Append(item.ToString());
            }
            builder.Append(']');
            return builder.ToString();
        }
    }

    public static class DictionaryExtensions
    {
        public static U GetValueNiceErrorMessage<T,U>(this Dictionary<T,U> dictionary, T key)
        {
            U value;
            if (!dictionary.TryGetValue(key, out value)) throw new KeyNotFoundException(String.Format("Key '{0}' was not found in the dictionary (has {1} items)",
                 key, dictionary.Count));
            return value;
        }
        public static String ToDataString<T,U>(this Dictionary<T,U> dictionary)
        {
            StringBuilder builder = new StringBuilder();
            ToDataString<T,U>(dictionary, builder);
            return builder.ToString();
        }
        public static void ToDataString<T,U>(this Dictionary<T,U> dictionary, StringBuilder builder)
        {
            if (dictionary == null)
            {
                builder.Append("null");
                return;
            }

            builder.Append('{');
            Boolean atFirst = true;
            foreach (KeyValuePair<T,U> pair in dictionary)
            {
                if (atFirst) { atFirst = false; } else { builder.Append(','); }
                builder.Append(pair.Key);
                builder.Append(':');
                builder.Append(pair.Value);
            }
            builder.Append('}');
        }
        public static U GetOrCreate<T, U>(this Dictionary<T, U> dictionary, T key) where U : new()
        {
            U value;
            if (dictionary.TryGetValue(key, out value)) return value;

            value = new U();
            dictionary.Add(key, value);
            return value;
        }
    }
    public static class StackExtensions
    {
        public static void Print<T>(this Stack<T> stack, TextWriter writer)
        {
            if (stack.Count <= 0)
            {
                writer.WriteLine("Empty");
            }
            else
            {
                foreach (T item in stack)
                {
                    writer.WriteLine(item);
                }
            }
        } 
    }
    public static class DateTimeExtensions
    {
        public static readonly DateTime UnixZeroTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /*
        public static double ToUnixTimeDouble(this DateTime dateTime)
        {
            return (double)(dateTime - UnixZeroTime).TotalSeconds;
        }
        */
        public static UInt32 ToUnixTime(this DateTime dateTime)
        {
            return (UInt32)(dateTime - UnixZeroTime).TotalSeconds;
        }
    }
    public static class TextWriterExtensions
    {
        public static void WriteString(this TextWriter writer, String str, UInt32 offset, UInt32 length)
        {
            if (offset == 0)
            {
                if (length == str.Length)
                {
                    writer.Write(str);
                }
                else
                {
                    writer.Write(str.Remove((Int32)length));
                }
            }
            else
            {
                writer.Write(str.Substring((Int32)offset, (Int32)length));
            }
        }
        public static void WriteLine(this TextWriter writer, UInt32 spaces, String fmt, params Object[] obj)
        {
            writer.Write(String.Format("{{0,{0}}}", spaces), String.Empty);
            writer.WriteLine(fmt, obj);
        }
        public static void WriteLine(this TextWriter writer, UInt32 spaces, String str)
        {
            writer.Write(String.Format("{{0,{0}}}", spaces), String.Empty);
            writer.WriteLine(str);
        }

        public static void Write(this TextWriter writer, UInt32 spaces, String fmt, params Object[] obj)
        {
            writer.Write(String.Format("{{0,{0}}}", spaces), String.Empty);
            writer.Write(fmt, obj);
        }
        public static void Write(this TextWriter writer, UInt32 spaces, String str)
        {
            writer.Write(String.Format("{{0,{0}}}", spaces), String.Empty);
            writer.Write(str);
        }
    }
    public static class StreamExtensions
    {
        public static void ReadFullSize(this Stream stream, Byte[] buffer, Int32 offset, Int32 size)
        {
            int lastBytesRead;

            do
            {
                lastBytesRead = stream.Read(buffer, offset, size);
                size -= lastBytesRead;

                if (size <= 0) return;

                offset += lastBytesRead;
            } while (lastBytesRead > 0);

            throw new IOException(String.Format("Reached end of stream but still expected {0} bytes", size));
        }
        public static void ReadFullSize(this Stream stream, StringBuilder builder, Encoding encoding, Byte[] buffer, Int32 size)
        {
            int lastBytesRead;

            do
            {
                lastBytesRead = stream.Read(buffer, 0, buffer.Length);
                size -= lastBytesRead;

                if (size <= 0) return;

                builder.Append(encoding.GetString(buffer, 0, lastBytesRead));
            } while (lastBytesRead > 0);

            throw new IOException(String.Format("Reached end of stream but still expected {0} bytes", size));
        }

        /*
        public static String ReadLine(this Stream stream, StringBuilder builder)
        {
            builder.Length = 0;
            while (true)
            {
                int next = stream.ReadByte();
                if (next < 0)
                {
                    if (builder.Length == 0) return null;
                    return builder.ToString();
                }

                if (next == '\n') return builder.ToString();
                if (next == '\r')
                {
                    do
                    {
                        next = stream.ReadByte();
                        if (next < 0)
                        {
                            if (builder.Length == 0) return null;
                            return builder.ToString();
                        }
                        if (next == '\n') return builder.ToString();
                        builder.Append('\r');
                    } while (next == '\r');
                }

                builder.Append((char)next);
            }
        }
        */
        /*
        public static UInt32 ReadUntil(this Stream stream, Byte[] readBuffer, out UInt32 readBufferOffset, StringBuilder builder, Byte until)
        {
            while (true)
            {
                Int32 bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                if (bytesRead <= 0) throw new IOException(String.Format("Reached end of stream while waiting for '{0}' ({1})", (Char)until, until));

                Int32 untilOffset;
                for (untilOffset = 0; untilOffset < bytesRead; untilOffset++)
                {
                    if (readBuffer[untilOffset] == until)
                    {
                        builder.Append(
                    }
                }



            }
        }
        */
    }

    public static class FileExtensions
    {
        public static Byte[] ReadFile(String filename)
        {
            //
            // 1. Get file size
            //
            FileInfo fileInfo = new FileInfo(filename);
            Int32 fileLength = (Int32)fileInfo.Length;
            Byte[] buffer = new Byte[fileLength];

            //
            // 2. Read the file contents
            //
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(filename, FileMode.Open);
                fileStream.ReadFullSize(buffer, 0, fileLength);
            }
            finally
            {
                if (fileStream != null) fileStream.Dispose();
            }

            return buffer;
        }
        public static String ReadFileToString(String filename)
        {
            FileStream fileStream = null;
            StreamReader reader = null;
            try
            {
                fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                reader = new StreamReader(fileStream);

                return reader.ReadToEnd();
            }
            finally
            {
                if(reader != null)
                {
                    reader.Dispose();
                }
                else
                {
                    if (fileStream != null) fileStream.Dispose();
                }
            }
        }

        public static Int32 ReadFile(FileInfo fileInfo, Int32 fileOffset, Byte[] buffer, FileShare shareOptions, out Boolean reachedEndOfFile)
        {
            fileInfo.Refresh();

            Int64 fileSize = fileInfo.Length;

            if (fileOffset >= fileSize)
            {
                reachedEndOfFile = true;
                return 0;
            }

            Int64 fileSizeFromOffset = fileSize - fileOffset;

            Int32 readLength;
            if (fileSizeFromOffset > (Int64)buffer.Length)
            {
                reachedEndOfFile = false;
                readLength = buffer.Length;
            }
            else
            {
                reachedEndOfFile = true;
                readLength = (Int32)fileSizeFromOffset;
            }
            if (readLength <= 0) return 0;

            using (FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, shareOptions))
            {
                fileStream.Position = fileOffset;
                fileStream.ReadFullSize(buffer, 0, readLength);
            }

            return readLength;
        }


        public static void SaveStringToFile(String filename, FileMode mode, String contents)
        {
            FileStream fileStream = null;
            StreamWriter writer = null;
            try
            {
                fileStream = new FileStream(filename, mode, FileAccess.Write);
                writer = new StreamWriter(fileStream);

                writer.Write(contents);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
                else
                {
                    if (fileStream != null) fileStream.Dispose();
                }
            }
        }

    }
}
