using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More.BinaryFormat
{
    public class BinaryFormatFile
    {
        public const String InputShaPrefix = "InputSha ";
        public const Int32 ShaByteLength = 20;
        public const Int32 ShaHexStringLength = ShaByteLength * 2;

        //
        // Enum Definitions
        //
        readonly Dictionary<String, EnumOrFlagsDefinition> enumOrFlagsDefinitionMap =
            new Dictionary<String, EnumOrFlagsDefinition>(StringComparer.OrdinalIgnoreCase);
        readonly List<EnumOrFlagsDefinition> enumDefinitions = new List<EnumOrFlagsDefinition>();
        readonly List<EnumOrFlagsDefinition> flagsDefinitions = new List<EnumOrFlagsDefinition>();

        public IEnumerable<EnumOrFlagsDefinition> EnumOrFlagsDefinitions
        { get { return enumOrFlagsDefinitionMap.Values; } }
        public IEnumerable<EnumOrFlagsDefinition> EnumDefinitions
        { get { return enumDefinitions; } }
        public IEnumerable<EnumOrFlagsDefinition> FlagsDefinitions
        { get { return flagsDefinitions; } }

        //
        // Object Definitions
        //
        readonly List<NamedObjectDefinition> objectDefinitions = new List<NamedObjectDefinition>();
        readonly Dictionary<String, NamedObjectDefinition> objectDefinitionMap =
            new Dictionary<String, NamedObjectDefinition>(StringComparer.OrdinalIgnoreCase);
        public IEnumerable<NamedObjectDefinition> ObjectDefinitions { get { return objectDefinitions; } }

        // returns definition key
        public void AddEnumOrFlagsDefinition(EnumOrFlagsDefinition enumOrFlagsDefinition)
        {
            if(enumOrFlagsDefinitionMap.ContainsKey(enumOrFlagsDefinition.globalReferenceNameLowerInvariant))
                throw new InvalidOperationException(String.Format("Found enum type key '{0}' twice", enumOrFlagsDefinition.globalReferenceNameLowerInvariant));

            enumOrFlagsDefinitionMap.Add(enumOrFlagsDefinition.globalReferenceNameLowerInvariant, enumOrFlagsDefinition);
            if (enumOrFlagsDefinition.isFlagsDefinition)
            {
                flagsDefinitions.Add(enumOrFlagsDefinition);
            }
            else
            {
                enumDefinitions.Add(enumOrFlagsDefinition);
            }
        }

        public EnumOrFlagsDefinition TryGetEnumOrFlagsDefinition(NamedObjectDefinition currentObject, String typeName)
        {
            EnumOrFlagsDefinition definition;
            String nameFromCurrentObject = currentObject.globalReferenceNameLowerInvariant + "." + typeName;

            // Try to get the defintion from any enum defined in the current object
            if (enumOrFlagsDefinitionMap.TryGetValue(nameFromCurrentObject, out definition))
            {
                return definition;
            }
            // Try to get the definition from a global enum definition
            if (enumOrFlagsDefinitionMap.TryGetValue(typeName, out definition))
            {
                return definition;
            }

            return null;
        }

        // returns definition key
        public void AddObjectDefinition(NamedObjectDefinition objectDefinition)
        {
            if (objectDefinitionMap.ContainsKey(objectDefinition.globalReferenceNameLowerInvariant))
            {
                throw new InvalidOperationException(String.Format("Object '{0}' already exists (globalkey='{1}')",
                    objectDefinition.name, objectDefinition.globalReferenceNameLowerInvariant));
            }

            objectDefinitionMap.Add(objectDefinition.globalReferenceNameLowerInvariant, objectDefinition);
            objectDefinitions.Add(objectDefinition);
        }
        public NamedObjectDefinition TryGetObjectDefinition(NamedObjectDefinition currentObject, String typeName)
        {
            NamedObjectDefinition definition;
            String nameFromCurrentObject = currentObject.globalReferenceNameLowerInvariant + "." + typeName;

            // Try to get the defintion from any object defined in the current object
            if (objectDefinitionMap.TryGetValue(nameFromCurrentObject, out definition))
            {
                return definition;
            }
            // Try to get the definition from a global object definition
            if (objectDefinitionMap.TryGetValue(typeName, out definition))
            {
                return definition;
            }
            return null;
        }

        //public readonly IDataStructureFactory dataStructureFactory;
        public BinaryFormatFile(/*IDataStructureFactory dataStructureFactory*/)
        {
            //this.dataStructureFactory = dataStructureFactory;
        }
    }

    //
    // Definition Classes
    //
    public class EnumValueDefinition
    {
        public readonly String name;
        public readonly String value;
        public EnumValueDefinition(String name, String value)
        {
            this.name = name;
            this.value = value;
        }
    }
    public class FlagsValueDefinition
    {
        public readonly String name;
        public readonly Byte bit;
        public FlagsValueDefinition(String name, Byte bit)
        {
            this.name = name;
            this.bit = bit;
        }
    }
    public class EnumOrFlagsDefinition
    {
        public readonly Boolean isFlagsDefinition;
        public readonly Boolean isGlobalType;

        public readonly BinaryFormatType underlyingIntegerType;
        public readonly Byte byteCount;

        public readonly String typeName;
        public readonly String typeNameLowerInvariantCase;
        public readonly String globalReferenceNameLowerInvariant;

        public readonly List<EnumValueDefinition> enumValues;
        public readonly List<FlagsValueDefinition> flagValues;

        public EnumOrFlagsDefinition(BinaryFormatFile file, Boolean isFlagsDefinition, NamedObjectDefinition objectDefinedIn,
            BinaryFormatType underlyingIntegerType, String typeName)
        {
            this.isFlagsDefinition = isFlagsDefinition;
            this.isGlobalType = (objectDefinedIn == null);

            this.underlyingIntegerType = underlyingIntegerType;
            this.byteCount = underlyingIntegerType.IntegerTypeByteCount();

            this.typeName = typeName;
            this.typeNameLowerInvariantCase = typeName.ToLowerInvariant();
            this.globalReferenceNameLowerInvariant = (objectDefinedIn == null) ? typeNameLowerInvariantCase :
                (objectDefinedIn.globalReferenceNameLowerInvariant + "." + typeNameLowerInvariantCase);

            //
            // Add the definition to the static flags or enum definitions list
            //
            if (isFlagsDefinition)
            {
                enumValues = null;
                flagValues = new List<FlagsValueDefinition>();
            }
            else
            {
                enumValues = new List<EnumValueDefinition>();
                flagValues = null;
            }

            //
            // Add the defintion to the object and the file
            //
            if (objectDefinedIn != null) objectDefinedIn.AddEnumOrFlagDefinition(this);
            file.AddEnumOrFlagsDefinition(this);
        }
        public void Add(EnumValueDefinition definition)
        {
            enumValues.Add(definition);
        }
        public void Add(FlagsValueDefinition definition)
        {
            flagValues.Add(definition);
        }
    }
}
