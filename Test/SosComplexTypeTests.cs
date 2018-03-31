// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Reflection;
using System.Runtime.Serialization;

using TestClasses;

namespace More
{
    [TestClass]
    public class UserDefinedTypes
    {
#if !WindowsCE
        [TestMethod]
        public void TestAllAvailableTypesInLoadedAssemblies()
        {
            StringBuilder builder = new StringBuilder();
            SosTypeSerializationVerifier verifier = new SosTypeSerializationVerifier();

            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < loadedAssemblies.Length; i++)
            {
                Assembly assembly = loadedAssemblies[i];

                PortableExecutableKinds peKind;
                ImageFileMachine imageFileMachine;
                assembly.ManifestModule.GetPEKind(out peKind, out imageFileMachine);
                Console.WriteLine("Assembly {0} PEKind {1} ImageFileMachine {2}", assembly, peKind, imageFileMachine);


                Type[] types = assembly.GetTypes();

                for (int j = 0; j < types.Length; j++)
                {
                    Type type = types[j];

                    if (type == typeof(String)) continue;
                    if (!type.IsVisible) continue;

                    if (type.FullName == "System.Net.Mail.MSAdminBase") continue;

                    /*
                    Console.WriteLine("Testing '{0}' {1}", type.FullName, type.Serialize());
                    Console.WriteLine("   type.AssemblyQualifiedName = " + type.AssemblyQualifiedName);
                    Console.WriteLine("   type.Attributes            = " + type.Attributes);
                    Console.WriteLine("   type.IsCOMObject           = " + type.IsCOMObject);
                    Console.WriteLine("   type.IsContextful          = " + type.IsContextful);
                    Console.WriteLine("   type.IsNested              = " + type.IsNested);
                    */
 
                    String cannotBeSerializedBecause = verifier.CannotBeSerializedBecause(type);
                    if (cannotBeSerializedBecause == null)
                    {
                        Console.WriteLine("{0} {1}", type.SosTypeName(), type.SosTypeDefinition());
                        Util.TestSerializer(builder, FormatterServices.GetUninitializedObject(type));
                    }
                    else
                    {
                        Console.WriteLine("{0} Cannot be serialized because {1}",
                            type.FullName, cannotBeSerializedBecause);
                    }
                }
            }
        }
#endif
        [TestMethod]
        public void TestSerialization()
        {
            StringBuilder builder = new StringBuilder();

            Util.TestSerializer(builder, new ClassWithPrimitiveTypes());
            Util.TestSerializer(builder, new ClassWithArrays());
            Util.TestSerializer(builder, new SuperClassWithPrimitivesAndArrays());
            Util.TestSerializer(builder, new StructWithPrimitiveTypes());
            Util.TestSerializer(builder, new ClassWithWeirdTypes());

            ClassWithPrimitiveTypes c = new ClassWithPrimitiveTypes();
            c.myObject = new Object();
            c.dayOfWeek = DayOfWeek.Friday;
            c.myString = "hello[]{}\"\"  \\";

            Util.TestSerializer(builder, c);

            ClassWithArrays ca = new ClassWithArrays();
            ca.ushortArray = new UInt16[] { 0, 99, ushort.MinValue, 7483, ushort.MaxValue, 7575 };
            ca.shortArray = new Int16[] { short.MinValue, short.MaxValue, 4848};
            ca.uintArray = new UInt32[] { uint.MinValue, uint.MaxValue, 9493, 382983, 444, 3443, 14427 };
            ca.intArray = new Int32[] { int.MinValue, int.MaxValue, -9494593, 384562983, 4244, 344523, 144227 };
            ca.ulongArray = new UInt64[] {ulong.MinValue, ulong.MaxValue };
            ca.longArray = new Int64[] { long.MinValue, long.MaxValue };
            ca.enumArray = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Saturday, DayOfWeek.Wednesday, DayOfWeek.Friday };
            ca.objectArray = new Object[] { new Object(), null, new Object() };
            ca.stringArray = new String[] { null, String.Empty, "hello", "[]", "{,,,}" };

            Console.WriteLine(ca.SerializeObject());
            Util.TestSerializer(builder, ca);

            SuperClassWithPrimitivesAndArrays sca = new SuperClassWithPrimitivesAndArrays();
            sca.c = c;
            sca.ca = ca;

            SuperClassWithPrimitivesAndArrays scaMess1 = new SuperClassWithPrimitivesAndArrays();
            SuperClassWithPrimitivesAndArrays scaMess2 = new SuperClassWithPrimitivesAndArrays();
            scaMess1.c = c;
            scaMess2.c = c;
            scaMess1.ca = ca;
            scaMess2.ca = ca;
            sca.mess = new SuperClassWithPrimitivesAndArrays[] { scaMess1, scaMess2 };


            Util.TestSerializer(builder, sca);
        }

        [TestMethod]
        public void TestBugsFound()
        {
            StringBuilder builder = new StringBuilder();

            Util.TestDeserialization(builder, 42, "42}", 2);
        }
    }
}
