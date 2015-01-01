using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;
using System.Collections.Generic;

namespace TestGraphMap
{
    [TestClass]
    public class DoorTest
    {
        [TestMethod]
        public void DoorCanBeUnlockedWithAMatchedClue()
        {
            var testDoor = new Door(new QuickGraph.TaggedEdge<int, string>(0, 1, "test"), new Connection(0, 1), new Connection(0, 1), "door1", 0, 1);
            var clue0 = new Clue(testDoor, new List<int>());

            Assert.IsTrue(testDoor.CanDoorBeUnlockedWithClues(new List<Clue>(new Clue[] { clue0 })));
        }

        [TestMethod]
        public void MultipleLockDoorCanBeUnlockedWithEnoughClues()
        {
            var testDoor = new Door(new QuickGraph.TaggedEdge<int, string>(0, 1, "test"), new Connection(0, 1), new Connection(0, 1), "door1", 0, 2);
            var clue0 = new Clue(testDoor, new List<int>());
            var clue1 = new Clue(testDoor, new List<int>());

            Assert.IsTrue(testDoor.CanDoorBeUnlockedWithClues(new List<Clue>(new Clue[] { clue0, clue1 })));
        }

        [TestMethod]
        public void DoorCantBeUnlockedWithNotEnoughClues()
        {
            var testDoor = new Door(new QuickGraph.TaggedEdge<int, string>(0, 1, "test"), new Connection(0, 1), new Connection(0, 1), "door1", 0, 2);
            var clue0 = new Clue(testDoor, new List<int>());

            Assert.IsFalse(testDoor.CanDoorBeUnlockedWithClues(new List<Clue>(new Clue[] { clue0 })));
        }

        [TestMethod]
        public void DoorCantBeUnlockedWithTheWrongClues()
        {
            var testDoor = new Door(new QuickGraph.TaggedEdge<int, string>(0, 1, "test"), new Connection(0, 1), new Connection(0, 1), "door1", 0, 1);
            //door index is important here
            var testDoor2 = new Door(new QuickGraph.TaggedEdge<int, string>(0, 1, "test"), new Connection(0, 1), new Connection(0, 1), "door2", 1, 1);
            var clue0 = new Clue(testDoor2, new List<int>());

            Assert.IsFalse(testDoor.CanDoorBeUnlockedWithClues(new List<Clue>(new Clue[] { clue0 })));
        }

        [TestMethod]
        public void DoorKnowsItsLocationOnNonReducedMap()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            var doorManager = mapModel.DoorAndClueManager;

            doorManager.PlaceDoorAndClue(new DoorRequirements(new Connection(2, 10), "lock0"), 2);
            var door = doorManager.GetDoorById("lock0");

            Assert.AreEqual(new Connection(9, 10), door.DoorConnectionFullMap);
        }

        private ConnectivityMap BuildStandardTestMap()
        {
            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Branch

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(3, 5);
            newMap.AddRoomConnection(5, 6);

            //Cycle

            newMap.AddRoomConnection(2, 8);
            newMap.AddRoomConnection(2, 7);
            newMap.AddRoomConnection(8, 9);
            newMap.AddRoomConnection(7, 9);

            //Post-cycle

            newMap.AddRoomConnection(9, 10);
            newMap.AddRoomConnection(10, 11);
            newMap.AddRoomConnection(11, 12);
            newMap.AddRoomConnection(11, 13);

            //2-way branches
            newMap.AddRoomConnection(13, 14);
            newMap.AddRoomConnection(13, 15);

            //Save to disk
            GraphvizExport.OutputUndirectedGraph(newMap.RoomConnectionGraph, "standard-test-map");

            return newMap;
        }
    }
}
