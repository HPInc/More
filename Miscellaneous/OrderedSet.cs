// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;

namespace More
{
    public static class OrderedSets
    {
        // NOTE: this can probably be made more efficient
        public static OrderedSet<T> Combine<T>(this IEnumerable<OrderedSet<T>> sets)
        {
            IEnumerator<OrderedSet<T>> enumerator = sets.GetEnumerator();
            if (!enumerator.MoveNext()) return default(OrderedSet<T>);

            OrderedSet<T> combined = enumerator.Current;

            while (enumerator.MoveNext())
            {
                combined = combined.Combine(enumerator.Current);
            }

            return combined;
        }
        public static UInt32 CombinedLength<T>(T[] orderedSetA, T[] orderedSetB)
        {
            if (orderedSetA == null || orderedSetA.Length <= 0)
            {
                return (orderedSetB == null) ? 0 : (UInt32)orderedSetB.Length;
            }
            if (orderedSetB == null || orderedSetB.Length <= 0)
            {
                return (orderedSetA == null) ? 0 : (UInt32)orderedSetA.Length;
            }

            UInt32 combinedLength = 0;

            UInt32 setAIndex = 0, setBIndex = 0;

            T setAValue = orderedSetA[0];
            T setBValue = orderedSetB[0];
            while (true)
            {
                Int32 compareValue = Comparer<T>.Default.Compare(setAValue, setBValue);
                if (compareValue == 0)
                {
                    combinedLength++;
                    setAIndex++;
                    setBIndex++;
                    if (setAIndex >= orderedSetA.Length || setBIndex >= orderedSetB.Length) break;
                    setAValue = orderedSetA[setAIndex];
                    setBValue = orderedSetB[setBIndex];
                }
                else if (compareValue < 0)
                {
                    combinedLength++;
                    setAIndex++;
                    if (setAIndex >= orderedSetA.Length) break;
                    setAValue = orderedSetA[setAIndex];
                }
                else
                {
                    combinedLength++;
                    setBIndex++;
                    if (setBIndex >= orderedSetB.Length) break;
                    setBValue = orderedSetB[setBIndex];
                }
            }

            while (setBIndex < orderedSetB.Length)
            {
                combinedLength++;
                setBIndex++;
            }
            while (setAIndex < orderedSetA.Length)
            {
                combinedLength++;
                setAIndex++;
            }
            return combinedLength;
        }
        public static void Combine<T>(T[] orderedSetA, T[] orderedSetB, T[] combined)
        {
            if (orderedSetA == null || orderedSetA.Length <= 0 ||
                orderedSetB == null || orderedSetB.Length <= 0) throw new ArgumentException("sets cannot be null or empty");

            UInt32 combinedIndex = 0;
            UInt32 setAIndex = 0, setBIndex = 0;

            T setAValue = orderedSetA[0];
            T setBValue = orderedSetB[0];
            while (true)
            {
                Int32 compareValue = Comparer<T>.Default.Compare(setAValue, setBValue);
                if (compareValue == 0)
                {
                    combined[combinedIndex++] = setAValue;
                    setAIndex++;
                    setBIndex++;
                    if (setAIndex >= orderedSetA.Length || setBIndex >= orderedSetB.Length) break;
                    setAValue = orderedSetA[setAIndex];
                    setBValue = orderedSetB[setBIndex];
                }
                else if (compareValue < 0)
                {
                    combined[combinedIndex++] = setAValue;
                    setAIndex++;
                    if (setAIndex >= orderedSetA.Length) break;
                    setAValue = orderedSetA[setAIndex];
                }
                else
                {
                    combined[combinedIndex++] = setBValue;
                    setBIndex++;
                    if (setBIndex >= orderedSetB.Length) break;
                    setBValue = orderedSetB[setBIndex];
                }
            }

            while (setBIndex < orderedSetB.Length)
            {
                combined[combinedIndex++] = orderedSetB[setBIndex];
                setBIndex++;
            }
            while (setAIndex < orderedSetA.Length)
            {
                combined[combinedIndex++] = orderedSetA[setAIndex];
                setAIndex++;
            }

            if (combinedIndex != combined.Length)
                throw new ArgumentException(String.Format("Expected combined length to be {0} but was {1}", combined.Length, combinedIndex));
        }
        public static Boolean OrderedContains<T>(this T[] ordered, T value)
        {
            if (ordered == null || ordered.Length <= 0) return false;

            Int32 lowIndex = 0, highIndex = ordered.Length - 1;
            while (true)
            {
                Int32 diff = highIndex - lowIndex;
                //Console.WriteLine("Value {0} Low {1} High {2}", value, lowIndex, highIndex);

                if (diff <= 1)
                {
                    return value.Equals(ordered[lowIndex]) || (diff > 0 && value.Equals(ordered[highIndex]));
                }

                Int32 index = lowIndex + (diff + 1) / 2;
                Int32 compareValue = Comparer<T>.Default.Compare(value, ordered[index]);
                if (compareValue == 0) return true;
                if (compareValue < 0)
                {
                    highIndex = index - 1;
                }
                else
                {
                    lowIndex = index + 1;
                }
            }
        }
    }
    public static class OrderedSet
    {
        public static OrderedSet<T> SortArrayAndGetSet<T>(params T[] set)
        {
            Array.Sort(set);
            // Verify there are no duplicates
            if (set.Length > 0)
            {
                T last = set[0];
                for (int i = 1; i < set.Length; i++)
                {
                    T current = set[i];
                    var result = Comparer<T>.Default.Compare(last, current);
                    if (result == 0)
                        throw new ArgumentException(String.Format("The array contained duplicates of '{0}' starting at index {1}",
                            current, i-1));
                    last = current;
                }
            }
            return GetSetUsingPreorderedSet(set);
        }
        public static OrderedSet<T> VerifySortedAndGetSet<T>(params T[] set)
        {
            if (set.Length > 0)
            {
                T last = set[0];
                for (int i = 1; i < set.Length; i++)
                {
                    T current = set[i];
                    var result = Comparer<T>.Default.Compare(last, current);
                    if (result >= 0)
                    {
                        if(result == 0)
                            throw new ArgumentException(String.Format("The array contained duplicates of '{0}'", current));
                        throw new ArgumentException(String.Format("The array was not sorted (unsorted at index {0})", i-1));
                    }
                    last = current;
                }
            }
            return GetSetUsingPreorderedSet(set);
        }
        static OrderedSet<T> GetSetUsingPreorderedSet<T>(T[] orderedSet)
        {
            //
            // Check if this set is already created
            //
            for (int i = 0; i < OrderedSet<T>.allCreatedSets.Count; i++)
            {
                OrderedSet<T> existingSet = OrderedSet<T>.allCreatedSets[i];
                if (orderedSet.Length == existingSet.orderedSet.Length)
                {
                    Boolean isMatch = true;
                    for (int j = 0; j < orderedSet.Length; j++)
                    {
                        if (!orderedSet[j].Equals(existingSet.orderedSet[j]))
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    if (isMatch) return existingSet;
                }
            }

            //
            // Create a new ordered set
            //
            OrderedSet<T> newSet = new OrderedSet<T>(orderedSet);
            OrderedSet<T>.allCreatedSets.Add(newSet);
            return newSet;
        }
    }
    public struct OrderedSet<T> : IEnumerable<T>, IEquatable<OrderedSet<T>>
    {
        internal static readonly List<OrderedSet<T>> allCreatedSets = new List<OrderedSet<T>>();

