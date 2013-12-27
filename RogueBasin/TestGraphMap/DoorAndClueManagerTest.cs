using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;
using System.Linq;
using System.Collections.Generic;

namespace TestGraphMap
{
    [TestClass]
    public class DoorAndClueManagerTest
    {
        [TestMethod]
        public void DoorsCanBeLockedAndCluePlacedAndRetrieved()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new Connection(11, 12), "lock0", 2);

            Door placedDoor = manager.GetDoorForEdge(new Connection(11, 12));
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

            manager.PlaceDoorAndClue(new Connection(9, 10), "lock0", 12);
        }

        [TestMethod]
        public void AddingAClueBehindADoorMeansTheNewDoorDependsOnTheOldDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new Connection(11, 13), "lock0", 2);
            manager.PlaceDoorAndClue(new Connection(5, 6), "lock1", 15);

            Assert.IsNotNull(manager.GetDependencyEdge("lock0", "lock1"));
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void AddingAClueBehindADoorMeansTheOldDoorDoesNotDependsOnTheNewDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new Connection(11, 13), "lock0", 2);
            manager.PlaceDoorAndClue(new Connection(5, 6), "lock1", 15);

            manager.GetDependencyEdge("lock1", "lock0");
        }

        [TestMethod]
        public void AddingTwoCluesBehindADoorMeansTheNewDoorsDependsOnTheOldDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new Connection(11, 13), "lock0", 2);
            manager.PlaceDoorAndClue(new Connection(5, 6), "lock1", 15);
            manager.PlaceDoorAndClue(new Connection(3, 4), "lock2", 14);

            //Maybe deprecate?
            Assert.IsNotNull(manager.GetDependencyEdge("lock0", "lock1"));
            Assert.IsNotNull(manager.GetDependencyEdge("lock0", "lock2"));
        }

        [TestMethod]
        public void AddingTwoCluesBehindADoorMeansThereAreTwoDependenciesOnDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new Connection(11, 13), "lock0", 2);
            manager.PlaceDoorAndClue(new Connection(5, 6), "lock1", 15);
            manager.PlaceDoorAndClue(new Connection(3, 4), "lock2", 14);

            List<string> dependentDoors = manager.GetDependentDoorIds("lock0");

            CollectionAssert.AreEquivalent(new List<string>(new string[] { "lock1", "lock2" }), dependentDoors);
        }

        [TestMethod]
        public void AddingACluesNotBehindADoorMeansThereAreNoDependentDoors()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new Connection(11, 13), "lock0", 2);
            manager.PlaceDoorAndClue(new Connection(5, 6), "lock1", 3);

            List<string> dependentDoors = manager.GetDependentDoorIds("lock0");

            CollectionAssert.AreEquivalent(new List<string>(), dependentDoors);
        }

        [TestMethod]
        public void LockingAClueWithANewDoorMakesTheOldDoorDependentOnTheNewDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new Connection(5, 6), "lock0", 13);
            manager.PlaceDoorAndClue(new Connection(11, 13), "lock1", 3);

            List<string> dependentDoors = manager.GetDependentDoorIds("lock1");
            CollectionAssert.AreEquivalent(new List<string>(new string[] { "lock0" }), dependentDoors);

            List<string> noDependentDoors = manager.GetDependentDoorIds("lock0");
            CollectionAssert.AreEquivalent(new List<string>(), noDependentDoors);
        }

        [TestMethod]
        public void CheckAllRoomsExceptThoseLockedByThisClueAreValidToPlaceClueWithNoDependencies()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new Connection(5, 6), "lock0", 4);

            var validRooms = manager.GetValidRoomsToPlaceClue(new Connection(10, 11)).ToList();

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1,2,3,4,5,6,10 }), validRooms);
        }

        [TestMethod]
        public void CheckRoomsBehindLockedClueAreNotValidToPlaceNewClue()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new Connection(3, 5), "lock0", 15);

            var validRooms = manager.GetValidRoomsToPlaceClue(new Connection(11, 13)).ToList();

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 10, 11, 12 }), validRooms);
        }

        [TestMethod]
        public void CheckRoomsBehindDependenciesLockedClueAreNotValidToPlaceNewClue()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new Connection(10, 11), "lock0", 6);
            //Make lock0 depend on lock1
            manager.PlaceDoorAndClue(new Connection(3, 5), "lock1", 4);
            
            //This would cover clue1. Therefore lock1 rooms are not accessible, which includes clue0
            //Therefore lock0 rooms are not accessible
            var validRooms = manager.GetValidRoomsToPlaceClue(new Connection(3, 4)).ToList();

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 10 }), validRooms);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void RequestingDependentDoorsForADoorIdThatDoesntExistThrowsAnException()
        {
            var manager = BuildStandardManager();
            List<string> dependentDoors = manager.GetDependentDoorIds("lock0");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void RequestingDependentDoorsForADoorIndexThatDoesntExistThrowsAnException()
        {
            var manager = BuildStandardManager();
            var dependentDoors = manager.GetDependentDoorIndices(0);

            CollectionAssert.AreEquivalent(new List<string>(), dependentDoors);
        }

        
        [TestMethod]
        public void FullyOpenMapIsFullyAccessibleWithNoClues()
        {
            var manager = BuildStandardManager();

            var validRooms = manager.GetAccessibleVerticesWithClues(new List<Clue>());

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10, 11, 12, 13, 14, 15 }), validRooms.ToList());
        }

        [TestMethod]
        public void MapWithOneLockIsPartiallyAccessibleWithNoClues()
        {
            var manager = BuildStandardManager();
            manager.PlaceDoorAndClue(new Connection(10, 11), "lock0", 6);

            var validRooms = manager.GetAccessibleVerticesWithClues(new List<Clue>());

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10 }), validRooms.ToList());
        }

        [TestMethod]
        public void MapWithOneLockIsFullyAccessibleWithRightClue()
        {
            var manager = BuildStandardManager();
            var clue0 = manager.PlaceDoorAndClue(new Connection(10, 11), "lock0", 6);

            var validRooms = manager.GetAccessibleVerticesWithClues(new List<Clue>(new Clue [] { clue0 }));

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10, 11, 12, 13, 14, 15 }), validRooms.ToList());
        }

        [TestMethod]
        public void MapWithTwoLocksIsPartiallyAccessibleWithOneClue()
        {
            var manager = BuildStandardManager();
            var clue0 = manager.PlaceDoorAndClue(new Connection(10, 11), "lock0", 6);
            var clue1 = manager.PlaceDoorAndClue(new Connection(5, 6), "lock1", 1);

            var validRooms = manager.GetAccessibleVerticesWithClues(new List<Clue>(new Clue[] { clue0 }));

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 10, 11, 12, 13, 14, 15 }), validRooms.ToList());
        }

        [TestMethod]
        public void MapRequiringTwoLocksIsFullyAccessibleWithTwoCorrectClues()
        {
            var manager = BuildStandardManager();
            manager.LockDoor(new DoorRequirements(new Connection(10, 11), "lock0", 2));
            var clue0 = manager.PlaceClue(5, "lock0");
            var clue1 = manager.PlaceClue(6, "lock0");

            var validRooms = manager.GetAccessibleVerticesWithClues(new List<Clue>(new Clue[] { clue0, clue1 }));

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10, 11, 12, 13, 14, 15 }), validRooms.ToList());
        }

        [TestMethod]
        public void MapRequiringTwoLocksIsNotFullyAccessibleWithOnlyOneCorrectClue()
        {
            var manager = BuildStandardManager();
            manager.LockDoor(new DoorRequirements(new Connection(10, 11), "lock0", 2));
            var clue0 = manager.PlaceClue(5, "lock0");

            var validRooms = manager.GetAccessibleVerticesWithClues(new List<Clue>(new Clue[] { clue0 }));

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10 }), validRooms.ToList());
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
