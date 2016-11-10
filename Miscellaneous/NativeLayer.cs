// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace More
{
    public static class NativeWindows
    {
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public const int STD_OUTPUT_HANDLE = -11;

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("kernel32")]
        private static extern IntPtr GetStdHandle(UInt32 nStdHandle);
        [DllImport("kernel32")]
        private static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);
        [DllImport("kernel32")]
        public static extern bool AllocConsole();
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32")]
        public static extern UInt32 GetConsoleCP();

        [DllImport("kernel32.dll", EntryPoint = "WriteFile")]
        public static extern bool TryWriteFile(
            IntPtr hFile,
            BytePtr lpBuffer,
            UInt32 nNumberOfBytesToWrite,
            out UInt32 lpNumberOfBytesWritten,
            IntPtr lpOverlapped);
        public static unsafe void WriteFile(IntPtr file, BytePtr buffer, UInt32 length)
        {
            UInt32 written = 0;
            if (!TryWriteFile(file, buffer, length, out written, IntPtr.Zero))
            {
                throw new InvalidOperationException(String.Format("WriteFile failed (lastError={0})", GetLastError()));
            }
            if (written != length)
            {
                throw new InvalidOperationException(String.Format("Attempted to write {0} bytes but only wrote {1}", length, written));
            }
        }
    }

    public static class NativeExtensions
    {
        public static Boolean IsWindows(this PlatformID platform)
        {
            return platform == PlatformID.Win32NT ||
                platform == PlatformID.Win32S ||
                platform == PlatformID.Win32Windows ||
                platform == PlatformID.WinCE;
        }
    }

    internal static class NativeWindowsConsole
    {
        internal static IntPtr StdOutFileHandle;
        public static UInt32 StdOutCodePage;

        static NativeWindowsConsole()
        {
            StdOutFileHandle = NativeWindows.GetStdHandle(NativeWindows.STD_OUTPUT_HANDLE);
            if (StdOutFileHandle == NativeWindows.INVALID_HANDLE_VALUE)
            {
                throw new InvalidOperationException(String.Format("GetStdHandle(STD_OUTPUT_HANDLE) failed (error={0})", NativeWindows.GetLastError()));
            }
            if (StdOutFileHandle != IntPtr.Zero)
            {
                NativeWindowsConsole.StdOutCodePage = NativeWindows.GetConsoleCP();
            }
        }
        public static void Write(BytePtr buffer, UInt32 length)
        {
            UInt32 written = 0;
            if (!NativeWindows.TryWriteFile(StdOutFileHandle, buffer, length, out written, IntPtr.Zero))
            {
                throw new InvalidOperationException(String.Format("WriteFile failed (lastError={0})", NativeWindows.GetLastError()));
            }
            if (written != length)
            {
                throw new InvalidOperationException(String.Format("Attempted to write {0} bytes but only wrote {1}", length, written));
            }
        }
    }

    internal class BufferedWindowsConsoleSink : BufferedSink
    {
        public static unsafe BufferedWindowsConsoleSink Create(UInt32 bufferLength, Boolean flushAtWriteLine)
        {
            var buffer = Marshal.AllocHGlobal((int)bufferLength);
            return new BufferedWindowsConsoleSink(buffer, ((byte*)buffer.ToPointer()) + bufferLength, flushAtWriteLine);
        }

        bool disposed;

        private BufferedWindowsConsoleSink(BytePtr buffer, BytePtr bufferLimit, Boolean flushAtWriteLine)
            : base(buffer, bufferLimit, flushAtWriteLine, Encoder.Utf8,
                (NativeWindowsConsole.StdOutFileHandle == IntPtr.Zero) ? new FlushHandler(TypedSink.DoNothingFlushHandler) :
                new FlushHandler(NativeWindowsConsole.Write), true)
        {
        }
        public override void Dispose()
        {
            if (!disposed)
            {
                Flush();
                Marshal.FreeHGlobal(buffer);
                disposed = true;
            }
        }
        ~BufferedWindowsConsoleSink()
        {
            Dispose();
        }
    }



    public abstract class IO
    {
        static IO()
        {
            if (Environment.OSVersion.Platform.IsWindows())
            {
                StdOut = BufferedWindowsConsoleSink.Create(64, true);
            }
            else
            {
                throw new PlatformNotSupportedException(String.Format("No console implemented for platform: {0}",
                    Environment.OSVersion.Platform));
            }
        }

        public static TypedSink StdOut;

        public static void EnsureConsoleOpen()
        {
            if (Environment.OSVersion.Platform.IsWindows())
            {
                if (NativeWindowsConsole.StdOutFileHandle == IntPtr.Zero)
                {
                    if (NativeWindows.AllocConsole() == false)
                    {
                        throw new InvalidOperationException(String.Format("Failed to create console (lasterror={0})", NativeWindows.GetLastError()));
                    }
                    NativeWindowsConsole.StdOutFileHandle = NativeWindows.GetStdHandle(NativeWindows.STD_OUTPUT_HANDLE);
                    if (NativeWindowsConsole.StdOutFileHandle == NativeWindows.INVALID_HANDLE_VALUE)
                    {
                        throw new InvalidOperationException(String.Format("GetStdHandle(STD_OUTPUT_HANDLE) failed (error={0})", NativeWindows.GetLastError()));
                    }
                    if (NativeWindowsConsole.StdOutFileHandle == IntPtr.Zero)
                    {
                        throw new InvalidOperationException(String.Format("GetStdHandle(STD_OUTPUT_HANDLE) returned 0 but this doesn't make sense because we just called AllocConsole? (lasterror={0})", NativeWindows.GetLastError()));
                    }
                    NativeWindowsConsole.StdOutCodePage = NativeWindows.GetConsoleCP();
                    StdOut = BufferedWindowsConsoleSink.Create(64, true);
                }
            }
            else
            {
                throw new PlatformNotSupportedException(String.Format("No console implemented for platform: {0}",
                    Environment.OSVersion.Platform));
            }
        }
    }
}