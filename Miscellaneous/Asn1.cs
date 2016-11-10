// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More
{
    public static class Asn1
    {
        public static readonly Byte[] Null = new Byte[] { Asn1.TYPE_NULL, 0 };

        public const Byte TYPE_INTEGER          = 0x02;
        public const Byte TYPE_OCTET_STRING     = 0x04;
        public const Byte TYPE_NULL             = 0x05;
        public const Byte TYPE_OBJECT_ID        = 0x06;

        public const Byte TYPE_SEQUENCE         = 0x30;
        public const Byte TYPE_GET_REQUEST_PDU  = 0xA0;
        public const Byte TYPE_GET_RESPONSE_PDU = 0xA2;
        public const Byte TYPE_SET_REQUEST_PDU  = 0xA3;

        static readonly String[] TypeNames = new String[] {
            "Eoc",             // 0x00
            "Boolean",         // 0x01
            "Integer",         // 0x02
            "BitString",       // 0x03
            "OctetString",     // 0x04
            "Null",            // 0x05
            "ObjectID",        // 0x06
            "ObjectDescriptor",// 0x07
            "External",        // 0x08
            "Real",            // 0x09
            "Enumerated",      // 0x0A
            "EmbeddedPdv",     // 0x0B
            "UTF8String",      // 0x0C
            "RelativeOid",     // 0x0D
            null,              // 0x0E
            null,              // 0x0F
            "Sequence",        // 0x10
            "Set",             // 0x11
            "NumericString",   // 0x12
            "PrintableString", // 0x13
            "T61String",       // 0x14
            "VideotexString",  // 0x15
            "IA5String",       // 0x16
            "UtcTime",         // 0x17
            "GeneralizedTime", // 0x18
            "GraphicString",   // 0x19
            "VisibleString",   // 0x1A
            "GeneralString",   // 0x1B
            "UniversalString", // 0x1C
            "CharacterString", // 0x1D
            "BmpString",       // 0x1E
        };

        public static String GetTypeOrNumberString(Byte asn1Type)
        {
            if (asn1Type < TypeNames.Length)
            {
                String typeString = TypeNames[asn1Type];
                return (typeString == null) ? String.Format("{0:2X}", asn1Type) : typeString;
            }
            switch (asn1Type)
            {
                case 0x30: return "Sequence";
                case 0xA0: return "GetRequestPdu";
                case 0xA2: return "GetResponsePdu";
                case 0xA3: return "SetRequestPdu";
                default: return String.Format("{0:2X}", asn1Type);
            }
        }
        public static String TryGetTypeString(Byte asn1Type)
        {
            if (asn1Type < TypeNames.Length)
            {
                return TypeNames[asn1Type];
            }
            switch (asn1Type)
            {
                case 0x30: return "Sequence";
                case 0xA0: return "GetRequestPdu";
                case 0xA2: return "GetResponsePdu";
                case 0xA3: return "SetRequestPdu";
                default: return null;
            }
        }

        public static Byte DefiniteLengthOctetCount(UInt32 length)
        {
            if (length <= 0x7F) return 1;
            if (length <= 0xFFFF) return 2;
            if (length <= 0xFFFFFF) return 3;
            return 4;
        }
        public static Byte IntegerOctetCount(Int32 value)
        {
            if (value >= 0)
            {
                if (value <= 0x7F) return 1;
                if (value <= 0x7FFF) return 2;
                if (value <= 0x7FFFFF) return 3;
                return 4;
            }
            else
            {
                if (value >= -0x80) return 1;
                if (value >= -0x8000) return 2;
                if (value >= -0x800000) return 3;
                return 4;
            }
        }
        public static Byte IntegerOctetCount(UInt32 value)
        {
            if (value <= 0x7F) return 1;
            if (value <= 0x7FFF) return 2;
            if (value <= 0x7FFFFF) return 3;
            if (value <= 0x7FFFFFFF) return 4;
            return 5;
        }


        public static UInt32 SetInteger(this Byte[] packet, UInt32 offset, Int32 value)
        {
            if (value >= 0) return SetInteger(packet, offset, (UInt32)value);

            throw new NotImplementedException("Setting negative Asn.1 integer values is not implemented yet");
            /*
            if (value <= 0x7F)
            {
                packet[offset] = (Byte)unsignedValue;
                return offset + 1;
            }
            if (unsignedValue <= 0x7FFF)
            {
                packet[offset] = (Byte)(unsignedValue >> 8);
                packet[offset + 1] = (Byte)unsignedValue;
                return offset + 2;
            }
            if (unsignedValue <= 0x7FFFFF)
            {
                packet[offset] = (Byte)(unsignedValue >> 16);
                packet[offset + 1] = (Byte)(unsignedValue >> 8);
                packet[offset + 2] = (Byte)unsignedValue;
                return offset + 3;
            }
            if (unsignedValue <= 0x7FFFFFFF)
            {
                packet[offset] = (Byte)(unsignedValue >> 24);
                packet[offset + 1] = (Byte)(unsignedValue >> 16);
                packet[offset + 2] = (Byte)(unsignedValue >> 8);
                packet[offset + 3] = (Byte)unsignedValue;
                return offset + 4;
            }

            packet[offset] = 0;
            packet[offset + 1] = (Byte)(unsignedValue >> 24);
            packet[offset + 2] = (Byte)(unsignedValue >> 16);
            packet[offset + 3] = (Byte)(unsignedValue >> 8);
            packet[offset + 4] = (Byte)unsignedValue;
            return offset + 5;
            */
        }
        public static UInt32 SetInteger(this Byte[] packet, UInt32 offset, UInt32 unsignedValue)
        {
            if (unsignedValue <= 0x7F)
            {
                packet[offset    ] = (Byte)unsignedValue;
                return offset + 1;
            }
            if (unsignedValue <= 0x7FFF)
            {
                packet[offset    ] = (Byte)(unsignedValue >> 8);
                packet[offset + 1] = (Byte)unsignedValue;
                return offset + 2;
            }
            if (unsignedValue <= 0x7FFFFF)
            {
                packet[offset    ] = (Byte)(unsignedValue >> 16);
                packet[offset + 1] = (Byte)(unsignedValue >> 8);
                packet[offset + 2] = (Byte)unsignedValue;
                return offset + 3;
            }
            if (unsignedValue <= 0x7FFFFFFF)
            {
                packet[offset    ] = (Byte)(unsignedValue >> 24);
                packet[offset + 1] = (Byte)(unsignedValue >> 16);
                packet[offset + 2] = (Byte)(unsignedValue >> 8);
                packet[offset + 3] = (Byte)unsignedValue;
                return offset + 4;
            }

            packet[offset    ] = 0;
            packet[offset + 1] = (Byte)(unsignedValue >> 24);
            packet[offset + 2] = (Byte)(unsignedValue >> 16);
            packet[offset + 3] = (Byte)(unsignedValue >> 8);
            packet[offset + 4] = (Byte)unsignedValue;
            return offset + 5;
        }

        public static UInt32 ParseDefiniteLength(Byte[] buffer, ref UInt32 offset)
        {
            Byte octetLength = buffer[offset++];

            if ((octetLength & 0x80) != 0x80) return octetLength;

            octetLength = (Byte)(octetLength & 0x7F);
            
            UInt32 value;
            switch (octetLength)
            {
                case 0: throw new FormatException("Expected the ASN.1 length to be definite but it is indefinite (meaning the data is probably ended by EOC");
                case 1:
                    value = buffer[offset];
                    break;
                case 2:
                    value = (UInt32)(buffer[offset] <<  8 | buffer[offset + 1]);
                    break;
                case 3:
                    value = (UInt32)(buffer[offset] << 16 | buffer[offset + 1] << 8 | buffer[offset + 2]);
                    break;
                case 4:
                    value = (UInt32)(buffer[offset] << 24 | buffer[offset + 1] << 16 | buffer[offset + 2] << 8 | buffer[offset]);
                    break;
                default:
                    throw new FormatException(String.Format("ASN.1 does not support lengths with {0} octets", octetLength));
            }

            offset += octetLength;
            return value;
        }

        public static Byte ParseIntegerToByte(Byte[] buffer, ref UInt32 lengthOffset)
        {
            UInt32 octetLength = ParseDefiniteLength(buffer, ref lengthOffset);

            if (octetLength != 1) throw new InvalidOperationException(String.Format(
                 "Expected Integer to have 1 octet but has {0}", octetLength));
            Byte value = buffer[lengthOffset];
            lengthOffset++;
            return value;
        }
        public static Int32 ParseIntegerToInt32(Byte[] buffer, ref UInt32 lengthOffset)
        {
            UInt32 octetLength = ParseDefiniteLength(buffer, ref lengthOffset);

            Int32 value;

            switch (octetLength)
            {
                case 0: throw new FormatException("ASN.1 octet length of the Integer type cannot be 0");
                case 1:
                    value = buffer[lengthOffset];
                    break;
                case 2:
                    value = ByteArray.BigEndianReadInt16(buffer, lengthOffset);
                    break;
                case 3:
                    value = ByteArray.BigEndianReadInt24(buffer, lengthOffset);
                    break;
                case 4:
                    value = ByteArray.BigEndianReadInt32(buffer, lengthOffset);
                    break;
                default:
                    throw new InvalidOperationException(String.Format("This ASN.1 Integer value is {0} octets but this method only supports up to 4", octetLength));
            }
            lengthOffset += octetLength;
            return value;
        }




        public static Byte[] ParseValue(String typeString, String valueString)
        {
            if (typeString.Equals("i") || typeString.Equals("int") || typeString.Equals("integer"))
            {
                return ParseInteger(valueString);
            }
            throw new NotImplementedException(String.Format("Asn1 Type '{0}' not implemented", typeString));
        }
        public static Byte[] ParseInteger(String valueString)
        {
            Int64 value = Int64.Parse(valueString);
            if (value >= 0)
            {
                if (value <= 0xFF)
                {
                    return new Byte[] {
                        Asn1.TYPE_INTEGER, 1,
                        (Byte)value,
                    };
                }
                if (value <= 0xFFFF)
                {
                    return new Byte[] {
                        Asn1.TYPE_INTEGER, 2,
                        (Byte)(value >> 8),
                        (Byte)value,
                    };
                }
                if (value <= 0xFFFFFF)
                {
                    return new Byte[] {
                        Asn1.TYPE_INTEGER, 3,
                        (Byte)(value >> 16),
                        (Byte)(value >> 8),
                        (Byte)value,
                    };
                }
                if (value <= 0xFFFFFF)
                {
                    return new Byte[] {
                        Asn1.TYPE_INTEGER, 4,
                        (Byte)(value >> 32),
                        (Byte)(value >> 16),
                        (Byte)(value >> 8),
                        (Byte)value,
                    };
                }
            }

            throw new NotImplementedException("Negative numbers not implemented");
        }
        public static UInt32 EncodingString(Byte[] packet, UInt32 offset, String value)
        {
            Byte valueLength = (Byte)value.Length;
            packet[offset++] = TYPE_OCTET_STRING; // String Type
            packet[offset++] = valueLength;
            for (Byte i = 0; i < valueLength; i++)
            {
                packet[offset++] = (Byte)value[i];
            }
            return offset;
        }
    }
}
