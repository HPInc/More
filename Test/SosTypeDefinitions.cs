// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;

using TestClasses;

namespace More
{
    [TestClass]
    public class SosTypeNames
    {
        [TestMethod]
        public void TestGenerics()
        {
            Console.WriteLine(typeof(List<Byte>).FullName);
        }

        [TestMethod]
        public void TestSosIsPrimitive()
        {
            Assert.IsTrue(typeof(Boolean).IsSosPrimitive());

            Assert.IsTrue(typeof(SByte).IsSosPrimitive());
            Assert.IsTrue(typeof(Byte).IsSosPrimitive());
            Assert.IsTrue(typeof(Int16).IsSosPrimitive());
            Assert.IsTrue(typeof(UInt16).IsSosPrimitive());
            Assert.IsTrue(typeof(Int32).IsSosPrimitive());
            Assert.IsTrue(typeof(UInt32).IsSosPrimitive());
            Assert.IsTrue(typeof(UInt64).IsSosPrimitive());
            Assert.IsTrue(typeof(UInt64).IsSosPrimitive());

            Assert.IsTrue(typeof(Single).IsSosPrimitive());
            Assert.IsTrue(typeof(Double).IsSosPrimitive());

            Assert.IsTrue(typeof(Char).IsSosPrimitive());
            Assert.IsTrue(typeof(String).IsSosPrimitive());

            //
            // Non primitive types
            //
            Assert.IsFalse(typeof(Object).IsSosPrimitive());
            Assert.IsFalse(typeof(Decimal).IsSosPrimitive());
            Assert.IsFalse(typeof(Enum).IsSosPrimitive());
            Assert.IsFalse(typeof(DayOfWeek).IsSosPrimitive());
            Assert.IsFalse(typeof(Array).IsSosPrimitive());
            Assert.IsFalse(typeof(Byte[]).IsSosPrimitive());
        }

        class NullCallback : ISosTypeDefinitionCallback
        {
            public static NullCallback Instance = new NullCallback();
            public void PrimitiveType(Type type){}
            public void Enum(SosEnumDefinition enumType){}
            public void ArrayType(string elementTypeDefinition){}
            public void ObjectType(SosObjectDefinition type) { }
        }
        void TestBadTypeDefinition(String typeDefinition)
        {
            try
            {
                SosTypes.ParseSosTypeDefinition(NullCallback.Instance, typeDefinition);
                Assert.Fail("Expected FormatException but did not get one for '{0}'", typeDefinition);
            }
            catch (FormatException e)
            {
                Console.WriteLine("Expected Format Exception: {0}", e.Message);
            }
        }
        [TestMethod]
        public void TestBadEnumTypeDefinitions()
        {
            TestBadTypeDefinition("Enum");
            TestBadTypeDefinition("Enuma");
            TestBadTypeDefinition("Enum    a");

            TestBadTypeDefinition("Enum{");
            TestBadTypeDefinition("Enum  {");
            TestBadTypeDefinition("Enum  {  ");

            TestBadTypeDefinition("Enum  {  *");
            TestBadTypeDefinition("Enum  {  a*");

            TestBadTypeDefinition("Enum  {a=");
            TestBadTypeDefinition("Enum  {a  =");

            TestBadTypeDefinition("Enum  {a  =  ,");
            TestBadTypeDefinition("Enum  {a  =  }");
            TestBadTypeDefinition("Enum  {a  =  b");

            TestBadTypeDefinition("Enum  {a  =  3");
            TestBadTypeDefinition("Enum  {a  =  3 ");
            TestBadTypeDefinition("Enum  {a  =  3 b");

            TestBadTypeDefinition("Enum  {a  =  3 } b");
        }

        public void TestEnumTypeDefinition(String typeDefinition, SosEnumDefinition expected)
        {
            SosTypes.ParseSosTypeDefinition(new TypeDefinitionVerifier(new CallbackObject(expected)), typeDefinition);
        }
        [TestMethod]
        public void TestEnumTypeDefinitions()
        {
            SosEnumDefinition oneEnumDefinition = new SosEnumDefinition();
            oneEnumDefinition.Add("One", 1);

            TestEnumTypeDefinition("Enum{One=1}", oneEnumDefinition);
            TestEnumTypeDefinition("Enum  {One=1}", oneEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One=1}", oneEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One  =1}", oneEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One  =  1}", oneEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One  =  1  }", oneEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One  =  1  }  ", oneEnumDefinition);


