using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.IO;
using RogueBasin;

namespace DDRogueTest
{
    [TestClass]
    public class RoomTemplateLoaderTest
    {
        [TestMethod]
        public void WidthAndHeightShouldBeSetCorrectlyFromLoadedFile()
        {
            RoomTemplate loadedTemplate = RoomTemplateLoader.LoadTemplateFromFile(GetFileStreamFromResources("DDRogueTest.testdata.vaults.teststandardsize.room"), StandardTemplateMapping.terrainMapping);
            Assert.AreEqual(loadedTemplate.Width, 8);
            Assert.AreEqual(loadedTemplate.Height, 4);
        }

        [TestMethod]
        public void PotentialDoorsShouldBeAvailableFromLoadedFile()
        {
            RoomTemplate loadedTemplate = RoomTemplateLoader.LoadTemplateFromFile(GetFileStreamFromResources("DDRogueTest.testdata.vaults.testpotentialdoors.room"), StandardTemplateMapping.terrainMapping);
            Assert.AreEqual(loadedTemplate.PotentialDoors.Count, 2);
            Assert.AreEqual(loadedTemplate.PotentialDoors[0].Location, new Point(4, 0));
            Assert.AreEqual(loadedTemplate.PotentialDoors[1].Location, new Point(7, 2));
        }

        [TestMethod]
        public void TrailingWhitespaceIsIgnored()
        {
            RoomTemplate loadedTemplate = RoomTemplateLoader.LoadTemplateFromFile(GetFileStreamFromResources("DDRogueTest.testdata.vaults.testtrailingspaces.room"), StandardTemplateMapping.terrainMapping);
            Assert.AreEqual(loadedTemplate.PotentialDoors.Count, 2);
            Assert.AreEqual(loadedTemplate.PotentialDoors[0].Location, new Point(4, 0));
            Assert.AreEqual(loadedTemplate.PotentialDoors[1].Location, new Point(7, 2));
            Assert.AreEqual(loadedTemplate.PotentialDoors[0].Location, new Point(4, 0));
            Assert.AreEqual(loadedTemplate.PotentialDoors[1].Location, new Point(7, 2));
        }

        public Stream GetFileStreamFromResources(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            return _assembly.GetManifestResourceStream(filePath);
        }


    }
}
