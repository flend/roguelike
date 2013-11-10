using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Reflection;
using System.IO;

namespace DDRogueTest
{
    [TestClass]
    public class TemplatedMapBuilderTest
    {
        [TestMethod]
        public void TestOverlappingSolidRoomsCantBeAdded()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(0, 0, 10, room1, TemplateRotation.Deg0);
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

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(8, 0, 10, room1, TemplateRotation.Deg0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void RoomsWithOverlapOnTransparentBoundariesCanBeAdded()
        {
            //Load sample template 10x6 (1 row transparent border around)
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testtransparent1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(9, 0, 1, room1, TemplateRotation.Deg0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void AlignedRoomsCanBeConnectedWithStraightCorridors()
        {
            //Load sample templates
            RoomTemplate corridorDoorRoom = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            mapGen.AddCorridorBetweenPoints(new Point(0, 0), new Point(0, 2), 0, corridorDoorRoom);

            /*Map outputMap = mapGen.GenerateMap();

            Map testMap = new Map(3, 3);
            testMap.MapSquares[0, 0].Terrain = MapTerrain.Wall;
            testMap.MapSquares[1, 0].Terrain = MapTerrain.Empty;
            testMap.MapSquares[2, 0].Terrain = MapTerrain.Wall;

            testMap.MapSquares[0, 1].Terrain = MapTerrain.Wall;
            testMap.MapSquares[1, 1].Terrain = MapTerrain.Empty;
            testMap.MapSquares[2, 1].Terrain = MapTerrain.Wall;

            testMap.MapSquares[0, 2].Terrain = MapTerrain.Wall;
            testMap.MapSquares[1, 2].Terrain = MapTerrain.Empty;
            testMap.MapSquares[2, 2].Terrain = MapTerrain.Wall;
             * */

            Assert.IsTrue(false);

            //actually we should test the corridor -> room generator function like this, instead of testing addcorridor
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException), "Corridors must be straight")]
        public void CorridorsMustBeStraight()
        {
            RoomTemplate corridorDoorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();
            mapGen.AddCorridorBetweenPoints(new Point(0, 0), new Point(1, 2), 0, corridorDoorTemplate);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException), "Corridors must be straight")]
        public void CorridorsTemplatesMustBeHeightOne()
        {
            RoomTemplate corridorDoorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testtransparent1.room"); //8x4

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();
            mapGen.AddCorridorBetweenPoints(new Point(0, 0), new Point(1, 2), 0, corridorDoorTemplate);
        }

        private RoomTemplate LoadTemplateFromAssemblyFile(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream(filePath);
            return RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
        }
    }
}
