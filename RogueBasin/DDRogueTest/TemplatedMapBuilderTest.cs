using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace DDRogueTest
{
    [TestClass]
    public class TemplatedMapBuilderTest
    {
        [TestMethod]
        public void TestAddTemplateOnTopWorksWithNoExistingTemplates()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplateOnTop(templatePos1));
        }

        [TestMethod]
        public void AddingOverlappingTemplatesWorksIfOverlapIsTransparent()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            //Start
            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplateOnTop(templatePos1);

            //End
            TemplatePositioned templatePos2 = new TemplatePositioned(-10, 20, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplateOnTop(templatePos2);

            //Middle
            TemplatePositioned templatePos3 = new TemplatePositioned(-8, 30, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplateOnTop(templatePos3);

            //Corridor from start - end that overlaps middle
            var expandedCorridorAndPoint = RoomTemplateUtilities.ExpandCorridorTemplateLShaped(6, 28, true, corridor1);
            var positionedCorridor = new TemplatePositioned(-2, 4, 0, expandedCorridorAndPoint.Item1, TemplateRotation.Deg0, 3);

            Assert.IsTrue(mapGen.AddPositionedTemplateOnTop(positionedCorridor));

        }

        [TestMethod]
        public void MapContainsCorrectIdOnSingleRoom()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 12);
            mapGen.AddPositionedTemplateOnTop(templatePos1);

            Map outputMap = mapGen.MergeTemplatesIntoMap(GetStandardTerrainMapping());

            Assert.IsTrue(outputMap.roomIdMap[0, 0] == 12);
        }

        [TestMethod]
        public void MapContainsCorrectIdOnTwoNonOverlappingRooms()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 1);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(8, 0, 10, room1, TemplateRotation.Deg0, 2);
            mapGen.AddPositionedTemplate(templatePos2);

            Map outputMap = mapGen.MergeTemplatesIntoMap(GetStandardTerrainMapping());

            Assert.IsTrue(outputMap.roomIdMap[0, 0] == 1);
            Assert.IsTrue(outputMap.roomIdMap[8, 0] == 2);
        }

        private Dictionary<RoomTemplateTerrain, MapTerrain> GetStandardTerrainMapping()
        {
            var terrainMapping = new Dictionary<RoomTemplateTerrain, MapTerrain>();
            terrainMapping[RoomTemplateTerrain.Wall] = MapTerrain.Wall;
            terrainMapping[RoomTemplateTerrain.Floor] = MapTerrain.Empty;
            terrainMapping[RoomTemplateTerrain.Transparent] = MapTerrain.Void;
            terrainMapping[RoomTemplateTerrain.WallWithPossibleDoor] = MapTerrain.ClosedDoor;

            return terrainMapping;
        }

        [TestMethod]
        public void TestOverlappingSolidRoomsCantBeAdded()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(0, 0, 10, room1, TemplateRotation.Deg0, 0);
            Assert.IsFalse(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void TestCompletelyOverlappingSolidRoomsCantBeAdded()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(0, 0, 1, room1, TemplateRotation.Deg0, 0);
            Assert.IsFalse(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void TestNonOverlappingSolidRoomsCanBeAdded()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(8, 0, 10, room1, TemplateRotation.Deg0, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException), "Rooms can't be placed at the same Z")]
        public void RoomsCantBePlacedAtSameZ()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testsolid1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(8, 0, 0, room1, TemplateRotation.Deg0, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void RoomsCanBePlacedAtPresetZUsingAddPositionedTemplateOnTop()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testsolid1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(8, 0, 0, room1, TemplateRotation.Deg0, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplateOnTop(templatePos2));
        }

        [TestMethod]
        public void RoomsWithOverlapOnTransparentBoundariesCanBeAdded()
        {
            RoomTemplate room1 = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testsolid1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(9, 0, 1, room1, TemplateRotation.Deg0, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void RoomsAtHighXWhichMergeShouldntOverlap()
        {
            RoomTemplate room1 = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testsolid1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(20, 20, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(21, 21, 1, room1, TemplateRotation.Deg0, 0);
            Assert.IsFalse(mapGen.AddPositionedTemplate(templatePos2));
        }
        
        private RoomTemplate LoadTemplateFromAssemblyFile(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream(filePath);
            return RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
        }

        private static RoomTemplate LoadTemplateFromFileRogueBasin(string filename)
        {
            return RoomTemplateLoader.LoadTemplateFromFile(filename, StandardTemplateMapping.terrainMapping);
        }
    }
}
