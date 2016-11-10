// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;

namespace More
{
    public struct BetterBitArray : IEnumerable<Boolean>
    {
        public class Enumerator : IEnumerator<Boolean>
        {
            readonly BetterBitArray bitarray;
            UInt32 bitIndex;
            Boolean currentElement;

            public Enumerator(BetterBitArray bitarray)
            {
                this.bitarray = bitarray;
                this.bitIndex = UInt32.MaxValue;
            }
            public void Dispose()
            {
            }
            public void Reset()
            {
                this.bitIndex = UInt32.MaxValue;
            }
            public Boolean Current
            {
                get
                {
                    if (bitIndex == UInt32.MaxValue) throw new InvalidOperationException("You have not called MoveNext yet");
                    if (bitIndex >= bitarray.bitLength) throw new InvalidOperationException("This enumerator is finished");
                    return this.currentElement;
                }
            }
            Object System.Collections.IEnumerator.Current
            {
                get { throw new NotSupportedException("Generic Current method is not supported"); }
            }
            public Boolean MoveNext()
            {
                if (bitIndex >= bitarray.bitLength && bitIndex != UInt32.MaxValue)
                    throw new InvalidOperationException("This enumerator has already been finished");

                bitIndex++;
                if (bitIndex >= bitarray.bitLength) return false;

                this.currentElement = bitarray.Get(bitIndex);
                return true;
            }
        }

        public readonly UInt32 bitLength;
        readonly UInt32[] array;

