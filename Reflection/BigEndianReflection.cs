// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Text;

namespace More
{
    /*
    public class ByteReflector : ClassFieldReflector
    {
        public ByteReflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(Byte))
        {
        }
        public override UInt32 FixedSerializationLength()           { return 1; }
        public override UInt32 SerializationLength(Object instance) { return 1; }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            array[offset++] = (Byte)fieldInfo.GetValue(instance);
            return offset;
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            fieldInfo.SetValue(instance, array[offset]);
            return offset + 1;
        }
        public override void DataString(object instance, StringBuilder builder)
        {
            builder.Append(((Byte)fieldInfo.GetValue(instance)).ToString());
        }
    }
    public class ByteSerializer : FixedLengthInstanceSerializer<Byte>
    {
        private static ByteSerializer instance;
        public static ByteSerializer Instance { get { if (instance == null) instance = new ByteSerializer(); return instance; } }
        private ByteSerializer() { }

        public override UInt32 FixedSerializationLength()                             { return 1; }
        public override void FixedLengthSerialize(byte[] bytes, int offset, Byte instance) { bytes[offset] = instance; }
        public override Byte FixedLengthDeserialize(byte[] array, int offset) { return array[offset]; }
        public override void DataString(Byte instance, StringBuilder builder)      { builder.Append(instance); }
        public override void DataSmallString(Byte instance, StringBuilder builder) { builder.Append(instance); }
    }
    */
    public class ByteEnumSerializer<EnumType> : FixedLengthInstanceSerializer<EnumType>
    {
        private static ByteEnumSerializer<EnumType> instance;
        public static ByteEnumSerializer<EnumType> Instance { get { if (instance == null) instance = new ByteEnumSerializer<EnumType>(); return instance; } }
        private ByteEnumSerializer() { }

        public override UInt32 FixedSerializationLength() { return 1; }
        public override void FixedLengthSerialize(byte[] bytes, UInt32 offset, EnumType instance) { bytes[offset] = Convert.ToByte(instance); }
        public override EnumType FixedLengthDeserialize(byte[] array, UInt32 offset) { return (EnumType)Enum.ToObject(typeof(EnumType), array[offset]); }
        public override void DataString(EnumType instance, StringBuilder builder) { builder.Append(instance); }
        public override void DataSmallString(EnumType instance, StringBuilder builder) { builder.Append(instance); }
    }
    /*
    public class SByteReflector : ClassFieldReflector
    {
        public SByteReflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(SByte))
        {
        }
        public override UInt32 FixedSerializationLength()           { return 1; }
        public override UInt32 SerializationLength(Object instance) { return 1; }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            array[offset++] = (Byte)fieldInfo.GetValue(instance);
            return offset;
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            fieldInfo.SetValue(instance, (SByte)array[offset]);
            return offset + 1;
        }
        public override void DataString(object instance, StringBuilder builder)
        {
            builder.Append(((SByte)fieldInfo.GetValue(instance)).ToString());
        }
    }
    public class SByteSerializer : FixedLengthInstanceSerializer<SByte>
    {
        private static SByteSerializer instance;
        public static SByteSerializer Instance { get { if (instance == null) instance = new SByteSerializer(); return instance; } }
        private SByteSerializer() { }

        public int FixedSerializationLength()                              { return 1; }
        public void Serialize(byte[] bytes, int offset, SByte instance)    { bytes[offset] = (Byte)instance; }
        public SByte Deserialize(byte[] array, int offset)                 { return (SByte)array[offset]; }
        public void DataString(SByte instance, StringBuilder builder)      { builder.Append(instance); }
        public void DataSmallString(SByte instance, StringBuilder builder) { builder.Append(instance); }
    }
    */

