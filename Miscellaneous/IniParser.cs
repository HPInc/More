// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace More
{
    public class IniKeyAndValue
    {
        public String key;
        public String value;
        public IniKeyAndValue(String key, String value)
        {
            this.key = key;
            this.value = value;
        }
    }
    public struct IniSectionKeyPair
    {
        public String section;
        public String key;
        public IniSectionKeyPair(String section, String key)
        {
            this.section = section;
            this.key = key;
        }
    }
    public class IniSettings
    {
        static readonly Char[] StringSplitByEquals = new Char[] { '=' };
        protected static Dictionary<IniSectionKeyPair, String> ParseIniStream(Stream stream, Encoding encoding, UInt32 readBufferSize, out HashSet<String> sections)
        {
            sections = new HashSet<String>();
            Dictionary<IniSectionKeyPair, String> keyPairs = new Dictionary<IniSectionKeyPair, String>();
            String currentSection = null;

            LineParser lineParser = new LineParser(encoding, readBufferSize);
            byte[] readBuffer = new Byte[readBufferSize];

            while (true)
            {
                Int32 bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                if (bytesRead <= 0) break;

                lineParser.Add(readBuffer, 0, (UInt32)bytesRead);

                String line;
                while ((line = lineParser.GetLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length > 0)
                    {
                        //String lineUpperCase = line.ToUpper();

                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            currentSection = line.Substring(1, line.Length - 2).ToUpper();
                            sections.Add(currentSection);
                        }
                        else
                        {
                            String[] keyPair = line.Split(StringSplitByEquals, 2);
                            if (keyPair == null) throw new InvalidOperationException(String.Format("String.Split byte '=' returned null of line '{0}'", line));

                            IniSectionKeyPair sectionPair;
                            sectionPair.section = currentSection;

                            String value;
                            if (keyPair.Length == 1)
                            {
                                sectionPair.key = keyPair[0].ToUpper();
                                value = null;
                            }
                            else if (keyPair.Length == 2)
                            {
                                sectionPair.key = keyPair[0].ToUpper();
                                value = keyPair[1];
                            }
                            else
                            {
                                throw new InvalidOperationException(String.Format(
                                    "Expected String.Split by '=' to return 1 or 2 for line '{0}' but it returned {1}", line, keyPair.Length));
                            }

                            keyPairs.Add(sectionPair, value);
                        }
                    }
                }
            }

            return keyPairs;
        }

        protected readonly HashSet<String> sections;
        protected readonly Dictionary<IniSectionKeyPair, String> keyPairs;

        public IniSettings(Stream stream, Encoding encoding, UInt32 readBufferSize)
        {
            this.keyPairs = ParseIniStream(stream, encoding, readBufferSize, out sections);
        }
        protected IniSettings(Dictionary<IniSectionKeyPair, String> keyPairs, HashSet<String> sections)
        {
            this.keyPairs = keyPairs;
            this.sections = sections;
        }

        //
        // Settings Accesssors
        //
        public String GetSetting(String key)
        {
            IniSectionKeyPair sectionPair;
            sectionPair.section = null;
            sectionPair.key = key.ToUpper();

            return keyPairs[sectionPair];
        }
        public Boolean TryGetSetting(String key, out String value)
        {
            IniSectionKeyPair sectionPair;
            sectionPair.section = null;
            sectionPair.key = key.ToUpper();
            return keyPairs.TryGetValue(sectionPair, out value);
        }

        public String GetSetting(String sectionName, String key)
        {
            IniSectionKeyPair sectionPair;
            sectionPair.section = sectionName.ToUpper();
            sectionPair.key = key.ToUpper();

            return keyPairs[sectionPair];
        }
        public Boolean TryGetSetting(String section, String key, out String value)
        {
            IniSectionKeyPair sectionPair;
            sectionPair.section = section.ToUpper();
            sectionPair.key = key.ToUpper();
            return keyPairs.TryGetValue(sectionPair, out value);
        }

        public IEnumerable<IniKeyAndValue> RootSettings()
        {
            foreach (KeyValuePair<IniSectionKeyPair, String> pair in keyPairs)
            {
                if (pair.Key.section == null)
                {
                    yield return new IniKeyAndValue(pair.Key.key, pair.Value);
                }
            }
        }
        public IEnumerable<IniKeyAndValue> SectionSettings(String section)
        {
            if (section == null) throw new InvalidOperationException("sectionName cannot be null, to get root settigns call RootSettings()");

            foreach (KeyValuePair<IniSectionKeyPair, String> pair in keyPairs)
            {
                if (pair.Key.section.Equals(section))
                {
                    yield return new IniKeyAndValue(pair.Key.key, pair.Value);
                }
            }
        }

        //
        // Settings Modifiers
        //
        public void AddRootSetting(String key, String value)
        {
            keyPairs.Add(new IniSectionKeyPair(null, key.ToUpper()), value);
        }
        public void AddRootSetting(String key)
        {
            keyPairs.Add(new IniSectionKeyPair(null, key.ToUpper()), null);
        }

        public void AddSectionSetting(String section, String key)
        {
            String sectionUpperCase = section.ToUpper();
            sections.Add(sectionUpperCase);
            keyPairs.Add(new IniSectionKeyPair(sectionUpperCase, key.ToUpper()), null);
        }
        public void AddSectionSetting(String section, String key, String value)
        {
            String sectionUpperCase = section.ToUpper();
            sections.Add(sectionUpperCase);
            keyPairs.Add(new IniSectionKeyPair(sectionUpperCase, key.ToUpper()), value);
        }

        public Boolean RemoveRootSetting(String key)
        {
            return keyPairs.Remove(new IniSectionKeyPair(null, key.ToUpper()));
        }
        public Boolean RemoveSectionSetting(String section, String key)
        {
            //
            // Need to remove section if it is empty
            //
            Boolean ret = keyPairs.Remove(new IniSectionKeyPair(section.ToUpper(), key.ToUpper()));

            //
            // Need to remove section if it is empty
            //
            foreach (KeyValuePair<IniSectionKeyPair, String> pair in keyPairs)
            {
                if (pair.Key.section.Equals(section))
                {
                    return ret;
                }
            }

            // Remove the section because it has no more entries
            sections.Remove(section);

            return ret;
        }
    }
    public class IniFile : IniSettings
    {
        public static IniFile Load(String filename, Encoding encoding, UInt32 readBufferSize)
        {
            HashSet<String> sections;
            Dictionary<IniSectionKeyPair,String> keyPairs;
            using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                keyPairs = IniSettings.ParseIniStream(fileStream, encoding, readBufferSize, out sections);
            }

            return new IniFile(filename, keyPairs, sections);
        }
        String iniFileName;

        private IniFile(String iniFileName, Dictionary<IniSectionKeyPair,String> keyPairs, HashSet<String> sections)
            : base(keyPairs, sections)
        {
            this.iniFileName = iniFileName;
        }
        public void SetNewFileName(String iniFileName)
        {
            this.iniFileName = iniFileName;
        }
        public void Save()
        {
            using(TextWriter writer = new StreamWriter(new FileStream(iniFileName, FileMode.Create, FileAccess.Write)))
            {
                WriteSectionKeyAndValues(writer, RootSettings());

                foreach (String section in sections)
                {
                    WriteSectionKeyAndValues(writer, SectionSettings(section));
                }
            }
        }
        void WriteSectionKeyAndValues(TextWriter writer, IEnumerable<IniKeyAndValue> keyAndValues)
        {
            foreach (IniKeyAndValue keyAndValue in keyAndValues)
            {
                if(keyAndValue.value == null)
                {
                    writer.WriteLine("{0}", keyAndValue.key);
                }
                else
                {
                    writer.WriteLine("{0}={1}", keyAndValue.key, keyAndValue.value);
                }
            }            
        }
    }
}