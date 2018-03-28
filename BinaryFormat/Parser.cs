using System;
using System.Collections.Generic;
using System.IO;

namespace More.BinaryFormat
{
    class ParseException : Exception
    {
        public ParseException(LfdLine line, String reason)
            : base(String.Format("Parse Error (line {0}): {1}\n at line: \"{2}\"",
                line.actualLineNumber, reason, line.ToString()))
        {
        }
        public ParseException(LfdLine line, String reasonFmt, params Object[] reasonObj)
            : this(line, String.Format(reasonFmt, reasonObj))
        {
        }
        public ParseException(LfdLineReader reader, String reason)
            : base(String.Format("Parse Error (line {0}): {1}", reader.LineNumber, reason))
        {
        }
    }

    public static class BinaryFormatParser
    {
        static void VerifyFieldCount(LfdLine line, Int32 fieldCount)
        {
            int lineFieldCount = (line.fields == null) ? 0 : line.fields.Length;
            if (lineFieldCount != fieldCount) throw new ParseException(line, "Expected line to have {0} fields but had {1}", fieldCount, lineFieldCount);
        }
        static void VerifyMinFieldCount(LfdLine line, Int32 minFieldCount)
        {
            int lineFieldCount = (line.fields == null) ? 0 : line.fields.Length;
            if (lineFieldCount < minFieldCount) throw new ParseException(line, "Expected line to have at least {0} fields but had {1}", minFieldCount, lineFieldCount);
        }
        static void Debug(String message)
        {
            //Console.Error.WriteLine("[DEBUG] " + message);
            //Console.Out.WriteLine("[DEBUG] " + message);
        }
        static void Debug(String fmt, params Object[] obj)
        {
            Debug(String.Format(fmt, obj));
        }
        public static void ParseObjectFieldLine(BinaryFormatFile file, LfdLineReader reader, NamedObjectDefinition containingNamedObject,
            IFieldContainer containingObject, LfdLine fieldLine, out LfdLine nextLine)
        {
            //
            // Check if it is only a definition (enum or flag)
            //
            if (fieldLine.id.Equals("enum", StringComparison.OrdinalIgnoreCase))
            {
                ParseEnumOrFlagsDefinition(file, reader, containingNamedObject, fieldLine, out nextLine, false);
                return;
            }
            if (fieldLine.id.Equals("flags", StringComparison.OrdinalIgnoreCase))
            {
                ParseEnumOrFlagsDefinition(file, reader, containingNamedObject, fieldLine, out nextLine, true);
                return;
            }

            String typeString = fieldLine.id;

            //
            // The rest of the fields can have arrayParse the Array Size Type
            //
            BinaryFormatArrayType arrayType;

            int indexOfOpenBracket = typeString.IndexOf('[');
            if(indexOfOpenBracket < 0)
            {
                arrayType = null;
            }
            else
            {
                String arraySizeTypeString = typeString.Substring(indexOfOpenBracket + 1);

                typeString = typeString.Remove(indexOfOpenBracket);

                int indexOfCloseBracket = arraySizeTypeString.IndexOf(']');
                if (indexOfCloseBracket < 0)
                    throw new ParseException(fieldLine, "Found an opening bracket '[' without a closing bracket");
                if (indexOfCloseBracket != arraySizeTypeString.Length - 1)
                    throw new ParseException(fieldLine, "The array size type '{0}' had a closing bracket, but the closing bracket was not the last character", arraySizeTypeString);

                arraySizeTypeString = arraySizeTypeString.Remove(indexOfCloseBracket);
                arrayType = BinaryFormatArrayType.Parse(fieldLine, arraySizeTypeString);
            }

            //
            // Parse object inline definition
            //
            if (typeString.Equals("object", StringComparison.OrdinalIgnoreCase))
            {
                VerifyFieldCount(fieldLine, 1);

                String objectDefinitionAndFieldName = fieldLine.fields[0];

                NamedObjectDefinition fieldObjectDefinition = ParseObjectDefinition(file, reader, fieldLine,
                    containingNamedObject, objectDefinitionAndFieldName, out nextLine);

                containingObject.AddField(new ObjectDefinitionField(
                    new ObjectTypeReference(objectDefinitionAndFieldName, fieldObjectDefinition, arrayType),
                    objectDefinitionAndFieldName));
                return;
            }

            //
            // Check if it is a serializer
            //
            if (fieldLine.id.Equals("serializer", StringComparison.OrdinalIgnoreCase))
            {
                VerifyFieldCount(fieldLine, 2);
                String serializerLengthTypeString = fieldLine.fields[0];
                String serializerFieldName = fieldLine.fields[1];
                BinaryFormatArrayType serializerLengthType = BinaryFormatArrayType.Parse(fieldLine, serializerLengthTypeString);

                containingObject.AddField(new ObjectDefinitionField(new SerializerTypeReference(serializerLengthType, arrayType), serializerFieldName));

                nextLine = reader.ReadLineIgnoreComments();
                return;
            }

            //
            // Check if it is an 'if' type
            //
            if (typeString.Equals("if", StringComparison.OrdinalIgnoreCase))
            {
                VerifyFieldCount(fieldLine, 1);

                IfTypeReference ifType = ParseIf(file, reader, fieldLine,
                    containingNamedObject, out nextLine);

                containingObject.AddField(new ObjectDefinitionField(ifType, null));
                return;
            }

            //
            // Check if it is a switch type
            //
            if (typeString.Equals("switch", StringComparison.OrdinalIgnoreCase))
            {
                VerifyFieldCount(fieldLine, 1);

                SwitchTypeReference switchType = ParseSwitch(file, reader, fieldLine,
                    containingNamedObject, out nextLine);

                containingObject.AddField(new ObjectDefinitionField(switchType, null));
                return;
            }

            //
            // The field is only one line, so read the next line now for the caller
            //
            nextLine = reader.ReadLineIgnoreComments();



            EnumOrFlagsDefinition enumDefinition = file.TryGetEnumOrFlagsDefinition(containingNamedObject, typeString);
            if (enumDefinition != null)
            {
                VerifyFieldCount(fieldLine, 1);
                containingObject.AddField(new ObjectDefinitionField(new EnumOrFlagsTypeReference(typeString, enumDefinition, arrayType),
                    fieldLine.fields[0]));
                return;
            }

            // Check if it is an object type
            NamedObjectDefinition objectDefinition = file.TryGetObjectDefinition(containingNamedObject, typeString);
            if (objectDefinition != null)
            {
                if (fieldLine.fields == null || fieldLine.fields.Length <= 0)
                {
                    //
                    // Add each field from the object definition to the current object definition
                    //
                    List<ObjectDefinitionField> objectDefinitionFields = objectDefinition.Fields;
                    for (int i = 0; i < objectDefinitionFields.Count; i++)
                    {
                        ObjectDefinitionField fieldDefinition = objectDefinitionFields[i];
                        containingObject.AddField(fieldDefinition);
                    }
                }
                else if(fieldLine.fields.Length == 1)
                {
                    containingObject.AddField(new ObjectDefinitionField(
                        new ObjectTypeReference(typeString, objectDefinition, arrayType), fieldLine.fields[0]));
                }
                else
                {
                    throw new ParseException(fieldLine, "Expected line to have 0 or 1 fields but had {0}", fieldLine.fields.Length);
                }                
                return;
            }

            //
            // Check if it a string type
            //
            if (typeString.Equals("ascii", StringComparison.OrdinalIgnoreCase))
            {
                VerifyFieldCount(fieldLine, 1);
                containingObject.AddField(new ObjectDefinitionField(
                    new AsciiTypeReference(typeString, arrayType), fieldLine.fields[0]));
                return;
            }


            //
            // It must be an integer type
            //
            VerifyFieldCount(fieldLine, 1);

            BinaryFormatType type;
            try { type = (BinaryFormatType)Enum.Parse(typeof(BinaryFormatType), typeString, true); }
            catch (ArgumentException) { throw new FormatException(String.Format("Unknown Type '{0}'", typeString)); }
            if (!type.IsIntegerType()) throw new InvalidOperationException(String.Format("Unhandled type '{0}'", type));

            containingObject.AddField(new ObjectDefinitionField(new IntegerTypeReference(type, arrayType), fieldLine.fields[0]));
        }

