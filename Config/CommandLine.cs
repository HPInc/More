// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;

#if WindowsCE
using EnumReflection = System.MissingInCEEnumReflection;
#else
using EnumReflection = System.Enum;
#endif

namespace More
{
    public class CommandLineException : SystemException
    {
        public CommandLineException(String message) : base(message) { }
    }
    public class CommandLineParser
    {
        public static String CreateOptionString(Char letter, String name)
        {
            if (letter == '\0')
            {
                if (name == null)
                {
                    return "<no letter or name>";
                }
                return "--" + name;
            }
            if (name == null)
            {
                return "-" + letter;
            }
            return String.Format("-{0} or --{1}", letter, name);
        }

        static Int32 DefaultConsoleWidth
        {
            get
            {
#if WindowsCE
                return 80;
#else
                try { return Console.BufferWidth; }
                catch (Exception) { return 80; }
#endif
            }
        }

        Int32 consoleWidth;

        readonly Dictionary<Char, CommandLineOption> optionLetters;
        readonly Dictionary<String, CommandLineOption> optionNames;
        readonly List<CommandLineOption> options;

        Int32 maxUsageLeftColumnWidth;

        public CommandLineParser()
            : this(DefaultConsoleWidth)
        {
        }
        public CommandLineParser(Int32 consoleWidth)
        {
            this.consoleWidth = consoleWidth;
            this.optionLetters = new Dictionary<Char, CommandLineOption>();
            this.optionNames = new Dictionary<String, CommandLineOption>();
            this.options = new List<CommandLineOption>();

            this.maxUsageLeftColumnWidth = 0;
        }
        public void Add(CommandLineOption option)
        {
            if (option.letter == '\0' && option.name == null)
            {
                throw new ArgumentException(String.Format("Invalid Option '{0}' has no letter and no name", option), "option");
            }

            CommandLineOption otherOption;
            if (option.letter != '\0')
            {
                if (optionLetters.TryGetValue(option.letter, out otherOption))
                {
                    throw new InvalidOperationException(String.Format("You've added two options with same letter '{0}' ({1}) and ({2})",
                        option.letter, otherOption, option));
                }
                optionLetters.Add(option.letter, option);
            }

            if (option.name != null)
            {
                if (optionNames.TryGetValue(option.name, out otherOption))
                {
                    throw new InvalidOperationException(String.Format("You've added two options with same name '{0}' ({1}) and ({2})",
                        option.name, otherOption, option));
                }
                optionNames.Add(option.name, option);
            }

            this.options.Add(option);

            //
            // Update maximum usage left column length
            //
            if (option.usageLeftColumn.Length > maxUsageLeftColumnWidth)
            {
                maxUsageLeftColumnWidth = option.usageLeftColumn.Length;
                if (maxUsageLeftColumnWidth >= consoleWidth - 40)
                {
                    consoleWidth = maxUsageLeftColumnWidth + 40;
                }
            }
        }
        public List<String> Parse(String[] args)
        {
            List<String> nonOptionArguments = new List<String>(args.Length);

            for (int originalArgIndex = 0; originalArgIndex < args.Length; originalArgIndex++)
            {
                String current = args[originalArgIndex];

                if (String.IsNullOrEmpty(current))
                {
                    continue;
                }

                if (current[0] != '-' || current.Length <= 1)
                {
                    nonOptionArguments.Add(current);
                    continue;
                }

                if (current[1] == '-')
                {
                    if (current.Length <= 2)
                    {
                        nonOptionArguments.Add(current);
                        continue;
                    }

                    String optionName = current.Substring(2);

                    CommandLineOption option;
                    if (!optionNames.TryGetValue(optionName, out option))
                    {
                        throw new CommandLineException(String.Format("Unknown Option Name '{0}'", optionName));
                    }

                    ProcessOption(option, args, true, ref originalArgIndex);
                }
                else
                {
                    for (int letterIndex = 1; letterIndex < current.Length; letterIndex++)
                    {
                        Char optionCharacter = current[letterIndex];

                        CommandLineOption option;
                        if (!optionLetters.TryGetValue(optionCharacter, out option))
                            throw new CommandLineException(String.Format("Unknown Option Letter '{0}' in argument '{1}'", optionCharacter, current));

                        ProcessOption(option, args, letterIndex >= current.Length - 1, ref originalArgIndex);
                    }
                }
            }
            return nonOptionArguments;
        }
        void ProcessOption(CommandLineOption option, String[] args, Boolean optionIsLastOrAlone, ref Int32 originalArgIndex)
        {
            // Check for duplicates
            if (option.set)
            {
                throw new CommandLineException(String.Format("Found option '{0}' twice", option));
            }
            option.set = true;

            if (option.hasArg)
            {
                if (!optionIsLastOrAlone)
                    throw new CommandLineException(String.Format("Option '{0}' requires an argument but it is not the last letter in a list of options", option));

                originalArgIndex++;
                if (originalArgIndex >= args.Length)
                    throw new CommandLineException(String.Format("Option '{0}' requires an argument but there was none", option));

                option.ParseArg(args[originalArgIndex]);
            }
        }

