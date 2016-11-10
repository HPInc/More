// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;

namespace More
{
    //
    // Pdu = "Protocol Data Unit"
    // 
    //
    public enum SnmpPduType
    {
        Get         = 0,
        GetNext     = 1,
        GetResponse = 2,
        Set         = 3,
        GetBulk     = 5,
        Inform      = 6,
        Trap        = 7,
        Report      = 8,
    }
    public enum SnmpErrorStatus
    {
        NoError      = 0,
        TooBig       = 1,
        NoSuchName   = 2,
        BadValue     = 3,
        ReadOnly     = 4,
        GenericError = 5,
    }
    public static class Snmp
    {
        public const String PublicCommunity = "public";
        /*
        public const Byte PDU_TYPE_GET      = 0;
        public const Byte PDU_TYPE_GET_NEXT = 1;
        public const Byte PDU_TYPE_RESPONSE = 2;
        public const Byte PDU_TYPE_SET      = 3;
        public const Byte PDU_TYPE_GET_BULK = 5;
        public const Byte PDU_TYPE_INFORM   = 6;
        public const Byte PDU_TYPE_TRAP     = 7;
        public const Byte PDU_TYPE_REPORT   = 8;
        */
        static Int32 lastID = -1;
        public static Int32 NextID()
        {
            lock(typeof(Snmp))
            {
                if (lastID == -1)
                {
                    lastID = new Random().Next(); // Initialize id with random value
                }
                else if(lastID == Int32.MaxValue)
                {
                    lastID = 0; // Wrap id back to 0
                }
                else
                {
                    lastID++;
                }

                return lastID;
            }
        }
        
        //
        // Oid Methods
        //
        public static void SerializeOidNumber(UInt32 id, IList<Byte> oidBytes)
        {
            if (id < 0x80)
            {
                oidBytes.Add((Byte)id);
            }
            else if (id < 0x4000)
            {
                oidBytes.Add((Byte)(0x80 | (id >> 7)));
                oidBytes.Add((Byte)(id & 0x7F));
            }
            else if (id < 0x200000)
            {
                oidBytes.Add((Byte)(0x80 | (id >> 14)));
                oidBytes.Add((Byte)(0x80 | (id >>  7)));
                oidBytes.Add((Byte)(id & 0x7F));
            }
            else if (id < 0x10000000)
            {
                oidBytes.Add((Byte)(0x80 | (id >> 21)));
                oidBytes.Add((Byte)(0x80 | (id >> 14)));
                oidBytes.Add((Byte)(0x80 | (id >>  7)));
                oidBytes.Add((Byte)(id & 0x7F));
            }
            else
            {
                oidBytes.Add((Byte)(0x80 | (id >> 28)));
                oidBytes.Add((Byte)(0x80 | (id >> 21)));
                oidBytes.Add((Byte)(0x80 | (id >> 14)));
                oidBytes.Add((Byte)(0x80 | (id >>  7)));
                oidBytes.Add((Byte)(id & 0x7F));
            }
        }
        public static UInt32 ParseUInt32ReturnOffset(String str, Int32 offset, out Int32 newOffset)
        {
            UInt32 value = (UInt32)(str[offset] - '0');
            while (true)
            {
                offset++;
                if (offset >= str.Length) { newOffset = offset; return value; }
                Char c = str[offset];
                if (c < '0' || c > '9') { newOffset = offset; return value; }
                value *= 10;
                value += (UInt32)(str[offset] - '0');
            }
        }
        public static void ParseOid(String oid, IList<Byte> oidBytes)
        {
            Char c = oid[0];
            if (c < '0' || c > '9') throw new FormatException(String.Format("An oid must begin with '0'-'9' but began with '{0}'", c));

            //
            // Get the first digit
            //
            Int32 oidOffset;
            UInt32 firstID = ParseUInt32ReturnOffset(oid, 0, out oidOffset);

            if(oidOffset >= oid.Length || oid[oidOffset] != '.')
                throw new FormatException(String.Format("Oid '{0}' is missing the '.' character", oid));
            oidOffset++;
            if (oidOffset >= oid.Length) throw new FormatException("An oid cannot end with the '.' character");

            c = oid[oidOffset];
            if (c < '0' || c > '9') throw new FormatException(String.Format("Expected '0'-'9' after '.', but found '{0}'", c));

            UInt32 secondID = ParseUInt32ReturnOffset(oid, oidOffset, out oidOffset);
            oidBytes.Add((Byte)(40 * firstID + secondID));

            while (true)
            {
                if (oidOffset >= oid.Length) break;
                c = oid[oidOffset];
                if (c != '.') throw new FormatException(String.Format("Expected '.' to follow a number in an oid but found '{0}'", c));

                oidOffset++;
                if (oidOffset >= oid.Length) throw new FormatException("An oid cannot end with the '.' character");

                c = oid[oidOffset];
                if (c < '0' || c > '9') throw new FormatException(String.Format("Expected '0'-'9' after '.', but found '{0}'", c));

                UInt32 nextID = ParseUInt32ReturnOffset(oid, oidOffset, out oidOffset);
                SerializeOidNumber(nextID, oidBytes);
            }
        }