            SosEnumDefinition twoEnumDefinition = new SosEnumDefinition();
            twoEnumDefinition.Add("One", 1);
            twoEnumDefinition.Add("Two", 2);

            TestEnumTypeDefinition("Enum{Two=2,One=1}", twoEnumDefinition);
            TestEnumTypeDefinition("Enum{One=1,Two=2}", twoEnumDefinition);
            TestEnumTypeDefinition("Enum  {One=1,Two=2}", twoEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One=1,Two=2}", twoEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One  =1,Two=2}", twoEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One  =  1,Two=2}", twoEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One  =  1  ,Two=2}", twoEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One  =  1  ,  Two=2}", twoEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One  =  1  ,  Two  =2}", twoEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One  =  1  ,  Two  =  2}", twoEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One  =  1  ,  Two  =  2  }", twoEnumDefinition);
            TestEnumTypeDefinition("Enum  {  One  =  1  ,  Two  =  2  }  ", twoEnumDefinition);
        }

        enum TwoEnum
        {
            One = 1,
            Two = 2,
        }
        [TestMethod]
        public void TestEnumDefinitionVerification()
        {
            Type[] enumTypes = new Type[] {
                typeof(DayOfWeek),
#if !WindowsCE
                typeof(ConsoleColor),
                typeof(ConsoleKey),
#endif
                typeof(TwoEnum),
                typeof(System.IO.FileAttributes),
            };

            for (int i = 0; i < enumTypes.Length; i++)
            {
                Type enumType = enumTypes[i];

                String typeDefinition = enumType.SosTypeDefinition();
                Console.WriteLine("Verifying " + typeDefinition);
                Assert.AreEqual(enumType,
                    SosTypes.ParseSosEnumTypeDefinition(typeDefinition, 4).GetAndVerifyType(enumType.AssemblyQualifiedName, 0));
            }
        }


        enum OneGoodValueEnum
        {
            FirstValue = 0,
        }
        enum ThreeGoodValuesEnum
        {
            FirstValue = 0,
            SecondValue = 1,
            ThirdValue = 2,
        }
        enum BadValueEnum
        {
            FirstValue = 0,
            SecondValue = 2, // bad value
        }

        enum LessAndExtraEnum
        {
            FirstValue = 0,
            ThirdValue = 3,
        }
        [TestMethod]
        public void TestEnumDefinitionVerifyCriteria()
        {
            SosEnumDefinition enumDefinition = new SosEnumDefinition();
            enumDefinition.Add("FirstValue", 0);
            enumDefinition.Add("SecondValue", 1);

            enumDefinition.VerifyType(typeof(OneGoodValueEnum), SosVerifyCriteria.AllowExtraEnumValuesInDefinition);
            enumDefinition.VerifyType(typeof(ThreeGoodValuesEnum), SosVerifyCriteria.AllowExtraEnumValuesInType);

            //
            try
            {
                enumDefinition.VerifyType(typeof(OneGoodValueEnum), 0);
                Assert.Fail("Expected exception but did not get one");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Got expected exception: {0}", e.Message);
            }
            try
            {
                enumDefinition.VerifyType(typeof(OneGoodValueEnum), SosVerifyCriteria.AllowExtraEnumValuesInType);
                Assert.Fail("Expected exception but did not get one");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Got expected exception: {0}", e.Message);
            }

            //
            try
            {
                enumDefinition.VerifyType(typeof(ThreeGoodValuesEnum), 0);
                Assert.Fail("Expected exception but did not get one");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Got expected exception: {0}", e.Message);
            }
            try
            {
                enumDefinition.VerifyType(typeof(ThreeGoodValuesEnum), SosVerifyCriteria.AllowExtraEnumValuesInDefinition);
                Assert.Fail("Expected exception but did not get one");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Got expected exception: {0}", e.Message);
            }

            //
            try
            {
                enumDefinition.VerifyType(typeof(BadValueEnum), 0);
                Assert.Fail("Expected exception but did not get one");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Got expected exception: {0}", e.Message);
            }

            //
            enumDefinition.VerifyType(typeof(LessAndExtraEnum), SosVerifyCriteria.AllowExtraEnumValuesInDefinition | SosVerifyCriteria.AllowExtraEnumValuesInType);
            try
            {
                enumDefinition.VerifyType(typeof(LessAndExtraEnum), 0);
                Assert.Fail("Expected exception but did not get one");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Got expected exception: {0}", e.Message);
            }
            try
            {
                enumDefinition.VerifyType(typeof(LessAndExtraEnum), SosVerifyCriteria.AllowExtraEnumValuesInDefinition);
                Assert.Fail("Expected exception but did not get one");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Got expected exception: {0}", e.Message);
            }
            try
            {
                enumDefinition.VerifyType(typeof(LessAndExtraEnum), SosVerifyCriteria.AllowExtraEnumValuesInType);
                Assert.Fail("Expected exception but did not get one");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Got expected exception: {0}", e.Message);
            }


        }
        [TestMethod]
        public void TestFailedEnumDefinitionVerification()
        {
            TestFailedEnumDefinitionVerification(typeof(TwoEnum), "Enum{One=1}");
            TestFailedEnumDefinitionVerification(typeof(TwoEnum), "Enum{One=1,Two=2,Three=3}");

            TestFailedEnumDefinitionVerification(typeof(TwoEnum), "Enum{One=1,Two=9}");
            TestFailedEnumDefinitionVerification(typeof(TwoEnum), "Enum{One=1,Twoo=2}");
        }
        public void TestFailedEnumDefinitionVerification(Type type, String incorrectTypeDefinitino)
        {
            try
            {
                SosTypes.ParseSosEnumTypeDefinition(incorrectTypeDefinitino, 4).VerifyType(type, 0);
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        [TestMethod]
        public void TestObjectDefinitionVerification()
        {
            Type[] objectTypes = new Type[] {
                typeof(Object),               
            };

            for (int i = 0; i < objectTypes.Length; i++)
            {
                Type objectType = objectTypes[i];

                String typeDefinition = objectType.SosTypeDefinition();
                Console.WriteLine("Verifying " + typeDefinition);
                Assert.AreEqual(objectType,
                    SosTypes.ParseSosObjectTypeDefinition(typeDefinition, 0).GetAndVerifyType(objectType.FullName));
            }
        }

        [TestMethod]
        public void TestFailedObjectDefinitionVerification()
        {
            TestFailedObjectDefinitionVerification(typeof(Object), "{Boolean:b}");

            TestFailedObjectDefinitionVerification(typeof(BooleanAndInt), "{Boolean:b}");
            TestFailedObjectDefinitionVerification(typeof(BooleanAndInt), "{Boolean:b,Int32:i,Int32:i2}");

            TestFailedObjectDefinitionVerification(typeof(BooleanAndInt), "{Boolean:b,Int32:other}");
            TestFailedObjectDefinitionVerification(typeof(BooleanAndInt), "{Boolean:b,Int64:i}");
        }
        public void TestFailedObjectDefinitionVerification(Type type, String incorrectTypeDefinitino)
        {
            try
            {
                SosTypes.ParseSosObjectTypeDefinition(incorrectTypeDefinitino, 0).VerifyType(type);
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        [TestMethod]
        public void TestSosTypeNames()
        {
            Assert.AreEqual("Boolean", typeof(Boolean).SosTypeName());
            Assert.AreEqual("SByte", typeof(SByte).SosTypeName());
            Assert.AreEqual("Byte", typeof(Byte).SosTypeName());
            Assert.AreEqual("Int16", typeof(Int16).SosTypeName());
            Assert.AreEqual("UInt16", typeof(UInt16).SosTypeName());
            Assert.AreEqual("Int32", typeof(Int32).SosTypeName());
            Assert.AreEqual("UInt32", typeof(UInt32).SosTypeName());
            Assert.AreEqual("Int64", typeof(Int64).SosTypeName());
            Assert.AreEqual("UInt64", typeof(UInt64).SosTypeName());

            Assert.AreEqual("Single", typeof(Single).SosTypeName());
            Assert.AreEqual("Double", typeof(Double).SosTypeName());

            Assert.AreEqual("Char", typeof(Char).SosTypeName());
            Assert.AreEqual("String", typeof(String).SosTypeName());

            Assert.AreEqual("System.DayOfWeek", typeof(DayOfWeek).SosTypeName());

            Assert.AreEqual("System.Object", typeof(Object).SosTypeName());

            //
            // Arrays
            //
            Assert.AreEqual("Boolean[]", typeof(Boolean[]).SosTypeName());
            Assert.AreEqual("SByte[]", typeof(SByte[]).SosTypeName());
            Assert.AreEqual("Byte[]", typeof(Byte[]).SosTypeName());
            Assert.AreEqual("Int16[]", typeof(Int16[]).SosTypeName());
            Assert.AreEqual("UInt16[]", typeof(UInt16[]).SosTypeName());
            Assert.AreEqual("Int32[]", typeof(Int32[]).SosTypeName());
            Assert.AreEqual("UInt32[]", typeof(UInt32[]).SosTypeName());
            Assert.AreEqual("Int64[]", typeof(Int64[]).SosTypeName());
            Assert.AreEqual("UInt64[]", typeof(UInt64[]).SosTypeName());

            Assert.AreEqual("Single[]", typeof(Single[]).SosTypeName());
            Assert.AreEqual("Double[]", typeof(Double[]).SosTypeName());

            Assert.AreEqual("Char[]", typeof(Char[]).SosTypeName());
            Assert.AreEqual("String[]", typeof(String[]).SosTypeName());

            Assert.AreEqual("System.Object[]", typeof(Object[]).SosTypeName());

            //
            // User Defined Types
            //
            Assert.AreEqual("TestClasses.ClassWithPrimitiveTypes", typeof(TestClasses.ClassWithPrimitiveTypes).SosTypeName());
            Assert.AreEqual("TestClasses.StructWithPrimitiveTypes", typeof(TestClasses.StructWithPrimitiveTypes).SosTypeName());
            Assert.AreEqual("TestClasses.ClassWithArrays", typeof(TestClasses.ClassWithArrays).SosTypeName());
            Assert.AreEqual("TestClasses.SuperClassWithPrimitivesAndArrays", typeof(TestClasses.SuperClassWithPrimitivesAndArrays).SosTypeName());

            Assert.AreEqual("TestClasses.ClassWithPrimitiveTypes[]", typeof(TestClasses.ClassWithPrimitiveTypes[]).SosTypeName());
            Assert.AreEqual("TestClasses.StructWithPrimitiveTypes[]", typeof(TestClasses.StructWithPrimitiveTypes[]).SosTypeName());
            Assert.AreEqual("TestClasses.ClassWithArrays[]", typeof(TestClasses.ClassWithArrays[]).SosTypeName());
            Assert.AreEqual("TestClasses.SuperClassWithPrimitivesAndArrays[]", typeof(TestClasses.SuperClassWithPrimitivesAndArrays[]).SosTypeName());
        }

        [TestMethod]
        public void BasicTypes()
        {
            Assert.AreEqual("Boolean", typeof(Boolean).SosTypeDefinition());
            Assert.AreEqual("SByte", typeof(SByte).SosTypeDefinition());
            Assert.AreEqual("Byte", typeof(Byte).SosTypeDefinition());
            Assert.AreEqual("Int16", typeof(Int16).SosTypeDefinition());
            Assert.AreEqual("UInt16", typeof(UInt16).SosTypeDefinition());
            Assert.AreEqual("Int32", typeof(Int32).SosTypeDefinition());
            Assert.AreEqual("UInt32", typeof(UInt32).SosTypeDefinition());
            Assert.AreEqual("Int64", typeof(Int64).SosTypeDefinition());
            Assert.AreEqual("UInt64", typeof(UInt64).SosTypeDefinition());

            Assert.AreEqual("Single", typeof(Single).SosTypeDefinition());
            Assert.AreEqual("Double", typeof(Double).SosTypeDefinition());

            Assert.AreEqual("Char", typeof(Char).SosTypeDefinition());
            Assert.AreEqual("String", typeof(String).SosTypeDefinition());

            Assert.AreEqual("Enum{Sunday=0,Monday=1,Tuesday=2,Wednesday=3,Thursday=4,Friday=5,Saturday=6}", typeof(DayOfWeek).SosTypeDefinition());

            Assert.AreEqual("{}", typeof(Object).SosTypeDefinition());

            //
            // Arrays
            //
            Assert.AreEqual("Boolean[]", typeof(Boolean[]).SosTypeDefinition());
            Assert.AreEqual("SByte[]", typeof(SByte[]).SosTypeDefinition());
            Assert.AreEqual("Byte[]", typeof(Byte[]).SosTypeDefinition());
            Assert.AreEqual("Int16[]", typeof(Int16[]).SosTypeDefinition());
            Assert.AreEqual("UInt16[]", typeof(UInt16[]).SosTypeDefinition());
            Assert.AreEqual("Int32[]", typeof(Int32[]).SosTypeDefinition());
            Assert.AreEqual("UInt32[]", typeof(UInt32[]).SosTypeDefinition());
            Assert.AreEqual("Int64[]", typeof(Int64[]).SosTypeDefinition());
            Assert.AreEqual("UInt64[]", typeof(UInt64[]).SosTypeDefinition());

            Assert.AreEqual("Single[]", typeof(Single[]).SosTypeDefinition());
            Assert.AreEqual("Double[]", typeof(Double[]).SosTypeDefinition());

            Assert.AreEqual("Char[]", typeof(Char[]).SosTypeDefinition());
            Assert.AreEqual("String[]", typeof(String[]).SosTypeDefinition());

            Assert.AreEqual("System.Object[]", typeof(Object[]).SosTypeDefinition());

            //
            // User Defined Types
            //
            Assert.AreEqual("{Boolean:myBoolean,Byte:myByte,UInt16:myUshort,Int16:myShort,UInt32:myUint,Int32:myInt,UInt64:myUlong,Int64:myLong,System.DayOfWeek:dayOfWeek,System.Object:myObject,String:myString}", typeof(TestClasses.ClassWithPrimitiveTypes).SosTypeDefinition());
            Assert.AreEqual("{Boolean:myBoolean,Byte:myByte,UInt16:myUshort,Int16:myShort,UInt32:myUint,Int32:myInt,UInt64:myUlong,Int64:myLong,System.DayOfWeek:dayOfWeek,System.Object:myObject,String:myString}", typeof(TestClasses.StructWithPrimitiveTypes).SosTypeDefinition());
            Assert.AreEqual("{Boolean[]:booleanArray,SByte[]:sByteArray,Byte[]:byteArray,UInt16[]:ushortArray,Int16[]:shortArray,UInt32[]:uintArray,Int32[]:intArray,UInt64[]:ulongArray,Int64[]:longArray,System.DayOfWeek[]:enumArray,System.Object[]:objectArray,String[]:stringArray}", typeof(TestClasses.ClassWithArrays).SosTypeDefinition());
            Assert.AreEqual("{TestClasses.ClassWithPrimitiveTypes:c,TestClasses.ClassWithArrays:ca,TestClasses.SuperClassWithPrimitivesAndArrays[]:mess}", typeof(TestClasses.SuperClassWithPrimitivesAndArrays).SosTypeDefinition());

            Assert.AreEqual("TestClasses.ClassWithPrimitiveTypes[]", typeof(TestClasses.ClassWithPrimitiveTypes[]).SosTypeDefinition());
            Assert.AreEqual("TestClasses.StructWithPrimitiveTypes[]", typeof(TestClasses.StructWithPrimitiveTypes[]).SosTypeDefinition());
            Assert.AreEqual("TestClasses.ClassWithArrays[]", typeof(TestClasses.ClassWithArrays[]).SosTypeDefinition());
            Assert.AreEqual("TestClasses.SuperClassWithPrimitivesAndArrays[]", typeof(TestClasses.SuperClassWithPrimitivesAndArrays[]).SosTypeDefinition());

        }

        [TestMethod]
        public void TestObjectTypesWithSpace()
        {
            Assert.AreEqual("{Boolean:b,Int32:i}", typeof(TestClasses.BooleanAndInt).SosTypeDefinition());

            String [] definitions = new String[] {
                "{Boolean:b,Int32:i}",
                "{    Boolean:b,Int32:i}",
                "{    Boolean   :b,Int32:i}",
                "{    Boolean   :    b,Int32:i}",
                "{    Boolean   :    b    ,Int32:i}",
                "{    Boolean   :    b    ,    Int32:i}",
                "{    Boolean   :    b    ,    Int32    :i}",
                "{    Boolean   :    b    ,    Int32    :    i}",
                "{    Boolean   :    b    ,    Int32    :    i     }",
                "{    Boolean   :    b    ,    Int32    :    i     }  ",
            };

            SosObjectDefinition objectDefinition = new SosObjectDefinition(
                "Boolean","b",
                "Int32","i");

            for(int i = 0; i < definitions.Length; i++)
            {
                String definition = definitions[i];
                Console.WriteLine(definition);
                Assert.AreEqual(objectDefinition, SosTypes.ParseSosObjectTypeDefinition(definition, 0));
            }
        }

        [TestMethod]
        public void RoundTripTests()
        {
            RoundTripTypeDefinition(typeof(Boolean), new TypeDefinitionVerifier(new CallbackObject(typeof(Boolean))));
            RoundTripTypeDefinition(typeof(SByte), new TypeDefinitionVerifier(new CallbackObject(typeof(SByte))));
            RoundTripTypeDefinition(typeof(Byte), new TypeDefinitionVerifier(new CallbackObject(typeof(Byte))));
            RoundTripTypeDefinition(typeof(Int16), new TypeDefinitionVerifier(new CallbackObject(typeof(Int16))));
            RoundTripTypeDefinition(typeof(UInt16), new TypeDefinitionVerifier(new CallbackObject(typeof(UInt16))));
            RoundTripTypeDefinition(typeof(Int32), new TypeDefinitionVerifier(new CallbackObject(typeof(Int32))));
            RoundTripTypeDefinition(typeof(UInt32), new TypeDefinitionVerifier(new CallbackObject(typeof(UInt32))));
            RoundTripTypeDefinition(typeof(Int64), new TypeDefinitionVerifier(new CallbackObject(typeof(Int64))));
            RoundTripTypeDefinition(typeof(UInt64), new TypeDefinitionVerifier(new CallbackObject(typeof(UInt64))));

            RoundTripTypeDefinition(typeof(Single), new TypeDefinitionVerifier(new CallbackObject(typeof(Single))));
            RoundTripTypeDefinition(typeof(Double), new TypeDefinitionVerifier(new CallbackObject(typeof(Double))));

            RoundTripTypeDefinition(typeof(Char), new TypeDefinitionVerifier(new CallbackObject(typeof(Char))));
            RoundTripTypeDefinition(typeof(String), new TypeDefinitionVerifier(new CallbackObject(typeof(String))));
            
            SosEnumDefinition enumDefinition = new SosEnumDefinition();
            enumDefinition.Add(DayOfWeek.Sunday.ToString(), (Int32)DayOfWeek.Sunday);
            enumDefinition.Add(DayOfWeek.Monday.ToString(), (Int32)DayOfWeek.Monday);
            enumDefinition.Add(DayOfWeek.Tuesday.ToString(), (Int32)DayOfWeek.Tuesday);
            enumDefinition.Add(DayOfWeek.Wednesday.ToString(), (Int32)DayOfWeek.Wednesday);
            enumDefinition.Add(DayOfWeek.Thursday.ToString(), (Int32)DayOfWeek.Thursday);
            enumDefinition.Add(DayOfWeek.Friday.ToString(), (Int32)DayOfWeek.Friday);
            enumDefinition.Add(DayOfWeek.Saturday.ToString(), (Int32)DayOfWeek.Saturday);
            RoundTripTypeDefinition(typeof(DayOfWeek), new TypeDefinitionVerifier(new CallbackObject(enumDefinition)));
            
            RoundTripTypeDefinition(typeof(Object), new TypeDefinitionVerifier(new CallbackObject(typeof(Object))));

            //
            // Arrays
            //
            RoundTripTypeDefinition(typeof(Boolean[]), new TypeDefinitionVerifier(new CallbackObject("Boolean")));
            RoundTripTypeDefinition(typeof(SByte[]), new TypeDefinitionVerifier(new CallbackObject("SByte")));
            RoundTripTypeDefinition(typeof(Byte[]), new TypeDefinitionVerifier(new CallbackObject("Byte")));
            RoundTripTypeDefinition(typeof(Int16[]), new TypeDefinitionVerifier(new CallbackObject("Int16")));
            RoundTripTypeDefinition(typeof(UInt16[]), new TypeDefinitionVerifier(new CallbackObject("UInt16")));
            RoundTripTypeDefinition(typeof(Int32[]), new TypeDefinitionVerifier(new CallbackObject("Int32")));
            RoundTripTypeDefinition(typeof(UInt32[]), new TypeDefinitionVerifier(new CallbackObject("UInt32")));
            RoundTripTypeDefinition(typeof(Int64[]), new TypeDefinitionVerifier(new CallbackObject("Int64")));
            RoundTripTypeDefinition(typeof(UInt64[]), new TypeDefinitionVerifier(new CallbackObject("UInt64")));

            RoundTripTypeDefinition(typeof(Single[]), new TypeDefinitionVerifier(new CallbackObject("Single")));
            RoundTripTypeDefinition(typeof(Double[]), new TypeDefinitionVerifier(new CallbackObject("Double")));

            RoundTripTypeDefinition(typeof(Char[]), new TypeDefinitionVerifier(new CallbackObject("Char")));
            RoundTripTypeDefinition(typeof(String[]), new TypeDefinitionVerifier(new CallbackObject("String")));

            RoundTripTypeDefinition(typeof(Object[]), new TypeDefinitionVerifier(new CallbackObject("System.Object")));

            //
            // User Defined Types
            //
            SosObjectDefinition firstObjectDefinition = new SosObjectDefinition(
                "myBoolean","Boolean",
                "myByte","Byte",
                "myUshort","UInt16",
                "myShort","Int16",
                "myUint","UInt32",
                "myInt","Int32",
                "myUlong","UInt64",
                "myLong","Int64",
                "dayOfWeek","System.DayOfWeek",
                "myObject","System.Object",
                "myString","String"
                );
            RoundTripTypeDefinition(typeof(TestClasses.ClassWithPrimitiveTypes), new TypeDefinitionVerifier(new CallbackObject(firstObjectDefinition)));


            SosObjectDefinition secondObjectDefinition = new SosObjectDefinition(
                "booleanArray", "Boolean[]",
                "sbyteArray", "SByte[]",
                "byteArray", "Byte[]",
                "ushortArray", "UInt16[]",
                "shortArray", "Int16[]",
                "uintArray", "UInt32[]",
                "intArray", "Int32[]",
                "ulongArray", "UInt64[]",
                "longArray", "Int64[]",
                "enumArray", "System.DayOfWeek[]",
                "stringArray", "String[]"
                );
            RoundTripTypeDefinition(typeof(TestClasses.ClassWithArrays), new TypeDefinitionVerifier(new CallbackObject(secondObjectDefinition)));

            /*
            SosObjectDefinition weirdObjectDefinition = new SosObjectDefinition(

                );
            RoundTripTypeDefinition(typeof(TestClasses.ClassWithWeirdTypes), new TypeDefinitionVerifier(new CallbackObject(weirdObjectDefinition)));
            */

        }
        void RoundTripTypeDefinition(Type t, TypeDefinitionVerifier verifier)
        {
            String typeDefinition = t.SosTypeDefinition();
            Console.WriteLine("Testing type definition: '{0}'", typeDefinition);
            SosTypes.ParseSosTypeDefinition(verifier, typeDefinition);
        }
        class CallbackObject
        {
            //enum Function {PrimitiveTypeOrEnum, ArrayType, Object };
            //public Function function;

            public Type type;
            public SosEnumDefinition enumDefinition;
            public SosObjectDefinition objectDefinition;
            public String elementTypeDefinition;
            public CallbackObject(Type type)
            {
                this.type = type;
            }
            public CallbackObject(SosEnumDefinition enumDefinition)
            {
                this.enumDefinition = enumDefinition;
            }
            public CallbackObject(SosObjectDefinition objectDefinition)
            {
                this.objectDefinition = objectDefinition;
            }
            public CallbackObject(String elementTypeDefinition)
            {
                this.elementTypeDefinition = elementTypeDefinition;
            }
            String ExpectedCallback()
            {
                if(type != null)
                {
                    if(!type.IsEnum) return String.Format("PrimitiveType({0})", type.FullName);
                    return String.Format("Enum({0})", type.FullName);
                }
                if (objectDefinition != null) return String.Format("ObjectType({0})", objectDefinition.TypeDefinition());
                if (enumDefinition != null) return enumDefinition.TypeDefinition();
                if (elementTypeDefinition != null) return String.Format("ArrayType(\"{0}\")", elementTypeDefinition);
                throw new InvalidOperationException();
            }
            public void AssertPrimitiveTypeCall(Type type)
            {
                Assert.AreEqual(this.type, type, String.Format("Expected {0} callback but got PrimitiveType({1})", ExpectedCallback(), type.FullName));
            }
            public void AssertEnumTypeCall(SosEnumDefinition enumDefinition)
            {
                Assert.AreEqual(this.enumDefinition, enumDefinition, String.Format("Expected {0} callback but got Enum({1})", ExpectedCallback(), enumDefinition.TypeDefinition()));
            }
            public void AssertArrayTypeCall(String elementTypeDefinition)
            {
                Assert.AreEqual(this.elementTypeDefinition, elementTypeDefinition, String.Format("Expected {0} callback but got ArrayType({1})", ExpectedCallback(), elementTypeDefinition));
            }
            public void AssertObjectTypeCall(SosObjectDefinition objectDefinition)
            {
                Assert.AreEqual(this.type, type, String.Format("Expected {0} callback but got {1}", ExpectedCallback(), objectDefinition.TypeDefinition()));
            }
        }

        class TypeDefinitionVerifier : ISosTypeDefinitionCallback
        {
            CallbackObject[] expectedCallbacks;
            Int32 callbackIndex;

            public TypeDefinitionVerifier(params CallbackObject[] expectedCallbacks)
            {
                this.expectedCallbacks = expectedCallbacks;
                this.callbackIndex = 0;
            }

            public void VerifyDone()
            {
                if (callbackIndex < expectedCallbacks.Length)
                    Assert.Fail(String.Format("Expected '{0}' callbacks but only got '{1}'", expectedCallbacks.Length, callbackIndex));
            }

            public void PrimitiveType(Type type)
            {
                if(callbackIndex >= expectedCallbacks.Length)
                    Assert.Fail(String.Format("Only expected '{0}' callbacks but got more", expectedCallbacks.Length, callbackIndex));
                CallbackObject expectedCallback = expectedCallbacks[callbackIndex++];
                expectedCallback.AssertPrimitiveTypeCall(type);
            }
            public void Enum(SosEnumDefinition enumDefinition)
            {
                if (callbackIndex >= expectedCallbacks.Length)
                    Assert.Fail(String.Format("Only expected '{0}' callbacks but got more", expectedCallbacks.Length, callbackIndex));
                CallbackObject expectedCallback = expectedCallbacks[callbackIndex++];
                expectedCallback.AssertEnumTypeCall(enumDefinition);
            }
            public void ArrayType(string elementTypeDefinition)
            {
                if (callbackIndex >= expectedCallbacks.Length)
                    Assert.Fail(String.Format("Only expected '{0}' callbacks but got more", expectedCallbacks.Length, callbackIndex));
                CallbackObject expectedCallback = expectedCallbacks[callbackIndex++];
                expectedCallback.AssertArrayTypeCall(elementTypeDefinition);
            }
            public void ObjectType(SosObjectDefinition objectDefinition)
            {
                if (callbackIndex >= expectedCallbacks.Length)
                    Assert.Fail(String.Format("Only expected '{0}' callbacks but got more", expectedCallbacks.Length, callbackIndex));
                CallbackObject expectedCallback = expectedCallbacks[callbackIndex++];
                expectedCallback.AssertObjectTypeCall(objectDefinition);
            }
        }
    }
}
