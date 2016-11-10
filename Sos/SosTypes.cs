// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

#if WindowsCE
using EnumReflection = System.MissingInCEEnumReflection;
#else
using EnumReflection = System.Enum;
#endif

namespace More
{
    /*
    public enum SosPrimitiveType
    {
        Boolean = 0,
        SByte = 1,
        Byte = 2,
        Int16 = 3,
        UInt16 = 4,
        Int32 = 5,
        UInt32 = 6,
        Int64 = 7,
        UInt64 = 8,
        Pointer = 9,
        Single = 10,
        Double = 11,
        Char = 12,
        String = 13,
    }
    */

    public class SosEnumDefinition
    {
        readonly Dictionary<String, Int32> values;
        public SosEnumDefinition()
        {
            this.values = new Dictionary<String, Int32>();
        }
        public Dictionary<String, Int32> Values
        {
            get { return values; }
        }
        public void Add(String valueName, Int32 value)
        {
            values.Add(valueName, value);
        }
        public Int32 ValueOf(String valueName)
        {
            Int32 value;
            if (values.TryGetValue(valueName, out value))
            {
                return value;
            }
            throw new KeyNotFoundException(String.Format("Could not find enum value of '{0}'", valueName));
        }
        public override Boolean Equals(object other)
        {
            SosEnumDefinition otherDefinition = other as SosEnumDefinition;
            if (otherDefinition == null) return false;
            return Equals(otherDefinition);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public Boolean Equals(SosEnumDefinition other)
        {
            Int32 count = values.Count;
            if (other.values.Count != count) return false;
            foreach(KeyValuePair<String,Int32> thisPair in values)
            {
                if (other.values[thisPair.Key] != thisPair.Value) return false;
            }
            return true;
        }
        public String TypeDefinition()
        {
            StringBuilder enumBuilder = new StringBuilder();
            enumBuilder.Append("Enum{");

            Boolean atFirst = true;

            foreach (KeyValuePair<String, Int32> thisPair in values)
            {
                if (atFirst) atFirst = false; else enumBuilder.Append(',');

                enumBuilder.Append(thisPair.Key);
                enumBuilder.Append('=');
                enumBuilder.Append(thisPair.Value);
            }
            enumBuilder.Append("}");

            return enumBuilder.ToString();
        }
        public Type GetAndVerifyType(String enumTypeName, SosVerifyCriteria criteria)
        {
            Type enumType = Type.GetType(enumTypeName);
            if (enumType == null)
                throw new InvalidOperationException(String.Format("Enum type '{0}' does not exist", enumTypeName));
            VerifyType(enumType, criteria);
            return enumType;
        }
        // allowNewEnumValues
        public void VerifyType(Type enumType, SosVerifyCriteria criteria)
        {
            if (!enumType.IsEnum)
            {
                throw new InvalidOperationException(String.Format("Type '{0}' is not an enum type", enumType.FullName));
            }

            Array localEnumValues = EnumReflection.GetValues(enumType);

            if ((criteria & SosVerifyCriteria.AllowExtraEnumValuesInDefinition) == SosVerifyCriteria.AllowExtraEnumValuesInDefinition)
            {
                if ((criteria & SosVerifyCriteria.AllowExtraEnumValuesInType) == SosVerifyCriteria.AllowExtraEnumValuesInType)
                {
                    // do not verify counts
                }
                else
                {
                    if (localEnumValues.Length > values.Count)
                    {
                        throw new InvalidOperationException(String.Format(
                            "Enum Type {0} has more values ({1} values) then the definition ({2} values)", enumType.FullName, localEnumValues.Length, values.Count));
                    }
                }
            }
            else
            {
                if ((criteria & SosVerifyCriteria.AllowExtraEnumValuesInType) == SosVerifyCriteria.AllowExtraEnumValuesInType)
                {
                    if (values.Count > localEnumValues.Length)
                    {
                        throw new InvalidOperationException(String.Format(
                            "Enum Type {0} has less values ({1} values) then the definition ({2} values)", enumType.FullName, localEnumValues.Length, values.Count));
                    }
                    foreach (var valueFromDefinition in values)
                    {
                        int enumValueAsInt;
                        try
                        {
                            enumValueAsInt = Convert.ToInt32(Enum.Parse(enumType, valueFromDefinition.Key, true));
                        }
                        catch (ArgumentException)
                        {
                            throw new InvalidOperationException(String.Format(
                                "Enum Type '{0}' is missing value {1}",
                                enumType.FullName, valueFromDefinition.Key));
                        }
                        if (enumValueAsInt != valueFromDefinition.Value)
                        {
                            throw new InvalidOperationException(String.Format("Enum Type '{0}' value '{1}' is '{2}' but the value from the definition is '{3}'",
                                enumType.FullName, valueFromDefinition.Key, enumValueAsInt, valueFromDefinition.Value));
                        }
                    }
                }
                else
                {
                    if (localEnumValues.Length != values.Count)
                    {
                        throw new InvalidOperationException(String.Format(
                            "Enum Type {0} has {1} value(s) but definition has {2}", enumType.FullName, localEnumValues.Length, values.Count));
                    }
                }
            }


            for (int i = 0; i < localEnumValues.Length; i++)
            {
                Enum localEnumValue = (Enum)localEnumValues.GetValue(i);

                Int32 valueFromEnumDefinition;
                if (values.TryGetValue(localEnumValue.ToString(), out valueFromEnumDefinition))
                {
                    Int32 enumValueAsInt32 = Convert.ToInt32(localEnumValue);
                    if (enumValueAsInt32 != valueFromEnumDefinition)
                    {
                        throw new InvalidOperationException(String.Format("Enum Type '{0}' value '{1}' is '{2}' but the value from the definition is '{3}'",
                            enumType.FullName, localEnumValue.ToString(), enumValueAsInt32, valueFromEnumDefinition));
                    }
                }
                else
                {
                    if ((criteria & SosVerifyCriteria.AllowExtraEnumValuesInType) == 0)
                    {
                        throw new InvalidOperationException(String.Format(
                            "Enum Type '{0}' has value '{1}' but the definition does not have that value",
                            enumType.FullName, localEnumValue.ToString()));
                    }
                }
            }
        }
    }
    [Flags]
    public enum SosVerifyCriteria
    {
        AllowExtraEnumValuesInDefinition = 0x01,
        AllowExtraEnumValuesInType       = 0x02,
    }

