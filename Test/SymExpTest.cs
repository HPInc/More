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
    /// Summary description for SymExpTest
    /// </summary>
    [TestClass]
    public class SymExpTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            TestSymExp("a");
            TestSymExp("z");
            TestSymExp("0");
            TestSymExp("9");

            TestSymExp("()");

            TestSymExp("(a b)");
            TestSymExp("(123 a)");
        }

        public void TestSymExp(String symExp)
        {
            Console.WriteLine("Testing '{0}'", symExp);

            SNode node = SymExp.Read(symExp);

            StringBuilder builder = new StringBuilder();
            node.ToSymExp(builder);

            Assert.AreEqual(symExp, builder.ToString());
        }
    }
}
