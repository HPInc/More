using System;
using System.Collections.Generic;
using System.IO;

namespace More.BinaryFormat
{
    class Options : CommandLineParser
    {
        public override void PrintUsageHeader()
        {
            Console.WriteLine("BinaryFormatCodeGenerator.exe <namespace> <input-file>");
        }
    }
    class Program
    {
        static Int32 Main(string[] args)
        {
            Options options = new Options();

            List<String> nonOptionArgs = options.Parse(args);

            if(nonOptionArgs.Count != 2)
            {
                return options.ErrorAndUsage("Expected 2 non-option arguments but got {0}", nonOptionArgs.Count);
            }

            String @namespace = nonOptionArgs[0];
            String inputFileName = nonOptionArgs[1];

            BinaryFormatFile file;
            using (StreamReader reader = new StreamReader(new FileStream(inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                file = BinaryFormatParser.ParseFile(reader);
            }

            CodeGenerator.GenerateCode(Console.Out, file, @namespace, false);

            return 0;
        }
    }

    
}
