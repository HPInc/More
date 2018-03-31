#if WindowsCE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

using More;

namespace System
{
    public class WindowsCESafeNativeMethods
    {
        [DllImport("coredll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
            String lpFileName,
            UInt32 dwDesiredAccess,
            UInt32 dwShareMode,
            IntPtr SecurityAttributes,
            UInt32 dwCreationDisposition,
            UInt32 dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [DllImport("coredll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern Boolean CloseHandle(IntPtr hObject);

        [DllImport("coredll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern Boolean GetDiskFreeSpaceEx(String lpDirectoryName,
            out UInt64 lpFreeBytesAvailable,
            out UInt64 lpTotalNumberOfBytes,
            out UInt64 lpTotalNumberOfFreeBytes);
    }

    public static class CEMissingTypeExtensions
    {
        /*
        public static Type MakeArrayType(this Type elementtype)
        {
            
        }
        */

    }
    public static class MissingInCEArrayCopier
    {
        public static void Copy(Array srcArray, Array dstArray, Int64 length)
        {
            if (length > Int32.MaxValue) throw new InvalidOperationException(String.Format(
                "Platform Error: The Compact .NET framework does not support Array.Copy for Int64 values, this call will perform differently on the compact .NET framework"));

            Array.Copy(srcArray, dstArray, (Int32)length);
        }
        public static void Copy(Array srcArray, Int64 srcOffset, Array dstArray, Int64 dstOffset, Int64 length)
        {
            if (srcOffset > Int32.MaxValue || dstOffset > Int32.MaxValue || length > Int32.MaxValue) throw new InvalidOperationException(String.Format(
                "Platform Error: The Compact .NET framework does not support Array.Copy for Int64 values, this call will perform differently on the compact .NET framework"));

            Array.Copy(srcArray, (Int32)srcOffset, dstArray, (Int32)dstOffset, (Int32)length);
        }
    }
    public static class MissingInCEByteParser
    {
        public static Boolean TryParse(String str, out Byte value)
        {
            try
            {
                value = Byte.Parse(str);
                return true;
            }
            catch (FormatException)
            {
                value = 0;
                return false;
            }
            catch (OverflowException)
            {
                value = 0;
                return false;
            }
        }
        public static Boolean TryParse(String str, Globalization.NumberStyles style, IFormatProvider provider, out Byte value)
        {
            try
            {
                value = Byte.Parse(str, style, provider);
                return true;
            }
            catch (FormatException)
            {
                value = 0;
                return false;
            }
            catch (OverflowException)
            {
                value = 0;
                return false;
            }
        }
    }
    public static class MissingInCEUInt16Parser
    {
        public static Boolean TryParse(String str, out UInt16 value)
        {
            try
            {
                value = UInt16.Parse(str);
                return true;
            }
            catch (FormatException)
            {
                value = 0;
                return false;
            }
            catch (OverflowException)
            {
                value = 0;
                return false;
            }
        }
    }
    public static class MissingInCEEnumReflection
    {
        public static String[] GetNames(Type enumType)
        {
            List<String> names = new List<String>();
            foreach (FieldInfo fieldInfo in enumType.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                names.Add(fieldInfo.Name);
            }
            return names.ToArray();
        }
        public static Array GetValues(Type enumType)
        {
            ArrayBuilder builder = new ArrayBuilder(enumType);
            foreach (FieldInfo fieldInfo in enumType.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                builder.Add(fieldInfo.GetValue(null));
            }
            return builder.Build();
        }
    }
    public static class MissingInCEString
    {
        public static String ToLowerInvariant(this String str)
        {
            return str.ToLower(System.Globalization.CultureInfo.InvariantCulture);
        }
        public static String Remove(this String str, Int32 index)
        {
            return str.Substring(0, index);
        }
        public static String[] Split(this String str, Char[] chars, Int32 maxStrings)
        {
            String[] strings = str.Split(chars);
            if (strings == null) return strings;

            if (strings.Length <= maxStrings) return strings;

            String[] newStrings = new String[maxStrings];
            Array.Copy(strings, newStrings, maxStrings);
            return newStrings;
        }
    }
}
namespace System.Collections.Generic
{
    [Serializable]
    public class HashSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable//, ISerializable, IDeserializationCallback
    {
        private Dictionary<T, Boolean> map;
        public HashSet()
        {
            this.map = new Dictionary<T, bool>();
        }
        public HashSet(IEnumerable<T> collection)
        {
            this.map = new Dictionary<T, bool>();
        }
        public HashSet(IEqualityComparer<T> comparer)
        {
            this.map = new Dictionary<T, bool>();
        }
        public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            this.map = new Dictionary<T, bool>();
        }
        //protected HashSet(SerializationInfo info, StreamingContext context);

        //public IEqualityComparer<T> Comparer { get; }
        public int Count { get { return map.Count; } }
        //public bool Add(T item) { return map.Add(item, false); }
        public void Clear() { map.Clear(); }
        public bool Contains(T item) { return map.ContainsKey(item); }
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0, Count);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex, Count);
        }
        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex + count > array.Length) throw new ArgumentException();
            if (count == Count)
            {
                foreach (T key in map.Keys)
                {
                    array[arrayIndex++] = key;
                }
            }
            else if (count > 0 && count < Count)
            {
                int limit = arrayIndex + count;
                foreach (T key in map.Keys)
                {
                    array[arrayIndex++] = key;
                    if (arrayIndex >= limit)
                        return;
                }
                throw new InvalidOperationException();
            }
            else if (count == 0)
            {
                // do nothing
            }
            else
                throw new ArgumentOutOfRangeException("count");
        }