        public Int32 ErrorAndUsage(String errorMessage, params Object[] obj)
        {
            return ErrorAndUsage(String.Format(errorMessage, obj));
        }
        public Int32 ErrorAndUsage(String errorMessage)
        {
            Console.WriteLine(errorMessage);
            PrintUsage();
            return -1;
        }

        public virtual void PrintUsageHeader()
        {
            Console.WriteLine("  not specified");
        }
        public virtual void PrintUsageFooter()
        {
        }
        public void PrintUsage()
        {
            PrintUsageHeader();

            if (options.Count > 0)
            {
                Console.WriteLine("Options:");
                foreach (CommandLineOption option in options)
                {
                    option.PrintUsage(maxUsageLeftColumnWidth + 2, consoleWidth - maxUsageLeftColumnWidth - 2);
                }
            }

            PrintUsageFooter();
        }
        public void PrintOptions()
        {
            Console.WriteLine("Options Status:");
            Console.WriteLine("Set Opt        Name");
            foreach (CommandLineOption option in options)
            {
                Console.WriteLine("[{0}] -{1} <arg> {2}", option.set ? "x" : " ", option.letter, option.name);
            }
        }

        public static void PrintRightColumn(String str, Int32 leftColumn, Int32 rightColumn, Boolean startAtLeftColumn)
        {
            if (startAtLeftColumn)
            {
                Console.Write("{0," + leftColumn + "}", String.Empty);
            }

            int strOffset = 0;
            while (true)
            {
                Int32 nextLength = str.Length - strOffset;
                if (nextLength > rightColumn)
                {
                    nextLength = rightColumn;
                }

                Console.WriteLine(str.Substring(strOffset, nextLength));
                strOffset += nextLength;
                if (strOffset >= str.Length) break;

                Console.Write("{0," + leftColumn + "}", String.Empty);
            }
        }
    }
    public abstract class CommandLineOption
    {
        public readonly Char letter;
        public readonly String name;
        public readonly Boolean hasArg;
        public readonly String description;

        public readonly String usageLeftColumn;

        public Boolean set;

        protected CommandLineOption(Char letter, String name, Boolean hasArg, String description)
        {
            this.letter = letter;
            this.name = name;
            this.hasArg = hasArg;
            this.description = description;

            if (letter == '\0')
            {
                this.usageLeftColumn = String.Format("  --{0}{1}", name, (hasArg) ? " <arg>" : String.Empty);
            }
            else
            {
                this.usageLeftColumn = String.Format("  -{0}{1}{2}", letter,
                    (name == null) ? "" : ",--" + name,
                    (hasArg) ? " <arg>" : String.Empty);
            }

            this.set = false;
        }
        public void PrintUsage(Int32 leftColumnLength, Int32 rightColumnLength)
        {
            Console.Write(usageLeftColumn + "{0," + (leftColumnLength - usageLeftColumn.Length) + "}", "");

            CommandLineParser.PrintRightColumn(description, leftColumnLength, rightColumnLength, false);

            // Print Arg Values
            String usageArgValues = UsageArgValues();
            if (usageArgValues != null)
            {
                CommandLineParser.PrintRightColumn(usageArgValues, leftColumnLength, rightColumnLength, true);
            }
        }
        internal abstract String UsageArgValues();
        public abstract void ParseArg(String arg);

