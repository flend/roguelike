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
            var standardMap = BuildStandardTestMap();
            var mapNoCycles = new MapCycleReducer(standardMap.RoomConnectionGraph.Edges);
            var mapMST = new MapMST(mapNoCycles.mapNoCycles.Edges);

            DoorAndClueManager manager = new DoorAndClueManager(mapNoCycles.mapNoCycles, mapMST, 1);

            manager.PlaceDoorAndClue(standardMap.GetEdgeBetweenRooms(11, 12), "lock0", 2);

            Door placedDoor = manager.GetDoorForEdge(standardMap.GetEdgeBetweenRooms(11, 12));
            Assert.AreEqual("lock0", placedDoor.Id);
            
            var doorIds = manager.GetClueIdForVertex(2).ToList();
            Assert.AreEqual(1, doorIds.Count);
            Assert.AreEqual("lock0", doorIds[0]);
        }

        [TestMethod]
        public void AddingAClueBehindADoorMeansTheNewDoorDependsOnTheOldDoor()
        {
            var standardMap = BuildStandardTestMap();
            var mapNoCycles = new MapCycleReducer(standardMap.RoomConnectionGraph.Edges);
            var mapMST = new MapMST(mapNoCycles.mapNoCycles.Edges);

            DoorAndClueManager manager = new DoorAndClueManager(mapNoCycles.mapNoCycles, mapMST, 1);

            manager.PlaceDoorAndClue(standardMap.GetEdgeBetweenRooms(11, 13), "lock0", 2);
            manager.PlaceDoorAndClue(standardMap.GetEdgeBetweenRooms(5, 6), "lock1", 15);

            var doorLock0Index = manager.GetDoorById("lock0").DoorIndex;
            var doorLock1Index = manager.GetDoorById("lock1").DoorIndex;

            var doorDepGraph = manager.DoorDependencyGraph;

            //Door 1 should depend on Door 0
            QuickGraph.Edge<int> door0ToDoor1Edge;
            doorDepGraph.TryGetEdge(0, 1, out door0ToDoor1Edge);
            Assert.IsNotNull(door0ToDoor1Edge);

            //Not the reverse
            QuickGraph.Edge<int> door1ToDoor0Edge;
            doorDepGraph.TryGetEdge(1, 0, out door1ToDoor0Edge);
            Assert.IsNull(door1ToDoor0Edge);
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