        public static UInt32 SerializeSnmp(Byte[] packet, UInt32 offset, String community, Byte pduAsnType,
            Int32 requestID, IList<Byte> oid, IList<Byte> asnValue)
        {
            //
            // Calculate lengths
            //
            UInt32 varbindLength =
                1U + Asn1.DefiniteLengthOctetCount((UInt32)oid.Count) + (UInt32)oid.Count + // Oid
                (UInt32)asnValue.Count;                                                        // Value

            UInt32 varbindListLength =
                1U + Asn1.DefiniteLengthOctetCount(varbindLength) + varbindLength;             // Varbind

            UInt32 pduLength =
                2U + Asn1.IntegerOctetCount(requestID) +                                       // Request ID
                3U +                                                                           // Error Status
                3U +                                                                           // Error Index
                1U + Asn1.DefiniteLengthOctetCount(varbindListLength) + varbindListLength;     // VarbindList

            UInt32 snmpSequenceLength =
                3U +                                                                           // Version                      
                1U + Asn1.IntegerOctetCount(community.Length) + (UInt32)community.Length +     // Comunity
                1U + Asn1.DefiniteLengthOctetCount(pduLength) + pduLength;                     // Pdu

            //
            // TODO: check that packet is large enough
            //

            //
            // SNMP Message Type
            //
            packet[offset++] = Asn1.TYPE_SEQUENCE;
            offset = packet.SetInteger(offset, snmpSequenceLength);

            //
            // Version
            //
            packet[offset++] = Asn1.TYPE_INTEGER;
            packet[offset++] = 1;
            packet[offset++] = 0; // Version 1

            //
            // Community
            //
            packet[offset++] = Asn1.TYPE_OCTET_STRING;
            offset = packet.SetInteger(offset, (UInt32)community.Length);
            for (int i = 0; i < community.Length; i++)
            {
                packet[offset++] = (Byte)community[i];
            }

            //
            // PDU Type
            //
            packet[offset++] = pduAsnType;
            offset = packet.SetInteger(offset, pduLength);

            //
            // Request ID
            //
            packet[offset++] = Asn1.TYPE_INTEGER;
            Byte requestIDOctetCount = Asn1.IntegerOctetCount(requestID);
            packet[offset++] = requestIDOctetCount;
            offset = Asn1.SetInteger(packet, offset, requestID);

            //
            // Error Status
            //
            packet[offset++] = Asn1.TYPE_INTEGER;
            packet[offset++] = 1;
            packet[offset++] = 0;

            //
            // Error Index
            //
            packet[offset++] = Asn1.TYPE_INTEGER;
            packet[offset++] = 1;
            packet[offset++] = 0;

            //
            // Variable Bindings
            //
            packet[offset++] = Asn1.TYPE_SEQUENCE;
            offset = packet.SetInteger(offset, varbindListLength);

            //
            // Varbind
            //
            packet[offset++] = Asn1.TYPE_SEQUENCE;
            offset = packet.SetInteger(offset, varbindLength);

            // OID
            packet[offset++] = Asn1.TYPE_OBJECT_ID;
            offset = packet.SetInteger(offset, (UInt32)oid.Count);
            for (Int32 i = 0; i < oid.Count; i++)
            {
                packet[offset++] = oid[i];
            }

            // Value
            for (int i = 0; i < asnValue.Count; i++)
            {
                packet[offset++] = asnValue[i];
            }

            return offset;
        }



