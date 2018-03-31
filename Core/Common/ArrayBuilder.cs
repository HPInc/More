// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;

namespace More
{
    public class ArrayBuilder
    {
        const Int32 InitialArraySize = 16;

        Type elementType;

        Array array;
        Int32 count;

        public ArrayBuilder(Type elementType)
            : this(elementType, InitialArraySize)
        {
        }
        public ArrayBuilder(Type elementType, Int32 initialArraySize)
        {
            this.elementType = elementType;
            this.array = Array.CreateInstance(elementType, initialArraySize);
            this.count = 0;
        }
        public void Add(Object obj)
        {
            if (this.count >= array.Length)
            {
                Array newArray = Array.CreateInstance(elementType, this.array.Length * 2);
                Array.Copy(this.array, newArray, this.count);
                this.array = newArray;
            }
            this.array.SetValue(obj, this.count++);
        }
        public Array Build()
        {
            if (array.Length != count)
            {
                Array newArray = Array.CreateInstance(elementType, this.count);
                Array.Copy(this.array, newArray, this.count);
                this.array = newArray;
            }

            return this.array;
        }
    }
    public class GenericArrayBuilder<T>
    {
        const Int32 InitialArraySize = 16;

        T[] array;
        Int32 count;

        public GenericArrayBuilder()
            : this(InitialArraySize)
        {
        }
        public GenericArrayBuilder(Int32 initialArraySize)
        {
            this.array = new T[initialArraySize];
            this.count = 0;
        }
        public void Add(T obj)
        {
            if (this.count >= array.Length)
            {
                T[] newArray = new T[this.array.Length * 2];
                Array.Copy(this.array, newArray, this.count);
                this.array = newArray;
            }
            this.array[this.count++] = obj;
        }
        public T[] Build()
        {
            if (array.Length != count)
            {
                T[] newArray = new T[this.count];
                Array.Copy(this.array, newArray, this.count);
                this.array = newArray;
            }
            return this.array;
        }
    }
}
