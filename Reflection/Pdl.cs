// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;

namespace More
{
    //
    // Serializer Reflector Type
    //
    /*
    public class PdlFieldSerializerReflector : ClassFieldReflector
    {
        readonly Byte lengthByteCount;

        public PdlFieldSerializerReflector(Type classThatHasThisField, String fieldName, Byte lengthByteCount)
            : base(classThatHasThisField, fieldName, typeof(ISerializer))
        {
            this.lengthByteCount = lengthByteCount;
        }
        // Since the serializer field can change, the length can always change
        public override UInt32 FixedSerializationLength()
        {
            return UInt32.MaxValue;
        }
        public override UInt32 SerializationLength(object instance)
        {
            return lengthByteCount + ((ISerializer)fieldInfo.GetValue(instance)).SerializationLength();
        }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            ISerializer fieldSerializer = (ISerializer)fieldInfo.GetValue(instance);
            
            Int32 originalOffset = offset;
            offset = fieldSerializer.Serialize(array, offset + lengthByteCount);

            UInt32 fieldLength = (UInt32)(offset - originalOffset - lengthByteCount);

            // check that length is valid
            if (!ByteArray.IsInRange(fieldLength, lengthByteCount))
                throw new InvalidOperationException(String.Format("This field {0}-byte reflector length {1} is out of range",
                    lengthByteCount, fieldLength));

            // insert length
            array.BigEndianSetUInt32Subtype(originalOffset, fieldLength, lengthByteCount);

            return offset;
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 offsetLimit)
        {
            UInt32 length = array.BigEndianReadUInt32Subtype(offset, lengthByteCount);

            offset += lengthByteCount;

            ISerializer fieldSerializer = (ISerializer)fieldInfo.GetValue(instance);
            Int32 deserializeOffset = fieldSerializer.Deserialize(array, offset, offset + (Int32)length - 1);

            if(deserializeOffset != offset + length)
                throw new InvalidOperationException(String.Format(
                    "Expected field serializer to deserialize {0} bytes but only used {1}",
                    length, deserializeOffset - offset));

            return deserializeOffset;
        }
        public override void DataSmallString(object instance, StringBuilder builder)
        {
            ((ISerializer)fieldInfo.GetValue(instance)).DataSmallString(builder);
        }
        public override void DataString(object instance, StringBuilder builder)
        {
            ((ISerializer)fieldInfo.GetValue(instance)).DataString(builder);
        }
    }
    */
}
