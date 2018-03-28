using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More.BinaryFormat
{
    /*
    enum PacketSizeType
    {
        SizeIncludedInPacket,
        SizeOutsidePacket,
        SizeFromDelimiter,
    }

    public abstract class PacketDefinition
    {
        public readonly String name;








    }
    */

    //
    // PacketDescriptionLanguage
    // <definitions> ::= <definition> <definition> ...
    //
    // <definition> ::= <enum-definition> | <data-block-definition>
    //
    // <enum-definition> ::= "enum" [ <unsigned-integer> ] <name> '{' <enum-values '}'
    // <enum-values> ::= 
    //
    //

    //
    // DataBlock Definition
    // A "data block" is a sequence of bytes that represent data.
    // A "data block definition" describes how to serialize a data block.
    // A data block definition must specify the following:
    //    1. How the size of the block is determined
    //         a. Is the size incldued in the data block itself? Will the size be specified outside the data block by something else? If it is in the data block, where is it?
    //         
    // A data block definition can contain the following
    //    1. bit (Since a bit takes up less than a byte, its values will likely not be in the same place as the definition...it will be placed in the first available bit)
    //         - Example: bit mybit;
    //    2. IntegerTypes: sbyte,byte,short,ushort,int,uint,long,ulong
    //         - Example: int myinteger;
    //    4. Enum Definition (An unsigned integer type and set of strings and values)
    //         - Format: enum [<unsigned-integer-type>] <name> { <value-name> [= <value>]  , ...}
    //         - Example: enum DaysOfWeek {Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday}
    //         - If the usnigned integer typs is not specified there, it will use the smallest unsigned custom integer type it can
    //         - The enum definition can be referenced outside the data block by prefixing the enum name with the containing data block names,
    //           i.e. MyDataBlock { enum MyEnum...}  (can be referenced by "MyDataBlock.MyEnum")
    //    5. Floating point types: float, double
    //    6. Strings: ascii, utf8, utf16,utf32
    //         - a string can either be dilimited or have a length
    //             - ascii mystring; (delimited by null)
    //             - ascii[byte] name; (a byte length ascii string)
    //    6. Arrays
    //         - 
    //         - Format: <type>[<unsigned-integer-type> | <length>] <name>;
    //         - Example: byte[20] myhash;
    //         - Example: byte[ushort] longByteArray;
    //         - Example: <SomeType>[byte] anArray;
    //         - Example: <SomeType>[ushort] anArray;
    //         - Example: <SomeType>[uint] anArray;
    //         - Example: <SomeType>[ulong] anArray;
    //         - There may be cases where the array length can be compressed for example, you can include 2 extra bits somewhere that can represent how many bytes the length is
    //              00 = byte   (1 byte)
    //              01 = ushort (2 bytes)
    //              10 = uint   (4 bytes)
    //              11 = ulong  (8 bytes)
    //         - Other types of lengths?
    //           A good format for an array length will have a maximum type so that a language can process the command and assure that the type it uses will always be able to hold the value
    //         - Maybe a delimited array would be a good idea?
    //
    //    7. SubObjects
    //         - Example: [byte] people { ascii name; byte age; }
    //
    //
    // Special Length Types (Given n as the bit barrioer)
    //
    // 0   0   0   0  ...  0   0 = 0
    // 0   1   2   3  ... n-2 n-1
    // 
    // 0   0   0   0  ...  0   1 = 1
    // 0   1   2   3  ... n-2 n-1
    //
    // 0   0   0   0  ...  1   0 = 2
    // 0   1   2   3  ... n-2 n-1
    //
    // 1   1   1   1  ...  1   0 = 2
    // 0   1   2   3  ... n-2 n-1
    //
    // n = 0 (Should probably never do this)
    // 0    = 0
    // 10   = 1
    // 110  = 2
    // 1110 = 3
    // ...
    //
    // n = 1
    // 00     = 0
    // 01     = 1
    // 10     = 2
    // 1100   = 3
    // 1101   = 4
    // 1110   = 5
    // 111100 = 6
    // 111101 = 7
    // 111110 = 8
    //
    // n = 2
    // 000      = 0
    // 001      = 1
    // 010      = 2
    // 011      = 3
    // 100      = 4
    // 101      = 5
    // 110      = 6
    // 111000   = 7
    // 111000   = 8
    // 111001   = 9
    // 111010   = 10
    // 111011   = 11
    // 111100   = 12
    // 111101   = 13
    // 111110   = 14
    //
    // b = f(n,v)
    // b is the number of bits needed for n and value v
    //
    // f(0,0) = 1
    // f(0,1) = 2
    // f(0,2) = 3
    // f(0,3) = 4
    //
    // f(1,0) = 2
    // f(1,1) = 2
    // f(1,2) = 2
    // f(1,3) = 4
    // f(1,4) = 4
    // f(1,5) = 4
    // f(1,6) = 6
    // f(1,7) = 6
    // f(1,8) = 6
    //
    //
    // f(2, 0) = 3
    // f(2, 1) = 3
    // f(2, 2) = 3
    // f(2, 3) = 3
    // f(2, 4) = 3
    // f(2, 5) = 3
    // f(2, 6) = 3
    // f(2, 7) = 6
    // f(2, 8) = 6
    // f(2, 9) = 6
    // f(2,10) = 6
    // f(2,11) = 6
    // f(2,12) = 6
    // f(2,13) = 6
    //
    // b = (n + 1) * (floor( v / (2^(n+1) - 1) ) + 1)


    //
    //
    // Example Data Blocks
    // Person
    // {
    //     enum EyeColor {Blue,Brown,Green};
    //
    //     byte age;
    //     EyeColor eyeColor;
    // }
    //

    //
    // A Data Block Definition defines how the data block will be access by code.
    // However, this text does not say how the data block will be encoded.
    //
    // CtpRequest {
    //   bit keepAlive;
    //   ascii host;
    //   ascii client;
    //   [byte] resources {
    //     ascii resourceName
    //   }    //   
    //   [byte] cachedResources {
    //     byte[HASH_LENGTH] hash;
    //     ascii resourceName;
    //   }
    //   [byte] cookies {
    //     byte cookieID
    //     byte[byte] cookie
    //   }
    // }
    // CtpResponse {
    //    bit events;
    //    enum Encoding {none, deflate, gzip} encoding;
    //    [byte] setCookies {
    //       byte cookieID
    //       ulong expireDateTime
    //       byte[byte] cookie
    //    }
    //    [byte] resources {
    //       bit cacheable
    //       byte[uint] content
    //    }
    //    [byte] cachedResources {
    //       option byte[uint] content
    //    }
    // }
    //











}
