// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;

namespace More
{
    public struct Sha1
    {
        public readonly UInt32 _0, _1, _2, _3, _4;
        public Sha1(UInt32 _0, UInt32 _1, UInt32 _2, UInt32 _3, UInt32 _4)
        {
            this._0 = _0;
            this._1 = _1;
            this._2 = _2;
            this._3 = _3;
            this._4 = _4;
        }
        /*
         * TODO: Maybe create a function to parse a hex string...maybe not?
        public Sha1(String hex)
        {

            bytes[offset++] = (Byte)(
                (hexString[hexStringOffset].HexValue() << 4) +
                 hexString[hexStringOffset + 1].HexValue());
            hexStringOffset += 2;


            hashBytes.ParseHex(0, hashString, hashStringOffset, HashByteLength * 2);

            hash = new UInt32[HashUInt32Length];
            hash[0] = hashBytes.BigEndianReadUInt32(0);
            hash[1] = hashBytes.BigEndianReadUInt32(4);
            hash[2] = hashBytes.BigEndianReadUInt32(8);
            hash[3] = hashBytes.BigEndianReadUInt32(12);
            hash[4] = hashBytes.BigEndianReadUInt32(16);
        }
        */
        public Boolean Equals(Sha1 other)
        {
            return
                this._0 == other._0 &&
                this._1 == other._1 &&
                this._2 == other._2 &&
                this._3 == other._3 &&
                this._4 == other._4;
        }
        public override String ToString()
        {
            return String.Format("{0:X8}{1:X8}{2:X8}{3:X8}{4:X8}", _0, _1, _2, _3, _4);
        }
        public String ToHex()
        {
            return ToString();
        }
        public static Sha1 ParseHex(String hexString, UInt32 offset)
        {
            return new Sha1(
                (Byte)((hexString[(int)offset + 0].HexDigitToValue() << 4) | hexString[(int)(offset + 1)].HexDigitToValue()),
                (Byte)((hexString[(int)offset + 2].HexDigitToValue() << 4) | hexString[(int)(offset + 3)].HexDigitToValue()),
                (Byte)((hexString[(int)offset + 4].HexDigitToValue() << 4) | hexString[(int)(offset + 5)].HexDigitToValue()),
                (Byte)((hexString[(int)offset + 6].HexDigitToValue() << 4) | hexString[(int)(offset + 7)].HexDigitToValue()),
                (Byte)((hexString[(int)offset + 8].HexDigitToValue() << 4) | hexString[(int)(offset + 9)].HexDigitToValue()));
        }
    }
    public class Sha1Builder
    {
        public const Int32 HashByteLength = 20;
        public const Int32 BlockByteLength = 64;

        const UInt32 InitialHash_0 = 0x67452301;
        const UInt32 InitialHash_1 = 0xEFCDAB89;
        const UInt32 InitialHash_2 = 0x98BADCFE;
        const UInt32 InitialHash_3 = 0x10325476;
        const UInt32 InitialHash_4 = 0xC3D2E1F0;

        const UInt32 K_0 = 0x5A827999;
        const UInt32 K_1 = 0x6ED9EBA1;
        const UInt32 K_2 = 0x8F1BBCDC;
        const UInt32 K_3 = 0xCA62C1D6;

        UInt32 hash_0, hash_1, hash_2, hash_3, hash_4;

        Byte[] block;
        Int32 blockIndex;
        UInt64 messageBitLength;