        bool ICollection<T>.IsReadOnly { get { return false; } }
        public void Add(T item) { map.Add(item, false); }

        //public static IEqualityComparer<HashSet<T>> CreateSetComparer();
        //public void ExceptWith(IEnumerable<T> other);
        public HashSet<T>.Enumerator GetEnumerator()
        {
            return new HashSet<T>.Enumerator(map.GetEnumerator());
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new HashSet<T>.Enumerator(map.GetEnumerator());
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new HashSet<T>.Enumerator(map.GetEnumerator());
        }

        //public virtual void GetObjectData(SerializationInfo info, StreamingContext context);
        //[SecurityCritical]
        //public void IntersectWith(IEnumerable<T> other);
        //[SecurityCritical]
        //public bool IsProperSubsetOf(IEnumerable<T> other);
        //[SecurityCritical]
        //public bool IsProperSupersetOf(IEnumerable<T> other);
        //[SecurityCritical]
        //public bool IsSubsetOf(IEnumerable<T> other);
        //public bool IsSupersetOf(IEnumerable<T> other);
        //public virtual void OnDeserialization(object sender);
        //public bool Overlaps(IEnumerable<T> other);
        public bool Remove(T item) { return map.Remove(item); }
        //public int RemoveWhere(Predicate<T> match);
        //[SecurityCritical]
        //public bool SetEquals(IEnumerable<T> other);
        //[SecurityCritical]
        //public void SymmetricExceptWith(IEnumerable<T> other);
        //public void TrimExcess();
        //public void UnionWith(IEnumerable<T> other);

        [Serializable]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            Dictionary<T, Boolean>.Enumerator mapEnumerator;
            public Enumerator(Dictionary<T, Boolean>.Enumerator mapEnumerator)
            {
                this.mapEnumerator = mapEnumerator;
            }
            public T Current { get { return mapEnumerator.Current.Key; } }
            public void Dispose() { mapEnumerator.Dispose(); }
            public bool MoveNext() { return mapEnumerator.MoveNext(); }

