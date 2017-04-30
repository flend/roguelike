﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos1));
        }

        [TestMethod]
        public void AddingOverlappingTemplatesWorksIfOverlapIsTransparent()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            //Start
            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            //End
            TemplatePositioned templatePos2 = new TemplatePositioned(-10, 20, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos2);

            //Middle
            TemplatePositioned templatePos3 = new TemplatePositioned(-8, 30, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos3);

            //Corridor from start - end that overlaps middle
            var expandedCorridorAndPoint = RoomTemplateUtilities.ExpandCorridorTemplateLShaped(6, 28, true, corridor1);
            var positionedCorridor = new TemplatePositioned(-2, 4, 0, expandedCorridorAndPoint.Item1, 3);

            Assert.IsTrue(mapGen.AddPositionedTemplate(positionedCorridor));
        }

        [TestMethod]
        public void AddingOverlappingTemplatesWorksIfOnlyOverlapIsOnWallsAndDoors()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.test4doors.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            //Base
            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            //Overlapping by wall and door only
            TemplatePositioned templatePos2 = new TemplatePositioned(7, 0, 0, room1, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void AddingOverlappingTemplatesDoesntWorkIfOOverlapBetweenWallAndDoor()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.test4doors.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            //Base
            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            //Overlapping by wall and door only
            TemplatePositioned templatePos2 = new TemplatePositioned(7, 1, 0, room1, 0);
            Assert.IsFalse(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void TemplateCantBeOverlappedUsingOverrideTemplateIfSecondTemplateTriesToReplacesNonFloorTiles()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.test4doors.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            Stream overlapFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testoverlap.room");
            RoomTemplate room2 = new RoomTemplateLoader(overlapFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            //Base
            TemplatePositioned templatePos1 = new TemplatePositioned(5, 5, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            //Overlap in smaller room
            TemplatePositioned templatePos2 = new TemplatePositioned(5, 5, 0, room2, 0);
            Assert.IsFalse(mapGen.OverridePositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void TemplateCanBeOverlappedUsingUnconditionallyOverrideTemplateWhenTerrainTypesDontMatch()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.test4doors.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            Stream overlapFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testoverlap.room");
            RoomTemplate room2 = new RoomTemplateLoader(overlapFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            //Base
            TemplatePositioned templatePos1 = new TemplatePositioned(5, 5, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            //Overlap in smaller room
            TemplatePositioned templatePos2 = new TemplatePositioned(5, 5, 0, room2, 0);
            mapGen.UnconditionallyOverridePositionedTemplate(templatePos2);

            Map outputMap = mapGen.MergeTemplatesIntoMap(GetStandardTerrainMapping());

            Assert.AreEqual(MapTerrain.Wall, outputMap.mapSquares[3, 0].Terrain);
            Assert.AreEqual(MapTerrain.ClosedDoor, outputMap.mapSquares[0, 1].Terrain);
        }

        [TestMethod]
        public void TemplateCanBeOverlappedUsingOverrideTemplateIfSecondTemplateOnlyReplacesNonFloorTiles()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            Stream overlapFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testoverlap.room");
            RoomTemplate room2 = new RoomTemplateLoader(overlapFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            //Base
            TemplatePositioned templatePos1 = new TemplatePositioned(5, 5, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            //Overlap in smaller room
            TemplatePositioned templatePos2 = new TemplatePositioned(5, 6, 0, room2, 0);
            mapGen.OverridePositionedTemplate(templatePos2);

            Map outputMap = mapGen.MergeTemplatesIntoMap(GetStandardTerrainMapping());

            Assert.AreEqual(MapTerrain.ClosedDoor, outputMap.mapSquares[0, 2].Terrain);
            Assert.AreEqual(MapTerrain.ClosedDoor, outputMap.mapSquares[3, 2].Terrain);
        }

        [TestMethod]
        public void TemplateCanBeOverlappedByItself()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            //Base
            TemplatePositioned templatePos1 = new TemplatePositioned(5, 5, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            //Overlap the same room as a limit test
            Assert.IsTrue(mapGen.OverridePositionedTemplate(templatePos1));
        }

        [TestMethod]
        public void MapContainsCorrectIdOnSingleRoom()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 12);
            mapGen.AddPositionedTemplate(templatePos1);

            Map outputMap = mapGen.MergeTemplatesIntoMap(GetStandardTerrainMapping());

            Assert.IsTrue(outputMap.roomIdMap[0, 0] == 12);
        }

        [TestMethod]
        public void MapContainsDefaultIdForTransparentPartsOfRoom()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testtransparent1.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 12);
            mapGen.AddPositionedTemplate(templatePos1);

            Map outputMap = mapGen.MergeTemplatesIntoMap(GetStandardTerrainMapping());

            Assert.IsTrue(outputMap.roomIdMap[0, 0] == TemplatedMapBuilder.defaultId);
            Assert.IsTrue(outputMap.roomIdMap[1, 1] == 12);
        }

        [TestMethod]
        public void MapContainsCorrectIdOnTwoNonOverlappingRooms()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 1);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(8, 0, 10, room1, 2);
            mapGen.AddPositionedTemplate(templatePos2);

            Map outputMap = mapGen.MergeTemplatesIntoMap(GetStandardTerrainMapping());

            Assert.IsTrue(outputMap.roomIdMap[0, 0] == 1);
            Assert.IsTrue(outputMap.roomIdMap[8, 0] == 2);
        }

        [TestMethod]
        public void TerrainAtPointCanBeOverriden()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 1);
            mapGen.AddPositionedTemplate(templatePos1);

            mapGen.AddOverrideTerrain(new Point(0, 0), RoomTemplateTerrain.Floor);

            Map outputMap = mapGen.MergeTemplatesIntoMap(GetStandardTerrainMapping());

            Assert.AreEqual(MapTerrain.Empty, outputMap.mapSquares[0, 0].Terrain);
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
        public void AddingOverlappingTemplatesWorksIfOnlyOverlapIsOnACorner()
        {
            //Completely overlapping rooms cause problems with door removal etc. so they can't be allowed
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(7, 3, 10, room1, 0);
            var addResult = mapGen.AddPositionedTemplate(templatePos2);
            Assert.IsTrue(addResult);
        }

        [TestMethod]
        public void AddingOverlappingTemplatesWorksIfTwoRowOverlapWithOneRowWall()
        {
            //Completely overlapping rooms cause problems with door removal etc. so they can't be allowed
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(6, 2, 10, room1, 0);
            var addResult = mapGen.AddPositionedTemplate(templatePos2);
            Assert.IsFalse(addResult);
        }

        [TestMethod]
        public void TestOverlappingSolidRoomsReturnFalseWhenTestedForOverlap()
        {
            //Completely overlapping rooms cause problems with door removal etc. so they can't be allowed
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(0, 0, 10, room1, 0);
            Assert.IsFalse(mapGen.CanBePlacedWithoutOverlappingOtherTemplates(templatePos2));
        }

        [TestMethod]
        public void RoomsWhichOverlapOnFloorWallCantBeAdded()
        {
            //Completely overlapping rooms cause problems with door removal etc. so they can't be allowed
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(1, 0, 10, room1, 0);
            Assert.IsFalse(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void TestNonOverlappingSolidRoomsCanBeAdded()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(8, 0, 10, room1, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void TestNonOverlappingSolidRoomsCanBeCheckedToBeAllowedToBePlaced()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(8, 0, 10, room1, 0);
            Assert.IsTrue(mapGen.CanBePlacedWithoutOverlappingOtherTemplates(templatePos2));
        }

        [TestMethod]
        public void RoomsCanBePlacedAtPresetZUsingAddPositionedTemplate()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testsolid1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(8, 0, 0, room1, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void RoomsWithOverlapOnTransparentBoundariesCanBeAdded()
        {
            RoomTemplate room1 = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testsolid1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(9, 0, 1, room1, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void RoomsAtHighXWhichMergeShouldntOverlap()
        {
            RoomTemplate room1 = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testsolid1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(20, 20, 0, room1, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(21, 21, 1, room1, 0);
            Assert.IsFalse(mapGen.AddPositionedTemplate(templatePos2));
        }
        
        private RoomTemplate LoadTemplateFromAssemblyFile(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream(filePath);
            return new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
        }

        private static RoomTemplate LoadTemplateFromFileRogueBasin(string filename)
        {
            return new RoomTemplateLoader(filename, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
        }
    }
}