    public class SosObjectDefinition
    {
        public static readonly SosObjectDefinition Empty = new SosObjectDefinition();

        public readonly Dictionary<String, String> fieldNamesToFieldTypes;
        public SosObjectDefinition(params String[] fieldTypesAndNames)
        {
            this.fieldNamesToFieldTypes = new Dictionary<String, String>();
            if (fieldTypesAndNames == null || fieldTypesAndNames.Length <= 0) return;

            if (fieldTypesAndNames.Length % 2 != 0) throw new InvalidOperationException("The constructor for SosObjectDefinition must have an even number of strings");

            for (int i = 0; i < fieldTypesAndNames.Length; i += 2)
            {
                fieldNamesToFieldTypes.Add(fieldTypesAndNames[i + 1], fieldTypesAndNames[i]);
            }
        }
        public SosObjectDefinition(Dictionary<String, String> fieldNamesToFieldTypes)
        {
            if (fieldNamesToFieldTypes == null) throw new ArgumentNullException("fieldNamesToFieldTypes");
            this.fieldNamesToFieldTypes = fieldNamesToFieldTypes;
        }
        public String TypeDefinition()
        {
            StringBuilder enumBuilder = new StringBuilder();
            enumBuilder.Append("{");

            Boolean atFirst = true;

            foreach (KeyValuePair<String, String> thisPair in fieldNamesToFieldTypes)
            {
                if (atFirst) atFirst = false; else enumBuilder.Append(',');

                enumBuilder.Append(thisPair.Value);
                enumBuilder.Append(':');
                enumBuilder.Append(thisPair.Key);
            }
            enumBuilder.Append("}");

            return enumBuilder.ToString();
        }
        public override Boolean Equals(object other)
        {
            SosObjectDefinition otherDefinition = other as SosObjectDefinition;
            if (otherDefinition == null) return false;
            return Equals(otherDefinition);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public Boolean Equals(SosObjectDefinition other)
        {
            Int32 count = fieldNamesToFieldTypes.Count;
            if (other.fieldNamesToFieldTypes.Count != count) return false;
            foreach (KeyValuePair<String, String> thisPair in fieldNamesToFieldTypes)
            {
                if (!other.fieldNamesToFieldTypes[thisPair.Key].Equals(thisPair.Value)) return false;
            }
            return true;
        }
        public Type GetAndVerifyType(String objectTypeName)
        {
            Type objectType = Type.GetType(objectTypeName);
            if (objectType == null)
                throw new InvalidOperationException(String.Format("Type '{0}' does not exist", objectTypeName));
            VerifyType(objectType);
            return objectType;
        }
        
        public void VerifyType(Type objectType)
        {
            FieldInfo[] fieldInfos = objectType.GetSerializedFields();
            if (fieldInfos == null || fieldInfos.Length <= 0)
            {
                if (fieldNamesToFieldTypes.Count > 0)
                    throw new InvalidOperationException(String.Format("Type '{0}' has no fields", objectType.FullName));
                return;
            }

            if (fieldInfos.Length != fieldNamesToFieldTypes.Count)
                throw new InvalidOperationException(String.Format(
                    "Object Definition has {0} fields but actual object has {1} fields", fieldNamesToFieldTypes.Count, fieldInfos.Length));

            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i];

                String fieldTypeNameFromDefinition;
                if (!fieldNamesToFieldTypes.TryGetValue(fieldInfo.Name, out fieldTypeNameFromDefinition))
                {
                    throw new InvalidOperationException(String.Format(
                        "Object Type '{0}' has field '{1}' but the remote object definition does not have that field",
                        objectType.FullName, fieldInfo.Name));
                }

                String fieldSosTypeName = fieldInfo.FieldType.SosTypeName();
                if (!fieldSosTypeName.Equals(fieldTypeNameFromDefinition))
                {
                    throw new InvalidOperationException(String.Format("Object Type '{0}' has field '{1}' of type '{2}', but the remote object type of that field is '{3}'",
                        objectType.FullName, fieldInfo.Name, fieldSosTypeName, fieldTypeNameFromDefinition));
                }
            }
        }
    }
    public class SosMethodDefinition
    {
        public class Parameter
        {
            public readonly String sosTypeName;
            public readonly String name;
            public Parameter(String sosTypeName, String name)
            {
                this.sosTypeName = sosTypeName;
                this.name = name;
            }
        }
        public readonly String methodName;
        public readonly String returnSosTypeName;
        public readonly Parameter[] parameters;
        public SosMethodDefinition(String methodName, String returnSosTypeName, Parameter[] parameters)
        {
            this.methodName = methodName;
            /*
            Int32 lastPeriodIndex = fullMethodName.LastIndexOf('.');
            if (lastPeriodIndex > 0)
            {
                this.methodPrefix = fullMethodName.Remove(lastPeriodIndex);
            }
            */
            this.returnSosTypeName = returnSosTypeName;
            this.parameters = parameters;
        }
        public SosMethodDefinition(String methodName, String returnSosTypeName, params String[] parameterTypesAndNames)
        {
            this.methodName = methodName;
            /*
            Int32 lastPeriodIndex = fullMethodName.LastIndexOf('.');
            if (lastPeriodIndex > 0)
            {
                this.methodPrefix = fullMethodName.Remove(lastPeriodIndex);
            }
            */
            this.returnSosTypeName = returnSosTypeName;

            if (parameterTypesAndNames == null)
            {
                this.parameters = null;
            }
            else
            {
                if (parameterTypesAndNames.Length % 2 != 0) throw new InvalidOperationException(String.Format(
                    "There must be an even number of strings for the parameterTypesAndNames but there are '{0}'", parameterTypesAndNames.Length));

                this.parameters = new Parameter[parameterTypesAndNames.Length / 2];
                Int32 parameterIndex = 0;
                for(int i = 0; i + 1 < parameterTypesAndNames.Length; i += 2)
                {
                    this.parameters[parameterIndex++] = new Parameter(parameterTypesAndNames[i], parameterTypesAndNames[i + 1]);
                }
            }
        }
        public String Definition()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(returnSosTypeName);
            builder.Append(' ');

