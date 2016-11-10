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
    public class EncodingTest
    {
        void TestDecodeUtf8(String s, params UInt32[] expectedChars)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            TestDecodeUtf8(bytes, 0, (UInt32)bytes.Length, expectedChars);
        }
        unsafe void TestDecodeUtf8(Byte[] s, UInt32 start, UInt32 limit, params UInt32[] expectedChars)
        {
            foreach(var expected in expectedChars)
            {
                if (start >= limit)
                {
                    Assert.Fail("Expected more decoded utf8 chars but input ended");
                }
                UInt32 encodedLength = limit - start;
                {
                    var saveStart = start;
                    var decoded = Utf8.Decode(s, ref start, limit);
                    if (decoded != expected)
                    {
                        Assert.Fail("decodeUtf8: Expected '{0}' 0x{1} but decoded '{2}' 0x{3}",
                            expected, expected, decoded, decoded);
                    }
                    Console.WriteLine("decodeUtf8('{0}')", decoded);
                }

                fixed (byte* sPointer = s)
                {
                    Utf8Pointer pointer = new Utf8Pointer(sPointer);
                    Utf8Pointer pointerLimit = new Utf8Pointer(sPointer + encodedLength);
                    var decoded = Utf8.Decode(ref pointer, pointerLimit);
                    if (decoded != expected)
                    {
                        Assert.Fail("decodeUtf8: Expected '{0}' 0x{1} but decoded '{2}' 0x{3}",
                            expected, expected, decoded, decoded);
                    }
                }

            }

            if (start != limit)
            {
                Assert.Fail("Expected {0} characters but didn't get enough", expectedChars.Length);
            }
        }
        void TestInvalidUtf8(Utf8ExceptionType expectedError, params Byte[] s)
        {
            UInt32 start = 0;
            UInt32 limit = (UInt32)s.Length;
            try
            {
                var decoded = Utf8.Decode(s, ref start, limit);
                Assert.Fail("expected error '%s' but no error was thrown", expectedError);
            } catch(Utf8Exception e)
            {
                Assert.AreEqual(expectedError, e.type, String.Format("expected error '{0}' but got '{1}'", expectedError, e.type));
            }

            Console.WriteLine("got expected error '{0}'", expectedError);
        }


        [TestMethod]
        public void TestUtf8()
        { 
            char[] testString = new char[256];
            Byte[] testStringBytes = new Byte[32];
            UInt32[] expectedCharsBuffer = new UInt32[256];

            TestInvalidUtf8(Utf8ExceptionType.StartedInsideCodePoint, 0x80);
            TestInvalidUtf8(Utf8ExceptionType.MissingBytes, 0xC0);
            TestInvalidUtf8(Utf8ExceptionType.MissingBytes, 0xE0, 0x80);
            
            for(Byte c = 0; c <= 0x7F; c++) {
                testStringBytes[0] = c;
                TestDecodeUtf8(testStringBytes, 0, 1, c);
            }

            TestDecodeUtf8("\u0000", 0x0000);
            TestDecodeUtf8("\u0001", 0x0001);

            TestDecodeUtf8("\u00a9", 0xa9);
            TestDecodeUtf8("\u00b1", 0xb1);
            TestDecodeUtf8("\u02c2", 0x02c2);

            TestDecodeUtf8("\u0080", 0x80);
            TestDecodeUtf8("\u07FF", 0x7FF);

            TestDecodeUtf8("\u0800", 0x800);
            TestDecodeUtf8("\u7fff", 0x7FFF);
            TestDecodeUtf8("\u8000", 0x8000);
            TestDecodeUtf8("\uFFFD", 0xFFFD);
            TestDecodeUtf8("\uFFFE", 0xFFFE);
            TestDecodeUtf8("\uFFFF", 0xFFFF);

            TestDecodeUtf8("\U00010000", 0x10000);
            TestDecodeUtf8("\U00100000", 0x00100000);
            TestDecodeUtf8("\U0010FFFF", 0x0010FFFF);
            //TestDecodeUtf8("\U00110000", 0x00110000); // DMD doesn't like this code point
        }
    }
}
