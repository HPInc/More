// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Diagnostics;

namespace More
{
    public class Encoder
    {
        public static unsafe readonly Encoder Ascii = new Encoder(
            More.Ascii.GetCharEncodeLength, More.Ascii.GetEncodeLength,
            More.Ascii.EncodeChar, More.Ascii.Encode, More.Ascii.EncodeUnsafe);
        public static unsafe readonly Encoder Utf8 = new Encoder(
            More.Utf8.GetCharEncodeLength, More.Utf8.GetEncodeLength,
            More.Utf8.EncodeChar, More.Utf8.Encode, More.Utf8.EncodeUnsafe);

        public delegate Byte GetCharEncodeLengthDelegate(Char c);
        public delegate UInt32 GetEncodeLengthDelegate(String str);
        public delegate UInt32 EncodeCharDelegate(Char c, Byte[] buffer, UInt32 offset);
        public delegate UInt32 EncodeDelegate(String str, Byte[] buffer, UInt32 offset);
        public unsafe delegate Byte* UnsafeEncodeDelegate(String str, Byte* buffer);

        public readonly GetCharEncodeLengthDelegate GetCharEncodeLength;
        public readonly GetEncodeLengthDelegate GetEncodeLength;
        public readonly EncodeCharDelegate EncodeChar;
        public readonly EncodeDelegate Encode;
        public readonly UnsafeEncodeDelegate EncodeUnsafe;
        private Encoder(
            GetCharEncodeLengthDelegate GetCharEncodeLength,
            GetEncodeLengthDelegate GetEncodeLength,
            EncodeCharDelegate EncodeChar,
            EncodeDelegate Encode,
            UnsafeEncodeDelegate EncodeUnsafe)
        {
            this.GetCharEncodeLength = GetCharEncodeLength;
            this.GetEncodeLength = GetEncodeLength;
            this.EncodeChar = EncodeChar;
            this.Encode = Encode;
            this.EncodeUnsafe = EncodeUnsafe;
        }
    }
}