// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.IO;

namespace More
{
    public delegate void Writer(String str, UInt32 offset, UInt32 length);
    public static class WriterFormatter
    {
        public static void Stdout(String str, UInt32 offset, UInt32 length)
        {
            if (offset == 0)
            {
                if (length == str.Length)
                {
                    Console.Out.Write(str);
                    return;
                }
                Console.Out.Write(str.Remove((Int32)length));
                return;
            }

            Console.Out.Write(str.Substring((Int32)offset, (Int32)length));
        }

        public const String Spaces = "                                ";
        public const String Tabs = "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t";

        public static void WriteLine(this Writer writer)
        {
            writer(Environment.NewLine, 0, (UInt32)Environment.NewLine.Length);
        }
        public static void Write(this Writer writer, String str)
        {
            writer(str, 0, (UInt32)str.Length);
        }
        public static void WriteLine(this Writer writer, String str)
        {
            writer(str, 0, (UInt32)str.Length);
            writer(Environment.NewLine, 0, (UInt32)Environment.NewLine.Length);
        }
        public static void Write(this Writer writer, String fmt, params Object[] obj)
        {
            String message = String.Format(fmt, obj);
            writer(message, 0, (UInt32)message.Length);
        }
        public static void WriteLine(this Writer writer, String fmt, params Object[] obj)
        {
            String message = String.Format(fmt, obj);
            writer(message, 0, (UInt32)message.Length);
            writer(Environment.NewLine, 0, (UInt32)Environment.NewLine.Length);
        }
        public static void WriteSpaces(this Writer writer, UInt32 count)
        {
            while (count > Spaces.Length)
            {
                writer.Write(Spaces);
                count -= (UInt32)Spaces.Length;
	        }
            if (count > 0)
            {
                writer(Spaces, 0, count);
            }
        }
        public static void WriteTabs(this Writer writer, UInt32 count)
        {
            while (count > Tabs.Length)
            {
                writer.Write(Tabs);
                count -= (UInt32)Tabs.Length;
            }
            if (count > 0)
            {
                writer(Tabs, 0, count);
            }
        }
    }



    public struct NamedObject
    {
        public readonly String name;
        public readonly Object value;
        public NamedObject(String name, Object value)
        {
            this.name = name;
            this.value = value;
        }
    }
    public abstract class DataWriter
    {
        public readonly Writer writer;
        public readonly Boolean compact;
        public readonly Stack<String> objectTree;
        protected UInt32 depth;

        Boolean atAttributes;

        public DataWriter(Writer writer, Boolean compact, Boolean verifyObjectNames)
        {
            this.writer = writer;
            this.compact = compact;
            this.objectTree = verifyObjectNames ? new Stack<String>() : null;
            this.depth = 0;
            this.atAttributes = false;
        }
        public void WriteComment(String comment)
        {
            if (atAttributes)
            {
                WriteEndOfAttributes();
                atAttributes = false;
            }
            WriteCommentImpl(comment);
        }
        public void WriteObjectStart(String objectName, params NamedObject[] attributes)
        {
            if (atAttributes)
            {
                WriteEndOfAttributes();
                atAttributes = false;
            }

            WriteObjectStartImpl(objectName);
            for (int i = 0; i < attributes.Length; i++)
            {
                WriteAttributeImpl(attributes[i].name, attributes[i].value);
            }
            if (objectTree != null) objectTree.Push(objectName);
            depth++;

            atAttributes = true;
        }
        public void WriteObjectEnd(String objectName)
        {
            if (depth <= 0) throw new InvalidOperationException(String.Format("Cannot end object '{0}' because all objects have been ended", objectName));
            if (objectTree != null)
            {
                if (objectName != objectTree.Peek())
                {
                    throw new InvalidOperationException(String.Format("Object '{0}' ended but current object is '{1}'",
                        objectName, objectTree.Peek()));
                }
                objectTree.Pop();
            }
            depth--;

            WriteObjectEndImpl(objectName, atAttributes);
            atAttributes = false;
        }
        public void WriteAttribute(NamedObject attribute)
        {
            WriteAttribute(attribute.name, attribute.value);
        }
        public void WriteAttribute(String name, Object value)
        {
            if (!atAttributes)
                throw new InvalidOperationException("Tried to add an attribute after a child object was already added");

            WriteAttributeImpl(name, value);
        }

        public void WriteObject(NamedObject obj, params NamedObject[] attributes)
        {
            if (atAttributes)
            {
                WriteEndOfAttributes();
                atAttributes = false;
            }
            WriteObjectFieldImpl(obj.name, attributes, obj.value);
        }
        public void WriteObject(String name, Object value, params NamedObject[] attributes)
        {
            if (atAttributes)
            {
                WriteEndOfAttributes();
                atAttributes = false;
            }
            WriteObjectFieldImpl(name, attributes, value);
        }

        protected abstract void WriteCommentImpl(String comment);
        protected abstract void WriteObjectStartImpl(String objectName);
        protected abstract void WriteObjectEndImpl(String objectName, Boolean insideAttributes);
        protected abstract void WriteObjectFieldImpl(String name, NamedObject[] attributes, Object value);
        protected abstract void WriteAttributeImpl(String name, Object value);
        protected abstract void WriteEndOfAttributes();
    }
    public class XmlWriter : DataWriter
    {
        public XmlWriter(Writer writer, Boolean compact, Boolean verifyObjectNames)
            : base(writer, compact, verifyObjectNames)
        {
        }
        protected override void WriteCommentImpl(String comment)
        {
            if (!compact) writer.WriteSpaces(depth * 2);
            writer.Write("<!--");
            writer.Write(comment);
            writer.WriteLine("-->");
        }
        protected override void WriteObjectStartImpl(String objectName)
        {
            if (!compact) writer.WriteSpaces(depth * 2);
            writer.Write("<");
            writer.Write(objectName);
        }
        protected override void WriteObjectEndImpl(String objectName, Boolean insideAttributes)
        {
            if (insideAttributes)
            {
                if (compact) writer.Write(" />");
                else writer.WriteLine(" />");
            }
            else
            {
                if (!compact) writer.WriteSpaces(depth * 2);
                writer.Write("</");
                writer.Write(objectName);
                if (compact) writer.Write(">"); else writer.WriteLine(">");
            }
        }
        protected override void WriteObjectFieldImpl(String name, NamedObject[] attributes, Object value)
        {
            if (!compact) writer.WriteSpaces(depth * 2);

            writer.Write("<");
            writer.Write(name);

            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    WriteAttributeImpl(attributes[i].name, attributes[i].value);
                }
            }

