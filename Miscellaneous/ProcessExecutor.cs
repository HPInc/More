// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace More
{
#if !WindowsCE
    public static class ProcessExecutor
    {
        public static Boolean RunWithReadersAndWriters(this Process process, TimeSpan timeout, TextReader stdin, TextWriter stdout, TextWriter stderr)
        {
            ReaderToWriterThread stdinRWThread = null, stdoutRWThread = null, stderrRWThread = null;
            Thread stdinThread = null, stdoutThread = null, stderrThread = null;

            if (stdin != null)
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
            }
            if (stdout != null)
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
            }
            if (stderr != null)
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
            }

            process.Start();

            if (stdin != null)
            {
                stdinRWThread = new ReaderToWriterThread(null, stdin, process.StandardInput);
                stdinThread = new Thread(stdinRWThread.Run);
                stdinThread.IsBackground = true;
                stdinThread.Start();
            }
            if (stdout != null)
            {
                stdoutRWThread = new ReaderToWriterThread(null, process.StandardOutput, stdout);
                stdoutThread = new Thread(stdoutRWThread.Run);
                stdoutThread.IsBackground = true;
                stdoutThread.Start();
            }
            if (stderr != null)
            {
                stderrRWThread = new ReaderToWriterThread(null, process.StandardOutput, stderr);
                stderrThread = new Thread(stderrRWThread.Run);
                stderrThread.IsBackground = true;
                stderrThread.Start();
            }
            if (timeout == TimeSpan.Zero)
            {
                process.WaitForExit();
                if (stdinThread != null) stdinThread.Join();
                if (stdoutThread != null) stdoutThread.Join();
                if (stdoutThread != null) stdoutThread.Join();
                return true;
            }
            else
            {
                if (process.WaitForExit((int)timeout.TotalMilliseconds))
                {
                    if (stdinThread != null) stdinThread.Join();
                    if (stdoutThread != null) stdoutThread.Join();
                    if (stdoutThread != null) stdoutThread.Join();
                    return true;
                }
                else
                {
                    if (stdinRWThread != null) stdinRWThread.StopLooping();
                    if (stdoutRWThread != null) stdoutRWThread.StopLooping();
                    if (stderrRWThread != null) stderrRWThread.StopLooping();
                    return false;
                }
            }
        }
    }
#endif
}
