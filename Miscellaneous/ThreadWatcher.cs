// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Threading;

namespace More
{
    public class RunWithFinishNotification
    {
        public readonly ThreadStart runMethod;
        readonly Action finishedCallback;

        public RunWithFinishNotification(ThreadStart runMethod, Action finishedCallback)
        {
            if (runMethod == null) throw new ArgumentNullException("runMethod");
            if (finishedCallback == null) throw new ArgumentNullException("finishedCallback");

            this.runMethod = runMethod;
            this.finishedCallback = finishedCallback;
        }
        public void Run()
        {
            try
            {
                runMethod();
            }
            finally
            {
                finishedCallback();
            }
        }
    }

    //
    // This class is used to be notified when a thread has finished
    //   Example: Thread thread = new Thread(new RunWithFinishNotification(RunMethod, FinishCallback, MyCallbackData).Run);
    //            thread.Start();
    //
    public class RunWithFinishNotification<CallbackDataType>
    {
        public delegate void ThreadFinished(CallbackDataType callbackData);

        public readonly ThreadStart runMethod;
        readonly ThreadFinished finishedCallback;
        readonly CallbackDataType callbackData;

        public RunWithFinishNotification(ThreadStart runMethod, ThreadFinished finishedCallback, CallbackDataType callbackData)
        {
            if (runMethod == null) throw new ArgumentNullException("runMethod");
            if (finishedCallback == null) throw new ArgumentNullException("finishedCallback");

            this.runMethod = runMethod;
            this.finishedCallback = finishedCallback;
            this.callbackData = callbackData;
        }
        public void Run()
        {
            try
            {
                runMethod();
            }
            finally
            {
                finishedCallback(callbackData);
            }
        }
    }
}
