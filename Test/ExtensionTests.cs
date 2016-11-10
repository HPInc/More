// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using More;

namespace More
{
    [TestClass]
    public class ExtensionTests
    {
        void TestStringBuilderExtensions(String testString)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendUtf8(Encoding.UTF8.GetBytes(testString));
            Assert.AreEqual(testString, builder.ToString());
            //Console.WriteLine("TestString '{0}'", testString);
        }
        [TestMethod]
        public void TestStringBuilderExtensions()
        {
            TestStringBuilderExtensions("");
            TestStringBuilderExtensions("a");
            TestStringBuilderExtensions("Hello, World!");
            TestStringBuilderExtensions("\0");
            TestStringBuilderExtensions("\uFFFF");
            TestStringBuilderExtensions("a\uFF00b\u9876c");
        }
        void TestInvalidParseInt32(String testString)
        {
            Byte[] testBuffer = new Byte[testString.Length + 5];
            Int32 actualValue;
            UInt32 offset;

            Encoding.ASCII.GetBytes(testString, 0, testString.Length, testBuffer, 0);
            offset = testBuffer.TryParseInt32(0U, (uint)testBuffer.Length, out actualValue);
            Assert.AreEqual(0U, offset);

            Encoding.ASCII.GetBytes(testString, 0, testString.Length, testBuffer, 5);
            offset = testBuffer.TryParseInt32(5U, (uint)testBuffer.Length, out actualValue);
            Assert.AreEqual(0U, offset);
        }
        void TestInvalidParseUInt32(String testString)
        {
            Byte[] testBuffer = new Byte[testString.Length + 5];
            UInt32 actualValue;
            UInt32 offset;

            Encoding.ASCII.GetBytes(testString, 0, testString.Length, testBuffer, 0);
            offset = testBuffer.TryParseUInt32(0U, (uint)testBuffer.Length, out actualValue);
            Assert.AreEqual(0U, offset);

            Encoding.ASCII.GetBytes(testString, 0, testString.Length, testBuffer, 5);
            offset = testBuffer.TryParseUInt32(5U, (uint)testBuffer.Length, out actualValue);
            Assert.AreEqual(0U, offset);
        }
        void TestParseInt32(Int32 expectedValue, String testString)
        {
            Byte[] testBuffer = new Byte[testString.Length + 5];
            Int32 actualValue;
            UInt32 offset;
            
            Encoding.ASCII.GetBytes(testString, 0, testString.Length, testBuffer, 0);
            offset = testBuffer.TryParseInt32(0U, (uint)testBuffer.Length, out actualValue);
            Assert.AreEqual((uint)testString.Length, offset);
            Assert.AreEqual(expectedValue, actualValue);

            Encoding.ASCII.GetBytes(testString, 0, testString.Length, testBuffer, 5);
            offset = testBuffer.TryParseInt32(5U, (uint)testBuffer.Length, out actualValue);
            Assert.AreEqual((uint)(testString.Length + 5), offset);
            Assert.AreEqual(expectedValue, actualValue);
        }
        void TestParseUInt32(UInt32 expectedValue, String testString)
        {
            Byte[] testBuffer = new Byte[testString.Length + 5];
            UInt32 actualValue;
            UInt32 offset;

            Encoding.ASCII.GetBytes(testString, 0, testString.Length, testBuffer, 0);
            offset = testBuffer.TryParseUInt32(0U, (uint)testBuffer.Length, out actualValue);
            Assert.AreEqual((uint)testString.Length, offset);
            Assert.AreEqual(expectedValue, actualValue);

            Encoding.ASCII.GetBytes(testString, 0, testString.Length, testBuffer, 5);
            offset = testBuffer.TryParseUInt32(5U, (uint)testBuffer.Length, out actualValue);
            Assert.AreEqual((uint)(testString.Length + 5), offset);
            Assert.AreEqual(expectedValue, actualValue);
        }
        void TestParseNum<T>(ByteArrayParser<T> parser, T expectedValue, String testString)
        {
            Byte[] testBuffer = new Byte[testString.Length + 5];
            T actualValue;
            UInt32 offset;

            Encoding.ASCII.GetBytes(testString, 0, testString.Length, testBuffer, 0);
            offset = parser(testBuffer, 0U, (uint)testBuffer.Length, out actualValue);
            Assert.AreEqual((uint)testString.Length, offset);
            Assert.AreEqual(expectedValue, actualValue);

            Encoding.ASCII.GetBytes(testString, 0, testString.Length, testBuffer, 5);
            offset = parser(testBuffer, 5U, (uint)testBuffer.Length, out actualValue);
            Assert.AreEqual((uint)(testString.Length + 5), offset);
            Assert.AreEqual(expectedValue, actualValue);
        }
        [TestMethod]
        public void TestParseNumbers()
        {
            // Invalid Values
            TestInvalidParseUInt32("");
            TestInvalidParseInt32("");
            TestInvalidParseUInt32("A");
            TestInvalidParseInt32("A");
            TestInvalidParseInt32("2147483648");

            TestParseInt32(0, "0");
            TestParseUInt32(0, "0");

            TestParseInt32(2147483647, "2147483647");
            TestParseUInt32(2147483647, "2147483647");

            TestParseInt32(-2147483648, "-2147483648");
            TestParseUInt32(4294967295, "4294967295");

            TestParseNum<Byte>  (ByteString.ParseByte  , 0, "0");
            TestParseNum<UInt16>(ByteString.ParseUInt16, 0, "0");
            TestParseNum<UInt32>(ByteString.ParseUInt32, 0, "0");
            TestParseNum<Int32> (ByteString.ParseInt32 , 0, "0");

            TestParseNum<Byte>  (ByteString.ParseByte  , 255       , "255");
            TestParseNum<UInt16>(ByteString.ParseUInt16, 65535     , "65535");
            TestParseNum<UInt32>(ByteString.ParseUInt32, 4294967295, "4294967295");
            TestParseNum<Int32> (ByteString.ParseInt32 , 2147483647, "2147483647");
        }
        [TestMethod]
        public void TestStopwatchTicksPrettyTime()
        {
            Assert.AreEqual("0 microseconds", (0L).StopwatchTicksAsPrettyTime(0));
            Assert.AreEqual("0.0 microseconds", (0L).StopwatchTicksAsPrettyTime(1));

            Console.WriteLine("Frequency {0}", Stopwatch.Frequency);

            Assert.AreEqual("1 microseconds", ((Int64)(.000001D * (Double)Stopwatch.Frequency)).StopwatchTicksAsPrettyTime(0));
            Assert.AreEqual("2 microseconds", ((Int64)(.000002D * (Double)Stopwatch.Frequency)).StopwatchTicksAsPrettyTime(0));

            Assert.AreEqual("1 milliseconds", ((Int64)(.001D * (Double)Stopwatch.Frequency)).StopwatchTicksAsPrettyTime(0));

            Assert.AreEqual("1 seconds", ((Int64)(1D * (Double)Stopwatch.Frequency)).StopwatchTicksAsPrettyTime(0));
        }
        /*
        [TestMethod]
        public void SubstringMethods()
        {
            try { Assert.AreEqual(0, StringExtensions.Match("", 0, "")); Assert.Fail(); }
            catch (ArgumentOutOfRangeException) { }
            try { Assert.AreEqual(0, StringExtensions.Match("A", 1, "")); Assert.Fail(); }
            catch (ArgumentOutOfRangeException) { }
            try { Assert.AreEqual(0, StringExtensions.Match("AB", 2, "")); Assert.Fail(); }
            catch (ArgumentOutOfRangeException) { }

            Assert.AreEqual(0, StringExtensions.Match("A", 0, ""));
            Assert.AreEqual(0, StringExtensions.Match("A", 0, "X"));
            Assert.AreEqual(1, StringExtensions.Match("A", 0, "A"));
            Assert.AreEqual(1, StringExtensions.Match("A", 0, "ABCDEFG"));

            Assert.AreEqual(0, StringExtensions.Match("AB", 0, ""));
            Assert.AreEqual(0, StringExtensions.Match("AB", 0, "X"));
            Assert.AreEqual(0, StringExtensions.Match("AB", 0, "XX"));
            Assert.AreEqual(0, StringExtensions.Match("AB", 0, "XXX"));
            Assert.AreEqual(0, StringExtensions.Match("AB", 0, "XXXXXXXXXXX"));
            Assert.AreEqual(1, StringExtensions.Match("AB", 0, "A"));
            Assert.AreEqual(1, StringExtensions.Match("AB", 0, "AX"));
            Assert.AreEqual(1, StringExtensions.Match("AB", 0, "AXX"));
            Assert.AreEqual(2, StringExtensions.Match("AB", 0, "AB"));
            Assert.AreEqual(2, StringExtensions.Match("AB", 0, "ABX"));
            Assert.AreEqual(2, StringExtensions.Match("AB", 0, "ABXX"));

            Assert.AreEqual(0, StringExtensions.Match("ABC", 0, ""));
            Assert.AreEqual(0, StringExtensions.Match("ABC", 0, "X"));
            Assert.AreEqual(0, StringExtensions.Match("ABC", 0, "XX"));
            Assert.AreEqual(0, StringExtensions.Match("ABC", 0, "XXX"));
            Assert.AreEqual(0, StringExtensions.Match("ABC", 0, "XXXXXXXXXXX"));
            Assert.AreEqual(1, StringExtensions.Match("ABC", 0, "A"));
            Assert.AreEqual(1, StringExtensions.Match("ABC", 0, "AX"));
            Assert.AreEqual(1, StringExtensions.Match("ABC", 0, "AXX"));
            Assert.AreEqual(2, StringExtensions.Match("ABC", 0, "AB"));
            Assert.AreEqual(2, StringExtensions.Match("ABC", 0, "ABX"));
            Assert.AreEqual(2, StringExtensions.Match("ABC", 0, "ABXX"));
            Assert.AreEqual(3, StringExtensions.Match("ABC", 0, "ABC"));
            Assert.AreEqual(3, StringExtensions.Match("ABC", 0, "ABCX"));
            Assert.AreEqual(3, StringExtensions.Match("ABC", 0, "ABCXX"));


            Assert.AreEqual(0, StringExtensions.Match(" ABC", 1, ""));
            Assert.AreEqual(0, StringExtensions.Match(" ABC", 1, "X"));
            Assert.AreEqual(0, StringExtensions.Match(" ABC", 1, "XX"));
            Assert.AreEqual(0, StringExtensions.Match(" ABC", 1, "XXX"));
            Assert.AreEqual(0, StringExtensions.Match(" ABC", 1, "XXXXXXXXXXX"));
            Assert.AreEqual(1, StringExtensions.Match(" ABC", 1, "A"));
            Assert.AreEqual(1, StringExtensions.Match(" ABC", 1, "AX"));
            Assert.AreEqual(1, StringExtensions.Match(" ABC", 1, "AXX"));
            Assert.AreEqual(2, StringExtensions.Match(" ABC", 1, "AB"));
            Assert.AreEqual(2, StringExtensions.Match(" ABC", 1, "ABX"));
            Assert.AreEqual(2, StringExtensions.Match(" ABC", 1, "ABXX"));
            Assert.AreEqual(3, StringExtensions.Match(" ABC", 1, "ABC"));
            Assert.AreEqual(3, StringExtensions.Match(" ABC", 1, "ABCX"));
            Assert.AreEqual(3, StringExtensions.Match(" ABC", 1, "ABCXX"));


            Assert.AreEqual(0, StringExtensions.Match("  ABC", 2, ""));
            Assert.AreEqual(0, StringExtensions.Match("  ABC", 2, "X"));
            Assert.AreEqual(0, StringExtensions.Match("  ABC", 2, "XX"));
            Assert.AreEqual(0, StringExtensions.Match("  ABC", 2, "XXX"));
            Assert.AreEqual(0, StringExtensions.Match("  ABC", 2, "XXXXXXXXXXX"));
            Assert.AreEqual(1, StringExtensions.Match("  ABC", 2, "A"));
            Assert.AreEqual(1, StringExtensions.Match("  ABC", 2, "AX"));
            Assert.AreEqual(1, StringExtensions.Match("  ABC", 2, "AXX"));
            Assert.AreEqual(2, StringExtensions.Match("  ABC", 2, "AB"));
            Assert.AreEqual(2, StringExtensions.Match("  ABC", 2, "ABX"));
            Assert.AreEqual(2, StringExtensions.Match("  ABC", 2, "ABXX"));
            Assert.AreEqual(3, StringExtensions.Match("  ABC", 2, "ABC"));
            Assert.AreEqual(3, StringExtensions.Match("  ABC", 2, "ABCX"));
            Assert.AreEqual(3, StringExtensions.Match("  ABC", 2, "ABCXX"));
        }
        */
        [TestMethod]
        public void UnderscoreToCamelCaseTest()
        {
            Assert.AreEqual("TheString", "the_string".UnderscoreToCamelCase());
            Assert.AreEqual("TheString", "_the_string".UnderscoreToCamelCase());
            Assert.AreEqual("TheString", "_the_string_".UnderscoreToCamelCase());
            Assert.AreEqual("TheString", "the__string".UnderscoreToCamelCase());
            Assert.AreEqual("TheString", "__the_string".UnderscoreToCamelCase());
            Assert.AreEqual("TheString", "_the_string__".UnderscoreToCamelCase());
            Assert.AreEqual("TheString", "____the___string___".UnderscoreToCamelCase());

            Assert.AreEqual("TheString", "__THE_STRING".UnderscoreToCamelCase());
            Assert.AreEqual("TheString", "_tHe_sTrInG__".UnderscoreToCamelCase());
        }

