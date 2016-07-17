using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.IO;
using System.Collections.Immutable;
using RogueBasin;

namespace DDRogueTest
{
    [TestClass]
    public class RoomTemplateLoaderTest
    {
        class TestFeature : Feature {

        }

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

        [TestMethod]
        public void DecorationFeaturesInFileArePlaced()
        {
            var loadedTemplate = LoadRoomTemplate("testsimplefeature.room");
            var expectedFeatureType = DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.HumanCorpse2];

            Assert.AreEqual(expectedFeatureType.representation, loadedTemplate.Features[new Point(3, 1)].Representation);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void FeatureCannotBePlacedOffBLMap()
        {
            RoomTemplate loadedTemplate = RoomTemplateLoader.LoadTemplateFromFile(GetFileStreamFromResources("DDRogueTest.testdata.vaults.testfeatureplacement.room"), StandardTemplateMapping.terrainMapping);
            loadedTemplate.AddFeature(new Point(0, -1), new TestFeature());
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void FeatureCannotBePlacedOffTRMap()
        {
            RoomTemplate loadedTemplate = RoomTemplateLoader.LoadTemplateFromFile(GetFileStreamFromResources("DDRogueTest.testdata.vaults.testfeatureplacement.room"), StandardTemplateMapping.terrainMapping);
            loadedTemplate.AddFeature(new Point(9, 5), new TestFeature());
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void FeatureCannotBePlacedOnWall()
        {
            RoomTemplate loadedTemplate = RoomTemplateLoader.LoadTemplateFromFile(GetFileStreamFromResources("DDRogueTest.testdata.vaults.testfeatureplacement.room"), StandardTemplateMapping.terrainMapping);
            loadedTemplate.AddFeature(new Point(3, 1), new TestFeature());
        }

        [TestMethod]
        public void PlacedFeaturesCanBeRetrieved()
        {
            RoomTemplate loadedTemplate = RoomTemplateLoader.LoadTemplateFromFile(GetFileStreamFromResources("DDRogueTest.testdata.vaults.testfeatureplacement.room"), StandardTemplateMapping.terrainMapping);
            loadedTemplate.AddFeature(new Point(4, 1), new TestFeature());
            Assert.IsNotNull(loadedTemplate.Features[new Point(4, 1)]);
        }

        public RoomTemplate LoadRoomTemplate(string fileName)
        {
            return RoomTemplateLoader.LoadTemplateFromFile(GetFileStreamFromResources("DDRogueTest.testdata.vaults." + fileName), StandardTemplateMapping.terrainMapping);
        }

        public Stream GetFileStreamFromResources(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            return _assembly.GetManifestResourceStream(filePath);
        }


    }
}
