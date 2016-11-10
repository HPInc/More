// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class OrderedSetTests
    {
        void TestArray<T>(T[] array)
        {
            OrderedSet<T> set;

            set = OrderedSet.VerifySortedAndGetSet(array);

            Array.Reverse(array);
            try
            {
                set = OrderedSet.VerifySortedAndGetSet(array);
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Got expected exception '{0}'", e.Message);
            }

            set = OrderedSet.SortArrayAndGetSet(array);
            set = OrderedSet.VerifySortedAndGetSet(array);
        }

        [TestMethod]
        public void TestVerifySorted()
        {
            OrderedSet<int> set;

            set = OrderedSet.VerifySortedAndGetSet(new int[] { });
            set = OrderedSet.VerifySortedAndGetSet(new int[] { 0});

            TestArray(new int[] { 0, 1 });
            TestArray(new int[] { 0, 1, 2 });
            TestArray(new int[] { 0, 1, 2, 10, 100, 1774 });


            try
            {
                set = OrderedSet.VerifySortedAndGetSet(new int[] { 0, 0 });
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Got expected exception '{0}'", e.Message);
            }

            try
            {
                set = OrderedSet.VerifySortedAndGetSet(new int[] { 0, 1, 1, 2 });
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Got expected exception '{0}'", e.Message);
            }
        }

        [TestMethod]
        public void TestSortArray()
        {
            OrderedSet<int> set;
            try
            {
                set = OrderedSet.SortArrayAndGetSet(new int[] { 0, 0 });
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Got expected exception '{0}'", e.Message);
            }
        }

        void TestSet(OrderedSet<Int32> set)
        {
            Assert.AreEqual(-1, set.IndexOf(set.orderedSet[0] - 1));
            for (int i = 0; i < set.orderedSet.Length; i++)
            {
                //Console.WriteLine("Testing IndexOf({0}) == {1}", set.IndexOf(set.orderedSet[i]), i);
                Assert.AreEqual(i, set.IndexOf(set.orderedSet[i]));
            }
            Assert.AreEqual(-1, set.IndexOf(set.orderedSet[set.orderedSet.Length-1] + 1));
        }
        [TestMethod]
        public void TestIndexOf()
        {
            TestSet(OrderedSet.VerifySortedAndGetSet(new Int32[] { 0, 1, 2, 3, 4 }));
            TestSet(OrderedSet.VerifySortedAndGetSet(new Int32[] { -14, 10, 20, 55, 98 }));
        }
    }
}