        [TestMethod]
        public void CamelToUnderscoreCaseTest()
        {
            Assert.AreEqual("THE_STRING", "TheString".CamelToUpperUnderscoreCase());
            Assert.AreEqual("THE_STRING", "THeString".CamelToUpperUnderscoreCase());

            Assert.AreEqual("THE_STRING", "TheSTring".CamelToUpperUnderscoreCase());
            Assert.AreEqual("THE_STRING", "TheSTRing".CamelToUpperUnderscoreCase());
            Assert.AreEqual("THE_STRING", "TheSTRIng".CamelToUpperUnderscoreCase());
            Assert.AreEqual("THE_STRING", "TheSTRINg".CamelToUpperUnderscoreCase());
            Assert.AreEqual("THE_STRING", "TheSTRING".CamelToUpperUnderscoreCase());

            Assert.AreEqual("THE_STRING_AGAIN", "TheStringAgain".CamelToUpperUnderscoreCase());
        }

        public void ValidateLiteralString(String literal, String actual)
        {
            Int32 outLength;
            byte[] data = literal.ParseStringLiteral(0, out outLength);

            byte[] expectedBytes = Encoding.UTF8.GetBytes(actual);
            Assert.AreEqual(expectedBytes.Length, outLength);
            for (int i = 0; i < expectedBytes.Length; i++)
            {
                Assert.AreEqual(expectedBytes[i], data[i]);
            }
        }