            String valueString = (value == null) ? null : value.ToString();
            if (String.IsNullOrEmpty(valueString))
            {
                writer.Write(" />");
            }
            else
            {
                writer.Write(">");
                writer.Write(valueString);
                writer.Write("</");
                writer.Write(name);
                writer.Write(">");
            }
            if (!compact) writer.WriteLine();
        }
        protected override void WriteAttributeImpl(String name, Object value)
        {
            writer.Write(" ");
            writer.Write(name);
            writer.Write("=\"");
            writer.Write(value.ToString());
            writer.Write('"'.ToString());
        }
        protected override void WriteEndOfAttributes()
        {
            if (compact) writer.Write(">");
            else writer.WriteLine(">");
        }
    }
    /*
    public class SdlWriter : DataWriter
    {
        readonly TextWriter writer;

        enum State
        {
            Initial,
            InsideTag,
        }
        State state;
        Stack<String> objectTree;

        public SdlWriter(TextWriter writer)
        {
            this.writer = writer;
            this.state = State.Initial;
            this.objectTree = new Stack<String>();
        }
        public void Dispose()
        {
            this.writer.Dispose();
        }

        void WriteAttributeImpl(Object name, Object value)
        {
            writer.Write(' ');
            writer.Write(name.ToString());
            writer.Write("=\"");
            writer.Write(value.ToString());
            writer.Write('"'.ToString());
        }
        public void WriteObjectStart(String objectName, params Object[] attributes)
        {
            if (state == State.InsideTag)
            {
                writer.WriteLine(" {");
            }

            writer.Write((UInt32)(objectTree.Count * 2), "{0}", objectName);
            for (int i = 0; i < attributes.Length; i += 2)
            {
                WriteAttributeImpl(attributes[i], attributes[i + 1]);
            }
            objectTree.Push(objectName);
            state = State.InsideTag;
        }
        public void WriteObjectEnd(String objectName)
        {
            if (objectName != objectTree.Peek())
            {
                throw new InvalidOperationException(String.Format("Object '{0}' ended but current object is '{1}'",
                    objectName, objectTree.Peek()));
            }
            objectTree.Pop();

            if (state == State.InsideTag)
            {
                writer.WriteLine();
                state = State.Initial;
            }
            else
            {
                writer.WriteLine((UInt32)(objectTree.Count * 2), "}");
            }
        }
        public void WriteAttribute(String name, Object value)
        {
            if (state != State.InsideTag)
                throw new InvalidOperationException("Tried to add an attribute after a child object was already added");

            WriteAttributeImpl(name, value);
        }
        public void WriteObject(String objectName, params Object[] attributes)
        {
            if (state == State.InsideTag)
            {
                writer.WriteLine(" {");
            }

            writer.Write((UInt32)(objectTree.Count * 2), "{0}", objectName);
            Int32 i;
            for (i = 0; i + 1 < attributes.Length; i += 2)
            {
                WriteAttributeImpl(attributes[i], attributes[i + 1]);
            }
            if (i < attributes.Length)
            {
                writer.Write(" {");
                writer.Write(attributes[attributes.Length - 1].ToString());
                writer.WriteLine("}");
            }
            else
            {
                writer.WriteLine();
            }
            state = State.Initial;
        }

        public void WriteObjectField(String fieldName, String value)
        {
            if (state == State.InsideTag)
            {
                writer.WriteLine(" {");
            }
            writer.WriteLine((UInt32)(objectTree.Count * 2), "{0} {1}", fieldName, value);
            state = State.Initial;
        }
        public void WriteObjectField(String fieldName, Int64 value)
        {
            if (state == State.InsideTag)
            {
                writer.WriteLine(" {");
            }
            writer.WriteLine((UInt32)(objectTree.Count * 2), "{0} {1}", fieldName, value);
            state = State.Initial;
        }
    }
    public class AsonWriter : DataWriter
    {
        readonly TextWriter writer;
        public AsonWriter(TextWriter writer)
        {
            this.writer = writer;
        }
        public void Dispose()
        {
            this.writer.Dispose();
        }
        public void WriteFileStart(String rootObjectName, params Object[] attributes)
        {
            throw new NotImplementedException();
        }
        public void WriteFileEnd(String rootObjectName)
        {
            throw new NotImplementedException();
        }
        public void WriteObjectStart(String objectName, params Object[] attributes)
        {
            throw new NotImplementedException();
        }
        public void WriteAttribute(String name, Object value)
        {
            throw new NotImplementedException();
        }
        public void WriteObject(String objectName, params Object[] attributes)
        {
            throw new NotImplementedException();
        }

        public void WriteObjectEnd(String objectName)
        {
            throw new NotImplementedException();
        }
        public void WriteObjectField(String fieldName, String value)
        {
            throw new NotImplementedException();
        }
        public void WriteObjectField(String fieldName, Int64 value)
        {
            throw new NotImplementedException();
        }
    }
    */
}
