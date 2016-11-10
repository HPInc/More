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
    public class CommandLineParserTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var parser = new CommandLineParser();

            var apple = new CommandLineSwitch('a', "apple", "Should I use apples?"); 
            parser.Add(apple);

            var banana = new CommandLineSwitch('b', "banana", "Should I use bananas?");
            parser.Add(banana);

            CommandLineArgument<Int32> integer = new CommandLineArgument<Int32>(Int32.Parse, 'i', "int", "This options has a long description, I am going to keep talking about this argument until I reach a good long length for this description");
            parser.Add(integer);

            CommandLineArgumentEnum<DayOfWeek> day = new CommandLineArgumentEnum<DayOfWeek>('d', "day", "The day of the week");
            parser.Add(day);

            parser.PrintUsage();

            parser.Parse(new String[] {
                "-a", "--day", "Sunday", "-b", "-i", "32", "43", 
            });
            Assert.AreEqual(true, apple.set);
            Assert.AreEqual(true, banana.set);
            Assert.AreEqual(true, integer.set);
            Assert.AreEqual(true, day.set);
            Assert.AreEqual(DayOfWeek.Sunday, day.ArgValue);
        }

        [TestMethod]
        public void TestFailures()
        {
            var parser = new CommandLineParser();
            parser.Add(new CommandLineSwitch('a', "apple", "Should I use apples?"));

            try
            {
                parser.Add(new CommandLineSwitch('a', "Should I use apples?"));
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
