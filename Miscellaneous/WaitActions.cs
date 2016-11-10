// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace More
{
    public delegate void WaitAction(WaitTimeAndAction waitTimeAndAction);
    public class WaitTimeAndAction
    {
        public static Int32 DecreasingComparison(WaitTimeAndAction x, WaitTimeAndAction y)
        {
            return (x.stopwatchTicksToHandleAction > y.stopwatchTicksToHandleAction) ? -1 :
                ((x.stopwatchTicksToHandleAction < y.stopwatchTicksToHandleAction) ? 1 : 0);
        }

        Int64 stopwatchTicksToHandleAction;
        WaitAction waitAction;

        public Int64 StopwatchTicksToHandleAction { get { return stopwatchTicksToHandleAction; } }

        public WaitTimeAndAction(Int64 stopwatchTicksToHandleAction, WaitAction waitAction)
        {
            this.stopwatchTicksToHandleAction = stopwatchTicksToHandleAction;
            this.waitAction = waitAction;
        }
        public WaitTimeAndAction(Int64 now, Int32 millisFromNow, WaitAction waitAction)
        {
            this.stopwatchTicksToHandleAction = now + millisFromNow.MillisToStopwatchTicks();
            this.waitAction = waitAction;
        }

        public Boolean HasAction()
        {
            return waitAction != null;
        }

        public void SetNextWaitTime(Int64 stopwatchTicksToHandleAction)
        {
            this.stopwatchTicksToHandleAction = stopwatchTicksToHandleAction;
        }
        public void SetNextWaitTime(Int64 now, Int32 millisFromNow)
        {
            this.stopwatchTicksToHandleAction = now + millisFromNow.MillisToStopwatchTicks();
        }
        public void SetNextWaitTimeFromNow(UInt32 millisFromNow)
        {
            this.stopwatchTicksToHandleAction = Stopwatch.GetTimestamp() + millisFromNow.MillisToStopwatchTicks();
        }
        public void SetNextAction(WaitAction newAction)
        {
            this.waitAction = newAction;
        }

        // Returns true if this action has been reset for another time
        public Boolean Execute()
        {
            if (waitAction == null) throw new InvalidOperationException("Someone called Execute on this wait action, but this WaitTimeAction was already executed and was not reset the last time it was executed");

            Int64 savedStopwatchTicksToHandleAction = this.stopwatchTicksToHandleAction;
            waitAction(this);

            if (waitAction == null) return false;
            if (savedStopwatchTicksToHandleAction == this.stopwatchTicksToHandleAction)
            {
                waitAction = null;
                return false;
            }

            return true;
        }

        public Int32 MillisFromNow()
        {
            return (stopwatchTicksToHandleAction - Stopwatch.GetTimestamp()).StopwatchTicksAsInt32Milliseconds();
        }
        public Int32 MillisFromNow(Int64 now)
        {
            return (stopwatchTicksToHandleAction - now).StopwatchTicksAsInt32Milliseconds();
        }
    }


    /// <summary>
    /// First note that for performance reasons, this class does not implement its own thread safety.
    /// 
    /// Next, note that this class has 2 constructors, one with an EventWaitHandle and one without.
    /// The HandleWaitActions returns the time till the next action, or it will return 0 if there are
    /// no actions left...if the thread that is calling HandleWaitActions wants to be notified when
    /// there are new actions that happen sooner than any current actions, then an EventWaitHandle
    /// must be passed to this class in the constructor that accepts one.
    /// </summary>
    public class WaitActionManager
    {
        readonly Int32 millisecondTolerance;
        readonly SortedList<WaitTimeAndAction> waitActions;

        /// <summary>
        /// Construct a wait action manager
        /// </summary>
        /// <param name="millisecondTolerance">
        /// The number of milliseconds an action will be executed if it is within the tolerance.
        /// For example, if an action occurs 5 milliseconds from now, and the tolerance is greater than or equal to 5 seconds, the action
        /// will be executed now.
        /// A minimum value of 1 is recommended to prevent threads that are timing out when the action occurs from waking early (waking when there is 1 millisecond left and then sleeping for 1 millisecond).
        /// </param>
        public WaitActionManager(Int32 millisecondTolerance)
        {
            if (millisecondTolerance < 0) throw new ArgumentOutOfRangeException(String.Format("The millisecond tolerance must be a non-negative value but it is {0}", millisecondTolerance));
            this.millisecondTolerance = millisecondTolerance;
            this.waitActions = new SortedList<WaitTimeAndAction>(128, 128, WaitTimeAndAction.DecreasingComparison);
        }

        /// <summary>
        /// Adds a new wait time and action, if the new time is sooner than any previous times it returns true.
        /// This can be used to signal to another thread that it needs to reset it's timer till the next event.
        /// </summary>
        /// <param name="newWaitTimeAndAction">The new wait time and action to add</param>
        public Boolean Add(WaitTimeAndAction newWaitTimeAndAction)
        {
            if (!newWaitTimeAndAction.HasAction()) throw new InvalidOperationException("Cannot add this wait time action because it has no action");

            if (waitActions.count <= 0)
            {
                waitActions.Add(newWaitTimeAndAction);
                return true;
            }

            //
            // Check if this new event happens sooner than the next event
            //
            Int64 now = Stopwatch.GetTimestamp();
            Int32 nextEventMillisFromNow = waitActions.elements[waitActions.count - 1].MillisFromNow(now);
            Int32 newEventMillisFromNow = newWaitTimeAndAction.MillisFromNow(now);

            waitActions.Add(newWaitTimeAndAction);

            return newEventMillisFromNow < nextEventMillisFromNow;
        }


        public UInt32 WaitActionTimes(ArrayReference<Int32> times, Int64 now)
        {
            times.EnsureCapacityCopyData(waitActions.count);

            UInt32 count = 0;
            for (int i = (Int32)waitActions.count - 1; i >= 0; i--)
            {
                WaitTimeAndAction nextWaitTimeAndAction = waitActions.elements[i];
                times.array[count] = nextWaitTimeAndAction.MillisFromNow(now);
                count++;
            }

            return count;
        }



        /// <summary>
        /// Handle wait actions
        /// </summary>
        /// <returns>Return milliseconds from now of the next action, or 0 if there are no more actions</returns>
        public Int32 HandleWaitActions()
        {
            while (true)
            {
                if (waitActions.count <= 0) return 0;

                //
                // Get the soonest action and check if it should be executed
                //
                WaitTimeAndAction nextWaitTimeAndAction = waitActions.elements[waitActions.count - 1];

                Int32 millisFromNow = nextWaitTimeAndAction.MillisFromNow();
                if (millisFromNow > millisecondTolerance) return millisFromNow;

                //
                // Remove the soonest action from the list
                //
                waitActions.count--;
                waitActions.elements[waitActions.count] = null; // remove reference

                //
                // Execute the action, and add it back to the list if it was reset
                //
                if (nextWaitTimeAndAction.Execute())
                {
                    waitActions.Add(nextWaitTimeAndAction);
                }
            }
        }
    }
}
