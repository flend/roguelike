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

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 12), "lock0"), 2);

            Door placedDoor = manager.GetDoorsForEdge(new Connection(11, 12)).First();
            Assert.AreEqual("lock0", placedDoor.Id);
            
            var doorIds = manager.GetClueIdForVertex(2).ToList();
            Assert.AreEqual(1, doorIds.Count);
            Assert.AreEqual("lock0", doorIds[0]);
        }

        [TestMethod]
        public void GetDoorForEdgeWorksInEitherOrientation()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 12), "lock0"), 2);

            Assert.IsNotNull(manager.GetDoorsForEdge(new Connection(12, 11)).First());
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void CluesCantBePlacedBehindTheirOwnDoors()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(9, 10), "lock0"), 12);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void CluesCantBePlacedInRecursiveLockingSituations()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(9, 10), "lock0"), 6);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock1"), 13);

        }

        [TestMethod]
        public void CluesCanBePlacedInRoomIfOnlyOneRoomPossible()
        {
            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);

            var mapNoCycles = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);
            var mapMST = new MapMST(mapNoCycles.mapNoCycles.Edges);
            var manager = new DoorAndClueManager(mapNoCycles, 1);

            Assert.IsNotNull(manager.PlaceDoorAndClue(new DoorRequirements(new Connection(1, 2), "lock0"), 1));
        }

        [TestMethod]
        public void AddingAnObjectiveAsAClueForAParentObjectiveMakesTheParentDependOnTheChild()
        {
            var manager = BuildStandardManager();

            manager.PlaceObjective(new ObjectiveRequirements(2, "obj0", 1));
            manager.PlaceObjective(new ObjectiveRequirements(2, "obj1", 1, new List<string>{"obj0"}));

            Assert.IsTrue(manager.IsLockDependentOnParentLock("obj0", "obj1"));
        }

        [TestMethod]
        public void AddingAnObjectiveAsAClueForMultipleParentObjectiveMakesTheParentsDependOnTheChild()
        {
            var manager = BuildStandardManager();

            manager.PlaceObjective(new ObjectiveRequirements(2, "obj0", 1));
            manager.PlaceObjective(new ObjectiveRequirements(3, "obj1", 1));
            manager.PlaceObjective(new ObjectiveRequirements(4, "obj2", 1, new List<string> { "obj0", "obj1" }));

            Assert.IsTrue(manager.IsLockDependentOnParentLock("obj0", "obj2"));
            Assert.IsTrue(manager.IsLockDependentOnParentLock("obj1", "obj2"));
        }

        [TestMethod]
        public void AddingAnObjectiveAsAClueForADoorMakesTheDoorDependOnTheObjective()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoor(new DoorRequirements(new Connection(11, 13), "lock0"));
            manager.PlaceObjective(new ObjectiveRequirements(2, "obj1", 1, new List<string> { "lock0" }));

            Assert.IsTrue(manager.IsLockDependentOnParentLock("lock0", "obj1"));
        }

        [TestMethod]
        public void AddingAClueBehindADoorMeansTheNewDoorDependsOnTheOldDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock0"), 2);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock1"), 15);

            Assert.IsTrue(manager.IsLockDependentOnParentLock("lock1", "lock0"));
        }

        [TestMethod]
        public void AddingAClueBehindADoorMeansTheOldDoorDoesNotDependsOnTheNewDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock0"), 2);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock1"), 15);

            Assert.IsFalse(manager.IsLockDependentOnParentLock("lock0", "lock1"));
        }

        [TestMethod]
        public void AddingTwoCluesBehindADoorMeansTheNewDoorsDependsOnTheOldDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock0"), 2);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock1"), 15);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(3, 4), "lock2"), 14);

            Assert.IsTrue(manager.IsLockDependentOnParentLock("lock1", "lock0"));
            Assert.IsTrue(manager.IsLockDependentOnParentLock("lock2", "lock0"));
        }
        
        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void CluesCannotBeAddedToExistingDoorsIfTheClueIsBehindTheDoorItself()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock0"), 6);

            manager.AddCluesToExistingDoor("lock0", new List<int> { 15 });
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ObjectivesCannotBeAddedToExistingDoorsIfTheObjectiveIsBehindTheDoorItself()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock0"), 6);
            manager.PlaceObjective(new ObjectiveRequirements(15, "obj0", 1, new List<string> { "lock0" }));
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void CluesCannotBeAddedToExistingDoorsIfTheyArePlacedBehindDoorsWeDependOn()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock0"), 6);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock1"), 2);

            manager.AddCluesToExistingDoor("lock1", new List<int>{ 15 } );
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ObjectivesCannotBeAddedToExistingDoorsIfTheyArePlacedBehindDoorsWeDependOn()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock0"), 6);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock1"), 2);

            manager.PlaceObjective(new ObjectiveRequirements(15, "obj1", 1, new List<string> { "lock1" }));
        }

        [TestMethod]
        public void CluesCanBeAddedToExistingDoorsIfTheyArePlacedInAllowedAreas()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock0"), 6);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock1"), 2);

            Assert.IsNotNull(manager.AddCluesToExistingDoor("lock1", new List<int> { 10 }));
        }

        [TestMethod]
        public void AddingAnClueToAnExistingDoorAddsTheRightDependency()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock0"), 2);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock1"), 2);

            manager.AddCluesToExistingDoor("lock1", new List<int> { 15 });

            Assert.IsTrue(manager.IsLockDependentOnParentLock("lock1", "lock0"));
        }

        [TestMethod]
        public void AddingAnClueToAnExistingDoorWhichAddsADependencyGivesTheCorrectlyUpdatedListOfPossibleClueVertices()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock0"), 2);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock1"), 2);

            manager.AddCluesToExistingDoor("lock1", new List<int> { 15 });

            var validRooms = manager.GetValidRoomsToPlaceClueForDoor("lock0").ToList();

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 10, 11, 12 }), validRooms);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void CluesCannotBeAddedToNoExistantDoors()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock0"), 6);

            manager.AddCluesToExistingDoor("lock1", new List<int> { 15 });
        }

        [TestMethod]
        public void AddingTheTwoCluesForADoorBehindOtherDoorsMeansTheNewDoorDependsOnTheTwoOtherDoors()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 12), "lock1"), 2);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock2"), 2);

            manager.PlaceDoorAndClues(new DoorRequirements(new Connection(5, 6), "lock3"), new List<int>(new int[] { 12, 13 }));

            Assert.IsTrue(manager.IsLockDependentOnParentLock("lock3", "lock1"));
            Assert.IsTrue(manager.IsLockDependentOnParentLock("lock3", "lock2"));
        }

        [TestMethod]
        public void ACluePlacedBehindTwoDoorsOnTheSameEdgeGivesADependencyOnBothDoors()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoor(new DoorRequirements(new Connection(10, 11), "lock1"));
            manager.PlaceDoor(new DoorRequirements(new Connection(10, 11), "lock2"));

            manager.PlaceDoorAndClues(new DoorRequirements(new Connection(5, 6), "lock3"), new List<int>(new int[] { 12 }));

            Assert.IsTrue(manager.IsLockDependentOnParentLock("lock3", "lock1"));
            Assert.IsTrue(manager.IsLockDependentOnParentLock("lock3", "lock2"));
        }

        [TestMethod]
        public void AClueLockedBehindTwoDoorsOnTheSameEdgeGivesADependencyOnBothDoors()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClues(new DoorRequirements(new Connection(5, 6), "lock3"), new List<int>(new int[] { 12 }));

            manager.PlaceDoor(new DoorRequirements(new Connection(10, 11), "lock1"));
            manager.PlaceDoor(new DoorRequirements(new Connection(10, 11), "lock2"));

            Assert.IsTrue(manager.IsLockDependentOnParentLock("lock3", "lock1"));
            Assert.IsTrue(manager.IsLockDependentOnParentLock("lock3", "lock2"));
        }

        [TestMethod]
        public void AddingTwoCluesBehindADoorMeansThereAreTwoDependenciesOnDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock0"), 2);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock1"), 15);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(3, 4), "lock2"), 14);

            List<string> dependentDoors = manager.GetDependentDoorIds("lock0");

            CollectionAssert.AreEquivalent(new List<string>(new string[] { "lock1", "lock2" }), dependentDoors);
        }

        [TestMethod]
        public void ObjectivesCanBePlacedAndRetrieved()
        {
            var manager = BuildStandardManager();

            manager.PlaceObjective(new ObjectiveRequirements(12, "obj1", 1));
            Assert.AreEqual("obj1", manager.GetObjectiveById("obj1").Id);
        }

        [TestMethod]
        public void CluesCanBeAddedToObjectivesCanBePlacedAndRetrieved()
        {
            var manager = BuildStandardManager();

            manager.PlaceObjective(new ObjectiveRequirements(12, "obj1", 1));
            var cluesPlaced = manager.AddCluesToExistingObjective("obj1", new List<int> { 2 });
            Assert.AreEqual(1, cluesPlaced.Count());
        }

        [TestMethod]
        public void AddingACluesNotBehindADoorMeansThereAreNoDependentDoors()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock0"), 2);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock1"), 3);

            List<string> dependentDoors = manager.GetDependentDoorIds("lock0");

            CollectionAssert.AreEquivalent(new List<string>(), dependentDoors);
        }

        [TestMethod]
        public void LockingAClueWithANewDoorMakesTheOldDoorDependentOnTheNewDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock0"), 13);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(11, 13), "lock1"), 3);

            List<string> dependentDoors = manager.GetDependentDoorIds("lock1");
            CollectionAssert.AreEquivalent(new List<string>(new string[] { "lock0" }), dependentDoors);

            List<string> noDependentDoors = manager.GetDependentDoorIds("lock0");
            CollectionAssert.AreEquivalent(new List<string>(), noDependentDoors);
        }

        [TestMethod]
        public void CheckAllRoomsExceptThoseLockedByThisClueAreValidToPlaceClueWithNoDependencies()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock0"), 4);

            var validRooms = manager.GetValidRoomsToPlaceClueForDoor(new Connection(10, 11)).ToList();

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1,2,3,4,5,6,10 }), validRooms);
        }

        [TestMethod]
        public void CheckRoomsBehindLockedClueAreNotValidToPlaceNewClue()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(3, 5), "lock0"), 15);

            var validRooms = manager.GetValidRoomsToPlaceClueForDoor(new Connection(11, 13)).ToList();

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 10, 11, 12 }), validRooms);
        }

        [TestMethod]
        public void CheckRoomsBehindDependenciesLockedClueAreNotValidToPlaceNewClue()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(10, 11), "lock0"), 6);
            //Make lock0 depend on lock1
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(3, 5), "lock1"), 4);
            
            //This would cover clue1. Therefore lock1 rooms are not accessible, which includes clue0
            //Therefore lock0 rooms are not accessible
            var validRooms = manager.GetValidRoomsToPlaceClueForDoor(new Connection(3, 4)).ToList();

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 10 }), validRooms);
        }

        [TestMethod]
        public void AllRoomsAreValidForCluesForSingleObjective()
        {
            var manager = BuildStandardManager();

            manager.PlaceObjective(new ObjectiveRequirements(15, "obj1", 1));
            
            var validRooms = manager.GetValidRoomsToPlaceClueForObjective("obj1").ToList();

            CollectionAssert.AreEquivalent(new List<int>{ 1, 2, 3, 4, 5, 6, 10, 11, 12, 13, 14, 15 }, validRooms);
        }

        [TestMethod]
        public void CheckRoomsBehindLockedDoorsThatDependOnObjectivesAreNotValidForPlacingObjectiveClues()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClues(new DoorRequirements(new Connection(10, 11), "lock0"), new List<int>());
            //lock0 depends on obj0. Therefore clues for obj0 cannot be placed behind lock0
            manager.PlaceObjective(new ObjectiveRequirements(6, "obj0", 1, new List<string>{"lock0"}));

            var validRooms = manager.GetValidRoomsToPlaceClueForObjective("obj0").ToList();

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10 }), validRooms);
        }

        [TestMethod]
        public void CheckRoomsBehindDoorsDependentOnParentObjectivesAreNotValidForChildObjectiveClues()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClues(new DoorRequirements(new Connection(10, 11), "lock0"), new List<int>());
            //lock0 depends on obj0. Therefore clues for obj0 cannot be placed behind lock0
            manager.PlaceObjective(new ObjectiveRequirements(6, "obj0", 1, new List<string>{"lock0"}));
            //obj0 depends on obj1. Therefore we can't place obj1 clues behind lock0
            manager.PlaceObjective(new ObjectiveRequirements(6, "obj1", 1, new List<string>{"obj0"}));

            var validRooms = manager.GetValidRoomsToPlaceClueForObjective("obj1").ToList();

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10 }), validRooms);
        }

        [TestMethod]
        public void CheckValidRoomsForCluesForADoorOnWhichAnObjectiveAndAnotherLockedDoorDepend()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoor(new DoorRequirements(new Connection(10, 11), "lock0"));
            //lock0 depends on obj0
            manager.PlaceObjective(new ObjectiveRequirements(6, "obj0", 1, new List<string>{"lock0"}));

            manager.PlaceDoor(new DoorRequirements(new Connection(3, 4), "lock1"));
            manager.AddCluesToExistingObjective("obj0", new List<int>{ 4 });
            //obj0 depends on lock1

            //lock1 clues can't be placed behind lock0 (or behind lock1)
            var validRooms = manager.GetValidRoomsToPlaceClueForDoor("lock1", new List<string>()).ToList();

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 5, 6, 10 }), validRooms);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ObjectivesThatAreRequiredToUnlockADoorCantBePlacedBehindDoorsThatDependOnTheDoor()
        {
            var manager = BuildStandardManager();

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(10, 11), "lock0"), 4);
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(3, 4), "lock1"), 2);

            manager.PlaceObjective(new ObjectiveRequirements(11, "obj1", 1, new List<string> { "lock1" }));
        }

        [TestMethod]
        public void ValidRoomsAreCorrectWhenSpecifyingExtraDependencies()
        {
            var manager = BuildStandardManager();

            //Extra dependencies

            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(2, 10), "lock0"), 1);
            //Make lock0 depend on lock1
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(3, 5), "lock1"), 4);

            //New lock
            //Asking for valid rooms for lock2 clue, specifying we don't want to have open lock0
            //We are locking lock1 clues, so rooms behind lock1 and lock0 are excluded
            var validRooms = manager.GetValidRoomsToPlaceClueForDoor(new Connection(3, 4), new List<string>(new string[] { "lock0" })).ToList();

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3 }), validRooms);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void RequestingDependentDoorsForADoorIdThatDoesntExistThrowsAnException()
        {
            var manager = BuildStandardManager();
            List<string> dependentDoors = manager.GetDependentDoorIds("lock0");
        }
      
        [TestMethod]
        public void FullyOpenMapIsFullyAccessibleWithNoClues()
        {
            var manager = BuildStandardManager();

            var validRooms = manager.GetAccessibleVerticesWithClues(new List<int>());

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10, 11, 12, 13, 14, 15 }), validRooms.ToList());
        }

        [TestMethod]
        public void MapWithOneLockIsPartiallyAccessibleWithNoClues()
        {
            var manager = BuildStandardManager();
            manager.PlaceDoorAndClue(new DoorRequirements(new Connection(10, 11), "lock0"), 6);

            var validRooms = manager.GetAccessibleVerticesWithClues(new List<int>());

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10 }), validRooms.ToList());
        }

        [TestMethod]
        public void MapWithOneLockIsFullyAccessibleWithRightClue()
        {
            var manager = BuildStandardManager();
            var clue0 = manager.PlaceDoorAndClue(new DoorRequirements(new Connection(10, 11), "lock0"), 6);

            var validRooms = manager.GetAccessibleVerticesWithClues(new List<int>(new int [] { clue0.OpenLockIndex }));

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10, 11, 12, 13, 14, 15 }), validRooms.ToList());
        }

        [TestMethod]
        public void MapWithTwoLocksIsPartiallyAccessibleWithOneClue()
        {
            var manager = BuildStandardManager();
            var clue0 = manager.PlaceDoorAndClue(new DoorRequirements(new Connection(10, 11), "lock0"), 6);
            var clue1 = manager.PlaceDoorAndClue(new DoorRequirements(new Connection(5, 6), "lock1"), 1);

            var validRooms = manager.GetAccessibleVerticesWithClues(new List<int>(new int[] { clue0.OpenLockIndex }));

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 10, 11, 12, 13, 14, 15 }), validRooms.ToList());
        }

        [TestMethod]
        public void MapRequiringTwoLocksIsFullyAccessibleWithTwoCorrectClues()
        {
            var manager = BuildStandardManager();
            var clues = manager.PlaceDoorAndClues(new DoorRequirements(new Connection(10, 11), "lock0", 2),
                                                  new List<int>(new int[] { 5, 6 }));

            var validRooms = manager.GetAccessibleVerticesWithClues(clues.Select(c => c.OpenLockIndex));

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10, 11, 12, 13, 14, 15 }), validRooms.ToList());
        }

        [TestMethod]
        public void MapRequiringTwoLocksIsNotFullyAccessibleWithOnlyOneCorrectClue()
        {
            var manager = BuildStandardManager();
            var clues = manager.PlaceDoorAndClues(new DoorRequirements(new Connection(10, 11), "lock0", 2),
                                                  new List<int>(new int[] { 5 }));

            var validRooms = manager.GetAccessibleVerticesWithClues(clues.Select(c => c.OpenLockIndex));

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10 }), validRooms.ToList());
        }

        [TestMethod]
        public void LockedDoorDependingOnAnObjectiveIsAccessibleIfCluesForTheObjectiveAreAccessible()
        {
            var manager = BuildStandardManager();
            manager.PlaceDoor(new DoorRequirements(new Connection(10, 11), "lock0", 1));
            manager.PlaceObjective(new ObjectiveRequirements(2, "obj0", 1, new List<string> { "lock0" }));
            var clues = manager.AddCluesToExistingObjective("obj0", new List<int> { 2 });

            var validRooms = manager.GetAccessibleVerticesWithClues(clues.Select(c => c.OpenLockIndex));

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10, 11, 12, 13, 14, 15 }), validRooms.ToList());
        }

        [TestMethod]
        public void LockedDoorDependingOnAnObjectiveIsInAccessibleIfInsufficientCluesForTheObjectiveAreAccessible()
        {
            var manager = BuildStandardManager();
            manager.PlaceDoor(new DoorRequirements(new Connection(10, 11), "lock0", 2));
            manager.PlaceObjective(new ObjectiveRequirements(2, "obj0", 1, new List<string> { "lock0" }));
            //red herring
            manager.PlaceObjective(new ObjectiveRequirements(3, "obj1", 1));
            //One clue before and one after the door (not enough to unlock it)
            var clues = manager.AddCluesToExistingObjective("obj0", new List<int> { 2 });
            
            var validRooms = manager.GetAccessibleVerticesWithClues(new List<int>{clues.First().OpenLockIndex});

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 10 }), validRooms.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void RedDoorCoveredByBlueDoorBlueKeyCoveredByYellowDoorYellowKeyCoveredByRedShouldNotBeAllowed()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            var doorManager = mapModel.DoorAndClueManager;

            doorManager.PlaceDoorAndClues(new DoorRequirements(new Connection(5, 6), "red"),
                new List<int>(new int[] { 2 }));

            doorManager.PlaceDoorAndClues(new DoorRequirements(new Connection(3, 5), "blue"),
                new List<int>(new int[] { 4 }));

            doorManager.PlaceDoorAndClues(new DoorRequirements(new Connection(3, 4), "yellow"),
                new List<int>(new int[] { 6 }));
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
