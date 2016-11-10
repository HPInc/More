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
    public class Oddities
    {
        struct StructWithValue
        {
            public int value;
            public StructWithValue(int value)
            {
                this.value = value;
            }
            public void ModifyNormal(int val)
            {
                this.value = val;
            }
        }
        class HasStructMembers
        {
            // A readonly struct member can behave very oddly
            public readonly StructWithValue ReadonlyStruct;
            public StructWithValue NormalStruct;
        }

        [TestMethod]
        public void TestStructsWithReadonlyParams()
        {
            var obj = new HasStructMembers();

            Assert.AreEqual(0, obj.ReadonlyStruct.value);
            Assert.AreEqual(0, obj.NormalStruct.value);

            obj.ReadonlyStruct.ModifyNormal(99);
            obj.NormalStruct.ModifyNormal(99);

            Assert.AreEqual(0, obj.ReadonlyStruct.value); // !!!!!!! This is the weird part
            Assert.AreEqual(99, obj.NormalStruct.value);
        }


    }
}
