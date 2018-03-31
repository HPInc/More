using System;
using System.Collections.Generic;
using System.IO;

namespace More.BinaryFormat
{
    public enum BinaryFormatType
    {
        // Primitive Types

        //   Unsigned
        Byte       = 0,
        UInt16     = 1,
        UInt24     = 2,
        UInt32     = 3,
        UInt64     = 4,

        //   Signed
        SByte      = 5,
        Int16      = 6,
        Int24      = 7,
        Int32      = 8,
        Int64      = 9,

        // String types
        Ascii      = 10,

        // Enum/Flags Types
        Enum       = 11,
        Flags      = 12,

        // The Object Type
        Object     = 13,

        // The Packed Type
        Packed     = 14,

        // The Serializer Type
        Serializer = 15,

        // The Case Types
        If         = 16,
        Switch     = 17,
    }
    public enum ArraySizeTypeEnum
    {
        Fixed              = 0, // Array Size Length is 0
        Byte               = 1, // Array Size Length is 1
        UInt16             = 2, // Array Size Length is 2
        UInt24             = 3, // Array Size Length is 3
        UInt32             = 4, // Array Size Length is 4
        UInt64             = 5, // Array Size Length is 5
        BasedOnCommand     = 7, // Array Size Length is 0
    }
    public static class BinaryFormatTypeExtensions
    {
        public static Boolean IsValidUnderlyingEnumIntegerType(this BinaryFormatType type)
        {
            return type <= BinaryFormatType.UInt64;
        }
        public static Boolean IsIntegerType(this BinaryFormatType type)
        {
            return type <= BinaryFormatType.Int64;
        }
        public static BinaryFormatType ParseIntegerType(String typeString)
        {
            BinaryFormatType type;
            try { type = (BinaryFormatType)Enum.Parse(typeof(BinaryFormatType), typeString, true); }
            catch (ArgumentException) { throw new FormatException(String.Format("Unknown Type '{0}'", typeString)); }
            if (!type.IsIntegerType()) throw new InvalidOperationException(String.Format(
                 "Expected an integer type but got type '{0}'", type));
            return type;
        }
        public static Byte IntegerTypeByteCount(this BinaryFormatType integerType)
        {
            switch(integerType)
            {
                case BinaryFormatType.Byte  : return 1;
                case BinaryFormatType.UInt16: return 2;
                case BinaryFormatType.UInt24: return 3;
                case BinaryFormatType.UInt32: return 4;
                case BinaryFormatType.UInt64: return 8;
                case BinaryFormatType.SByte : return 1;
                case BinaryFormatType.Int16 : return 2;
                case BinaryFormatType.Int24 : return 3;
                case BinaryFormatType.Int32 : return 4;
                case BinaryFormatType.Int64 : return 8;
                default: throw new InvalidOperationException(String.Format("Type '{0}' is not an integer type", integerType));
            }
        }
        public static Byte LengthByteCount(this ArraySizeTypeEnum sizeType)
        {
            if (sizeType == ArraySizeTypeEnum.BasedOnCommand) return 0;
            return (Byte)sizeType;
        }
        public static Boolean IntegerTypeIsUnsigned(this BinaryFormatType type)
        {
            if (type <= BinaryFormatType.UInt64) return true;
            if (type <= BinaryFormatType.Int64 ) return false;
            throw new InvalidOperationException(String.Format("Cannot call this method on a non-integer type '{0}'", type));
        }
        public static String EnglishNumberString(this Byte value)
        {
            switch (value)
            {
                case 1: return "One";
                case 2: return "Two";
                case 3: return "Three";
                case 4: return "Four";
                case 8: return "Eight";
            }
            throw new InvalidOperationException(String.Format("Unsupported value '{0}'", value));
        }
    }
    public abstract class BinaryFormatArrayType
    {
        static readonly VariableLengthArrayType BasedOnCommandLength = new VariableLengthArrayType(ArraySizeTypeEnum.BasedOnCommand);