        // Ordered set can be null
        public readonly T[] orderedSet;
        internal OrderedSet(T[] orderedSet)
        {
            this.orderedSet = orderedSet;
        }
        public Boolean IsEmpty()
        {
            return orderedSet == null || orderedSet.Length <= 0;
        }
        public Boolean Equals(OrderedSet<T> other)
        {
            if (this.IsEmpty()) return other.IsEmpty();
            if (other.IsEmpty()) return false;

            // This comparison can be done because no identical array will be created
            return this.orderedSet == other.orderedSet;
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return orderedSet.GetGenericEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return orderedSet.GetGenericEnumerator();
        }
        //
        // Uses a fast binary search method
        //
        public Boolean Contains(T t)
        {
            return IndexOf(t) >= 0;
        }
        /// <summary>
        /// Returns the index of the given parameter.
        /// </summary>
        /// <param name="t">The parameter to search for</param>
        /// <returns></returns>
        public Int32 IndexOf(T t)
        {
            if (orderedSet == null || orderedSet.Length <= 0) return -1;

            Int32 lowIndex = 0, highIndex = orderedSet.Length - 1;
            while (true)
            {
                Int32 diff = highIndex - lowIndex;

                if (diff <= 1)
                {
                    if (t.Equals(orderedSet[lowIndex]))
                        return lowIndex;
                    if (t.Equals(orderedSet[highIndex]))
                        return highIndex;
                    return -1;
                }

                Int32 index = lowIndex + (diff / 2);
                Int32 compareValue = Comparer<T>.Default.Compare(t, orderedSet[index]);
                if (compareValue == 0) return index;
                if (compareValue < 0)
                {
                    highIndex = index - 1;
                }
                else
                {
                    lowIndex = index + 1;
                }
            }
        }
        public void WriteToConsole()
        {
            Console.Write("{");
            for (int i = 0; i < orderedSet.Length; i++)
            {
                if (i > 0) Console.Write(" ,");
                Console.Write(orderedSet[i]);
            }
            Console.Write("}");
        }
        //
        // TODO: This function may have some bugs/innefficiencies...
        //       I should make a suite of regression tests for it.
        //
        public OrderedSet<T> Combine(OrderedSet<T> otherSet)
        {
            if (this.IsEmpty()) return otherSet;
            if (otherSet.IsEmpty()) return this;
            if (this.orderedSet == otherSet.orderedSet) return this;

            UInt32 combinedLength = OrderedSets.CombinedLength(this.orderedSet, otherSet.orderedSet);
            UInt32 thisSetIndex, otherSetIndex;

            //
            // Check if this combination has already been created
            //
            Boolean isMatch = true;
            for (int i = 0; i < allCreatedSets.Count; i++)
            {
                OrderedSet<T> existingSet = allCreatedSets[i];
                if (existingSet.IsEmpty()) continue;
                if (combinedLength != existingSet.orderedSet.Length) continue;

                //
                // Small optimization, no need to check the set if it is the same as an existing set
                //
                if (existingSet.orderedSet == this.orderedSet || existingSet.orderedSet == otherSet.orderedSet) continue;

                //
                // Loop through the combined set
                //
                UInt32 existingSetIndex = 0;
                thisSetIndex = 0;
                otherSetIndex = 0;
                while (existingSetIndex < existingSet.orderedSet.Length)
                {
                    T nextValue;
                    if (thisSetIndex >= this.orderedSet.Length)
                    {
                        if (otherSetIndex >= otherSet.orderedSet.Length) break;
                        nextValue = otherSet.orderedSet[otherSetIndex];
                        otherSetIndex++;
                    }
                    else if (otherSetIndex >= otherSet.orderedSet.Length)
                    {
                        nextValue = this.orderedSet[thisSetIndex];
                        thisSetIndex++;
                    }
                    else
                    {
                        nextValue = this.orderedSet[thisSetIndex];
                        T otherValue = otherSet.orderedSet[otherSetIndex];
                        Int32 compareValue = Comparer<T>.Default.Compare(nextValue, otherValue);
                        if (compareValue == 0)
                        {
                            thisSetIndex++;
                            otherSetIndex++;
                        }
                        else if (compareValue < 0)
                        {
                            thisSetIndex++;
                        }
                        else
                        {
                            nextValue = otherValue;
                            otherSetIndex++;
                        }
                    }

                    if (!nextValue.Equals(existingSet.orderedSet[existingSetIndex]))
                    {
                        isMatch = false;
                        break;
                    }
                    existingSetIndex++;
                }
                if (isMatch) return existingSet;
            }

            //
            // Create the combination
            //
            T[] combined = new T[combinedLength];
            OrderedSets.Combine(this.orderedSet, otherSet.orderedSet, combined);

            OrderedSet<T> newOrderedSet = new OrderedSet<T>(combined);
            allCreatedSets.Add(newOrderedSet);

            return newOrderedSet;
        }
    }
    public static class SingletonArrays
    {
        /*
        // NOTE: this can probably be made more efficient
        public static OrderedSet<T> Combine<T>(this IEnumerable<OrderedSet<T>> sets)
        {
            IEnumerator<OrderedSet<T>> enumerator = sets.GetEnumerator();
            if (!enumerator.MoveNext()) return default(OrderedSet<T>);

            OrderedSet<T> combined = enumerator.Current;

            while (enumerator.MoveNext())
            {
                combined = combined.Combine(enumerator.Current);
            }

            return combined;
        }
        public static UInt32 CombinedLength<T>(T[] orderedSetA, T[] orderedSetB)
        {
            if (orderedSetA == null || orderedSetA.Length <= 0)
            {
                return (orderedSetB == null) ? 0 : (UInt32)orderedSetB.Length;
            }
            if (orderedSetB == null || orderedSetB.Length <= 0)
            {
                return (orderedSetA == null) ? 0 : (UInt32)orderedSetA.Length;
            }

            UInt32 combinedLength = 0;

            UInt32 setAIndex = 0, setBIndex = 0;

            T setAValue = orderedSetA[0];
            T setBValue = orderedSetB[0];
            while (true)
            {
                Int32 compareValue = Comparer<T>.Default.Compare(setAValue, setBValue);
                if (compareValue == 0)
                {
                    combinedLength++;
                    setAIndex++;
                    setBIndex++;
                    if (setAIndex >= orderedSetA.Length || setBIndex >= orderedSetB.Length) break;
                    setAValue = orderedSetA[setAIndex];
                    setBValue = orderedSetB[setBIndex];
                }
                else if (compareValue < 0)
                {
                    combinedLength++;
                    setAIndex++;
                    if (setAIndex >= orderedSetA.Length) break;
                    setAValue = orderedSetA[setAIndex];
                }
                else
                {
                    combinedLength++;
                    setBIndex++;
                    if (setBIndex >= orderedSetB.Length) break;
                    setBValue = orderedSetB[setBIndex];
                }
            }

            while (setBIndex < orderedSetB.Length)
            {
                combinedLength++;
                setBIndex++;
            }
            while (setAIndex < orderedSetA.Length)
            {
                combinedLength++;
                setAIndex++;
            }
            return combinedLength;
        }
        public static void Combine<T>(T[] orderedSetA, T[] orderedSetB, T[] combined)
        {
            if (orderedSetA == null || orderedSetA.Length <= 0 ||
                orderedSetB == null || orderedSetB.Length <= 0) throw new ArgumentException("sets cannot be null or empty");

            UInt32 combinedIndex = 0;
            UInt32 setAIndex = 0, setBIndex = 0;

            T setAValue = orderedSetA[0];
            T setBValue = orderedSetB[0];
            while (true)
            {
                Int32 compareValue = Comparer<T>.Default.Compare(setAValue, setBValue);
                if (compareValue == 0)
                {
                    combined[combinedIndex++] = setAValue;
                    setAIndex++;
                    setBIndex++;
                    if (setAIndex >= orderedSetA.Length || setBIndex >= orderedSetB.Length) break;
                    setAValue = orderedSetA[setAIndex];
                    setBValue = orderedSetB[setBIndex];
                }
                else if (compareValue < 0)
                {
                    combined[combinedIndex++] = setAValue;
                    setAIndex++;
                    if (setAIndex >= orderedSetA.Length) break;
                    setAValue = orderedSetA[setAIndex];
                }
                else
                {
                    combined[combinedIndex++] = setBValue;
                    setBIndex++;
                    if (setBIndex >= orderedSetB.Length) break;
                    setBValue = orderedSetB[setBIndex];
                }
            }

            while (setBIndex < orderedSetB.Length)
            {
                combined[combinedIndex++] = orderedSetB[setBIndex];
                setBIndex++;
            }
            while (setAIndex < orderedSetA.Length)
            {
                combined[combinedIndex++] = orderedSetA[setAIndex];
                setAIndex++;
            }

            if (combinedIndex != combined.Length)
                throw new ArgumentException(String.Format("Expected combined length to be {0} but was {1}", combined.Length, combinedIndex));
        }
        public static Boolean OrderedContains<T>(this T[] ordered, T value)
        {
            if (ordered == null || ordered.Length <= 0) return false;

            Int32 lowIndex = 0, highIndex = ordered.Length - 1;
            while (true)
            {
                Int32 diff = highIndex - lowIndex;
                //Console.WriteLine("Value {0} Low {1} High {2}", value, lowIndex, highIndex);

                if (diff <= 1)
                {
                    return value.Equals(ordered[lowIndex]) || (diff > 0 && value.Equals(ordered[highIndex]));
                }

                Int32 index = lowIndex + (diff + 1) / 2;
                Int32 compareValue = Comparer<T>.Default.Compare(value, ordered[index]);
                if (compareValue == 0) return true;
                if (compareValue < 0)
                {
                    highIndex = index - 1;
                }
                else
                {
                    lowIndex = index + 1;
                }
            }
        }
        */
    }
    public static class SingletonArray
    {
        //
        // NOTE: I could optimize this a little by sorting the existing arrays by length.
        //
        public static SingletonArray<T> Get<T>(params T[] array)
        {
            if (array == null)
            {
                return SingletonArray<T>.Null;
            }

            //
            // Check if this set is already created
            //
            lock (SingletonArray<T>.allCreated)
            {
                for (int i = 0; i < SingletonArray<T>.allCreated.Count; i++)
                {
                    SingletonArray<T> existingArray = SingletonArray<T>.allCreated[i];
                    if (array.Length == existingArray.array.Length)
                    {
                        Boolean isMatch = true;
                        for (int j = 0; j < array.Length; j++)
                        {
                            if (!array[j].Equals(existingArray.array[j]))
                            {
                                isMatch = false;
                                break;
                            }
                        }
                        if (isMatch) return existingArray;
                    }
                }

                //
                // Create a new ordered set
                //
                SingletonArray<T> newArray = new SingletonArray<T>(array);
                SingletonArray<T>.allCreated.Add(newArray);
                return newArray;
            }
        }
    }
    /// <summary>
    /// A structure that wraps an array.
    /// The structure guarantees that there is only one instance of every equivalent array.
    /// </summary>
    public struct SingletonArray<T> : IEnumerable<T>, IEquatable<SingletonArray<T>>
    {
        public static readonly SingletonArray<T> Null = new SingletonArray<T>(null);
        internal static readonly List<SingletonArray<T>> allCreated = new List<SingletonArray<T>>();

        // Note: can be null
        public readonly T[] array;
        internal SingletonArray(T[] array)
        {
            this.array = array;
        }
        public Boolean IsEmpty()
        {
            return array == null || array.Length <= 0;
        }
        public Boolean Equals(SingletonArray<T> other)
        {
            if (this.IsEmpty()) return other.IsEmpty();
            if (other.IsEmpty()) return false;

            // This comparison can be done because no identical array will be created
            return this.array == other.array;
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return array.GetGenericEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return array.GetGenericEnumerator();
        }
        public Boolean Contains(T t)
        {
            return IndexOf(t) >= 0;
        }
        /// <summary>
        /// Returns the index of the given parameter.
        /// </summary>
        /// <param name="t">The parameter to search for</param>
        /// <returns></returns>
        public Int32 IndexOf(T t)
        {
            if (array == null) return -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(t))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