        //
        // TODO: Maybe accept UInt64?
        //
        public BetterBitArray(UInt32 bitLength)
            : this(bitLength, false)
        {
        }
        public BetterBitArray(UInt32 bitLength, Boolean defaultValue)
        {
            this.bitLength = bitLength;

            // TODO: Fix this so it can accept UInt32.MaxValue
            this.array = new UInt32[(bitLength + 0x1f) / 0x20];

            if (defaultValue)
            {
                for (UInt32 i = 0; i < array.Length; i++)
                {
                    array[i] = 0xFFFFFFFF;
                }
            }
        }
        IEnumerator<Boolean> IEnumerable<Boolean>.GetEnumerator()
        {
            return new Enumerator(this);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException("Generic GetEnumerator is not supported");
        }
        public Boolean Get(UInt32 bitIndex)
        {
            if (bitIndex >= this.bitLength) throw new ArgumentOutOfRangeException("index",
                String.Format("The given bit index {0} cannot be >= to {1}", bitIndex, this.bitLength));

            return ((this.array[bitIndex / 0x20] & (1U << (Byte)(bitIndex % 0x20))) != 0);
        }
        public void Assert(UInt32 bitIndex)
        {
            if (bitIndex >= this.bitLength) throw new ArgumentOutOfRangeException("index",
                String.Format("The given bit index {0} cannot be >= to {1}", bitIndex, this.bitLength));

            this.array[bitIndex / 0x20] |= (1U << (Byte)(bitIndex % 0x20));
        }
        public void Deassert(UInt32 bitIndex)
        {
            if (bitIndex >= this.bitLength) throw new ArgumentOutOfRangeException("index",
                String.Format("The given bit index {0} cannot be >= to {1}", bitIndex, this.bitLength));

            this.array[bitIndex / 0x20] &= ~(1U << (Byte)(bitIndex % 0x20));
        }
        public void Flip(UInt32 bitIndex)
        {
            if (bitIndex >= this.bitLength) throw new ArgumentOutOfRangeException("index",
                String.Format("The given bit index {0} cannot be >= to {1}", bitIndex, this.bitLength));

            UInt32 arrayOffset = bitIndex / 0x20;
            UInt32 arrayValue = this.array[arrayOffset];
            UInt32 bit = 1U << (Byte)(bitIndex % 0x20);
            this.array[arrayOffset] = ((arrayValue & bit) != 0) ? arrayValue & ~bit : arrayValue | bit;
        }
        public void SetAll(Boolean asserted)
        {
            UInt32 arrayValue = asserted ? 0xFFFFFFFF : 0;

            UInt32 thisArrayLength = (UInt32)array.Length;
            for (UInt32 i = 0; i < thisArrayLength; i++)
            {
                array[i] = arrayValue;
            }
        }
        public void NotEquals()
        {
            UInt32 thisArrayLength = (UInt32)this.array.Length;
            for (int i = 0; i < thisArrayLength; i++)
            {
                this.array[i] = ~this.array[i];
            }
        }
        public void AndEquals(BetterBitArray other)
        {
            if (this.bitLength != other.bitLength) throw new ArgumentException(String.Format("Given bit array is {0} bits but this bit array is {1}", other.bitLength, this.bitLength));

            UInt32 thisArrayLength = (UInt32)this.array.Length;
            for (UInt32 i = 0; i < thisArrayLength; i++)
            {
                this.array[i] &= other.array[i];
            }
        }
        public void OrEquals(BetterBitArray other)
        {
            if (this.bitLength != other.bitLength) throw new ArgumentException(String.Format("Given bit array is {0} bits but this bit array is {1}", other.bitLength, this.bitLength));

            UInt32 thisArrayLength = (UInt32)this.array.Length;
            for (UInt32 i = 0; i < thisArrayLength; i++)
            {
                this.array[i] |= other.array[i];
            }
        }
        public void XorEquals(BetterBitArray other)
        {
            if (this.bitLength != other.bitLength) throw new ArgumentException(String.Format("Given bit array is {0} bits but this bit array is {1}", other.bitLength, this.bitLength));

            UInt32 thisArrayLength = (UInt32)this.array.Length;
            for (UInt32 i = 0; i < thisArrayLength; i++)
            {
                this.array[i] ^= other.array[i];
            }
        }
    }
    public static class Bits
    {
        public static Boolean GetBit(this Byte[] bitField, UInt32 bitFieldIndex)
        {
            UInt32 byteIndex = bitFieldIndex / 8;
            if (byteIndex >= bitField.Length) return false;
            byte bitFlag = (byte)(1 << (int)(bitFieldIndex % 8U));
            return (bitField[byteIndex] & bitFlag) != 0;
        }
        public static void SetBit(this Byte[] bitField, Int32 bitFieldIndex, Boolean on)
        {
            int byteIndex = bitFieldIndex / 8;

            if (byteIndex >= bitField.Length) throw new ArgumentOutOfRangeException("bitFieldIndex", String.Format(
                 "Cannot set bit {0} in bit field of length {1}", bitFieldIndex, bitField.Length));

            byte bitFlag = (byte)(1 << (bitFieldIndex % 8));

            if (on)
            {
                bitField[byteIndex] |= bitFlag;
            }
            else
            {
                bitField[byteIndex] &= ((byte)~bitFlag);
            }
        }
    }
    public class BitFieldBuilder
    {
        public readonly List<Byte> bytes;

        public BitFieldBuilder()
        {
            this.bytes = new List<Byte>();
        }
        public Byte[] ToBitField()
        {
            return bytes.ToArray();
        }
        public void Zero()
        {
            for (int i = 0; i < bytes.Count; i++)
            {
                bytes[i] = 0;
            }
        }
        public Boolean GetBit(Int32 bitFieldIndex)
        {
            int byteIndex = bitFieldIndex / 8;
            if (byteIndex >= bytes.Count) return false;
            byte bitFlag = (byte)(1 << (bitFieldIndex % 8));
            return (bytes[byteIndex] & bitFlag) != 0;
        }
        public void SetBit(Int32 bitFieldIndex, Boolean on)
        {
            int byteIndex = bitFieldIndex / 8;
            byte bitFlag = (byte)(1 << (bitFieldIndex % 8));

            // Extend the list to fit the byte flag
            while (bytes.Count <= byteIndex)
            {
                bytes.Add(0);
            }

            if (on)
            {
                bytes[byteIndex] |= bitFlag;
            }
            else
            {
                bytes[byteIndex] &= ((byte)~bitFlag);
            }
        }
    }
}
