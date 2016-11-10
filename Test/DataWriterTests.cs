// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class DataWriterTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            XmlWriter writer = new XmlWriter(WriterFormatter.Stdout, true, true);

            writer.WriteObjectStart("JetlinkDevice",
                new NamedObject("Type", "Output"),
                new NamedObject("Name", "Kihuaro"));

            writer.WriteObject(new NamedObject("OutputPortsAvailable", "23"));

            {
                writer.WriteObjectStart("Detach", new NamedObject("Type", "DoorOpen"));
                writer.WriteAttribute("DoorNumber", "3");
                writer.WriteObjectEnd("Detach");
            }

            {
                writer.WriteObjectStart("DeviceConditions");

                writer.WriteObject("DeviceCondition", null,
                    new NamedObject("JetlinkCauseCode", "3"),
                    new NamedObject("JediErrorCode", "1.2.3"));
                writer.WriteObject("DeviceCondition", "Is this working?",
                    new NamedObject("JetlinkCauseCode", "4"),
                    new NamedObject("JediErrorCode", "a.b.c"));

                writer.WriteObjectEnd("DeviceConditions");
            }

            writer.WriteObjectEnd("JetlinkDevice");
        }
    }
}
