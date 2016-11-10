// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;

namespace More
{
    public class ObjectManager<ObjectType> : Dictionary<ObjectType, UInt32>
    {
        public interface IObjectFactory
        {
            ObjectType GenerateObject();
        }

        readonly IObjectFactory factory;
        readonly UInt32 extendLength;

        ObjectType[] objects;
        UInt32 nextIndex;

        readonly SortedList<UInt32> sortedFreeIndices;

        public ObjectManager(IObjectFactory factory, UInt32 initialCapacity, UInt32 extendLength)
        {
            if (extendLength <= 0) throw new ArgumentOutOfRangeException("Extend length cannot be less than 1");

            this.factory = factory;
            this.extendLength = extendLength;

            this.objects = new ObjectType[initialCapacity];
            nextIndex = 0;

            this.sortedFreeIndices = new SortedList<UInt32>(initialCapacity, extendLength, CommonComparisons.IncreasingUInt32);
        }
        public Boolean ThereExistsAllocatedObjectsThatAreFree()
        {
            return sortedFreeIndices.count > 0;
        }
        public UInt32 AllocatedObjectsCount()
        {
            return nextIndex;
        }
        public UInt32 ReservedObjectsCount()
        {
            return nextIndex - sortedFreeIndices.count;
        }
        public ObjectType Reserve()
        {
            if (sortedFreeIndices.count > 0)
            {
                UInt32 index = sortedFreeIndices.GetAndRemoveLastElement();
                return objects[index];
            }
            if (nextIndex >= UInt32.MaxValue)
                throw new InvalidOperationException(String.Format("The Free Stack Unique Object Tracker is tracking too many objects: {0}", nextIndex));

            if (nextIndex >= objects.Length)
            {
                // extend local path array
                ObjectType[] newObjectsArray = new ObjectType[objects.Length + extendLength];
                Array.Copy(objects, newObjectsArray, objects.Length);
                objects = newObjectsArray;
            }

            UInt32 newestObjectIndex = nextIndex;
            nextIndex++;

            ObjectType newestObject = factory.GenerateObject();
            objects[newestObjectIndex] = newestObject;
            Add(newestObject, newestObjectIndex);

            return newestObject;

        }
        public void Release(ObjectType obj)
        {
            UInt32 index;
            if (!TryGetValue(obj, out index))
                throw new InvalidOperationException(String.Format("Object '{0}' was not found", obj));

            if (index == nextIndex - 1)
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
                sortedFreeIndices.Add(index);
            }
        }
    }
}
