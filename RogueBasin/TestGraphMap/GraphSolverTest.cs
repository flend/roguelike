using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;
using System.Collections.Generic;

namespace TestGraphMap
{
    [TestClass]
    public class GraphSolverTest
    {
        
        [TestMethod]
        public void MapWithNoDoorsIsSolvable()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);

            GraphSolver solver = new GraphSolver(mapModel);

            Assert.IsTrue(solver.MapCanBeSolved());
        }

        [TestMethod]
        public void MapWithLockedDoorAndNoCluesIsNotSolvable()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            var doorManager = mapModel.DoorAndClueManager;
            doorManager.PlaceDoorAndClues(new DoorRequirements(new Connection(10, 11), "lock0"),
                new List<int>());

            GraphSolver solver = new GraphSolver(mapModel);

            Assert.IsFalse(solver.MapCanBeSolved());
        }

        [TestMethod]
        public void MapWithObjectiveAndNoCluesIsNotSolvable()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            var doorManager = mapModel.DoorAndClueManager;
            doorManager.PlaceDoorAndClues(new DoorRequirements(new Connection(10, 11), "lock0"),
                new List<int>());

            GraphSolver solver = new GraphSolver(mapModel);

            Assert.IsFalse(solver.MapCanBeSolved());
        }

        [TestMethod]
        public void MapWithLockedDoorAndClueIsSolvable()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            var doorManager = mapModel.DoorAndClueManager;

            doorManager.PlaceDoorAndClues(new DoorRequirements(new Connection(10, 11), "lock0"),
                new List<int>(new int[] { 6 }));

            Assert.IsTrue(doorManager.ClueMap.Count > 0);
            //Assert.IsTrue(doorManager.DoorMap.Count > 0);

            GraphSolver solver = new GraphSolver(mapModel);

            //Assert.IsTrue(solver.MapCanBeSolved());
        }

        [TestMethod]
        public void MapWithLockedDoorAndInaccessibleClueIsNotSolvable()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            var doorManager = mapModel.DoorAndClueManager;
            doorManager.PlaceDoorAndCluesNoChecks(new DoorRequirements(new Connection(10, 11), "lock0"),
                new List<int>(new int[] { 12 }));

            GraphSolver solver = new GraphSolver(mapModel);

            Assert.IsFalse(solver.MapCanBeSolved());
        }

        [TestMethod]
        public void MapWithLockedDoorAndRecursivelyLockedTwoCluesIsSolvable()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            var doorManager = mapModel.DoorAndClueManager;

            doorManager.PlaceDoorAndClues(new DoorRequirements(new Connection(10, 11), "lock0"),
                new List<int>(new int[] { 6 }));

            doorManager.PlaceDoorAndClues(new DoorRequirements(new Connection(5, 6), "lock1"),
                new List<int>(new int[] { 4 }));

            GraphSolver solver = new GraphSolver(mapModel);

            Assert.IsTrue(solver.MapCanBeSolved());
        }

        [TestMethod]
        public void MapWithRecursivelyDeadLockedDoorIsNotSolvable()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            var doorManager = mapModel.DoorAndClueManager;

            doorManager.PlaceDoorAndClues(new DoorRequirements(new Connection(10, 11), "lock0"),
                new List<int>(new int[] { 6 }));

            doorManager.PlaceDoorAndCluesNoChecks(new DoorRequirements(new Connection(5, 6), "lock1"),
                new List<int>(new int[] { 13 }));

            GraphSolver solver = new GraphSolver(mapModel);

            Assert.IsFalse(solver.MapCanBeSolved());
        }

        [TestMethod]
        public void MapWithLockedDoorWithDependencyOnObjectiveIsSolvable()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            var doorManager = mapModel.DoorAndClueManager;
            doorManager.PlaceDoor(new DoorRequirements(new Connection(10, 11), "lock0", 1));
            doorManager.PlaceObjective(new ObjectiveRequirements(4, "obj0", 1, new List<string> { "lock0" }));
            doorManager.AddCluesToExistingObjective("obj0", new List<int> { 1 });

            GraphSolver solver = new GraphSolver(mapModel);

            Assert.IsTrue(solver.MapCanBeSolved());
        }

        [TestMethod]
        public void MapWithLockedDoorWithDependencyOnUnsolveableObjectiveIsNotSolvable()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            var doorManager = mapModel.DoorAndClueManager;
            doorManager.PlaceDoor(new DoorRequirements(new Connection(10, 11), "lock0", 1));
            doorManager.PlaceObjective(new ObjectiveRequirements(4, "obj0", 1, new List<string> { "lock0" }));
            //missing clue for objective

            GraphSolver solver = new GraphSolver(mapModel);

            Assert.IsFalse(solver.MapCanBeSolved());
        }

        [TestMethod]
        public void MapWithLockedDoorWithRecursiveDependencyOnObjectiveIsSolvable()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            var doorManager = mapModel.DoorAndClueManager;
            doorManager.PlaceDoor(new DoorRequirements(new Connection(10, 11), "lock0", 1));
            doorManager.PlaceObjective(new ObjectiveRequirements(4, "obj0", 1, new List<string> { "lock0" }));
            doorManager.PlaceObjective(new ObjectiveRequirements(4, "obj1", 1, new List<string> { "obj0" }));
            doorManager.AddCluesToExistingObjective("obj1", new List<int> { 1 });

            GraphSolver solver = new GraphSolver(mapModel);

            Assert.IsTrue(solver.MapCanBeSolved());
        }

        [TestMethod]
        public void MapWithLockedDoorWithMultipleDependencyOnObjectivesIsSolvable()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            var doorManager = mapModel.DoorAndClueManager;
            doorManager.PlaceDoor(new DoorRequirements(new Connection(10, 11), "lock0", 2));
            doorManager.PlaceObjective(new ObjectiveRequirements(4, "obj0", 1, new List<string> { "lock0" }));
            doorManager.PlaceObjective(new ObjectiveRequirements(5, "obj1", 1, new List<string> { "lock0" }));
            doorManager.AddCluesToExistingObjective("obj0", new List<int> { 1 });
            doorManager.AddCluesToExistingObjective("obj1", new List<int> { 1 });

            GraphSolver solver = new GraphSolver(mapModel);

            Assert.IsTrue(solver.MapCanBeSolved());
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
