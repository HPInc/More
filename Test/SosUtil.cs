// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestClasses
{
    class NoEmptyConstructor
    {
        public NoEmptyConstructor(Int32 value)
        {
        }
    }

    class SubclassHasNoEmptyConstructor
    {
        public NoEmptyConstructor n;
        public SubclassHasNoEmptyConstructor() { }
    }

    public class GenericClass<T>
    {
    }

    public class ClassWithWeirdTypes
    {
        public GenericClass<Int32> genericInt32Class;
        public GenericClass<GenericClass<Int32>> genericGenericInt32Class;
        public GenericClass<String>[] genericStringArrayClass;
    }

    public abstract class AbstractClass
    {
    }

    public class DeclaringClass
    {
        public class ClassInsideAClass
        {
        }
    }


    public class ClassWithPrimitiveTypes
    {
        readonly private Boolean myBoolean;
        protected Byte myByte;
        public UInt16 myUshort;
        public Int16 myShort;
        readonly public UInt32 myUint;
        readonly public Int32 myInt;
        public UInt64 myUlong;
        public Int64 myLong;
        public DayOfWeek dayOfWeek;
        public Object myObject;
        public String myString;
    }
    public struct StructWithPrimitiveTypes
    {
        private Boolean myBoolean;
        private Byte myByte;
        public UInt16 myUshort;
        public Int16 myShort;
        public UInt32 myUint;
        public Int32 myInt;
        public UInt64 myUlong;
        public Int64 myLong;
        public DayOfWeek dayOfWeek;
        public Object myObject;
        public String myString;
        public StructWithPrimitiveTypes(Int32 a)
        {
            this.myBoolean = false;
            this.myByte = 0;
            this.myUshort = 0;
            this.myShort = 0;
            this.myUint = 0;
            this.myInt = 0;
            this.myUlong = 0;
            this.myLong = 0;
            this.dayOfWeek = DayOfWeek.Sunday;
            this.myObject = new Object();
            this.myString = null;
        }
    }
    public class ClassWithArrays
    {
        private Boolean[] booleanArray;
        public SByte[] sByteArray;
        protected Byte[] byteArray;
        public UInt16[] ushortArray;
        public Int16[] shortArray;
        public UInt32[] uintArray;
        public Int32[] intArray;
        public UInt64[] ulongArray;
        public Int64[] longArray;
        public DayOfWeek[] enumArray;
        public Object[] objectArray;
        public String[] stringArray;
    }
    public class SuperClassWithPrimitivesAndArrays
    {
        public ClassWithPrimitiveTypes c;
        public ClassWithArrays ca;
        public SuperClassWithPrimitivesAndArrays[] mess;
    }

    public class BooleanAndInt
    {
        Boolean b;
        Int32 i;
        public BooleanAndInt(Boolean b, Int32 i)
        {
            this.b = b;
            this.i = i;
        }
    }
    public class BooleanWrapper
    {
        Boolean b;
    }


}


namespace More
{
    public static class Util
    {
        public static void TestDeserializationToNull(StringBuilder builder, Type type, String str)
        {
            TestDeserializationToNull(builder, type, str, str.Length);
        }
        public static void TestDeserializationToNull(StringBuilder builder, Type type, String str, Int32 expectedLengthUsed)
        {
            Object obj;
            Int32 offset = Sos.Deserialize(out obj, type, str, 0, str.Length);

            Console.WriteLine("String '{0}' Object After Deserialization '{1}'", str, obj);

            Assert.AreEqual(expectedLengthUsed, offset);
            Assert.IsNull(obj);
        }
        public static void TestDeserialization(StringBuilder builder, Object expectedObject, String str)
        {
            TestDeserialization(builder, expectedObject, str, str.Length);
        }
        public static void TestDeserialization(StringBuilder builder, Object expectedObject, String str, Int32 expectedLengthUsed)
        {
            Object newObject;
            Int32 offset = Sos.Deserialize(out newObject, expectedObject.GetType(), str, 0, str.Length);

            Console.WriteLine("String '{0}' Object After Deserialization '{1}'", str, newObject);

            String diffMessage = expectedObject.Diff(newObject);
            if (diffMessage != null) Assert.Fail(diffMessage);

            Assert.AreEqual(expectedLengthUsed, offset);
        }
        public static void TestDeserializeFormatException(StringBuilder builder, Type type, String str)
        {
            Object obj;
            try
            {
                Int32 offset = Sos.Deserialize(out obj, type, str, 0, str.Length);
                Assert.Fail("Expected FormatException but didn't get one for string '{0}'", str);
            }
            catch (FormatException e)
            {
                Console.WriteLine("String '{0}' Produced FormatException '{1}'", str, e.Message);
            }
        }
        public static void TestSerializer(StringBuilder builder, Object obj)
        {
            builder.Length = 0;
            obj.SerializeObject(builder);
            String seralizationString = builder.ToString();

            //Console.WriteLine("SerializationString '{0}'", seralizationString);

            Object newObject = null;

            Int32 offset = -1;
            try
            {
                offset = Sos.Deserialize(out newObject, obj.GetType(), seralizationString, 0, seralizationString.Length);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Deserialization failed: {0}. Serialized string was '{1}'", e, seralizationString), e);
            }


            /*
            String objString;
            try
            {
                objString = obj.ToString();
            }
            catch (NullReferenceException)
            {
                objString = "[NullReferenceException in ToString() Method]";
            }
            String newObjectString;
            try
            {
                newObjectString = newObject.ToString();
            }
            catch (NullReferenceException)
            {
                newObjectString = "[NullReferenceException in ToString() Method]";
            }
            Console.WriteLine("Before Serialization '{0}' ({1}) SerializeString '{2}' After '{3}' ({4})",
                objString, obj.GetType(), seralizationString, newObjectString, newObject.GetType());
            */
 

            Assert.AreEqual(seralizationString.Length, offset);

            String diffMessage = obj.Diff(newObject);
            if (diffMessage != null) Assert.Fail(diffMessage);
        }
    }
}