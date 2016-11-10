// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace More
{
    public class JsonException : FormatException
    {
        public JsonException(UInt32 lineNumber, String message)
            : base(String.Format("Error in Json(line {0}): {1}", lineNumber, message))
        {
        }
        public JsonException(UInt32 lineNumber, String message, Exception innerException)
            : base(String.Format("Error in Json(line {0}): {1}", lineNumber, message), innerException)
        {
        }
    }
    public struct NameValueOffsets
    {
        public OffsetLength name;
        public UInt32 valueOffset;
    }
}



#if COMMENT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace More
{
    public class JsonException : FormatException
    {
        public JsonException(UInt32 lineNumber, String message)
            : base(String.Format("Error in Json(line {0}): {1}", lineNumber, message))
        {
        }
        public JsonException(UInt32 lineNumber, String message, Exception innerException)
            : base(String.Format("Error in Json(line {0}): {1}", lineNumber, message), innerException)
        {
        }
    }
    public struct NameValueOffsets
    {
        public OffsetLength name;
        public UInt32 valueOffset;
    }
    public delegate UInt32 ByteArrayParser<T>(Byte[] array, UInt32 offset, UInt32 limit, out T value);
    public struct JsonConsumer
    {
        public readonly Byte[] text;
        public readonly UInt32 limit;

        public UInt32 lineNumber;

        public JsonConsumer(Byte[] text)
        {
            this.text = text;
            this.limit = (UInt32)text.Length;
            this.lineNumber = 1;
        }
        public JsonConsumer(Byte[] text, UInt32 limit)
        {
            this.text = text;
            this.limit = limit;
            this.lineNumber = 1;
        }        
        /// <summary>
        /// Returns an offset to the next non-whitespace character at or beyond given offset.
        /// If no non-whitespace is found before the json limit, it will return the limit.
        /// Because of this, make sure to check if (offset >= limit) before using the offset.
        /// </summary>
        /// <param name="offset">The offset to begin the search for the non-whitespace character.</param>
        /// <returns>The offset to the next non-whitespace character or the limit if no such character is found.</returns>
        public UInt32 ConsumeWhitespace(UInt32 offset)
        {
            while (true)
            {
                if (offset >= limit)
                    return offset;

                var c = text[offset];
                if (c == '\n')
                {
                    lineNumber++;
                }
                else if (c != ' ' && c != '\t' && c != '\r')
                {
                    return offset;
                }
                offset++;
            }
        }
        /// <summary>
        /// Returns an offset to the character after the given character.
        /// If another non-whitespace character is found first, or the json limit is reached before the given character, a JsonException will be thrown.
        /// </summary>
        /// <param name="consume">The character to consume.</param>
        /// <param name="offset">The offset to begin the search for the given <paramref name="consume"/> character.</param>
        /// <returns>The offset to the character after the given <paramref name="consume"/> character.</returns>
        /// <exception cref="JsonException"></exception>
        public UInt32 ConsumeChar(Byte consume, UInt32 offset)
        {
            while (true)
            {
                if (offset >= limit)
                    throw new JsonException(lineNumber, String.Format("expected '{0}' but reached end of input", (Char)consume));
                var c = text[offset];
                if (c == consume)
                    return offset + 1;
                if (c == '\n')
                {
                    lineNumber++;
                }
                else if (c != ' ' && c != '\t' && c != '\r')
                {
                    throw new JsonException(lineNumber, String.Format(
                        "expected '{0}' but got '{1}' (0x{2:X})", (Char)consume, (Char)c, (Byte)c));
                }
                offset++;
            }
        }
        /// <summary>
        /// Returns an offset to the character after the next json string.
        /// If the next json element is not a valid string, or the json limit is reached before a valid string, a JsonException will be thrown.
        /// </summary>
        /// <param name="offset">The offset to begin the search for a valid json string.</param>
        /// <returns>The offset to the character after the next valid json string.</returns>
        /// <exception cref="JsonException"></exception>
        public UInt32 ConsumeString(UInt32 offset)
        {
            offset = ConsumeChar((Byte)'"', offset);

            while (true)
            {
                if (offset >= limit)
                    throw new JsonException(lineNumber, "expected end of string \" but reached end of json");
                var c = text[offset];
                if (c == (Byte)'"')
                    return offset + 1;
                if (c == (Byte)'\\')
                {
                    offset++;
                    if (offset >= limit)
                        throw new JsonException(lineNumber, "expected end of string \" but reached end of json");
                    c = text[offset];
                    if (c == (Byte)'u')
                    {
                        offset += 5;
                    }
                    else
                    {
                        offset++;
                    }
                }
                else
                {
                    offset++;
                }
            }
        }
        /// <summary>
        /// Parses the next string (or 'null') and returns an offset to the character after it.
        /// If the next json element is not a valid string, or the json limit is reached before a valid string, a JsonException will be thrown.
        /// This function handles UTF8 encoded json data correctly.
        /// </summary>
        /// <param name="value">Used to return the parsed string.</param>
        /// <param name="offset">The offset to begin the search for a valid json string.</param>
        /// <returns>The offset to the character after the next valid json string.</returns>
        /// <exception cref="JsonException"></exception>
        public UInt32 ParseNullableString(out String value, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, "expected string or null but reached end of input");

            if (
                offset + 3 < limit &&
                text[offset    ] == 'n' &&
                text[offset + 1] == 'u' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 'l')
            {
                value = null;
                return offset + 4;
            }
            return ParseString(out value, offset);
        }
        /// <summary>
        /// Parses the next string and returns an offset to the character after it.
        /// If the next json element is not a valid string, or the json limit is reached before a valid string, a JsonException will be thrown.
        /// This function handles UTF8 encoded json data correctly.
        /// </summary>
        /// <param name="value">Used to return the parsed string.</param>
        /// <param name="offset">The offset to begin the search for a valid json string.</param>
        /// <returns>The offset to the character after the next valid json string.</returns>
        /// <exception cref="JsonException"></exception>
        public UInt32 ParseString(out String value, UInt32 offset)
        {
            offset = ConsumeChar((Byte)'"', offset);

            UInt32 startOfString = offset;
            UInt32 removeEscapeChars = 0;
            while (true)
            {
                if (offset >= limit)
                    throw new JsonException(lineNumber, "expected end of string \" but reached end of json");
                var c = text[offset];
                if (c == (Byte)'"')
                    break;
                if (c == (Byte)'\\')
                {
                    offset++;
                    if (offset >= limit)
                        throw new JsonException(lineNumber, "expected end of string \" but reached end of json");
                    c = text[offset];
                    if (c == (Byte)'u')
                    {
                        Char unicodeChar = (Char)(
                            (((Char)text[offset + 1]).HexDigitToValue() << 12) |
                            (((Char)text[offset + 2]).HexDigitToValue() <<  8) |
                            (((Char)text[offset + 3]).HexDigitToValue() <<  4) |
                            (((Char)text[offset + 4]).HexDigitToValue()      ) );
                        offset += 5;
                        removeEscapeChars += (6U - Utf8.GetCharEncodeLength(unicodeChar));
                    }
                    else
                    {
                        offset++;
                        removeEscapeChars++;
                    }
                }
                else
                {
                    offset++;
                }
            }

            if (removeEscapeChars == 0)
            {
                value = Encoding.UTF8.GetString(text, (Int32)startOfString, (Int32)(offset - startOfString));
            }
            else
            {
                Byte[] utf8Buffer = new Byte[offset - startOfString - removeEscapeChars];
                UInt32 utf8Offset = 0;
                offset = startOfString;
                while (utf8Offset < utf8Buffer.Length)
                {
                    var c = text[offset];
                    if (c == (Byte)'\\')
                    {
                        offset++;
                        c = text[offset];
                        if (c == (Byte)'u')
                        {
                            utf8Offset = Utf8.EncodeChar((Char)(
                                    (((Char)text[offset + 1]).HexDigitToValue() << 12) |
                                    (((Char)text[offset + 2]).HexDigitToValue() <<  8) |
                                    (((Char)text[offset + 3]).HexDigitToValue() <<  4) |
                                    (((Char)text[offset + 4]).HexDigitToValue()      ) ), utf8Buffer, utf8Offset);
                            offset += 5;
                        }
                        else
                        {
                            switch (c)
                            {
                                case (Byte)'"': utf8Buffer[utf8Offset]  = (Byte)'"' ; break;
                                case (Byte)'\\': utf8Buffer[utf8Offset] = (Byte)'\\'; break;
                                case (Byte)'/': utf8Buffer[utf8Offset]  = (Byte)'/' ; break;
                                case (Byte)'b': utf8Buffer[utf8Offset]  = (Byte)'\b'; break;
                                case (Byte)'f': utf8Buffer[utf8Offset]  = (Byte)'\f'; break;
                                case (Byte)'n': utf8Buffer[utf8Offset]  = (Byte)'\n'; break;
                                case (Byte)'r': utf8Buffer[utf8Offset]  = (Byte)'\r'; break;
                                case (Byte)'t': utf8Buffer[utf8Offset]  = (Byte)'\t'; break;
                                default:
                                    throw new JsonException(lineNumber, String.Format("Unrecognized escape \\{0}", c));
                            }
                            utf8Offset++;
                            offset++;
                        }
                    }
                    else
                    {
                        utf8Buffer[utf8Offset] = (Byte)c;
                        utf8Offset++;
                        offset++;
                    }
                }

                Debug.Assert(text[offset] == '"');
                value = Encoding.UTF8.GetString(utf8Buffer, 0, utf8Buffer.Length);
            }

            return offset + 1;
        }
        // Returns a string of the next value located at offset
        // offset should point to some type of character
        // that was unexpected.  It cannot be
        // 1. Whitespace
        public String NextValueForError(UInt32 offset)
        {
            var start = offset;
            while (true)
            {
                offset++;
                if (offset >= limit)
                    break;
                var c = text[offset];
                if (c == '"' || c == ',' ||
                    c == ' ' || c == '\t' || c == '\n' || c == '\r' ||
                    c == '{' || c == ']' || c == '}' || c == '[')
                    break;
            }
            return Encoding.UTF8.GetString(text, (Int32)start, (Int32)(offset - start));
        }
        /// <summary>
        /// Consumes the next json value. A JsonException will be thrown if the end of json is found or the value is invalid.
        /// </summary>
        /// <param name="offset">The offset to begin the search for a valid json value.</param>
        /// <returns>The offset to the character after the next valid json value.</returns>
        public UInt32 ConsumeValue(UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoValueMessage);

            var c = text[offset];
            if (c == '"')
                return ConsumeString(offset);
            if (c == '{')
                return ConsumeObject(offset);
            if (c == '[')
                return ConsumeArray(offset);

            if (c == 'n' &&
                offset + 3 < limit &&
                text[offset + 1] == 'u' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 'l')
                return offset + 4;

            if (c == 't' &&
                offset + 3 < limit &&
                text[offset + 1] == 'r' &&
                text[offset + 2] == 'u' &&
                text[offset + 3] == 'e')
                return offset + 4;

            if (c == 'f' &&
                offset + 4 < limit &&
                text[offset + 1] == 'a' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 's' &&
                text[offset + 4] == 'e')
                return offset + 5;

            if (c == '-' || (c >= '0' && c <= '9'))
            {
                do
                {
                    offset++;
                    if (offset >= limit)
                        return offset;
                    c = text[offset];
                } while((c >= '0' && c <= '9') ||
                        c == '.' ||
                        c == 'e' ||
                        c == 'E' ||
                        c == '+' ||
                        c == '-');
                return offset;
            }

            throw new JsonException(lineNumber, String.Format(
                "expected value but got '{0}' (0x{1:X})", (Char)c, (Byte)c));
        }
        public UInt32 ParseValue(out Object value, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoValueMessage);

            var c = text[offset];
            if (c == '"')
            {
                String valueString;
                offset = ParseString(out valueString, offset);
                value = valueString;
                return offset;
            }
            if (c == '{')
            {
                Dictionary<String, Object> valueDictionary = new Dictionary<String, Object>();
                offset = ParseObject(valueDictionary, offset);
                value = valueDictionary;
                return offset;
            }
            if (c == '[')
            {
                List<Object> array = new List<Object>();
                offset = ParseArray(array, offset);
                value = array.ToArray();
                return offset;
            }
            if (c == 'n' &&
                offset + 3 < limit &&
                text[offset + 1] == 'u' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 'l')
            {
                value = null;
                return offset + 4;
            }
            if (c == 't' &&
                offset + 3 < limit &&
                text[offset + 1] == 'r' &&
                text[offset + 2] == 'u' &&
                text[offset + 3] == 'e')
            {
                value = true;
                return offset + 4;
            }
            if (c == 'f' &&
                offset + 4 < limit &&
                text[offset + 1] == 'a' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 's' &&
                text[offset + 4] == 'e')
            {
                value = false;
                return offset + 5;
            }
            if (c == '-')
            {
                Int32 valueInt32;
                offset = Parse<Int32>(out valueInt32, ByteArray.ParseInt32, offset);
                value = valueInt32;
                return offset;
            }
            if (c >= '0' && c <= '9')
            {
                UInt32 valueUInt32;
                offset = Parse<UInt32>(out valueUInt32, ByteArray.ParseUInt32, offset);
                value = valueUInt32;
                return offset;
            }
            throw new JsonException(lineNumber, String.Format(
                "expected value but got '{0}'", NextValueForError(offset)));
        }

        public const String NoValueMessage          = "reached end of json before value";
        public const String EndedInsideValueMessage = "reached end of json while inside a value";
        public const String NoEndOfArrayMessage     = "reached end of json inside an array";
        public const String NoEndOfObjectMessage    = "reached end of json before inside an object";
        public UInt32 Parse<T>(out T value, Parser<T> parser, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoValueMessage);
            UInt32 startOfValue = offset;
            offset = ConsumeValue(offset);
            var valueString = Encoding.UTF8.GetString(text, (Int32)startOfValue, (Int32)(offset - startOfValue));
            try
            {
                value = parser(valueString);
                return offset;
            }
            catch (Exception e)
            {
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as type {1}: {2}", valueString, typeof(T).Name, e.Message));
            }
        }
        public UInt32 Parse<T>(out T value, ByteArrayParser<T> parser, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoValueMessage);
            UInt32 startOfValue = offset;
            offset = ConsumeValue(offset);

            UInt32 parserOffset;
            try
            {
                parserOffset = parser(this.text, startOfValue, offset, out value);
            }
            catch (Exception e)
            {
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as {1}: {2}",
                    Encoding.UTF8.GetString(text, (int)startOfValue, (int)(offset - startOfValue)), typeof(T).Name, e.Message), e);
            }
            if (parserOffset == 0)
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as {1}",
                    Encoding.UTF8.GetString(text, (int)startOfValue, (int)(offset - startOfValue)), typeof(T).Name));
            if (parserOffset != offset)
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as {1}, there were {2} characters but only {3} were valid",
                    Encoding.UTF8.GetString(text, (int)startOfValue, (int)(offset - startOfValue)), typeof(T).Name,
                    offset - startOfValue, parserOffset - startOfValue));
            return offset;
        }
        public UInt32 ParseWithOptionalQuotes<T>(out T value, Parser<T> parser, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoValueMessage);
            UInt32 startOfValue = offset;
            offset = ConsumeValue(offset);

            String valueString;
            if (text[startOfValue] == '"')
            {
                startOfValue++;
                valueString = Encoding.UTF8.GetString(text, (Int32)startOfValue, (Int32)(offset - startOfValue - 1));
            }
            else
            {
                valueString = Encoding.UTF8.GetString(text, (Int32)startOfValue, (Int32)(offset - startOfValue));
            }
            try
            {
                value = parser(valueString);
                return offset;
            }
            catch (Exception e)
            {
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as type {1}: {2}", valueString, typeof(T).Name, e.Message));
            }
        }
        public UInt32 ParseWithOptionalQuotes<T>(out T value, ByteArrayParser<T> parser, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoValueMessage);
            UInt32 startOfValue = offset;
            offset = ConsumeValue(offset);

            UInt32 valueLimit;
            if (text[startOfValue] == '"')
            {
                startOfValue++;
                valueLimit = offset - 1;
            }
            else
            {
                valueLimit = offset;
            }
            UInt32 parserOffset;            
            try
            {
                parserOffset = parser(text, startOfValue, valueLimit, out value);
            }
            catch (Exception e)
            {
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as {1}: {2}",
                    Encoding.UTF8.GetString(text, (int)startOfValue, (int)(valueLimit - startOfValue)), typeof(T).Name, e.Message));
            }
            if (parserOffset == 0)
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as {1}",
                    Encoding.UTF8.GetString(text, (int)startOfValue, (int)(valueLimit - startOfValue)), typeof(T).Name));
            if (parserOffset != valueLimit)
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as {1}, there were {2} characters but only {3} were valid",
                    Encoding.UTF8.GetString(text, (int)startOfValue, (int)(valueLimit - startOfValue)), typeof(T).Name,
                    valueLimit - startOfValue, parserOffset - startOfValue));
            return offset;
        }
        public UInt32 ParseBoolean(out Boolean value, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoValueMessage);
            Byte c = text[offset];
            if (c == 't' &&
                offset + 3 < limit &&
                text[offset + 1] == 'r' &&
                text[offset + 2] == 'u' &&
                text[offset + 3] == 'e')
            {
                value = true;
                return offset + 4;
            }

            if (c == 'f' &&
                offset + 4 < limit &&
                text[offset + 1] == 'a' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 's' &&
                text[offset + 4] == 'e')
            {
                value = false;
                return offset + 5;
            }
            throw new JsonException(lineNumber, String.Format(
                "expected boolean 'true' or 'false' but got '{0}'", NextValueForError(offset - 1)));
        }
        public UInt32 ParseBooleanWithOptionalQuotes(out Boolean value, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoValueMessage);

            if (text[offset] == '"')
            {
                offset++;
                if (offset >= limit)
                    throw new JsonException(lineNumber, EndedInsideValueMessage);
                offset = ParseBoolean(out value, offset);
                if (offset >= limit)
                    throw new JsonException(lineNumber, EndedInsideValueMessage);
                if (text[offset] != '"')
                    throw new JsonException(lineNumber, String.Format(
                        "expected '\"' after \"{0} but got '{1}' (0x{2:X})", value, (Char)text[offset], (Byte)text[offset]));
                return offset + 1;
            }
            else
            {
                return ParseBoolean(out value, offset);
            }
        }
        /*
        public UInt32 ParseInt32(out Int32 value, UInt32 offset)
        {
            return Parse<Int32>(out value, ByteArray.ParseInt32, offset);
        }
        public UInt32 ParseUInt32(out UInt32 value, UInt32 offset)
        {
            return Parse<UInt32>(out value, ByteArray.ParseUInt32, offset);
        }
        */
        /*
        public UInt32 ParseDouble(out Double value, UInt32 offset)
        {
            offset = NextNonWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoValueMessage);
            UInt32 startOfNumber = offset;
            Byte c;
            do
            {
                offset++;
                if (offset >= limit)
                    break;
                c = text[offset];
            } while ((c >= '0' && c <= '9') || c == '.' || c == 'e' ||
                    c == 'E' || c == '+' || c == '-');
            value = Double.Parse(Encoding.UTF8.GetString(text, (Int32)startOfNumber, (Int32)(offset - startOfNumber)));
            return offset;
        }
        */
        // Returns index after closing bracket ']'
        public UInt32 ConsumeArray(UInt32 offset)
        {
            offset = ConsumeChar((Byte)'[', offset);

            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoEndOfArrayMessage);

            {
                var c = text[offset];
                if (c == ']')
                    return offset + 1;
            }

            while (true)
            {
                offset = ConsumeValue(offset);

                offset = ConsumeWhitespace(offset);
                if (offset >= limit)
                    throw new JsonException(lineNumber, NoEndOfArrayMessage);

                var c = text[offset];
                if (c == ',')
                {
                    offset++;
                    if (offset >= limit)
                        throw new JsonException(lineNumber, NoEndOfArrayMessage);
                }
                else if (c == ']')
                {
                    return offset + 1;
                }
                else
                {
                    throw new JsonException(lineNumber, String.Format(
                        "expected ']' or ',' but got '{0}' ({1:X})", (Char)c, (Byte)c));
                }
            }
        }
        public UInt32 ParseArray(IList<Object> array, UInt32 offset)
        {
            offset = ConsumeChar((Byte)'[', offset);

            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoEndOfArrayMessage);

            {
                var c = text[offset];
                if (c == ']')
                    return offset + 1;
            }

            while (true)
            {
                Object value;
                offset = ParseValue(out value, offset);
                array.Add(value);

                offset = ConsumeWhitespace(offset);
                if (offset >= limit)
                    throw new JsonException(lineNumber, NoEndOfArrayMessage);

                var c = text[offset];
                if (c == ']')
                    return offset + 1;
                offset = ConsumeChar((Byte)',', offset);
            }
        }
        // Returns index after closing brace '}'
        public UInt32 ConsumeObject(UInt32 offset)
        {
            offset = ConsumeChar((Byte)'{', offset);

            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoEndOfObjectMessage);

            {
                var c = text[offset];
                if (c == '}')
                    return offset + 1;
            }

            while (true)
            {
                offset = ConsumeString(offset);
                offset = ConsumeChar((Byte)':', offset);
                offset = ConsumeValue(offset);

                offset = ConsumeWhitespace(offset);
                if (offset >= limit)
                    throw new JsonException(lineNumber, NoEndOfObjectMessage);

                var c = text[offset];
                if (c == ',')
                {
                    offset++;
                    if (offset >= limit)
                        throw new JsonException(lineNumber, NoEndOfObjectMessage);
                }
                else if (c == '}')
                {
                    return offset + 1;
                }
                else
                {
                    throw new JsonException(lineNumber, String.Format(
                        "expected '}' or ',' but got '{0}' ({1:X})", (Char)c, (Byte)c));
                }
            }
        }
        // Expects offset to point somewhere after the open brace or a comma
        // If end of object occurs...returns the index of the closing brace.
        // Returns the index of the value of the given key
        // Note: Guarantees return offset < limit
        public UInt32 ToObjectValue(UInt32 offset, String key)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoEndOfObjectMessage);

            {
                var c = text[offset];
                if (c == '}')
                    return offset; // no value found
            }

            while (true)
            {
                // Offset points to the starting quote
                {
                    var textStringOffset = offset + 1;
                    offset = ConsumeString(offset);
                    if (key.Length + 1 == offset - textStringOffset)
                    {
                        Boolean foundKey = true;
                        for (int i = 0; i < key.Length; i++)
                        {
                            if (text[textStringOffset] != key[i])
                            {
                                foundKey = false;
                                break;
                            }
                            textStringOffset++;
                        }

                        if (foundKey)
                        {
                            offset = ConsumeChar((Byte)':', offset);
                            offset = ConsumeWhitespace(offset);
                            if (offset >= limit)
                                throw new JsonException(lineNumber, NoEndOfObjectMessage);
                            return offset;
                        }
                    }
                }

                offset = ConsumeChar((Byte)':', offset);
                offset = ConsumeValue(offset);

                offset = ConsumeWhitespace(offset);
                if (offset >= limit)
                    throw new JsonException(lineNumber, NoEndOfObjectMessage);

                var c = text[offset];
                if (c == ',')
                {
                    offset++;
                    if (offset >= limit)
                        throw new JsonException(lineNumber, NoEndOfObjectMessage);
                }
                else if (c == '}')
                {
                    return offset;
                }
                else
                {
                    throw new JsonException(lineNumber, String.Format(
                        "expected '}' or ',' but got '{0}' ({1:X})", (Char)c, (Byte)c));
                }

                // Go to next char
                offset = ConsumeWhitespace(offset);
                if (offset >= limit)
                    throw new JsonException(lineNumber, NoEndOfObjectMessage);
            }
        }
        // Expects offset to point somewhere after the open brace or a comma
        // If end of object occurs...returns the index of the closing brace.
        // Returns the index of character after the open brace
        public UInt32 ConsumeRestOfObject(UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoEndOfObjectMessage);

            {
                var c = text[offset];
                if (c == '}')
                    return offset + 1;
            }

            while (true)
            {
                offset = ConsumeString(offset);
                offset = ConsumeChar((Byte)':', offset);
                offset = ConsumeValue(offset);
                offset = ConsumeWhitespace(offset);
                if (offset >= limit)
                    throw new JsonException(lineNumber, NoEndOfObjectMessage);

                var c = text[offset];
                if (c == ',')
                {
                    offset++;
                    if (offset >= limit)
                        throw new JsonException(lineNumber, NoEndOfObjectMessage);
                }
                else if (c == '}')
                {
                    return offset + 1;
                }
                else
                {
                    throw new JsonException(lineNumber, String.Format(
                        "expected '}' or ',' but got '{0}' ({1:X})", (Char)c, (Byte)c));
                }
            }
        }

        public UInt32 Lookup(IEnumerable<String> path, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit || text[offset] != '{')
                throw new JsonException(lineNumber, String.Format("Failed to find path '{0}'", BuildPath(path)));
            offset++;
            
            offset = Lookup(path.GetEnumerator(), offset);
            if (offset == UInt32.MaxValue)
                throw new JsonException(lineNumber, String.Format("Failed to find path '{0}'", BuildPath(path)));
            return offset;
        }
        // returns UInt32.MaxValue if not found
        UInt32 Lookup(IEnumerator<String> path, UInt32 offset)
        {
            if (!path.MoveNext())
            {
                return ConsumeWhitespace(offset);
            }

            var nextPath = path.Current;
            offset = ToObjectValue(offset, nextPath);
            if (text[offset] == '}')
                return UInt32.MaxValue;
            return Lookup(path, offset);
        }
        static String BuildPath(IEnumerable<String> path)
        {
            StringBuilder builder = new StringBuilder();
            foreach(var node in path)
            {
                if(builder.Length > 0)
                    builder.Append('.');
                builder.Append(node);
            }
            return builder.ToString();
        }
        public ObjectEnumerator ObjectMembers(UInt32 offset)
        {
            return new ObjectEnumerator(this, offset);
        }
        public struct ObjectEnumerator : IEnumerator<NameValueOffsets>
        {
            public readonly JsonConsumer consumer;
            public readonly UInt32 startOffset; // points to first char after open brace '{'

            public UInt32 currentOffset;
            NameValueOffsets current;

            // Offset should point to '{' or somewhere before the open brace
            public ObjectEnumerator(JsonConsumer consumer, UInt32 startOffset)
            {
                this.consumer = consumer;
                this.startOffset = consumer.ConsumeChar((Byte)'{', startOffset);
                this.currentOffset = startOffset;
                this.current = default(NameValueOffsets);
            }
            public void Reset()
            {
                this.currentOffset = startOffset;
            }
            public NameValueOffsets Current
            {
                get { return current; }
            }
            Object System.Collections.IEnumerator.Current
            {
                get { return current; }
            }
            public void Dispose()
            {
            }
            public Boolean MoveNext()
            {
                currentOffset = consumer.ConsumeWhitespace(currentOffset);
                if (currentOffset >= consumer.limit)
                    throw new JsonException(consumer.lineNumber, NoEndOfObjectMessage);

                {
                    var c = consumer.text[currentOffset];
                    if (c == '}')
                    {
                        currentOffset++;
                        return false;
                    }
                }

                current.name.offset = currentOffset + 1;
                currentOffset = consumer.ConsumeString(currentOffset);
                current.name.length = currentOffset - 1 - current.name.offset;

                currentOffset = consumer.ConsumeChar((Byte)':', currentOffset);
                currentOffset = consumer.ConsumeWhitespace(currentOffset);
                current.valueOffset = currentOffset;
                currentOffset = consumer.ConsumeValue(currentOffset);
                currentOffset = consumer.ConsumeWhitespace(currentOffset);
                if (currentOffset >= consumer.limit)
                    throw new JsonException(consumer.lineNumber, NoEndOfObjectMessage);

                if (consumer.text[currentOffset] == ',')
                {
                    currentOffset++;
                    if (currentOffset >= consumer.limit)
                        throw new JsonException(consumer.lineNumber, NoEndOfObjectMessage);
                }
                return true;
            }
        }
        // Check member.valueOffset == 0 to find out if the object is ended
        public UInt32 ConsumeMember(out NameValueOffsets member, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoEndOfObjectMessage);

            {
                var c = text[offset];
                if (c == '}')
                {
                    member = default(NameValueOffsets);
                    return offset + 1;
                }
            }

            member.name.offset = offset + 1;
            offset = ConsumeString(offset);
            member.name.length = offset - 1 - member.name.offset;

            offset = ConsumeChar((Byte)':', offset);
            offset = ConsumeWhitespace(offset);
            member.valueOffset = offset;
            offset = ConsumeValue(offset);
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoEndOfObjectMessage);
            if (text[offset] == ',')
            {
                offset++;
                if (offset >= limit)
                    throw new JsonException(lineNumber, NoEndOfObjectMessage);
            }
            return offset;
        }

        /*
        // Offset should point to the next character after '{' or ',' inside an object
        // Which means the first thing found should be a close brace or a key
        // Returns UInt32.Max to indicate the end of the object
        public UInt32 ToKey(UInt32 offset)
        {

        }
        */
        
        public UInt32 ParseNullableObject(out Dictionary<String,Object> @object, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, "expected object or null but reached end of input");

            if (
                offset + 3 < limit &&
                text[offset    ] == 'n' &&
                text[offset + 1] == 'u' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 'l')
            {
                @object = null;
                return offset + 4;
            }
            @object = new Dictionary<String,Object>();
            return ParseObject(@object, offset);
        }
        public UInt32 ParseObject(IDictionary<String,Object> @object, UInt32 offset)
        {
            offset = ConsumeChar((Byte)'{', offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoEndOfObjectMessage);
            offset = ConsumeWhitespace(offset);
            if (text[offset] == '}')
            {
                return offset + 1;
            }
            while (true)
            {
                String name;
                offset = ParseString(out name, offset);
                offset = ConsumeChar((Byte)':', offset);
                Object value;
                offset = ParseValue(out value, offset);
                @object.Add(name, value);
                offset = ConsumeWhitespace(offset);
                if (text[offset] == '}')
                {
                    return offset + 1;
                }
                offset = ConsumeChar((Byte)',', offset);
            }
        }
        /*
        // offset points to the first character of the value or a character before the value
        // returns the offset after the parsed value
        public UInt32 ParseObject<T>(out T value, UInt32 offset)
        {
            offset = NextNonWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, "reached end of json before a value was found");

            if(value == typeof(String))
                return ParseString(out value, offset);

            var c = text[offset];
            if (c == '"')
            {

            }
                return ConsumeString(offset);
            if (c == '{')
                return ConsumeObject(offset);
            if (c == '[')
                return ConsumeArray(offset);

            if (c == 'n' &&
                offset + 3 < limit &&
                text[offset + 1] == 'u' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 'l')
                return offset + 4;

            if (c == 't' &&
                offset + 3 < limit &&
                text[offset + 1] == 'r' &&
                text[offset + 2] == 'u' &&
                text[offset + 3] == 'e')
                return offset + 4;

            if (c == 'f' &&
                offset + 4 < limit &&
                text[offset + 1] == 'a' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 's' &&
                text[offset + 4] == 'e')
                return offset + 5;

            if (c == '-' || (c >= '0' && c <= '9'))
            {
                do
                {
                    offset++;
                    if (offset >= limit)
                        return offset;
                    c = text[offset];
                } while ((c >= '0' && c <= '9') ||
                        c == '.' ||
                        c == 'e' ||
                        c == 'E' ||
                        c == '+' ||
                        c == '-');
                return offset;
            }

            throw new JsonException(lineNumber, String.Format(
                "expected value but got '{0}' (0x{1:X})", (Char)c, (Byte)c));
        }
        */
    }


