// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class ParseTests
    {
        [TestMethod]
        public void ParseFieldNameTest()
        {
            ParseValidFieldNameTest("simple", "simple,");
            ParseValidFieldNameTest("simple", "simple}");
            ParseValidFieldNameTest("Simple", " Simple,");
            ParseValidFieldNameTest("simple", " simple}");
            ParseValidFieldNameTest("simple", " simple  ,");
            ParseValidFieldNameTest("simple", " simple  }");
            ParseValidFieldNameTest("simple", "\r\n\t \f\vsimple     ,");
            ParseValidFieldNameTest("simple", "\r\n\t \f\vsimple     }");

            ParseInvalidFieldNameTest("");
            ParseInvalidFieldNameTest(":");
            ParseInvalidFieldNameTest("\r\n\t \f\vsimple     ");
            ParseInvalidFieldNameTest("abcdefg");
            ParseInvalidFieldNameTest("ajfljda:");
            ParseInvalidFieldNameTest("a");
        }
        void ParseValidFieldNameTest(String expected, String testString)
        {
            String actual;
            Int32 offset = SosTypes.ParseName(testString, 0, '}', out actual);

            Assert.AreEqual(offset, testString.Length - 1);
            Assert.AreEqual(expected, actual);
        }
        void ParseInvalidFieldNameTest(String testString)
        {
            String actual;
            try
            {
                SosTypes.ParseName(testString, 0, '}', out actual);
                Assert.Fail("Expected FormatException");
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        [TestMethod]
        public void ParseTypeNameTest()
        {

            ParseValidTypeNameTest("sim.ple", "sim.ple:");
            ParseValidTypeNameTest("simple", "      simple:");
            ParseValidTypeNameTest("simple", "      simple    :");
            ParseValidTypeNameTest("sImple", "sImple     :");
            ParseValidTypeNameTest("simple", "\r\n\t \f\vsimple     :");
            ParseValidTypeNameTest("a", "a:");
            ParseValidTypeNameTest("a", " a:");
            ParseValidTypeNameTest("a", "a :");

            ParseInvalidTypeNameTest("");
            ParseInvalidTypeNameTest(":");
            ParseInvalidTypeNameTest("\r\n\t \f\vsimple     ");
            ParseInvalidTypeNameTest("abcdefg");
            ParseInvalidTypeNameTest("ajfljda,");
            ParseInvalidTypeNameTest("a,:");
        }
        void ParseValidTypeNameTest(String expected, String testString)
        {
            String actual;
            Int32 offset = SosTypes.ParseTypeName(testString, 0, ':', out actual);

            Assert.AreEqual(offset, testString.Length - 1);
            Assert.AreEqual(expected, actual);
        }
        void ParseInvalidTypeNameTest(String testString)
        {
            String actual;
            try
            {
                SosTypes.ParseTypeName(testString, 0, ':', out actual);
                Assert.Fail("Expected FormatException");
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
            }
        }


        [TestMethod]
        public void NextWhitespaceTest()
        {
            String[] validStrings = new String[] {
                "0 ",
                "9 ",
                "a ",
                "z ",
                "A ",
                "Z ",
                "\"\" ",
                "\"\\\"\" ",
                "\"\\\\\\\"\" ",
                "[ ",
                "{ ",
            };

            for (int i = 0; i < validStrings.Length; i++)
            {
                String validString = validStrings[i];

                Int32 offset = Sos.NextNonQuotedWhitespace(validString, 0);
                Assert.AreEqual(validString.Length - 1, offset);
            }
        }
        [TestMethod]
        public void TestInvalidStringsForNextWhitespace()
        {
            String[] invalidStrings = new String[] {
                "\" ",
                "\"\\\\\\\" ",
            };

            for (int i = 0; i < invalidStrings.Length; i++)
            {
                String invalidString = invalidStrings[i];
                try
                {
                    Sos.NextNonQuotedWhitespace(invalidString, 0);
                    Assert.Fail("Expected format exception but didn't get one for '{0}'", invalidString);
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Expected format exception for '{0}': {1}", invalidString, e.Message);
                }
            }
        }
    }
}
