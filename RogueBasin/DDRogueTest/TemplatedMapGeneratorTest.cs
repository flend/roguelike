using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using RogueBasin;
using System.IO;
using GraphMap;
using System.Linq;
using System.Collections.Generic;

namespace DDRogueTest
{
    [TestClass]
    public class TemplatedMapGeneratorTest
    {
        [TestMethod]
        public void AddingTwoAlignedRoomsGiveTwoConnectedNodeGraph()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
            roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate room2 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAlignedWithExistingDoor(room2, null, mapGen.PotentialDoors[0], 1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 1) }), allConnections);
        }

        [TestMethod]
        public void AddingTwoAlignedRoomsWithACorridorGivesThreeConnectedNodeGraph()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
            roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate room2 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAlignedWithExistingDoor(room2, corridor1, mapGen.PotentialDoors[0], 2);

            var connectivityMap = mapGen.ConnectivityMap;

            //Ensure all connections are ordered for comparison purposes
            //It's a bit yuck having to know how indexes are set. I should probably refactor to throw exceptions rather than pass back false
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();
            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }
    }
}
