using System;
using System.IO;

namespace More.BinaryFormat
{
    public interface ICodeGenerator
    {
        void GenerateInstanceSerializerConstructor(TextWriter writer, UInt32 tabs, String className, String fieldName, IntegerTypeReference type);
        void GenerateInstanceSerializerConstructor(TextWriter writer, UInt32 tabs, String className, String fieldName, EnumOrFlagsTypeReference type);
        void GenerateInstanceSerializerConstructor(TextWriter writer, UInt32 tabs, String className, String fieldName, ObjectTypeReference type);
        void GenerateInstanceSerializerConstructor(TextWriter writer, UInt32 tabs, String className, String fieldName, SerializerTypeReference type);
        /*
        void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String className, String fieldName, IntegerTypeReference type);
        void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String className, String fieldName, EnumOrFlagsTypeReference type);
        void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String className, String fieldName, ObjectTypeReference type);
        void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String className, String fieldName, SerializerTypeReference type);
        */
    }
}