        public Sha1Builder()
        {
            hash_0 = InitialHash_0;
            hash_1 = InitialHash_1;
            hash_2 = InitialHash_2;
            hash_3 = InitialHash_3;
            hash_4 = InitialHash_4;
            this.block = new Byte[BlockByteLength];
        }
        public void Reset()
        {
            hash_0 = InitialHash_0;
            hash_1 = InitialHash_1;
            hash_2 = InitialHash_2;
            hash_3 = InitialHash_3;
            hash_4 = InitialHash_4;
            for (int i = 0; i < BlockByteLength; i++)
            {
                this.block[i] = 0;
            }
            this.blockIndex = 0;
            this.messageBitLength = 0;
        }
        public void Add(String str, Int32 offset, Int32 length)
        {
            throw new NotImplementedException();
        }
        public void Add(Byte[] bytes, Int32 offset, Int32 length)
        {
            while(length > 0)
            {
                Int32 blockBytesLeft = BlockByteLength - blockIndex;
                if (length < blockBytesLeft)
                {
                    Array.Copy(bytes, offset, block, blockIndex, length);
                    blockIndex += length;
                    messageBitLength += ((UInt64)length << 3); // length * 8
                    return;
                }

                Array.Copy(bytes, offset, block, blockIndex, blockBytesLeft);
                blockIndex = 0;
                messageBitLength += ((UInt64)blockBytesLeft << 3); // length * 8
                HashBlock();
                offset += blockBytesLeft;
                length -= blockBytesLeft;
            }
        }
        public Sha1 Finish(Boolean reuse)
        {
            Pad();
            block.BigEndianSetUInt64(56, messageBitLength);
            HashBlock();

            var saved = new Sha1(hash_0, hash_1, hash_2, hash_3, hash_4);
            if (reuse)
            {
                Reset();
            }
            else
            {
                this.block = null; // Throw away the memory.  Should cause an exception if the object is reused.
            }
            return saved;
        }
        void Pad()
        {
            block[blockIndex++] = 0x80;
            if (blockIndex > 56)
            {
                while (blockIndex < BlockByteLength) block[blockIndex++] = 0;
                blockIndex = 0;
                HashBlock();
            }
            while(blockIndex < 56) block[blockIndex++] = 0;
        }
        static UInt32 CircularShift(UInt32 value, Int32 shift)
        {
            return (value << shift) | (value >> (32 - shift));
        }
        void HashBlock()
        {
            Byte temp8;
            UInt32 temp32;

            UInt32[] W = new UInt32[80];

            // Initialize the first 16 words in array W
            for (Byte i = 0; i < 16; i++)
            {
                temp8 = (Byte)(i << 2);
                W[i] = (UInt32)(
                    (block[temp8   ] << 24) |
                    (block[temp8 + 1] << 16) |
                    (block[temp8 + 2] <<  8) |
                    (block[temp8 + 3]      ) );
            }

            // Initialize the rest of the words in array W
            for (Byte i = 16; i < 80; i++)
            {
                W[i] = CircularShift(W[i - 3] ^ W[i - 8] ^ W[i - 14] ^ W[i - 16], 1);
            }

            UInt32
                A = hash_0,
                B = hash_1,
                C = hash_2,
                D = hash_3,
                E = hash_4;

            for (int i = 0; i < 20; i++)
            {
                temp32 = CircularShift(A, 5) + ((B & C) | ((~B) & D)) + E + W[i] + K_0;
                E = D;
                D = C;
                C = CircularShift(B, 30);
                B = A;
                A = temp32;
            }
            for (int i = 20; i < 40; i++)
            {
                temp32 = CircularShift(A, 5) + (B ^ C ^ D) + E + W[i] + K_1;
                E = D;
                D = C;
                C = CircularShift(B, 30);
                B = A;
                A = temp32;
            }
            for (int i = 40; i < 60; i++)
            {
                temp32 = CircularShift(A, 5) + ((B & C) | (B & D) | (C & D)) + E + W[i] + K_2;
                E = D;
                D = C;
                C = CircularShift(B, 30);
                B = A;
                A = temp32;
            }
            for (int i = 60; i < 80; i++)
            {
                temp32 = CircularShift(A, 5) + (B ^ C ^ D) + E + W[i] + K_3;
                E = D;
                D = C;
                C = CircularShift(B, 30);
                B = A;
                A = temp32;
            }

            hash_0 += A;
            hash_1 += B;
            hash_2 += C;
            hash_3 += D;
            hash_4 += E;
        }
    }
}
