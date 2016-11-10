// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Runtime.InteropServices;

namespace More
{
    public static class Unicode
    {
        public static unsafe void AsciiToUtf16(Byte* src, Char* dest, UInt32 length)
        {
            for (int i = 0; i < length; i++)
            {
                // TODO: should I be checking this?
                if (src[i] > 127)
                {
                    throw new FormatException("Called AsciiToUtf16 on a string with non-ascii characters");
                }
                dest[i] = (Char)src[i];
            }
        }

        public static unsafe void Utf16ToAscii(String source, Byte* dest)
        {
            for (int i = 0; i < source.Length; i++)
            {
                // TODO: should I be checking this?
                if (source[i] > 127)
                {
                    throw new FormatException("Called Utf16ToAscii on a string with non-ascii characters");
                }
                dest[i] = (Byte)source[i];
            }
        }
        public static unsafe void Utf8ToUtf16(Byte* src, Char* dest, UInt32 length)
        {
            for (int i = 0; i < length; i++)
            {
                if (src[i] > 127)
                {
                    throw new NotImplementedException("Utf8 not fully implemented yet");
                }
                dest[i] = (Char)src[i];
            }
        }
    }
    public struct Utf16Pointer
    {
        public unsafe Char* ptr;
    }
    public struct Utf16Slice
    {
        public Utf16Pointer ptr;
        public UInt32 length;
    }

    public struct CharPtr
    {
        public unsafe Char* ptr;
        public unsafe CharPtr(Char* ptr)
        {
            this.ptr = ptr;
        }
    }

    public struct BytePtr
    {
        //public static unsafe readonly BytePtr Null = new BytePtr(null);
        public unsafe void SetToNull()
        {
            this.ptr = null;
        }

        public unsafe Byte* ptr;
        public unsafe BytePtr(Byte* ptr)
        {
            this.ptr = ptr;
        }

        //
        // Implicit Cast Operators
        //
        public static unsafe implicit operator BytePtr(Byte* ptr)
        {
            return new BytePtr(ptr);
        }
        public static unsafe implicit operator BytePtr(IntPtr ptr)
        {
            return new BytePtr((Byte*)ptr.ToPointer());
        }
        public static unsafe implicit operator IntPtr(BytePtr ptr)
        {
            return new IntPtr(ptr.ptr);
        }
        public static unsafe implicit operator BytePtr(GCHandle handle)
        {
            return (IntPtr)handle;
        }
        public static unsafe implicit operator GCHandle(BytePtr ptr)
        {
            return (GCHandle)ptr;
        }
        //
        // Explicit Cast Operators
        //
        public static unsafe explicit operator UInt32(BytePtr ptr)
        {
            return (UInt32)ptr.ptr;
        }
        public static unsafe explicit operator Int32(BytePtr ptr)
        {
            return (Int32)ptr.ptr;
        }

        public static unsafe Boolean operator ==(BytePtr a, Byte* p)
        {
            return a.ptr == p;
        }
        public static unsafe Boolean operator !=(BytePtr a, Byte* p)
        {
            return a.ptr != p;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //
        // Comparison Operators
        //
        public static unsafe Boolean operator <(BytePtr a, BytePtr b)
        {
            return a.ptr < b.ptr;
        }
        public static unsafe Boolean operator >(BytePtr a, BytePtr b)
        {
            return a.ptr > b.ptr;
        }
        public static unsafe Boolean operator <=(BytePtr a, BytePtr b)
        {
            return a.ptr <= b.ptr;
        }
        public static unsafe Boolean operator >=(BytePtr a, BytePtr b)
        {
            return a.ptr >= b.ptr;
        }
        public static unsafe BytePtr operator +(BytePtr a, UInt32 b)
        {
            return a.ptr + b;
        }

        public unsafe Byte this[UInt32 i]
        {
            get
            {
                return ptr[i];
            }
        }

        public unsafe UInt32 DiffWithSmallerNumber(BytePtr smaller)
        {
            return (UInt32)(this.ptr - smaller.ptr);
        }


        public unsafe override string ToString()
        {
            return new IntPtr(ptr).ToString();
        }

        // TODO: use native memcpy
        public unsafe void CopyTo(BytePtr dest, UInt32 length)
        {
            for (uint i = 0; i < length; i++)
            {
                dest.ptr[i] = this.ptr[i];
            }
        }
    }
    public struct BufferSlice
    {
        public BytePtr ptr;
        public UInt32 length;
        public BufferSlice(BytePtr ptr, UInt32 length)
        {
            this.ptr = ptr;
            this.length = length;
        }
    }