    public class BigEndianUInt16Reflector : ClassFieldReflector
    {
        public BigEndianUInt16Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(UInt16))
        {
        }
        public override UInt32 FixedSerializationLength() { return 2; }
        public override UInt32 SerializationLength(Object instance) { return 2; }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            UInt16 value = (UInt16)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 8);
            array[offset++] = (Byte)(value);
            return offset;
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 maxOffset)
        {
            UInt16 value = (UInt16)(
                (0xFF00 & (array[offset] << 8)) |
                (0x00FF & (array[offset + 1])));

            fieldInfo.SetValue(instance, value);
            return offset + 2;
        }
        public override void DataString(object instance, StringBuilder builder)
        {
            builder.Append(((UInt16)fieldInfo.GetValue(instance)).ToString());
        }
    }
    public class BigEndianUInt16Serializer : FixedLengthInstanceSerializer<UInt16>
    {
        private static BigEndianUInt16Serializer instance;
        public static BigEndianUInt16Serializer Instance { get { if (instance == null) instance = new BigEndianUInt16Serializer(); return instance; } }
        private BigEndianUInt16Serializer() { }

        public override UInt32 FixedSerializationLength() { return 2; }
        public override void FixedLengthSerialize(byte[] bytes, UInt32 offset, UInt16 value)
        {
            bytes[offset++] = (Byte)(value >> 8);
            bytes[offset++] = (Byte)(value);
        }
        public override UInt16 FixedLengthDeserialize(byte[] array, UInt32 offset)
        {
            return (UInt16)(
                (0xFF00 & (array[offset] << 8)) |
                (0x00FF & (array[offset + 1])));
        }
        public override void DataString(UInt16 instance, StringBuilder builder) { builder.Append(instance); }
        public override void DataSmallString(UInt16 instance, StringBuilder builder) { builder.Append(instance); }
    }

    public class BigEndianInt16Reflector : ClassFieldReflector
    {
        public BigEndianInt16Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(Int16))
        {
        }
        public override UInt32 FixedSerializationLength() { return 2; }
        public override UInt32 SerializationLength(Object instance) { return 2; }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            Int16 value = (Int16)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 8);
            array[offset++] = (Byte)(value);
            return offset;
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 offsetLimit)
        {
            Int16 value = (Int16)(
                (0xFF00 & (array[offset] << 8)) |
                (0x00FF & (array[offset + 1])));

            fieldInfo.SetValue(instance, value);
            return offset + 2;
        }
        public override void DataString(object instance, StringBuilder builder)
        {
            builder.Append(((Int16)fieldInfo.GetValue(instance)).ToString());
        }
    }
    public class BigEndianInt16Serializer : FixedLengthInstanceSerializer<Int16>
    {
        private static BigEndianInt16Serializer instance;
        public static BigEndianInt16Serializer Instance { get { if (instance == null) instance = new BigEndianInt16Serializer(); return instance; } }
        private BigEndianInt16Serializer() { }

        public override UInt32 FixedSerializationLength() { return 2; }
        public override void FixedLengthSerialize(byte[] bytes, UInt32 offset, Int16 value)
        {
            bytes[offset++] = (Byte)(value >> 8);
            bytes[offset++] = (Byte)(value);
        }
        public override Int16 FixedLengthDeserialize(byte[] array, UInt32 offset)
        {
            return (Int16)(
                (0xFF00 & (array[offset] << 8)) |
                (0x00FF & (array[offset + 1])));
        }
        public override void DataString(Int16 instance, StringBuilder builder) { builder.Append(instance); }
        public override void DataSmallString(Int16 instance, StringBuilder builder) { builder.Append(instance); }
    }

    public class BigEndianUInt24Reflector : ClassFieldReflector
    {
        public BigEndianUInt24Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(UInt32))
        {
        }
        public override UInt32 FixedSerializationLength() { return 3; }
        public override UInt32 SerializationLength(Object instance) { return 3; }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            UInt32 value = (UInt32)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 16);
            array[offset++] = (Byte)(value >> 8);
            array[offset++] = (Byte)(value);
            return offset;
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 maxOffset)
        {
            UInt32 value = (UInt32)(
                (0xFF0000 & (array[offset] << 16)) |
                (0x00FF00 & (array[offset + 1] << 8)) |
                (0x0000FF & (array[offset + 2])));

            fieldInfo.SetValue(instance, value);
            return offset + 3;
        }
        public override void DataString(object instance, StringBuilder builder)
        {
            builder.Append(((UInt32)fieldInfo.GetValue(instance)).ToString());
        }
    }
    public class BigEndianInt24Reflector : ClassFieldReflector
    {
        public BigEndianInt24Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(Int32))
        {
        }
        public override UInt32 FixedSerializationLength() { return 3; }
        public override UInt32 SerializationLength(Object instance) { return 3; }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            Int32 value = (Int32)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 16);
            array[offset++] = (Byte)(value >> 8);
            array[offset++] = (Byte)(value);
            return offset;
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 maxOffset)
        {
            Int32 value = (Int32)(
                (0xFF0000 & (array[offset] << 16)) |
                (0x00FF00 & (array[offset + 1] << 8)) |
                (0x0000FF & (array[offset + 2])));

            // Extend sign bit
            if ((value & 0x800000) == 0x800000)
            {
                value |= unchecked((Int32)0xFF000000);
            }

            fieldInfo.SetValue(instance, value);
            return offset + 3;
        }
        public override void DataString(object instance, StringBuilder builder)
        {
            builder.Append(((Int32)fieldInfo.GetValue(instance)).ToString());
        }
    }
    public class BigEndianUInt32Reflector : ClassFieldReflector
    {
        public BigEndianUInt32Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(UInt32))
        {
        }
        public override UInt32 FixedSerializationLength() { return 4; }
        public override UInt32 SerializationLength(Object instance) { return 4; }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            UInt32 value = (UInt32)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 24);
            array[offset++] = (Byte)(value >> 16);
            array[offset++] = (Byte)(value >> 8);
            array[offset++] = (Byte)(value);
            return offset;
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 maxOffset)
        {
            UInt32 value = (UInt32)(
                (0xFF000000U & (array[offset] << 24)) |
                (0x00FF0000U & (array[offset + 1] << 16)) |
                (0x0000FF00U & (array[offset + 2] << 8)) |
                (0x000000FFU & (array[offset + 3])));

            fieldInfo.SetValue(instance, value);
            return offset + 4;
        }
        public override void DataString(object instance, StringBuilder builder)
        {
            builder.Append(((UInt32)fieldInfo.GetValue(instance)).ToString());
        }
    }
    public class BigEndianUInt32Serializer : FixedLengthInstanceSerializer<UInt32>
    {
        private static BigEndianUInt32Serializer instance;
        public static BigEndianUInt32Serializer Instance { get { if (instance == null) instance = new BigEndianUInt32Serializer(); return instance; } }
        private BigEndianUInt32Serializer() { }

        public override UInt32 FixedSerializationLength() { return 4; }
        public override void FixedLengthSerialize(Byte[] bytes, UInt32 offset, UInt32 instance)
        {
            bytes[offset++] = (Byte)(instance >> 24);
            bytes[offset++] = (Byte)(instance >> 16);
            bytes[offset++] = (Byte)(instance >> 8);
            bytes[offset++] = (Byte)(instance);
        }
        public override UInt32 FixedLengthDeserialize(Byte[] array, UInt32 offset)
        {
            return (UInt32)(
                (0xFF000000U & (array[offset] << 24)) |
                (0x00FF0000U & (array[offset + 1] << 16)) |
                (0x0000FF00U & (array[offset + 2] << 8)) |
                (0x000000FFU & (array[offset + 3])));
        }
        public override void DataString(UInt32 instance, StringBuilder builder) { builder.Append(instance); }
        public override void DataSmallString(UInt32 instance, StringBuilder builder) { builder.Append(instance); }
    }
    public class BigEndianInt32Reflector : ClassFieldReflector
    {
        public BigEndianInt32Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(Int32))
        {
        }
        public override UInt32 FixedSerializationLength() { return 4; }
        public override UInt32 SerializationLength(Object instance) { return 4; }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            Int32 value = (Int32)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 24);
            array[offset++] = (Byte)(value >> 16);
            array[offset++] = (Byte)(value >> 8);
            array[offset++] = (Byte)(value);
            return offset;
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 maxOffset)
        {
            Int32 value = (Int32)(
                (0xFF000000U & (array[offset] << 24)) |
                (0x00FF0000U & (array[offset + 1] << 16)) |
                (0x0000FF00U & (array[offset + 2] << 8)) |
                (0x000000FFU & (array[offset + 3])));

            fieldInfo.SetValue(instance, value);
            return offset + 4;
        }
        public override void DataString(object instance, StringBuilder builder)
        {
            builder.Append(((Int32)fieldInfo.GetValue(instance)).ToString());
        }
    }
    public class BigEndianUInt64Reflector : ClassFieldReflector
    {
        public BigEndianUInt64Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(UInt64))
        {
        }
        public override UInt32 FixedSerializationLength() { return 8; }
        public override UInt32 SerializationLength(Object instance) { return 8; }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            UInt64 value = (UInt64)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 56);
            array[offset++] = (Byte)(value >> 48);
            array[offset++] = (Byte)(value >> 40);
            array[offset++] = (Byte)(value >> 32);
            array[offset++] = (Byte)(value >> 24);
            array[offset++] = (Byte)(value >> 16);
            array[offset++] = (Byte)(value >> 8);
            array[offset++] = (Byte)(value);
            return offset;
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 maxOffset)
        {
            UInt64 value = (UInt64)(
                (0xFF00000000000000UL & (UInt64)(array[offset] << 56)) |
                (0x00FF000000000000UL & (UInt64)(array[offset + 1] << 48)) |
                (0x0000FF0000000000UL & (UInt64)(array[offset + 2] << 40)) |
                (0x000000FF00000000UL & (UInt64)(array[offset + 3] << 32)) |
                (0x00000000FF000000UL & (UInt64)(array[offset + 4] << 24)) |
                (0x0000000000FF0000UL & (UInt64)(array[offset + 5] << 16)) |
                (0x000000000000FF00UL & (UInt64)(array[offset + 6] << 8)) |
                (0x00000000000000FFUL & (UInt64)(array[offset + 7])));

            fieldInfo.SetValue(instance, value);
            return offset + 8;
        }
        public override void DataString(object instance, StringBuilder builder)
        {
            builder.Append(((UInt64)fieldInfo.GetValue(instance)).ToString());
        }
    }
    public class BigEndianInt64Reflector : ClassFieldReflector
    {
        public BigEndianInt64Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(Int64))
        {
        }
        public override UInt32 FixedSerializationLength() { return 8; }
        public override UInt32 SerializationLength(Object instance) { return 8; }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            Int64 value = (Int64)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 56);
            array[offset++] = (Byte)(value >> 48);
            array[offset++] = (Byte)(value >> 40);
            array[offset++] = (Byte)(value >> 32);
            array[offset++] = (Byte)(value >> 24);
            array[offset++] = (Byte)(value >> 16);
            array[offset++] = (Byte)(value >> 8);
            array[offset++] = (Byte)(value);
            return offset;
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 maxOffset)
        {
            Int64 value = (Int64)(
                (0xFF00000000000000UL & (UInt64)(array[offset] << 56)) |
                (0x00FF000000000000UL & (UInt64)(array[offset + 1] << 48)) |
                (0x0000FF0000000000UL & (UInt64)(array[offset + 2] << 40)) |
                (0x000000FF00000000UL & (UInt64)(array[offset + 3] << 32)) |
                (0x00000000FF000000UL & (UInt64)(array[offset + 4] << 24)) |
                (0x0000000000FF0000UL & (UInt64)(array[offset + 5] << 16)) |
                (0x000000000000FF00UL & (UInt64)(array[offset + 6] << 8)) |
                (0x00000000000000FFUL & (UInt64)(array[offset + 7])));

            fieldInfo.SetValue(instance, value);
            return offset + 8;
        }
        public override void DataString(object instance, StringBuilder builder)
        {
            builder.Append(((Int64)fieldInfo.GetValue(instance)).ToString());
        }
    }

    //
    // Enum Reflector types
    //
    public class BigEndianUnsignedEnumReflector<EnumType> : ClassFieldReflector
    {
        readonly Byte byteCount;
        public BigEndianUnsignedEnumReflector(Type classThatHasThisField, String fieldName, Byte byteCount)
            : base(classThatHasThisField, fieldName, typeof(EnumType))
        {
            this.byteCount = byteCount;
            if (byteCount > 4) throw new NotImplementedException();
            if (byteCount == 0) throw new ArgumentOutOfRangeException("byteCount", "byteCount cannot be 0");
        }
        public override UInt32 FixedSerializationLength() { return byteCount; }
        public override UInt32 SerializationLength(Object instance) { return byteCount; }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            UInt32 value = (UInt32)Convert.ToUInt32((Enum)fieldInfo.GetValue(instance));
            array.BigEndianSetUInt32Subtype(offset, value, byteCount);
            return offset + byteCount;
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 offsetLimit)
        {
            UInt32 valueAsUInt32 = array.BigEndianReadUInt32Subtype(offset, byteCount);
            Enum value = (Enum)Enum.ToObject(typeof(EnumType), valueAsUInt32);
            fieldInfo.SetValue(instance, value);
            return offset + 1;
        }
        public override void DataString(object instance, StringBuilder builder)
        {
            builder.Append(((EnumType)fieldInfo.GetValue(instance)).ToString());
        }
    }
    public class BigEndianUnsignedEnumSerializer<EnumType> : FixedLengthInstanceSerializer<EnumType>
    {
        private static BigEndianUnsignedEnumSerializer<EnumType>
            twoByteInstance, threeByteInstance, fourByteInstance;
        public static BigEndianUnsignedEnumSerializer<EnumType> TwoByteInstance { get { if (twoByteInstance == null) twoByteInstance = new BigEndianUnsignedEnumSerializer<EnumType>(2); return twoByteInstance; } }
        public static BigEndianUnsignedEnumSerializer<EnumType> ThreeByteInstance { get { if (threeByteInstance == null) threeByteInstance = new BigEndianUnsignedEnumSerializer<EnumType>(3); return threeByteInstance; } }
        public static BigEndianUnsignedEnumSerializer<EnumType> FourByteInstance { get { if (fourByteInstance == null) fourByteInstance = new BigEndianUnsignedEnumSerializer<EnumType>(4); return fourByteInstance; } }
        private BigEndianUnsignedEnumSerializer() { }

        public readonly Byte byteCount;
        public BigEndianUnsignedEnumSerializer(Byte byteCount)
        {
            this.byteCount = byteCount;
        }
        public override UInt32 FixedSerializationLength() { return byteCount; }
        public override void FixedLengthSerialize(byte[] bytes, uint offset, EnumType instance)
        {
            bytes.BigEndianSetUInt32Subtype(offset, Convert.ToUInt32(instance), byteCount);
        }
        public override EnumType FixedLengthDeserialize(byte[] bytes, uint offset)
        {
            return (EnumType)Enum.ToObject(typeof(EnumType), bytes.BigEndianReadUInt32Subtype(offset, byteCount));
        }
        public override void DataString(EnumType instance, StringBuilder builder)
        {
            builder.Append(instance.ToString());
        }
        public override void DataSmallString(EnumType instance, StringBuilder builder)
        {
            builder.Append(instance.ToString());
        }
    }
}
