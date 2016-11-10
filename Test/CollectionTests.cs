// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class CollectionTests
    {
        void AssertEqual(int[] expected, SortedNumberSet actual)
        {
            int i = 0;
            if (expected.Length != actual.Count)
            {
                Console.WriteLine("Expected: {0}", expected.SerializeObject());
                Console.WriteLine("Actual  : {0}", actual.RangeString());
                Assert.AreEqual(expected.Length, actual.Count);
            }
            foreach (var value in actual)
            {
                if (expected[i] != value)
                {
                    Console.WriteLine("Expected: {0}", expected.SerializeObject());
                    Console.WriteLine("Actual  : {0}", actual.RangeString());
                    Assert.Fail("Expected value at index {0} to be {1}, but is {2}", i, expected[i], value);
                }
                i++;
            }
        }

        [TestMethod]
        public void SortedNumberSetBadCallsTest()
        {
            SortedNumberSet set = new SortedNumberSet();
            try
            {
                set.AddRange(10, 10);
                Assert.Fail("Expected exception but got none");
            }
            catch (ArgumentException)
            {
            }
            try
            {
                set.AddRange(10, 9);
                Assert.Fail("Expected exception but got none");
            }
            catch (ArgumentException)
            {
            }
        }
        [TestMethod]
        public void SortedNumberSetAddTest()
        {
            SortedNumberSet set = new SortedNumberSet();

            set.Clear();
            AssertEqual(new int[0], set);
            set.Add(10);
            AssertEqual(new int[] { 10 }, set);
            set.Add(10);
            AssertEqual(new int[] { 10 }, set);
            set.Add(11);
            AssertEqual(new int[] { 10, 11 }, set);
            set.Add(11);
            AssertEqual(new int[] { 10, 11 }, set);
            set.Add(13);
            AssertEqual(new int[] { 10, 11, 13 }, set);
            set.Add(13);
            AssertEqual(new int[] { 10, 11, 13 }, set);
            set.Add(5);
            AssertEqual(new int[] { 5, 10, 11, 13 }, set);
            set.Add(9);
            AssertEqual(new int[] { 5, 9, 10, 11, 13 }, set);

        }
        [TestMethod]
        public void SortedNumberSetAddRange1Test()
        {
            SortedNumberSet set = new SortedNumberSet();

            set.Clear();
            AssertEqual(new int[0], set);
            set.AddRange(10, 11);
            AssertEqual(new int[] { 10 }, set);
            set.AddRange(10, 11);
            AssertEqual(new int[] { 10 }, set);
            set.AddRange(11, 12);
            AssertEqual(new int[] { 10, 11 }, set);
            set.AddRange(11, 12);
            AssertEqual(new int[] { 10, 11 }, set);
            set.AddRange(13, 14);
            AssertEqual(new int[] { 10, 11, 13 }, set);
            set.AddRange(13, 14);
            AssertEqual(new int[] { 10, 11, 13 }, set);
            set.AddRange(5, 6);
            AssertEqual(new int[] { 5, 10, 11, 13 }, set);
            set.AddRange(9, 10);
            AssertEqual(new int[] { 5, 9, 10, 11, 13 }, set);
        }
        [TestMethod]
        public void SortedNumberSetAddRange2Test()
        {
            SortedNumberSet set = new SortedNumberSet();

            set.Clear();
            AssertEqual(new int[0], set);
            set.AddRange(10, 12);
            AssertEqual(new int[] { 10, 11 }, set);
            set.AddRange(7, 10);
            AssertEqual(new int[] { 7, 8, 9, 10, 11 }, set);
            set.AddRange(5, 9);
            AssertEqual(new int[] { 5, 6, 7, 8, 9, 10, 11 }, set);
            set.AddRange(3, 15);
            AssertEqual(new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, set);
            set.AddRange(20, 22);
            AssertEqual(new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 20, 21 }, set);
            set.AddRange(2, 23);
            AssertEqual(new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22 }, set);

            set.Clear();
            AssertEqual(new int[0], set);
            set.AddRange(10, 12);
            AssertEqual(new int[] { 10, 11 }, set);
            set.AddRange(14, 17);
            AssertEqual(new int[] { 10, 11, 14, 15, 16 }, set);
            set.AddRange(25, 29);
            AssertEqual(new int[] { 10, 11, 14, 15, 16, 25, 26, 27, 28 }, set);
            set.AddRange(5, 35);
            AssertEqual(new int[] {
                5, 6, 7, 8, 9, 10, 11, 12, 13,
                14, 15, 16, 17, 18, 19, 20, 21,
                22, 23, 24, 25, 26, 27, 28, 29,
                30, 31, 32, 33, 34}, set);

            set.Clear();
            AssertEqual(new int[0], set);
            set.AddRange(10, 12);
            AssertEqual(new int[] { 10, 11 }, set);
            set.AddRange(14, 17);
            AssertEqual(new int[] { 10, 11, 14, 15, 16 }, set);
            set.AddRange(25, 29);
            AssertEqual(new int[] { 10, 11, 14, 15, 16, 25, 26, 27, 28 }, set);
            set.AddRange(5, 24);
            AssertEqual(new int[] {
                    5, 6, 7, 8, 9, 10, 11, 12, 13,
                    14, 15, 16, 17, 18, 19, 20, 21,
                    22, 23, 25, 26, 27, 28}, set);

            for (int i = 0; i <= 3; i++)
            {
                set.Clear();
                AssertEqual(new int[0], set);
                set.AddRange(10, 12);
                AssertEqual(new int[] { 10, 11 }, set);
                set.AddRange(14, 17);
                AssertEqual(new int[] { 10, 11, 14, 15, 16 }, set);
                set.AddRange(25, 29);
                AssertEqual(new int[] { 10, 11, 14, 15, 16, 25, 26, 27, 28 }, set);
                set.AddRange(5, 25 + i);
                AssertEqual(new int[] {
                     5,  6,  7,  8,  9, 10, 11, 12,
                    13, 14, 15, 16, 17, 18, 19, 20,
                    21, 22, 23, 24, 25, 26, 27, 28}, set);
            }

            set.Clear();
            set.AddRange(40, 47);
            AssertEqual(new int[] { 40, 41, 42, 43, 44, 45, 46 }, set);
            for (int i = 1; i <= 7; i++)
            {
                set.AddRange(40, 40 + i);
                AssertEqual(new int[] { 40, 41, 42, 43, 44, 45, 46 }, set);
            }
            set.AddRange(40, 48);
            AssertEqual(new int[] { 40, 41, 42, 43, 44, 45, 46, 47 }, set);

        }
        [TestMethod]
        public void SortedNumberSetAddRange3Test()
        {
            SortedNumberSet set = new SortedNumberSet();

            for (int i = 0; i <= 2; i++)
            {
                set.Clear();
                AssertEqual(new int[0], set);
                set.AddRange(10, 12);
                AssertEqual(new int[] { 10, 11 }, set);
                set.AddRange(6, 8);
                set.AddRange(2, 6 + i);
                AssertEqual(new int[] { 2, 3, 4, 5, 6, 7, 10, 11 }, set);
            }

            set.Clear();
            AssertEqual(new int[0], set);
            set.AddRange(10, 12);
            AssertEqual(new int[] { 10, 11 }, set);
            set.AddRange(20, 24);
            AssertEqual(new int[] { 10, 11, 20, 21, 22, 23 }, set);
            set.AddRange(8, 13);
            AssertEqual(new int[] { 8, 9, 10, 11, 12, 20, 21, 22, 23 }, set);

            set.Clear();
            AssertEqual(new int[0], set);
            set.AddRange(10, 12);
            AssertEqual(new int[] { 10, 11 }, set);
            set.AddRange(20, 24);
            AssertEqual(new int[] { 10, 11, 20, 21, 22, 23 }, set);
            set.AddRange(8, 19);
            AssertEqual(new int[] { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 20, 21, 22, 23 }, set);


            for (int i = 0; i <= 4; i++)
            {
                set.Clear();
                AssertEqual(new int[0], set);
                set.AddRange(10, 12);
                AssertEqual(new int[] { 10, 11 }, set);
                set.AddRange(20, 24);
                AssertEqual(new int[] { 10, 11, 20, 21, 22, 23 }, set);
                set.AddRange(8, 20 + i);
                AssertEqual(new int[] { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, set);
            }

            set.Clear();
            AssertEqual(new int[0], set);
            set.AddRange(10, 12);
            AssertEqual(new int[] { 10, 11 }, set);
            set.AddRange(20, 24);
            AssertEqual(new int[] { 10, 11, 20, 21, 22, 23 }, set);
            set.AddRange(8, 25);
            AssertEqual(new int[] { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, set);

            set.Clear();
            AssertEqual(new int[0], set);
            set.AddRange(10, 12);
            AssertEqual(new int[] { 10, 11 }, set);
            set.AddRange(20, 24);
            AssertEqual(new int[] { 10, 11, 20, 21, 22, 23 }, set);
            set.AddRange(8, 26);
            AssertEqual(new int[] { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, set);
        }
    }
}
