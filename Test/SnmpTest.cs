// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class SnmpTest
    {
        [TestMethod]
        public void TestUInt32Parser()
        {
            Int32 outOffset;
            Assert.AreEqual(0U, Snmp.ParseUInt32ReturnOffset("0", 0, out outOffset));
            Assert.AreEqual(1, outOffset);
            Assert.AreEqual(0U, Snmp.ParseUInt32ReturnOffset("0.", 0, out outOffset));
            Assert.AreEqual(1, outOffset);
            Assert.AreEqual(0U, Snmp.ParseUInt32ReturnOffset("abc0", 3, out outOffset));
            Assert.AreEqual(4, outOffset);
            Assert.AreEqual(0U, Snmp.ParseUInt32ReturnOffset("abc0def", 3, out outOffset));
            Assert.AreEqual(4, outOffset);

            Assert.AreEqual(1U, Snmp.ParseUInt32ReturnOffset("1", 0, out outOffset));
            Assert.AreEqual(1, outOffset);
            Assert.AreEqual(1U, Snmp.ParseUInt32ReturnOffset("1.", 0, out outOffset));
            Assert.AreEqual(1, outOffset);
            Assert.AreEqual(1U, Snmp.ParseUInt32ReturnOffset("1 ", 0, out outOffset));
            Assert.AreEqual(1, outOffset);
            Assert.AreEqual(1U, Snmp.ParseUInt32ReturnOffset("ab1", 2, out outOffset));
            Assert.AreEqual(3, outOffset);

            Assert.AreEqual(100U, Snmp.ParseUInt32ReturnOffset("100", 0, out outOffset));
            Assert.AreEqual(3, outOffset);
            Assert.AreEqual(10000000U, Snmp.ParseUInt32ReturnOffset("10000000", 0, out outOffset));
            Assert.AreEqual(8, outOffset);
            Assert.AreEqual(4294967295, Snmp.ParseUInt32ReturnOffset("4294967295", 0, out outOffset));
            Assert.AreEqual(10, outOffset);
        }

        readonly List<Byte> oidBytes = new List<Byte>();
        public void TestOidParser(Byte[] expectedBytes, String oidString)
        {
            oidBytes.Clear();
            Console.WriteLine("Testing '{0}'", oidString);
            Snmp.ParseOid(oidString, oidBytes);

            Byte[] actual = oidBytes.ToArray();
            String diff = Sos.Diff(expectedBytes, actual);
            if (diff != null)
            {
                Console.WriteLine("Expected: " + expectedBytes.SerializeObject());
                Console.WriteLine("Actual  : " + actual.SerializeObject());
                Assert.Fail(diff);
            }
        }

        [TestMethod]
        public void TestOidParser()
        {
            //
            // Test invalid OIDs
            //
            try
            {
                Snmp.ParseOid("a", oidBytes);
                Assert.Fail("Did not get the expected exception");
            }
            catch (FormatException e)
            {
                Console.WriteLine("Caught expected exception '{0}'", e.Message);
            }
            try
            {
                Snmp.ParseOid("0", oidBytes);
                Assert.Fail("Did not get the expected exception");
            }
            catch (FormatException e)
            {
                Console.WriteLine("Caught expected exception '{0}'", e.Message);
            }
            try
            {
                Snmp.ParseOid("0x", oidBytes);
                Assert.Fail("Did not get the expected exception");
            }
            catch (FormatException e)
            {
                Console.WriteLine("Caught expected exception '{0}'", e.Message);
            }
            try
            {
                Snmp.ParseOid("0.", oidBytes);
                Assert.Fail("Did not get the expected exception");
            }
            catch (FormatException e)
            {
                Console.WriteLine("Caught expected exception '{0}'", e.Message);
            }
            try
            {
                Snmp.ParseOid("0.b", oidBytes);
                Assert.Fail("Did not get the expected exception");
            }
            catch (FormatException e)
            {
                Console.WriteLine("Caught expected exception '{0}'", e.Message);
            }
            try
            {
                Snmp.ParseOid("0.0c", oidBytes);
                Assert.Fail("Did not get the expected exception");
            }
            catch (FormatException e)
            {
                Console.WriteLine("Caught expected exception '{0}'", e.Message);
            }
            try
            {
                Snmp.ParseOid("0.0.", oidBytes);
                Assert.Fail("Did not get the expected exception");
            }
            catch (FormatException e)
            {
                Console.WriteLine("Caught expected exception '{0}'", e.Message);
            }
            try
            {
                Snmp.ParseOid("0.0.d", oidBytes);
                Assert.Fail("Did not get the expected exception");
            }
            catch (FormatException e)
            {
                Console.WriteLine("Caught expected exception '{0}'", e.Message);
            }


            
            TestOidParser(new Byte[] {    0,      }, "0.0");
            TestOidParser(new Byte[] {    0,    0 }, "0.0.0");
            TestOidParser(new Byte[] {    0,    0, 0, 0, 0, 0, 0, 0, 0 }, "0.0.0.0.0.0.0.0.0.0");
            TestOidParser(new Byte[] {    0,    1 }, "0.0.1");
            TestOidParser(new Byte[] {   39,    1 }, "0.39.1");

            TestOidParser(new Byte[] { 40, 0 }, "1.0.0");
            TestOidParser(new Byte[] { 40, 1 }, "1.0.1");

            TestOidParser(new Byte[] { 41, 0 }, "1.1.0");
            TestOidParser(new Byte[] { 43, 0 }, "1.3.0");
            TestOidParser(new Byte[] { 43, 6 }, "1.3.6");
            TestOidParser(new Byte[] { 43, 6, 1, 4, 1, 11 }, "1.3.6.1.4.1.11");

            TestOidParser(new Byte[] { 80, 0 }, "1.40.0");
            TestOidParser(new Byte[] { 80, 1 }, "1.40.1");
            TestOidParser(new Byte[] { 80, 0 }, "2.0.0");
            TestOidParser(new Byte[] { 80, 1 }, "2.0.1");

            TestOidParser(new Byte[] { 0, 0x81, 0x00, 0x81, 0x01 }, "0.0.128.129");
            TestOidParser(new Byte[] { 0, 0x81, 0x7F, 1 }         , "0.0.255.1");

            TestOidParser(new Byte[] { 0, 0xFF, 0x7F }                  , "0.0.16383");

            TestOidParser(new Byte[] { 0, 0x81, 0x80, 0x00 }            , "0.0.16384");
            TestOidParser(new Byte[] { 0, 0xFF, 0xFF, 0x7F }            , "0.0.2097151");

            TestOidParser(new Byte[] { 0, 0x81, 0x80, 0x80, 0x00 }      , "0.0.2097152");
            TestOidParser(new Byte[] { 0, 0xFF, 0xFF, 0xFF, 0x7F }      , "0.0.268435455");

            TestOidParser(new Byte[] { 0, 0x81, 0x80, 0x80, 0x80, 0x00 }, "0.0.268435456");
            TestOidParser(new Byte[] { 0, 0x8F, 0xFF, 0xFF, 0xFF, 0x7F }, "0.0.4294967295");


            TestOidParser(new Byte[] { 0, 0x8F, 0xFF, 0xFF, 0xFF, 0x7F, 0, 0, 1 }, "0.0.4294967295.0.0.1");
        }


        [TestMethod]
        public void ManuallyVerifyErrorMessages()
        {
            Byte temp;

            Byte[] packet = new Byte[256];

            List<Byte> oidList = new List<Byte>();
            Snmp.ParseOid("1.2.3.4.5.6.7", oidList);
            Snmp.SerializeGet(packet, 0, "public", 0, oidList);

            SnmpPacket snmpPacket = new SnmpPacket();

            //
            // Test that this is a valid packet
            //
            snmpPacket.Deserialize(packet, 0);

            //
            // Invalid ASN.1 Type
            //
            temp = packet[0];
            packet[0] = 0; // Should be ASN.1 Sequence
            try
            {
                snmpPacket.Deserialize(packet, 0);
                Assert.Fail("Expected exception that did not occur");
            }
            catch (FormatException e) { Console.WriteLine("Caught exception for invalid packet type: '{0}'", e.Message); }
            packet[0] = temp;


            //
            // Truncated packet
            //
            Byte[] packetTooSmall = new Byte[10];
            packetTooSmall[0] = Asn1.TYPE_SEQUENCE;
            packetTooSmall[1] = 100;
            try
            {
                snmpPacket.Deserialize(packetTooSmall, 0);
                Assert.Fail("Expected exception that did not occur");
            }
            catch (FormatException e) { Console.WriteLine("Caught exception for truncated packet: '{0}'", e.Message); }

            //
            // Invalid Version Type
            //
            temp = packet[2];
            packet[2] = Asn1.TYPE_NULL;
            try
            {
                snmpPacket.Deserialize(packet, 0);
                Assert.Fail("Expected exception that did not occur");
            }
            catch (FormatException e) { Console.WriteLine("Caught exception for invalid snmp version type: '{0}'", e.Message); }
            packet[2] = temp;

            //
            // Invalid Community Type
            //
            temp = packet[5];
            packet[5] = Asn1.TYPE_INTEGER;
            try
            {
                snmpPacket.Deserialize(packet, 0);
                Assert.Fail("Expected exception that did not occur");
            }
            catch (FormatException e) { Console.WriteLine("Caught exception for invalid community type: '{0}'", e.Message); }
            packet[5] = temp;
        }

    }
}
