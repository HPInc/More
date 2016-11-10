// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class LineParserTest
    {
        [TestMethod]
        public void TestMethod()
        {
            Encoding encoding = Encoding.ASCII;
            LineParser lineParser = new LineParser(encoding, 3);

            lineParser.Add(encoding.GetBytes("abcd\n"));
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());

            lineParser.Add(encoding.GetBytes("abcd\r\n"));
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());

            lineParser.Add(encoding.GetBytes("abcd\nefgh\r\n"));
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.AreEqual("efgh", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());

            lineParser.Add(encoding.GetBytes("abcd\r\nefghijkl"));
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());
            lineParser.Add(encoding.GetBytes("\n"));
            Assert.AreEqual("efghijkl", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());

            lineParser.Add(encoding.GetBytes("abcd\n"));
            lineParser.Add(encoding.GetBytes("abcd\r\n"));
            lineParser.Add(encoding.GetBytes("abcd\n"));
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());

            lineParser.Add(encoding.GetBytes("a"));
            Assert.IsNull(lineParser.GetLine());
            lineParser.Add(encoding.GetBytes("bc"));
            Assert.IsNull(lineParser.GetLine());
            lineParser.Add(encoding.GetBytes("d"));
            Assert.IsNull(lineParser.GetLine());
            lineParser.Add(encoding.GetBytes("\r\ntu"));
            lineParser.Add(encoding.GetBytes("v"));
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());
            lineParser.Add(encoding.GetBytes("\r"));
            Assert.IsNull(lineParser.GetLine());
            lineParser.Add(encoding.GetBytes("\n"));
            Assert.AreEqual("tuv", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());
        }
    }
}
