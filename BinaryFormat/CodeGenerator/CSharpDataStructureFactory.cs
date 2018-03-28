using System;
using System.IO;

namespace More.BinaryFormat
{
    public class CSharpCodeGenerator : ICodeGenerator
    {
        public void GenerateInstanceSerializerConstructor(TextWriter writer, uint tabs, string className, string fieldName, IntegerTypeReference type)
        {
            if (type.arrayType == null)
            {
                writer.WriteLine(tabs * 4, "{0}{1}Serializer,", (type.byteCount == 1) ? "" : "BigEndian", type.type);
            }
            else
            {
                throw new NotImplementedException();
                //writer.WriteLine(tabs * 4, "{0}{1}Length{2}{3}Serializer", (type.arrayType., type.type);
            }
        }
        public void GenerateInstanceSerializerConstructor(TextWriter writer, uint tabs, string className, string fieldName, EnumOrFlagsTypeReference type)
        {
            throw new NotImplementedException();
        }
        public void GenerateInstanceSerializerConstructor(TextWriter writer, uint tabs, string className, string fieldName, ObjectTypeReference type)
        {
            throw new NotImplementedException();
        }
        public void GenerateInstanceSerializerConstructor(TextWriter writer, uint tabs, string className, string fieldName, SerializerTypeReference type)
        {
            throw new NotImplementedException();
        }


        /*
        String ArrayTypeToReflectorString(BinaryFormatArrayType arrayType)
        {
            if (arrayType == null) return "";
            if (arrayType.type == ArraySizeTypeEnum.BasedOnCommand) throw new NotImplementedException();
            return arrayType.type.ToString() + "Length";
        }

        public void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String className, String fieldName, IntegerTypeReference type)
        {
            if (type.arrayType == null)
            {
                String prefix = (type.type == BinaryFormtType.Byte) ? "" : "BigEndian";
                writer.WriteLine(tabs * 4, "new {0}{1}Reflector(typeof({2}), \"{3}\"),",
                    prefix, type.type, className, fieldName);
            }
            else
            {
                if (type.type == BinaryFormtType.Byte)
                {
                    if (type.arrayType.type == ArraySizeTypeEnum.Fixed)
                    {
                        writer.WriteLine(tabs * 4, "new FixedLengthByteArrayReflector(typeof({0}), \"{1}\", {2}),",
                            className, fieldName, type.arrayType.GetFixedArraySize());
                    }
                    else
                    {
                        writer.WriteLine(tabs * 4, "new ByteArrayReflector(typeof({0}), \"{1}\", {2}),",
                            className, fieldName, type.arrayType.GetArraySizeByteCount());
                    }
                }
                else
                {
                    writer.WriteLine(tabs * 4, "new FixedLengthElementArrayReflector<{0}>(typeof({1}), \"{2}\", {3}, {0}.Serializer, {4}),",
                        type.type, className, fieldName, type.arrayType.GetArraySizeByteCount(), type.byteCount);
                }
            }
        }
        public void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String className, String fieldName, EnumOrFlagsTypeReference type)
        {
            if (type.arrayType == null)
            {
                String signedString = type.definition.underlyingIntegerType.IntegerTypeIsUnsigned() ? "Unsigned" : "Signed";

                writer.WriteLine(tabs * 4, "new BigEndian{0}EnumReflector<{1}>(typeof({2}), \"{3}\", {4}),",
                    signedString, type.definition.typeName, className, fieldName, type.definition.byteCount);
            }
            else
            {
                writer.WriteLine(tabs * 4, "new FixedLengthElementArrayReflector<{0}>(typeof({1}), \"{2}\", {3}, {4}EnumSerializer<{0}>.Instance),",
                    type.definition.typeName, className, fieldName, type.arrayType.GetArraySizeByteCount(), type.definition.underlyingIntegerType);
            }
        }
        public void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String className, String fieldName, ObjectTypeReference type)
        {
            if (type.arrayType == null)
            {
                throw new NotImplementedException("Non-Array Object types are not implemented yet");
            }
            else
            {
                Int32 fixedSerializationLength = type.FixedSerializationLength;
                if (fixedSerializationLength >= 0)
                {
                    writer.WriteLine(tabs * 4, "new FixedLengthElementArrayReflector<{0}>(typeof({1}), \"{2}\", {3}, {0}.FixedSerializer, {4}),",
                        type.definition.name, className, fieldName, type.arrayType.GetArraySizeByteCount(), fixedSerializationLength);
                }
                else
                {
                    writer.WriteLine(tabs * 4, "new DynamicSizeElementArrayReflector<{0}>(typeof({1}), \"{2}\", {3}, {0}.DynamicSerializer),",
                        type.definition.name, className, fieldName, type.arrayType.GetArraySizeByteCount());
                }
            }
        }
        public void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String className, String fieldName, SerializerTypeReference type)
        {
            writer.WriteLine(tabs * 4, "new BinaryFormtFieldSerializerReflector(typeof({0}), \"{1}\", {2}),",
                className, fieldName, type.arrayType.GetArraySizeByteCount());
        }
        */
    }
}