        static readonly VariableLengthArrayType ByteLength = new VariableLengthArrayType(ArraySizeTypeEnum.Byte);
        static readonly VariableLengthArrayType UInt16Length = new VariableLengthArrayType(ArraySizeTypeEnum.UInt16);
        static readonly VariableLengthArrayType UInt24Length = new VariableLengthArrayType(ArraySizeTypeEnum.UInt24);
        static readonly VariableLengthArrayType UInt32Length = new VariableLengthArrayType(ArraySizeTypeEnum.UInt32);
        static readonly VariableLengthArrayType UInt64Length = new VariableLengthArrayType(ArraySizeTypeEnum.UInt64);

        public static BinaryFormatArrayType Parse(LfdLine line, String sizeTypeString)
        {
#if WindowsCE
            throw new NotImplementedException();
#else
            if(String.IsNullOrEmpty(sizeTypeString))
            {
                return BasedOnCommandLength;
            }

            Char firstChar = sizeTypeString[0];

            if(firstChar >= '0' && firstChar <= '9')
            {
                UInt32 fixedLength;
                if(!UInt32.TryParse(sizeTypeString, out fixedLength))
                    throw new ParseException(line, "First character of array size type '{0}' was a number but coult not parse it as a number", sizeTypeString);
                
                return new FixedLengthArrayType(fixedLength);
            }
            
            ArraySizeTypeEnum typeEnum;
            try
            {
                typeEnum = (ArraySizeTypeEnum)Enum.Parse(typeof(ArraySizeTypeEnum), sizeTypeString);
            }
            catch (ArgumentException)
            {
                throw new ParseException(line,
                    "The array size type inside the brackets '{0}' is invalid, expected 'Byte','UInt16','UInt24' or 'UInt32'", sizeTypeString);
            }

            if(typeEnum == ArraySizeTypeEnum.Fixed)
                throw new InvalidOperationException("Fixed is an invalid array size type, use an unsigned integer to specify a fixed length array");

            switch (typeEnum)
            {
                case ArraySizeTypeEnum.BasedOnCommand: return BasedOnCommandLength;
                case ArraySizeTypeEnum.Byte          : return ByteLength;
                case ArraySizeTypeEnum.UInt16        : return UInt16Length;
                case ArraySizeTypeEnum.UInt24        : return UInt24Length;
                case ArraySizeTypeEnum.UInt32        : return UInt32Length;
                case ArraySizeTypeEnum.UInt64        : return UInt64Length;
            }

            throw new InvalidOperationException(String.Format("CodeBug: Unhandled ArraySizeTypeEnum {0}", typeEnum));
#endif
        }

        public readonly ArraySizeTypeEnum type;
        protected BinaryFormatArrayType(ArraySizeTypeEnum type)
        {
            this.type = type;
        }
        public abstract String GetArraySizeString();
        public Byte GetArraySizeByteCount()
        {
            if (type == ArraySizeTypeEnum.BasedOnCommand) return 0;
            return (Byte)type;
        }
        public abstract UInt32 GetFixedArraySize();
        public abstract String LengthSerializeExpression(String arrayString, String offsetString, String lengthString);
        public abstract String LengthDeserializeExpression(String arrayString, String offsetString, String assignToString);