#endif



#if COMMENT


    public struct JsonConsumer2
    {
        public readonly Utf8PointerLengthSlice text;
        public UInt32 lineNumber;

        public JsonConsumer2(Utf8PointerLengthSlice text)
        {
            this.text = text;
            this.lineNumber = 1;
        }
        /// <summary>
        /// Returns an offset to the next non-whitespace character at or beyond given offset.
        /// If no non-whitespace is found before the json limit, it will return the limit.
        /// Because of this, make sure to check if (offset >= limit) before using the offset.
        /// </summary>
        /// <param name="offset">The offset to begin the search for the non-whitespace character.</param>
        /// <returns>The offset to the next non-whitespace character or the limit if no such character is found.</returns>
        public UInt32 ConsumeWhitespace(UInt32 offset)
        {
            while (true)
            {
                if (offset >= text.length)
                    return offset;

                var c = text[offset];
                if (c == '\n')
                {
                    lineNumber++;
                }
                else if (c != ' ' && c != '\t' && c != '\r')
                {
                    return offset;
                }
                offset++;
            }
        }
        /// <summary>
        /// Returns an offset to the character after the given character.
        /// If another non-whitespace character is found first, or the json limit is reached before the given character, a JsonException will be thrown.
        /// </summary>
        /// <param name="consume">The character to consume.</param>
        /// <param name="offset">The offset to begin the search for the given <paramref name="consume"/> character.</param>
        /// <returns>The offset to the character after the given <paramref name="consume"/> character.</returns>
        /// <exception cref="JsonException"></exception>
        public UInt32 ConsumeChar(Byte consume, UInt32 offset)
        {
            while (true)
            {
                if (offset >= text.length)
                    throw new JsonException(lineNumber, String.Format("expected '{0}' but reached end of input", (Char)consume));
                var c = text[offset];
                if (c == consume)
                    return offset + 1;
                if (c == '\n')
                {
                    lineNumber++;
                }
                else if (c != ' ' && c != '\t' && c != '\r')
                {
                    throw new JsonException(lineNumber, String.Format(
                        "expected '{0}' but got '{1}' (0x{2:X})", (Char)consume, (Char)c, (Byte)c));
                }
                offset++;
            }
        }
        /// <summary>
        /// Returns an offset to the character after the next json string.
        /// If the next json element is not a valid string, or the json limit is reached before a valid string, a JsonException will be thrown.
        /// </summary>
        /// <param name="offset">The offset to begin the search for a valid json string.</param>
        /// <returns>The offset to the character after the next valid json string.</returns>
        /// <exception cref="JsonException"></exception>
        public UInt32 ConsumeString(UInt32 offset)
        {
            offset = ConsumeChar((Byte)'"', offset);

            while (true)
            {
                if (offset >= text.length)
                    throw new JsonException(lineNumber, "expected end of string \" but reached end of json");
                var c = text[offset];
                if (c == (Byte)'"')
                    return offset + 1;
                if (c == (Byte)'\\')
                {
                    offset++;
                    if (offset >= text.length)
                        throw new JsonException(lineNumber, "expected end of string \" but reached end of json");
                    c = text[offset];
                    if (c == (Byte)'u')
                    {
                        offset += 5;
                    }
                    else
                    {
                        offset++;
                    }
                }
                else
                {
                    offset++;
                }
            }
        }
        /// <summary>
        /// Parses the next string (or 'null') and returns an offset to the character after it.
        /// If the next json element is not a valid string, or the json limit is reached before a valid string, a JsonException will be thrown.
        /// This function handles UTF8 encoded json data correctly.
        /// </summary>
        /// <param name="value">Used to return the parsed string.</param>
        /// <param name="offset">The offset to begin the search for a valid json string.</param>
        /// <returns>The offset to the character after the next valid json string.</returns>
        /// <exception cref="JsonException"></exception>
        public UInt32 ParseNullableString(out String value, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, "expected string or null but reached end of input");

            if (
                offset + 3 < text.length &&
                text[offset] == 'n' &&
                text[offset + 1] == 'u' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 'l')
            {
                value = null;
                return offset + 4;
            }
            return ParseString(out value, offset);
        }
        /// <summary>
        /// Parses the next string and returns an offset to the character after it.
        /// If the next json element is not a valid string, or the json limit is reached before a valid string, a JsonException will be thrown.
        /// This function handles UTF8 encoded json data correctly.
        /// </summary>
        /// <param name="value">Used to return the parsed string.</param>
        /// <param name="offset">The offset to begin the search for a valid json string.</param>
        /// <returns>The offset to the character after the next valid json string.</returns>
        /// <exception cref="JsonException"></exception>
        public UInt32 ParseString(out String value, UInt32 offset)
        {
            offset = ConsumeChar((Byte)'"', offset);

            UInt32 startOfString = offset;
            UInt32 removeEscapeChars = 0;
            while (true)
            {
                if (offset >= text.length)
                    throw new JsonException(lineNumber, "expected end of string \" but reached end of json");
                var c = text[offset];
                if (c == (Byte)'"')
                    break;
                if (c == (Byte)'\\')
                {
                    offset++;
                    if (offset >= text.length)
                        throw new JsonException(lineNumber, "expected end of string \" but reached end of json");
                    c = text[offset];
                    if (c == (Byte)'u')
                    {
                        Char unicodeChar = (Char)(
                            (((Char)text[offset + 1]).HexValue() << 12) |
                            (((Char)text[offset + 2]).HexValue() << 8) |
                            (((Char)text[offset + 3]).HexValue() << 4) |
                            (((Char)text[offset + 4]).HexValue()));
                        offset += 5;
                        removeEscapeChars += (6U - Utf8.GetCharEncodeLength(unicodeChar));
                    }
                    else
                    {
                        offset++;
                        removeEscapeChars++;
                    }
                }
                else
                {
                    offset++;
                }
            }

            if (removeEscapeChars == 0)
            {
                Utf8.DecodeToString(
                Encoder.Utf8.Encode
                value = Encoding.UTF8.GetString(text.ptr.ptr, (Int32)startOfString, (Int32)(offset - startOfString));
            }
            else
            {
                Byte[] utf8Buffer = new Byte[offset - startOfString - removeEscapeChars];
                UInt32 utf8Offset = 0;
                offset = startOfString;
                while (utf8Offset < utf8Buffer.Length)
                {
                    var c = text[offset];
                    if (c == (Byte)'\\')
                    {
                        offset++;
                        c = text[offset];
                        if (c == (Byte)'u')
                        {
                            utf8Offset = Utf8.EncodeChar((Char)(
                                    (((Char)text[offset + 1]).HexValue() << 12) |
                                    (((Char)text[offset + 2]).HexValue() << 8) |
                                    (((Char)text[offset + 3]).HexValue() << 4) |
                                    (((Char)text[offset + 4]).HexValue())), utf8Buffer, utf8Offset);
                            offset += 5;
                        }
                        else
                        {
                            switch (c)
                            {
                                case (Byte)'"': utf8Buffer[utf8Offset] = (Byte)'"'; break;
                                case (Byte)'\\': utf8Buffer[utf8Offset] = (Byte)'\\'; break;
                                case (Byte)'/': utf8Buffer[utf8Offset] = (Byte)'/'; break;
                                case (Byte)'b': utf8Buffer[utf8Offset] = (Byte)'\b'; break;
                                case (Byte)'f': utf8Buffer[utf8Offset] = (Byte)'\f'; break;
                                case (Byte)'n': utf8Buffer[utf8Offset] = (Byte)'\n'; break;
                                case (Byte)'r': utf8Buffer[utf8Offset] = (Byte)'\r'; break;
                                case (Byte)'t': utf8Buffer[utf8Offset] = (Byte)'\t'; break;
                                default:
                                    throw new JsonException(lineNumber, String.Format("Unrecognized escape \\{0}", c));
                            }
                            utf8Offset++;
                            offset++;
                        }
                    }
                    else
                    {
                        utf8Buffer[utf8Offset] = (Byte)c;
                        utf8Offset++;
                        offset++;
                    }
                }

                Debug.Assert(text[offset] == '"');
                value = Encoding.UTF8.GetString(utf8Buffer, 0, utf8Buffer.Length);
            }

            return offset + 1;
        }
        // Returns a string of the next value located at offset
        // offset should point to some type of character
        // that was unexpected.  It cannot be
        // 1. Whitespace
        public String NextValueForError(UInt32 offset)
        {
            var start = offset;
            while (true)
            {
                offset++;
                if (offset >= text.length)
                    break;
                var c = text[offset];
                if (c == '"' || c == ',' ||
                    c == ' ' || c == '\t' || c == '\n' || c == '\r' ||
                    c == '{' || c == ']' || c == '}' || c == '[')
                    break;
            }
            return Encoding.UTF8.GetString(text, (Int32)start, (Int32)(offset - start));
        }
        /// <summary>
        /// Consumes the next json value. A JsonException will be thrown if the end of json is found or the value is invalid.
        /// </summary>
        /// <param name="offset">The offset to begin the search for a valid json value.</param>
        /// <returns>The offset to the character after the next valid json value.</returns>
        public UInt32 ConsumeValue(UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoValueMessage);

            var c = text[offset];
            if (c == '"')
                return ConsumeString(offset);
            if (c == '{')
                return ConsumeObject(offset);
            if (c == '[')
                return ConsumeArray(offset);

            if (c == 'n' &&
                offset + 3 < text.length &&
                text[offset + 1] == 'u' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 'l')
                return offset + 4;

            if (c == 't' &&
                offset + 3 < text.length &&
                text[offset + 1] == 'r' &&
                text[offset + 2] == 'u' &&
                text[offset + 3] == 'e')
                return offset + 4;

            if (c == 'f' &&
                offset + 4 < text.length &&
                text[offset + 1] == 'a' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 's' &&
                text[offset + 4] == 'e')
                return offset + 5;

            if (c == '-' || (c >= '0' && c <= '9'))
            {
                do
                {
                    offset++;
                    if (offset >= text.length)
                        return offset;
                    c = text[offset];
                } while ((c >= '0' && c <= '9') ||
                        c == '.' ||
                        c == 'e' ||
                        c == 'E' ||
                        c == '+' ||
                        c == '-');
                return offset;
            }

            throw new JsonException(lineNumber, String.Format(
                "expected value but got '{0}' (0x{1:X})", (Char)c, (Byte)c));
        }
        public UInt32 ParseValue(out Object value, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoValueMessage);

            var c = text[offset];
            if (c == '"')
            {
                String valueString;
                offset = ParseString(out valueString, offset);
                value = valueString;
                return offset;
            }
            if (c == '{')
            {
                Dictionary<String, Object> valueDictionary = new Dictionary<String, Object>();
                offset = ParseObject(valueDictionary, offset);
                value = valueDictionary;
                return offset;
            }
            if (c == '[')
            {
                List<Object> array = new List<Object>();
                offset = ParseArray(array, offset);
                value = array.ToArray();
                return offset;
            }
            if (c == 'n' &&
                offset + 3 < text.length &&
                text[offset + 1] == 'u' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 'l')
            {
                value = null;
                return offset + 4;
            }
            if (c == 't' &&
                offset + 3 < text.length &&
                text[offset + 1] == 'r' &&
                text[offset + 2] == 'u' &&
                text[offset + 3] == 'e')
            {
                value = true;
                return offset + 4;
            }
            if (c == 'f' &&
                offset + 4 < text.length &&
                text[offset + 1] == 'a' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 's' &&
                text[offset + 4] == 'e')
            {
                value = false;
                return offset + 5;
            }
            if (c == '-')
            {
                Int32 valueInt32;
                offset = Parse<Int32>(out valueInt32, ByteArray.ParseInt32, offset);
                value = valueInt32;
                return offset;
            }
            if (c >= '0' && c <= '9')
            {
                UInt32 valueUInt32;
                offset = Parse<UInt32>(out valueUInt32, ByteArray.ParseUInt32, offset);
                value = valueUInt32;
                return offset;
            }
            throw new JsonException(lineNumber, String.Format(
                "expected value but got '{0}'", NextValueForError(offset)));
        }

        public const String NoValueMessage = "reached end of json before value";
        public const String EndedInsideValueMessage = "reached end of json while inside a value";
        public const String NoEndOfArrayMessage = "reached end of json inside an array";
        public const String NoEndOfObjectMessage = "reached end of json before inside an object";
        public UInt32 Parse<T>(out T value, Parser<T> parser, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoValueMessage);
            UInt32 startOfValue = offset;
            offset = ConsumeValue(offset);
            var valueString = Encoding.UTF8.GetString(text, (Int32)startOfValue, (Int32)(offset - startOfValue));
            try
            {
                value = parser(valueString);
                return offset;
            }
            catch (Exception e)
            {
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as type {1}: {2}", valueString, typeof(T).Name, e.Message));
            }
        }
        public UInt32 Parse<T>(out T value, ByteArrayParser<T> parser, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoValueMessage);
            UInt32 startOfValue = offset;
            offset = ConsumeValue(offset);

            UInt32 parserOffset;
            try
            {
                parserOffset = parser(this.text, startOfValue, offset, out value);
            }
            catch (Exception e)
            {
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as {1}: {2}",
                    Encoding.UTF8.GetString(text, (int)startOfValue, (int)(offset - startOfValue)), typeof(T).Name, e.Message), e);
            }
            if (parserOffset == 0)
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as {1}",
                    Encoding.UTF8.GetString(text, (int)startOfValue, (int)(offset - startOfValue)), typeof(T).Name));
            if (parserOffset != offset)
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as {1}, there were {2} characters but only {3} were valid",
                    Encoding.UTF8.GetString(text, (int)startOfValue, (int)(offset - startOfValue)), typeof(T).Name,
                    offset - startOfValue, parserOffset - startOfValue));
            return offset;
        }
        public UInt32 ParseWithOptionalQuotes<T>(out T value, Parser<T> parser, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoValueMessage);
            UInt32 startOfValue = offset;
            offset = ConsumeValue(offset);

            String valueString;
            if (text[startOfValue] == '"')
            {
                startOfValue++;
                valueString = Encoding.UTF8.GetString(text, (Int32)startOfValue, (Int32)(offset - startOfValue - 1));
            }
            else
            {
                valueString = Encoding.UTF8.GetString(text, (Int32)startOfValue, (Int32)(offset - startOfValue));
            }
            try
            {
                value = parser(valueString);
                return offset;
            }
            catch (Exception e)
            {
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as type {1}: {2}", valueString, typeof(T).Name, e.Message));
            }
        }
        public UInt32 ParseWithOptionalQuotes<T>(out T value, ByteArrayParser<T> parser, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoValueMessage);
            UInt32 startOfValue = offset;
            offset = ConsumeValue(offset);

            UInt32 valueLimit;
            if (text[startOfValue] == '"')
            {
                startOfValue++;
                valueLimit = offset - 1;
            }
            else
            {
                valueLimit = offset;
            }
            UInt32 parserOffset;
            try
            {
                parserOffset = parser(text, startOfValue, valueLimit, out value);
            }
            catch (Exception e)
            {
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as {1}: {2}",
                    Encoding.UTF8.GetString(text, (int)startOfValue, (int)(valueLimit - startOfValue)), typeof(T).Name, e.Message));
            }
            if (parserOffset == 0)
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as {1}",
                    Encoding.UTF8.GetString(text, (int)startOfValue, (int)(valueLimit - startOfValue)), typeof(T).Name));
            if (parserOffset != valueLimit)
                throw new JsonException(lineNumber, String.Format("Failed to parse '{0}' as {1}, there were {2} characters but only {3} were valid",
                    Encoding.UTF8.GetString(text, (int)startOfValue, (int)(valueLimit - startOfValue)), typeof(T).Name,
                    valueLimit - startOfValue, parserOffset - startOfValue));
            return offset;
        }
        public UInt32 ParseBoolean(out Boolean value, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoValueMessage);
            Byte c = text[offset];
            if (c == 't' &&
                offset + 3 < text.length &&
                text[offset + 1] == 'r' &&
                text[offset + 2] == 'u' &&
                text[offset + 3] == 'e')
            {
                value = true;
                return offset + 4;
            }

            if (c == 'f' &&
                offset + 4 < text.length &&
                text[offset + 1] == 'a' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 's' &&
                text[offset + 4] == 'e')
            {
                value = false;
                return offset + 5;
            }
            throw new JsonException(lineNumber, String.Format(
                "expected boolean 'true' or 'false' but got '{0}'", NextValueForError(offset - 1)));
        }
        public UInt32 ParseBooleanWithOptionalQuotes(out Boolean value, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoValueMessage);

            if (text[offset] == '"')
            {
                offset++;
                if (offset >= text.length)
                    throw new JsonException(lineNumber, EndedInsideValueMessage);
                offset = ParseBoolean(out value, offset);
                if (offset >= text.length)
                    throw new JsonException(lineNumber, EndedInsideValueMessage);
                if (text[offset] != '"')
                    throw new JsonException(lineNumber, String.Format(
                        "expected '\"' after \"{0} but got '{1}' (0x{2:X})", value, (Char)text[offset], (Byte)text[offset]));
                return offset + 1;
            }
            else
            {
                return ParseBoolean(out value, offset);
            }
        }
        /*
        public UInt32 ParseInt32(out Int32 value, UInt32 offset)
        {
            return Parse<Int32>(out value, ByteArray.ParseInt32, offset);
        }
        public UInt32 ParseUInt32(out UInt32 value, UInt32 offset)
        {
            return Parse<UInt32>(out value, ByteArray.ParseUInt32, offset);
        }
        */
        /*
        public UInt32 ParseDouble(out Double value, UInt32 offset)
        {
            offset = NextNonWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, NoValueMessage);
            UInt32 startOfNumber = offset;
            Byte c;
            do
            {
                offset++;
                if (offset >= limit)
                    break;
                c = text[offset];
            } while ((c >= '0' && c <= '9') || c == '.' || c == 'e' ||
                    c == 'E' || c == '+' || c == '-');
            value = Double.Parse(Encoding.UTF8.GetString(text, (Int32)startOfNumber, (Int32)(offset - startOfNumber)));
            return offset;
        }
        */
        // Returns index after closing bracket ']'
        public UInt32 ConsumeArray(UInt32 offset)
        {
            offset = ConsumeChar((Byte)'[', offset);

            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoEndOfArrayMessage);

            {
                var c = text[offset];
                if (c == ']')
                    return offset + 1;
            }

            while (true)
            {
                offset = ConsumeValue(offset);

                offset = ConsumeWhitespace(offset);
                if (offset >= text.length)
                    throw new JsonException(lineNumber, NoEndOfArrayMessage);

                var c = text[offset];
                if (c == ',')
                {
                    offset++;
                    if (offset >= text.length)
                        throw new JsonException(lineNumber, NoEndOfArrayMessage);
                }
                else if (c == ']')
                {
                    return offset + 1;
                }
                else
                {
                    throw new JsonException(lineNumber, String.Format(
                        "expected ']' or ',' but got '{0}' ({1:X})", (Char)c, (Byte)c));
                }
            }
        }
        public UInt32 ParseArray(IList<Object> array, UInt32 offset)
        {
            offset = ConsumeChar((Byte)'[', offset);

            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoEndOfArrayMessage);

            {
                var c = text[offset];
                if (c == ']')
                    return offset + 1;
            }

            while (true)
            {
                Object value;
                offset = ParseValue(out value, offset);
                array.Add(value);

                offset = ConsumeWhitespace(offset);
                if (offset >= text.length)
                    throw new JsonException(lineNumber, NoEndOfArrayMessage);

                var c = text[offset];
                if (c == ']')
                    return offset + 1;
                offset = ConsumeChar((Byte)',', offset);
            }
        }
        // Returns index after closing brace '}'
        public UInt32 ConsumeObject(UInt32 offset)
        {
            offset = ConsumeChar((Byte)'{', offset);

            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoEndOfObjectMessage);

            {
                var c = text[offset];
                if (c == '}')
                    return offset + 1;
            }

            while (true)
            {
                offset = ConsumeString(offset);
                offset = ConsumeChar((Byte)':', offset);
                offset = ConsumeValue(offset);

                offset = ConsumeWhitespace(offset);
                if (offset >= text.length)
                    throw new JsonException(lineNumber, NoEndOfObjectMessage);

                var c = text[offset];
                if (c == ',')
                {
                    offset++;
                    if (offset >= text.length)
                        throw new JsonException(lineNumber, NoEndOfObjectMessage);
                }
                else if (c == '}')
                {
                    return offset + 1;
                }
                else
                {
                    throw new JsonException(lineNumber, String.Format(
                        "expected '}' or ',' but got '{0}' ({1:X})", (Char)c, (Byte)c));
                }
            }
        }
        // Expects offset to point somewhere after the open brace or a comma
        // If end of object occurs...returns the index of the closing brace.
        // Returns the index of the value of the given key
        // Note: Guarantees return offset < limit
        public UInt32 ToObjectValue(UInt32 offset, String key)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoEndOfObjectMessage);

            {
                var c = text[offset];
                if (c == '}')
                    return offset; // no value found
            }

            while (true)
            {
                // Offset points to the starting quote
                {
                    var textStringOffset = offset + 1;
                    offset = ConsumeString(offset);
                    if (key.Length + 1 == offset - textStringOffset)
                    {
                        Boolean foundKey = true;
                        for (int i = 0; i < key.Length; i++)
                        {
                            if (text[textStringOffset] != key[i])
                            {
                                foundKey = false;
                                break;
                            }
                            textStringOffset++;
                        }

                        if (foundKey)
                        {
                            offset = ConsumeChar((Byte)':', offset);
                            offset = ConsumeWhitespace(offset);
                            if (offset >= text.length)
                                throw new JsonException(lineNumber, NoEndOfObjectMessage);
                            return offset;
                        }
                    }
                }

                offset = ConsumeChar((Byte)':', offset);
                offset = ConsumeValue(offset);

                offset = ConsumeWhitespace(offset);
                if (offset >= text.length)
                    throw new JsonException(lineNumber, NoEndOfObjectMessage);

                var c = text[offset];
                if (c == ',')
                {
                    offset++;
                    if (offset >= text.length)
                        throw new JsonException(lineNumber, NoEndOfObjectMessage);
                }
                else if (c == '}')
                {
                    return offset;
                }
                else
                {
                    throw new JsonException(lineNumber, String.Format(
                        "expected '}' or ',' but got '{0}' ({1:X})", (Char)c, (Byte)c));
                }

                // Go to next char
                offset = ConsumeWhitespace(offset);
                if (offset >= text.length)
                    throw new JsonException(lineNumber, NoEndOfObjectMessage);
            }
        }
        // Expects offset to point somewhere after the open brace or a comma
        // If end of object occurs...returns the index of the closing brace.
        // Returns the index of character after the open brace
        public UInt32 ConsumeRestOfObject(UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoEndOfObjectMessage);

            {
                var c = text[offset];
                if (c == '}')
                    return offset + 1;
            }

            while (true)
            {
                offset = ConsumeString(offset);
                offset = ConsumeChar((Byte)':', offset);
                offset = ConsumeValue(offset);
                offset = ConsumeWhitespace(offset);
                if (offset >= text.length)
                    throw new JsonException(lineNumber, NoEndOfObjectMessage);

                var c = text[offset];
                if (c == ',')
                {
                    offset++;
                    if (offset >= text.length)
                        throw new JsonException(lineNumber, NoEndOfObjectMessage);
                }
                else if (c == '}')
                {
                    return offset + 1;
                }
                else
                {
                    throw new JsonException(lineNumber, String.Format(
                        "expected '}' or ',' but got '{0}' ({1:X})", (Char)c, (Byte)c));
                }
            }
        }

        public UInt32 Lookup(IEnumerable<String> path, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= text.length || text[offset] != '{')
                throw new JsonException(lineNumber, String.Format("Failed to find path '{0}'", BuildPath(path)));
            offset++;

            offset = Lookup(path.GetEnumerator(), offset);
            if (offset == UInt32.MaxValue)
                throw new JsonException(lineNumber, String.Format("Failed to find path '{0}'", BuildPath(path)));
            return offset;
        }
        // returns UInt32.MaxValue if not found
        UInt32 Lookup(IEnumerator<String> path, UInt32 offset)
        {
            if (!path.MoveNext())
            {
                return ConsumeWhitespace(offset);
            }

            var nextPath = path.Current;
            offset = ToObjectValue(offset, nextPath);
            if (text[offset] == '}')
                return UInt32.MaxValue;
            return Lookup(path, offset);
        }
        static String BuildPath(IEnumerable<String> path)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var node in path)
            {
                if (builder.Length > 0)
                    builder.Append('.');
                builder.Append(node);
            }
            return builder.ToString();
        }
        public ObjectEnumerator ObjectMembers(UInt32 offset)
        {
            return new ObjectEnumerator(this, offset);
        }
        public struct ObjectEnumerator : IEnumerator<NameValueOffsets>
        {
            public readonly JsonConsumer consumer;
            public readonly UInt32 startOffset; // points to first char after open brace '{'

            public UInt32 currentOffset;
            NameValueOffsets current;

            // Offset should point to '{' or somewhere before the open brace
            public ObjectEnumerator(JsonConsumer consumer, UInt32 startOffset)
            {
                this.consumer = consumer;
                this.startOffset = consumer.ConsumeChar((Byte)'{', startOffset);
                this.currentOffset = startOffset;
                this.current = default(NameValueOffsets);
            }
            public void Reset()
            {
                this.currentOffset = startOffset;
            }
            public NameValueOffsets Current
            {
                get { return current; }
            }
            Object System.Collections.IEnumerator.Current
            {
                get { return current; }
            }
            public void Dispose()
            {
            }
            public Boolean MoveNext()
            {
                currentOffset = consumer.ConsumeWhitespace(currentOffset);
                if (currentOffset >= consumer.limit)
                    throw new JsonException(consumer.lineNumber, NoEndOfObjectMessage);

                {
                    var c = consumer.text[currentOffset];
                    if (c == '}')
                    {
                        currentOffset++;
                        return false;
                    }
                }

                current.name.offset = currentOffset + 1;
                currentOffset = consumer.ConsumeString(currentOffset);
                current.name.length = currentOffset - 1 - current.name.offset;

                currentOffset = consumer.ConsumeChar((Byte)':', currentOffset);
                currentOffset = consumer.ConsumeWhitespace(currentOffset);
                current.valueOffset = currentOffset;
                currentOffset = consumer.ConsumeValue(currentOffset);
                currentOffset = consumer.ConsumeWhitespace(currentOffset);
                if (currentOffset >= consumer.limit)
                    throw new JsonException(consumer.lineNumber, NoEndOfObjectMessage);

                if (consumer.text[currentOffset] == ',')
                {
                    currentOffset++;
                    if (currentOffset >= consumer.limit)
                        throw new JsonException(consumer.lineNumber, NoEndOfObjectMessage);
                }
                return true;
            }
        }
        // Check member.valueOffset == 0 to find out if the object is ended
        public UInt32 ConsumeMember(out NameValueOffsets member, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoEndOfObjectMessage);

            {
                var c = text[offset];
                if (c == '}')
                {
                    member = default(NameValueOffsets);
                    return offset + 1;
                }
            }

            member.name.offset = offset + 1;
            offset = ConsumeString(offset);
            member.name.length = offset - 1 - member.name.offset;

            offset = ConsumeChar((Byte)':', offset);
            offset = ConsumeWhitespace(offset);
            member.valueOffset = offset;
            offset = ConsumeValue(offset);
            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoEndOfObjectMessage);
            if (text[offset] == ',')
            {
                offset++;
                if (offset >= text.length)
                    throw new JsonException(lineNumber, NoEndOfObjectMessage);
            }
            return offset;
        }

        /*
        // Offset should point to the next character after '{' or ',' inside an object
        // Which means the first thing found should be a close brace or a key
        // Returns UInt32.Max to indicate the end of the object
        public UInt32 ToKey(UInt32 offset)
        {

        }
        */

        public UInt32 ParseNullableObject(out Dictionary<String, Object> @object, UInt32 offset)
        {
            offset = ConsumeWhitespace(offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, "expected object or null but reached end of input");

            if (
                offset + 3 < text.length &&
                text[offset] == 'n' &&
                text[offset + 1] == 'u' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 'l')
            {
                @object = null;
                return offset + 4;
            }
            @object = new Dictionary<String, Object>();
            return ParseObject(@object, offset);
        }
        public UInt32 ParseObject(IDictionary<String, Object> @object, UInt32 offset)
        {
            offset = ConsumeChar((Byte)'{', offset);
            if (offset >= text.length)
                throw new JsonException(lineNumber, NoEndOfObjectMessage);
            offset = ConsumeWhitespace(offset);
            if (text[offset] == '}')
            {
                return offset + 1;
            }
            while (true)
            {
                String name;
                offset = ParseString(out name, offset);
                offset = ConsumeChar((Byte)':', offset);
                Object value;
                offset = ParseValue(out value, offset);
                @object.Add(name, value);
                offset = ConsumeWhitespace(offset);
                if (text[offset] == '}')
                {
                    return offset + 1;
                }
                offset = ConsumeChar((Byte)',', offset);
            }
        }
        /*
        // offset points to the first character of the value or a character before the value
        // returns the offset after the parsed value
        public UInt32 ParseObject<T>(out T value, UInt32 offset)
        {
            offset = NextNonWhitespace(offset);
            if (offset >= limit)
                throw new JsonException(lineNumber, "reached end of json before a value was found");

            if(value == typeof(String))
                return ParseString(out value, offset);

            var c = text[offset];
            if (c == '"')
            {

            }
                return ConsumeString(offset);
            if (c == '{')
                return ConsumeObject(offset);
            if (c == '[')
                return ConsumeArray(offset);

            if (c == 'n' &&
                offset + 3 < limit &&
                text[offset + 1] == 'u' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 'l')
                return offset + 4;

            if (c == 't' &&
                offset + 3 < limit &&
                text[offset + 1] == 'r' &&
                text[offset + 2] == 'u' &&
                text[offset + 3] == 'e')
                return offset + 4;

            if (c == 'f' &&
                offset + 4 < limit &&
                text[offset + 1] == 'a' &&
                text[offset + 2] == 'l' &&
                text[offset + 3] == 's' &&
                text[offset + 4] == 'e')
                return offset + 5;

            if (c == '-' || (c >= '0' && c <= '9'))
            {
                do
                {
                    offset++;
                    if (offset >= limit)
                        return offset;
                    c = text[offset];
                } while ((c >= '0' && c <= '9') ||
                        c == '.' ||
                        c == 'e' ||
                        c == 'E' ||
                        c == '+' ||
                        c == '-');
                return offset;
            }

            throw new JsonException(lineNumber, String.Format(
                "expected value but got '{0}' (0x{1:X})", (Char)c, (Byte)c));
        }
        */
    }
#endif
