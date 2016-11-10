// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace More
{
    public class LinkedQueue<T> : IEnumerable<T>, ICollection<T>
    {
        readonly LinkedList<T> items = new LinkedList<T>();
        public LinkedQueue()
            : base()
        {
        }
        public Int32 Count { get { return items.Count; } }
        public void Enqueue(T item)
        {
            items.AddLast(item);
        }
        public T Dequeue()
        {
            var first = items.First;
            if (first == null)
                throw new InvalidOperationException("Cannot call Dequeue on an empty queue");
            var firstValue = first.Value;
            items.RemoveFirst();
            return firstValue;
        }
        public T Peek()
        {
            var first = items.First;
            if (first == null)
                throw new InvalidOperationException("Cannot call Peek on an empty queue");
            return first.Value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>Same as Enqueue, only exists to accomodate the ICollection interface</summary>
        /// <param name="item">The item to enqueue</param>
        public void Add(T item)
        {
            Enqueue(item);
        }
        /// <summary>
        /// Removes the first occurrence of the specified value.  Performance cost is O(n).
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>true if the value was removed, otherwise, false</returns>
        public Boolean Remove(T item)
        {
            return items.Remove(item);
        }
        public void Clear()
        {
            items.Clear();
        }
        public Boolean Contains(T item)
        {
            return items.Contains(item);
        }
        public void CopyTo(T[] array, int index)
        {
            items.CopyTo(array, index);
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
    }
    /// <summary>
    /// A struct that contains one item, and a list of other items if there are any more.
    /// This is used for lists of items, where there will typically be 1 item per entry, but
    /// could have more then 1, and each entry will also never have 0 entries.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct OneOrMore<T> : IList<T>
    {
        public T first;
        List<T> others;
        public OneOrMore(T first)
        {
            this.first = first;
            this.others = null;
        }
        public Int32 Count
        {
            get { return (others == null) ? 1 : 1 + others.Count; }
        }
        public void Add(T item)
        {
            if (others == null)
            {
                others = new List<T>();
            }
            others.Add(item);
        }
        public void Clear()
        {
            throw new NotSupportedException("Cannot clear an instance of OneOrMore, must always have at least one item");
        }
        public Boolean Contains(T item)
        {
            if (first.Equals(item))
            {
                return true;
            }
            return (others == null) ? false : others.Contains(item);
        }
        public void CopyTo(T[] array, Int32 arrayIndex)
        {
            array[arrayIndex] = first;
            if (others != null)
            {
                others.CopyTo(array, arrayIndex + 1);
            }
        }
        public Boolean IsReadOnly
        {
            get { return false; }
        }
        public bool Remove(T item)
        {
            if (item.Equals(first))
            {
                if (others == null || others.Count == 0)
                {
                    throw new InvalidOperationException("Cannot remove because this list must always have at least 1 item");
                }
                first = others[others.Count - 1];
                others.RemoveAt(others.Count - 1);
                return true;
            }
            return (others == null) ? false : others.Remove(item);
        }
        public IEnumerator<T> GetEnumerator()
        {
            yield return first;
            if (others != null)
            {
                for (int i = 0; i < others.Count; i++)
                {
                    yield return others[i];
                }
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public int IndexOf(T item)
        {
            if (item.Equals(first))
            {
                return 0;
            }
            return (others == null) ? -1 : others.IndexOf(item);
        }
        public T this[int index]
        {
            get
            {
                return (index == 0) ? first : others[index - 1];
            }
            set
            {
                if (index == 0)
                {
                    first = value;
                }
                else
                {
                    others[index - 1] = value;
                }
            }
        }
        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }


    public class SortedNumberSet : IEnumerable<int>, ICollection<int>
    {
        public struct LimitRange
        {
            public int start;
            public int limit;
            public LimitRange(int start, int limit)
            {
                this.start = start;
                this.limit = limit;
            }
        }

        List<LimitRange> sortedRanges = new List<LimitRange>();
        UInt32 count;
        public SortedNumberSet()
        {
        }

        public void Clear()
        {
            this.sortedRanges.Clear();
            this.count = 0;
        }

        public bool Contains(int item)
        {
            throw new NotImplementedException();
        }
        public void CopyTo(int[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        public int Count
        {
            get { return (int)count; }
        }
        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }
        public bool Remove(int item)
        {
            throw new NotImplementedException();
        }


        public void Add(int value)
        {
            TryAdd(value);
        }

        /// <returns>True if the value was added</returns>
        public Boolean TryAdd(int value)
        {
            for (int i = 0; i < sortedRanges.Count; i++)
            {
                var range = sortedRanges[i];
                if (value < range.start)
                {
                    if (value + 1 == range.start)
                    {
                        sortedRanges[i] = new LimitRange(value, range.limit);
                    }
                    else
                    {
                        sortedRanges.Insert(i, new LimitRange(value, value + 1));
                    }
                    count++;
                    return true; // was added
                }

                if (value.Equals(range.start))
                {
                    return false; // was already added
                }
            }

            if (sortedRanges.Count == 0)
            {
                sortedRanges.Add(new LimitRange(value, value + 1));
                count++;
                return true;
            }

            var lastRange = sortedRanges[sortedRanges.Count - 1];
            if (lastRange.limit < value)
            {
                sortedRanges.Add(new LimitRange(value, value + 1));
                count++;
                return true; // was added
            }
            if (lastRange.limit == value)
            {
                sortedRanges[sortedRanges.Count - 1] = new LimitRange(lastRange.start, value + 1);
                count++;
                return true; // was added
            }
            return false; // was already added
        }

        void CombineRanges(int startRangeIndex, int limit)
        {
            var startRange = sortedRanges[startRangeIndex];
            if (limit > startRange.limit)
            {
                var previousRange = startRange;
                for (int ii = startRangeIndex + 1; ; ii++)
                {
                    if (ii >= sortedRanges.Count)
                    {
                        if (ii > startRangeIndex)
                        {
                            sortedRanges.RemoveRange(startRangeIndex + 1, ii - 1);
                        }
                        count += (uint)(limit - previousRange.limit);
                        sortedRanges[startRangeIndex] = new LimitRange(startRange.start, limit);
                        return;
                    }
                    var range = sortedRanges[ii];
                    if (limit < range.start)
                    {
                        if (ii > startRangeIndex)
                        {
                            sortedRanges.RemoveRange(startRangeIndex + 1, ii - 1);
                        }
                        count += (uint)(limit - previousRange.limit);
                        sortedRanges[startRangeIndex] = new LimitRange(startRange.start, limit);
                        return;
                    }
                    count += (uint)(range.start - previousRange.limit);
                    if (limit <= range.limit)
                    {
                        if (ii > startRangeIndex)
                        {
                            sortedRanges.RemoveRange(startRangeIndex + 1, ii);
                        }
                        sortedRanges[startRangeIndex] = new LimitRange(startRange.start, range.limit);
                        return;
                    }
                    previousRange = range;
                }
            }
        }

        public void AddRange(int start, int limit)
        {
            if (start >= limit)
            {
                throw new ArgumentException(String.Format("start ({0}) cannot be >= to limit ({1})", start, limit));
            }

            for (int i = 0; i < sortedRanges.Count; i++)
            {
                var range = sortedRanges[i];
                if (start <= range.limit)
                {
                    if (limit < range.start)
                    {
                        sortedRanges.Insert(i, new LimitRange(start, limit));
                        count += (uint)(limit - start);
                        return;
                    }

                    if (start < range.start)
                    {
                        count += (uint)(range.start - start);
                        sortedRanges[i] = new LimitRange(start, range.limit);
                    }
                    CombineRanges(i, limit);
                    return;
                }
            }

            if (sortedRanges.Count == 0)
            {
                sortedRanges.Add(new LimitRange(start, limit));
                count += (uint)(limit - start);
                return;
            }

            var lastRange = sortedRanges[sortedRanges.Count - 1];
            if (lastRange.limit < start)
            {
                sortedRanges.Add(new LimitRange(start, limit));
                count += (uint)(limit - start);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public IEnumerator<int> GetEnumerator()
        {
            foreach (var range in sortedRanges)
            {
                for (var value = range.start; value < range.limit; value++)
                {
                    yield return value;
                }
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public String RangeString()
        {
            StringBuilder builder = new StringBuilder();
            Boolean atFirst = true;
            foreach (var range in sortedRanges)
            {
                if(atFirst) { atFirst = false; } else { builder.Append(","); }
                if(range.limit == range.start + 1)
                {
                    builder.Append(range.start);
                }
                else
                {
                    builder.Append(range.start);
                    builder.Append('-');
                    builder.Append(range.limit - 1);
                }
            }
            return builder.ToString();
        }
    }
}