        class VariableLengthArrayType : BinaryFormatArrayType
        {
            readonly Byte lengthByteCount;
            public VariableLengthArrayType(ArraySizeTypeEnum type)
                : base(type)
            {
                if (type == ArraySizeTypeEnum.Fixed) throw new InvalidOperationException("This class does not accept Fixed array types");
                this.lengthByteCount = type.LengthByteCount();
            }
            public override String GetArraySizeString()
            {
                if (type == ArraySizeTypeEnum.BasedOnCommand) return "[]";
                return "[" + type.ToString() + "]";
            }
            public override UInt32 GetFixedArraySize()
            {
                throw new InvalidOperationException("CodeBug: This method cannot be called on an array of variable length");
            }
            public override String LengthSerializeExpression(String arrayString, String offsetString, String lengthString)
            {
                if (type == ArraySizeTypeEnum.BasedOnCommand)
                    throw new InvalidOperationException(String.Format("Cannot call this method on VariableLengthArrayType '{0}'", type));
                if (lengthByteCount == 1)
                {
                    return String.Format("{0}[{1}] = (Byte){2}", arrayString, offsetString, lengthString);
                }
                else
                {
                    return String.Format("{0}.BigEndianSet{1}({2}, {3})",
                        arrayString, type, offsetString, lengthString);
                }
            }
            public override String LengthDeserializeExpression(String arrayString, String offsetString, String assignToString)
            {
                if (type == ArraySizeTypeEnum.BasedOnCommand)
                    throw new InvalidOperationException(String.Format("Cannot call this method on VariableLengthArrayType '{0}'", type));
                if (lengthByteCount == 1)
                {
                    return String.Format("{0} = (Byte){1}[{2}]", assignToString, arrayString, offsetString);
                }
                else
                {
                    return String.Format("{0} = {1}.BigEndianRead{2}({3})",
                        assignToString, arrayString, type, offsetString);
                }
            }
        }
        public class FixedLengthArrayType : BinaryFormatArrayType
        {
            public readonly UInt32 fixedLength;
            public FixedLengthArrayType(UInt32 fixedLength)
                : base(ArraySizeTypeEnum.Fixed)
            {
                this.fixedLength = fixedLength;
            }
            public override String GetArraySizeString()
            {
                return fixedLength.ToString();
            }
            public override UInt32 GetFixedArraySize()
            {
                return fixedLength;
            }
            public override String LengthSerializeExpression(String arrayString, String offsetString, String lengthString)
            {
                throw new InvalidOperationException(String.Format("Cannot call this method on FixedLengthArrayType '{0}'", type));
            }
            public override String LengthDeserializeExpression(String arrayString, String offsetString, String assignToString)
            {
                throw new InvalidOperationException(String.Format("Cannot call this method on FixedLengthArrayType '{0}'", type));
            }
        }
    }

    public abstract class TypeReference
    {
        public readonly String relativeTypeReferenceString;
        public readonly BinaryFormatType type;
        public readonly BinaryFormatArrayType arrayType;

        public TypeReference(String relativeTypeReferenceString, BinaryFormatType type, BinaryFormatArrayType arrayType)
        {
            this.relativeTypeReferenceString = relativeTypeReferenceString;
            this.type = type;
            this.arrayType = arrayType;
        }

        // return UInt32.Max if element serialization length is not fixed, otherwise, return fixed serialization length
        public abstract UInt32 FixedElementSerializationLength { get; }

        public abstract String ElementDynamicSerializationLengthExpression(String instanceString);
        public abstract String ElementSerializeExpression(String arrayString, String offsetString, String instanceString);
        public abstract String ElementFixedLengthDeserializeExpression(String arrayString, String offsetString);
        public abstract String ElementDeserializeArrayExpression(String arrayString, String offsetString, String countString);
        public abstract String ElementDataStringExpression(String builderString, String instanceString, Boolean small);

        //public abstract String SerializeString(String valueString);

        public abstract IntegerTypeReference AsIntegerTypeReference { get; }
        public abstract EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference { get; }
        public abstract ObjectTypeReference AsObjectTypeReference { get; }
        public abstract SerializerTypeReference AsSerializerTypeReference { get; }

        public virtual void WriteBinaryFormat(TextWriter writer, String fieldName)
        {
            writer.Write(relativeTypeReferenceString);

            if (arrayType != null) writer.WriteLine(arrayType.GetArraySizeString());

            writer.Write(' ');
            writer.WriteLine(fieldName);
        }

        public abstract String CodeBaseTypeString { get; }
        public String CodeTypeString()
        {
            if (type == BinaryFormatType.Serializer) return "ISerializer";
            if (type == BinaryFormatType.Ascii && arrayType != null) return "String";
            if (arrayType == null) return CodeBaseTypeString;
            return CodeBaseTypeString + "[]";
        }
    }
    public class IntegerTypeReference : TypeReference
    {
        public readonly Byte byteCount;

