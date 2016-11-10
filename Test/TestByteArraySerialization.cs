// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    /// <summary>
    /// Summary description for ByteArraySerialization
    /// </summary>
    [TestClass]
    public class TestByteArraySerialization
    {
        [TestMethod]
        public void TestBigEndianSerializationForEveryInt16()
        {
            Byte[] buffer = new Byte[2];

            for (int i = Int16.MinValue; i <= Int16.MaxValue; i++)
            {
                Int16 testValue = (Int16)i;
                buffer.BigEndianSetInt16(0, testValue);
                Assert.AreEqual((Byte)(testValue >> 8), buffer[0]);
                Assert.AreEqual((Byte)testValue, buffer[1]);
                Assert.AreEqual(testValue, buffer.BigEndianReadInt16(0));
            }
        }
        [TestMethod]
        public void TestBigEndianSerializationForEveryUInt16()
        {
            Byte[] buffer = new Byte[2];

            for (int i = UInt16.MinValue; i <= UInt16.MaxValue; i++)
            {
                UInt16 testValue = (UInt16)i;
                buffer.BigEndianSetUInt16(0, testValue);
                Assert.AreEqual((Byte)(testValue >> 8), buffer[0]);
                Assert.AreEqual((Byte)testValue, buffer[1]);
                Assert.AreEqual(testValue, buffer.BigEndianReadUInt16(0));
            }
        }
        [TestMethod]
        public void TestBigEndianSerializationForEveryInt24()
        {
            Byte[] buffer = new Byte[3];

            for (int i = Int24.MinValue; i <= Int24.MaxValue; i++)
            {
                buffer.BigEndianSetInt24(0, i);
                Assert.AreEqual((Byte)(i >> 16), buffer[0]);
                Assert.AreEqual((Byte)(i >> 8), buffer[1]);
                Assert.AreEqual((Byte)i, buffer[2]);
                Assert.AreEqual(i, buffer.BigEndianReadInt24(0));
            }
        }
        [TestMethod]
        public void TestBigEndianSerializationForEveryUInt24()
        {
            Byte[] buffer = new Byte[3];

            for (UInt32 i = UInt24.MinValue; i <= UInt24.MaxValue; i++)
            {
                buffer.BigEndianSetUInt24(0, i);
                Assert.AreEqual((Byte)(i >> 16), buffer[0]);
                Assert.AreEqual((Byte)(i >> 8), buffer[1]);
                Assert.AreEqual((Byte)i, buffer[2]);
                Assert.AreEqual(i, buffer.BigEndianReadUInt24(0));
            }
        }
        [TestMethod]
        public void TestBigEndianSerializationForLargeIntegerTypes()
        {
            Byte[] buffer = new Byte[256];

            // UInt32
            {
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("Testing UInt32...");
                Console.WriteLine("-----------------------------------------");
                UInt32[] testValues = new UInt32[] { 0, 0x80, 0x800000, 0x80000000, 0xFF, 0xFFFF, 0xFFFFFF, 0xFFFFFFF, 0xFFFFFFFF,
                    0x7F, 0x7FFFFF, 0x7FFFFFFF,  0x100, 0x80, 0xFF, 1, 2, 0x128EF92E, 0xFF00FF00, 0x12345678 };
                for (UInt32 i = 0; i < testValues.Length; i++)
                {
                    UInt32 testValue = testValues[i];
                    Console.WriteLine(testValue);
                    Console.WriteLine(testValue);
                    buffer.BigEndianSetUInt32(i, testValue);
                    Assert.AreEqual((Byte)(testValue >> 24), buffer[i]);
                    Assert.AreEqual((Byte)(testValue >> 16), buffer[i + 1]);
                    Assert.AreEqual((Byte)(testValue >> 8), buffer[i + 2]);
                    Assert.AreEqual((Byte)testValue, buffer[i + 3]);
                    Assert.AreEqual(testValue, buffer.BigEndianReadUInt32(i));
                }
            }

            // Int32
            {
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("Testing Int32...");
                Console.WriteLine("-----------------------------------------");
                Int32[] testValues = new Int32[] { 0, 0x80, 0x800000, -1, 0xFF, 0xFFFF, 0xFFFFFF, 0xFFFFFFF,
                    0x7F, 0x7FFFFF, 0x7FFFFFFF, 0x100, 0x80, 0xFF, 1, 2, 0x128EF92E, 0x7F00FF00, 0x12345678 };
                for (UInt32 i = 0; i < testValues.Length; i++)
                {
                    Int32 testValue = testValues[i];
                    Console.WriteLine(testValue);
                    buffer.BigEndianSetInt32(i, testValue);
                    Assert.AreEqual((Byte)(testValue >> 24), buffer[i]);
                    Assert.AreEqual((Byte)(testValue >> 16), buffer[i + 1]);
                    Assert.AreEqual((Byte)(testValue >> 8), buffer[i + 2]);
                    Assert.AreEqual((Byte)testValue, buffer[i + 3]);
                    Assert.AreEqual(testValue, buffer.BigEndianReadInt32(i));
                }
                for (UInt32 i = 0; i < testValues.Length; i++)
                {
                    Int32 testValue = -testValues[i];
                    Console.WriteLine(testValue);
                    buffer.BigEndianSetInt32(i, testValue);
                    Assert.AreEqual((Byte)(testValue >> 24), buffer[i]);
                    Assert.AreEqual((Byte)(testValue >> 16), buffer[i + 1]);
                    Assert.AreEqual((Byte)(testValue >> 8), buffer[i + 2]);
                    Assert.AreEqual((Byte)testValue, buffer[i + 3]);
                    Assert.AreEqual(testValue, buffer.BigEndianReadInt32(i));
                }
            }
            /*
            // UInt64
            {
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("Testing UInt64...");
                Console.WriteLine("-----------------------------------------");
                UInt64[] testValues = new UInt64[] { 0, 0x80, 0x800000, 0x80000000, 0xFF, 0xFFFF, 0xFFFFFF, 0xFFFFFFF, 0xFFFFFFFF,
                    0x7F, 0x7FFFFF, 0x7FFFFFFF,  0x100, 0x80, 0xFF, 1, 2, 0x128EF92E, 0xFF00FF00, 0x12345678 };
                for (int i = 0; i < testValues.Length; i++)
                {
                    UInt64 testValue = testValues[i];
                    Console.WriteLine(testValue);
                    Console.WriteLine(testValue);
                    buffer.BigEndianSetUInt64(i, testValue);
                    Assert.AreEqual((Byte)(testValue >> 24), buffer[i]);
                    Assert.AreEqual((Byte)(testValue >> 16), buffer[i + 1]);
                    Assert.AreEqual((Byte)(testValue >> 8), buffer[i + 2]);
                    Assert.AreEqual((Byte)testValue, buffer[i + 3]);
                    Assert.AreEqual(testValue, buffer.BigEndianReadUInt64(i));
                }
            }
            */
        }
        [TestMethod]
        public void TestHexStrings()
        {
            Byte[][] testArrays = new Byte[][] {
                new Byte[] {},
                new Byte[] {0},
                new Byte[] {0, 1, 2, 3},
                new Byte[] {0xFF, 0xFE},
                new Byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16},
            };

            for (int i = 0; i < testArrays.Length; i++)
            {
                Byte[] testArray = testArrays[i];

                String hexString = testArray.ToHexString(0, testArray.Length);
                Console.WriteLine("Testing {0}", hexString);

                Byte[] deserializeCopy = new Byte[testArray.Length];
                deserializeCopy.ParseHex(0, hexString, 0, hexString.Length);

                Sos.Diff(testArray, deserializeCopy);
            }
        }
    }
}
