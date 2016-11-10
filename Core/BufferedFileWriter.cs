// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.IO;
using System.Security;

namespace More
{
    static class WindowsNativeMethods
    {
        [DllImport("kernel32")]
        public static extern UInt32 GetLastError();

        [DllImport("kernel32", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
             [MarshalAs(UnmanagedType.LPTStr)] String filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);
        
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteFile(IntPtr hFile, byte[] buffer,
           UInt32 nNumberOfBytesToWrite, out UInt32 lpNumberOfBytesWritten,
           [In] IntPtr lpOverlapped);
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe extern bool WriteFile(IntPtr hFile, byte* buffer,
           UInt32 nNumberOfBytesToWrite, out UInt32 lpNumberOfBytesWritten,
           [In] IntPtr lpOverlapped);
    }

    public static class NativeFile
    {
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        public static IntPtr TryOpen(String filename, FileMode mode, FileAccess access, FileShare share)
        {
            return WindowsNativeMethods.CreateFile(filename, access, share,
                IntPtr.Zero, mode, FileAttributes.Normal, IntPtr.Zero);
        }
        public static IntPtr Open(String filename, FileMode mode, FileAccess access, FileShare share)
        {
            var fileHandle = WindowsNativeMethods.CreateFile(filename, access, share,
                IntPtr.Zero, mode, FileAttributes.Normal, IntPtr.Zero);
            if (fileHandle == INVALID_HANDLE_VALUE)
            {
                throw new Exception(String.Format("CreateFile '{0}' (mode={1}, access={2}, share={3}) failed (error={4})",
                    filename, mode, access, share, WindowsNativeMethods.GetLastError()));
            }
            return fileHandle;
        }
    }

    public class BufferedFileWriter : IDisposable, IFormatWriter
    {
        IntPtr fileHandle;
        byte[] buffer;
        uint bufferedLength;

        public BufferedFileWriter(IntPtr fileHandle, byte[] buffer)
        {
            this.fileHandle = fileHandle;
            this.buffer = buffer;
        }
        public void Dispose()
        {
            if (fileHandle != NativeFile.INVALID_HANDLE_VALUE)
            {
                Flush();
                WindowsNativeMethods.CloseHandle(fileHandle);
                fileHandle = NativeFile.INVALID_HANDLE_VALUE;
            }
        }
        public void Flush()
        {
            if (bufferedLength > 0)
            {
                UInt32 written;
                if (false == WindowsNativeMethods.WriteFile(fileHandle, buffer, bufferedLength, out written, IntPtr.Zero))
                {
                    throw new IOException(String.Format("WriteFile({0} bytes) failed (error={1})", bufferedLength, WindowsNativeMethods.GetLastError()));
                }
                if (written != bufferedLength)
                {
                    throw new IOException(String.Format("Only wrote {0} out of {1}", written, bufferedLength));
                }
                //Console.WriteLine("Flushed {0} bytes:", bufferedLength);
                //Console.WriteLine("{0}", System.Text.Encoding.UTF8.GetString(buffer, 0, (int)bufferedLength));

                bufferedLength = 0;
            }
        }

        // TODO: tweek this value to maximize performance
        //       Compare the cost of a flush to the cost of copying the data to
        //       the buffer and flushing the buffer later.
        //       if you buffer
        //          copy data to buffer
        //          (later you will flush)
        //       if you don't buffer
        //          
        //          write data to file
        //          (later you will call flush)
        const UInt32 SmallEnoughToBuffer = 16;
        public unsafe void Write(byte* data, uint length)
        {
            if (length <= SmallEnoughToBuffer)
            {
                if (bufferedLength + length > buffer.Length)
                {
                    Flush();
                }
                // TODO: call a native function to perform the copy
                for (uint i = 0; i < length; i++)
                {
                    buffer[bufferedLength + i] = data[i];
                }
                bufferedLength += length;
            }
            else
            {
                Flush();
                UInt32 written;
                if (false == WindowsNativeMethods.WriteFile(fileHandle, data, length, out written, IntPtr.Zero))
                {
                    throw new IOException(String.Format("WriteFile({0} bytes) failed (error={1})", length, WindowsNativeMethods.GetLastError()));
                }
                if (written != length)
                {
                    throw new IOException(String.Format("Only wrote {0} out of {1}", written, length));
                }
            }
        }
        public unsafe void Write(byte[] byteString)
        {
            fixed (byte* byteStringPtr = byteString)
            {
                Write(byteStringPtr, (uint)byteString.Length);
            }
        }
        public unsafe void Write(byte[] byteString, uint offset, uint length)
        {
            fixed (byte* byteStringPtr = byteString)
            {
                Write(byteStringPtr + offset, length - offset);
            }
        }