        public IntegerTypeReference(BinaryFormatType integerType, BinaryFormatArrayType arrayType)
            : base(integerType.ToString(), integerType, arrayType)
        {
            this.byteCount = integerType.IntegerTypeByteCount();
        }
        public override UInt32 FixedElementSerializationLength
        {
            get { return byteCount; }
        }
        public override String ElementDynamicSerializationLengthExpression(String instanceString)
        {
            throw new InvalidOperationException("CodeBug: this method should not be called on an integer type reference");
        }
        public override String ElementSerializeExpression(String arrayString, String offsetString, String instanceString)
        {
            if (byteCount == 1)
            {
                return String.Format("{0}[{1}] = {2}{3}", arrayString, offsetString, (type == BinaryFormatType.SByte) ? "(Byte)" : "", instanceString);
            }
            else
            {
                return String.Format("{0}.BigEndianSet{1}({2}, {3})",
                    arrayString, type, offsetString, instanceString);
            }
        }
        public override String ElementFixedLengthDeserializeExpression(String arrayString, String offsetString)
        {
            if (byteCount == 1)
            {
                return String.Format("{0}{1}[{2}]", (type == BinaryFormatType.SByte) ? "(SByte)" : "", arrayString, offsetString);
            }
            else
            {
                return String.Format("{0}.BigEndianRead{1}({2})",
                    arrayString, type, offsetString);
            }
        }
        public override String ElementDeserializeArrayExpression(String arrayString, String offsetString, String countString)
        {
            if (byteCount == 1)
            {
                return String.Format("{0}.CreateSub{1}Array({2}, {3})",
                    arrayString, (type == BinaryFormatType.Byte) ? "" : "SByte", offsetString, countString);
            }
            else
            {
                return String.Format("BigEndian{0}Serializer.Instance.FixedLengthDeserializeArray({1}, {2}, {3})",
                    type, arrayString, offsetString, countString);
            }
        }
        public override string ElementDataStringExpression(string builderString, string instanceString, bool small)
        {
            return String.Format("{0}.Append({1})", builderString, instanceString);
        }
        public override IntegerTypeReference AsIntegerTypeReference
        {
            get { return this; }
        }
        public override EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override ObjectTypeReference AsObjectTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override SerializerTypeReference AsSerializerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override string CodeBaseTypeString
        {
            get {
                switch (type)
                {
                    case BinaryFormatType.UInt24: return "UInt32";
                    case BinaryFormatType.Int24: return "Int32";
                    default: return type.ToString();
                }
            }
        }
    }
    public class AsciiTypeReference : TypeReference
    {
        public AsciiTypeReference(String typeString, BinaryFormatArrayType arrayType)
            : base(typeString, BinaryFormatType.Ascii, arrayType)
        {
        }
        public override UInt32 FixedElementSerializationLength
        {
            get { return 1; }
        }
        public override String ElementDynamicSerializationLengthExpression(String instanceString)
        {
            throw new InvalidOperationException("CodeBug: this method should not be called on an integer type reference");
        }
        public override String ElementSerializeExpression(String arrayString, String offsetString, String instanceString)
        {
            return String.Format("{0}[{1}] = (Byte){2}", arrayString, offsetString, instanceString);
        }
        public override String ElementFixedLengthDeserializeExpression(String arrayString, String offsetString)
        {
            return String.Format("(Byte){0}[{1}]", arrayString, offsetString);
        }
        public override String ElementDeserializeArrayExpression(String arrayString, String offsetString, String countString)
        {
            return String.Format("Encoding.ASCII.GetString({0}, (Int32){1}, (Int32){2})",
                arrayString, offsetString, countString);
        }
        public override string ElementDataStringExpression(string builderString, string instanceString, bool small)
        {
            return String.Format("{0}.Append({1})", builderString, instanceString);
        }
        public override IntegerTypeReference AsIntegerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override ObjectTypeReference AsObjectTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override SerializerTypeReference AsSerializerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override String CodeBaseTypeString
        {
            get { return "Char"; }
        }
    }
    public class EnumOrFlagsTypeReference : TypeReference
    {
        public readonly String relativeEnumReferenceTypeString;
        public readonly EnumOrFlagsDefinition definition;
        public EnumOrFlagsTypeReference(String relativeEnumReferenceTypeString, EnumOrFlagsDefinition definition, BinaryFormatArrayType arrayType)
            : base(relativeEnumReferenceTypeString, definition.isFlagsDefinition ? BinaryFormatType.Flags : BinaryFormatType.Enum, arrayType)
        {
            this.relativeEnumReferenceTypeString = relativeEnumReferenceTypeString;
            this.definition = definition;
        }
        public override UInt32 FixedElementSerializationLength
        {
            get { return definition.byteCount; }
        }
        public override String ElementDynamicSerializationLengthExpression(String instanceString)
        {
            throw new InvalidOperationException("CodeBug: this method should not be called on an enum type reference");
        }
        public override String ElementSerializeExpression(String arrayString, String offsetString, String instanceString)
        {
            if (definition.byteCount == 1)
            {
                return String.Format("{0}EnumSerializer<{1}>.Instance.FixedLengthSerialize({2}, {3}, {4})",
                    definition.underlyingIntegerType, definition.typeName, arrayString, offsetString, instanceString);
            }
            else
            {
                return String.Format("BigEndian{0}EnumSerializer<{1}>.{2}ByteInstance.FixedLengthSerialize({3}, {4}, {5})",
                    definition.underlyingIntegerType.IntegerTypeIsUnsigned() ? "Unsigned" : "Signed",
                    definition.typeName,
                    definition.byteCount.EnglishNumberString(),
                    arrayString, offsetString, instanceString);
            }
        }
        public override String ElementFixedLengthDeserializeExpression(String arrayString, String offsetString)
        {
            if (definition.byteCount == 1)
            {
                return String.Format("{0}EnumSerializer<{1}>.Instance.FixedLengthDeserialize({2}, {3})",
                    definition.underlyingIntegerType, definition.typeName, arrayString, offsetString);
            }
            else
            {
                return String.Format("BigEndian{0}EnumSerializer<{1}>.{2}ByteInstance.FixedLengthDeserialize({3}, {4})",
                    definition.underlyingIntegerType.IntegerTypeIsUnsigned() ? "Unsigned" : "Signed",
                    definition.typeName,
                    definition.byteCount.EnglishNumberString(),
                    arrayString, offsetString);
            }
        }
        public override String ElementDeserializeArrayExpression(String arrayString, String offsetString, String countString)
        {
            if (definition.byteCount == 1)
            {
                return String.Format("{0}EnumSerializer<{1}>.Instance.FixedLengthDeserializeArray({2}, {3}, {4})",
                    definition.underlyingIntegerType, definition.typeName, arrayString, offsetString, countString);
            }
            else
            {
                return String.Format("BigEndian{0}EnumSerializer<{1}>.{2}ByteInstance.FixedLengthDeserializeArray({3}, {4}, {5})",
                    definition.underlyingIntegerType.IntegerTypeIsUnsigned() ? "Unsigned" : "Signed",
                    definition.typeName,
                    definition.byteCount.EnglishNumberString(),
                    arrayString, offsetString, countString);
            }
        }
        public override string ElementDataStringExpression(string builderString, string instanceString, bool small)
        {
            return String.Format("{0}.Append({1})", builderString, instanceString);
        }
        public override IntegerTypeReference AsIntegerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference
        {
            get { return this; }
        }
        public override ObjectTypeReference AsObjectTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override SerializerTypeReference AsSerializerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override string CodeBaseTypeString
        {
            get { return relativeEnumReferenceTypeString; }
        }
    }
    public class ObjectTypeReference : TypeReference
    {
        public readonly String relativeObjectReferenceTypeString;
        public readonly NamedObjectDefinition definition;

