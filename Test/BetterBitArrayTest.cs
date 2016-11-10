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
    public class BetterBitArrayTest
    {
        [TestMethod]
        public void TestFlip()
        {
            BetterBitArray bitArray = new BetterBitArray(100000);
            for (UInt32 i = 0; i < bitArray.bitLength; i++)
            {
                Assert.IsFalse(bitArray.Get(i));
            }

            for (UInt32 i = 0; i < bitArray.bitLength; i++)
            {
                bitArray.Flip(i);
            }

            for (UInt32 i = 0; i < bitArray.bitLength; i++)
            {
                Assert.IsTrue(bitArray.Get(i));
            }

            for (UInt32 i = 0; i < bitArray.bitLength; i++)
            {
                bitArray.Flip(i);
            }

            for (UInt32 i = 0; i < bitArray.bitLength; i++)
            {
                Assert.IsFalse(bitArray.Get(i));
            }
        }
    }
}
