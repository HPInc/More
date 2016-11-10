// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Threading;

namespace More
{
    public delegate void ExceptionCallback(Exception e);
    public class WorkerThread
    {
        struct ActionAndCallback
        {
            public readonly Action action;
            public readonly ExceptionCallback exceptionCallback;
            public ActionAndCallback(Action action, ExceptionCallback exceptionCallback)
            {
                if (action == null) throw new ArgumentNullException("action");
                this.action = action;
                this.exceptionCallback = exceptionCallback;
            }
        }

        readonly ManualResetEvent waitEvent;
        readonly ExceptionCallback defaultExceptionCallback;
        readonly Queue<ActionAndCallback> synchronizedQueueOfActions;

        Boolean keepRunning;

        public WorkerThread()
            : this(null)
        {
        }
        public WorkerThread(ExceptionCallback defaultExceptionCallback)
        {
            this.waitEvent = new ManualResetEvent(false);
            this.defaultExceptionCallback = defaultExceptionCallback;
            this.synchronizedQueueOfActions = new Queue<ActionAndCallback>();
            this.keepRunning = true;
            Thread thread = new Thread(Run);
            thread.IsBackground = true;
            thread.Start();
        }
        public void Stop()
        {
            this.keepRunning = false;
            waitEvent.Set();
        }
        public void Add(Action action)
        {
            lock (synchronizedQueueOfActions)
            {
                synchronizedQueueOfActions.Enqueue(new ActionAndCallback(action, null));
                waitEvent.Set();
            }
        }
        public void Add(Action action, ExceptionCallback exceptionCallback)
        {
            lock (synchronizedQueueOfActions)
            {
                synchronizedQueueOfActions.Enqueue(new ActionAndCallback(action, exceptionCallback));
                waitEvent.Set();
            }
        }
        void Run()
        {
            while (keepRunning)
            {
                waitEvent.WaitOne();
                if (!keepRunning) break;

                while (true)
                {
                    //
                    // See if there are any actions to perform
                    //
                    ActionAndCallback action;
                    lock (synchronizedQueueOfActions)
                    {
                        if (synchronizedQueueOfActions.Count <= 0)
                        {
                            waitEvent.Reset();
                            break;
                        }
                        action = synchronizedQueueOfActions.Dequeue();
                    }

                    if(!keepRunning) break;

                    //
                    // Perform the action
                    //
                    try
                    {
                        action.action();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            if (action.exceptionCallback != null)
                            {
                                action.exceptionCallback(e);
                            }
                            else
                            {
                                if (defaultExceptionCallback != null) defaultExceptionCallback(e);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }
    }
}
