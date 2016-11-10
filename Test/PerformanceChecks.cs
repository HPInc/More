// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class PerformanceChecks
    {
        public static Boolean BruteForceContains<T>(T[] array, T value) where T : IComparable<T>
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(value))
                {
                    return true;
                }
            }
            return false;
        }
        [TestMethod]
        public void PerformanceTestSortedContains()
        {
            //PerformanceTestSortedContains(2, 1000000, new Int32[] { 0 });
            //PerformanceTestSortedContains(2, 1000000, new Int32[] { 0, 1 });
            //PerformanceTestSortedContains(2, 1000000, new Int32[] { 0, 1, 2 });
            //PerformanceTestSortedContains(2, 1000000, new Int32[] { 0, 1, 2, 3 });
            //PerformanceTestSortedContains(2, 1000000, new Int32[] { 0, 1, 2, 3, 4, 5, 6, 7 });

            //PerformanceTestSortedContains(2, 1000000, new Int32[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 });
            
            Int32[] array = new Int32[300];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }
            PerformanceTestSortedContains(2, 1000, array);
            
        }
        public void PerformanceTestSortedContains(UInt32 runCount, UInt32 loopCount, Int32[] sorted)
        {
            long before;

            //
            // Run once to jit the code
            //
            Boolean contains;
            contains = sorted.OrderedContains(0);
            contains = BruteForceContains(sorted, 0);

            for (UInt32 run = 0; run < runCount; run++)
            {
                Console.WriteLine("Run {0}", run);

                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < loopCount; i++)
                {
                    contains = BruteForceContains(sorted, sorted[0] - 1);
                    contains = BruteForceContains(sorted, sorted[sorted.Length - 1] + 1);
                    for (int sortedIndex = 0; sortedIndex < sorted.Length; sortedIndex++)
                    {
                        contains = BruteForceContains(sorted, sorted[sortedIndex]);
                    }
                }
                Console.WriteLine("NonSortedContains: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMilliseconds());
                Console.WriteLine("NonSortedContains: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < loopCount; i++)
                {
                    contains = sorted.OrderedContains(sorted[0] - 1);
                    contains = sorted.OrderedContains(sorted[sorted.Length - 1] + 1);
                    for (int sortedIndex = 0; sortedIndex < sorted.Length; sortedIndex++)
                    {
                        contains = sorted.OrderedContains(sorted[sortedIndex]);
                    }
                }
                Console.WriteLine("SortedContains: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMilliseconds());
                Console.WriteLine("SortedContains: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
            }
        }
        

        public static String JsonEncodeSlower(String str)
        {
            if (str == null) return "null";
            str = str.Trim();
            if (str.Length <= 0) return "\"\"";
            return String.Format("\"{0}\"", str.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t"));
        }
        [TestMethod]
        public void PerformanceTestJsonEncode()
        {
            //PerformanceTestJsonEncode(3, null);
            //PerformanceTestJsonEncode(3, "");
            //PerformanceTestJsonEncode(3, 10000, "Hello\n\twhat\"hey");
            PerformanceTestJsonEncode(3, 10000, "This string shouldn't need any encoding");
        }
        public void PerformanceTestJsonEncode(UInt32 runCount, UInt32 loopCount, String str)
        {
            long before;

            //
            // Run once to jit the code
            //
            str.JsonEncode();
            JsonEncodeSlower(str);

            for (UInt32 run = 0; run < runCount; run++)
            {
                Console.WriteLine("Run {0}", run);

                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < loopCount; i++)
                {
                    String a = str.JsonEncode();
                }
                Console.WriteLine("Fast: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMilliseconds());
                Console.WriteLine("Fast: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < loopCount; i++)
                {
                    String a = JsonEncodeSlower(str);
                }
                Console.WriteLine("Slow: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMilliseconds());
                Console.WriteLine("Slow: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
            }
        }


        /*
        struct ArrayAndUInts
        {
            Byte[] data;
            UInt32 a;
            UInt32 b;
        }
        struct LengthArray
        {
            Byte[] array;
            UInt32 offset;
            UInt32 length;
        }
        struct LimitArray
        {
            Byte[] array;
            UInt32 offset;
            UInt32 limit;
        }
        [TestMethod]
        public void PerformanceTestCastBetweenStructs()
        {
        }
        public void PerformanceTestCastBetweenStructs(UInt32 runCount)
        {
            long before;

            ArrayAndUInts arr = new ArrayAndUInts();
            LengthArray len = new LengthArray();
            LimitArray lim = new LimitArray();
            //
            // Run once to jit the code
            //
            len = (LengthArray)arr;


            for (UInt32 run = 0; run < runCount; run++)
            {
                Console.WriteLine("Run {0}", run);

                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < 10000000; i++)
                {
                }
                Console.WriteLine("None: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMilliseconds());
                Console.WriteLine("None: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < 10000000; i++)
                {
                    len = (LengthArray)arr;
                }
                Console.WriteLine("Cast: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMilliseconds());
                Console.WriteLine("Cast: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
            }
        }
        */












        struct XYStruct
        {
            public readonly Int32 a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z;
            public void TestMe()
            {
                Int32 x = a;
                Int32 y = b;
            }
        }
        XYStruct ReturnMe(XYStruct s) { return s; }
        void ReadVariables(XYStruct s)
        {
            Int32 x = s.a;
            Int32 y = s.b;
        }
        void ReadVariables(ref XYStruct s)
        {
            Int32 x = s.a;
            Int32 y = s.b;
        }
        class XYClass
        {
            public readonly Int32 a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z;
            public void TestMe()
            {
                Int32 x = a;
                Int32 y = b;
            }
        }
        XYClass ReturnMe(XYClass c) { return c; }
        void ReadVariables(XYClass c)
        {
            Int32 x = c.a;
            Int32 y = c.b;
        }


        [TestMethod]
        public void PerformanceTestReturnStructVsClass()
        {
            PerformanceTestReturnStructVsClass(3);
        }
        public void PerformanceTestReturnStructVsClass(UInt32 runCount)
        {
            long before;

            XYClass c = new XYClass();
            XYStruct s = new XYStruct();

            //
            // Run once to jit the code
            //
            c = ReturnMe(c);
            s = ReturnMe(s);

            for (UInt32 run = 0; run < runCount; run++)
            {
                Console.WriteLine("Run {0}", run);

                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < 10000000; i++)
                {
                    c = ReturnMe(c);
                }
                Console.WriteLine("Class: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMilliseconds());
                Console.WriteLine("Class: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < 10000000; i++)
                {
                    s = ReturnMe(s);
                }
                Console.WriteLine("Struct: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMilliseconds());
                Console.WriteLine("Struct: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
            }
        }
        [TestMethod]
        public void PerformanceTestStructVsClassMethod()
        {
            PerformanceTestStructVsClassMethod(3);
        }
        public void PerformanceTestStructVsClassMethod(UInt32 runCount)
        {
            long before;

            XYClass c = new XYClass();
            XYStruct s = new XYStruct();

            //
            // Run once to jit the code
            //
            c.TestMe();
            s.TestMe();

            for (UInt32 run = 0; run < runCount; run++)
            {
                Console.WriteLine("Run {0}", run);

                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < 10000000; i++)
                {
                    c.TestMe();
                }
                Console.WriteLine("Class: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMilliseconds());
                Console.WriteLine("Class: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < 10000000; i++)
                {
                    s.TestMe();
                }
                Console.WriteLine("Struct: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMilliseconds());
                Console.WriteLine("Struct: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
            }
        }
        [TestMethod]
        public void PerformanceTestStructRefVsClass()
        {
            PerformanceTestStructRefVsClass(3);
        }
        public void PerformanceTestStructRefVsClass(UInt32 runCount)
        {
            long before;
            
            XYClass c = new XYClass();
            XYStruct s = new XYStruct();

            //
            // Run once to jit the code
            //
            ReadVariables(c);
            ReadVariables(s);
            ReadVariables(ref s);

            for (UInt32 run = 0; run < runCount; run++)
            {
                Console.WriteLine("Run {0}", run);

                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < 10000000; i++)
                {
                    ReadVariables(c);
                }
                Console.WriteLine("Class: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMilliseconds());
                Console.WriteLine("Class: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < 10000000; i++)
                {
                    ReadVariables(s);
                }
                Console.WriteLine("Struct: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMilliseconds());
                Console.WriteLine("Struct: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < 10000000; i++)
                {
                    ReadVariables(ref s);
                }
                Console.WriteLine("RefStruct: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMilliseconds());
                Console.WriteLine("RefStruct: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
            }
        }

        [TestMethod]
        public void PerformanceTestSubstringCompare()
        {
            //PerformanceTestSubstringCompare("Hello there, this is an example string", "string", 31);
            //PerformanceTestSubstringCompare(3, "string", "string", 0);
            PerformanceTestSubstringCompare(6, "is this working string", "string", 17);
        }
        public void PerformanceTestSubstringCompare(UInt32 runCount, String haystack, String needle, Int32 offset)
        {
            long before;

            //
            // Run once to jit the code
            //
            haystack.Substring(offset).Equals(needle);
            haystack.SubstringEquals(offset, needle);

            for (UInt32 run = 0; run < runCount; run++)
            {
                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < 10000; i++)
                {
                    haystack.Substring(offset).Equals(needle);
                }
                Console.WriteLine("Framework: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMicroseconds());
                Console.WriteLine("Framework: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < 10000; i++)
                {
                    haystack.SubstringEquals(offset, needle);
                }
                Console.WriteLine("Custom: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsDoubleMicroseconds());
                Console.WriteLine("Custom: GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
            }
        }
        [TestMethod]
        public void PerformanceTestCaseVersusReturn()
        {
            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Double a = StopwatchExtensions.StopwatchTicksAsDoubleMilliseconds(0);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Double a = (Double)StopwatchExtensions.StopwatchTicksAsInt64Milliseconds(10);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
        }

        [TestMethod]
        public void PerformanceTestCharToUpperVsIsUpper()
        {
            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Char.ToUpper('c');
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Char.IsUpper('c');
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
        }
        /*
        [TestMethod]
        public void PerformanceTestSortVsOrderby()
        {
            PerformanceTestArrayBuilderInt32(500);
        }
        public static void PerformanceTestSortVsOrderby(List<Object> l)
        {
            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000; i++)
            {
                l.Sort();
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000; i++)
            {
                l.Ord
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
        }
        */
        [TestMethod]
        public void PerformanceTestArrayBuilder()
        {
            PerformanceTestArrayBuilderInt32(500);
            //PerformanceTestArrayBuilderObject(500);
        }
        public static void PerformanceTestArrayBuilderInt32(Int32 arrayLength)
        {
            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000; i++)
            {
                ArrayBuilder builder = new ArrayBuilder(typeof(Int32));
                for (int j = 0; j < arrayLength; j++)
                {
                    builder.Add(j);
                }
                Array array = builder.Build();
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000; i++)
            {
                GenericArrayBuilder<Int32> builder = new GenericArrayBuilder<Int32>();
                for (int j = 0; j < arrayLength; j++)
                {
                    builder.Add(j);
                }
                Array array = builder.Build();
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
        }
        public static void PerformanceTestArrayBuilderObject(Int32 arrayLength)
        {
            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000; i++)
            {
                ArrayBuilder builder = new ArrayBuilder(typeof(Object));
                for (int j = 0; j < arrayLength; j++)
                {
                    builder.Add(new Object());
                }
                Array array = builder.Build();
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000; i++)
            {
                GenericArrayBuilder<Object> builder = new GenericArrayBuilder<Object>();
                for (int j = 0; j < arrayLength; j++)
                {
                    builder.Add(new Object());
                }
                Array array = builder.Build();
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
        }


        [TestMethod]
        public void PerformanceTestBitArrays()
        {
            PerformanceTestBitArrays(100000);
        }
        public static void PerformanceTestBitArrays(Int32 length)
        {
            long before;

            System.Collections.BitArray bitArray = new System.Collections.BitArray(length);
            BetterBitArray betterBitArray = new BetterBitArray((UInt32)length);

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000; i++)
            {
                bitArray.SetAll(false);
                //bitArray.Set(0, true);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000; i++)
            {
                betterBitArray.SetAll(false);
                //betterBitArray.Assert(0);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
        }





        [TestMethod]
        public void PerformanceTestFixedDictionary()
        {
            //PerformanceTestFixedDictionary(new String[] { "abcdefg", "1234567", "what the heck" });
            //PerformanceTestFixedDictionary(10000000, new Byte[] { 0 });
            //PerformanceTestFixedDictionary(100000, new Byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            //PerformanceTestFixedDictionary(100000, new String[] { "apple", "banana", "orange" });
        }

        public static void PerformanceTestFixedDictionary<T>(Int32 iterations, T[] keys)
        {
            long before;
            
            Int32 value;

            /*
            FixedMap<T, Int32>.KeyValuePair[] pairs = new FixedMap<T,Int32>.KeyValuePair[keys.Length];
            for(int i = 0; i < keys.Length; i++)
            {
                pairs[i] = new FixedMap<T,int>.KeyValuePair(keys[i], i);
            }
            */
            Dictionary<T, Int32> variableDictionary = new Dictionary<T, Int32>();
            Dictionary<T, Int32> fixedDictionary = new Dictionary<T, Int32>(keys.Length);
            IDictionary<T, Int32> variableListDictionary = new SortedList<T, Int32>();
            IDictionary<T, Int32> fixedListDictionary = new SortedList<T, Int32>(keys.Length);
            //IFixedMap<T, Int32> fixedOneElementMap = new FixedMapOneElement<T, Int32>(pairs[0]);
            //IFixedMap<T, Int32> fixedArrayMap = new FixedMapUsingArray<T, Int32>(pairs);

            for (int i = 0; i < keys.Length; i++)
            {
                variableDictionary.Add(keys[i], i);
                fixedDictionary.Add(keys[i], i);
                variableListDictionary.Add(keys[i], i);
                fixedListDictionary.Add(keys[i], i);
            }

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < iterations; i++)
            {
                for(int j = 0; j < keys.Length; j++)
                {
                    value = variableDictionary[keys[j]];
                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < keys.Length; j++)
                {
                    value = fixedDictionary[keys[j]];
                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < keys.Length; j++)
                {
                    value = variableListDictionary[keys[j]];
                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < keys.Length; j++)
                {
                    value = fixedListDictionary[keys[j]];
                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
            /*
            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < keys.Length; j++)
                {
                    value = fixedOneElementMap[keys[j]];
                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < keys.Length; j++)
                {
                    value = fixedArrayMap[keys[j]];
                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
            */
        }



        [TestMethod]
        public void PerformanceTestClassVsInterface()
        {
            long before;

            List<Byte> theList = new List<byte>();
            IList<Byte> theInterfaceList = theList;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                theList.Add(0);
                theList.Clear();
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                theInterfaceList.Add(0);
                theInterfaceList.Clear();
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                theList.Add(0);
                theList.Clear();
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                theInterfaceList.Add(0);
                theInterfaceList.Clear();
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
        }


        [TestMethod]
        public void PerformanceTestByteArraySerialization()
        {
            long before;

            UInt32 value;
            Byte[] array = new Byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000000; i++)
            {
                value = (UInt32)(
                    (0xFF000000U & (array[0] << 24)) |
                    (0x00FF0000U & (array[1] << 16)) |
                    (0x0000FF00U & (array[2] << 8)) |
                    (0x000000FFU & (array[3])));
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000000; i++)
            {
                value = BitConverter.ToUInt32(array, 1);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000000; i++)
            {
                value = array.BigEndianReadUInt32(1);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }


        class SocketEqualityComparer : IEqualityComparer<Socket>
        {
            public bool Equals(Socket x, Socket y)
            {
                return ((IPEndPoint)x.LocalEndPoint).Port == ((IPEndPoint)y.LocalEndPoint).Port;
            }
            public int GetHashCode(Socket socket)
            {
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }

        [TestMethod]
        public void PerformanceTestSocketDictionary()
        {
            SocketDictionaryTest(20);
        }
        void SocketDictionaryTest(int socketCount)
        {
            Socket[] sockets = new Socket[socketCount];

            try
            {
                for(int i = 0; i < socketCount; i++)
                {
                    sockets[i] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sockets[i].Bind(new IPEndPoint(IPAddress.Any, 0));
                }

                Dictionary<Socket, Object> defaultDictionary = new Dictionary<Socket, Object>();
                Dictionary<Socket, Object> customDictionary = new Dictionary<Socket, Object>(new SocketEqualityComparer());

                long before;

                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < 100; i++)
                {
                    for(int j = 0; j < socketCount; j++)
                    {
                        defaultDictionary.Add(sockets[j], null);
                    }
                    defaultDictionary.Clear();
                }
                Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

                before = Stopwatch.GetTimestamp();
                for (int i = 0; i < 100; i++)
                {
                    for (int j = 0; j < socketCount; j++)
                    {
                        customDictionary.Add(sockets[j], null);
                    }
                    customDictionary.Clear();
                }
                Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
            }
            finally
            {
                for(int i = 0; i < sockets.Length; i++)
                {
                    try
                    {
                        sockets[i].Close();
                    }
                    catch(Exception){}
                }
            }
        }





        [TestMethod]
        public void PerformanceTestStringSerializationOptions()
        {
            StringSerializeOptionsPerformance("heytherefjdljakdfjlaskfjlk");
            //StringSerializeOptionsPerformance("heytherefj\\dlj\"akdfjlaskfjlk");
            //StringSerializeOptionsPerformance("heythejdkajfdlasjkfjdlajfka;fjaeijfoajifoasjefa;ojfeiajf8uf0-9u439u8f9upfuaq89fuapjfj3498439125oafjkeajpfje8aj98329tuilajjf9a8j34j;auf8u239j4fi9a83tra98ura9pyu3r84a9fja98rrefjdljakdfjlaskfjlk\\");
        }

        void StringSerializeOptionsPerformance(String testString)
        {
            Char[] stringEscapes = new Char[] {'"','\\'};

            long before;
            String a;


            //
            // This one is best when there are no escape characters, but is by far the worst if it there are escape characters near the end of a long string
            //
            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                if (testString.IndexOfAny(stringEscapes) < 0)
                {
                    a = "\"" + testString + "\"";
                }
                else
                {
                    a = "\"" + testString.Replace(@"\", @"\\").Replace("\"", "\\\"") + "\"";
                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());


            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                a = String.Format("\"{0}\"", testString.Replace(@"\", @"\\").Replace("\"", "\\\""));
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                a = '"' + testString.Replace(@"\", @"\\").Replace("\"", "\\\"") + '"';
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                a = "\"" + testString.Replace(@"\", @"\\").Replace("\"", "\\\"") + "\"";
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

        }

        [TestMethod]
        public void PerformanceConvertCharToString()
        {
            long before;
            String a;
            Char c = 'a';


            //
            // This one is best when there are no escape characters, but is by far the worst if it there are escape characters near the end of a long string
            //
            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000000; i++)
            {
                a = new String(c, 1);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());


            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000000; i++)
            {
                a = c.ToString();
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }


        [TestMethod]
        public void PerformanceCharToHexCode()
        {
            ConvertCharToHexCode('a');
            //Console.WriteLine(String.Format(@"\x{0:x4}", (ushort)'a'));
        }
        void ConvertCharToHexCode(Char c)
        {
            long before;
            String a;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                a = String.Format(@"\x{0:x4}", (ushort)c);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                a = @"\x" + String.Format("{0:x4}", (ushort)c);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                a = @"\x" + ((ushort)c).ToString("x4");
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }

        [TestMethod]
        public void PerformanceCastOrToString()
        {
            CastOrToString("hello");
        }
        public void CastOrToString(Object o)
        {
            long before;
            String a;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000000; i++)
            {
                a = o.ToString();
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000000; i++)
            {
                a = (String)o;
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

        }



        [TestMethod]
        public void PerformanceIsWhitespace()
        {
            IsWhitespace('a');
            IsWhitespace(' ');
            IsWhitespace('\n');
            IsWhitespace('\t');
            IsWhitespace('\f');
            IsWhitespace('\v');
            IsWhitespace('a');
        }
        public void IsWhitespace(Char c)
        {
            long before;
            Boolean test;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                test = Char.IsWhiteSpace(c);
            }
            Console.WriteLine("Char.IsWhitespace: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
            /*
            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                test = c.IsSosWhitespace();
            }
            Console.WriteLine("Sos.IsWhitespace : " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
            */
            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                test = (c == ' ' || c == '\t' || c == '\n' || c == '\r');
            }
            Console.WriteLine("Inline           : " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }






        [TestMethod]
        public void PerformanceTryGetDictionaryValue()
        {
            Dictionary<String, Object> d = new Dictionary<String,Object>();
            d.Add("key", new Object());

            TryGetDictionaryValue(d, "key");
            //TryGetDictionaryValue(d, "key1");
        }
        public void TryGetDictionaryValue(Dictionary<String,Object> d, String key)
        {
            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000000; i++)
            {
                Object o;
                d.TryGetValue(key, out o);
            }
            Console.WriteLine("TryGet: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000000; i++)
            {
                Object o = d[key];
            }
            Console.WriteLine("[]    : " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }

        [TestMethod]
        public void PerformanceTestSosTypeEquivalence()
        {
            SosEnumDefinition enumDefinition = new SosEnumDefinition();
            enumDefinition.Add("ajdkasjlfjdka", 23432);
            enumDefinition.Add("ferwr", 232);
            enumDefinition.Add("adfffcnz,.nvmzvnzvlvhzvzgfhioawehrhoahchZAfhrffdad", 25432);
            enumDefinition.Add("fdasdffaefkasjlfjasifjelasjfiasjlfjeiajflaejsifjlasjiefjlasdf", 2377);
            enumDefinition.Add("zcdjfkajfljeaijfeoiajfnlzjljfiljlaijlfeiajflajsefiljajfelaljefiajlsefxvzxcvzxvzcxv", 22);
            TestSosTypeEquivalence(enumDefinition);
        }
        public void TestSosTypeEquivalence(SosEnumDefinition enumDefinition)
        {
            String enumDefinitionString = enumDefinition.TypeDefinition();
            String enumDefinitionString2 = enumDefinition.TypeDefinition();


            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                enumDefinition.Equals(enumDefinition);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                enumDefinitionString.Equals(enumDefinitionString2);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }

        [TestMethod]
        public void PerformanceTestDictionaryIteration()
        {
            List<String> list = new List<String>();
            for (int i = 0; i < 65535; i++)
            {
                list.Add(new String((Char)(i + '~'), 10));
                //Console.WriteLine(list[i]);
            }

            TestDictionaryIteration(list);
        }
        public void TestDictionaryIteration(List<String> values)
        {
            Dictionary<String, String> dictionary = new Dictionary<String, String>();
            foreach (String value in values)
            {
                dictionary.Add(value, value);
            }


            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100; i++)
            {
                foreach (String value in values)
                {

                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100; i++)
            {
                foreach (String value in dictionary.Keys)
                {

                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }



        /*
        [TestMethod]
        public void TestFloatRecognition()
        {
            TestFloatRecognition("0", 0);
            TestFloatRecognition("-0.00E109", 0);
        }
        public void TestFloatRecognition(String numberString, Int32 offset)
        {
            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Sos.FloatNumberRegexBase10.Match(numberString, offset, numberString.Length - offset);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Sos.FloatLength(numberString, offset, numberString.Length);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }

        [TestMethod]
        public void TestWholeNumberRecognition()
        {
            TestWholeNumberRecognition("0", 0);
            TestWholeNumberRecognition("-010923085", 0);
        }
        public void TestWholeNumberRecognition(String numberString, Int32 offset)
        {
            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Sos.WholeNumberRegexBase10.Match(numberString, offset, numberString.Length - offset);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Sos.WholeNumberLength(numberString, offset, numberString.Length);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }
        [TestMethod]
        public void TestEnumRecognition()
        {
            TestEnumRecognition("0", 0);
            TestEnumRecognition("-010923085", 0);
            TestEnumRecognition("Apple_Oranges012", 0);
        }
        public void TestEnumRecognition(String numberString, Int32 offset)
        {
            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Sos.EnumValueNameRegex.Match(numberString, offset, numberString.Length - offset);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Sos.EnumLength(numberString, offset, numberString.Length);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }
        */
        static readonly Type[] StringParam = new Type[] { typeof(String) };
        [TestMethod]
        public void PerformanceTestParseNumber()
        {
            TestParseNumber(typeof(UInt16), "32");
            TestParseNumber(typeof(Double), "32.3829E10");
        }
        public void TestParseNumber(Type type, String s)
        {
            long before;


            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                if (type == typeof(SByte))
                {
                    SByte.Parse(s);
                }
                else if (type == typeof(Byte))
                {
                    Byte.Parse(s);
                }
                else if (type == typeof(Int16))
                {
                    Int16.Parse(s);
                }
                else if (type == typeof(UInt16))
                {
                    UInt16.Parse(s);
                }
                else if (type == typeof(Int32))
                {
                    Int32.Parse(s);
                }
                else if (type == typeof(UInt32))
                {
                    UInt32.Parse(s);
                }
                else if (type == typeof(Int64))
                {
                    Int64.Parse(s);
                }
                else if (type == typeof(UInt64))
                {
                    UInt64.Parse(s);
                }
                else if (type == typeof(Single))
                {
                    Single.Parse(s);
                }
                else if (type == typeof(Double))
                {
                    Double.Parse(s);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());



            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                MethodInfo parseMethod = type.GetMethod("Parse", StringParam);
                parseMethod.Invoke(null, new Object[] { s });
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }

        [TestMethod]
        public void PerformanceTestContructionWithReflecion()
        {
            //TestContructionWithReflecion(typeof(UInt16));
            //TestContructionWithReflecion(typeof(Double));
            TestContructionWithReflecion(typeof(TestClasses.ClassWithWeirdTypes));
        }
        public void TestContructionWithReflecion(Type type)
        {
            long before;
            Object obj;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                obj = Activator.CreateInstance(type);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                obj = FormatterServices.GetUninitializedObject(type);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }

        [TestMethod]
        public void PerformanceTestStringBuildling()
        {
            TestStringBuildling("abcdefg");
            TestStringBuildling("abcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefg");
            TestStringBuildling("abcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgab\"cdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefgabcdefg");
        }
        public void TestStringBuildling(String stringToCopy)
        {
            long before;
            String str;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                StringBuilder builder = new StringBuilder();
                Int32 stringIndex = 0;
                while(true)
                {
                    if(stringIndex >= stringToCopy.Length) break;
                    Char c = stringToCopy[stringIndex];
                    if(c == '"') break;
                    builder.Append(c);
                    stringIndex++;
                }
                str = builder.ToString();
            }
            Console.WriteLine("GC.Count Gen0: {0}, Gen1: {1}, Gen2: {2} StringBuilder: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds(),
                GCExtensions.CountDiff(0), GCExtensions.CountDiff(1), GCExtensions.CountDiff(2));

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                Int32 stringIndex = 0;
                while(true)
                {
                    if(stringIndex >= stringToCopy.Length) break;
                    Char c = stringToCopy[stringIndex];
                    if (c == '"') break;
                    stringIndex++;
                }

                Char[] chars = new Char[stringIndex];
                for(int j = 0; j < stringIndex; j++)
                {
                    chars[j] = stringToCopy[j];
                }
                str = new String(chars);
            }
            Console.WriteLine("GC.Count Gen0: {0}, Gen1: {1}, Gen2: {2} CharArray    : " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds(),
                GCExtensions.CountDiff(0), GCExtensions.CountDiff(1), GCExtensions.CountDiff(2));
        }
    }
}
