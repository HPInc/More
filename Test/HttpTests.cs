// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using More.Net;

namespace More
{
    [TestClass]
    public class HttpTests
    {
        [TestMethod]
        public void TestUrlDecode()
        {
            StringBuilder expectedBuilder = new StringBuilder();
            StringBuilder encodedBuilder = new StringBuilder();
            for(Byte b = 0; b < 128; b++)
            {
                expectedBuilder.Append((Char)b);
                expectedBuilder.Append((Char)b);

                encodedBuilder.Append('%');
                encodedBuilder.Append(b.ToString("x2"));
                encodedBuilder.Append('%');
                encodedBuilder.Append(b.ToString("X2"));
            }

            String expectedString = expectedBuilder.ToString();
            String encodedString = encodedBuilder.ToString();

            Console.WriteLine("Testing: {0} > {1}", encodedString, expectedString);

            Assert.AreEqual(expectedString, Http.UrlDecode(encodedString));
        }
    }
}
