// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.IO;

namespace More
{
    public static class PlatformPath
    {
        public static Boolean IsValidUnixFileName(String fileName)
        {
            return !fileName.Contains("/");
            //return !fileName.Contains('/');
        }
        public static String LocalCombine(String parent, String child)
        {
            return String.Format("{0}{1}{2}", parent, Path.DirectorySeparatorChar, child);
        }
        public static String LocalPathDiff(String localParentDirectory, String localPathAndFileName)
        {
            if (!localPathAndFileName.StartsWith(localParentDirectory))
                throw new InvalidOperationException(String.Format("You attempted to take the local path diff of '{0}' and '{1}' but the second path does not start with the first path",
                    localParentDirectory, localPathAndFileName));

            Int32 index;
            for (index = localParentDirectory.Length; true; index++)
            {
                if (index >= localPathAndFileName.Length)
                    throw new InvalidOperationException(String.Format("The local path diff of '{0}' and '{1}' is empty",
                        localParentDirectory, localPathAndFileName));
                if (localPathAndFileName[index] != Path.DirectorySeparatorChar) break;
            }

            return localPathAndFileName.Substring(index);
        }
        public static String LocalToUnixPath(String localPath)
        {
            if (Path.DirectorySeparatorChar == '/') return localPath;
            return localPath.Replace(Path.DirectorySeparatorChar, '/');
        }

        public static String UnixToLocalPath(String unixPath)
        {
            if (Path.DirectorySeparatorChar == '/') return unixPath;
            return unixPath.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
