// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class SinkTests
    {
        [TestMethod]
        public unsafe void TestStdOutSink()
        {
            Byte[] testMessage = new Byte[] {(Byte)'T',(Byte)'e',(Byte)'s',(Byte)'t'};
            fixed (byte* testMessagePtr = testMessage)
            {
                IO.EnsureConsoleOpen();
                IO.StdOut.Write(testMessage);
                IO.StdOut.WriteLine();
                IO.StdOut.Write(testMessagePtr, (uint)testMessage.Length);
            }
        }
    }
}