            builder.Append(methodName);
            builder.Append("(");

            if (parameters != null)
            {
                Boolean atFirst = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (atFirst) atFirst = false; else builder.Append(',');

                    Parameter parameter = parameters[i];
                    builder.Append(parameter.sosTypeName);
                    builder.Append(' ');
                    builder.Append(parameter.name);
                }
            }
            builder.Append(")");

            return builder.ToString();
        }
        public override Boolean Equals(object other)
        {
            SosObjectDefinition otherDefinition = other as SosObjectDefinition;
            if (otherDefinition == null) return false;
            return Equals(otherDefinition);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public Boolean Equals(SosMethodDefinition other)
        {
            if (!methodName.Equals(other.methodName)) return false;
            if (!returnSosTypeName.Equals(other.returnSosTypeName)) return false;

            if (parameters == null) return other.parameters == null || other.parameters.Length <= 0;
            if (other.parameters == null) return parameters.Length <= 0;

            if (parameters.Length != other.parameters.Length) return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                Parameter parameter = parameters[i];
                Parameter otherParameter = other.parameters[i];
                if (!parameter.sosTypeName.Equals(otherParameter.sosTypeName)) return false;
                if (!parameter.name.Equals(otherParameter.name)) return false;
            }
            return true;
        }
        public String Diff(SosMethodDefinition other)
        {
            if (!methodName.Equals(other.methodName))
                return String.Format("MethodName '{0}' differs from '{1}", methodName, other.methodName);
            if (!returnSosTypeName.Equals(other.returnSosTypeName))
                return String.Format("ReturnType '{0}' differs from '{1}", returnSosTypeName, other.returnSosTypeName);

            if (parameters == null)
            {
                if (other.parameters == null || other.parameters.Length <= 0) return null;
                return String.Format("Method has no parameters but other method has '{0}' parameters", other.parameters.Length);
            }
            if (other.parameters == null)
            {
                if (parameters.Length <= 0) return null;
                return String.Format("Method has '{0}' parameters but other method has no parameters", other.parameters.Length);
            }

            if (parameters.Length != other.parameters.Length)
            {
                return String.Format("Method has '{0}' parameters but other method has '{1}' parameters",
                    parameters.Length, other.parameters.Length);
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                Parameter parameter = parameters[i];
                Parameter otherParameter = other.parameters[i];
                if (!parameter.sosTypeName.Equals(otherParameter.sosTypeName))
                    return String.Format("Parameter {0} type is '{1}' but other type is '{2}'",
                        i, parameter.sosTypeName, otherParameter.sosTypeName);
                if (!parameter.name.Equals(otherParameter.name))
                    return String.Format("Parameter {0} name is '{1}' but other name is '{2}'",
                        i, parameter.name, otherParameter.name);
            }
            return null;
        }
    }
    public interface ISosTypeDefinitionCallback
    {
        void PrimitiveType(Type type);
        void Enum(SosEnumDefinition enumType);
        void ArrayType(String elementTypeDefinition);
        void ObjectType(SosObjectDefinition type);
    }
    public static class SosTypes
    {
        /*
        static readonly String[] SosPrimitives = new String[] {
            "Boolean",
            
            "SByte",
            "Byte",
            "Int16",
            "UInt16",
            "Int32",
            "UInt32",
            "Int64",
            "UInt64",

            "Pointer",

            "Single",
            "Double",

            "Char",
            "String",
        };
        */
        public static Boolean IsSosPrimitive(this Type type)
        {
            return type.IsPrimitive || type == typeof(String);
        }
        static readonly Dictionary<String, Type> SosPrimitiveDictionary;
        static SosTypes()
        {
            SosPrimitiveDictionary = new Dictionary<String, Type>();
            SosPrimitiveDictionary.Add("Boolean", typeof(Boolean));
            
            SosPrimitiveDictionary.Add("SByte", typeof(SByte));
            SosPrimitiveDictionary.Add("Byte", typeof(Byte));
            SosPrimitiveDictionary.Add("Int16", typeof(Int16));
            SosPrimitiveDictionary.Add("UInt16", typeof(UInt16));
            SosPrimitiveDictionary.Add("Int32", typeof(Int32));
            SosPrimitiveDictionary.Add("UInt32", typeof(UInt32));
            SosPrimitiveDictionary.Add("Int64", typeof(Int64));
            SosPrimitiveDictionary.Add("UInt64", typeof(UInt64));

            SosPrimitiveDictionary.Add("Pointer", typeof(UIntPtr));

            SosPrimitiveDictionary.Add("Single", typeof(Single));
            SosPrimitiveDictionary.Add("Double", typeof(Double));

            SosPrimitiveDictionary.Add("Char", typeof(Char));
            SosPrimitiveDictionary.Add("String", typeof(String));
        }
        public static Boolean IsSosPrimitive(this String typeName)
        {
            return SosPrimitiveDictionary.ContainsKey(typeName);
        }
        public static Type TryGetSosPrimitive(this String typeName)
        {
            Type type;
            if (SosPrimitiveDictionary.TryGetValue(typeName, out type)) return type;
            return null;
        }
        public static String SosTypeName(this Type type)
        {
            if (type == typeof(void)) return "Void";
            if (type == typeof(IntPtr) || type == typeof(UIntPtr)) return "Pointer";

            // These types are primitve to Sos so their type names are used instead of their FullName
            if (type.IsPrimitive || type == typeof(String)) return type.Name;

            if (type == typeof(Enum)) return type.FullName;

            if (type.IsArray) return type.GetElementType().SosTypeName() + "[]";

            return type.FullName.Replace('+', '.');
        }
        public static String SosShortTypeName(this Type type)
        {
            if (type == typeof(void)) return "Void";
            if (type == typeof(IntPtr) || type == typeof(UIntPtr)) return "Pointer";

            // These types are primitve to Sos so their type names are used instead of their FullName
            if (type.IsPrimitive || type == typeof(String) || type == typeof(Enum)) return type.Name;
            if (type.IsArray) return type.GetElementType().SosShortTypeName() + "[]";

            return type.Name.Replace('+', '.');
        }
        public static FieldInfo[] GetSerializedFields(this Type type)
        {
            return type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }
        public static String SosTypeDefinition(this Type type)
        {
            //
            // Handle primitive types
            //
            if (type == typeof(IntPtr) || type == typeof(UIntPtr))
            {
                return "Pointer";
            }
            if ((type.IsPrimitive || type == typeof(String)))
            {
                return type.Name;
            }
            if (type.IsArray) return type.GetElementType().SosTypeName() + "[]";

            //
            // Handle enums
            //
            if (type.IsEnum)
            {
                StringBuilder enumBuilder = new StringBuilder();
                enumBuilder.Append("Enum{");

                Array enumValues = EnumReflection.GetValues(type);
                for (int i = 0; i < enumValues.Length; i++)
                {
                    if (i > 0) enumBuilder.Append(',');

                    Enum enumValue = (Enum)enumValues.GetValue(i);
                    enumBuilder.Append(enumValue.ToString());
                    enumBuilder.Append('=');
                    enumBuilder.Append(enumValue.ToString("D"));
                }
                enumBuilder.Append("}");

                return enumBuilder.ToString();
            }

            //
            // Serialize it as a an object of primitive types
            //
            FieldInfo[] fieldInfos = type.GetSerializedFields();

            if (fieldInfos == null || fieldInfos.Length <= 0)
            {
                return "{}";
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("{");

            FieldInfo fieldInfo = fieldInfos[0];

            builder.Append(fieldInfo.FieldType.SosTypeName());
            builder.Append(":");
            builder.Append(fieldInfo.Name);

            for (int i = 1; i < fieldInfos.Length; i++)
            {
                fieldInfo = fieldInfos[i];
                builder.Append(',');
                builder.Append(fieldInfo.FieldType.SosTypeName());
                builder.Append(":");
                builder.Append(fieldInfo.Name);
            }
            builder.Append('}');
            return builder.ToString();
        }

        // returns offset of ',' or '}'
        static Int32 ParseTypeName(String str, Int32 offset, Char delimiter, out String typeName)
        {
            if (offset >= str.Length) throw new FormatException("expected type name but reached end of string");

            Char c = str[offset];

            while (true)
            {
                if (!Char.IsWhiteSpace(c)) break;
                offset++;
                if (offset >= str.Length) throw new FormatException("reached end of string before start of type name");
                c = str[offset];
            }

            Int32 start = offset;
            offset++;
            if (offset >= str.Length) throw new FormatException("reached end of string before end of type name");

            c = str[offset];

            while (true)
            {
                if (c == delimiter)
                {
                    typeName = str.Substring(start, offset - start);
                    return offset;
                }

                if (
                    (c < 'a' || c > 'z') &&
                    (c < 'A' || c > 'Z') &&
                    (c < '0' || c > '9') &&
                    (c != '.') &&
                    (c != '_')) break;
                offset++;
                if (offset >= str.Length) throw new FormatException("reached end of string at a type name");
                c = str[offset];
            }

            //
            // handle arrays
            //
            while(true)
            {
                if (c != '[') break;

                offset++;
                if (offset >= str.Length) throw new FormatException("Found '[' but reached end of input before ']'");
                c = str[offset];

                if (c != ']') throw new FormatException(String.Format("Found '[' but next character was '{0}' (charcode={1}) instead of ']'", c, (UInt32)c));

                offset++;
                if(offset >= str.Length) throw new FormatException("reached end of string at a type name");
                c = str[offset];

                if (c == delimiter)
                {
                    typeName = str.Substring(start, offset - start);
                    return offset;
                }
            }

            typeName = str.Substring(start, offset - start);

            while (true)
            {
                if (!Char.IsWhiteSpace(c)) throw new FormatException(String.Format("Expected '{0}' to delimit end of type name but got '{1}' (charcode={2})", delimiter, c, (UInt32)c));
                offset++;
                if (offset >= str.Length) throw new FormatException("expected ',' or '}' to end type name but reached end of string");
                c = str[offset];
                if (c == delimiter) return offset;
            }
        }

        //
        // Returns offset of colon and field name string
        //
        static Int32 ParseName(String str, Int32 offset, Char endCharacter, out String fieldName)
        {
            if (offset >= str.Length) throw new FormatException("expected name but reached end of string");

            Char c = str[offset];

            while (true)
            {
                if (!Char.IsWhiteSpace(c)) break;
                offset++;
                if (offset >= str.Length) throw new FormatException("reached end of string before start of name");
                c = str[offset];
            }

            Int32 start = offset;
            offset++;
            if (offset >= str.Length) throw new FormatException("reached end of string before end of name");

            c = str[offset];

            while (true)
            {
                if (c == ',' || c == endCharacter)
                {
                    fieldName = str.Substring(start, offset - start);
                    return offset;
                }
                if (
                    (c < 'a' || c > 'z') &&
                    (c < 'A' || c > 'Z') &&
                    (c < '0' || c > '9') &&
                    (c != '_')) break;
                offset++;
                if (offset >= str.Length) throw new FormatException(String.Format("Expected ',' or '{0}' to delimit end of name but reached end of string", endCharacter));
                c = str[offset];
            }

            fieldName = str.Substring(start, offset - start);

            while (true)
            {
                if (!Char.IsWhiteSpace(c)) throw new FormatException(String.Format("Expected ',' or '{0}' to delimit end of name but got '{1}' (charcode={2})", endCharacter, c, (UInt32)c));
                offset++;
                if (offset >= str.Length) throw new FormatException("expected ':' to end field name but reached end of string");
                c = str[offset];
                if (c == ',' || c == endCharacter) return offset;
            }
        }
        public static void ParseSosTypeDefinition(ISosTypeDefinitionCallback callback, String sosTypeDefinition)
        {
            if (
                sosTypeDefinition[sosTypeDefinition.Length - 1] == ']' &&
                sosTypeDefinition[sosTypeDefinition.Length - 2] == '[')
            {
                callback.ArrayType(sosTypeDefinition.Remove(sosTypeDefinition.Length - 2));
                return;
            }

            //
            // Parse Object Definition
            //
            if(sosTypeDefinition[0] == '{')
            {
                callback.ObjectType(ParseSosObjectTypeDefinition(sosTypeDefinition, 0));
                return;
            }

            //
            // Parse Enum Definition
            //
            if (sosTypeDefinition.StartsWith("Enum"))
            {
                SosEnumDefinition enumDefinition = ParseSosEnumTypeDefinition(sosTypeDefinition, 4);
                callback.Enum(enumDefinition);
                return;
            }

            Type primitiveType = TryGetSosPrimitive(sosTypeDefinition);
            if (primitiveType != null)
            {
                callback.PrimitiveType(primitiveType);
                return;
            }

            throw new InvalidOperationException(String.Format("Invalid type definition '{0}'", sosTypeDefinition));
        }
        static void UnexpectedEndOfMethodDefinition(String methodDefinition, Int32 startOffset)
        {
            throw new FormatException(String.Format(
                    "Method definition '{0}' ended unexpectedly", methodDefinition.Substring(startOffset)));
        }
        public static SosMethodDefinition ParseMethodDefinition(String methodDefinition, Int32 offset)
        {
            String originalMethodDefinition = methodDefinition;
            Int32 originalOffset = offset;

            String returnType = methodDefinition.Peel(offset, out methodDefinition);            
            if(String.IsNullOrEmpty(returnType)) UnexpectedEndOfMethodDefinition(originalMethodDefinition, originalOffset);
            if(String.IsNullOrEmpty(methodDefinition)) UnexpectedEndOfMethodDefinition(originalMethodDefinition, originalOffset);

            Char c;
            offset = 0;
            String methodName;
            String methodParametersDefinition;
            while (true)
            {
                offset++;
                if (offset >= methodDefinition.Length) UnexpectedEndOfMethodDefinition(originalMethodDefinition, originalOffset);
                c = methodDefinition[offset];
                if (c == '(')
                {
                    methodName = methodDefinition.Remove(offset);
                    methodParametersDefinition = methodDefinition.Substring(offset);
                    break;
                }
                if (Char.IsWhiteSpace(c))
                {
                    methodName = methodDefinition.Remove(offset);
                    while (true)
                    {
                        offset++;
                        if (offset >= methodDefinition.Length) UnexpectedEndOfMethodDefinition(originalMethodDefinition, originalOffset);
                        c = methodDefinition[offset];
                        if (c == '(') break;
                        if (!Char.IsWhiteSpace(c)) throw new FormatException(String.Format(
                             "Expected '(' to follow method name but got '{0}' (charcode={1})", c, (UInt32)c));
                    }
                    methodParametersDefinition = methodDefinition.Substring(offset);
                    break;
                }
            }
            List<String> parameters = ParseMethodDefinitionParameters(methodParametersDefinition, 0);

            return new SosMethodDefinition(methodName, returnType, (parameters == null) ? null : parameters.ToArray());
        }
        public static List<String> ParseMethodDefinitionParameters(String typeAndNames, Int32 offset)
        {
            Char c = typeAndNames[offset];

            if (c != '(') throw new FormatException(String.Format("Expected method definition to start with '(' but started with '{0}' (charcode={1})",
                 c, (UInt32)c));

            // skip whitespace
            while (true)
            {
                offset++;
                if (offset >= typeAndNames.Length) throw new FormatException("Object type definition missing ending '}'");
                c = typeAndNames[offset];
                if (!Char.IsWhiteSpace(c)) break;
            }

            if (c == ')') return null;

            List<String> parameters = new List<String>();

            while (true)
            {
                //
                // State: c contains first character of field name
                //
                String parameterType;
                offset = ParseTypeName(typeAndNames, offset, ' ', out parameterType);

                //
                // Skip whitespace
                //
                while (true)
                {
                    offset++;
                    if (offset >= typeAndNames.Length) throw new FormatException("Expected field value but reached end of type definition");
                    c = typeAndNames[offset];
                    if (!Char.IsWhiteSpace(c)) break;
                }

                //
                // Parse Field Name
                //
                String parameterName;
                offset = ParseName(typeAndNames, offset, ')', out parameterName);

                //
                // Add parameter type
                //
                parameters.Add(parameterType);
                parameters.Add(parameterName);

                //
                // Get to next field name or end of type definition
                //
                c = typeAndNames[offset];
                if (c == ',')
                {
                    // Skip whitespace till field name
                    while (true)
                    {
                        offset++;
                        if (offset >= typeAndNames.Length) throw new FormatException("Expected field type after ',' but reached end of type definition");
                        c = typeAndNames[offset];
                        if (!Char.IsWhiteSpace(c)) break;
                    }
                    continue;
                }
                if (c == ')')
                {
                    // skip ending whitespace
                    while (true)
                    {
                        offset++;
                        if (offset >= typeAndNames.Length)
                        {
                            return parameters;
                        }
                        c = typeAndNames[offset];
                        if (!Char.IsWhiteSpace(c))
                            throw new FormatException(String.Format("Reached end of object type definition and then got more character(s) \"{0}\"", typeAndNames.Substring(offset)));
                    }
                }

                throw new InvalidOperationException(String.Format(
                    "CodeBug: SosTypes.ParseName function should only return the offset of ',' or ')' but it returned the offset of '{0}' (charcode={1})",
                    c, (UInt32)c));
            }
        }
        //
        // Offset is the offset of '{'
        //
        public static SosObjectDefinition ParseSosObjectTypeDefinition(String sosTypeDefinition, Int32 offset)
        {
            Dictionary<String, String> fields = new Dictionary<String, String>();

            Char c = sosTypeDefinition[offset];

            if (c != '{') throw new FormatException(String.Format("Expected object type definition to start with '{{' but started with '{0}' (charcode={1})",
                 c, (UInt32)c));

            // skip whitespace
            while (true)
            {
                offset++;
                if (offset >= sosTypeDefinition.Length) throw new FormatException("Object type definition missing ending '}'");
                c = sosTypeDefinition[offset];
                if (!Char.IsWhiteSpace(c)) break;
            }

            if (c == '}') return SosObjectDefinition.Empty;

            while (true)
            {
                //
                // State: c contains first character of field name
                //
                String fieldType;
                offset = ParseTypeName(sosTypeDefinition, offset, ':', out fieldType);

                //
                // Skip whitespace
                //
                while (true)
                {
                    offset++;
                    if (offset >= sosTypeDefinition.Length) throw new FormatException("Expected field value but reached end of type definition");
                    c = sosTypeDefinition[offset];
                    if (!Char.IsWhiteSpace(c)) break;
                }

                //
                // Parse Field Name
                //
                String fieldName;
                offset = ParseName(sosTypeDefinition, offset, '}', out fieldName);

                //
                // Add field type
                //
                fields.Add(fieldName, fieldType);

                //
                // Get to next field name or end of type definition
                //
                c = sosTypeDefinition[offset];
                if (c == ',')
                {
                    // Skip whitespace till field name
                    while (true)
                    {
                        offset++;
                        if (offset >= sosTypeDefinition.Length) throw new FormatException("Expected field type after ',' but reached end of type definition");
                        c = sosTypeDefinition[offset];
                        if (!Char.IsWhiteSpace(c)) break;
                    }
                    continue;
                }
                if (c == '}')
                {
                    // skip ending whitespace
                    while (true)
                    {
                        offset++;
                        if (offset >= sosTypeDefinition.Length)
                        {
                            return new SosObjectDefinition(fields);
                        }
                        c = sosTypeDefinition[offset];
                        if (!Char.IsWhiteSpace(c))
                            throw new FormatException(String.Format("Reached end of object type definition and then got more character(s) \"{0}\"", sosTypeDefinition.Substring(offset)));
                    }
                }

                throw new InvalidOperationException(String.Format(
                    "CodeBug: SosTypes.ParseName function should only return the offset of ',' or '}}' but it returned the offset of '{0}' (charcode={1})",
                    c, (UInt32)c));
            }
        }
        //
        // offset is is first character after 'Enum'
        //
        public static SosEnumDefinition ParseSosEnumTypeDefinition(String sosTypeDefinition, Int32 offset)
        {
            Char c;

            //
            // Skip whitespace
            //
            while (true)
            {
                if (offset >= sosTypeDefinition.Length)
                    throw new FormatException("Expected '{' after 'Enum' but reached end of input");

                c = sosTypeDefinition[offset];

                if (!Char.IsWhiteSpace(c)) break;
                offset++;
            }

            if (c != '{') throw new FormatException(String.Format(
                 "Expected '{{' after 'Enum' but got '{0}' (charcode={1})", c, (UInt32)c));

            SosEnumDefinition enumDefinition = new SosEnumDefinition();

            //
            // Parse Enum Values
            //
            while (true)
            {
                //
                // Skip Whitespace
                //
                while (true)
                {
                    offset++;
                    if (offset >= sosTypeDefinition.Length)
                        throw new FormatException("Expected enum value name but reached end of input");
                    c = sosTypeDefinition[offset];
                    if (!Char.IsWhiteSpace(c)) break;
                }

                String enumValueName;
                Int32 start = offset;
                //
                // Get Enum Value Name
                //
                while (true)
                {
                    if (
                        (c < 'a' || c > 'z') &&
                        (c < 'A' || c > 'Z') &&
                        (c < '0' || c > '9') &&
                        (c != '_'))
                        throw new FormatException(String.Format("Enum value names can only contain A-Z,a-z or '_', but found '{0}' (charcode={1})", c, (UInt32)c));

                    offset++;
                    if (offset >= sosTypeDefinition.Length)
                        throw new FormatException("Expected '=' after enum value name but reached end of input");
                    c = sosTypeDefinition[offset];
                    if (c == '=')
                    {
                        enumValueName = sosTypeDefinition.Substring(start, offset - start);
                        break;
                    }
                    if (Char.IsWhiteSpace(c))
                    {
                        enumValueName = sosTypeDefinition.Substring(start, offset - start);
                        while (true)
                        {
                            offset++;
                            if (offset >= sosTypeDefinition.Length) throw new FormatException("Expected '=' but reached end of input");
                            c = sosTypeDefinition[offset];
                            if (c == '=') break;
                            if (!Char.IsWhiteSpace(c))
                                throw new FormatException(String.Format("Expected '=' but got '{0}' (charcode={1})", c, (UInt32)c));
                        }
                        break;
                    }
                }

                //
                // Skip Whitespace
                //
                while(true)
                {
                    offset++;
                    if (offset >= sosTypeDefinition.Length)
                        throw new FormatException("Expected value after '=' but reached end of input");
                    c = sosTypeDefinition[offset];
                    if (!Char.IsWhiteSpace(c)) break;
                }
                
                if ((c < '0' || c > '9') && (c != '-')) throw new FormatException(String.Format(
                    "Expected enum value to be a number but it started with '{0}' (charcode={1})", c, (UInt32)c));

                Int32 numberStart = offset;
                while (true)
                {
                    offset++;
                    if (offset >= sosTypeDefinition.Length) throw new FormatException("Expected whitespace, ',' or '}' after enum value but reached end of input");
                    c = sosTypeDefinition[offset];
                    if (c < '0' || c > '9') break;
                }

                String enumValueString = sosTypeDefinition.Substring(numberStart, offset - numberStart);
                Int32 enumValue = Int32.Parse(enumValueString);
                enumDefinition.Add(enumValueName, enumValue);

                // skip whitespace
                while (true)
                {
                    if (!Char.IsWhiteSpace(c)) break;
                    offset++;
                    if (offset >= sosTypeDefinition.Length) throw new FormatException("Expected ',' or '}' after enum value but reached end of input");
                    c = sosTypeDefinition[offset];
                }

                if (c == ',') continue;

                if (c != '}')
                    throw new FormatException(String.Format("Expected ',' or '}}' after enum value but got '{0}' (charcode={1})", c, (UInt32)c));

                // reached end of definition
                while (true)
                {
                    offset++;
                    if (offset >= sosTypeDefinition.Length)
                    {
                        return enumDefinition;
                    }
                    c = sosTypeDefinition[offset];
                    if (!Char.IsWhiteSpace(c)) throw new FormatException(String.Format(
                         "Reached end of enum definition but found non whitespace after the definition '{0}' (charcode={1})", c, (UInt32)c));
                }
            }
        }
    }
}
