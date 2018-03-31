More
========================================================
This repository contains commonly useful C# libraries.

How to Build
========================================================
* Build Metabuild\BootstrapMetabuild.sln
Open the solution file to build or build from the command line:
```
msbuild Metabuild\BootstrapMetabuild.sln
```

* Generate solution/project files:
```
Metabuild\bin\Metabuild.exe generate VisualStudio2008 More.metaproj
```
> NOTE: use --wince to generate a build configuration for windows CE

* Build TemplateProcessor
Open `Generated.VisualStudio2008\More.sln` and build the `TemplateProcessor` project, or just build from the command line:
```
msbuild Generated.VisualStudio2008\TemplateProcessor.csproj
```

* Generate Code
```
Generated.VisualStudio2008\bin\Debug\TemplateProcessor.exe CommonTemplates\json.template Utf8\Json\Json.generated.cs (StringType)=Byte[] (StringTypeVarName)=ByteArray (StringTypeSafety)=
# TODO: at some point we should support Byte* instead of Byte[]
# Generated.VisualStudio2008\bin\Debug\TemplateProcessor.exe CommonTemplates\json.template Utf8\Json\Json.generated.cs (StringType)=Byte* (StringTypeVarName)=BytePtr (StringTypeSafety)=unsafe
```

* Open `Generated.VisualStudio2008\More.sln`

Breakdown of Libraries
========================================================
### More.dll
A merged assembly of all the other assemblies.

### More.Core.dll
Contains code used commonly througout all of the More libraries.

* Helpers to convert between stopwatch ticks and other time units
* Buffer wrappers that support size extension
* String extension methods (Peel, SubstringEquals)
* Classes for representing slices of arrays
* Extension methods for byte arrays that enable some common serialization
* The LineParser class that can turn blocks of bytes into strings line by line
* The EncodingEx class that contains extensions to encoding functionality
* New Writer interfaces that support new functionality
    - A BufferedFileWriter that implements the new writer interfaces

### More.Net.dll
General networking libraries.

* Some useful socket extensions
  * Try/Catch wrappers around some functions
  * Function to send a file
  * Function to read up to a given size
* SOCKS and HTTP proxy clients
* The SelectServer class which provides a callback eventing interface for sockets

### More.Net.Http.dll
An HTTP client and some Http helper methods

### More.Net.Dns.dll

* Some useful DNS resolution methods
* The StringEndPoint class.  Useful if an application has a hostname represented as a string that could be used in various networking libraries.

### More.Sos.dll
Sos (Simple Object Serialization) used to serialize any .NET object into a simple format that can be read by humans and deserialized. The one restriction is that the object cannot contain circular references.

### More.Reflection.dll
Facilitates serialization of classes via reflection.

### More.ByteString.dll

* The ByteString static class with methods for manipulating byte arrays as strings.
* The ITextBuilder interface with an implementation for String and ByteString.

### More.Utf8.dll
Libraries to support and manipulate utf8 encoded strings.  The String class in .NET is encoded as UTF-16 (kinda, it usese 16 bits for characters but doesn't fully support UTF-16).  This library provides code to deal with strings actually encoded in UTF-8.  Under the hood, they will be stored in byte pointers and byte arrays.  This is especially useful for network applications, where network data is sent and received as bytes arrays.  This library allows .NET network applications to keep their network data encoded as UTF-8 without having to copy data every time they want to start using the data with any useful .NET libraries.

### More.Utf8.Json.dll
A JSON parser that works with utf8 strings implemented in More.Utf8.dll.

### More.Miscellaneous.dll
Libraries that may not fit into a category or just haven't been placed into one yet.

* Serialization/Deserialization via reflection
* ASN.1 Encoding/Decoding
* AnsiEscape Decoding
* S BitArray class that performs better than the Microsoft Common Framework BitArray
* Extension methods for big/little endian seralization of numbers.
* Command line parsing
* A class for “smarter” type searching (Used by the NPC library to find types exposed by functions from a remote NPC server)
* Some Enumerator classes for empty lists and single object lists.
* Various Extension methods such as
  * Hex Parsing and Generation from binary data
  * Some extra string methods
  * Some Garbage Collector extension methods for counting GarbageCollection runs
  * A StringLiteral Parser
  * CamelCase to UnderscoreCase Conversion
  * List to String methods
  * Dictionary Extensions to create strings or help getting values
  * Conversion of DateTime to Unix Time
  * TextWriter extensions to prefix a line with n spaces
  * Stream extensions to read the entire stream
  * FileExtensions to read the entire file or write a string to a file
* A JSON parser
* An Ini-format parser
* An LFD parser/reader (another file format)
* A line parser that can be used by readers who aren’t guaranteed to receive data on line boundaries
* Various classes to enhance the WindowsCE Framework to function like the regular framework
* Some methods to help with platform specific file paths such as: IsValidUnixFileName, LocalCombine, LocalPathDiff, LocalToUnixPath, UnixToLocalPath
* A convenient method to run an external process
* A method to open/roll a rolling log
* A set of classes to run servers using select
* Some classes to tunnel sockets and streams to each other
* A SHA1 hasher
* Some SNMP helper methods
* A SortedList implementation (different than the Microsoft one)
* SOS serialization/deserialization methods (SOS = “Simple Object Serialization”)
* A StringBuilderWriter
* Some Thread helper methods
* A special type of dictionary called UniqueIndexObjectDictionary.
* A class to facilitate WaitActions (actions that should be executed later)
