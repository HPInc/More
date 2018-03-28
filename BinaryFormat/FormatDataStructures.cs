using System;
using System.Collections.Generic;
using System.IO;

namespace More.BinaryFormat
{
    public class ObjectDefinitionField
    {
        public readonly TypeReference typeReference;
        public readonly String name;
        public ObjectDefinitionField(TypeReference typeReference, String name)
        {
            this.typeReference = typeReference;
            this.name = name;
        }
        public void WriteBinaryFormat(TextWriter writer)
        {
            typeReference.WriteBinaryFormat(writer, name);
        }
        //public abstract void GenerateReflectorConstructor(TextWriter writer, Int32 tabs, String destination);
    }


    public interface IFieldContainer
    {
        void AddField(ObjectDefinitionField field);
        void CalculateFixedSerializationLength();
    }

    public class ObjectDefinition : IFieldContainer
    {
        //public readonly NamedObjectDefinition objectDefinedIn;

        //public List<EnumOrFlagsDefinition> enumOrFlagDefinitions;
        //public List<NamedObjectDefinition> objectDefinitions;

        protected readonly List<ObjectDefinitionField> fields;
        public List<ObjectDefinitionField> Fields
        {
            get
            {
                if (!calculatedFixedSerializationLength) throw new InvalidOperationException(
                    "Cannot access fields until serialization length has been calculated");
                return fields;
            }
        }

        Boolean calculatedFixedSerializationLength;
        UInt32 fixedSerializationLength;
        public UInt32 FixedSerializationLength
        {
            get
            {
                if (!calculatedFixedSerializationLength) throw new InvalidOperationException(
                    "Cannot access FixedSerializationLength until serialization length has been calculated");
                return fixedSerializationLength;
            }
        }

        public ObjectDefinition(BinaryFormatFile file/*, NamedObjectDefinition objectDefinedIn*/)
        {
            //this.objectDefinedIn = objectDefinedIn;

            this.fields = new List<ObjectDefinitionField>();
            this.calculatedFixedSerializationLength = false;
        }
        /*
        public void AddEnumOrFlagDefinition(EnumOrFlagsDefinition definition)
        {
            if (enumOrFlagDefinitions == null) enumOrFlagDefinitions = new List<EnumOrFlagsDefinition>();
            enumOrFlagDefinitions.Add(definition);
        }
        public void AddObjectDefinition(NamedObjectDefinition definition)
        {
            if (objectDefinitions == null) objectDefinitions = new List<NamedObjectDefinition>();
            objectDefinitions.Add(definition);
        }
        */
        public void AddField(ObjectDefinitionField field)
        {
            if (calculatedFixedSerializationLength) throw new InvalidOperationException(
                "Cannot add fields after FixedSerializationLength has been calculated");
            fields.Add(field);
        }
        public void CalculateFixedSerializationLength()
        {
            if (calculatedFixedSerializationLength) throw new InvalidOperationException(
                "Cannot calculate FixedSerializationLength after it has already been calculated");

            UInt32 length = 0;
            for (int i = 0; i < fields.Count; i++)
            {
                ObjectDefinitionField field = fields[i];
                TypeReference fieldTypeReference = field.typeReference;
                UInt32 fieldFixedSerializationLength = fieldTypeReference.FixedElementSerializationLength;
                if (fieldFixedSerializationLength == UInt32.MaxValue)
                {
                    this.fixedSerializationLength = UInt32.MaxValue;
                    calculatedFixedSerializationLength = true;
                    return;
                }

                BinaryFormatArrayType arrayType = fieldTypeReference.arrayType;
                if (arrayType == null)
                {
                    length += fieldFixedSerializationLength;
                }
                else
                {
                    if (arrayType.type != ArraySizeTypeEnum.Fixed)
                    {
                        this.fixedSerializationLength = UInt32.MaxValue;
                        calculatedFixedSerializationLength = true;
                        return;
                    }
                    length += arrayType.GetFixedArraySize() * fieldFixedSerializationLength;
                }

            }

            this.fixedSerializationLength = length;
            calculatedFixedSerializationLength = true;
        }
        /*
        public void WriteBinaryFormat(TextWriter writer)
        {
            writer.WriteLine(name + " {");
            for (int i = 0; i < fields.Count; i++)
            {
                ObjectDefinitionField field = fields[i];
                field.WriteBinaryFormat(writer);
            }
        }
        */
    }

    public class NamedObjectDefinition : ObjectDefinition
    {
        public readonly String name;
        public readonly String nameLowerInvariant;

        public readonly String globalReferenceNameLowerInvariant;

        public readonly NamedObjectDefinition objectDefinedIn;

        public List<EnumOrFlagsDefinition> enumOrFlagDefinitions;
        public List<NamedObjectDefinition> objectDefinitions;

        public NamedObjectDefinition(BinaryFormatFile file, String name, String nameLowerInvariant, NamedObjectDefinition objectDefinedIn)
            : base(file)
        {
            this.name = name;
            this.nameLowerInvariant = nameLowerInvariant;

            this.globalReferenceNameLowerInvariant = (objectDefinedIn == null) ? nameLowerInvariant :
                (objectDefinedIn.globalReferenceNameLowerInvariant + "." + nameLowerInvariant);

            this.objectDefinedIn = objectDefinedIn;

            //this.fields = new List<ObjectDefinitionField>();
            //this.firstOptionalFieldIndex = -1;

            //this.calculatedFixedSerializationLength = false;

            //
            // Add definition to file and parent object
            //
            file.AddObjectDefinition(this);
            if (objectDefinedIn != null) objectDefinedIn.AddObjectDefinition(this);
        }
        public void AddEnumOrFlagDefinition(EnumOrFlagsDefinition definition)
        {
            if (enumOrFlagDefinitions == null) enumOrFlagDefinitions = new List<EnumOrFlagsDefinition>();
            enumOrFlagDefinitions.Add(definition);
        }
        public void AddObjectDefinition(NamedObjectDefinition definition)
        {
            if (objectDefinitions == null) objectDefinitions = new List<NamedObjectDefinition>();
            objectDefinitions.Add(definition);
        }
        public void WriteBinaryFormat(TextWriter writer)
        {
            writer.WriteLine(name + " {");
            for (int i = 0; i < fields.Count; i++)
            {
                ObjectDefinitionField field = fields[i];
                field.WriteBinaryFormat(writer);
            }
        }
    }
}