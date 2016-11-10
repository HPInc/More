// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class SortedSetTests
    {
        [TestMethod]
        public void TestSortedContins()
        {
            Int32[] sorted;

            sorted = new Int32[0];

            Assert.AreEqual(false, sorted.OrderedContains(0));

            sorted = new Int32[] { 0 };
            Assert.AreEqual(false, sorted.OrderedContains(-1));
            Assert.AreEqual(true, sorted.OrderedContains(0));
            Assert.AreEqual(false, sorted.OrderedContains(1));

            sorted = new Int32[] { 0, 1 };
            Assert.AreEqual(false, sorted.OrderedContains(-1));
            Assert.AreEqual(true, sorted.OrderedContains(0));
            Assert.AreEqual(true, sorted.OrderedContains(1));
            Assert.AreEqual(false, sorted.OrderedContains(2));

            sorted = new Int32[] { 0, 1, 2 };
            Assert.AreEqual(false, sorted.OrderedContains(-1));
            Assert.AreEqual(true, sorted.OrderedContains(0));
            Assert.AreEqual(true, sorted.OrderedContains(1));
            Assert.AreEqual(true, sorted.OrderedContains(2));
            Assert.AreEqual(false, sorted.OrderedContains(3));

            sorted = new Int32[] { 0, 1, 2, 3 };
            Assert.AreEqual(false, sorted.OrderedContains(-1));
            Assert.AreEqual(true, sorted.OrderedContains(0));
            Assert.AreEqual(true, sorted.OrderedContains(1));
            Assert.AreEqual(true, sorted.OrderedContains(2));
            Assert.AreEqual(true, sorted.OrderedContains(3));
            Assert.AreEqual(false, sorted.OrderedContains(4));
        }
        [TestMethod]
        public void TestCombinedLength()
        {
            Assert.AreEqual(0U, OrderedSets.CombinedLength((Int32[])null, null));
            Assert.AreEqual(0U, OrderedSets.CombinedLength(new Int32[0], null));
            Assert.AreEqual(0U, OrderedSets.CombinedLength((Int32[])null, new Int32[0]));
            Assert.AreEqual(0U, OrderedSets.CombinedLength(new Int32[0], new Int32[0]));

            Assert.AreEqual(1U, OrderedSets.CombinedLength(new Int32[] { 0 }, null));
            Assert.AreEqual(1U, OrderedSets.CombinedLength(new Int32[] { 0 }, new Int32[0]));
            Assert.AreEqual(1U, OrderedSets.CombinedLength(null, new Int32[] { 0 }));
            Assert.AreEqual(1U, OrderedSets.CombinedLength(new Int32[0], new Int32[] { 0 }));
            Assert.AreEqual(1U, OrderedSets.CombinedLength(new Int32[] { 0 }, new Int32[] { 0 }));

            Assert.AreEqual(2U, OrderedSets.CombinedLength(new Int32[] { 0 }, new Int32[] { 1 }));
            Assert.AreEqual(2U, OrderedSets.CombinedLength(new Int32[] { 1 }, new Int32[] { 0 }));
            Assert.AreEqual(2U, OrderedSets.CombinedLength(new Int32[] { 0, 1 }, new Int32[] { 0 }));
            Assert.AreEqual(2U, OrderedSets.CombinedLength(new Int32[] { 0, 1 }, new Int32[] { 1 }));
            Assert.AreEqual(2U, OrderedSets.CombinedLength(new Int32[] { 0 }, new Int32[] { 0, 1 }));
            Assert.AreEqual(2U, OrderedSets.CombinedLength(new Int32[] { 1 }, new Int32[] { 0, 1 }));
        }
        [TestMethod]
        public void TestCombined()
        {
            /*
             * TODO: Write these tests
             */
        }

        void AssertEquals<T>(T[] expected, T[] actual)
        {
            if (expected == null)
            {
                if (actual != null) Assert.Fail("Expected null but got {0}", Sos.SerializeObject(actual));
            }
            else if (actual == null)
            {
                Assert.Fail("Expected {0} but got null", Sos.SerializeObject(expected));
            }
            else
            {
                if (expected.Length != actual.Length)
                    Assert.Fail("Expected array of length {0} but actual was length {1} (expected {2}, actual {3})",
                        expected.Length, actual.Length, Sos.SerializeObject(expected), Sos.SerializeObject(actual));
                for (int i = 0; i < expected.Length; i++)
                {
                    if (!expected[i].Equals(actual[i]))
                        Assert.Fail("Array mismatch at index {0} (expected {1}, actual {2})", i, Sos.SerializeObject(expected), Sos.SerializeObject(actual));
                }
            }
        }

        [TestMethod]
        public void TestCombined2()
        {
            OrderedSet<Int32> First = OrderedSet.SortArrayAndGetSet(new Int32[] { 0, 1, 3});

            AssertEquals(new Int32[] { 0, 1, 3 }, First.orderedSet);
            
            OrderedSet<Int32> Second = First.Combine(OrderedSet.SortArrayAndGetSet(new Int32[] { 0, 2 }));

            AssertEquals(new Int32[] { 0, 1, 2, 3 }, Second.orderedSet);
        }
    }
}
