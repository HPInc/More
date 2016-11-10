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
    /// Summary description for BufferTests
    /// </summary>
    [TestClass]
    public class BufferTests
    {
        /*
        [TestMethod]
        public void TestEqualsString()
        {
            Byte[] testBuffer = new Byte[32];
            testBuffer[0] = (Byte)' ';
            testBuffer[1] = (Byte)'a';
            testBuffer[2] = (Byte)'B';

            Slice<Byte> segment = new Slice<Byte>(testBuffer);

            segment.offset = 0;
            segment.length = 0;
            Assert.IsTrue(segment.EqualsString("", false));

            segment.offset = 0;
            segment.length = 1;
            Assert.IsTrue(segment.EqualsString(" ", false));

            segment.offset = 1;
            segment.length = 2;
            Assert.IsTrue(segment.EqualsString("aB", false));

            segment.offset = 0;
            segment.length = 3;
            Assert.IsFalse(segment.EqualsString(" AB", false));
            Assert.IsFalse(segment.EqualsString(" ab", false));

            Assert.IsTrue(segment.EqualsString(" AB", true));
            Assert.IsTrue(segment.EqualsString(" ab", true));
        }
        */
        [TestMethod]
        public void TestUtf8EqualsString()
        {
            Byte[] testBuffer = new Byte[32];
            testBuffer[0] = (Byte)' ';
            testBuffer[1] = (Byte)'a';
            testBuffer[2] = (Byte)'B';
            Assert.IsTrue(Utf8.EqualsString(testBuffer, 0, 0, "", false));

            Assert.IsTrue(Utf8.EqualsString(testBuffer, 0, 1, " ", false));

            Assert.IsTrue(Utf8.EqualsString(testBuffer, 1, 3, "aB", false));

            Assert.IsFalse(Utf8.EqualsString(testBuffer, 0, 3, " AB", false));
            Assert.IsFalse(Utf8.EqualsString(testBuffer, 0, 3, " ab", false));

            Assert.IsTrue(Utf8.EqualsString(testBuffer, 0, 3, " AB", true));
            Assert.IsTrue(Utf8.EqualsString(testBuffer, 0, 3, " ab", true));
        }

        [TestMethod]
        public void TestPeel()
        {
            Byte[] testBuffer = new Byte[32];
            testBuffer[ 0] = (Byte)' ';
            testBuffer[ 1] = (Byte)' ';
            testBuffer[ 2] = (Byte)'\t';
            testBuffer[ 3] = (Byte)' ';
            testBuffer[ 4] = (Byte)'h';
            testBuffer[ 5] = (Byte)'e';
            testBuffer[ 6] = (Byte)'l';
            testBuffer[ 7] = (Byte)'l';
            testBuffer[ 8] = (Byte)'o';
            testBuffer[ 9] = (Byte)'\t';
            testBuffer[10] = (Byte)' ';
            testBuffer[11] = (Byte)'w';
            testBuffer[12] = (Byte)'o';
            testBuffer[13] = (Byte)'r';
            testBuffer[14] = (Byte)'l';
            testBuffer[15] = (Byte)'d';
            testBuffer[16] = (Byte)' ';
            testBuffer[17] = (Byte)'\t';

            Slice<Byte> segment = new Slice<Byte>();
            OffsetLength peeledSegment;

            segment.length = 0;
            peeledSegment = Slice<Byte>.PeelAscii(ref segment);
            Assert.AreEqual(0U, segment.length);
            Assert.AreEqual(0U, peeledSegment.length);

            segment.array = testBuffer;
            segment.offset = 0;
            segment.length = 4;
            peeledSegment = Slice<Byte>.PeelAscii(ref segment);
            Assert.AreEqual(4U, segment.offset);
            Assert.AreEqual(0U, segment.length);
            Assert.AreEqual(4U, peeledSegment.offset);
            Assert.AreEqual(0U, peeledSegment.length);

            segment.offset = 2;
            segment.length = 2;
            peeledSegment = Slice<Byte>.PeelAscii(ref segment);
            Assert.AreEqual(4U, segment.offset);
            Assert.AreEqual(0U, segment.length);
            Assert.AreEqual(4U, peeledSegment.offset);
            Assert.AreEqual(0U, peeledSegment.length);

            segment.offset = 3;
            segment.length = 2;
            peeledSegment = Slice<Byte>.PeelAscii(ref segment);
            Assert.AreEqual(5U, segment.offset);
            Assert.AreEqual(0U, segment.length);
            Assert.AreEqual(4U, peeledSegment.offset);
            Assert.AreEqual(1U, peeledSegment.length);

            segment.offset = 1;
            segment.length = 13;
            peeledSegment = Slice<Byte>.PeelAscii(ref segment);
            Assert.AreEqual(11U, segment.offset);
            Assert.AreEqual(3U, segment.length);
            Assert.AreEqual(4U, peeledSegment.offset);
            Assert.AreEqual(5U, peeledSegment.length);

            segment.offset = 2;
            segment.length = 16;
            peeledSegment = Slice<Byte>.PeelAscii(ref segment);
            Assert.AreEqual(11U, segment.offset);
            Assert.AreEqual(7U, segment.length);
            Assert.AreEqual(4U, peeledSegment.offset);
            Assert.AreEqual(5U, peeledSegment.length);
        }

        [TestMethod]
        public void TestPeelUtf8()
        {
            Byte[] testBuffer = new Byte[32];
            testBuffer[0] = (Byte)' ';
            testBuffer[1] = (Byte)' ';
            testBuffer[2] = (Byte)'\t';
            testBuffer[3] = (Byte)' ';
            testBuffer[4] = (Byte)'h';
            testBuffer[5] = (Byte)'e';
            testBuffer[6] = (Byte)'l';
            testBuffer[7] = (Byte)'l';
            testBuffer[8] = (Byte)'o';
            testBuffer[9] = (Byte)'\t';
            testBuffer[10] = (Byte)' ';
            testBuffer[11] = (Byte)'w';
            testBuffer[12] = (Byte)'o';
            testBuffer[13] = (Byte)'r';
            testBuffer[14] = (Byte)'l';
            testBuffer[15] = (Byte)'d';
            testBuffer[16] = (Byte)' ';
            testBuffer[17] = (Byte)'\t';

            OffsetLimit peeled;
            SliceByLimit<Byte> testSegment;
            testSegment.array = testBuffer;

            testSegment.offset = 0;
            testSegment.limit = 0;
            peeled = Utf8.Peel(ref testSegment);
            Assert.AreEqual( 0U, testSegment.offset);
            Assert.AreEqual( 0U, peeled.limit);

            testSegment.offset = 0;
            testSegment.limit = 4;
            peeled = Utf8.Peel(ref testSegment);
            Assert.AreEqual( 4U, testSegment.offset);
            Assert.AreEqual( 4U, peeled.offset);
            Assert.AreEqual( 4U, peeled.limit);

            testSegment.offset = 2;
            testSegment.limit = 4;
            peeled = Utf8.Peel(ref testSegment);
            Assert.AreEqual( 4U, testSegment.offset);
            Assert.AreEqual( 4U, peeled.offset);
            Assert.AreEqual( 4U, peeled.limit);

            testSegment.offset = 3;
            testSegment.limit = 5;
            peeled = Utf8.Peel(ref testSegment);
            Assert.AreEqual( 5U, testSegment.offset);
            Assert.AreEqual( 4U, peeled.offset);
            Assert.AreEqual( 5U, peeled.limit);

            testSegment.offset = 1;
            testSegment.limit = 14;
            peeled = Utf8.Peel(ref testSegment);
            Assert.AreEqual(10U, testSegment.offset);
            Assert.AreEqual( 4U, peeled.offset);
            Assert.AreEqual( 9U, peeled.limit);

            testSegment.offset = 2;
            testSegment.limit = 18;
            peeled = Utf8.Peel(ref testSegment);
            Assert.AreEqual(10U, testSegment.offset);
            Assert.AreEqual( 4U, peeled.offset);
            Assert.AreEqual( 9U, peeled.limit);
        }
    }
}
