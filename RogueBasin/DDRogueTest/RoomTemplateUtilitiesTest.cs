using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Reflection;
using System.IO;

namespace DDRogueTest
{
    [TestClass]
    public class RoomTemplateUtilitiesTest
    {
        [TestMethod]
        public void CorridorTemplatesCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedcorridor1.room");

            RoomTemplate expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplate(false, 2, corridorTemplate);

            Assert.AreEqual(expandedTemplate, correctOutput);
        }

        [TestMethod]
        public void CorridorTemplatesCanBeExpandedAndSwitchedToHorizontalCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedcorridorhoriz1.room");

            RoomTemplate expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplate(true, 2, corridorTemplate);

            Assert.AreEqual(expandedTemplate, correctOutput);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException), "Corridors must be of length 1")]
        public void CorridorsTemplatesMustBeHeightOne()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testtransparent1.room"); //8x4

            RoomTemplateUtilities.ExpandCorridorTemplate(false, 2, corridorTemplate);
        }

        private RoomTemplate LoadTemplateFromAssemblyFile(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream(filePath);
            return RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
        }
    }
}