        [TestMethod]
        public void ParseLiteralStringTest()
        {
            Int32 outLength;
            try
            {
                @"\".ParseStringLiteral(0, out outLength);
                Assert.Fail();
            }
            catch (FormatException) { }
            try
            {
                @"\e".ParseStringLiteral(0, out outLength);
                Assert.Fail();
            }
            catch (FormatException) { }
            try
            {
                @"\x0".ParseStringLiteral(0, out outLength);
                Assert.Fail();
            }
            catch (FormatException) { }

            ValidateLiteralString(@"\n", "\n");
            ValidateLiteralString(@"\n\\\0\a\r\t\v\x01", "\n\\\0\a\r\t\v\x01");
            ValidateLiteralString(@"hey \nwhat \\I am \0 testing\a\r\t\v \x67", "hey \nwhat \\I am \0 testing\a\r\t\v \x67");
        }


        public void ValidateStringArray(String[] actualStrings, params String[] expectedStrings)
        {
            Assert.AreEqual(expectedStrings.Length, actualStrings.Length);

            for (int i = 0; i < expectedStrings.Length; i++)
            {
                Assert.AreEqual(expectedStrings[i], actualStrings[i]);
            }
        }
        [TestMethod]
        public void SplitCorrectlyTest()
        {
            ValidateStringArray("1,2,3,4".SplitCorrectly(','), "1", "2", "3", "4");
            ValidateStringArray("1".SplitCorrectly(','), "1");
            ValidateStringArray("100".SplitCorrectly(','), "100");

            try
            {
                ValidateStringArray(",1".SplitCorrectly(','), "1");
                Assert.Fail();
            }
            catch (FormatException) { }
            try
            {
                ValidateStringArray(",".SplitCorrectly(','), "1");
                Assert.Fail();
            }
            catch (FormatException) { }
            try
            {
                ValidateStringArray("1,".SplitCorrectly(','), "1");
                Assert.Fail();
            }
            catch (FormatException) { }
        }
    }
}
