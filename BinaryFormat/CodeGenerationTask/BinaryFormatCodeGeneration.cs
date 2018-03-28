using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.Build.Framework;

namespace More.BinaryFormat
{
    class InputFileObject
    {
        public readonly String name;
        public readonly String contents;
        public InputFileObject(String name, Byte[] readBuffer, StringBuilder builder, Sha1Builder shaBuilder, Encoding encoding)
        {
            this.name = name;

            // Read the file
            builder.Length = 0;
            using (FileStream stream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                while (true)
                {
                    Int32 bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                    if (bytesRead <= 0) break;
                    shaBuilder.Add(readBuffer, 0, bytesRead);

                    String str = encoding.GetString(readBuffer, 0, bytesRead);
                    builder.Append(str);
                }
            }
            this.contents = builder.ToString();
        }
    }

    public class BinaryFormatCodeGeneration : ITask
    {
        const Boolean TaskFailed = false;
        const Boolean TaskSucceeded = true;

        IBuildEngine buildEngine;
        ITaskHost taskHost;

        String[] inputFiles;
        String outputFile;
        String @namespace;

        Boolean forceCodeGeneration = false;
        Boolean generateStructs = false;

        public IBuildEngine BuildEngine
        {
            get { return buildEngine; }
            set { this.buildEngine = value; }
        }
        public ITaskHost HostObject
        {
            get { return taskHost; }
            set { this.taskHost = value; }
        }

        [Required]
        public String[] InputFiles
        {
            get { return inputFiles; }
            set { this.inputFiles = value; }
        }
        [Required]
        public String OutputFile
        {
            get { return outputFile; }
            set { this.outputFile = value; }
        }
        [Required]
        public String Namespace
        {
            get { return @namespace; }
            set { this.@namespace = value; }
        }
        public Boolean ForceCodeGeneration
        {
            get { return forceCodeGeneration; }
            set { this.forceCodeGeneration = value; }
        }

        public Boolean GenerateStructs
        {
            get { return generateStructs; }
            set { this.generateStructs = value; }
        }
        
        void Log(MessageImportance importance, String message)
        {
            buildEngine.LogMessageEvent(new BuildMessageEventArgs(message, null, null, importance));
        }
        void Log(MessageImportance importance, String fmt, params Object[] obj)
        {
            Log(importance, String.Format(fmt, obj));
        }

        void LogError(String message)
        {
            buildEngine.LogErrorEvent(new BuildErrorEventArgs(null, null, null, 0, 0, 0, 0, message, null, null));
        }
        void LogError(String fmt, params Object[] obj)
        {
            LogError(String.Format(fmt, obj));
        }

        /// <returns>Null if it could not retrieve an input hash</returns>
        public String TryGetSavedInputHash(String outputFile, out Sha1 savedInputHash)
        {
            if (!File.Exists(outputFile))
            {
                savedInputHash = default(Sha1);
                return null;
            }

            String firstLine;
            using (StreamReader reader = new StreamReader(new FileStream(outputFile, FileMode.Open, FileAccess.Read, FileShare.Read), true))
            {
                firstLine = reader.ReadLine();
            }

            // Output file is empty
            if (firstLine == null)
            {
                savedInputHash = default(Sha1);
                return null;
            }

            Int32 inputShaIndex = firstLine.IndexOf(BinaryFormatFile.InputShaPrefix);
            if (inputShaIndex < 0)
            {
                // First line does not contain hash
                buildEngine.LogWarningEvent(new BuildWarningEventArgs(null, null, outputFile, 1, inputShaIndex, 1, firstLine.Length,
                    String.Format("Expected first line to contain InputSha but it was '{0}'", firstLine), null, null));
                savedInputHash = default(Sha1);
                return null;
            }

            inputShaIndex += BinaryFormatFile.InputShaPrefix.Length;
            String savedInputHashString = firstLine.Substring(inputShaIndex);
            if (savedInputHashString.Length != BinaryFormatFile.ShaHexStringLength)
            {
                buildEngine.LogWarningEvent(new BuildWarningEventArgs(null, null, outputFile, 1, inputShaIndex, 1, firstLine.Length,
                    String.Format("Expected the InputSha of the output file to be 40 characters but it was {0}", savedInputHashString.Length), null, null));
                savedInputHash = default(Sha1);
                return null;
            }

            try
            {
                savedInputHash = Sha1.ParseHex(savedInputHashString, 0);
                return savedInputHashString;
            }
            catch (Exception e)
            {
                buildEngine.LogWarningEvent(new BuildWarningEventArgs(null, null, outputFile, 1, inputShaIndex, 1, firstLine.Length,
                    String.Format("Exception occured while parsing InputSha of the output file '{0}': {1}", savedInputHashString, e.Message), null, null));
                savedInputHash = default(Sha1);
                return null;
            }
        }