        public void Write(string str)
        {
            Write(str, 0, (uint)str.Length);
        }
        public unsafe void Write(string str, uint offset, uint length)
        {
            fixed (Byte* bufferPtr = buffer)
            {
                byte* bufferLimit = bufferPtr + buffer.Length;
                uint charsEncoded;
                {
                    byte* bufferedDataPtr = bufferPtr + bufferedLength;
                    byte* writtenTo = EncodingEx.EncodeMaxUtf8(out charsEncoded,
                        str, offset, length, bufferedDataPtr, bufferLimit);
                    bufferedLength += (uint)(writtenTo - bufferedDataPtr);
                }

                while (charsEncoded < length)
                {
                    Flush();
                    offset += charsEncoded;
                    length -= charsEncoded;
                    byte* writtenTo = EncodingEx.EncodeMaxUtf8(out charsEncoded,
                        str, offset, length, bufferPtr, bufferLimit);
                    bufferedLength += (uint)(writtenTo - bufferPtr);
                }
            }
        }

        public unsafe void WriteLine()
        {
            Byte newLine = (Byte)'\n';
            Write(&newLine, 1);
        }
        public unsafe void WriteLine(Byte* buffer, UInt32 length)
        {
            Write(buffer, length);
            WriteLine();
        }
        public void WriteLine(Byte[] buffer)
        {
            Write(buffer, 0, (uint)buffer.Length);
            WriteLine();
        }
        public void WriteLine(Byte[] buffer, UInt32 offset, UInt32 length)
        {
            Write(buffer, offset, length);
            WriteLine();
        }
        public void WriteLine(String str, UInt32 offset, UInt32 length)
        {
            Write(str, offset, length);
            WriteLine();
        }
        public void WriteLine(String str)
        {
            WriteLine(str, 0, (uint)str.Length);
        }

        public void Write(Stream stream)
        {
            while (true)
            {
                Flush();
                int size = stream.Read(buffer, 0, buffer.Length);
                if (size <= 0)
                {
                    if (size < 0)
                    {
                        throw new IOException();
                    }
                    break;
                }
                bufferedLength = (uint)size;
            }
        }

        // TODO: instead of calling String.Format, I should
        //       put the data in the file buffer...but this would
        //       probably mean re-implementing String.Format.
        //       But that might be ok, because instead of having
        //       String.Format call ToString, it should call a ToString
        //       that writes to a callback, like dlang does.
        public void Write(String format, Object a)
        {
            Write(String.Format(format, a));
        }
        public void Write(String format, Object a, Object b)
        {
            Write(String.Format(format, a, b));
        }
        public void Write(String format, Object a, Object b, Object c)
        {
            Write(String.Format(format, a, b, c));
        }
        public void Write(String format, params Object[] obj)
        {
            Write(String.Format(format, obj));
        }

        public void WriteLine(String format, Object a)
        {
            WriteLine(String.Format(format, a));
        }
        public void WriteLine(String format, Object a, Object b)
        {
            WriteLine(String.Format(format, a, b));
        }
        public void WriteLine(String format, Object a, Object b, Object c)
        {
            WriteLine(String.Format(format, a, b, c));
        }
        public void WriteLine(String format, params Object[] obj)
        {
            WriteLine(String.Format(format, obj));
        }
    }
}