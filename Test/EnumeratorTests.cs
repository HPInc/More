// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class EnumeratorTests
    {
        [TestMethod]
        public void TestRollingArrayEnumerator()
        {
            TestRollingArrayEnumerator       (new UInt32[] { 1 }, 0, new UInt32[] { 1 });
            TestRollingArrayReverseEnumerator(new UInt32[] { 1 }, 0, new UInt32[] { 1 });

            TestRollingArrayEnumerator       (new UInt32[] { 1, 2 }, 0, new UInt32[] { 1, 2 });
            TestRollingArrayEnumerator       (new UInt32[] { 1, 2 }, 1, new UInt32[] { 2, 1 });
            TestRollingArrayReverseEnumerator(new UInt32[] { 1, 2 }, 0, new UInt32[] { 1, 2 });
            TestRollingArrayReverseEnumerator(new UInt32[] { 1, 2 }, 1, new UInt32[] { 2, 1 });

            TestRollingArrayEnumerator       (new UInt32[] { 1, 2, 3, 4 }, 0, new UInt32[] { 1, 2, 3, 4 });
            TestRollingArrayEnumerator       (new UInt32[] { 1, 2, 3, 4 }, 1, new UInt32[] { 2, 3, 4, 1 });
            TestRollingArrayEnumerator       (new UInt32[] { 1, 2, 3, 4 }, 2, new UInt32[] { 3, 4, 1, 2 });
            TestRollingArrayEnumerator       (new UInt32[] { 1, 2, 3, 4 }, 3, new UInt32[] { 4, 1, 2, 3 });
            TestRollingArrayReverseEnumerator(new UInt32[] { 1, 2, 3, 4 }, 0, new UInt32[] { 1, 4, 3, 2 });
            TestRollingArrayReverseEnumerator(new UInt32[] { 1, 2, 3, 4 }, 1, new UInt32[] { 2, 1, 4, 3 });
            TestRollingArrayReverseEnumerator(new UInt32[] { 1, 2, 3, 4 }, 2, new UInt32[] { 3, 2, 1, 4 });
            TestRollingArrayReverseEnumerator(new UInt32[] { 1, 2, 3, 4 }, 3, new UInt32[] { 4, 3, 2, 1 });
        }
        void TestRollingArrayEnumerator(UInt32[] array, UInt32 startIndex, UInt32[] expectedValues)
        {
            RollingArray<UInt32> rollingArray = new RollingArray<UInt32>(array, startIndex);

            UInt32 expectedCompareIndex = 0;
            foreach (UInt32 value in rollingArray)
            {
                Assert.AreEqual(expectedValues[expectedCompareIndex], value);
                expectedCompareIndex++;
            }

            Assert.AreEqual((UInt32)expectedValues.Length, expectedCompareIndex);
        }
        void TestRollingArrayReverseEnumerator(UInt32[] array, UInt32 startIndex, UInt32[] expectedValues)
        {
            RollingArrayReversed<UInt32> rollingArray = new RollingArrayReversed<UInt32>(array, startIndex);

            UInt32 expectedCompareIndex = 0;
            foreach (UInt32 value in rollingArray)
            {
                Assert.AreEqual(expectedValues[expectedCompareIndex], value);
                expectedCompareIndex++;
            }

            Assert.AreEqual((UInt32)expectedValues.Length, expectedCompareIndex);
        }
    }
}
