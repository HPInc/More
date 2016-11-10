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
    /// Summary description for TestSha1
    /// </summary>
    [TestClass]
    public class TestSha1
    {
        [TestMethod]
        public void TestSha1HashApi()
        {
            Sha1 expectedHash = new Sha1(
                0x12DADA1F, 0xFF4D4787, 0xADE33331, 0x47202C3B, 0x443E376F
            );

            Console.WriteLine("EXPCTD:" + expectedHash);
            Console.WriteLine("----------------------------------------------------------");

            Byte[] data = new Byte[] { 1, 2, 3, 4 };
            String diff;

            //
            Sha1Builder fourCallSha = new Sha1Builder();

            fourCallSha.Add(data, 0, 1);
            fourCallSha.Add(data, 1, 1);
            fourCallSha.Add(data, 2, 1);
            fourCallSha.Add(data, 3, 1);

            var fourCallFinalHash = fourCallSha.Finish(false);
            Console.WriteLine("FINAL1 :" + fourCallFinalHash);

            diff = expectedHash.Diff(fourCallFinalHash);
            Assert.IsNull(diff);

            Console.WriteLine("----------------------------------------------------------");
            //
            Sha1Builder twoCallSha = new Sha1Builder();

            twoCallSha.Add(data, 0, 2);

            twoCallSha.Add(data, 2, 2);

            var twoCallFinahHash = twoCallSha.Finish(false);
            Console.WriteLine("FINAL2 :" + twoCallFinahHash);

            diff = expectedHash.Diff(twoCallFinahHash);
            Assert.IsNull(diff);

            Console.WriteLine("----------------------------------------------------------");
            //
            Sha1Builder oneCallSha = new Sha1Builder();

            oneCallSha.Add(data, 0, 4);

            var oneCallFinalHash = oneCallSha.Finish(false);
            Console.WriteLine("FINAL3 :" + oneCallFinalHash);

            diff = expectedHash.Diff(oneCallFinalHash);
            Assert.IsNull(diff);
        }

        class TestClass
        {
            public readonly String contentString;
            public readonly Byte[] contentBytes;
            
            public readonly Sha1 expectedHash;
            public TestClass(String contentString, params UInt32[] expectedHash)
            {
                this.contentString = contentString;
                this.contentBytes = Encoding.ASCII.GetBytes(contentString);
                this.expectedHash = new Sha1(expectedHash[0], expectedHash[1],
                    expectedHash[2], expectedHash[3], expectedHash[4]);
            }
        }

        [TestMethod]
        public void TestKnownHashes()
        {
            TestClass[] tests = new TestClass[] {
                new TestClass("abc", 0xA9993E36, 0x4706816A, 0xBA3E2571, 0x7850C26C, 0x9Cd0d89D),
                new TestClass("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq",
                    0x84983E44, 0x1C3BD26E, 0xBAAE4AA1, 0xF95129E5, 0xE54670F1),
                new TestClass("12345678901234567890123456789012345678901234567890123456789012345678901234567890",
                    0x50ABF570, 0x6A150990, 0xA08B2C5E, 0xA40FA0E5, 0x85554732),
                new TestClass("jafdnznjkl89fn4q3poiunqn8vrnaru8apr8umpau8rfpnu312--1-0-139-110un45paiouwepiourpoqiwrud0-ur238901unmxd-0r1u-rdu0-12u3rm-u-uqfoprufquwioperupauwperuq2cfurq2urduq;w3uirmparuw390peuaf;wuir;oui;avuwao; aro aruawrv au;ru ;aweuriafuwer23f0quprmpuqpuqurq[0q5tau=53una54fion[5cnuq30m5uq903uqncf4",
                    0xEEC53E5E, 0x78191154, 0x0A073AE1, 0x39743E68, 0x8A6CD077),
            };

            Sha1Builder reusedShaBuilder = new Sha1Builder();

            for(int i = 0; i < tests.Length; i++)
            {
                TestClass test = tests[i];

                //
                // Test using 1 call
                //
                {
                    Sha1Builder newShaBuilder = new Sha1Builder();

                    newShaBuilder.Add(test.contentBytes, 0, test.contentBytes.Length);
                    reusedShaBuilder.Add(test.contentBytes, 0, test.contentBytes.Length);

                    var newFinished = newShaBuilder.Finish(false);
                    var reusedFinished = reusedShaBuilder.Finish(true);

                    Console.WriteLine("Content '{0}'", test.contentString);
                    Console.WriteLine("    Expected {0}", test.expectedHash);
                    Console.WriteLine("    Actual   {0}", newFinished);
                    Console.WriteLine("    Reused   {0}", reusedFinished);

                    Assert.AreEqual(test.expectedHash, newFinished);
                    Assert.AreEqual(test.expectedHash, reusedFinished);
                    //String sosDiff = Sos.Diff(test.expectedHash, finished);
                    //Assert.IsNull(sosDiff, sosDiff);
                }

                //
                // Test using multiple calls
                //
                for (int addLength = 1; addLength < test.contentBytes.Length; addLength++)
                {
                    Console.WriteLine("Test AddLength {0}", addLength);
                    Sha1Builder shaBuilder = new Sha1Builder();

                    // Add the bytes
                    Int32 bytesToWrite = test.contentBytes.Length;
                    Int32 contentBytesOffset = 0;
                    while (bytesToWrite > 0)
                    {
                        Int32 writeLength = Math.Min(bytesToWrite, addLength);
                        shaBuilder.Add(test.contentBytes, contentBytesOffset, writeLength);
                        reusedShaBuilder.Add(test.contentBytes, contentBytesOffset, writeLength);
                        contentBytesOffset += writeLength;
                        bytesToWrite -= writeLength;
                    }

                    var shaFinished = shaBuilder.Finish(false);
                    var reusedShaFinished = reusedShaBuilder.Finish(true);

                    var sosDiff = Sos.Diff(test.expectedHash, shaFinished);
                    if (sosDiff != null)
                    {
                        Console.WriteLine("Content '{0}'", test.contentString);
                        Console.WriteLine("    Expected {0}", test.expectedHash);
                        Console.WriteLine("    Actual   {0}", shaFinished);
                        Assert.Fail();
                    }
                    Assert.AreEqual(test.expectedHash, shaFinished);
                    Assert.AreEqual(test.expectedHash, reusedShaFinished);
                }
            }
        }
    }
}
