// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Text;

namespace More
{
    public static class CommonComparisons
    {
        public static Int32 IncreasingInt32(Int32 x, Int32 y)
        {
            return (x > y) ? 1 : ((x < y) ? -1 : 0);
        }
        public static Int32 DecreasingInt32(Int32 x, Int32 y)
        {
            return (x > y) ? -1 : ((x < y) ? 1 : 0);
        }
        public static Int32 IncreasingUInt32(UInt32 x, UInt32 y)
        {
            return (x > y) ? 1 : ((x < y) ? -1 : 0);
        }
        public static Int32 DecreasingUInt32(UInt32 x, UInt32 y)
        {
            return (x > y) ? -1 : ((x < y) ? 1 : 0);
        }
    }
    public class SortedList<T> : IList<T>
    {
        public class Enumerator : IEnumerator<T>
        {
            public readonly SortedList<T> list;
            public UInt32 state;
            public Enumerator(SortedList<T> list)
            {
                this.list = list;
                this.state = UInt32.MaxValue;
            }
            public void Reset()
            {
                this.state = UInt32.MaxValue;
            }
            public void Dispose()
            {
            }
            public T Current
            {
                get { return list.elements[this.state]; }
            }
            object System.Collections.IEnumerator.Current
            {
                get { return list.elements[this.state]; }
            }
            public Boolean MoveNext()
            {
                state++;
                return state < list.count;
            }
        }

        public T[] elements;
        public UInt32 count;

        private readonly UInt32 extendLength;

        private readonly Comparison<T> comparison;

        public SortedList(UInt32 initialCapacity, UInt32 extendLength, Comparison<T> comparison)
        {
            if (comparison == null) throw new ArgumentNullException("comparison");

            this.elements = new T[initialCapacity];
            this.count = 0;

            this.extendLength = extendLength;

            this.comparison = comparison;
        }
        public T this[int i]
        {
            get { return elements[i]; }
            set { throw new InvalidOperationException("Cannot set an element to a specific index on a SortedList"); }
        }
        public Boolean IsReadOnly
        {
            get { return false; }
        }
        public Int32 Count { get { return (Int32)this.count; } }
        public Boolean Contains(T item)
        {
            for (int i = 0; i < count; i++)
            {
                if (item.Equals(elements[i]))
                {
                    return true;
                }
            }
            return false;
        }
        public Int32 IndexOf(T item)
        {
            for (int i = 0; i < count; i++)
            {
                T listItem = elements[i];
                if (listItem.Equals(item)) return i;
            }
            return -1;
        }
        public void CopyTo(T[] array, Int32 arrayIndex)
        {
            for (int i = 0; i < count; i++)
            {
                array[arrayIndex++] = elements[i];
            }
        }
        public void Insert(int index, T item)
        {
            throw new InvalidOperationException("You cannot call Insert on a sorted list, call Add");
        }
        private void InsertPreSorted(UInt32 index, T item)
        {
            if (count >= elements.Length)
            {
                T[] newElements = new T[elements.Length + extendLength];
                Array.Copy(elements, newElements, elements.Length);
                elements = newElements;
            }
            // Shift elements after 'index' to the right by 1
            for (UInt32 dstIndex = count;  dstIndex > index; dstIndex--)
            {
                elements[dstIndex] = elements[dstIndex - 1];
            }
            elements[index] = item;
            count++;
        }
        public void Add(T newElement)
        {
            UInt32 index = 0;
            for (; index < count; index++)
            {
                if (comparison(newElement, elements[index]) <= 0)
                {
                    break;
                }
            }
            InsertPreSorted(index, newElement);
        }
        public void AddFromEnd(T newElement)
        {
            UInt32 index = count;
            while(index > 0)
            {
                index--;
                if (comparison(newElement, elements[index]) >= 0)
                {
                    index++;
                    break;
                }
            }
            InsertPreSorted(index, newElement);
        }

        public void Clear()
        {
            // remove references if necessary
            if (typeof(T).IsClass)
            {
                for (int i = 0; i < count; i++)
                {
                    this.elements[i] = default(T);
                }
            }
            this.count = 0;
        }
        public T GetAndRemoveLastElement()
        {
            count--;
            T element = elements[count];

            elements[count] = default(T); // Delete reference to this object

            return element;
        }
        public Boolean Remove(T element)
        {
            for (int i = 0; i < count; i++)
            {
                if (element.Equals(elements[i]))
                {
                    RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        public void RemoveAt(int index)
        {
            while (index < count - 1)
            {
                elements[index] = elements[index + 1];
                index++;
            }
            elements[index] = default(T); // Delete reference to this object
            count--;
        }
        public void RemoveFromStart(UInt32 count)
        {
            if (count <= 0) return;
            if (count >= this.count)
            {
                Clear();
                return;
            }

            this.count -= count;
            for (int i = 0; i < this.count; i++)
            {
                elements[i] = elements[count + i];
            }

            if (typeof(T).IsClass)
            {
                for (int i = 0; i < count; i++)
                {
                    this.elements[this.count + i] = default(T);
                }
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
