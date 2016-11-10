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
    public class JsonTest
    {
        [TestMethod]
        public void TestGetUtf8ByteCount()
        {
            Byte[] encoded = new Byte[16];
            //Char[] chars = new Char[] { '"', '\\', 'u', 'x', 'x', 'x', 'x', '"' };
            //String hexMap = "0123456789ABCDEF";
            for (int i = 0; i < 1; i++)
            {
                //chars[3] = hexMap[i];
                for (int j = 0; j < 16; j++)
                {
                    //chars[4] = hexMap[j];
                    for (int k = 0; k < 16; k++)
                    {
                        //chars[5] = hexMap[k];
                        for (int l = 0; l < 16; l++)
                        {
                            //chars[6] = hexMap[l];

                            Char unicodeChar = (Char)(
                                (i << 12) |
                                (j << 8) |
                                (k << 4) |
                                l);
                            Assert.AreEqual(Encoding.UTF8.GetByteCount(new Char[] { unicodeChar }), Utf8.GetCharEncodeLength(unicodeChar));

                            UInt32 length = Utf8.EncodeChar(unicodeChar, encoded, 0);
                            String decoded = Encoding.UTF8.GetString(encoded, 0, (Int32)length);
                            Assert.AreEqual(unicodeChar, decoded[0]);
                            //Console.WriteLine("Decoded = '{0}'", decoded);
                            //Assert.AreEqual((Byte)((i << 4) | j), encoded
                        }
                    }
                }
            }
        }

        void TestConsumeObject(String json)
        {
            JsonByteArrayConsumer consumer = new JsonByteArrayConsumer(Encoding.UTF8.GetBytes(json));
            Assert.AreEqual((UInt32)json.Length, consumer.ConsumeObject(0));
        }

        [TestMethod]
        public void TestMethod1()
        {
            TestConsumeObject("{}");
            TestConsumeObject("{\"what\":\"hey\"}");

            TestConsumeObject("{\"key1\":null}");
            TestConsumeObject("{\"key2\":false}");
            TestConsumeObject("{\"key3\":true}");

            TestConsumeObject("{\"key3\":{}}");
            TestConsumeObject("{\"key3\":[]}");
            TestConsumeObject("{\"key3\":123}");
        }

        void TestFailConsumeObject(String json)
        {
            JsonByteArrayConsumer consumer = new JsonByteArrayConsumer(Encoding.UTF8.GetBytes(json));
            try
            {
                consumer.ConsumeObject(0);
                Assert.Fail(String.Format("Expected exception but did not get one for text '{0}'", json));
            }
            catch(JsonException e)
            {
                Console.WriteLine("[Test-Error] '{0}' threw {1}", json, e.Message);
            }
        }
        [TestMethod]
        public void TestFailures()
        {
            TestFailConsumeObject("");

            TestFailConsumeObject("{");
            TestFailConsumeObject(" {");
            TestFailConsumeObject(" { ");

            TestFailConsumeObject(" { key : null }");
            TestFailConsumeObject(" { , \"key\" : null }");

            TestFailConsumeObject(" { \"key\" , null }");
            TestFailConsumeObject(" { \"key\"  null }");

            TestFailConsumeObject(" { \"key\" : a }");
            TestFailConsumeObject(" { \"key\" : z }");
            TestFailConsumeObject(" { \"key\" : , }");

            TestFailConsumeObject(" { \"key\" : null");
            TestFailConsumeObject(" { \"key\" : null ");
        }

        void TestLookup(String json, IEnumerable<String> path, UInt32 valueOffset, UInt32 valueLimit)
        {
            JsonByteArrayConsumer consumer = new JsonByteArrayConsumer(Encoding.UTF8.GetBytes(json));
            var actualValueOffset = consumer.Lookup(path, 0);
            Assert.AreEqual(valueOffset, actualValueOffset);
            Assert.AreEqual(valueLimit, consumer.ConsumeValue(actualValueOffset));
        }
        [TestMethod]
        public void TestLookup()
        {
            //TestLookup("{}", null, 0);

            TestLookup("{\"key\":\"value\"}", new String[] { "key" }, 7, 14);
            TestLookup("{\"key\": 123}", new String[] { "key" }, 8, 11);

            TestLookup("{\"key\":\"value\", \"key2\":1234}", new String[] { "key2" }, 23, 27);
        }

        void TestParseString(String expected, String json)
        {
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            JsonByteArrayConsumer consumer = new JsonByteArrayConsumer(jsonBytes);
            String actual;
            Assert.AreEqual((UInt32)jsonBytes.Length, consumer.ParseString(out actual, 0));
            //Console.WriteLine("Json '{0}' Actual '{1}'", json, actual);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void TestParseString()
        {
            TestParseString("", @"""""");
            TestParseString("hello, world", @"""hello, world""");

            TestParseString("\"", @"""\""""");
            TestParseString("\\", @"""\\""");
            TestParseString("/" , @"""\/""");
            TestParseString("\b", @"""\b""");
            TestParseString("\f", @"""\f""");
            TestParseString("\n", @"""\n""");
            TestParseString("\r", @"""\r""");
            TestParseString("\t", @"""\t""");

            TestParseString("\"\\/\b\f\n\r\t", @"""\""\\\/\b\f\n\r\t""");

            TestParseString("what\"\\/is happening\b\f\nis this ok?\r\t", @"""what\""\\\/is happening\b\f\nis this ok?\r\t""");

            // Test \uXXXX
            TestParseString("\u0000", @"""\u0000""");
            TestParseString("\u0001", @"""\u0001""");
            TestParseString("\uab82", @"""\uab82""");

            TestParseString("\n\n\t\r\uab82\b\b\f\n\t", @"""\n\n\t\r\uab82\b\b\f\n\t""");

            TestParseString("\nhello\nagain\t\r\uab82what\b\b\f\n\t", @"""\nhello\nagain\t\r\uab82what\b\b\f\n\t""");

            TestParseString("\u007F", @"""\u007F""");

            TestParseString("\u0080", @"""\u0080""");
            TestParseString("\u07FF", @"""\u07FF""");
            Console.WriteLine();
            TestParseString("\u0800", @"""\u0800""");
            TestParseString("\uFFFF", @"""\uFFFF""");
            TestParseString("\uFFFF\uFFFF\u0000\u1010", @"""\uFFFF\uFFFF\u0000\u1010""");
        }
    }
}
