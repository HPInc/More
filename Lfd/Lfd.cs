// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Text;

namespace More
{
    public class LfdFormatException : FormatException
    {
        public readonly UInt32 lineNumber;
        public readonly String line;
        public LfdFormatException(UInt32 lineNumber, String line, String msg)
            : base(String.Format("Line {0} \"{1}\" : {2}", lineNumber, line, msg))
        {
        }
    }
    public delegate void LfdLineConfigHandler(LfdLine line);
    public delegate LfdLine LfdConfigHandler(LfdLineReader reader, LfdLine line);
    public class LfdCallbackParser
    {
        readonly Dictionary<String, LfdLineConfigHandler> lineParsers =
            new Dictionary<String, LfdLineConfigHandler>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<String, LfdConfigHandler> readerParsers =
            new Dictionary<String, LfdConfigHandler>(StringComparer.OrdinalIgnoreCase);

        public void Add(String name, LfdLineConfigHandler handler)
        {
            lineParsers.Add(name, handler);
        }
        public void Add(String name, LfdConfigHandler handler)
        {
            readerParsers.Add(name, handler);
        }

        public LfdLine Handle(LfdLineReader reader, LfdLine parentLine)
        {
            LfdLine line = reader.ReadLineIgnoreComments();
            while (true)
            {
                if (line == null) return null;
                if (line.parent != parentLine) return line;

                LfdLineConfigHandler lineConfigHandler;
                LfdConfigHandler readerConfigHandler;
                if (lineParsers.TryGetValue(line.id, out lineConfigHandler))
                {
                    lineConfigHandler(line);
                    line = reader.ReadLineIgnoreComments();
                }
                else if (readerParsers.TryGetValue(line.id, out readerConfigHandler))
                {
                    line = readerConfigHandler(reader, line);
                }
                else
                {
                    throw new FormatException(String.Format("Unknown config name '{0}'", line.id));
                }
            }
        }
        public void Parse(LfdLineReader reader)
        {
            LfdLine line = reader.ReadLineIgnoreComments();
            while (true)
            {
                if (line == null) break;

                LfdLineConfigHandler lineConfigHandler;
                LfdConfigHandler readerConfigHandler;
                if (lineParsers.TryGetValue(line.id, out lineConfigHandler))
                {
                    lineConfigHandler(line);
                    line = reader.ReadLineIgnoreComments();
                }
                else if (readerParsers.TryGetValue(line.id, out readerConfigHandler))
                {
                    line = readerConfigHandler(reader, line);
                }
                else
                {
                    throw new FormatException(String.Format("Unknown config name '{0}'", line.id));
                }
            }
        }
    }
    public class LfdLine
    {
        public static void ParseLine(List<String> fields, Byte[] line, Int32 offset, Int32 offsetPlusLength)
        {
            while (true)
            {
                Int32 nextFieldStart = offset;

                Byte c;

                //
                // Skip whitespace
                //
                while (true)
                {
                    if (nextFieldStart >= offsetPlusLength) return;
                    c = line[nextFieldStart];
                    if (c != ' ' && c != '\t') break;
                    nextFieldStart++;
                }

                if (c != '"')
                {
                    offset = nextFieldStart + 1;
                    while (true)
                    {
                        if (offset >= offsetPlusLength)
                        {
                            fields.Add(Encoding.UTF8.GetString(line, nextFieldStart, offset - nextFieldStart));
                            return;
                        }
                        c = line[offset];
                        if (c == ' ' || c == '\t')
                        {
                            fields.Add(Encoding.UTF8.GetString(line, nextFieldStart, offset - nextFieldStart));
                            nextFieldStart = offset + 1;
                            break;
                        }
                        offset++;
                    }
                }
                else // Quoted String
                {
                    nextFieldStart++;
                    offset = nextFieldStart;
                    while (true)
                    {
                        if (offset >= offsetPlusLength)
                        {
                            throw new FormatException("string missing closing quote");
                        }
                        c = line[offset];
                        if (c == '"')
                        {
                            fields.Add(Encoding.UTF8.GetString(line, nextFieldStart, offset - nextFieldStart));
                            offset++;
                            break;
                        }
                        offset++;
                    }
                }
            }
        }
        public static void ParseLine(List<String> fields, String line, Int32 offset, Int32 length)
        {
            Int32 offsetPlusLength = offset + length;

            while (true)
            {
                Int32 nextFieldStart = offset;

                Char c;

                //
                // Skip whitespace
                //
                while (true)
                {
                    if (nextFieldStart >= offsetPlusLength) return;
                    c = line[nextFieldStart];
                    if (c != ' ' && c != '\t') break;
                    nextFieldStart++;
                }

                if (c != '"')
                {
                    offset = nextFieldStart + 1;
                    while (true)
                    {
                        if (offset >= offsetPlusLength)
                        {
                            fields.Add(line.Substring(nextFieldStart, offset - nextFieldStart));
                            return;
                        }
                        c = line[offset];
                        if (c == ' ' || c == '\t')
                        {
                            fields.Add(line.Substring(nextFieldStart, offset - nextFieldStart));
                            nextFieldStart = offset + 1;
                            break;
                        }
                        offset++;
                    }
                }
                else
                {
                    nextFieldStart++;
                    if (nextFieldStart >= offsetPlusLength)
                    {
                        fields.Add(String.Empty);
                    }

                    offset = nextFieldStart + 1;
                    while (true)
                    {
                        if (offset >= offsetPlusLength)
                        {
                            fields.Add(line.Substring(nextFieldStart, offset - nextFieldStart));
                            return;
                        }
                        c = line[offset];
                        if (c == '"')
                        {
                            fields.Add(line.Substring(nextFieldStart, offset - nextFieldStart));
                            nextFieldStart = offset + 1;
                            break;
                        }
                        offset++;
                    }
                }
            }
        }
        public readonly LfdLine parent;
        public readonly String id;
        public readonly String[] fields;
        readonly Boolean comment;

        public readonly UInt32 actualLineNumber;

        public LfdLine(LfdLine parent, String comment, UInt32 actualLineNumber)
        {
            this.parent  = parent;
            this.id      = comment;
            this.fields  = null;
            this.comment = true;

            this.actualLineNumber = actualLineNumber;
        }
        public LfdLine(LfdLine parent, String id, String[] fields, UInt32 actualLineNumber)
        {
            if (id == null) throw new ArgumentNullException("id");

            this.parent  = parent;
            this.id      = id;
            this.fields  = fields;
            this.comment = false;

            this.actualLineNumber = actualLineNumber;
        }
        public Boolean IsComment()
        {
            return comment;
        }
        public String CreateContextString()
        {
            LfdLine currentParent = parent;
            Stack<LfdLine> parents = new Stack<LfdLine>();
            while (currentParent != null)
            {
                parents.Push(currentParent);
                currentParent = currentParent.parent;
            }

            if (parents.Count <= 1) return id;

            // Pop off the root
            parents.Pop();

            StringBuilder stringBuilder = new StringBuilder();
            while (parents.Count > 0)
            {
                stringBuilder.Append(parents.Pop().id);
                stringBuilder.Append('.');
            }
            stringBuilder.Append(id);
            return stringBuilder.ToString();
        }
        public override String ToString()
        {
            if (fields == null || fields.Length <= 0) return id;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(id);
            stringBuilder.Append(' ');
            for (int i = 0; i < fields.Length - 1; i++)
            {
                stringBuilder.Append(fields[i]);
                stringBuilder.Append(' ');
            }
            stringBuilder.Append(fields[fields.Length - 1]);
            return stringBuilder.ToString();
        }
    }
}