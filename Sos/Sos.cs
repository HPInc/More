// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

//
// This protocol needs documentation.
// Also in the future it should implement syntax to represent tables
//
// Note: Sos does not have a primitive Char type, to serialize a char you must use a string
//
// Sos Primitives:
//
//     Boolean     true|false
//
//   NumberTypes:
//     SByte                        -128 through 127
//     Byte                            0 through 255
//
//     Int16                     -32,768 through 32,767
//     UInt16                          0 through 65,535
//
//     Int32               2,147,483,648 through 2,147,483,647
//     UInt32                          0 through 4,294,967,295
//
//     Int64  -9,223,372,036,854,775,808 through 9,223,372,036,854,775,807
//     UInt64                          0 through 18,446,744,073,709,551,615
//
//     Single              -3.402823E+38 through 3.402823E+38
//     Double  NotNullable   -1.79769313486232E+308 through 1.79769313486232E+308
//
//   The String Type:
//     String  "<some-string"
//
// Enum Types:
//     <EnumName> [a-zA-Z_]
//     An enum type has
//        1. An enum name
//        2. A set of enum values, each of which has a name and an explicit value
//        
//
// Sos Combination Types (used to combine other Sos types to make more complex types):
//     Sos Array  '[' (value) [ ',' (value) ] ']'
//         Example: [1,2,3,4]
//
//     Sos Object '{' (field-name) ':' (field-value) [ ',' (field-name) ':' (field-value) ]* '}'
//         Example: {MyBoolean:false,MyInteger:3,MyString:"hello",AnotherObject:{LittleInteger:-1},SomeNumbers:[1,2,3]}
//
//     Sos Table  '<' (header-name) [ ',' (header-name) ]* [ ':' (object-value) [ ',' (object-value) ] ] '>'
//         Example: <Name,Age,Gender:"Joe Smith",23,Male:"Amy White",42,Female>
//         (Note: This feature is not implemented yet but is a part of the specification)


namespace More
{
    public class SosTypeSerializationVerifier
    {
        readonly HashSet<Type> inRecursionTree = new HashSet<Type>();
        public SosTypeSerializationVerifier()
        {
        }
        public String CannotBeSerializedBecause(Type type)
        {
            //Console.Write("   Debug: ");
            //Console.Write(new String(' ', inRecursionTree.Count));
            //Console.WriteLine(type.FullName);

            //
            // IsAbstract must be checked before type == typeof(Enum) because enum types can be serialized but the
            // actual typeof(Enum) type cannot
            //
            if (type.IsAbstract) return "it is abstract";

            //
            // This must be checked before IsPrimitive because IntPtr and UIntPtr are primitive types
            //
            //if (type == typeof(IntPtr) || type == typeof(UIntPtr)) return "it is a pointer";

            if (type.IsPrimitive || type == typeof(String) || type == typeof(Enum)) return null;

            if (type.IsArray) return CannotBeSerializedBecause(type.GetElementType());

            //
            // Check other conditions
            //
            if (type.IsGenericTypeDefinition) return "it is a generic type definition";
            if (type.IsPointer) return "it is a pointer";

            //
            // Check the type name
            //
            String fullTypeName = type.SosTypeName();
            for (int i = 0; i < fullTypeName.Length; i++)
            {
                Char c = fullTypeName[i];
                if (
                    (c < 'a' || c > 'z') &&
                    (c < 'A' || c > 'Z') &&
                    (c != '.') &&
                    (c < '0' || c > '9') &&
                    (c != '_'))
                {
                    return "the type name contains invalid characters";
                }
            }

            //
            // Check that each field can be serialized
            //
            if (inRecursionTree.Contains(type)) return null;
            inRecursionTree.Add(type);

            String message = null;

            FieldInfo[] fieldInfos = type.GetSerializedFields();
            if (fieldInfos != null && fieldInfos.Length > 0)
            {
                for (int i = 0; i < fieldInfos.Length; i++)
                {
                    FieldInfo fieldInfo = fieldInfos[i];
                    message = CannotBeSerializedBecause(fieldInfo.FieldType);
                    if (message != null)
                    {
                        message = String.Format("the field '{0}' of type '{1}' cannot be serialized because {2}",
                            fieldInfo.Name, fieldInfo.FieldType.SosTypeName(), message);
                        break;
                    }
                }
            }
            inRecursionTree.Remove(type);

            return message;
        }
    }

    public static class Sos
    {
        //public static readonly Type[] StringParam = new Type[] { typeof(String) };

        //
        // According to the performance checks, this method is much faster when it is inlined
        //
        /*
        public static Boolean IsSosWhitespace(this Char c)
        {
            return (c == ' ' || c == '\t' || c == '\n' || c == '\r');
        }
        */


