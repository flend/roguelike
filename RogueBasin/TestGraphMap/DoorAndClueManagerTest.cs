using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;
using System.Linq;

namespace TestGraphMap
{
    [TestClass]
    public class DoorAndClueManagerTest
    {
        [TestMethod]
        public void DoorsCanBeLockedAndCluePlacedAndRetrieved()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(11, 12, "lock0", 2);

            Door placedDoor = manager.GetDoorForEdge(11, 12);
            Assert.AreEqual("lock0", placedDoor.Id);
            
            var doorIds = manager.GetClueIdForVertex(2).ToList();
            Assert.AreEqual(1, doorIds.Count);
            Assert.AreEqual("lock0", doorIds[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void CluesCantBePlacedBehindTheirOwnDoors()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(9, 10, "lock0", 12);
        }

        [TestMethod]
        public void AddingAClueBehindADoorMeansTheNewDoorDependsOnTheOldDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(11, 13, "lock0", 2);
            manager.PlaceDoorAndClue(5, 6, "lock1", 15);

            Assert.IsNotNull(manager.GetDependencyEdge("lock0", "lock1"));
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void AddingAClueBehindADoorMeansTheOldDoorDoesNotDependsOnTheNewDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(11, 13, "lock0", 2);
            manager.PlaceDoorAndClue(5, 6, "lock1", 15);

            manager.GetDependencyEdge("lock1", "lock0");
        }

        [TestMethod]
        public void AddingTwoCluesBehindADoorMeansTheNewDoorsDependsOnTheOldDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(11, 13, "lock0", 2);
            manager.PlaceDoorAndClue(5, 6, "lock1", 15);
            manager.PlaceDoorAndClue(3, 4, "lock2", 14);

            Assert.IsNotNull(manager.GetDependencyEdge("lock0", "lock1"));
            Assert.IsNotNull(manager.GetDependencyEdge("lock0", "lock2"));
        }

        private DoorAndClueManager BuildStandardManager()
        {
            var standardMap = BuildStandardTestMap();
            var mapNoCycles = new MapCycleReducer(standardMap.RoomConnectionGraph.Edges);
            var mapMST = new MapMST(mapNoCycles.mapNoCycles.Edges);

            return new DoorAndClueManager(mapNoCycles, 1);
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
            GraphvizExport.OutputUndirectedGraph(newMap.RoomConnectionGraph, "door-and-clue-manager-map");

            return newMap;
        }
    }
}
