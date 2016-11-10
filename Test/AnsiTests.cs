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
    public class AnsiTests
    {
        class AnsiDecoderTestProcessor
        {
            public readonly Byte[][] expected;

            public Int32 callCount;

            public AnsiDecoderTestProcessor(Byte[][] expected)
            {
                this.expected = expected;
                this.callCount = 0;
            }
            public void Escape(Byte[] data, UInt32 offset, UInt32 length)
            {
                if (callCount >= expected.Length) Assert.Fail("Expected {0} calls but got more (this call is '{1}')",
                    expected.Length, Encoding.ASCII.GetString(data, (Int32)offset, (Int32)length));

                Byte[] nextExpected = expected[callCount];

                if (nextExpected[0] != 0x1B) Assert.Fail("Expected call {0} to be an escape but was data", callCount + 1);
                for (int i = 1; i < nextExpected.Length; i++)
                {
                    Assert.AreEqual(nextExpected[i], data[offset + i - 1]);
                }

                callCount++;
            }
            public void Data(Byte[] data, UInt32 offset, UInt32 length)
            {
                if (callCount >= expected.Length) Assert.Fail("Expected {0} calls but got more (this call is '{1}')",
                    expected.Length, Encoding.ASCII.GetString(data, (Int32)offset, (Int32)length));

                Byte[] nextExpected = expected[callCount];

                if (nextExpected[0] == 0x1B) Assert.Fail("Expected call {0} to be data but was an escape", callCount + 1);
                for (int i = 0; i < nextExpected.Length; i++)
                {
                    Assert.AreEqual(nextExpected[i], data[offset + i]);
                }

                callCount++;
            }
        }



        void TestDecoder(String dataString, params String[] expected)
        {
            Byte[][] expectedByteArrays = new Byte[expected.Length][];
            for(int i = 0; i < expectedByteArrays.Length; i++)
            {
                expectedByteArrays[i] = Encoding.ASCII.GetBytes(expected[i]);
            }

            AnsiDecoderTestProcessor testDecoder = new AnsiDecoderTestProcessor(expectedByteArrays);

            AnsiEscapeDecoder escapeDecoder = new AnsiEscapeDecoder(testDecoder.Escape,
                testDecoder.Data);

            Byte[] dataBytes = Encoding.ASCII.GetBytes(dataString);
            escapeDecoder.Decode(dataBytes, 0, (UInt32)dataBytes.Length);

            Assert.AreEqual(expected.Length, testDecoder.callCount);
        }


        [TestMethod]
        public void TestAnsiDecoder()
        {
            TestDecoder("a", new String[] { "a" });
            TestDecoder("\x1b@", new String[] { "\x1b@" });
            TestDecoder("\x1b[1234m", new String[] { "\x1b[1234m" });
            TestDecoder("a\x1b@", new String[] { "a", "\x1b@" });
            TestDecoder("a\x1b@b", new String[] { "a", "\x1b@", "b" });
            TestDecoder("\x1b@a\x1b@b", new String[] { "\x1b@", "a", "\x1b@", "b"});
            TestDecoder("\x1b@a\x1b@b\x1b[33;44@c", new String[] { "\x1b@", "a", "\x1b@", "b", "\x1b[33;44@", "c" });

            TestDecoder("hello   \x1b@abcd\x1b@ again", new String[] { "hello   ", "\x1b@", "abcd", "\x1b@", " again" });


            //TestDecoder("hello   \x1b@abcd\x1b@ again", new String[] { "hello   ", "\x1b@", "abcd", "\x1b@", " again" });




        }
    }
}
