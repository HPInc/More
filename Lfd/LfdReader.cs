// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace More
{
    public class LfdTextReader : LfdLineReader , IDisposable
    {
        readonly TextReader underlyingTextReader;
        public LfdTextReader(TextReader underlyingTextReader)
            : base(underlyingTextReader.ReadLine)
        {
            this.underlyingTextReader = underlyingTextReader;
        }
        public void Dispose()
        {
            underlyingTextReader.Dispose();
        }
    }
    public class LfdLineReader
    {
        private readonly ReadLineDelegate readLineDelegate;
        private Stack<LfdLine> context;
        private UInt32 lineNumber;

        public LfdLineReader(ReadLineDelegate readLineDelegate)
        {
            if (readLineDelegate == null) throw new ArgumentNullException("readLineDelegate");
            this.readLineDelegate = readLineDelegate;
            this.context = new Stack<LfdLine>();
            this.lineNumber = 0;
        }

        public UInt32 LineNumber { get { return lineNumber; } set { this.lineNumber = value; } }
        private FormatException FormatError(String line, String msg)
        {
            return new LfdFormatException(lineNumber, line, msg);
        }
        private FormatException FormatError(String line, String fmt, params Object[] obj)
        {
            return new LfdFormatException(lineNumber, line,String.Format(fmt, obj));
        }
        public LfdLine ReadLineIgnoreComments()
        {
            LfdLine line;
            while (true)
            {
                line = ReadLine();
                if (line == null) return null;
                if (!line.IsComment()) return line;
            }
        }
        public LfdLine ReadLine()
        {
            while (true)
            {
                String line = readLineDelegate();
                if (line == null) return null;
                lineNumber++;

                //
                // Identify line
                //
                for (int offset = 0; true; offset++)
                {
                    // Empty Line
                    if (offset >= line.Length) break;
                    if (Char.IsWhiteSpace(line[offset])) continue;

                    //
                    // If the first character is a '#' then the line is a comment
                    if (line[offset] == '#')
                    {
                        return new LfdLine((context.Count > 0) ? context.Peek() : null, line, lineNumber);
                    }

                    //
                    // If the first character is '}' then the line must be a BLOCK_END line
                    //
                    if (line[offset] == '}')
                    {
                        // Verify whitespace till end of line
                        while (true)
                        {
                            offset++;
                            if (offset >= line.Length) break;
                            if (!Char.IsWhiteSpace(line[offset])) throw FormatError(line, "Line starting with '}' must be followed by only whitespace");
                        }

                        if (context.Count <= 0) throw FormatError(line, "The block end '}' had no matching block begin");
                        context.Pop();
                        break;
                    }

                    //
                    // '{' and '"' are invalid characters to start a line
                    //
                    if (line[offset] == '{' || line[offset] == '"')
                    {
                        throw FormatError(line, "A line cannot start with '{0}'", line[offset]);
                    }
                    if (line[offset] == '\\')
                    {
                        // verify whitespace till end
                        while (true)
                        {
                            offset++;
                            if (offset >= line.Length)
                            {
                                line = readLineDelegate();
                                if (line == null) return null;
                                offset = 0;
                                lineNumber++;
                                break;
                            }
                            if (!Char.IsWhiteSpace(line[offset]))
                                throw FormatError(line, "The '\\' character cannot have non whitespace after it");
                        }
                        continue;
                    }

                    //
                    // Get the line id
                    //
                    Int32 saveOffset = offset;
                    while (true)
                    {
                        offset++;
                        if (offset >= line.Length)
                        {
                            return new LfdLine((context.Count > 0) ? context.Peek() : null,
                                line.Substring(saveOffset), null, lineNumber);
                        }
                        if (line[offset] == '{')
                        {
                            String lineIdSpecialCase = line.Substring(saveOffset, offset - saveOffset);
                            //verify whitespace till the end
                            while (true)
                            {
                                offset++;
                                if (offset >= line.Length) break;
                                if (!Char.IsWhiteSpace(line[offset])) throw FormatError(line, "The '{' character just after the line Id can only be followed by whitespace");
                            }

                            LfdLine newLine = new LfdLine((context.Count > 0) ? context.Peek() : null,
                                lineIdSpecialCase, null, lineNumber);
                            context.Push(newLine);
                            return newLine;
                        }
                        if (Char.IsWhiteSpace(line[offset])) break;
                    }

                    String lineId = line.Substring(saveOffset, offset - saveOffset);

                    // Skip whitespace until you get to the first field (or end of line)
                    while (true)
                    {
                        offset++;
                        if (offset >= line.Length)
                        {
                            return new LfdLine((context.Count > 0) ? context.Peek() : null,
                                lineId, null, lineNumber);
                        }
                        if (!Char.IsWhiteSpace(line[offset])) break;
                    }

                    //
                    // Get all fields in the line
                    //
                    List<String> fields = new List<String>();
                    while (true)
                    {
                        Int32 fieldStart = offset;

                        //
                        // A quoted field
                        //
                        if (line[offset] == '"')
                        {
                            fieldStart++;

                            Boolean continueLoop = false;
                            while (true)
                            {
                                offset++;
                                if (offset >= line.Length)
                                {
                                    throw FormatError(line, "Found a quoted field without an ending quote");
                                }
                                if (line[offset] == '"')
                                {
                                    if (line[offset - 1] != '\\')
                                    {
                                        fields.Add(line.Substring(fieldStart, offset - fieldStart));
                                        offset++;
                                        break;
                                    }
                                    else
                                    {
                                        continueLoop = true;
                                        break;
                                    }
                                }
                            }

                            //
                            // If there is an escaped quote in the string, then the rest of the characters
                            // must be added to a new string to remove any extra backslashes
                            //
                            if (continueLoop)
                            {
                                StringBuilder builder = new StringBuilder(line.Substring(fieldStart, offset - 1 - fieldStart));
                                builder.Append('"');
                                while (true)
                                {
                                    offset++;
                                    if (offset >= line.Length)
                                    {
                                        throw FormatError(line, "Found a quoted field without an ending quote");
                                    }

                                    if (line[offset] == '"')
                                    {
                                        fields.Add(builder.ToString());
                                        offset++;
                                        break;
                                    }
                                    if (line[offset] == '\\')
                                    {
                                        offset++;
                                        if (offset >= line.Length) throw FormatError(line, "Line ended with '\\' but was in the middle of a quote");
                                        if (line[offset] == '\\')
                                        {
                                            builder.Append('\\');
                                        }
                                        else if (line[offset] == '"')
                                        {
                                            builder.Append('"');
                                        }
                                        else
                                        {
                                            throw FormatError(line, "Unrecognized escape character '{0}', expected '\"' or '\\'", line[offset]);
                                        }
                                    }
                                    else
                                    {
                                        builder.Append(line[offset]);
                                    }
                                }
                            }
                        }
                        //
                        // An Open Brace field which must be the last field
                        //
                        else if (line[offset] == '{')
                        {
                            //verify whitespace till the end
                            while (true)
                            {
                                offset++;
                                if (offset >= line.Length) break;
                                if (!Char.IsWhiteSpace(line[offset])) throw FormatError(line, "A Field starting with the '{' character can only be followed by whitespace unless escaped with '\\{'");
                            }

                            LfdLine newLine = new LfdLine((context.Count > 0) ? context.Peek() : null,
                                lineId, fields.ToArray(), lineNumber);
                            context.Push(newLine);
                            return newLine;
                        }
                        //
                        // An escape character '\' used for starting fields with the '{' character
                        // and continuing the fields on the next line
                        //
                        else if (line[offset] == '\\')
                        {

                            offset++;
                            if (offset >= line.Length)
                            {
                                line = readLineDelegate();
                                if (line == null) return null;
                                offset = 0;
                                lineNumber++;
                            }
                            else
                            {
                                if (line[offset] == '{')
                                {
                                    fieldStart++;
                                    while (true)
                                    {
                                        offset++;
                                        if (offset >= line.Length)
                                        {
                                            fields.Add(line.Substring(fieldStart));
                                            return new LfdLine((context.Count > 0) ? context.Peek() : null,
                                                lineId, fields.ToArray(), lineNumber);
                                        }
                                        if (Char.IsWhiteSpace(line[offset]))
                                        {
                                            fields.Add(line.Substring(fieldStart, offset - fieldStart));
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    while (true)
                                    {
                                        offset++;
                                        if (offset >= line.Length)
                                        {
                                            line = readLineDelegate();
                                            if (line == null) return null;
                                            offset = 0;
                                            lineNumber++;
                                            break;
                                        }
                                        if (!Char.IsWhiteSpace(line[offset]))
                                            throw FormatError(line, "The '\\' character cannot have non whitespace after it");
                                    }
                                }
                            }
                        }
                        //
                        // A normal field that ends when whitespace is encountered
                        //
                        else
                        {
                            while (true)
                            {
                                offset++;
                                if (offset >= line.Length)
                                {
                                    fields.Add(line.Substring(fieldStart));
                                    return new LfdLine((context.Count > 0) ? context.Peek() : null,
                                        lineId, fields.ToArray(), lineNumber);
                                }
                                if (Char.IsWhiteSpace(line[offset]))
                                {
                                    fields.Add(line.Substring(fieldStart, offset - fieldStart));
                                    break;
                                }
                            }
                        }

                        //
                        // Skip whitespace, or return a line if the end of line is found
                        //
                        while (true)
                        {
                            if (offset >= line.Length)
                            {
                                return new LfdLine((context.Count > 0) ? context.Peek() : null,
                                    lineId, fields.ToArray(), lineNumber);
                            }
                            if (!Char.IsWhiteSpace(line[offset])) break;
                            offset++;
                        }
                    }
                }
            }
        }
    }
}
