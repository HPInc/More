// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace More
{
    [TestClass]
    public class SimpleTypeTests
    {
        enum ByteEnum : byte { First, Second, Max = byte.MaxValue }
        enum SByteEnum : sbyte { First, Second, Max = sbyte.MaxValue, Min = sbyte.MinValue }
        enum UShortEnum : ushort { First, Second, Max = ushort.MaxValue }
        enum ShortEnum : short { First, Second, Max = short.MaxValue, Min = short.MinValue }
        enum UIntEnum : uint { First, Second, Max = uint.MaxValue }
        enum IntEnum : int { First, Second, Max = int.MaxValue, Min = int.MinValue }
        enum ULongEnum : ulong { First, Second, Max = ulong.MaxValue }
        enum LongEnum : long { First, Second, Max = long.MaxValue, Min = long.MinValue }

        enum WeirdEnum { Has_Underscore = 0 }

        [TestMethod]
        public void TestDeserializationOfSomeEnums()
        {
            StringBuilder builder = new StringBuilder();

            Util.TestDeserialization(builder, ByteEnum.First, "0");
            Util.TestDeserialization(builder, ByteEnum.Second, "1");
            Util.TestDeserialization(builder, ByteEnum.Max, Byte.MaxValue.ToString());

            Util.TestDeserialization(builder, ByteEnum.First, "fiRST");
            Util.TestDeserialization(builder, ByteEnum.Second, "second");
            Util.TestDeserialization(builder, ByteEnum.Max, "Max");

            Util.TestDeserialization(builder, SByteEnum.First, "0");
            Util.TestDeserialization(builder, SByteEnum.Second, "1");
            Util.TestDeserialization(builder, SByteEnum.Max, SByte.MaxValue.ToString());
            Util.TestDeserialization(builder, SByteEnum.Min, SByte.MinValue.ToString());

            Util.TestDeserialization(builder, SByteEnum.First, "fiRST");
            Util.TestDeserialization(builder, SByteEnum.Second, "second");
            Util.TestDeserialization(builder, SByteEnum.Max, "max");
            Util.TestDeserialization(builder, SByteEnum.Min, "min");

            Util.TestDeserialization(builder, WeirdEnum.Has_Underscore, "Has_Underscore");
            Util.TestDeserialization(builder, WeirdEnum.Has_Underscore, "has_uNDERSCORE");
            Util.TestDeserialization(builder, WeirdEnum.Has_Underscore, "0");
        }

        [TestMethod]
        public void TestForSomeEnums()
        {
            StringBuilder builder = new StringBuilder();

            Type[] enumTypes = new Type[]{
                typeof(DayOfWeek),
                typeof(ByteEnum),
                typeof(SByteEnum),
                typeof(UShortEnum),
                typeof(ShortEnum),
                typeof(UIntEnum),
                typeof(IntEnum),
                typeof(ULongEnum),
                typeof(LongEnum),
                typeof(WeirdEnum),
            };

            for (int i = 0; i < enumTypes.Length; i++)
            {
                Type enumType = enumTypes[i];

                Array values = Enum.GetValues(enumType);
                foreach (Object value in values)
                {
                    String valueStringAsNumber = Convert.ChangeType(value, Enum.GetUnderlyingType(enumType)).ToString();
                    Util.TestDeserialization(builder, value, valueStringAsNumber);

                    Util.TestSerializer(builder, value);
                }
            }
        }

        [TestMethod]
        public void TestPrimitiveLimits()
        {
            StringBuilder builder = new StringBuilder();

            Util.TestSerializer(builder, false);
            Util.TestSerializer(builder, true);

            //
            // Test Serializing Numbers
            //
            Util.TestSerializer(builder, Byte.MinValue);
            Util.TestSerializer(builder, Byte.MaxValue);

            Util.TestSerializer(builder, (SByte)0);
            Util.TestSerializer(builder, SByte.MinValue);
            Util.TestSerializer(builder, SByte.MaxValue);

            Util.TestSerializer(builder, UInt16.MinValue);
            Util.TestSerializer(builder, UInt16.MaxValue);

            Util.TestSerializer(builder, (Int16)0);
            Util.TestSerializer(builder, Int16.MinValue);
            Util.TestSerializer(builder, Int16.MaxValue);

            Util.TestSerializer(builder, UInt32.MinValue);
            Util.TestSerializer(builder, UInt32.MaxValue);

            Util.TestSerializer(builder, (Int32)0);
            Util.TestSerializer(builder, Int32.MinValue);
            Util.TestSerializer(builder, Int32.MaxValue);

            Util.TestSerializer(builder, UInt64.MinValue);
            Util.TestSerializer(builder, UInt64.MaxValue);

            Util.TestSerializer(builder, (Int64)0);
            Util.TestSerializer(builder, Int64.MinValue);
            Util.TestSerializer(builder, Int64.MaxValue);

            //
            // Test Single Floats
            //
            Util.TestDeserialization(builder, 1.234e10f, "1.234e10");
            Util.TestDeserialization(builder, 1.234e10f, "1.234E10");

            Util.TestSerializer(builder, 0.0f);
            Util.TestSerializer(builder, 1.0f);
            Util.TestSerializer(builder, Single.Epsilon);
            Util.TestSerializer(builder, Single.MinValue);
            Util.TestSerializer(builder, Single.MaxValue);
            Util.TestSerializer(builder, Single.NaN);
            Util.TestSerializer(builder, Single.NegativeInfinity);
            Util.TestSerializer(builder, Single.PositiveInfinity);

            //
            // Test Double Floats
            //
            Util.TestDeserialization(builder, 1.234e10, "1.234e10");
            Util.TestDeserialization(builder, 1.234e10, "1.234E10");

            Util.TestSerializer(builder, 0.0);
            Util.TestSerializer(builder, 1.0);
            Util.TestSerializer(builder, Double.Epsilon);
            Util.TestSerializer(builder, Double.MinValue);
            Util.TestSerializer(builder, Double.MaxValue);
            Util.TestSerializer(builder, Double.NaN);
            Util.TestSerializer(builder, Double.NegativeInfinity);
            Util.TestSerializer(builder, Double.PositiveInfinity);
        }

        [TestMethod]
        public void FullUTF16CharacterTestCoverage()
        {
            StringBuilder builder = new StringBuilder();

            Util.TestDeserialization(builder, '\0', @"""\0""");
            Util.TestDeserialization(builder, '\n', @"""\n""");
            Util.TestDeserialization(builder, '\r', @"""\r""");
            Util.TestDeserialization(builder, '\t', @"""\t""");
            Util.TestDeserialization(builder, '\\', @"""\\""");
            Util.TestDeserialization(builder, '\xab', @"""\xab""");
            Util.TestDeserialization(builder, '\x9F', @"""\x9F""");
            Util.TestDeserialization(builder, '\uabCe', @"""\uabCe""");
            Util.TestDeserialization(builder, '\u9F3C', @"""\u9F3C""");
            //Util.TestDeserialization(builder, '\a', @"""\a""");
            //Util.TestDeserialization(builder, '\b', @"""\b""");
            //Util.TestDeserialization(builder, '\f', @"""\f""");
            //Util.TestDeserialization(builder, '\v', @"""\v""");

            for (Int32 cAsInt = 0; cAsInt <= 65535; cAsInt++)
            {
                Char c = (Char)cAsInt;

                String seralizationString = c.SerializeChar();

                Object newObject = null;

                Int32 offset = -1;
                try
                {
                    offset = Sos.Deserialize(out newObject, typeof(Char), seralizationString, 0, seralizationString.Length);
                }
                catch (Exception e)
                {
                    throw new Exception(String.Format("Deserialization failed: {0}. Serialized string was '{1}'", e, seralizationString), e);
                }

                //Console.WriteLine("Before Serialization '{0}' ({1}) SerializeString '{2}' After '{3}' ({4})",
                //    c, typeof(Char), seralizationString, newObject, newObject.GetType());

                Assert.AreEqual(seralizationString.Length, offset);

                String diffMessage = c.Diff(newObject);
                if (diffMessage != null) Assert.Fail(diffMessage);
            }
        }

        [TestMethod]
        public void TestStrings()
        {
            StringBuilder builder = new StringBuilder();

            String[] strings = new String[] {
                String.Empty,
                "a",
                "b",
                "hello how are you",
                @"I am great",
                @"The little boy said ""Wow this is a cool test?""",
                @"Some escapes \ what the heck \\ ",
                @"\""",
                //"non-quotedstring",
            };

            for (int i = 0; i < strings.Length; i++)
            {
                Util.TestSerializer(builder, strings[i]);
            }
        }
    }    


    [TestClass]
    public class FloatingPointTests
    {
        public void TestValidFloatRegex(String floatString)
        {
            Console.WriteLine("ValidTestString '{0}'", floatString);

            //Match m = Sos.FloatNumberRegexBase10.Match(floatString);
            Int32 numberLength = Sos.FloatLength(floatString, 0, floatString.Length);
            String matchString = floatString.Substring(0, numberLength);

            Console.WriteLine("Matched '{0}'", matchString);
            Assert.AreEqual(floatString.Length, matchString.Length);
        }
        public void TestInvalidFloatRegex(String floatString)
        {
            Console.WriteLine("InvalidTestString '{0}'", floatString);
            //Match m = Sos.FloatNumberRegexBase10.Match(floatString);
            Int32 numberLength = Sos.FloatLength(floatString, 0, floatString.Length);
            String matchString = floatString.Substring(0, numberLength);

            Console.WriteLine("Invalid '{0}' Matched '{1}'", floatString, matchString);

            if (numberLength > 0)
            {
                try
                {
                    Single.Parse(matchString);
                    Assert.Fail("Expected parse to fail but didn't");
                }
                catch (FormatException)
                {
                }
            }
        }

        [TestMethod]
        public void TestGoodFloats()
        {
            String[] testStrings = new String[] {
                "0",
                "1",
                "109",
                "255",
                "1.000",
                "1.9",
                ".0",
                "0.0",
                ".192",
                ".9",
                "0.9",
            };

            for (int i = 0; i < testStrings.Length; i++)
            {
                TestValidFloatRegex(testStrings[i]);
            }
            for (int i = 0; i < testStrings.Length; i++)
            {
                TestValidFloatRegex("-" + testStrings[i]);
            }
            for (int i = 0; i < testStrings.Length; i++)
            {
                TestValidFloatRegex(testStrings[i] + "e108");
                TestValidFloatRegex(testStrings[i] + "E108");
                TestValidFloatRegex(testStrings[i] + "e+99");
                TestValidFloatRegex(testStrings[i] + "E+9");
                TestValidFloatRegex(testStrings[i] + "e-0");
                TestValidFloatRegex(testStrings[i] + "E-99");
            }
        }
        [TestMethod]
        public void TestBadFloats()
        {
            String[] badTestStrings = new String[] {
                "!",
                ".",
            };
            for (int i = 0; i < badTestStrings.Length; i++)
            {
                TestInvalidFloatRegex(badTestStrings[i]);
            }
        }
    }
}