        static void ParseEnumOrFlagsDefinition(BinaryFormatFile file, LfdLineReader reader, NamedObjectDefinition currentObjectDefinition, LfdLine enumDefinitionLine, out LfdLine nextLine, Boolean isFlagsDefinition)
        {
            String enumOrFlagsString = isFlagsDefinition ? "Flags" : "Enum";

            VerifyFieldCount(enumDefinitionLine, 2);

            String underlyingIntegerTypeString = enumDefinitionLine.fields[0];
            BinaryFormatType underlyingIntegerType = BinaryFormatTypeExtensions.ParseIntegerType(underlyingIntegerTypeString);

            EnumOrFlagsDefinition definition;
            try
            {
                definition = new EnumOrFlagsDefinition(file, isFlagsDefinition, currentObjectDefinition,
                underlyingIntegerType, enumDefinitionLine.fields[1]); }
            catch (FormatException e) { throw new ParseException(enumDefinitionLine, e.Message); }

            Debug("  Entering {0} '{1}' (IntegerType={2})", enumOrFlagsString, enumDefinitionLine.id, definition.underlyingIntegerType);

            //
            // Read enum values
            //
            nextLine = reader.ReadLineIgnoreComments();
            while (nextLine != null && nextLine.parent == enumDefinitionLine)
            {
                LfdLine enumValueLine = nextLine;
                VerifyFieldCount(enumValueLine, 1);
                Debug("    {0} {1} {2}", enumOrFlagsString, enumValueLine.id, enumValueLine.fields[0]);

                if (isFlagsDefinition)
                {
                    definition.Add(new FlagsValueDefinition(enumValueLine.fields[0], Byte.Parse(enumValueLine.id)));
                }
                else
                {
                    definition.Add(new EnumValueDefinition(enumValueLine.id, enumValueLine.fields[0]));
                }

                nextLine = reader.ReadLineIgnoreComments();
            }
            Debug("  Exiting {0} '{1}'", enumOrFlagsString, definition.typeName);
        }


