// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.IO;

namespace More
{
    public abstract class Config
    {
        public readonly String name;
        public readonly String description;

        public Config(String name, String description)
        {
            this.name = name;
            this.description = description;
        }
        protected void WriteDescription(String linePrefix, TextWriter writer)
        {
            if (description != null)
            {
                writer.Write(linePrefix);
                writer.WriteLine(description);
            }
        }
        public abstract void Handle(LfdLine lfdLine);
        public void WriteTemplate(TextWriter writer)
        {
            WriteTemplate("", writer);
        }
        public abstract void WriteTemplate(String linePrefix, TextWriter writer);
    }
    public class ConfigSwitch : Config
    {
        public readonly Boolean turnOnInTemplate;

        public Boolean value;

        public ConfigSwitch(String name, String description, Boolean turnOnInTemplate)
            : base(name, description)
        {
            this.turnOnInTemplate = turnOnInTemplate;
        }
        public override void Handle(LfdLine lfdLine)
        {
            if (value) throw new FormatException(String.Format("The '{0}' config option appeared more than once", name));
            if (lfdLine.fields != null) throw new FormatException(String.Format(
                 "The '{0}' config option may not have any arguments", name));
            value = true;
        }
        public override void WriteTemplate(String linePrefix, TextWriter writer)
        {
            WriteDescription(linePrefix, writer);
            writer.Write(linePrefix);
            writer.WriteLine(name);
        }
    }
    public class ConfigString : Config
    {
        //public readonly Boolean optional;
        public readonly String templateValue;

        public String value;

        public ConfigString(String name, String description, /*Boolean optional, */String templateValue)
            : base(name, description)
        {
            //this.optional = optional;
            this.templateValue = templateValue;
        }
        public override void Handle(LfdLine lfdLine)
        {
            if (value != null) throw new FormatException(String.Format("The '{0}' config option appeared more than once", name));
            if (lfdLine.fields == null) throw new FormatException(
                String.Format("The '{0}' config option requires a single argument", name));
            if (lfdLine.fields.Length > 1) throw new FormatException(
                String.Format("The '{0}' config option can have at most 1 argument but it has {1}", name, lfdLine.fields.Length));
            this.value = lfdLine.fields[0];
        }
        public override void WriteTemplate(String linePrefix, TextWriter writer)
        {
            WriteDescription(linePrefix, writer);
            writer.Write(linePrefix);
            writer.Write(name);
            writer.Write(' ');
            writer.WriteLine(templateValue);
        }
    }
    public class ConfigStrings : Config
    {
        readonly String templateArgs;
        public String[] strings;

        public ConfigStrings(String name, String description, String templateArgs)
            : base(name, description)
        {
            this.templateArgs = templateArgs;
        }
        public override void Handle(LfdLine lfdLine)
        {
            if (strings != null) throw new FormatException(String.Format("The '{0}' config option appeared more than once", name));
            if (lfdLine.fields == null)
            {
                strings = new String[0];
            }
            else
            {
                strings = lfdLine.fields;
            }
        }
        public override void WriteTemplate(String linePrefix, TextWriter writer)
        {
            WriteDescription(linePrefix, writer);
            writer.Write(linePrefix);
            writer.Write(name);
            writer.Write(' ');
            writer.WriteLine(templateArgs);
        }
    }
    public class Config<T> : Config
    {
        public readonly Parser<T> parser;
        public readonly Boolean optional;
        public readonly T templateValue;

        public Boolean set;
        public T value;

        public Config(String name, String description, Parser<T> parser, Boolean optional, T templateValue)
            : base(name, description)
        {
            this.parser = parser;
            this.optional = optional;
            this.templateValue = templateValue;
        }
        public override void Handle(LfdLine lfdLine)
        {
            if (set) throw new FormatException(String.Format("The '{0}' config option appeared more than once", name));
            if (lfdLine.fields == null) throw new FormatException(
                 String.Format("The '{0}' config option requires a single argument", name));
            if (lfdLine.fields.Length > 1) throw new FormatException(
                String.Format("The '{0}' config option can have at most 1 argument but it has {1}", name, lfdLine.fields.Length));

            this.value = parser(lfdLine.fields[0]);
            set = true;
        }
        public override void WriteTemplate(String linePrefix, TextWriter writer)
        {
            WriteDescription(linePrefix, writer);
            writer.Write(linePrefix);
            writer.Write(name);
            writer.Write(' ');
            writer.WriteLine(templateValue);
        }
    }
    public class MultiConfigStrings : Config
    {
        readonly String templateArgs;
        public readonly List<String> strings;
        public MultiConfigStrings(String name, String description, String templateArgs)
            : base(name, description)
        {
            this.templateArgs = templateArgs;
            strings = new List<String>();
        }
        public override void Handle(LfdLine lfdLine)
        {
            if (lfdLine.fields == null) throw new FormatException(String.Format("Option '{0}' requires at least one field but got none", name));
            for (int i = 0; i < lfdLine.fields.Length; i++)
            {
                strings.Add(lfdLine.fields[i]);
            }
        }
        public override void WriteTemplate(String linePrefix, TextWriter writer)
        {
            WriteDescription(linePrefix, writer);
            writer.Write(linePrefix);
            writer.Write(name);
            writer.Write(' ');
            writer.WriteLine(templateArgs);
        }
    }

    public abstract class LfdConfig : LfdCallbackParser
    {
        readonly List<Config> configs = new List<Config>();

        public void Add(Config config)
        {
            configs.Add(config);
            Add(config.name, config.Handle);
        }

        public void WriteTemplate(TextWriter writer)
        {
            foreach (Config config in configs)
            {
                writer.WriteLine();
                config.WriteTemplate("# ", writer);
            }
        }
    }

}
