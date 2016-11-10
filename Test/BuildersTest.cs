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
    /// Summary description for BuildersTest
    /// </summary>
    [TestClass]
    public class BuildersTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            ByteBuilder builder = new ByteBuilder(0);

            builder.AppendNumber(0, 10);
            Assert.AreEqual((Byte)'0', builder.bytes[0]);

            builder.Clear();
            builder.AppendNumber(1, 10);
            Assert.AreEqual((Byte)'1', builder.bytes[0]);

            builder.Clear();
            builder.AppendNumber(9, 10);
            Assert.AreEqual((Byte)'9', builder.bytes[0]);

            builder.Clear();
            builder.AppendNumber(10, 10);
            Assert.AreEqual((Byte)'1', builder.bytes[0]);
            Assert.AreEqual((Byte)'0', builder.bytes[1]);

            builder.Clear();
            builder.AppendNumber(48, 10);
            Assert.AreEqual((Byte)'4', builder.bytes[0]);
            Assert.AreEqual((Byte)'8', builder.bytes[1]);

            builder.Clear();
            builder.AppendNumber(975, 10);
            Assert.AreEqual((Byte)'9', builder.bytes[0]);
            Assert.AreEqual((Byte)'7', builder.bytes[1]);
            Assert.AreEqual((Byte)'5', builder.bytes[2]);

            builder.Clear();
            builder.AppendNumber(0xAF12, 16);
            Assert.AreEqual((Byte)'A', builder.bytes[0]);
            Assert.AreEqual((Byte)'F', builder.bytes[1]);
            Assert.AreEqual((Byte)'1', builder.bytes[2]);
            Assert.AreEqual((Byte)'2', builder.bytes[3]);
        }
    }
}
