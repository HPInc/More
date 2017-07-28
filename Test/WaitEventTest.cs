// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    class WaitActionVerifier
    {
        public readonly String id;
        public Boolean expectedToHandle;
        public WaitActionVerifier(String id)
        {
            this.id = id;
            this.expectedToHandle = false;
        }
        public void HandleEvent(WaitTimeAndAction waitTimeAndAction)
        {
            if (this.expectedToHandle == false) Assert.Fail("Did not expect to handle this event yet");
            Console.WriteLine("Handling {0}", id);
            this.expectedToHandle = false;
        }
    }
    [TestClass]
    public class WaitEventTest
    {
        [TestMethod]
        public void WaitActionOrderTest()
        {
            WaitActionManager waitActions = new WaitActionManager(0);

            WaitActionVerifier[] waitActionVerifiers = new WaitActionVerifier[] {
                new WaitActionVerifier("Waiter 0"),
                new WaitActionVerifier("Waiter 1"),
                new WaitActionVerifier("Waiter 2"),
                new WaitActionVerifier("Waiter 3"),
                new WaitActionVerifier("Waiter 4"),
                new WaitActionVerifier("Waiter 5"),
                new WaitActionVerifier("Waiter 6"),
            };

            Int64 startTime = Stopwatch.GetTimestamp();

            Assert.IsTrue (waitActions.AddAndReturnTrueIfNewer(new WaitTimeAndAction(startTime,   700, waitActionVerifiers[4].HandleEvent)));
            Assert.IsTrue (waitActions.AddAndReturnTrueIfNewer(new WaitTimeAndAction(startTime,   600, waitActionVerifiers[3].HandleEvent)));
            Assert.IsFalse(waitActions.AddAndReturnTrueIfNewer(new WaitTimeAndAction(startTime, 800, waitActionVerifiers[5].HandleEvent)));
            Assert.IsTrue (waitActions.AddAndReturnTrueIfNewer(new WaitTimeAndAction(startTime,     0, waitActionVerifiers[2].HandleEvent)));
            Assert.IsFalse(waitActions.AddAndReturnTrueIfNewer(new WaitTimeAndAction(startTime, 900, waitActionVerifiers[6].HandleEvent)));
            Assert.IsTrue (waitActions.AddAndReturnTrueIfNewer(new WaitTimeAndAction(startTime,  -100, waitActionVerifiers[1].HandleEvent)));
            Assert.IsTrue (waitActions.AddAndReturnTrueIfNewer(new WaitTimeAndAction(startTime,  -200, waitActionVerifiers[0].HandleEvent)));

            // Handle the first 2 actions which should have happened in the past
            waitActionVerifiers[0].expectedToHandle = true;
            waitActionVerifiers[1].expectedToHandle = true;

            for (int i = 2; i < waitActionVerifiers.Length; i++)
            {
                Console.WriteLine("(Time={0}) Testing {1}", (Stopwatch.GetTimestamp() - startTime).StopwatchTicksAsInt32Milliseconds(), waitActionVerifiers[i].id);

                waitActionVerifiers[i].expectedToHandle = true;
                Int32 nextSleep = waitActions.HandleWaitActions();

                Console.WriteLine("(Time={0}) Next Sleep {1}", (Stopwatch.GetTimestamp() - startTime).StopwatchTicksAsInt32Milliseconds(), nextSleep);
                Assert.IsFalse(waitActionVerifiers[i].expectedToHandle);

                if(nextSleep > 0) Thread.Sleep(nextSleep + 1);
            }
        }
        [TestMethod]
        public void WaitActionSignalTest()
        {
            WaitActionManager waitActions = new WaitActionManager(0);

            Int64 startTime = Stopwatch.GetTimestamp();
            Int32 timeOffset = 1000000;

            Assert.IsTrue(waitActions.AddAndReturnTrueIfNewer(new WaitTimeAndAction(startTime + timeOffset, (WaitTimeAndAction a) =>
                { Assert.Fail("This action should not be called in this test"); })));

            Assert.IsTrue(waitActions.AddAndReturnTrueIfNewer(new WaitTimeAndAction(startTime - timeOffset, (WaitTimeAndAction a) => { Console.WriteLine("Successfully executed the correct action"); })));
        }
    }
}