        public ObjectTypeReference(String relativeObjectReferenceTypeString, NamedObjectDefinition definition, BinaryFormatArrayType arrayType)
            : base(relativeObjectReferenceTypeString, BinaryFormatType.Object, arrayType)
        {
            this.relativeObjectReferenceTypeString = relativeObjectReferenceTypeString;
            this.definition = definition;
        }
        public override UInt32 FixedElementSerializationLength
        {
            get { return definition.FixedSerializationLength; }
        }
        public override String ElementDynamicSerializationLengthExpression(String instanceString)
        {
            if(FixedElementSerializationLength != UInt32.MaxValue)
                throw new InvalidOperationException(
                    "CodeBug: this method should not be called on an object type reference with a fixed element serialization length");

            return String.Format("{0}.Serializer.SerializationLength({1})",
                definition.name, instanceString);
        }
        public override String ElementSerializeExpression(String arrayString, String offsetString, String instanceString)
        {
            return String.Format("{0}.Serializer.Serialize({1}, {2}, {3})",
                definition.name, arrayString, offsetString, instanceString);
        }
        public override String ElementFixedLengthDeserializeExpression(String arrayString, String offsetString)
        {
            return String.Format("{0}.Serializer.FixedLengthDeserialize({1}, {2})",
                definition.name, arrayString, offsetString);
        }
        public override String ElementDeserializeArrayExpression(String arrayString, String offsetString, String countString)
        {
            if(FixedElementSerializationLength != UInt32.MaxValue)
            {
                return String.Format("{0}.Serializer.FixedLengthDeserializeArray({1}, {2}, {3})",
                    definition.name, arrayString, offsetString, countString);
            }
            throw new InvalidOperationException("This method only applies to fixed object types");
        }
        public override string ElementDataStringExpression(string builderString, string instanceString, bool small)
        {
            return String.Format("{0}.Serializer.Data{1}String({2}, {3})",
                definition.name, small ? "Small" : "", instanceString, builderString);
        }
        public override IntegerTypeReference AsIntegerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override ObjectTypeReference AsObjectTypeReference
        {
            get { return this; }
        }
        public override SerializerTypeReference AsSerializerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override void WriteBinaryFormat(TextWriter writer, String fieldName)
        {
            writer.Write(relativeObjectReferenceTypeString);

            if (arrayType != null) writer.Write(arrayType.GetArraySizeString());

            writer.Write(' ');
            writer.WriteLine(fieldName);
        }
        public override string CodeBaseTypeString
        {
            get { return relativeObjectReferenceTypeString; }
        }
    }
    public class SerializerTypeReference : TypeReference
    {
        BinaryFormatArrayType lengthType;
        public SerializerTypeReference(BinaryFormatArrayType lengthType, BinaryFormatArrayType arrayType)
            : base("Serializer", BinaryFormatType.Serializer, arrayType)
        {
            if (lengthType == null)
                throw new InvalidOperationException("A Serializer must have an array type");
            this.lengthType = lengthType;
        }
        public override UInt32 FixedElementSerializationLength
        {
            get { return UInt32.MaxValue; }
        }
        public override String ElementDynamicSerializationLengthExpression(String instanceString)
        {
            return String.Format("{0}.SerializationLength()", instanceString);
        }
        public override String ElementSerializeExpression(String arrayString, String offsetString, String instanceString)
        {
            return String.Format("{0}.Serialize({1}, {2})",
                instanceString, arrayString, offsetString);
        }
        public override String ElementFixedLengthDeserializeExpression(String arrayString, String offsetString)
        {
            throw new InvalidOperationException("Method ElementFixedLengthDeserializeExpression is not valid for a Serialier type");
        }
        public override String ElementDeserializeArrayExpression(String arrayString, String offsetString, String countString)
        {
            throw new InvalidOperationException("This method only applies to fixed object types");
        }
        public override string ElementDataStringExpression(string builderString, string instanceString, bool small)
        {
            return String.Format("{0}.Data{1}String({2})", instanceString, small ? "Small" : "", builderString);
        }
        public override IntegerTypeReference AsIntegerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override ObjectTypeReference AsObjectTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override SerializerTypeReference AsSerializerTypeReference
        {
            get { return this; }
        }
        public override string CodeBaseTypeString
        {
            get
            {
                return "ISerializer";
            }
        }
    }