        public static UInt32 MaximumBufferForGet(UInt32 communityLength, UInt32 oidLength)
        {
            return
                1U + 5U +                       // SNMP Sequence
                3U +                            // Version
                1U +                            // Community
                Asn1.DefiniteLengthOctetCount(communityLength) +
                communityLength +
                1U + 5U +                       // PDU Type
                2U + 5U +                       // Request ID
                3U +                            // Error Status
                3U +                            // Error Index
                1U + 5U +                       // Varbind List
                1U + 5U +                       // Varbind
                1U +                            // Oid
                Asn1.DefiniteLengthOctetCount(oidLength) +
                oidLength +
                2U;                             // Null value
        }
        public static UInt32 SerializeGet(Byte[] packet, UInt32 offset, String community, Int32 requestID, IList<Byte> oid)
        {
            return SerializeSnmp(packet, offset, community,
                Asn1.TYPE_GET_REQUEST_PDU, requestID, oid, Asn1.Null);
        }

        //
        // The Maximum Buffer assumes every unknown length requires 5 octets
        //
        public static UInt32 MaximumBufferForSet(UInt32 communityLength, UInt32 oidLength, UInt32 valueAsnLength)
        {
            return
                1U + 5U +                       // SNMP Sequence
                3U +                            // Version
                1U +                            // Community
                Asn1.DefiniteLengthOctetCount(communityLength) +
                communityLength +     
                1U + 5U +                       // PDU Type
                2U + 5U +                       // Request ID
                3U +                            // Error Status
                3U +                            // Error Index
                1U + 5U +                       // Varbind List
                1U + 5U +                       // Varbind
                1U +                            // Oid
                Asn1.DefiniteLengthOctetCount(oidLength) +
                oidLength +
                valueAsnLength;                 // Value
        }
        public static UInt32 SerializeSet(Byte[] packet, UInt32 offset, String community, Int32 requestID, IList<Byte> oid, IList<Byte> asnValue)
        {
            return SerializeSnmp(packet, offset, community,
                Asn1.TYPE_SET_REQUEST_PDU, requestID, oid, asnValue);
        }
    }


    public static class SnmpDeserialization
    {
        public static Int32 ParsePduInt32(Byte[] packet, UInt32 offset, UInt32 length)
        {
            UInt32 initialOffset = offset;

            //
            // Varbind
            //
            if (packet[offset] != Asn1.TYPE_SEQUENCE) throw new FormatException(String.Format(
                 "Expected varbind to be of type Sequence but is {0}", Asn1.GetTypeOrNumberString(packet[offset])));
            offset++;

            UInt32 varbindLength = Asn1.ParseDefiniteLength(packet, ref offset);
            if (packet.Length - offset < varbindLength) throw new FormatException(String.Format(
                 "Failed to parse snmp packet because it was truncated from {0} bytes to {1}",
                 varbindLength + (offset - initialOffset), packet.Length - initialOffset));

            //
            // Skip the bound variable
            //
            offset++; // Skip the type
            UInt32 boundVariableLength = Asn1.ParseDefiniteLength(packet, ref offset);
            offset += boundVariableLength;

            //
            // Parse the integer
            //
            if (packet[offset] != Asn1.TYPE_INTEGER) throw new FormatException(String.Format(
                 "Expected value to be of type Integer but is {0}", Asn1.GetTypeOrNumberString(packet[offset])));
            offset++;
            return Asn1.ParseIntegerToInt32(packet, ref offset);
        }
    }


    public class SnmpPacket
    {
        public Byte version;
        //public SnmpPdu pdu;
        public SnmpErrorStatus errorStatus;
        public Int32 errorIndex;
        public Int32 requestID;
        public Byte[] packetBytes;

        public SnmpPacket()
        {
        }