        public static BinaryFormatFile ParseFile(String filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ParseFile(stream);
            }
        }
        public static BinaryFormatFile ParseFile(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                return ParseFile(reader);
            }
        }
        public static BinaryFormatFile ParseFile(TextReader reader)
        {
            using (LfdTextReader lfdReader = new LfdTextReader(reader))
            {
                return ParseFile(lfdReader);
            }
        }

        static IfTypeReference ParseIf(BinaryFormatFile file, LfdLineReader reader, LfdLine ifLine, NamedObjectDefinition containingNamedObject, out LfdLine nextLine)
        {
            Case trueCase, falseCase = null;

            String conditionString = ifLine.fields[0];
            Debug("Entering If Definition '{0}'", conditionString);

            ObjectDefinition trueCaseObjectDefinition = new ObjectDefinition(file);

            nextLine = reader.ReadLineIgnoreComments();
            while (nextLine != null && nextLine.parent == ifLine)
            {
                ParseObjectFieldLine(file, reader, containingNamedObject, trueCaseObjectDefinition, nextLine, out nextLine);
            }
            trueCaseObjectDefinition.CalculateFixedSerializationLength();
            trueCase = new Case(conditionString, trueCaseObjectDefinition);

            if (nextLine != null && nextLine.id.Equals("else", StringComparison.OrdinalIgnoreCase))
            {
                LfdLine falseCaseLine = nextLine;
                ObjectDefinition falseCaseObjectDefinition = new ObjectDefinition(file);

                nextLine = reader.ReadLineIgnoreComments();
                while (nextLine != null && nextLine.parent == falseCaseLine)
                {
                    ParseObjectFieldLine(file, reader, containingNamedObject, falseCaseObjectDefinition, nextLine, out nextLine);
                }
                falseCaseObjectDefinition.CalculateFixedSerializationLength();
                falseCase = new Case(conditionString, falseCaseObjectDefinition);
            }

            return new IfTypeReference(conditionString, trueCase, falseCase);
        }
        static SwitchTypeReference ParseSwitch(BinaryFormatFile file, LfdLineReader reader, LfdLine switchLine, NamedObjectDefinition containingNamedObject, out LfdLine nextLine)
        {
            List<Case> cases = new List<Case>();

            String switchFieldString = switchLine.fields[0];
            Debug("Entering Switch Definition '{0}'", switchFieldString);

            nextLine = reader.ReadLineIgnoreComments();
            while (nextLine != null && nextLine.parent == switchLine)
            {
                Boolean isCase = nextLine.id.Equals("case", StringComparison.OrdinalIgnoreCase);
                if (!isCase && !nextLine.id.Equals("default", StringComparison.OrdinalIgnoreCase))
                    throw new FormatException(String.Format("Expected 'Case' or 'Default' but got '{0}'", nextLine));

                VerifyFieldCount(nextLine, isCase ? 1 : 0);

                LfdLine currentCaseLine = nextLine;
                String currentCaseValueString = isCase ? nextLine.fields[0] : null;
                ObjectDefinition currentCaseObjectDefinition = new ObjectDefinition(file);
                
                nextLine = reader.ReadLineIgnoreComments();
                while (nextLine != null && nextLine.parent == currentCaseLine)
                {
                    ParseObjectFieldLine(file, reader, containingNamedObject, currentCaseObjectDefinition, nextLine, out nextLine);
                }

                currentCaseObjectDefinition.CalculateFixedSerializationLength();
                cases.Add(new Case(currentCaseValueString, currentCaseObjectDefinition));
            }

            return new SwitchTypeReference(switchFieldString, cases);
        }

        static NamedObjectDefinition ParseObjectDefinition(BinaryFormatFile file, LfdLineReader reader, LfdLine objectDefinitionLine,
            NamedObjectDefinition currentObjectDefinition, String objectDefinitionName, out LfdLine nextLine)
        {
            NamedObjectDefinition objectDefinition = new NamedObjectDefinition(file, objectDefinitionName,
                objectDefinitionName.ToLowerInvariant(), currentObjectDefinition);

            Debug("Entering Object Definition '{0}'", objectDefinition.name);

            nextLine = reader.ReadLineIgnoreComments();
            while (nextLine != null && nextLine.parent == objectDefinitionLine)
            {
                ParseObjectFieldLine(file, reader, objectDefinition, objectDefinition, nextLine, out nextLine);
            }

            objectDefinition.CalculateFixedSerializationLength();

            return objectDefinition;
        }
        public static BinaryFormatFile ParseFile(LfdLineReader reader)
        {
            BinaryFormatFile file = new BinaryFormatFile();
            ParseFile(file, reader);
            return file;
        }
        public static void ParseFile(BinaryFormatFile file, LfdLineReader reader)
        {
            LfdLine nextLine = reader.ReadLineIgnoreComments();
            while (nextLine != null)
            {
                if (nextLine.id.Equals("enum", StringComparison.OrdinalIgnoreCase))
                {
                    ParseEnumOrFlagsDefinition(file, reader, null, nextLine, out nextLine, false);
                }
                else if (nextLine.id.Equals("flags", StringComparison.OrdinalIgnoreCase))
                {
                    ParseEnumOrFlagsDefinition(file, reader, null, nextLine, out nextLine, true);
                }
                else
                {
                    LfdLine currentCommandLine = nextLine;

                    VerifyFieldCount(currentCommandLine, 0);
                    NamedObjectDefinition objectDefinition = ParseObjectDefinition(file, reader, currentCommandLine, null, currentCommandLine.id, out nextLine);
                }
            }
        }
    }
}