    public struct Utf8Pointer
    {
        public static unsafe Boolean operator <(Utf8Pointer left, Utf8Pointer right)
        {
            return left.ptr < right.ptr;
        }
        public static unsafe Boolean operator >(Utf8Pointer left, Utf8Pointer right)
        {
            return left.ptr > right.ptr;
        }
        public static unsafe Boolean operator <=(Utf8Pointer left, Utf8Pointer right)
        {
            return left.ptr <= right.ptr;
        }
        public static unsafe Boolean operator >=(Utf8Pointer left, Utf8Pointer right)
        {
            return left.ptr >= right.ptr;
        }
        public static unsafe Boolean operator ==(Utf8Pointer left, Utf8Pointer right)
        {
            return left.ptr == right.ptr;
        }
        public static unsafe Boolean operator !=(Utf8Pointer left, Utf8Pointer right)
        {
            return left.ptr != right.ptr;
        }
        public static unsafe Utf8Pointer operator +(Utf8Pointer ptr, UInt32 value)
        {
            return new Utf8Pointer(ptr.ptr + value);
        }
        public static unsafe Utf8Pointer operator ++(Utf8Pointer ptr)
        {
            return new Utf8Pointer(ptr.ptr + 1);
        }


        public unsafe Byte* ptr;
        public unsafe Utf8Pointer(Byte* ptr)
        {
            this.ptr = ptr;
        }

        public unsafe Byte this[UInt32 key]
        {
            get { return ptr[key]; }
            set { ptr[key] = value; }
        }
        public unsafe override bool Equals(object obj)
        {
            return (obj is Utf8Pointer) && ( ptr == ((Utf8Pointer)obj).ptr );
        }
        public unsafe String ToNewString(UInt32 length)
        {
            // TODO: add support for multi-byte utf8 characters

            // Create temporary character buffer on the stack
            // TODO: what should we do if the length seems too long for the stack?
            char* stackBuffer = stackalloc char[(int)length];
            if (stackBuffer == null)
            {
                // If this happens, I'll probably want to build the string using a StringBuilder or something
                throw new NotImplementedException(String.Format(
                    "Failed to create temporary stack buffer of {0} characters, a solution for this has not been implemented",
                    length));
            }
            Unicode.Utf8ToUtf16(ptr, stackBuffer, length);
            return new String(stackBuffer, 0, (int)length);
        }
        /*
        public Boolean Equals(UInt32 length, String s)
        {
            if (length == header.name.Length)
            {
                Boolean match = true;
                for (int cmp = 0; cmp < length; cmp++)
                {
                    if ((Char)at.ptr[cmp] != header.name[cmp])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    client.currentHeader = header;
                    break;
                }
            }
        }
        */

    }

    public struct Utf8LengthSlice
    {
        public Byte[] array;
        public UInt32 offset;
        public UInt32 length;
    }
    public struct Utf8LimitSlice
    {
        public Byte[] array;
        public UInt32 offset;
        public UInt32 length;
    }

    public struct Utf8PointerLimitSlice
    {
        public Utf8Pointer ptr;
        public Utf8Pointer limit;
        public Utf8PointerLimitSlice(Utf8Pointer ptr, Utf8Pointer limit)
        {
            this.ptr = ptr;
            this.limit = limit;
        }
        public Byte this[UInt32 key]
        {
            get { return ptr[key]; }
            set { ptr[key] = value; }
        }
    }
    public struct Utf8PointerLengthSlice
    {
        public Utf8Pointer ptr;
        public UInt32 length;

        public Utf8PointerLengthSlice(Utf8Pointer ptr, UInt32 length)
        {
            this.ptr = ptr;
            this.length = length;
        }
        public Byte this[UInt32 key]
        {
            get { return ptr[key]; }
            set { ptr[key] = value; }
        }
        public Utf8Pointer LimitPointer { get { return ptr + length; } }
        public unsafe String ToNewString()
        {
            return ptr.ToNewString(length);
        }
        public unsafe Boolean Equals(String s)
        {
            if (length == s.Length)
            {
                for (int i = 0; i < length; i++)
                {
                    if ((Char)ptr.ptr[i] != s[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        /*
        public override string ToString()
        {
            return ptr.ToNewString(length);
        }
        */
    }


    //
    // How should printing work?
    // 
    // You should have a state machine that takes format instructions and data.
    //
    // Case1: Print a C# Utf16 String
    // Question: What encoding are we using?
    //   Case UTF8: The formatter will have to convert the C# Utf16 string to Utf8 in chunks as
    //              it get's formatted
    //   Case UTF16: Simply 
    public class Utf8Formatter
    {

    }

}