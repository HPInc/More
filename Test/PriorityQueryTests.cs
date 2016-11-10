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
    public class PriorityQueryTests
    {
        static Priority PassThroughPriority(UInt32 value)
        {
            return new Priority(value);
        }

        [TestMethod]
        public void TestPrioritySelect()
        {
            PriorityValue<UInt32> priorityValue;

            priorityValue = new UInt32[] { }.PrioritySelect(PassThroughPriority);
            Assert.AreEqual(priorityValue.priority, Priority.Ignore);

            priorityValue = new UInt32[] {0}.PrioritySelect(PassThroughPriority);
            Assert.AreEqual(priorityValue.priority, Priority.Ignore);

            priorityValue = new UInt32[] { 0, 0 }.PrioritySelect(PassThroughPriority);
            Assert.AreEqual(priorityValue.priority, Priority.Ignore);

            priorityValue = new UInt32[] { 0, 1, 2, 3, 3, 2, 1, 0 }.PrioritySelect(PassThroughPriority);
            Assert.AreEqual(priorityValue.priority.value, 3U);
            Assert.AreEqual(priorityValue.value, 3U);

            priorityValue = new UInt32[] { Priority.HighestValue }.PrioritySelect(PassThroughPriority);
            Assert.AreEqual(priorityValue.priority.value, Priority.HighestValue);
            Assert.AreEqual(priorityValue.value, Priority.HighestValue);

            priorityValue = new UInt32[] { Priority.SecondHighest.value, Priority.HighestValue }.PrioritySelect(PassThroughPriority);
            Assert.AreEqual(priorityValue.priority.value, Priority.HighestValue);
            Assert.AreEqual(priorityValue.value, Priority.HighestValue);
        }
    }
}