        // returns offset of varbind list
        public UInt32 Deserialize(Byte[] packet, UInt32 offset)
        {
            this.packetBytes = packet;

            UInt32 initialOffset = offset;

            //
            // SNMP Message Type
            //
            if (packet[offset] != Asn1.TYPE_SEQUENCE) throw new FormatException(String.Format(
                 "Expected snmp packet to be of type Sequence but is {0}", Asn1.GetTypeOrNumberString(packet[offset])));
            offset++;

            UInt32 sequenceLength = Asn1.ParseDefiniteLength(packet, ref offset);
            UInt32 sequenceOffsetLimit = offset + sequenceLength;
            if (packet.Length - offset < sequenceLength) throw new FormatException(String.Format(
                 "Failed to parse snmp packet because it was truncated from {0} bytes to {1}",
                 sequenceLength + (offset - initialOffset), packet.Length - initialOffset));

            //
            // Version
            //
            if (packet[offset] != Asn1.TYPE_INTEGER) throw new FormatException(String.Format(
                 "Expected snmp version to be of type Integer but is {0}", Asn1.GetTypeOrNumberString(packet[offset])));
            offset++;
            this.version = Asn1.ParseIntegerToByte(packet, ref offset);
            if (this.version != 0)
            {
                throw new NotImplementedException(String.Format("Snmp version {0} (binary code {1}) is not implemented yet",
                    this.version + 1, this.version));
            }

            //
            // Community
            //
            if (packet[offset] != Asn1.TYPE_OCTET_STRING) throw new FormatException(String.Format(
                 "Expected snmp community to be of type OctetString but is '{0}'", Asn1.GetTypeOrNumberString(packet[offset])));
            offset++;
            Int32 communityLength = (Byte)Asn1.ParseDefiniteLength(packet, ref offset);
            if(communityLength < 0) throw new FormatException(String.Format("Expected community octet string length to be positive but is {0}", communityLength));
            offset += (UInt32)communityLength;
            
            //
            // PDU Type
            //
            Byte pduTypeByte = packet[offset];
            offset++;

            if (pduTypeByte < 0xA0) throw new FormatException(String.Format("Expected SNMP pdu type to be >= 0xA0 but is {0:2X}", pduTypeByte));

            //
            // Use similar logic to parse similar types
            // Get, GetNext, GetResponse, Set
            if (pduTypeByte <= 0xA3)
            {
                SnmpPduType pduType = (SnmpPduType)(pduTypeByte - 0xA0);

                UInt32 pduLength = Asn1.ParseDefiniteLength(packet, ref offset);
                if (offset + pduLength != sequenceOffsetLimit) throw new FormatException(String.Format(
                     "Snmp packet is malformed, the pdu which ends at {0} does not end at the same place as the sequence at {1}",
                     offset + pduLength, sequenceOffsetLimit));

                //
                // Request ID
                //
                if (packet[offset] != Asn1.TYPE_INTEGER) throw new FormatException(String.Format(
                     "Expected Snmp{0}Pdu to start with the Integer type but started with '{1}'", pduType, Asn1.GetTypeOrNumberString(packet[offset])));
                offset++;
                this.requestID = Asn1.ParseIntegerToInt32(packet, ref offset);

                //
                // Error Status
                //
                if (packet[offset] != Asn1.TYPE_INTEGER) throw new FormatException(String.Format(
                     "Expected Snmp{0}Pdu ErrorStatus to be an Integer type but is '{1}'", pduType, Asn1.GetTypeOrNumberString(packet[offset])));
                offset++;
                this.errorStatus = (SnmpErrorStatus)Asn1.ParseIntegerToInt32(packet, ref offset);

                //
                // Error Index
                //
                if (packet[offset] != Asn1.TYPE_INTEGER) throw new FormatException(String.Format(
                     "Expected Snmp{0}Pdu ErrorIndex to be an Integer type but is '{1}'", pduType, Asn1.GetTypeOrNumberString(packet[offset])));
                offset++;
                this.errorIndex = Asn1.ParseIntegerToInt32(packet, ref offset);

                //
                // Varbind List
                //
                if (packet[offset] != Asn1.TYPE_SEQUENCE) throw new FormatException(String.Format(
                     "Expected varbind list to be of type Sequence but is {0}", Asn1.GetTypeOrNumberString(packet[offset])));
                offset++;

                UInt32 varbindListLength = Asn1.ParseDefiniteLength(packet, ref offset);
                if (offset + varbindListLength != sequenceOffsetLimit) throw new FormatException(String.Format(
                     "Snmp packet is malformed, the varbind list which ends at {0} does not end at the same place as the sequence at {1}",
                     offset + varbindListLength, sequenceOffsetLimit));

                return offset; // return the offset to the varbind variables
            }
            else
            {
                throw new NotImplementedException(String.Format("Snmp pdu type {0} is not implemented yet", pduTypeByte));
            }
        }
    }
}
