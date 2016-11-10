// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.IO;
using System.Text;

namespace More.Net
{
    public class StringBuilderWriter : TextWriter
    {
        readonly StringBuilder builder;

        public StringBuilderWriter()
        {
            this.builder = new StringBuilder();
        }
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
        public override IFormatProvider FormatProvider
        {
            get
            {
                throw new NotSupportedException();
            }
        }
        public override string NewLine
        {
            get
            {
                return Environment.NewLine;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        public override void Close() { }
        protected override void Dispose(bool disposing) { }
        public override void Flush() { }
        public override void Write(bool value) { builder.Append(value); }
        public override void Write(char value) { builder.Append(value); }
        public override void Write(char[] buffer) { builder.Append(buffer); }
        public override void Write(decimal value) { builder.Append(value); }
        public override void Write(double value) { builder.Append(value); }
        public override void Write(float value) { builder.Append(value); }
        public override void Write(int value) { builder.Append(value); }
        public override void Write(long value) { builder.Append(value); }
        public override void Write(object value) { builder.Append(value); }
        public override void Write(string value) { builder.Append(value); }
        public override void Write(uint value) { builder.Append(value); }
        public override void Write(ulong value) { builder.Append(value); }
        public override void Write(string format, object obj)
        { builder.AppendFormat(format, obj); }
        public override void Write(string format, params object[] obj)
        { builder.AppendFormat(format, obj); }
        public override void Write(char[] buffer, int index, int count)
        { builder.Append(buffer, index, count); }
        public override void Write(string format, object obj1, object obj2)
        { builder.AppendFormat(format, obj1, obj2); }
#if !WindowsCE
        public override void Write(string format, object obj1, object obj2, object obj3)
        { builder.AppendFormat(format, obj1, obj2, obj3); }
#endif
        public override void WriteLine() { builder.AppendLine(); }
        public override void WriteLine(bool value) { builder.AppendLine(value.ToString()); }
        public override void WriteLine(char value) { builder.AppendLine(value.ToString()); }
        public override void WriteLine(char[] buffer) { builder.AppendLine(new String(buffer)); }
        public override void WriteLine(decimal value) { builder.AppendLine(value.ToString()); }
        public override void WriteLine(double value) { builder.AppendLine(value.ToString()); }
        public override void WriteLine(float value) { builder.AppendLine(value.ToString()); }
        public override void WriteLine(int value) { builder.AppendLine(value.ToString()); }
        public override void WriteLine(long value) { builder.AppendLine(value.ToString()); }
        public override void WriteLine(object value) { builder.AppendLine(value.ToString()); }
        public override void WriteLine(string value) { builder.AppendLine(value); }
        public override void WriteLine(uint value) { builder.AppendLine(value.ToString()); }
        public override void WriteLine(ulong value) { builder.AppendLine(value.ToString()); }

        public override void WriteLine(string format, object obj)
        { builder.AppendLine(String.Format(format, obj)); }
        public override void WriteLine(string format, params object[] obj)
        { builder.AppendLine(String.Format(format, obj)); }
        public override void WriteLine(char[] buffer, int index, int count)
        { builder.AppendLine(new String(buffer, index, count)); }
        public override void WriteLine(string format, object obj1, object obj2)
        { builder.AppendLine(String.Format(format, obj1, obj2)); }
#if !WindowsCE
        public override void WriteLine(string format, object obj1, object obj2, object obj3)
        { builder.AppendLine(String.Format(format, obj1, obj2, obj3)); }
#endif
    }
}
