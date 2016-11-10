// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{

    public class SortedListExtensions<T>
    {
        public void Print(SortedList<T> list)
        {
            for (int print = 0; print < list.count; print++)
            {
                Console.WriteLine("{0,20}", list.elements[print]);
            }
            Console.WriteLine();
        }
    }

    [TestClass]
    public class SortedListTest
    {
        SortedListExtensions<Int32> int32Printer = new SortedListExtensions<int>();
        SortedListExtensions<String> stringPrinter = new SortedListExtensions<String>();



        private void AssertIncreasing(SortedList<Int32> list)
        {
            for (int i = 0; i < list.count - 1; i++)
            {
                Assert.IsTrue(list.elements[i] <= list.elements[i + 1]);
            }
        }
        private void AssertDecreasing(SortedList<Int32> list)
        {
            for (int i = 0; i < list.count - 1; i++)
            {
                Assert.IsTrue(list.elements[i] >= list.elements[i + 1]);
            }
        }

        [TestMethod]
        public void TestIncreasingSortedList()
        {
            Random generator = new Random();



            SortedList<Int32> increasingList = new SortedList<Int32>(0, 1, CommonComparisons.IncreasingInt32);

            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    increasingList.Add(generator.Next());
                    AssertIncreasing(increasingList);
                }

                int32Printer.Print(increasingList);

                //
                // remove some
                //
                int removeCount = generator.Next((Int32)increasingList.count);
                Console.WriteLine("remove count {0}", removeCount);
                for (int j = 0; j < removeCount; j++)
                {
                    int removeIndex = generator.Next((Int32)increasingList.count - 1);

                    increasingList.Remove(increasingList.elements[removeIndex]);
                    AssertIncreasing(increasingList);
                }

                int32Printer.Print(increasingList);

                increasingList.Clear();
                Assert.AreEqual(0U, increasingList.count);

            }

        }
        [TestMethod]
        public void TestDecreasingSortedList()
        {
            Random generator = new Random();

            SortedList<Int32> decreasingList = new SortedList<Int32>(0, 1, CommonComparisons.DecreasingInt32);

            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    decreasingList.Add(generator.Next());
                    AssertDecreasing(decreasingList);
                }

                int32Printer.Print(decreasingList);

                //
                // remove some
                //
                int removeCount = generator.Next((Int32)decreasingList.count);
                Console.WriteLine("remove count {0}", removeCount);
                for (int j = 0; j < removeCount; j++)
                {
                    int removeIndex = generator.Next((Int32)decreasingList.count - 1);

                    decreasingList.Remove(decreasingList.elements[removeIndex]);
                    AssertDecreasing(decreasingList);
                }

                int32Printer.Print(decreasingList);

                decreasingList.Clear();
                Assert.AreEqual(0U, decreasingList.count);
            }
        }


        [TestMethod]
        public void TestRemoveFromStart()
        {
            SortedList<Int32> list = new SortedList<Int32>(0, 1, CommonComparisons.IncreasingInt32);

            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(1);
            list.Add(2);
            list.Add(3);

            int32Printer.Print(list);

            list.RemoveFromStart(1);
            Console.WriteLine("---------------------");
            int32Printer.Print(list);


            list.RemoveFromStart(3);
            Console.WriteLine("---------------------");
            int32Printer.Print(list);
        }


        [TestMethod]
        public void TestRemoveWithStrings()
        {
            SortedList<String> list = new SortedList<String>(0, 1, StringComparer.OrdinalIgnoreCase.Compare);

            list.Add("c");
            list.Add("a");
            list.Add("b");
            list.Add("a");
            list.Add("b");
            list.Add("b");
            list.Add("c");

            AssertList(new String[] { "a", "a", "b", "b", "b", "c", "c" }, list);

            list.RemoveFromStart(1);
            AssertList(new String[] { "a", "b", "b", "b", "c", "c" }, list);

            list.RemoveFromStart(3);
            AssertList(new String[] { "b", "c", "c" }, list);


            list.Add("z");
            list.Add("z");
            list.Add("a");

            AssertList(new String[] { "a", "b", "c", "c", "z", "z" }, list);

            list.RemoveFromStart(6);
            AssertList(new String[] {}, list);


            list.Add("q");
            list.Add("y");
            list.Add("d");

            AssertList(new String[] { "d", "q", "y"}, list);

            list.Remove("q");
            AssertList(new String[] { "d", "y" }, list);

            Assert.AreEqual("y", list.GetAndRemoveLastElement());
            AssertList(new String[] { "d" }, list);
        }

        [TestMethod]
        public void TestAddStrings()
        {
            SortedList<String> list = new SortedList<String>(0, 100, StringComparer.OrdinalIgnoreCase.Compare);

            AssertList(new String[] { }, list);

            list.Add("d");
            AssertList(new String[] { "d" }, list);

            list.Add("a");
            AssertList(new String[] { "a", "d" }, list);

            list.Add("a");
            AssertList(new String[] { "a", "a", "d" }, list);

            list.Add("d");
            AssertList(new String[] { "a", "a", "d", "d" }, list);

            list.Add("e");
            AssertList(new String[] { "a", "a", "d", "d", "e" }, list);

            list.Clear();
            AssertList(new String[] { }, list);

            list.Add("d");
            AssertList(new String[] { "d" }, list);

            list.Add("e");
            AssertList(new String[] { "d", "e" }, list);
        }
        void AssertList(String[] expected, SortedList<String> actual)
        {
            Assert.AreEqual((UInt32)expected.Length, actual.count);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual.elements[i], String.Format("at index {0}", i));
            }
            for (UInt32 i = actual.count; i < actual.elements.Length; i++)
            {
                Assert.IsNull(actual.elements[i]);
            }
        }
    }
}