    public class IfTypeReference : TypeReference
    {
        public readonly String conditionalString;
        Case trueCase;
        Case falseCase;

        public IfTypeReference(String conditionalString, Case trueCase, Case falseCase)
            : base("If", BinaryFormatType.If, null)
        {
            this.conditionalString = conditionalString;
            this.trueCase = trueCase;
            this.falseCase = falseCase;
        }
        public override UInt32 FixedElementSerializationLength
        {
            get { return UInt32.MaxValue; }
        }
        public override String ElementDynamicSerializationLengthExpression(String instanceString)
        {
            if (FixedElementSerializationLength != UInt32.MaxValue)
                throw new InvalidOperationException(
                    "CodeBug: this method should not be called on an object type reference with a fixed element serialization length");

            return String.Format("throw new NotImplementedException()/*(0).Serializer.SerializationLength(1)*/");
        }
        public override String ElementSerializeExpression(String arrayString, String offsetString, String instanceString)
        {
            return String.Format("{0}.Serializer.Serialize({1}, {2}, {3})",
                "IF", arrayString, offsetString, instanceString);
        }
        public override String ElementFixedLengthDeserializeExpression(String arrayString, String offsetString)
        {
            return String.Format("{0}.Serializer.FixedLengthDeserialize({1}, {2})",
                "IF", arrayString, offsetString);
        }
        public override String ElementDeserializeArrayExpression(String arrayString, String offsetString, String countString)
        {
            if (FixedElementSerializationLength != UInt32.MaxValue)
            {
                return String.Format("{0}.Serializer.FixedLengthDeserializeArray({1}, {2}, {3})",
                    "IF", arrayString, offsetString, countString);
            }
            throw new InvalidOperationException("This method only applies to fixed object types");
        }
        public override string ElementDataStringExpression(string builderString, string instanceString, bool small)
        {
            return String.Format("{0}.Serializer.Data{1}String({2}, {3})",
                "IF", small ? "Small" : "", instanceString, builderString);
        }
        public override IntegerTypeReference AsIntegerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override ObjectTypeReference AsObjectTypeReference
        {
            get { throw new InvalidOperationException("Not an ObjectTypeReference"); }
        }
        public override SerializerTypeReference AsSerializerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override void WriteBinaryFormat(TextWriter writer, String fieldName)
        {
            throw new NotImplementedException();
        }
        public override string CodeBaseTypeString
        {
            get { throw new NotImplementedException(); }
        }

    }


