// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.IO;

namespace More
{
    public static class RollingLog
    {
        public static FileStream RollAndOpen(String name, Int32 rollCount, Boolean readAccess, FileShare share)
        {
            FileAccess access = readAccess ? FileAccess.ReadWrite : FileAccess.Write;

            if (File.Exists(name))
            {
                Int32 lastRoll;
                for (lastRoll = 1; lastRoll < rollCount; lastRoll++)
                {
                    if (!File.Exists(name + "." + lastRoll.ToString())) break;
                }

                String rollToName = name + "." + lastRoll.ToString();
                if (lastRoll == rollCount && File.Exists(rollToName)) File.Delete(rollToName);
                lastRoll--;

                while (lastRoll > 0)
                {
                    String nextRollToName = name + "." + lastRoll.ToString();

                    File.Move(nextRollToName, rollToName);

                    rollToName = nextRollToName;
                    lastRoll--;

                }

                File.Move(name, rollToName);
            }

            return new FileStream(name, FileMode.Create, access, share);
        }
    }
}
