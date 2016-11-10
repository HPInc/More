// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Text;

namespace More
{
    public abstract class SNode
    {
        public abstract void ToSymExp(StringBuilder builder);
    }
    public class SNumber : SNode
    {
        public readonly String numberString;
        public SNumber(String numberString)
        {
            this.numberString = numberString;
        }
        public override void ToSymExp(StringBuilder builder)
        {
            builder.Append(numberString);
        }
        public override String ToString()
        {
            return numberString;
        }
    }
    public class SSymbol : SNode
    {
        public readonly String symbol;
        public SSymbol(String symbol)
        {
            this.symbol = symbol;
        }
        public override void ToSymExp(StringBuilder builder)
        {
            builder.Append(symbol);
        }
        public override String ToString()
        {
            return symbol;
        }
    }
    public class SList : SNode
    {
        public readonly List<SNode> children;

        public SList(List<SNode> children)
        {
            this.children = children;
        }
        public override void ToSymExp(StringBuilder builder)
        {
            builder.Append('(');
            for (int i = 0; i < children.Count; i++)
            {
                if(i > 0) builder.Append(' ');
                children[i].ToSymExp(builder);
            }
            builder.Append(')');
        }
    }
    public static class SymExp
    {
        static Boolean IsSymbolCharacter(Char c)
        {
            return  (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9') ||
                    (c == '.') ||
                    (c == '_') ||
                    (c == '-') ;
        }

        public static SNode Read(String symExp)
        {
            SNode node;
            Read(symExp, 0, out node);
            return node;
        }
        public static SNode Read(String symExp, Int32 offset)
        {
            SNode node;
            Read(symExp, offset, out node);
            return node;
        }
        public static Int32 Read(String symExp, Int32 offset, out SNode node)
        {
            //
            // Skip whitespace
            //
            Char c = symExp[offset];
            while (Char.IsWhiteSpace(c))
            {
                offset++;
                if (offset >= symExp.Length) throw new InvalidOperationException("Expected symbolic expression but was empty");
                c = symExp[offset];
            }

            //
            // Check first character
            //
            if (c == '(')
            {
                SList list;
                offset = ReadList(symExp, offset, out list);
                node = list;
                return offset;
            }
            else if (c >= '0' && c <= '9')
            {
                SNumber number;
                offset = ReadNumber(symExp, offset, out number);
                node = number;
                return offset;
            }
            else if (IsSymbolCharacter(c))
            {
                SSymbol symbol;
                offset = ReadSSymbol(symExp, offset, out symbol);
                node = symbol;
                return offset;
            }
            else
            {
                throw new FormatException(String.Format("Unhandled character '{0}' (charcode={1})",
                    c, (Int32)c));
            }
        }

        // offset is pointing at the first digit
        static Int32 ReadNumber(String symExp, Int32 offset, out SNumber number)
        {
            Int32 startOffset = offset;

            while (true)
            {
                offset++;
                if (offset >= symExp.Length)
                {
                    number = new SNumber(symExp.Substring(startOffset));
                    return offset;
                }

                Char c = symExp[offset];
                if ((c < '0' || c > '9') && (c != '.'))
                {
                    number = new SNumber(symExp.Substring(startOffset, offset - startOffset));
                    return offset;
                }
            }

        }

        // offset is pointing at the first digit
        static Int32 ReadSSymbol(String symExp, Int32 offset, out SSymbol symbol)
        {
            Int32 startOffset = offset;

            while (true)
            {
                offset++;
                if(offset >= symExp.Length)
                {
                    symbol = new SSymbol(symExp.Substring(startOffset));
                    return offset;
                }

                Char c = symExp[offset];
                if (!IsSymbolCharacter(c))
                {
                    symbol = new SSymbol(symExp.Substring(startOffset, offset - startOffset));
                    return offset;
                }
            }
        }


        // offset is pointing at the opening '('
        static Int32 ReadList(String symExp, Int32 offset, out SList list)
        {
            offset++;
            if(offset >= symExp.Length) throw new FormatException("Missing ending ')'");


            List<SNode> nodes = new List<SNode>();
            while (true) {
                Char c = symExp[offset];

                if(c == ')')
                {
                    list = new SList(nodes);
                    return offset + 1;
                }

                SNode node;
                offset = Read(symExp, offset, out node);
                nodes.Add(node);

                if(offset >= symExp.Length) throw new FormatException("Missing ending ')'");
            }
        }
    }
}