        public static Boolean IsSosEndOfValueDelimiter(this Char c)
        {
            return (c == ',' || c == ']' || c == '}' || c == ':' || c == '>');
        }
        /*
         * I commented this out because it wasn't being used
        static readonly Char[] EndOfValue = new Char[] {
            ',', // If the value is not the last item in an array/object/table
            ']', // If the value is the last item in an array
            '}', // If the value is the last item in an object
            ':', // If the value is the last item in a table row
            '>'  // If the value is the last item in a table
        };
        */
        // Used to create a user friendly string describing the difference
        // between the given expected object and the actual object
        public static String Diff(this Object expected, Object actual)
        {
            if (expected == null) return (actual == null) ? null : String.Format("Expected <null> but got '{0}'", actual);
            if (actual == null) return String.Format("Expected '{0}' but got <null>", expected);

            Type type = expected.GetType();
            Type actualType = actual.GetType();

            if (type != actualType) return String.Format("Expected object to be of type '{0}' but actual type is '{1}'", type.FullName, actualType.FullName);

            if (type.IsPrimitive || type == typeof(Enum)) return expected.Equals(actual) ? null :
                String.Format("Expected '{0}' but got '{1}'", expected, actual);

            if (type == typeof(String)) return expected.Equals(actual) ? null :
                String.Format("Expected \"{0}\" but got \"{1}\"", expected, actual);

            if (type.IsArray)
            {
                Array expectedArray = (Array)expected;
                Array actualArray = (Array)actual;

                if (expectedArray.Length != actualArray.Length)
                    return String.Format("Expected array of length {0} but actual length is {1}",
                        expectedArray.Length, actualArray.Length);

                for (int i = 0; i < expectedArray.Length; i++)
                {
                    Object expectedElement = expectedArray.GetValue(i);
                    Object actualElement = actualArray.GetValue(i);

                    String diffMessage = Diff(expectedElement, actualElement);
                    if (diffMessage != null) return String.Format("At index {0}: {1}", i, diffMessage);
                }

                return null;
            }

            //
            // Check that each field can be serialized
            //
            FieldInfo[] expectedFieldInfos = type.GetSerializedFields();
            if (expectedFieldInfos == null || expectedFieldInfos.Length <= 0)
            {
                FieldInfo[] actualFieldInfos = actualType.GetSerializedFields();
                return (actualFieldInfos == null || actualFieldInfos.Length <= 0) ? null :
                    String.Format("Expected object to have no fields but actual object has {0} fields", actualFieldInfos.Length);
            }
            for (int i = 0; i < expectedFieldInfos.Length; i++)
            {
                FieldInfo expectedFieldInfo = expectedFieldInfos[i];

                Object expectedField = expectedFieldInfo.GetValue(expected);
                Object actualField = expectedFieldInfo.GetValue(actual);

                String diffMessage = Diff(expectedField, actualField);
                if (diffMessage != null) return String.Format("Field '{0}': {1}", expectedFieldInfo.Name, diffMessage);
            }

            return null;
        }

        // An Sos Object always starts with 0-9, a-z, A-Z, -, ", [, {, or <
        public static Boolean IsValidStartOfSosString(Char c)
        {
            return
                (c >= '0' && c <= '9') || // Decimal digit
                (c >= 'a' && c <= 'z') || // Lower case letter
                (c >= 'A' && c <= 'Z') || // Upper case letter
                (c == '-') ||
                (c == '"') ||
                (c == '[') ||
                (c == '{') ||
                (c == '<');
        }
        public static Boolean IsNullable(Type type)
        {
            return !type.IsPrimitive && !type.IsEnum;
        }
        
        public static void ArrayToString(Array array, StringBuilder builder)
        {
            builder.Append('[');
            for (int i = 0; i < array.Length; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(array.GetValue(i).ToString());
            }
            builder.Append(']');
        }

        // Returns the index of the next whitespace that is not part of an Sos string or character.
        public static Int32 NextNonQuotedWhitespace(String str, Int32 offset)
        {
            //
            // Make sure Sos object starts with valid character
            //
            Char c = str[offset];

            while (true)
            {
                // Started
                if (c == '"')
                {
                    while (true)
                    {
                        offset++;
                        if (offset >= str.Length) throw new FormatException("Found double quote that was never closed");
                        c = str[offset];
                        if (c == '"')
                        {
                            // Make sure the quote is not escaped

                            // Get the first previous character that is not a '\'
                            Int32 back;
                            for (back = 1; str[offset - back] == '\\'; back++) ;

                            Boolean endOfString = (back % 2) == 1;

                            if (endOfString) break;
                            continue;
                        }
                    }
                }

                offset++;
                if (offset >= str.Length) return offset;

                c = str[offset];
                if (Char.IsWhiteSpace(c)) return offset;
            }
        }


        //public delegate void ArraySerializer(Array array, StringBuilder builder);


        //
        // Boolean Type
        //
        public static String SerializeBoolean(this Boolean value)                      { return value ? "true" : "false"; }
        public static void SerializeBoolean(this Boolean value, StringBuilder builder) { builder.Append(value ? "true" : "false"); }
        //public static void SerializeBooleanArrayByCasting(Array array, StringBuilder builder) { SerializeBooleanArray((Boolean[])array, builder); }
        public static void SerializeBooleanArray(this Boolean[] values, StringBuilder builder)
        {
            if (values == null) { builder.Append("null"); return; }
            builder.Append('[');
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(values[i] ? "true" : "false");
            }
            builder.Append(']');
        }

        //
        // Integer Types
        //
        public static String SerializeSByte (this SByte  value) { return value.ToString(); }
        public static String SerializeByte  (this Byte   value) { return value.ToString(); }
        public static String SerializeInt16 (this Int16  value) { return value.ToString(); }
        public static String SerializeUInt16(this UInt16 value) { return value.ToString(); }
        public static String SerializeInt32 (this Int32  value) { return value.ToString(); }
        public static String SerializeUInt32(this UInt32 value) { return value.ToString(); }
        public static String SerializeInt64 (this Int64  value) { return value.ToString(); }
        public static String SerializeUInt64(this UInt64 value) { return value.ToString(); }
        public static void   SerializeSByte (this SByte  value, StringBuilder builder) { builder.Append(value.ToString()); }
        public static void   SerializeByte  (this Byte   value, StringBuilder builder) { builder.Append(value.ToString()); }
        public static void   SerializeInt16 (this Int16  value, StringBuilder builder) { builder.Append(value.ToString()); }
        public static void   SerializeUInt16(this UInt16 value, StringBuilder builder) { builder.Append(value.ToString()); }
        public static void   SerializeInt32 (this Int32  value, StringBuilder builder) { builder.Append(value.ToString()); }
        public static void   SerializeUInt32(this UInt32 value, StringBuilder builder) { builder.Append(value.ToString()); }
        public static void   SerializeInt64 (this Int64  value, StringBuilder builder) { builder.Append(value.ToString()); }
        public static void   SerializeUInt64(this UInt64 value, StringBuilder builder) { builder.Append(value.ToString()); }