        public Boolean Execute()
        {
            //
            // Check Options
            //
            if (buildEngine == null) throw new ArgumentNullException("BuildEngine");

            if (inputFiles == null || inputFiles.Length <= 0)
            {
                LogError("Missing InputFiles");
                return TaskFailed;
            }
            if (String.IsNullOrEmpty(outputFile))
            {
                LogError("Missing OutputFile");
                return TaskFailed;
            }
            if (String.IsNullOrEmpty(@namespace))
            {
                LogError("Missing Namespace");
                return TaskFailed;
            }

            //
            // Check that input files exist
            //
            Int32 missingInputFileCount = 0;
            for (int i = 0; i < inputFiles.Length; i++)
            {
                String inputFile = inputFiles[i];
                if (!File.Exists(inputFiles[i]))
                {
                    missingInputFileCount++;
                    LogError("Missing InputFile '{0}'", inputFile);
                }
            }
            if (missingInputFileCount > 0)
            {
                LogError("{0} of the input files {1} missing", missingInputFileCount, (missingInputFileCount == 1) ? "is" : "are");
                return TaskFailed;
            }

            //
            // Load the input files
            //
            InputFileObject[] inputFileObjects = new InputFileObject[inputFiles.Length];

            Byte[] fileBuffer = new Byte[1024];
            Sha1Builder inputShaBuilder = new Sha1Builder();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < inputFileObjects.Length; i++)
            {
                inputFileObjects[i] = new InputFileObject(inputFiles[i], fileBuffer, builder, inputShaBuilder, Encoding.UTF8);
            }
            Sha1 inputHash = inputShaBuilder.Finish(false);
            String inputHashString = inputHash.ToString();

            if (forceCodeGeneration)
            {
                Log(MessageImportance.High, "Skipping the InputHash check because ForceCodeGeneration is set to true");
            }
            else
            {
                //
                // Try to get the saved hash from output file
                //
                Sha1 savedInputHash;
                String savedInputHashString = TryGetSavedInputHash(outputFile, out savedInputHash);
                if (savedInputHashString != null)
                {
                    if (inputHash.Equals(savedInputHash))
                    {
                        Log(MessageImportance.Normal, "Input hash matches saved input hash, no code generation done");
                        return TaskSucceeded;
                    }
                }
            }

            //
            // Parse BinaryFormat Files
            //
            BinaryFormatFile file = new BinaryFormatFile();
            for (int i = 0; i < inputFileObjects.Length; i++)
            {
                InputFileObject inputFileObject = inputFileObjects[i];
                BinaryFormatParser.ParseFile(file, new LfdTextReader(new StringReader(inputFileObject.contents)));
            }

            //
            // Generate the code
            //
            StringBuilder outputStringBuilder = new StringBuilder();

            using (StringWriter outputStringWriter = new StringWriter(outputStringBuilder))
            {
                // Save the hash first
                outputStringWriter.WriteLine("// {0}{1}", BinaryFormatFile.InputShaPrefix, inputHashString);
                CodeGenerator.GenerateCode(outputStringWriter, file, @namespace, generateStructs);
            }

            String outputContents = outputStringBuilder.ToString();

            FileExtensions.SaveStringToFile(outputFile, FileMode.Create, outputContents);

            return TaskSucceeded;
        }
    }
}
