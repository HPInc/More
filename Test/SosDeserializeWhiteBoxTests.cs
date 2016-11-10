// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestClasses;

namespace More.Net
{
    [TestClass]
    public class DeserializeWhiteBoxTests
    {
        static StringBuilder builder = new StringBuilder();
        static void Case(Object expectedObject, String sosString)
        {
            Util.TestDeserialization(builder, expectedObject, sosString);
        }
        static void Case(Object expectedObject, String sosString, Int32 expectedSosStringLengthUsed)
        {
            Util.TestDeserialization(builder, expectedObject, sosString, expectedSosStringLengthUsed);
        }
        static void BadCase(Type expectedType, String sosString)
        {
            Util.TestDeserializeFormatException(builder, expectedType, sosString);
        }
        static void ExpectNullCase(Type expectedType, String sosString)
        {
            Util.TestDeserializationToNull(builder, expectedType, sosString);
        }


        //
        // This test attempts to excercie the Sos.Deserialize() method.
        // It attempts to reach every line of code in the function
        //
        [TestMethod]
        public void WhiteBoxDeserializeTests()
        {
            //
            // Empty String
            //
            BadCase(typeof(String), "");

            //
            // Boolean
            //
            Case(false, "false");
            Case(true, "true");
            BadCase(typeof(Boolean), "0");
            BadCase(typeof(Boolean), "falsE");
            BadCase(typeof(Boolean), "truE");

            //
            // Single/Double keywords
            //
            Case(Single.NaN, "nan");
            Case(Single.NaN, "NAN");
            Case(Single.NaN, "Nan");

            Case(Double.NaN, "nan");
            Case(Double.NaN, "NAN");
            Case(Double.NaN, "NAN");

            Case(Single.PositiveInfinity, "infinity");
            Case(Single.PositiveInfinity, "INFINITY");
            Case(Single.PositiveInfinity, "Infinity");

            Case(Double.PositiveInfinity, "infinity");
            Case(Double.PositiveInfinity, "INFINITY");
            Case(Double.PositiveInfinity, "Infinity");

            Case(Single.NegativeInfinity, "-infinity");
            Case(Single.NegativeInfinity, "-INFINITY");
            Case(Single.NegativeInfinity, "-Infinity");

            Case(Double.NegativeInfinity, "-infinity");
            Case(Double.NegativeInfinity, "-INFINITY");
            Case(Double.NegativeInfinity, "-Infinity");

            //
            // Expected number
            //
            BadCase(typeof(SByte), "{");
            BadCase(typeof(Byte), "[");
            BadCase(typeof(Int16), "<");
            BadCase(typeof(UInt16), ",");
            BadCase(typeof(Int32), "_");
            BadCase(typeof(UInt32), "a");
            BadCase(typeof(Int64), "e");
            BadCase(typeof(Single), "e");
            BadCase(typeof(Double), "f");

            //
            // No need to test numbers, they will be tested elsewhere
            //

            //
            // Enums
            //
            Case(DayOfWeek.Sunday, "0");
            Case(DayOfWeek.Sunday, "Sunday");
            BadCase(typeof(DayOfWeek), "f");
            BadCase(typeof(DayOfWeek), "_");
            BadCase(typeof(DayOfWeek), "[");

            //
            // Nullable
            //
            BadCase(typeof(Char), "null");
            ExpectNullCase(typeof(String), "null");
            ExpectNullCase(typeof(Boolean[]), "null");
            ExpectNullCase(typeof(NoEmptyConstructor), "null");
            ExpectNullCase(typeof(NoEmptyConstructor[]), "null");

            //
            // Strings and Chars
            //
            BadCase(typeof(Char)  , @"""");
            BadCase(typeof(String), @"""");
            BadCase(typeof(Char)  , "\"   ");
            BadCase(typeof(String), "\"  ");
            BadCase(typeof(Char)  , "\"123");
            BadCase(typeof(String), "\"123");

            BadCase(typeof(Char), @"""12""");
            Case("12", @"""12""");
            
            BadCase(typeof(Char)  , @"""\");
            BadCase(typeof(String), @"""\");
            BadCase(typeof(Char)  , @"""\1234");
            BadCase(typeof(String), @"""\1234");
            
            BadCase(typeof(Char)  , @"""\1234\""");
            BadCase(typeof(String), @"""\1234\""");
            
            BadCase(typeof(String), @"""\""");
            BadCase(typeof(String), @"""\\  \""");
            BadCase(typeof(String), @"""\\\\  \\\""");
            BadCase(typeof(String), @"""\\\\\\  \\\\\""");
            
            Case(@"\", @"""\\""");
            Case(@"\  \", @"""\\  \\""");
            Case(@"\  \\", @"""\\  \\\\""");
            Case(@"\\  \\\", @"""\\\\  \\\\\\""");
            
            Case('\\'  , @"""\\""");
            Case(@"\", @"""\\""");

            Case('"'  , @"""\""""");
            Case(@"""", @"""\""""");

            Case('\n'  , @"""\n""");
            Case("\n", @"""\n""");

            Case('\r'  , @"""\r""");
            Case("\r", @"""\r""");

            Case('\0'  , @"""\0""");
            Case("\0", @"""\0""");              

            Case('\uABCD'  , @"""\uabcd""");
            Case("\uABCD", @"""\uabcd""");

            Case('\xAB'  , @"""\xab""");
            Case("\xAB", @"""\xab""");

            // Non-quoted strings/chars
            Case('a', "a");
            Case("a", "a");

            Case('c', "cb", 1);

            Case('1', "1");
            Case("123abcdefg", "123abcdefg");

            Case("123abc", "123abc,", 6); // a middle item in an array/object/table
            Case("123abc", "123abc]", 6); // The last item in an array
            Case("123abc", "123abc}", 6); // The last item in an object
            Case("123abc", "123abc:", 6); // The last item in a row of a table
            Case("123abc", "123abc>", 6); // The last item in a table
            
            Case("123abc", "123abc ", 6); // The last item in a table
            Case("123abc", "123abc\t", 6); // The last item in a table
            Case("123abc", "123abc\r", 6); // The last item in a table
            Case("123abc", "123abc\n", 6); // The last item in a table

            //
            // Arrays
            //
            BadCase(typeof(Int32[]), "[");
            BadCase(typeof(Int32[]), "[ ");

            Case(new Int32[0], "[]");


        }
    }
}
