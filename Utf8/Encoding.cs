// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Diagnostics;

namespace More
{
    public class Encoder
    {
        public static readonly Encoder Ascii = new Encoder(
            More.Ascii.GetCharEncodeLength, More.Ascii.GetEncodeLength,
            More.Ascii.EncodeChar, More.Ascii.Encode);
        public static readonly Encoder Utf8 = new Encoder(
            More.Utf8.GetCharEncodeLength, More.Utf8.GetEncodeLength,
            More.Utf8.EncodeChar, More.Utf8.Encode);

        public delegate Byte GetCharEncodeLengthDelegate(Char c);
        public delegate UInt32 GetEncodeLengthDelegate(String str);
        public delegate UInt32 EncodeCharDelegate(Char c, Byte[] buffer, UInt32 offset);
        public delegate void EncodeDelegate(String str, Byte[] buffer, UInt32 offset);

        public readonly GetCharEncodeLengthDelegate GetCharEncodeLength;
        public readonly GetEncodeLengthDelegate GetEncodeLength;
        public readonly EncodeCharDelegate EncodeChar;
        public readonly EncodeDelegate Encode;
        private Encoder(
            GetCharEncodeLengthDelegate GetCharEncodeLength,
            GetEncodeLengthDelegate GetEncodeLength,
            EncodeCharDelegate EncodeChar,
            EncodeDelegate Encode)
        {
            this.GetCharEncodeLength = GetCharEncodeLength;
            this.GetEncodeLength = GetEncodeLength;
            this.EncodeChar = EncodeChar;
            this.Encode = Encode;
        }
    }
}