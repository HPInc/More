// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Diagnostics;

namespace More
{
    public static class ArrayExt
    {
        public static UInt32 CalculateNewSize(UInt32 currentSize, UInt32 neededSize)
        {
            if (currentSize == 0)
                currentSize = neededSize;
            else
            {
                while (currentSize < neededSize)
                {
                    currentSize *= 2;
                }
            }
            return currentSize;
        }
        public static void EnsureCapacityNoCopy<T>(ref T[] array, UInt32 capacity)
        {
            if ((UInt32)array.Length < capacity)
            {
                array = new T[CalculateNewSize((UInt32)array.Length, capacity)];
            }
        }
        public static void EnsureCapacityCopySomeData<T>(ref T[] array, UInt32 dataLength, UInt32 capacity)
        {
            if (array.Length < capacity)
            {
                T[] newArray = new T[CalculateNewSize((UInt32)array.Length, capacity)];
                System.Array.Copy(array, newArray, dataLength);
                array = newArray;
            }
        }
        public static void EnsureCapacityCopyAllData<T>(ref T[] array, UInt32 capacity)
        {
            if (array.Length < capacity)
            {
                Array.Resize<T>(ref array, (Int32)CalculateNewSize((UInt32)array.Length, capacity));
            }
        }
    }

    /*
    /// <summary>
    /// This class wraps a Byte array that can be passed to and from functions
    /// that will ensure that the array will be expanded to accomodate as much data is needed.
    /// </summary>
    public class Buf
    {
        public const UInt32 DefaultExpandLength = 128;
        public const UInt32 DefaultInitialCapacity = 128;

        public Byte[] array;
        public readonly UInt32 expandLength;

        public Buf()
            : this(DefaultInitialCapacity, DefaultExpandLength)
        {
        }
        public Buf(UInt32 initialCapacity)
        {
            this.expandLength = initialCapacity;
            this.array = new Byte[initialCapacity];
        }
        public Buf(UInt32 initialCapacity, UInt32 expandLength)
        {
            this.expandLength = expandLength;
            this.array = new Byte[initialCapacity];
        }
        public void EnsureCapacityNoCopy(UInt32 capacity)
        {
            if ((UInt32)array.Length < capacity)
            {
                UInt32 diff = capacity - (UInt32)array.Length;
                UInt32 newSizeDiff = (diff > expandLength) ? diff : expandLength;
                this.array = new Byte[array.Length + newSizeDiff];
            }
        }
        public void EnsureCapacityCopyData(Int32 capacity)
        {
            if (array.Length < capacity)
            {
                UInt32 diff = (UInt32)(capacity - array.Length);
                UInt32 newSizeDiff = (diff > expandLength) ? diff : expandLength;

                Byte[] newArray = new Byte[array.Length + newSizeDiff];
                System.Array.Copy(array, newArray, array.Length);
                this.array = newArray;
            }
        }
        public void EnsureCapacityCopyData(UInt32 capacity)
        {
            if ((UInt32)array.Length < capacity)
            {
                UInt32 diff = capacity - (UInt32)array.Length;
                UInt32 newSizeDiff = (diff > expandLength) ? diff : expandLength;

                Byte[] newArray = new Byte[array.Length + newSizeDiff];
                System.Array.Copy(array, newArray, array.Length);
                this.array = newArray;
            }
        }
    }
    /// <summary>
    /// This class wraps an array that can be passed to and from functions
    /// that will ensure that the array size is expanded when needed
    /// </summary>
    /// <typeparam name="T">The array element type</typeparam>
    public class Expandable<T>
    {
        public const UInt32 DefaultExpandLength = 128;
        public const UInt32 DefaultInitialCapacity = 128;

        public T[] array;
        public readonly UInt32 expandLength;

        public Expandable()
            : this(DefaultInitialCapacity, DefaultExpandLength)
        {
        }
        public Expandable(UInt32 initialCapacity, UInt32 expandLength)
        {
            this.expandLength = expandLength;
            this.array = new T[initialCapacity];
        }
        public void EnsureCapacityNoCopy(UInt32 capacity)
        {
            if ((UInt32)array.Length < capacity)
            {
                UInt32 diff = capacity - (UInt32)array.Length;
                UInt32 newSizeDiff = (diff > expandLength) ? diff : expandLength;
                this.array = new T[array.Length + newSizeDiff];
            }
        }
        public void EnsureCapacityCopyData(Int32 capacity)
        {
            if (array.Length < capacity)
            {
                UInt32 diff = (UInt32)(capacity - array.Length);
                UInt32 newSizeDiff = (diff > expandLength) ? diff : expandLength;

                T[] newArray = new T[array.Length + newSizeDiff];
                System.Array.Copy(array, newArray, array.Length);
                this.array = newArray;
            }
        }
        public void EnsureCapacityCopyData(UInt32 capacity)
        {
            if ((UInt32)array.Length < capacity)
            {
                UInt32 diff = capacity - (UInt32)array.Length;
                UInt32 newSizeDiff = (diff > expandLength) ? diff : expandLength;

                T[] newArray = new T[array.Length + newSizeDiff];
                System.Array.Copy(array, newArray, array.Length);
                this.array = newArray;
            }
        }
    }
    */
    /// <summary>
    /// A class wrapper for a byte array.
    /// </summary>
    public class ByteArrayReference
    {
        /// <summary>
        /// The referenced array.
        /// </summary>
        public Byte[] array;

        /// <summary>
        /// Creates an array with the given size.
        /// </summary>
        /// <param name="size">The size of the array.</param>
        public ByteArrayReference(UInt32 size)
        {
            this.array = new Byte[size];
        }
        public ByteArrayReference(Byte[] array)
        {
            this.array = array;
        }
        public void EnsureCapacityNoCopy(UInt32 capacity)
        {
            ArrayExt.EnsureCapacityNoCopy(ref array, capacity);
        }
        public void EnsureCapacityCopyAllData(UInt32 capacity)
        {
            ArrayExt.EnsureCapacityCopyAllData(ref array, capacity);
        }
    }
    /// <summary>
    /// This class wraps an array that can be passed to and from functions
    /// that will ensure that the array size is expanded when needed
    /// </summary>
    /// <typeparam name="T">The array element type</typeparam>
    public class ArrayReference<T>
    {
        public T[] array;
        
        public ArrayReference(UInt32 size)
        {
            this.array = new T[size];
        }
        public ArrayReference(T[] array)
        {
            this.array = array;
        }
        public void EnsureCapacityNoCopy(UInt32 capacity)
        {
            if ((UInt32)array.Length < capacity)
            {
                UInt32 newSize = (UInt32)array.Length * 2;
                if (newSize < capacity)
                {
                    newSize = capacity;
                }
                this.array = new T[newSize];
            }
        }
        public void EnsureCapacityCopyData(UInt32 capacity)
        {
            if (array.Length < capacity)
            {
                UInt32 newSize = (UInt32)array.Length * 2;
                if (newSize < capacity)
                {
                    newSize = capacity;
                }
                T[] newArray = new T[newSize];
                System.Array.Copy(array, newArray, array.Length);
                this.array = newArray;
            }
        }
    }
    /// <summary>
    /// Used to pass around a buffer with a length representing the content length.
    /// </summary>
    public struct BufStruct
    {
        public Byte[] buf;
        public UInt32 contentLength;
        public BufStruct(Byte[] buf)
        {
            this.buf = buf;
            this.contentLength = 0;
        }
        public BufStruct(Byte[] buf, UInt32 contentLength)
        {
            this.buf = buf;
            this.contentLength = contentLength;
        }
    }
}
