// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.IO;

using More;

namespace TemplateProcessor
{
    class TemplateProcessor
    {
        static void Usage()
        {
            Console.WriteLine("TemplateProcessor <input-file> <output-file> <vars>");
        }
        static Int32 Main(String[] args)
        {
            if (args.Length < 2)
            {
                Usage();
                return 1;
            }
            String inputFile = args[0];
            Console.WriteLine("InputFile  : {0}", inputFile);
            String outputFile = args[1];
            Console.WriteLine("OutputFile : {0}", outputFile);

            VarValue[] varValues = new VarValue[args.Length - 2];
            for (int i = 2; i < args.Length; i++)
            {
                String varValue = args[i];
                Int32 equalIndex = varValue.IndexOf('=');
                if (equalIndex < 0)
                {
                    Console.WriteLine("Error: expected 'var=value', but got '%s'", varValue);
                }
                String var = "$" + varValue.Remove(equalIndex);
                String value = varValue.Substring(equalIndex + 1);
                varValues[i - 2] = new VarValue(var, value);
                Console.WriteLine("Arg        : {0} = {1}", var, value);
            }

            if (!File.Exists(inputFile))
            {
                Console.WriteLine("Error: {0} does not exist", inputFile);
            }

            using (var writer = new StreamWriter(new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
            {
                using (var reader = new StreamReader(new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    Boolean printConditionalBlock = true;
                    Stack<Boolean> blockStack = new Stack<Boolean>();

                    while (true)
                    {
                        String line = reader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }

                        if (line.StartsWith("##"))
                        {
                            foreach (var varValue in varValues)
                            {
                                line = line.Replace(varValue.var, varValue.value);
                            }

                            String originalLine = line;
                            if (line.StartsWith("##if"))
                            {
                                line = line.Substring(5);

                                String left = line.Peel(out line);
                                String comparison = line.Peel(out line);
                                String right = line.Peel(out line);

                                Boolean mustMatch;
                                if (comparison == "==")
                                {
                                    mustMatch = true;
                                }
                                else if (comparison == "!=")
                                {
                                    mustMatch = false;
                                }
                                else
                                {
                                    Console.WriteLine("Expected '==' or '!=' but got '{0}'", comparison);
                                    return 1;
                                }

                                blockStack.Push(printConditionalBlock);
                                if (mustMatch)
                                {
                                    printConditionalBlock = left.Equals(right);
                                }
                                else
                                {
                                    printConditionalBlock = !left.Equals(right);
                                }
                            }
                            else if (line.StartsWith("##else"))
                            {
                                printConditionalBlock = !printConditionalBlock;
                            }
                            else if (line.StartsWith("##endif"))
                            {
                                if (blockStack.Count == 0)
                                {
                                    Console.WriteLine("Error: too many endifs");
                                    return 1;
                                }
                                printConditionalBlock = blockStack.Pop();
                            }
                            else
                            {
                                Console.WriteLine("Error: unknown directive '{0}'", originalLine);
                                return 0;
                            }

                        }
                        else if (printConditionalBlock)
                        {
                            foreach (var varValue in varValues)
                            {
                                line = line.Replace(varValue.var, varValue.value);
                            }
                            writer.WriteLine(line);
                        }
                    }

                    if (blockStack.Count > 0)
                    {
                        Console.WriteLine("Error: not enough endifs");
                        return 1;
                    }
                }
            }
            return 0;
        }
    }
    struct VarValue
    {
        public readonly String var;
        public readonly String value;
        public VarValue(String var, String value)
        {
            this.var = var;
            this.value = value;
        }
    }
}
