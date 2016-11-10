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
    /// Summary description for Asn1Tests
    /// </summary>
    [TestClass]
    public class Asn1Tests
    {
        [TestMethod]
        public void IntegerOctetCountTests()
        {
            Assert.AreEqual(1U, Asn1.IntegerOctetCount(0));
            Assert.AreEqual(1U, Asn1.IntegerOctetCount(1));
            Assert.AreEqual(1U, Asn1.IntegerOctetCount(2));
            Assert.AreEqual(1U, Asn1.IntegerOctetCount(-1));
            Assert.AreEqual(1U, Asn1.IntegerOctetCount(-2));

            Assert.AreEqual(1U, Asn1.IntegerOctetCount(0x7F));
            Assert.AreEqual(2U, Asn1.IntegerOctetCount(0x80));
            Assert.AreEqual(2U, Asn1.IntegerOctetCount(0x7FFF));
            Assert.AreEqual(3U, Asn1.IntegerOctetCount(0x8000));
            Assert.AreEqual(3U, Asn1.IntegerOctetCount(0x7FFFFF));
            Assert.AreEqual(4U, Asn1.IntegerOctetCount(0x800000));
            Assert.AreEqual(4U, Asn1.IntegerOctetCount(0x7FFFFFFF));

            Assert.AreEqual(1U, Asn1.IntegerOctetCount(-0x80));
            Assert.AreEqual(2U, Asn1.IntegerOctetCount(-0x81));
            Assert.AreEqual(2U, Asn1.IntegerOctetCount(-0x8000));
            Assert.AreEqual(3U, Asn1.IntegerOctetCount(-0x8001));
            Assert.AreEqual(3U, Asn1.IntegerOctetCount(-0x800000));
            Assert.AreEqual(4U, Asn1.IntegerOctetCount(-0x800001));
            Assert.AreEqual(4U, Asn1.IntegerOctetCount(-0x7FFFFFFF));
            Assert.AreEqual(4U, Asn1.IntegerOctetCount((Int32)(-0x80000000)));

        }

        [TestMethod]
        public void ParseByteTests()
        {
            Byte[] buffer = new Byte[3];

            // Test valid octet lengths
            buffer[0] = 1;

            for (int i = 0; i < 255; i++)
            {
                buffer[1] = (Byte)i;

                UInt32 offset = 0;
                Byte parsedValue = Asn1.ParseIntegerToByte(buffer, ref offset);

                Assert.AreEqual(2U, offset);
                Assert.AreEqual((Byte)i, parsedValue);
            }

            // Test invalid octet lengths
            buffer[0] = 0;
            try
            {
                UInt32 offset = 0;
                Asn1.ParseIntegerToByte(buffer, ref offset);
                Assert.Fail("Expected exception that did not occur");
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught expected exception: {0}", e.Message);
            }

            // Test invalid octet lengths
            buffer[0] = 2;
            try
            {
                UInt32 offset = 0;
                Asn1.ParseIntegerToByte(buffer, ref offset);
                Assert.Fail("Expected exception that did not occur");
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught expected exception: {0}", e.Message);
            }
        }
    }
}
