// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Diagnostics;

namespace More
{
    public static class SliceExtensions
    {
        public static SliceByLimit<T> SliceByLimit<T>(this T[] array)
        {
            return new SliceByLimit<T>(array, 0, (UInt32)array.Length);
        }
        public static Slice<T> Slice<T>(this T[] array)
        {
            return new Slice<T>(array, 0, (UInt32)array.Length);
        }
    }
    public struct OffsetLength
    {
        public UInt32 offset;
        public UInt32 length;
        public OffsetLength(UInt32 offset, UInt32 length)
        {
            this.offset = offset;
            this.length = length;
        }
    }
    public struct OffsetLimit
    {
        public UInt32 offset;
        public UInt32 limit;
        public OffsetLimit(UInt32 offset, UInt32 limit)
        {
            this.offset = offset;
            this.limit = limit;
        }
    }
    public struct SliceByLimit<T>
    {
        public T[] array;
        public UInt32 offset;
        public UInt32 limit;
        public SliceByLimit(T[] array, UInt32 offset, UInt32 limit)
        {
            this.array = array;
            this.offset = offset;
            this.limit = limit;

            Debug.Assert(InValidState());
        }
        public Boolean InValidState()
        {
            return (offset <= limit) &&
                (
                    (
                        (array != null) && (limit <= array.Length)
                    ) || (
                        (array == null) && (limit - offset == 0)
                    )
                );
        }
        public static Boolean InValidState(Byte[] array, UInt32 offset, UInt32 limit)
        {
            return (offset <= limit) &&
                (
                    (
                        (array != null) && (limit <= array.Length)
                    ) || (
                        (array == null) && (limit - offset == 0)
                    )
                );
        }
        public static explicit operator SliceByLimit<T>(T[] array)
        {
            return new SliceByLimit<T>(array, 0, (UInt32)array.Length);
        }
        public Slice<T> ByLength()
        {
            return new Slice<T>(array, offset, offset + limit);
        }
    }
    public struct Slice<T>
    {
        public T[] array;
        public UInt32 offset;
        public UInt32 length;
        public Slice(T[] array, UInt32 offset, UInt32 length)
        {
            this.array = array;
            this.offset = offset;
            this.length = length;

            Debug.Assert(InValidState());
        }
        public Slice(T[] array)
        {
            this.array = array;
            this.offset = 0;
            this.length = (UInt32)array.Length;
        }
        public Boolean InValidState()
        {
            return length == 0 || (array != null && offset + length <= array.Length);
        }
        public static Boolean InValidState(Byte[] array, UInt32 offset, UInt32 length)
        {
            return length == 0 || (array != null && offset + length <= array.Length);
        }
        public static explicit operator Slice<T>(T[] array)
        {
            return new Slice<T>(array, 0, (UInt32)array.Length);
        }
        /*
        public Boolean EqualsString(String compare, Boolean ignoreCase)
        {
            if (length != compare.Length) return false;

            for (UInt32 i = 0; i < length; i++)
            {
                if ((Char)array[offset + i] != compare[(int)i])
                {
                    if (!ignoreCase) return false;
                    if (Char.IsUpper(compare[(int)i]))
                    {
                        if (Char.IsUpper((Char)array[offset + i])) return false;
                        if (Char.ToUpper((Char)array[offset + i]) != compare[(int)i]) return false;
                    }
                    else
                    {
                        if (Char.IsLower((Char)array[offset + i])) return false;
                        if (Char.ToLower((Char)array[offset + i]) != compare[(int)i]) return false;
                    }
                }
            }
            return true;
        }
        */

        // TODO: this function needs an update
        // Peel the first string until whitespace
        public static OffsetLength PeelAscii(ref Slice<Byte> slice)
        {
            Debug.Assert(slice.InValidState());

            if (slice.length == 0)
                return new OffsetLength(slice.offset, 0);

            Char c;

            UInt32 offset = slice.offset;
            UInt32 segmentLimit = offset + slice.length;

            //
            // Skip beginning whitespace
            //
            while (true)
            {
                if (offset >= segmentLimit)
                {
                    slice.offset = offset;
                    slice.length = 0;
                    return new OffsetLength(offset, 0);
                }
                c = (Char)slice.array[offset];
                if (!Char.IsWhiteSpace(c)) break;
                offset++;
            }

            UInt32 startOffset = offset;

            //
            // Find next whitespace
            //
            OffsetLength peelSegment;

            while (true)
            {
                offset++;
                if (offset >= segmentLimit)
                {
                    peelSegment = new OffsetLength(startOffset, offset - startOffset);
                    slice.offset = offset;
                    slice.length = 0;
                    return peelSegment;
                }
                c = (Char)slice.array[offset];
                if (Char.IsWhiteSpace(c)) break;
            }

            peelSegment = new OffsetLength(startOffset, offset - startOffset);

            //
            // Remove whitespace till rest
            //
            while (true)
            {
                offset++;
                if (offset >= segmentLimit)
                {
                    slice.offset = offset;
                    slice.length = 0;
                    return peelSegment;
                }
                if (!Char.IsWhiteSpace((Char)slice.array[offset]))
                {
                    slice.length -= (offset - slice.offset);
                    slice.offset = offset;
                    return peelSegment;
                }
            }
        }
    }
}