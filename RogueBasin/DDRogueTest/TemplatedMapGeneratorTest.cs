﻿using System;
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

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(5, 10));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void TemplatesCanBeReturnedInMapCoords()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(-5, -5));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(5, 10));

            var roomTemplatesInWorldCoords = mapGen.GetRoomTemplatesInWorldCoords();

            Assert.AreEqual(new Point(10, 15), roomTemplatesInWorldCoords[1].Location);
        }

        [TestMethod]
        public void DoorsCanBeReturnedInMapCoords()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(-5, -5));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(5, 10));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var doorsTemplatesInWorldCoords = mapGen.GetDoorsInMapCoords();

            CollectionAssert.AreEquivalent(new Dictionary<Connection, Point>() {
                { new Connection(0, 2), new Point(4, 3) },
                { new Connection(1, 2), new Point(11, 15) }
                }, doorsTemplatesInWorldCoords);
        }

        [TestMethod]
        public void RoomTemplatesCanBeRetrievedById()
        {
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            var room1Id = mapGen.PlaceRoomTemplateAtPosition(room1, new Point(-5, -5));
            TemplatePositioned roomTemplate = mapGen.GetRoomTemplateById(room1Id);

            Assert.AreEqual(new Point(-5, -5), roomTemplate.Location);
        }

        [TestMethod]
        public void AddingSideToSideRoomsGivesConnectedNodeGraph()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(3, 4));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 1) }), allConnections);
        }

        [TestMethod]
        public void AddingSideToSideAllowedOverlappingRoomsGivesConnectedNodeGraph()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(3, 3));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 1) }), allConnections);
        }

        [TestMethod]
        public void RoomTemplateMayBeRetrievedAfterAddingRoom()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            
            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));

            Assert.AreEqual(room1, mapGen.GetRoomTemplatesInWorldCoords()[0].Room);
        }

        [TestMethod]
        public void RoomTemplateMayBeRetrievedAfterAddingAlignedRoom()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(room2, null, mapGen.PotentialDoors[0], 0, 0);

            Assert.AreEqual(room2, mapGen.GetRoomTemplatesInWorldCoords()[1].Room);
        }

        [TestMethod]
        public void AligningARoomWithNoCorridorReturnsTheConnectionToTheNewRoom()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            Assert.AreEqual(new Connection(0, 1), 
                mapGen.PlaceRoomTemplateAlignedWithExistingDoor(room2, null, mapGen.PotentialDoors[0], 0, 0));
        }

                [TestMethod]
        public void AligningARoomWithACorridorReturnsTheConnectionToTheNewRoom()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            Assert.AreEqual(new Connection(1, 2),
                mapGen.PlaceRoomTemplateAlignedWithExistingDoor(room2, corridor1, mapGen.PotentialDoors[0], 0, 10));
        }

        [TestMethod]
        public void RoomTemplateMayBeRetrievedAfterAddingCorridor()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-7, 8));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            //We are only testing that the corridor exists, where it is is tested elsewhere
            Assert.AreNotEqual(null, mapGen.GetRoomTemplatesInWorldCoords()[2].Room);
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
        public void UnconnectedDoorsCanBeReplacedWithOtherTerrain()
        {
            //With 4 doors
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.test4doors.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));

            mapGen.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            //Check the doors are walls when merged
            var map = mapBuilder.MergeTemplatesIntoMap(GetStandardTerrainMapping());
            Assert.AreEqual(map.mapSquares[0, 3].Terrain, MapTerrain.Wall);
            Assert.AreEqual(map.mapSquares[7, 1].Terrain, MapTerrain.Wall);
            Assert.AreEqual(map.mapSquares[7, 0].Terrain, MapTerrain.Wall);
            Assert.AreEqual(map.mapSquares[3, 3].Terrain, MapTerrain.Wall);
        }

        [TestMethod]
        public void ConnectedDoorsCanBeReplacedWithOtherTerrain()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(7, 8));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            mapGen.ReplaceConnectedDoorsWithTerrain(RoomTemplateTerrain.Floor);

            //Check the doors are floor when merged
            var map = mapBuilder.MergeTemplatesIntoMap(GetStandardTerrainMapping());
            Assert.AreEqual(MapTerrain.Empty, map.mapSquares[4, 3].Terrain);
            Assert.AreEqual(MapTerrain.Empty, map.mapSquares[8, 8].Terrain);
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

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-7, 8));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }

        [TestMethod]
        public void PotentialDoorsAreRemovedAfterRoomsAreConnectedByACorridor()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-7, 8));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            Assert.AreEqual(0, mapGen.PotentialDoors.Count);
        }

        [TestMethod]
        public void CorrectPotentialDoorsAreRemovedAfterMultiDoorsRoomsAreConnectedByACorridor()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.test4doors.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.test2doors.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(room2, null, mapGen.PotentialDoors[3], 1, 1);

            var expectedDoors = new List<Point>(new Point[] { new Point(3, 0), new Point(0, 1), new Point(7, 1),
                                                              new Point(6, 6)});
            var actualDoors = mapGen.PotentialDoors.Select(d => d.MapCoords);

            CollectionAssert.AreEquivalent(expectedDoors.ToList(), actualDoors.ToList());
        }

        [TestMethod]
        public void PotentialDoorsAreRemovedAfterRoomsAreConnectedByPlacingSideToSide()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(3, 4));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            Assert.AreEqual(0, mapGen.PotentialDoors.Count);
        }

        [TestMethod]
        public void PotentialDoorsAreRemovedAfterRoomsAreAligned()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(room2, null, mapGen.PotentialDoors[0], 0, 1);

            Assert.AreEqual(0, mapGen.PotentialDoors.Count);
        }

        [TestMethod]
        public void AligningOverlappingRoomsOnADoorRemovesBothDoors()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(room2, null, mapGen.PotentialDoors[0], 0, 0);

            Assert.AreEqual(0, mapGen.PotentialDoors.Count);
        }

        [TestMethod]
        public void ConnectingDoorBetweenTwoOverlappingRoomsCanBeIdentified()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(room2, null, mapGen.PotentialDoors[0], 0, 0);

            var connectingDoor = mapGen.GetDoorForConnection(new Connection(0, 1));

            Assert.AreEqual(new Point(4, 3), connectingDoor.MapCoords);
        }

        [TestMethod]
        public void ConnectingDoorsBetweenTwoRoomsAlignedWithACorridorCanBeIdentified()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(room2, corridor1, mapGen.PotentialDoors[0], 0, 5);

            var room1ToCorridorDoor = mapGen.GetDoorForConnection(new Connection(0, 2));
            var room2ToCorridorDoor = mapGen.GetDoorForConnection(new Connection(1, 2));

            Assert.AreEqual(new Point(4, 3), room1ToCorridorDoor.MapCoords);
            Assert.AreEqual(new Point(4, 8), room2ToCorridorDoor.MapCoords);
        }

        [TestMethod]
        public void ConnectingDoorsBetweenTwoRoomsJoinedWithACorridorCanBeIdentified()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(10, -8));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var room1ToCorridorDoor = mapGen.GetDoorForConnection(new Connection(0, 2));
            var room2ToCorridorDoor = mapGen.GetDoorForConnection(new Connection(1, 2));

            Assert.AreEqual(new Point(1, 0), room1ToCorridorDoor.MapCoords);
            Assert.AreEqual(new Point(14, -5), room2ToCorridorDoor.MapCoords);
        }

        [TestMethod]
        public void ConnectingDoorsBetweenTwoRoomsJoinedWithACorridorButAdjacentCanBeIdentified()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate room2 = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");
            RoomTemplate corridor1 = LoadTemplateFromFileRogueBasin("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-7, 1));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var room1ToRoom2Door = mapGen.GetDoorForConnection(new Connection(0, 1));

            Assert.AreEqual(new Point(0, 2), room1ToRoom2Door.MapCoords);
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

            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(10, -8));

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

            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(-10, -8));

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

            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(-3, -8));

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

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-15, -10));

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

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-15, 1));

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

            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-15, -10));
            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));

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

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-15, 10));

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

            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(25, 10));

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

            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(25, -12));

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

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(room2, null, mapGen.PotentialDoors[0], 0, 1);

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

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(room2, corridor1, mapGen.PotentialDoors[0], 0, 2);

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

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(15, 10));

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

            mapGen.PlaceRoomTemplateAtPosition(room1, new Point(0, 0));
            mapGen.PlaceRoomTemplateAtPosition(room2, new Point(-15, 10));

            bool corridorPlacement = mapGen.JoinDoorsWithCorridor(mapGen.PotentialDoors[0], mapGen.PotentialDoors[1], corridor1);

            var connectivityMap = mapGen.ConnectivityMap;
            var allConnections = connectivityMap.GetAllConnections().Select(c => c.Ordered).ToList();

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(0, 2), new Connection(1, 2) }), allConnections);
        }
        
        [TestMethod]
        public void RoomCanBeReplacedWithSmallerRoomsWithAlignmentCorrected()
        {
            //Load sample template 8x4
            RoomTemplate baseRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate joinedRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");

            RoomTemplate replacementRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testoverlap2.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(baseRoom, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(joinedRoom, null, mapGen.PotentialDoors[0], 0, 0);
            
            var replaceSuccess = mapGen.ReplaceRoomTemplate(0, new Connection(0,1), replacementRoom, 0);

            //Check replacement was successful and terrain is correct
            var map = mapBuilder.MergeTemplatesIntoMap(GetStandardTerrainMapping());
            Assert.AreEqual(MapTerrain.Wall, map.mapSquares[4, 1].Terrain);
        }

        [TestMethod]
        public void ExcessAreaFromReplacedRoomsAreRemovedWhenSmallerRoomReplacesThem()
        {
            //Load sample template 8x4
            RoomTemplate baseRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate joinedRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");

            RoomTemplate replacementRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testoverlap2.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(baseRoom, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(joinedRoom, null, mapGen.PotentialDoors[0], 0, 0);

            mapGen.ReplaceRoomTemplate(0, new Connection(0, 1), replacementRoom, 0);

            var map = mapBuilder.MergeTemplatesIntoMap(GetStandardTerrainMapping());
            Assert.AreEqual(MapTerrain.Void, map.mapSquares[0, 0].Terrain);
        }

        [TestMethod]
        public void RoomCantBeReplacedWithLargeRooms()
        {
            //Load sample template 8x4
            RoomTemplate baseRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate joinedRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");

            RoomTemplate replacementRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testoverlap3.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(baseRoom, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(joinedRoom, null, mapGen.PotentialDoors[0], 0, 0);

            Assert.IsFalse(mapGen.ReplaceRoomTemplate(0, new Connection(0, 1), replacementRoom, 0));
        }

        [TestMethod]
        public void ReplaceRoomsHaveTheSameIds()
        {
            //Load sample template 8x4
            RoomTemplate baseRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate joinedRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");

            RoomTemplate replacementRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testoverlap2.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            var originalId = mapGen.PlaceRoomTemplateAtPosition(baseRoom, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(joinedRoom, null, mapGen.PotentialDoors[0], 0, 0);

            var placementSucess = mapGen.ReplaceRoomTemplate(0, new Connection(0, 1), replacementRoom, 0);

            var roomTemplateById = mapGen.GetRoomTemplateById(originalId).Room;

            //This is a bit weak as an integration test - the replacement room gets rotated hence the strange comparison
            Assert.AreEqual(replacementRoom.Height, roomTemplateById.Width);
        }

        [TestMethod]
        public void RoomReplacedWithSmallRoomsCantThenBeMadeLargeAgain()
        {
            //Load sample template 8x4
            RoomTemplate baseRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate joinedRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");

            RoomTemplate replacementRoom = LoadTemplateFromFile("DDRogueTest.testdata.vaults.testoverlap2.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            mapGen.PlaceRoomTemplateAtPosition(baseRoom, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(joinedRoom, null, mapGen.PotentialDoors[0], 0, 0);

            mapGen.ReplaceRoomTemplate(0, new Connection(0, 1), replacementRoom, 0);

            //Here we use the knowledge that we deleted index 0, so the new replacement room is at index 1
            Assert.IsFalse(mapGen.ReplaceRoomTemplate(0, new Connection(0, 1), baseRoom, 0));
        }

        private static RoomTemplate LoadTemplateFromFile(string filename)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream _fileStream = _assembly.GetManifestResourceStream(filename);

            return new RoomTemplateLoader(_fileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
        }

        private static RoomTemplate LoadTemplateFromFileRogueBasin(string filename)
        {
            return new RoomTemplateLoader(filename, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
        }
    }
}
