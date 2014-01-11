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
        public void AddingAVerticalCorridorBetweenTwoRoomsGivesConnectedNodeGraphBL()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAtPosition(room2, new Point(5, 10));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void AddingAVerticalCorridorBetweenTwoRoomsGivesConnectedNodeGraphBR()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-7, 8));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void AddingAVerticalCorridorBetweenTwoRoomsGivesConnectedNodeGraphTR()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room2, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(10, -8));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void AddingAVerticalCorridorBetweenTwoRoomsGivesConnectedNodeGraphTL()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room2, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(-10, -8));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void AddingAStraightVerticalCorridorBetweenTwoRoomsGivesConnectedNodeGraphTL()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room2, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(-3, -8));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void AddingAHorizontalCorridorBetweenTwoRoomsGivesConnectedNodeGraphTL()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-15, -10));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void AddingAStraightHorizontalCorridorBetweenTwoRoomsGivesConnectedNodeGraphTL()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-15, 1));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void AddingAHorizontalCorridorBetweenTwoRoomsGivesConnectedNodeGraphTLReversed()
        {
            //Simple of test of reversing placement order invariance

            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement2 = mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-15, -10));
            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[1], mapGen.PotentialDoors[0], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void AddingAHorizontalCorridorBetweenTwoRoomsGivesConnectedNodeGraphBL()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-15, 10));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void AddingAHorizontalCorridorBetweenTwoRoomsGivesConnectedNodeGraphBR()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room2, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(25, 10));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void AddingAHorizontalCorridorBetweenTwoRoomsGivesConnectedNodeGraphTR()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room2, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(25, -12));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void AddingTwoAlignedRoomsGiveTwoConnectedNodeGraph()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");

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
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

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

        [TestMethod]
        public void AddingALCorridorBetweenTwoRoomsGivesConnectedNodeGraphBR()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAtPosition(room2, new Point(15, 10));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void AddingALCorridorBetweenTwoRoomsGivesConnectedNodeGraphBL()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            bool placement1 = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            bool placement2 = mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-15, 10));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        private static RoomTemplate LoadTemplateFromFile(string filename)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream _fileStream = _assembly.GetManifestResourceStream(filename);

            return RoomTemplateLoader.LoadTemplateFromFile(_fileStream, StandardTemplateMapping.terrainMapping);
        }

        private static RoomTemplate LoadTemplateFromFileRogueBasin(string filename)
        {
            return RoomTemplateLoader.LoadTemplateFromFile(filename, StandardTemplateMapping.terrainMapping);
        }
    }
}