    public class Case
    {
        public readonly String caseString;
        public readonly ObjectDefinition objectDefinition;
        public Case(String caseString, ObjectDefinition objectDefinition)
        {
            this.caseString = caseString;
            this.objectDefinition = objectDefinition;
        }
    }
    public class SwitchTypeReference : TypeReference
    {
        public readonly String fieldString;
        List<Case> cases;

        public SwitchTypeReference(String fieldString, List<Case> cases)
            : base("Switch", BinaryFormatType.Switch, null)
        {
            this.fieldString = fieldString;
            this.cases = cases;
        }
        public override UInt32 FixedElementSerializationLength
        {
            get { return UInt32.MaxValue; }
        }
        public override String ElementDynamicSerializationLengthExpression(String instanceString)
        {
            if (FixedElementSerializationLength != UInt32.MaxValue)
                throw new InvalidOperationException(
                    "CodeBug: this method should not be called on an object type reference with a fixed element serialization length");

            return String.Format("throw new NotImplementedException()/*(0).Serializer.SerializationLength(1)*/");
        }
        public override String ElementSerializeExpression(String arrayString, String offsetString, String instanceString)
        {
            return String.Format("{0}.Serializer.Serialize({1}, {2}, {3})",
                "SWITCH", arrayString, offsetString, instanceString);
        }
        public override String ElementFixedLengthDeserializeExpression(String arrayString, String offsetString)
        {
            return String.Format("{0}.Serializer.FixedLengthDeserialize({1}, {2})",
                "SWITCH", arrayString, offsetString);
        }
        public override String ElementDeserializeArrayExpression(String arrayString, String offsetString, String countString)
        {
            if (FixedElementSerializationLength != UInt32.MaxValue)
            {
                return String.Format("{0}.Serializer.FixedLengthDeserializeArray({1}, {2}, {3})",
                    "SWITCH", arrayString, offsetString, countString);
            }
            throw new InvalidOperationException("This method only applies to fixed object types");
        }
        public override string ElementDataStringExpression(string builderString, string instanceString, bool small)
        {
            return String.Format("{0}.Serializer.Data{1}String({2}, {3})",
                "SWITCH", small ? "Small" : "", instanceString, builderString);
        }
        public override IntegerTypeReference AsIntegerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override ObjectTypeReference AsObjectTypeReference
        {
            get { throw new InvalidOperationException("Not an ObjectTypeReference"); }
        }
        public override SerializerTypeReference AsSerializerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override void WriteBinaryFormat(TextWriter writer, String fieldName)
        {
            throw new NotImplementedException();
        }
        public override string CodeBaseTypeString
        {
            get { throw new NotImplementedException(); }
        }
    }
}