            object IEnumerator.Current
            {
                get { return mapEnumerator.Current; }
            }
            void IEnumerator.Reset() { ((IEnumerator)mapEnumerator).Reset(); }
        }
    }
}
namespace System.IO
{
    public enum DriveType
    {
        // Summary:
        //     The type of drive is unknown.
        Unknown = 0,
        //
        // Summary:
        //     The drive does not have a root directory.
        NoRootDirectory = 1,
        //
        // Summary:
        //     The drive is a removable storage device, such as a floppy disk drive or a
        //     USB flash drive.
        Removable = 2,
        //
        // Summary:
        //     The drive is a fixed disk.
        Fixed = 3,
        //
        // Summary:
        //     The drive is a network drive.
        Network = 4,
        //
        // Summary:
        //     The drive is an optical disc device, such as a CD or DVD-ROM.
        CDRom = 5,
        //
        // Summary:
        //     The drive is a RAM disk.
        Ram = 6,
    }

    // Summary:
    //     Provides access to information on a drive.
    [Serializable]
    public sealed class DriveInfo
    {
        public readonly String driveName;

        private UInt64 availableAndFreeBytes;
        private UInt64 totalBytes;
        private UInt64 totalFreeBytes;

        public DriveInfo(String driveName)
        {
            this.driveName = driveName;
        }

        void RefreshDiskSpace()
        {
            if (!WindowsCESafeNativeMethods.GetDiskFreeSpaceEx(driveName,
                out availableAndFreeBytes, out totalBytes, out totalFreeBytes))
                throw new IOException("GetDiskFreeSpaceEx failed");
        }

        public String DriveFormat { get { return "Unknown"; } }

        public DriveType DriveType { get { return DriveType.Unknown; } }

        public Boolean IsReady { get { return true; } }

        public String Name { get { return driveName; } }

        public DirectoryInfo RootDirectory { get { throw new NotSupportedException(); } }

        public Int64 AvailableFreeSpace { get { RefreshDiskSpace(); return (Int64)availableAndFreeBytes; } }
        public Int64 TotalFreeSpace { get { RefreshDiskSpace(); return (Int64)totalFreeBytes; } }
        public Int64 TotalSize { get { RefreshDiskSpace(); return (Int64)totalBytes; } }

        public string VolumeLabel { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
        public static DriveInfo[] GetDrives()
        {
            throw new NotSupportedException();
        }
        public override String ToString()
        {
            return driveName;
        }
    }
}
namespace System.Net
{
    public static class MissingInCEIPParser
    {
        public static Boolean TryParse(String ipString, out IPAddress address)
        {
            try
            {
                address = IPAddress.Parse(ipString);
                return true;
            }
            catch (FormatException)
            {
                address = null;
                return false;
            }
        }
    }
}
/*
// Moved to More.Net because it depends on it

namespace System.Net.Sockets
{
    public static class MissingInCESocket
    {
        public static void Connect(this Socket socket, String host, Int32 port)
        {
            var ip = EndPoints.ParseIPOrResolveHost(host, DnsPriority.IPv4ThenIPv6);
            socket.Connect(new IPEndPoint(ip, port));
        }
    }
}
*/
namespace System.Runtime.ConstrainedExecution
{
    [Serializable]
    public enum Consistency
    {
        MayCorruptProcess = 0,
        MayCorruptAppDomain = 1,
        MayCorruptInstance = 2,
        WillNotCorruptState = 3,
    }
    [Serializable]
    public enum Cer
    {
        None = 0,
        MayFail = 1,
        Success = 2,
    }
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Interface, Inherited = false)]
    public sealed class ReliabilityContractAttribute : Attribute
    {
        private Consistency consistencyGuarantee;
        private Cer cer;
        public ReliabilityContractAttribute(Consistency consistencyGuarantee, Cer cer)
        {
            this.consistencyGuarantee = consistencyGuarantee;
            this.cer = cer;
        }
        public Cer Cer { get { return cer; } }
        public Consistency ConsistencyGuarantee { get { return consistencyGuarantee; } }
    }
}
namespace System.Runtime.Serialization
{
    public sealed class FormatterServices
    {
        public static object GetUninitializedObject(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
namespace System.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = true, Inherited = false)]
    [ComVisible(true)]
    public sealed class SuppressUnmanagedCodeSecurityAttribute : Attribute
    {
        public SuppressUnmanagedCodeSecurityAttribute()
        {
        }
    }
}
#endif