        //
        // Float Types
        //
        public static String SerializeSingle(this Single value) { return value.ToString("r"); }
        public static String SerializeDouble(this Double value) { return value.ToString("r"); }
        public static void   SerializeSingle(this Single value, StringBuilder builder) { builder.Append(value.ToString("r")); }
        public static void   SerializeDouble(this Double value, StringBuilder builder) { builder.Append(value.ToString("r")); }

        //public static void SerializeSingleArrayByCasting(Array array, StringBuilder builder) { SerializeSingleArray((Single[])array, builder); }
        public static void SerializeSingleArray(this Single[] values, StringBuilder builder)
        {
            if (values == null) { builder.Append("null"); return; }
            builder.Append('[');
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(values[i].ToString("r"));
            }
            builder.Append(']');
        }
        //public static void SerializeDoubleArrayByCasting(Array array, StringBuilder builder) { SerializeDoubleArray((Double[])array, builder); }
        public static void SerializeDoubleArray(this Double[] values, StringBuilder builder)
        {
            if (values == null) { builder.Append("null"); return; }
            builder.Append('[');
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(values[i].ToString("r"));
            }
            builder.Append(']');
        }

        //
        // Character Types
        //
        public static String SerializeChar(this Char value)
        {
            if (value >= ' ' && value <= '~')
            {
                if (value == '\\') return "\"\\\\\"";   // returns "\\"
                if (value == '"') return "\"\\\"\"";    // return  "\""
                return "\"" + value + "\"";
            }

            if (value == '\n') return @"""\n""";
            if (value == '\r') return @"""\r""";
            if (value == '\t') return @"""\t""";
            if (value == '\0') return @"""\0""";

            UInt32 valueAsUInt32 = (UInt32)value;
            if (valueAsUInt32 <= 0xFF) return @"""\x" + valueAsUInt32.ToString("X2") + "\"";
            if (valueAsUInt32 <= 0xFFFF) return @"""\u" + valueAsUInt32.ToString("X4") + "\"";
            throw new InvalidOperationException(String.Format("The character '{0}' (code = {1}) has a code that cannot be represented by 2 bytes, which is currently not supported", value, valueAsUInt32));
        }
        //public static void SerializeCharArrayByCasting(Array array, StringBuilder builder) { SerializeCharArray((Char[])array, builder); }
        public static void SerializeCharArray(this Char[] values, StringBuilder builder)
        {
            if (values == null) { builder.Append("null"); return; }
            builder.Append('[');
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(SerializeChar((Char)values[i]));
            }
            builder.Append(']');
        }
        public static String SerializeString(this String value)
        {
            if (value == null) return "null";
            if (value.Length <= 0) return "\"\"";

            //
            // Sos will only serialize strings as non-quoted if they only contain characters
            // from [0-9A-Za-z].
            //
            for (int i = 0; i < value.Length; i++)
            {
                Char c = value[i];
                if (
                    (c < '0') ||
                    (c > '9' && c < 'A') ||
                    (c > 'Z' && c < 'a') ||
                    (c > 'z'))
                {
                    //
                    // TODO: escape non-printable characeters
                    //
                    // Add quotes and escape necessary characters
                    //return String.Format("\"{0}\"", value.Replace(@"\", @"\\").Replace("\"", "\\\""));
                    return "\"" + value.Replace(@"\", @"\\").Replace("\"", "\\\"") + "\""; // Faster
                }
            }
            if (value.Equals("null")) return "\"null\""; // the string "null" cannot be sent without quotes because it will be interpreted as a null string
            return value;
        }
        /*
        public static void SerializeStringArrayByCasting(Array array, StringBuilder builder)
        {
            SerializeStringArray((String[])array, builder);
        }
        */
        public static void SerializeStringArray(this String[] values, StringBuilder builder)
        {
            if (values == null) { builder.Append("null"); return; }
            builder.Append('[');
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(SerializeString(values[i]));
            }
            builder.Append(']');
        }

        public static String SerializeObject(this Object obj, Type serializeType)
        {
            StringBuilder builder = new StringBuilder();
            SerializeObject(obj, serializeType, builder);
            return builder.ToString();
        }
        public static String SerializeObject(this Object obj)
        {
            if (obj == null) return "null";

            StringBuilder builder = new StringBuilder();
            SerializeObject(obj, obj.GetType(), builder);
            return builder.ToString();
        }

        //
        // Right now it does not handle circular references
        //
        public static void SerializeObject(this Object obj, StringBuilder builder)
        {
            if (obj == null)
            {
                builder.Append("null");
                return;
            }

            SerializeObject(obj, obj.GetType(), builder);
        }
        public static void SerializeObject(this Object obj, Type serializeType, StringBuilder builder)
        {
            //
            // Handle primitive types
            //
            if (serializeType.IsPrimitive)
            {
                //
                // Primitive types that do not call the default ToString()
                //
                if (serializeType == typeof(Boolean))
                {
                    builder.Append(((Boolean)obj) ? "true" : "false");
                    return;
                }
                if (serializeType == typeof(Char))
                {
                    builder.Append(SerializeChar((Char)obj));
                    return;
                }
                if (serializeType == typeof(Single) || serializeType == typeof(Double))
                {
                    // The 'r' format specifier stands for "Round Trip"
                    // Which means the number should be printed in a way that the value
                    // can be parsed from the string without losing information.
                    if (serializeType == typeof(Single))
                    {
                        builder.Append(((Single)obj).ToString("r"));
                        return;
                    }
                    else
                    {
                        builder.Append(((Double)obj).ToString("r"));
                        return;
                    }
                }

                //
                // The rest of the primitive types just call the default ToString()
                //
                builder.Append(obj.ToString());
                return;
            }

            //
            // Handle enums
            //
            if (serializeType.IsEnum)
            {
                builder.Append(obj.ToString());
                return;
            }

            //
            // The rest of the types are nullable
            //
            if (obj == null)
            {
                builder.Append("null");
                return;
            }

            if (serializeType == typeof(String))
            {
                builder.Append(SerializeString((String)obj));
                return;
            }

            //
            // Handle Arrays
            //
            if (serializeType.IsArray)
            {
                SerializeArray((Array)obj, builder);
                return;
            }

            //
            // Serialize it as a an object of primitive types
            //
            builder.Append('{');
            FieldInfo[] fieldInfos = serializeType.GetSerializedFields();

            if (fieldInfos == null || fieldInfos.Length <= 0)
            {
                builder.Append('}');
                return;
            }

            FieldInfo fieldInfo = fieldInfos[0];

            builder.Append(fieldInfo.Name);
            builder.Append(":");
            SerializeObject(fieldInfo.GetValue(obj), fieldInfo.FieldType, builder);

            for (int i = 1; i < fieldInfos.Length; i++)
            {
                builder.Append(',');

                fieldInfo = fieldInfos[i];
                builder.Append(fieldInfo.Name);
                builder.Append(":");
                SerializeObject(fieldInfo.GetValue(obj), fieldInfo.FieldType, builder);
            }
            builder.Append('}');
        }
        public static void SerializeArray(this Array array, StringBuilder builder)
        {
            if (array == null)
            {
                builder.Append("null");
                return;
            }

            Type elementType = array.GetType().GetElementType();

            //
            // Handle primitive types
            //
            if (elementType.IsPrimitive)
            {
                //
                // Primitive types that do not call the default ToString()
                //
                if (elementType == typeof(Boolean))
                {
                    SerializeBooleanArray((Boolean[])array, builder);
                    return;
                }
                if (elementType == typeof(Char))
                {
                    SerializeCharArray((Char[])array, builder);
                    return;
                }
                if (elementType == typeof(Single))
                {
                    SerializeSingleArray((Single[])array, builder);
                    return;
                }
                if (elementType == typeof(Double))
                {
                    SerializeDoubleArray((Double[])array, builder);
                    return;
                }

                //
                // The rest of the primitive types just call the default ToString()
                //
                ArrayToString(array, builder);
                return;
            }

            if (elementType == typeof(String))
            {
                SerializeStringArray((String[])array, builder);
                return;
            }

            //
            // Handle Arrays
            //
            if (elementType.IsArray)
            {
                builder.Append('[');
                for (int i = 0; i < array.Length; i++)
                {
                    if (i > 0) builder.Append(',');
                    SerializeArray(array, builder);
                }
                builder.Append(']');
                return;
            }

            //
            // Handle enums
            //
            if (elementType.IsEnum)
            {
                ArrayToString(array, builder);
                return;
            }

            //
            // The data is an array of objects so it is a table
            //
            /*
             * FOR NOW TABLES ARE NOT SUPPORTED
             * 
            builder.Append('<');
            FieldInfo[] fieldInfos = elementType.GetSerializedFields();

            if (fieldInfos == null || fieldInfos.Length <= 0)
            {
                builder.Append('>');
                return;
            }

            //
            // Serialize Table Headers
            //
            FieldInfo fieldInfo = fieldInfos[0];
            builder.Append(fieldInfo.Name);
            for (int fieldIndex = 1; fieldIndex < fieldInfos.Length; fieldIndex++)
            {
                fieldInfo = fieldInfos[fieldIndex];
                builder.Append(',');
                builder.Append(fieldInfo.Name);
            }

            //
            // Serialize Rows
            //
            for (int i = 0; i < array.Length; i++)
            {
                Object rowObject = array.GetValue(i);

                builder.Append(":");

                fieldInfo = fieldInfos[0];
                builder.Append(SerializeObject(fieldInfo.GetValue(rowObject)));
                for (int  fieldIndex = 1; fieldIndex < fieldInfos.Length; fieldIndex++)
                {
                    fieldInfo = fieldInfos[fieldIndex];
                    builder.Append(',');
                    builder.Append(SerializeObject(fieldInfo.GetValue(rowObject)));
                }
            }
            builder.Append('>');
            */
            builder.Append('[');
            FieldInfo[] fieldInfos = elementType.GetSerializedFields();

            //
            // Serialize Rows
            //
            for (int i = 0; i < array.Length; i++)
            {
                if (i > 0) builder.Append(',');

                Object rowObject = array.GetValue(i);

                if (rowObject == null)
                {
                    builder.Append("null");
                }
                else
                {
                    builder.Append('{');
                    for (int fieldIndex = 0; fieldIndex < fieldInfos.Length; fieldIndex++)
                    {
                        FieldInfo fieldInfo = fieldInfos[fieldIndex];
                        if (fieldIndex > 0) builder.Append(',');
                        builder.Append(fieldInfo.Name);
                        builder.Append(':');
                        builder.Append(SerializeObject(fieldInfo.GetValue(rowObject)));
                    }
                    builder.Append('}');
                }
            }
            builder.Append(']');
        }



        //
        // Deserialization Methods
        //
        public static Int32 EnumValueLength(String str, Int32 offset, Int32 offsetLimit)
        {
            //
            // Check if it is a number version
            //
            Int32 numberLength = WholeNumberLength(str, offset, offsetLimit);
            if (numberLength > 0) return numberLength;

            Int32 originalOffset = offset;

            while (true)
            {
                if (offset >= offsetLimit) return offset - originalOffset;

                Char check = str[offset];
                if ((check < 'a' || check > 'z') &&
                    (check < 'A' || check > 'Z' ) &&
                    (check < '0' || check > '9') &&
                    check != '_') return offset - originalOffset;

                offset++;
            }
        }
        public static Int32 WholeNumberLength(String str, Int32 offset, Int32 offsetLimit)
        {
            Int32 originalOffset = offset;
            if (str[offset] == '-') offset++;
            while (true)
            {
                if (offset >= offsetLimit) return offset - originalOffset;

                Char check = str[offset];
                if ((check < '0' || check > '9')) return offset - originalOffset;

                offset++;
            }
        }
        public static Int32 FloatLength(String str, Int32 offset, Int32 offsetLimit)
        {
            Int32 originalOffset = offset;
            if(str[offset] == '-') offset++; // By checking it first we are speeding up the performance in the average case

            while(true)
            {
                if (offset >= offsetLimit) return offset - originalOffset;

                Char check = str[offset];
                if(
                    (check < '0' || check > '9') &&
                    check != '.' &&
                    check != 'E' &&
                    check != 'e' &&
                    check != '-' &&
                    check != '+') return offset - originalOffset;

                offset++;
            }
        }

        /*
        delegate Int32 DeserializerMethod(out Object obj, String value, Int32 offset, Int32 offsetLimit);
        public static Int32 DeserializeBoolean(out Object obj, String value, Int32 offset, Int32 offsetLimit)
        {

        }
        */


        //
        // offset points to the first location after '<'
        // returns null if the table is 
        /*
        public static Int32 ParseTableFieldOrder(out FieldInfo[] fieldOrder, Type type, String str, Int32 offset, Int32 offsetLimit)
        {
            Char c;
            //
            // Skip Whitespace
            //
            while (true)
            {
                if (offset >= offsetLimit) throw new FormatException("Table missing closing bracket '>'");
                c = str[offset];
                if (!Char.IsWhiteSpace(c)) break;
                offset++;
            }

            if(c == '>')
            {
                fieldOrder = null;
                return offset + 1;
            }

            Int32 fieldNameStartOffset = offset;

            //
            // Count how many fields there are
            //            
            Int32 commaCount = 0;
            while (true)
            {
                if (c == ',')
                {
                    commaCount++;
                }
                else if(c == ':')
                {
                    break;
                }
                
                offset++;
                if(offset >= offsetLimit) throw new FormatException("Expecting colon ':' to end table fields but reached end of input");
                c = str[offset];
            }

            fieldOrder = new FieldInfo[commaCount + 1];

            Int32 fieldIndex = 0;
            offset = fieldNameStartOffset;
            while (true)
            {
                // Start of field name
                c = str[offset];
                if (c == ',') throw new FormatException("Table field name cannot be empty");
                
                throw new NotImplementedException();
                
                //while (true)
                //{
                //    offset++;
                //    c = str[offset];
                //    if(
                //}
                




            }
        }
        */



        //
        // The function is very long, but it it highly tested in the DeserializeWhiteBoxTest.cs file
        // The white box test should be exercising every line of code in this function.
        //
        // When deserializing an array, right now each element must go through the entire Deserialize function.
        // In the future it would likely be faster to create a function for each primitive type and then 
        // create a function to get the deserializer method for an array's element type.
        public static Int32 Deserialize(out Object obj, Type type, String value, Int32 offset, Int32 offsetLimit)
        {
            //
            // Skip Whitespace
            //
            while (true)
            {
                if (offset >= offsetLimit) throw new FormatException("Unexpected end of input");
                Char c = value[offset];
                if (!Char.IsWhiteSpace(c)) break;
                offset++;
            }

            //
            // Handle primitive types
            //
            if (type.IsPrimitive && type != typeof(Char))
            {
                if (type == typeof(Boolean))
                {
                    if (
                        value[offset    ] == 'f' &&
                        value[offset + 1] == 'a' &&
                        value[offset + 2] == 'l' &&
                        value[offset + 3] == 's' &&
                        value[offset + 4] == 'e')
                    {
                        obj = false;
                        return offset + 5;
                    }
                    else if (
                        value[offset    ] == 't' &&
                        value[offset + 1] == 'r' &&
                        value[offset + 2] == 'u' &&
                        value[offset + 3] == 'e')
                    {
                        obj = true;
                        return offset + 4;
                    }

                    throw new FormatException(String.Format(
                        "Expected boolean value to be 'true' or 'false' but was '{0}'...?",
                        value.Substring(offset, offsetLimit - offset)));
                }
                if (type == typeof(Single) || type == typeof(Double))
                {
                    //
                    // Check for NaN
                    //
                    if (
                        (value[offset    ] == 'N' || value[offset    ] == 'n') &&
                        (value[offset + 1] == 'a' || value[offset + 1] == 'A') &&
                        (value[offset + 2] == 'N' || value[offset + 2] == 'n'))
                    {
                        if (type == typeof(Single))
                        {
                            obj = Single.NaN;
                        }
                        else
                        {
                            obj = Double.NaN;
                        }
                        return offset + 3;
                    }

                    //
                    // Check for Infinity
                    //
                    Int32 infinityOffset = offset + ((value[offset] == '-') ? 1 : 0);
                    if (
                        (value[infinityOffset    ] == 'I' || value[infinityOffset    ] == 'i') &&
                        (value[infinityOffset + 1] == 'n' || value[infinityOffset + 1] == 'N') &&
                        (value[infinityOffset + 2] == 'f' || value[infinityOffset + 2] == 'F') &&
                        (value[infinityOffset + 3] == 'i' || value[infinityOffset + 3] == 'I') &&
                        (value[infinityOffset + 4] == 'n' || value[infinityOffset + 4] == 'N') &&
                        (value[infinityOffset + 5] == 'i' || value[infinityOffset + 5] == 'I') &&
                        (value[infinityOffset + 6] == 't' || value[infinityOffset + 6] == 'T') &&
                        (value[infinityOffset + 7] == 'y' || value[infinityOffset + 7] == 'Y'))
                    {
                        if (infinityOffset == offset)
                        {
                            if (type == typeof(Single))
                            {
                                obj = Single.PositiveInfinity;
                            }
                            else
                            {
                                obj = Double.PositiveInfinity;
                            }
                            return offset + 8;
                        }
                        if (type == typeof(Single))
                        {
                            obj = Single.NegativeInfinity;
                        }
                        else
                        {
                            obj = Double.NegativeInfinity;
                        }
                        return offset + 9;
                    }
                }

                //
                // Get number characters
                //
                Int32 numberLength = FloatLength(value, offset, offsetLimit);
                if(numberLength <= 0) throw new FormatException(String.Format(
                    "Expected number but got '{0}'", value.Substring(offset)));

                String numberString = value.Substring(offset, numberLength);
                offset += numberLength;

                //
                // Slower way using reflection
                //
                //MethodInfo parseMethod = type.GetMethod("Parse", StringParam);
                //obj = parseMethod.Invoke(null, new Object[] { numberString });

                //
                // Faster way using If-ElseIf
                //
                if (type == typeof(SByte))
                {
                    obj = SByte.Parse(numberString);
                }
                else if (type == typeof(Byte))
                {
                    obj = Byte.Parse(numberString);
                }
                else if (type == typeof(Int16))
                {
                    obj = Int16.Parse(numberString);
                }
                else if (type == typeof(UInt16))
                {
                    obj = UInt16.Parse(numberString);
                }
                else if (type == typeof(Int32))
                {
                    obj = Int32.Parse(numberString);
                }
                else if (type == typeof(UInt32))
                {
                    obj = UInt32.Parse(numberString);
                }
                else if (type == typeof(Int64))
                {
                    obj = Int64.Parse(numberString);
                }
                else if (type == typeof(UInt64))
                {
                    obj = UInt64.Parse(numberString);
                }
                else if (type == typeof(Single))
                {
                    obj = Single.Parse(numberString);
                }
                else if (type == typeof(Double))
                {
                    obj = Double.Parse(numberString);
                }
                else if (type == typeof(IntPtr))
                {
                    obj = new IntPtr(Int64.Parse(numberString));
                }
                else if (type == typeof(UIntPtr))
                {
                    obj = new UIntPtr(UInt64.Parse(numberString));
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Unknown Type '{0}'", type.Name));
                }

                return offset;
            }

            //
            // Handle enums
            //
            if (type.IsEnum)
            {
                Int32 enumValueLength = EnumValueLength(value, offset, offsetLimit);
                if (enumValueLength > 0)
                {
                    String enumValueString = value.Substring(offset, enumValueLength);
                    try
                    {
                        obj = Enum.Parse(type, enumValueString, true);
                    }
                    catch (Exception)
                    {
                        throw new FormatException(String.Format("Enum value '{0}' is not valid for enum '{1}'", enumValueString, type.Name));
                    }
                    return offset + enumValueLength;
                }
                throw new FormatException(String.Format("Expected an enum value name or a number but got '{0}'",
                    value.Substring(offset)));
            }

            //
            // The rest of the types are nullable so now is the time to check if it is null
            //
            if (
                (offset + 3 < offsetLimit) &&
                value[offset    ] == 'n' &&
                value[offset + 1] == 'u' &&
                value[offset + 2] == 'l' &&
                value[offset + 3] == 'l')
            {
                if (type == typeof(Char)) throw new FormatException("Sos string 'null' is not valid for the CSharp Char type");

                obj = null;
                return offset + 4;
            }

            if (type == typeof(String) || type == typeof(Char))
            {
                Char c = value[offset];

                //
                // Quoted String
                //
                if (c == '"')
                {
                    offset++;
                    Int32 startOffset = offset;

                    while (true)
                    {
                        if (offset >= offsetLimit) throw new FormatException(String.Format("Missing closing quote for string '{0}'", value.Substring(startOffset-1, offsetLimit - startOffset + 1)));
                        c = value[offset];
                        if (c == '"')
                        {
                            if(type == typeof(Char))
                            {
                                if (offset - startOffset > 1) throw new FormatException(String.Format("Sos String representing a CSharp Char type can only be one character long but it is {0}", offset - startOffset));
                                obj = value[startOffset];
                            }
                            else
                            {
                                obj = value.Substring(startOffset, offset - startOffset);
                            }
                            return offset + 1;
                        }
                        if (c == '\\') break;
                        offset++;
                    }

                    Int32 firstEscapeOffset = offset;
                    offset += 2;

                    //
                    // Find the ending quote
                    //
                    while (true)
                    {
                        if (offset >= offsetLimit) throw new FormatException(String.Format("Missing closing quote for string '{0}'", value.Substring(startOffset-1, offsetLimit - startOffset + 1)));
                        c = value[offset];

                        if (c == '"')
                        {
                            // Check that the quote is not escaped
                            Boolean isEscaped;
                            Int32 backwardOffset = offset - 1;
                            while(true)
                            {
                                c = value[backwardOffset];
                                backwardOffset--;
                                if(c != '\\' || backwardOffset <= firstEscapeOffset)
                                {
                                    isEscaped = false;
                                    break;
                                }
                                c = value[backwardOffset];
                                backwardOffset--;
                                if (c != '\\' || backwardOffset <= firstEscapeOffset)
                                {
                                    isEscaped = true;
                                    break;
                                }
                            }
                            if (!isEscaped) break;
                        }
                        offset++;
                    }
                    
                    Char[] stringChars = new Char[offset - startOffset];
                    Int32 stringCharsIndex ;
                    for (stringCharsIndex = 0; stringCharsIndex < firstEscapeOffset - startOffset; stringCharsIndex++)
                    {
                        stringChars[stringCharsIndex] = value[startOffset + stringCharsIndex];
                    }

                    offset = firstEscapeOffset;

                    while (true)
                    {
                        if (value[offset] != '\\')
                        {
                            stringChars[stringCharsIndex++] = value[offset];
                        }
                        else
                        {
                            offset++; // No need to check if offset exceeds limit because it won't because the closing quote was already found

                            //
                            // TODO: use a map to translate the char to it's respective value
                            //
                            c = value[offset];
                            if (c == '\\')
                            {
                                stringChars[stringCharsIndex++] = '\\';
                            }
                            else if (c == '"')
                            {
                                stringChars[stringCharsIndex++] = '\"';
                            }
                            else if (c == 'n')
                            {
                                stringChars[stringCharsIndex++] = '\n';
                            }
                            else if (c == 't')
                            {
                                stringChars[stringCharsIndex++] = '\t';
                            }
                            else if (c == 'r')
                            {
                                stringChars[stringCharsIndex++] = '\r';
                            }
                            else if (c == '0')
                            {
                                stringChars[stringCharsIndex++] = '\0';
                            }
                            else if (c == 'x')
                            {
                                if (offset + 2 >= offsetLimit) throw new FormatException("The \\x escape sequence must be followed by exactly two hex characters but reached end of input");

                                stringChars[stringCharsIndex++] = (Char)((value[offset + 1].HexDigitToValue() << 4) + value[offset + 2].HexDigitToValue());

                                offset += 2;
                            }
                            else if (c == 'u')
                            {
                                if (offset + 4 >= offsetLimit) throw new FormatException("The \\u escape sequence must be followed by exactly four hex characters but reached end of input");
                                stringChars[stringCharsIndex++] = (Char)(
                                    (value[offset + 1].HexDigitToValue() << 12) +
                                    (value[offset + 2].HexDigitToValue() << 8) +
                                    (value[offset + 3].HexDigitToValue() << 4) +
                                    (value[offset + 4].HexDigitToValue()));
                                offset += 4;
                            }
                            else
                            {
                                throw new FormatException(String.Format("Unknown escape sequence '\\{0}'", c));
                            }
                        }
                        offset++;
                        if (offset >= offsetLimit) throw new FormatException("Missing closing quote for string");

                        if (value[offset] == '"')
                        {
                            if(type == typeof(Char))
                            {
                                if (stringCharsIndex > 1) throw new FormatException(String.Format("Sos String representing a CSharp Char type can only be one character long but it is {0}", stringCharsIndex));
                                obj = stringChars[0];
                            }
                            else
                            {
                                obj = new String(stringChars, 0, stringCharsIndex);
                            }
                            return offset + 1;
                        }
                    }
                }
                //
                // Non-Quoted String
                //
                else
                {
                    if (type == typeof(Char))
                    {
                        obj = c;
                        return offset + 1;
                    }


                    Int32 startOffset = offset;
                    while (true)
                    {
                        offset++;                        
                        if (offset >= offsetLimit)
                        {
                            obj = value.Substring(startOffset);
                            return offset;
                        }
                        c = value[offset];
                        if(Char.IsWhiteSpace(c) || IsSosEndOfValueDelimiter(c))
                        {
                            obj = value.Substring(startOffset, offset - startOffset);
                            return offset;
                        }
                    }
                }
            }

            //
            // Handle Arrays/Tables
            //
            if (type.IsArray)
            {
                //
                // Is an Sos Array
                //
                if (value[offset] == '[')
                {
                    Char c;
                    // Skip Whitespace
                    while (true)
                    {
                        offset++;
                        if (offset >= offsetLimit) throw new FormatException("Array missing closing bracket ']'");
                        c = value[offset];
                        if (!Char.IsWhiteSpace(c)) break;
                    }

                    Type elementType = type.GetElementType();

                    //
                    // Check if array is empty
                    //
                    if (c == ']')
                    {
                        obj = Array.CreateInstance(elementType, 0);
                        return offset + 1;
                    }

                    ArrayBuilder arrayBuilder = new ArrayBuilder(elementType);

                    while (true)
                    {
                        Object nextElement;
                        offset = Deserialize(out nextElement, elementType, value, offset, offsetLimit);
                        arrayBuilder.Add(nextElement);

                        // Skip Whitespace
                        while (true)
                        {
                            if (offset >= offsetLimit) throw new FormatException("Array missing closing bracket ']'");
                            c = value[offset];
                            if (!Char.IsWhiteSpace(c)) break;
                            offset++;
                        }
                        if (c == ']')
                        {
                            obj = arrayBuilder.Build();
                            return offset + 1;
                        }
                        if (c != ',')
                            throw new FormatException(String.Format("Expected ',' to delimit an array element but got '{0}' (charcode={1})", c, (UInt32)c));

                        offset++;
                        if (offset >= offsetLimit) throw new FormatException(String.Format("Missing closing ']' for array"));
                    }
                }
                //
                // Is an Sos Table
                //
                else if (value[offset] == '<')
                {
                    Char c;

                    // Skip Whitespace
                    while (true)
                    {
                        offset++;
                        if (offset >= offsetLimit) throw new FormatException("Table missing closing bracket '>'");
                        c = value[offset];
                        if (!Char.IsWhiteSpace(c)) break;
                    }

                    Type rowType = type.GetElementType();

                    //
                    // Check if table is empty
                    //
                    if (value[offset] == '>')
                    {
                        offset++;
                        obj = Array.CreateInstance(rowType, 0);
                        return offset;
                    }

                    throw new NotImplementedException();
                    //ParseFieldOrder(...)
                    /*
                    Int32 headerOffsetStart = offset;
                    Int32 commaCount = 0;
                    while(true)
                    {
                        if(c == ',')
                        {
                            commaCount++;
                        }
                        else if(c == ':')
                        {
                            break;
                        }
                        else if (c == '>')
                        {
                            offset++;
                            obj = Array.CreateInstance(rowType, 0);
                            return offset;
                        }
                        offset++;
                        if (offset >= offsetLimit) throw new FormatException(String.Format("Missing closing '>' for table"));
                    }

                    //
                    // Get header field order
                    //
                    throw new NotImplementedException("Tables not fully implemented yet");
                    
                    Int32 fieldIndex = 0;
                    Int32 headerOffset = 0;
                    String headers = value.Substring(headerOffsetStart, offset - headerOffsetStart);
                    FieldInfo[] fieldOrder = new FieldInfo[commaCount + 1];
                    while(true)
                    {
                        headerOffset++;
                        if (headerOffset >= headers.Length) break;

                        Char c = headers[headerOffset];
                        if (c == ',')
                        {
                            elementType.GetField(headers.Substring(
                        }




                    }


                    offset++;
                    if (offset >= offsetLimit) throw new FormatException(String.Format("Missing closing '>' for table"));







                    //
                    // Get field order
                    //


                    List<String> fields = new List<String>();





                    ArrayBuilder arrayBuilder = new ArrayBuilder(elementType);

                    while (true)
                    {
                        Object nextElement;
                        offset = Deserialize(out nextElement, elementType, value, offset, offsetLimit);
                        arrayBuilder.Add(nextElement);

                        if (offset >= offsetLimit) throw new FormatException(String.Format("Missing closing ']' for array"));
                        if (value[offset] == ']')
                        {
                            offset++;
                            obj = arrayBuilder.Build();
                            return offset;
                        }
                        if (value[offset] != ',')
                            throw new FormatException(String.Format("Expected ',' to delimit an array element but got '{0}' (charcode={1})", value[offset], (UInt32)value[offset]));
                        offset++;
                        if (offset >= offsetLimit) throw new FormatException(String.Format("Missing closing ']' for array"));
                    }
                    */
                }
                else
                {
                    throw new FormatException(String.Format("Expected array to start with '[' or '<' but it started with '{0}' (charcode={1})", value[offset], (UInt32)value[offset]));
                }
            }

            //
            // Serialize it as a an object of primitive types
            //
            obj = FormatterServices.GetUninitializedObject(type);
            //obj = Activator.CreateInstance(type);

            return DeserializeObject(obj, value, offset, offsetLimit);
        }
        private static Int32 DeserializeObject(Object obj, String value, Int32 offset, Int32 offsetLimit)
        {
            Char c;
            // Skip till '{'
            while (true)
            {
                if (offset >= offsetLimit)
                    throw new FormatException("Expected object to start with '{' but it was empty");
                c = value[offset];                
                if (c == '{') break;
                if(!Char.IsWhiteSpace(c)) throw new FormatException(String.Format(
                    "Expected object to start with '{{' but it started with '{0}' (charcode={1})", c, (UInt32)c));
                offset++;
            }

            offset++;
            if (offset >= offsetLimit) throw new FormatException("Missing ending '}'");

            Type type = obj.GetType();

            while (true)
            {
                Int32 fieldNameStart = offset;

                // Skip till ':'
                while (true)
                {
                    c = value[offset];

                    //
                    // End of Object '}'
                    //
                    if (c == '}')
                    {
                        // Check that there is only whitespace
                        while (fieldNameStart < offset)
                        {
                            if (value[fieldNameStart] != ' ' && value[fieldNameStart] != '\t' && value[fieldNameStart] != '\n')
                                throw new FormatException(string.Format("Found end of object '}' but it was preceded by '{0}'",
                                    value.Substring(fieldNameStart, offset - fieldNameStart)));
                            fieldNameStart++;
                        }
                        return offset + 1;
                    }

                    //
                    // At Field Value ':'
                    //
                    if (c == ':') break;

                    offset++;
                    if (offset >= offsetLimit) throw new FormatException("Expected ':' but reached end of string'");
                }

                offset++;
                if (offset >= offsetLimit) throw new FormatException("Expected value after ':' but reached end of string");

                //
                // Deserialize the field
                //
                String fieldName = value.Substring(fieldNameStart, offset - fieldNameStart - 1).Trim();
                FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (fieldInfo == null) throw new InvalidOperationException(String.Format("Could not find field '{0}' in type {1}", fieldName, type.Name));

                Type fieldType = fieldInfo.FieldType;


                Object fieldValue;
                offset = Deserialize(out fieldValue, fieldType, value, offset, offsetLimit);
                fieldInfo.SetValue(obj, fieldValue);

                //
                // Check for ',' or '}'
                //
                if (offset >= offsetLimit) throw new FormatException("Missing ending '}'");
                Char next = value[offset];
                if (next == ',')
                {
                    offset++;
                    if (offset >= offsetLimit) throw new FormatException("Expected field name but reached end of string");
                    continue;
                }
                if (next == '}') return offset + 1;
                throw new FormatException(String.Format("Expected ',' or '}}' but got '{0}' (charcode={1})", next, (UInt32)next));
            }
        }
    }
}
