// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More
{
    /*
    public interface IFixedMap<T,K>
    {
        Boolean ContainsKey(T key);
        Boolean TryGetValue(T key, out K value);

        K this[T key] { get; }
        FixedMap<T, K>.KeyValuePair[] Entries { get; }
    }

    public static class FixedMap<T, K>
    {
        public class KeyValuePair
        {
            public readonly T key;
            public readonly K value;
            public KeyValuePair(T key, K value)
            {
                this.key = key;
                this.value = value;
            }
        }
        public static IFixedMap<T,K> CreateFastestMap(KeyValuePair[] entries)
        {
            return null;
        }
    }

    public class EmptyFixedMap<T,K> : IFixedMap<T,K>
    {
        static EmptyFixedMap<T,K> instance = null;
        public EmptyFixedMap<T,K> Instace { get { if( instance == null) instance = new EmptyFixedMap<T,K>(); return instance; } }
        private EmptyFixedMap() { }
        public Boolean ContainsKey(T key) { return false; }
        public Boolean TryGetValue(T key, out K value)
        {
            value = default(K);
            return false;
        }
        public K this[T key] { get { return default(K); } }
        public FixedMap<T, K>.KeyValuePair[] Entries
        {
            get { return null; }
        }
    }

    public class FixedMapOneElement<T, K> : IFixedMap<T, K>
    {
        public readonly FixedMap<T, K>.KeyValuePair pair;
        readonly FixedMap<T, K>.KeyValuePair[] pairAsArray;

        public FixedMapOneElement(FixedMap<T,K>.KeyValuePair pair)
        {
            this.pair = pair;
            this.pairAsArray = new FixedMap<T, K>.KeyValuePair[] { pair };
        }
        public Boolean ContainsKey(T key)
        {
            return key.Equals(pair.key);
        }
        public Boolean TryGetValue(T key, out K value)
        {
            if (key.Equals(pair.key))
            {
                value = pair.value;
                return true;
            }
            value = default(K);
            return false;
        }
        public K this[T key]
        {
            get { if (key.Equals(pair.key)) return pair.value; throw new KeyNotFoundException(); }
        }
        public FixedMap<T, K>.KeyValuePair[] Entries
        {
            get { return pairAsArray; }
        }
    }

    public class FixedMapUsingArray<T, K> : IFixedMap<T, K>
    {
        readonly FixedMap<T,K>.KeyValuePair[] pairs;
        public FixedMapUsingArray(FixedMap<T,K>.KeyValuePair[] pairs)
        {
            if(pairs == null) throw new ArgumentNullException("pairs");
            this.pairs = pairs;
        }
        public Boolean ContainsKey(T key)
        {
            for(int i = 0; i < pairs.Length; i++)
            {
                if(pairs[i].key.Equals(key)) return true;
            }
            return false;
        }
        public Boolean TryGetValue(T key, out K value)
        {
            for (int i = 0; i < pairs.Length; i++)
            {
                if (pairs[i].key.Equals(key))
                {
                    value = pairs[i].value;
                    return true;
                }
            }
            value = default(K);
            return false;
        }
        public K this[T key]
        {
            get
            {
                for (int i = 0; i < pairs.Length; i++)
                {
                    if (pairs[i].key.Equals(key))
                    {
                        return pairs[i].value;
                    }
                }
                throw new KeyNotFoundException(String.Format("Key '{0}' not found", key));
            }
        }
        public FixedMap<T, K>.KeyValuePair[] Entries
        {
            get { return pairs; }
        }
    }
    */


}
