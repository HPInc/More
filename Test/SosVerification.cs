// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestClasses;

namespace More
{
    [TestClass]
    public class TestVerification
    {
        [TestMethod]
        public void VerificationTests()
        {
            SosTypeSerializationVerifier verifier = new SosTypeSerializationVerifier();

            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(Boolean)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(SByte)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(Byte)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(Int16)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(UInt16)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(Int16)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(Int32)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(UInt32)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(Int64)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(UInt64)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(Char)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(String)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(Object)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(DayOfWeek)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(NoEmptyConstructor)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(SubclassHasNoEmptyConstructor)));

            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(ClassWithPrimitiveTypes)));
            Assert.IsNull(verifier.CannotBeSerializedBecause(typeof(StructWithPrimitiveTypes)));

            Assert.IsNotNull(verifier.CannotBeSerializedBecause(typeof(AbstractClass)));
            Assert.IsNotNull(verifier.CannotBeSerializedBecause(typeof(GenericClass<Int32>)));
            Assert.IsNotNull(verifier.CannotBeSerializedBecause(typeof(ClassWithWeirdTypes)));
        }
    }
}
