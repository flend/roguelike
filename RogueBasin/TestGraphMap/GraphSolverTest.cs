using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;

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
        public void MapWithLockedDoorIsNotSolvable()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            mapModel.DoorAndClueManager.LockDoor(10, 11, "lock0");

            GraphSolver solver = new GraphSolver(mapModel);

            Assert.IsFalse(solver.MapCanBeSolved());
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
