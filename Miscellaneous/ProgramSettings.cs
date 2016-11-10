// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Text;


namespace More
{
    public abstract class ProgramSetting
    {
        public readonly String name;
        public readonly String description;

        protected ProgramSetting(String name, String description)
        {
            this.name = name;
            this.description = description;
        }
    }

    public enum ProgramSettingsFormat
    {
        Sos,Xml,Json,Ini
    }

    public abstract class ProgramSettings
    {
        readonly Dictionary<String, ProgramSetting> settingDictionary;

        public ProgramSettings()
        {
            this.settingDictionary = new Dictionary<String, ProgramSetting>();
        }

        public void LoadFromFile(String filename, ProgramSettingsFormat format)
        {
            throw new NotImplementedException("ProgramSettings.LoadFromFile is not implemented yet");
            /*
            if (format == ProgramSettingsFormat.Sos)
            {
                String fileContents = Encoding.UTF8.GetString(FileExtensions.ReadFile(filename));
                

            }
            else
            {
                throw new NotSupportedException(String.Format("Program Setting Format '{0}' is not currently supported", format));
            }
            */
        }

        public void LoadFromCommandLine(String[] args)
        {
        }

        public void AddSetting(ProgramSetting setting)
        {
            settingDictionary.Add(setting.name, setting);
        }
    }
}