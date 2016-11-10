// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;

namespace More
{
    public class UniqueIndexObjectDictionary<ObjectType>
    {
        public interface IObjectGenerator
        {
            ObjectType GenerateObject(UInt32 uniqueIndex);
        }

        ObjectType[] objects;
        UInt32 nextIndex;

        readonly UInt32 extendLength;

        readonly SortedList<UInt32> sortedFreeIndices;
        readonly Dictionary<ObjectType, UInt32> objectToIndexDictionary;

        public UniqueIndexObjectDictionary(UInt32 initialFreeStackCapacity, UInt32 freeStackExtendLength,
            UInt32 initialTotalObjectsCapacity, UInt32 extendLength, IEqualityComparer<ObjectType> objectComparer)
        {
            this.objects = new ObjectType[initialTotalObjectsCapacity];
            nextIndex = 0;

            this.extendLength = extendLength;

            this.sortedFreeIndices = new SortedList<UInt32>(initialFreeStackCapacity, freeStackExtendLength, CommonComparisons.IncreasingUInt32);
            this.objectToIndexDictionary = new Dictionary<ObjectType, UInt32>(objectComparer);
        }
        public UniqueIndexObjectDictionary(UInt32 initialFreeStackCapacity, UInt32 freeStackExtendLength,
            UInt32 initialTotalObjectsCapacity, UInt32 extendLength)
        {
            this.objects = new ObjectType[initialTotalObjectsCapacity];
            nextIndex = 0;

            this.extendLength = extendLength;

            this.sortedFreeIndices = new SortedList<UInt32>(initialFreeStackCapacity, freeStackExtendLength, CommonComparisons.IncreasingUInt32);
            this.objectToIndexDictionary = new Dictionary<ObjectType, UInt32>();
        }
        private UInt32 GetFreeUniqueIndex()
        {
            if (sortedFreeIndices.count > 0) return sortedFreeIndices.GetAndRemoveLastElement();

            if (nextIndex >= UInt32.MaxValue)
                throw new InvalidOperationException(String.Format("The Free Stack Unique Object Tracker is tracking too many objects: {0}", nextIndex));

            // Make sure the local path buffer is big enough
            if (nextIndex >= objects.Length)
            {
                // extend local path array
                ObjectType[] newObjectsArray = new ObjectType[objects.Length + extendLength];
                Array.Copy(objects, newObjectsArray, objects.Length);
                objects = newObjectsArray;
            }

            UInt32 newestObjectIndex = nextIndex;
            nextIndex++;
            return newestObjectIndex;
        }
        public UInt32 GetUniqueIndexOf(ObjectType obj)
        {
            UInt32 uniqueIndex;
            if (objectToIndexDictionary.TryGetValue(obj, out uniqueIndex)) return uniqueIndex;

            uniqueIndex = GetFreeUniqueIndex();
            objects[uniqueIndex] = obj;
            objectToIndexDictionary.Add(obj, uniqueIndex);

            return uniqueIndex;
        }
        public ObjectType GetObject(UInt32 uniqueIndex)
        {
            return objects[uniqueIndex];
        }
        public UInt32 Add(ObjectType newObject)
        {
            UInt32 uniqueIndex = GetFreeUniqueIndex();

            objects[uniqueIndex] = newObject;
            objectToIndexDictionary.Add(newObject, uniqueIndex);

            return uniqueIndex;
        }
        public ObjectType GenerateNewObject(out UInt32 uniqueIndex, IObjectGenerator objectGenerator)
        {
            uniqueIndex = GetFreeUniqueIndex();

            ObjectType newObject = objectGenerator.GenerateObject(uniqueIndex);
            objects[uniqueIndex] = newObject;
            objectToIndexDictionary.Add(newObject, uniqueIndex);

            return newObject;
        }
        public void Free(UInt32 uniqueIndex)
        {
            ObjectType obj = objects[uniqueIndex];
            objectToIndexDictionary.Remove(obj);

            if (uniqueIndex == nextIndex - 1)
            {
                while (true)
                {
                    nextIndex--;
                    if (nextIndex <= 0) break;
                    if (sortedFreeIndices.count <= 0) break;
                    if (sortedFreeIndices.elements[sortedFreeIndices.count - 1] != nextIndex - 1) break;
                    sortedFreeIndices.count--;
                }
            }
            else
            {
                sortedFreeIndices.Add(uniqueIndex);
            }
        }
    }
}