        public override string ToString()
        {
            return String.Format("Letter '{0}'", letter);
        }
    }
    public class CommandLineSwitch : CommandLineOption
    {
        public CommandLineSwitch(Char letter, String description)
            : base(letter, null, false, description)
        {
        }
        public CommandLineSwitch(String name, String description)
            : base('\0', name, false, description)
        {
        }
        public CommandLineSwitch(Char letter, String name, String description)
            : base(letter, name, false, description)
        {
        }
        internal override String UsageArgValues()
        {
            return null;
        }
        public override void ParseArg(String arg)
        {
            throw new InvalidOperationException("Cannot parse an argument from an option with no argument");
        }
    }
    public class CommandLineArgument<T> : CommandLineOption
    {
        static Parser<T> defaultParser;

        Parser<T> parser;

        Boolean hasDefault;
        T defaultValue;

        T value;

        public CommandLineArgument(Parser<T> parser, Char letter, String description)
            : this(parser, letter, null, description)
        {
        }
        public CommandLineArgument(Parser<T> parser, String name, String description)
            : this(parser, '\0', name, description)
        {
        }
        public CommandLineArgument(Parser<T> parser, Char letter, String name, String description)
            : base(letter, name, true, description)
        {
            if (parser == null)
            {
                throw new ArgumentNullException("parser");
            }
            this.parser = parser;
        }
        public T Default
        {
            get
            {
                return defaultValue;
            }
            set
            {
                this.hasDefault = true;
                this.defaultValue = value;
                this.value = value;
            }
        }
        public T ArgValue
        {
            get { return value; }
        }
        internal override String UsageArgValues()
        {
            if (hasDefault) return String.Format("(default={0})", defaultValue);
            return null;
        }
        public override void ParseArg(String arg)
        {
            this.value = parser(arg);
        }
    }
    public class CommandLineArgumentUInt32 : CommandLineArgument<UInt32>
    {
        public CommandLineArgumentUInt32(Char letter, String description)
            : base(UInt32.Parse, letter, null, description)
        {
        }
        public CommandLineArgumentUInt32(String name, String description)
            : base(UInt32.Parse, '\0', name, description)
        {
        }
        public CommandLineArgumentUInt32(Char letter, String name, String description)
            : base(UInt32.Parse, letter, name, description)
        {
        }
    }
    public class CommandLineArgumentInt32 : CommandLineArgument<Int32>
    {
        public CommandLineArgumentInt32(Char letter, String description)
            : base(Int32.Parse, letter, null, description)
        {
        }
        public CommandLineArgumentInt32(String name, String description)
            : base(Int32.Parse, '\0', name, description)
        {
        }
        public CommandLineArgumentInt32(Char letter, String name, String description)
            : base(Int32.Parse, letter, name, description)
        {
        }
    }
    public class CommandLineArgumentString : CommandLineArgument<String>
    {
        static String PassThrough(String s) { return s; }

        public CommandLineArgumentString(Char letter, String description)
            : base(PassThrough, letter, null, description)
        {
        }
        public CommandLineArgumentString(String name, String description)
            : base(PassThrough, '\0', name, description)
        {
        }
        public CommandLineArgumentString(Char letter, String name, String description)
            : base(PassThrough, letter, name, description)
        {
        }
    }
    public class CommandLineArgumentEnum<EnumType> : CommandLineOption
    {
        Boolean hasDefault;
        EnumType defaultValue;
        EnumType value;
        public CommandLineArgumentEnum(Char letter, String description)
            : base(letter, null, true, description)
        {
        }
        public CommandLineArgumentEnum(String name, String description)
            : base('\0', name, true, description)
        {
        }
        public CommandLineArgumentEnum(Char letter, String name, String description)
            : base(letter, name, true, description)
        {
        }
        public void SetDefault(EnumType defaultValue)
        {
            this.hasDefault = true;
            this.defaultValue = defaultValue;
            this.value = defaultValue;
        }
        public EnumType ArgValue
        {
            get { return value; }
        }
        internal override String UsageArgValues()
        {
            String[] enumNames = EnumReflection.GetNames(typeof(EnumType));

            StringBuilder builder = new StringBuilder();
            builder.Append("Values={");
            for (int i = 0; i < enumNames.Length; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(enumNames[i]);
            }
            builder.Append("}");
            if (hasDefault)
            {
                builder.Append(" Default=");
                builder.Append(defaultValue);
            }
            return builder.ToString();
        }
        public override void ParseArg(String arg)
        {
            this.value = (EnumType)Enum.Parse(typeof(EnumType), arg, true);
            if (this.value == null) throw new CommandLineException(String.Format("Could not parse '{0}' as an enum of type '{1}'",
                 arg, typeof(EnumType).Name));
        }
    }
}
