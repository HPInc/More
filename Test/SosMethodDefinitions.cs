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
    public class TestMethodDefinitionsClass
    {
        [TestMethod]
        public void TestMethodDefinitions()
        {
            TestMethodDefinition("Void Method()", new SosMethodDefinition("Method", "Void"));
            TestMethodDefinition("     Void   Method   (  )  ", new SosMethodDefinition("Method", "Void"));
            TestMethodDefinition("     Void   Method   ( Int32 first  )  ", new SosMethodDefinition("Method", "Void", "Int32", "first"));
            TestMethodDefinition("     Void   Method.WithDot   ( Int32 first, SomeType second  )  ", new SosMethodDefinition("Method.WithDot", "Void", "Int32", "first", "SomeType", "second"));
        }

        public void TestMethodDefinition(String methodDefinitionString, SosMethodDefinition expectedMethodDefinition)
        {
            Console.WriteLine("Testing '{0}'", methodDefinitionString);
            SosMethodDefinition methodDefinition = SosTypes.ParseMethodDefinition(methodDefinitionString, 0);
            String diff = expectedMethodDefinition.Diff(methodDefinition);
            if (diff != null)
            {
                Assert.Fail("Expected diff to be null but was '{0}'", diff);
            }
            if (!expectedMethodDefinition.Equals(methodDefinition))
            {
                Assert.Fail(String.Format("Diff was null but Equals failed Expected '{0}' Actual '{1}'",
                    expectedMethodDefinition.Definition(), methodDefinition.Definition()));
            }
        }

        [TestMethod]
        public void TestMethodParameterDefinitions()
        {
            TestMethodParameterDefinition("()");
            TestMethodParameterDefinition("(    )");
            TestMethodParameterDefinition("(    )    ");
            TestMethodParameterDefinition("()    ");

            TestMethodParameterDefinition("(Int32 first)", "Int32", "first");
            TestMethodParameterDefinition("(Int32 another)", "Int32", "another");


            TestMethodParameterDefinition("(System.DayOfWeek day)", "System.DayOfWeek", "day");

            TestMethodParameterDefinition("(CustomType type,Int32 i)", "CustomType", "type", "Int32", "i");
            TestMethodParameterDefinition("(CustomType type, Int32 i )", "CustomType", "type", "Int32", "i");

            TestMethodParameterDefinition("(Boolean b, Int32 i, UInt32 u, AnotherType t )",
                "Boolean", "b", "Int32", "i", "UInt32", "u", "AnotherType", "t");

        }
        public void TestMethodParameterDefinition(String methodParametersDefinition, params String[] expectedTypesAndNames)
        {
            Console.WriteLine("Testing '{0}'", methodParametersDefinition);
            List<String> actualTypesAndNames = SosTypes.ParseMethodDefinitionParameters(methodParametersDefinition, 0);

            if (actualTypesAndNames == null || actualTypesAndNames.Count <= 0)
            {
                Assert.IsTrue(expectedTypesAndNames == null || expectedTypesAndNames.Length <= 0);
            }
            else
            {
                Assert.AreEqual(expectedTypesAndNames.Length, actualTypesAndNames.Count,
                    String.Format("Expected '{0}' types and names but got '{1}' from method definition '{2}'",
                    expectedTypesAndNames.Length, actualTypesAndNames.Count, methodParametersDefinition));
                for(int i = 0; i < expectedTypesAndNames.Length; i++)
                {
                    Assert.AreEqual(expectedTypesAndNames[i], actualTypesAndNames[i],
                        String.Format("Method Definition '{0}' expected '{1}' at index {2} but got '{2}'",
                        methodParametersDefinition, expectedTypesAndNames[i], i, actualTypesAndNames[i]));
                }
            }
        }
    }
}
