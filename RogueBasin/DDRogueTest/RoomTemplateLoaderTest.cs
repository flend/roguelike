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
        [TestMethod]
        public void WidthAndHeightShouldBeSetCorrectlyFromLoadedFile()
        {
            RoomTemplate loadedTemplate = new RoomTemplateLoader(GetFileStreamFromResources("DDRogueTest.testdata.vaults.teststandardsize.room"), StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            Assert.AreEqual(loadedTemplate.Width, 8);
            Assert.AreEqual(loadedTemplate.Height, 4);
        }

        [TestMethod]
        public void PotentialDoorsShouldBeAvailableFromLoadedFile()
        {
            RoomTemplate loadedTemplate = new RoomTemplateLoader(GetFileStreamFromResources("DDRogueTest.testdata.vaults.testpotentialdoors.room"), StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            Assert.AreEqual(loadedTemplate.PotentialDoors.Count, 2);
            Assert.AreEqual(loadedTemplate.PotentialDoors[0].Location, new Point(4, 0));
            Assert.AreEqual(loadedTemplate.PotentialDoors[1].Location, new Point(7, 2));
        }

        [TestMethod]
        public void TrailingWhitespaceIsIgnored()
        {
            RoomTemplate loadedTemplate = new RoomTemplateLoader(GetFileStreamFromResources("DDRogueTest.testdata.vaults.testtrailingspaces.room"), StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            Assert.AreEqual(loadedTemplate.PotentialDoors.Count, 2);
            Assert.AreEqual(loadedTemplate.PotentialDoors[0].Location, new Point(4, 0));
            Assert.AreEqual(loadedTemplate.PotentialDoors[1].Location, new Point(7, 2));
            Assert.AreEqual(loadedTemplate.PotentialDoors[0].Location, new Point(4, 0));
            Assert.AreEqual(loadedTemplate.PotentialDoors[1].Location, new Point(7, 2));
        }

        [TestMethod]
        public void SingleDecorationFeatureInFileIsPlaced()
        {
            var loadedTemplate = LoadRoomTemplate("testsimplefeature.room");
            var expectedFeatureType = DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.HumanCorpse2];

            Assert.AreEqual(expectedFeatureType.representation, loadedTemplate.Features[new Point(3, 1)].CreateFeature().Representation);
        }

        [TestMethod]
        public void MultipleDecorationFeaturesInFileArePlaced()
        {
            var loadedTemplate = LoadRoomTemplate("testsimplefeatures.room");
            var expectedFeatureType1 = DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.HumanCorpse2];
            var expectedFeatureType2 = DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Screen4];

            Assert.AreEqual(expectedFeatureType1.representation, loadedTemplate.Features[new Point(3, 1)].CreateFeature().Representation);
            Assert.AreEqual(expectedFeatureType2.representation, loadedTemplate.Features[new Point(4, 2)].CreateFeature().Representation);
            Assert.AreEqual(expectedFeatureType2.representation, loadedTemplate.Features[new Point(5, 2)].CreateFeature().Representation);
        }

        [TestMethod]
        public void MultipleFeatureLabelsInFileArePlaced()
        {
            var loadedTemplate = LoadRoomTemplate("testsimplefeaturemarkers.room");
            var expectedLabel1 = "Quest";
            var expectedLabel2 = "My Goal";

            Assert.AreEqual(expectedLabel1, loadedTemplate.FeatureMarkers[new Point(1, 1)].featureLabel);
            Assert.AreEqual(expectedLabel2, loadedTemplate.FeatureMarkers[new Point(5, 2)].featureLabel);
            Assert.AreEqual(expectedLabel2, loadedTemplate.FeatureMarkers[new Point(6, 2)].featureLabel);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void UnknownFeaturePlacementThrowsException()
        {
            var loadedTemplate = LoadRoomTemplate("testunknownfeatures.room");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void UnknownMapCharThrowsException()
        {
            var loadedTemplate = LoadRoomTemplate("testunknownmapchar.room");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void FeatureCannotBePlacedOffBLMap()
        {
            RoomTemplate loadedTemplate = new RoomTemplateLoader(GetFileStreamFromResources("DDRogueTest.testdata.vaults.testfeatureplacement.room"), StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            loadedTemplate.AddFeature(new Point(0, -1), TestFeatureGenerator());
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void FeatureCannotBePlacedOffTRMap()
        {
            RoomTemplate loadedTemplate = new RoomTemplateLoader(GetFileStreamFromResources("DDRogueTest.testdata.vaults.testfeatureplacement.room"), StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            loadedTemplate.AddFeature(new Point(9, 5), TestFeatureGenerator());
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void FeatureCannotBePlacedOnWall()
        {
            RoomTemplate loadedTemplate = new RoomTemplateLoader(GetFileStreamFromResources("DDRogueTest.testdata.vaults.testfeatureplacement.room"), StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            loadedTemplate.AddFeature(new Point(3, 1), TestFeatureGenerator());
        }

        [TestMethod]
        public void PlacedFeaturesCanBeRetrieved()
        {
            RoomTemplate loadedTemplate = new RoomTemplateLoader(GetFileStreamFromResources("DDRogueTest.testdata.vaults.testfeatureplacement.room"), StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            loadedTemplate.AddFeature(new Point(4, 1), TestFeatureGenerator());
            Assert.IsNotNull(loadedTemplate.Features[new Point(4, 1)]);
        }

        public RoomTemplate LoadRoomTemplate(string fileName)
        {
            return new RoomTemplateLoader(GetFileStreamFromResources("DDRogueTest.testdata.vaults." + fileName), StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
        }

        public Stream GetFileStreamFromResources(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            return _assembly.GetManifestResourceStream(filePath);
        }

        private FeatureGenerator TestFeatureGenerator()
        {
            return new FeatureGenerator("HumanCorpse2");
        }
    }
}
