using GraphMap;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace TestGraphMap
{
    [TestClass]
    public class MapHeuristicsTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        public void NodesOnSideOfConnectionCanBeFound()
        {
            MapHeuristics mapH = BuildMapHeuristics();

            var accessibleNodes = mapH.GetNodesOnStartSideOfConnection(new Connection(2, 10));
            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 5, 6 }), accessibleNodes.ToList());
        }

        [TestMethod]
        public void DeadEndFinalRoomsCanBeFound()
        {
            MapHeuristics mapH = BuildMapHeuristics();
            var deadEndNodes = mapH.GetTerminalBranchNodes()[0].ToList();

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 4, 6, 12, 14, 15 }), deadEndNodes);
        }

        [TestMethod]
        public void RoomsOfMultipleDepthFromDeadEndFinalRoomsCanBeFound()
        {
            var standardMap = BuildBranchingTestMap();
            var mapNoCycles = new MapCycleReducer(standardMap.RoomConnectionGraph.Edges);
            var mapH = new MapHeuristics(mapNoCycles, 1);

            var expectedNodes = new Dictionary<int, List<int>> {
                {0, new List<int>(new int[]{1, 6, 14, 15, 17, 18})},
                {1, new List<int>(new int[]{5, 12, 16})},
                {2, new List<int>(new int[]{4})}
            };

            var terminalNodesFound = mapH.GetTerminalBranchNodes();

            CollectionAssert.AreEquivalent(expectedNodes[0], terminalNodesFound[0]);
            CollectionAssert.AreEquivalent(expectedNodes[1], terminalNodesFound[1]);
            CollectionAssert.AreEquivalent(expectedNodes[2], terminalNodesFound[2]);
            CollectionAssert.AreEquivalent(expectedNodes.Keys, terminalNodesFound.Keys);
        }

        [TestMethod]
        public void ConnectionsOfMultipleDepthFromDeadEndFinalRoomsCanBeFound()
        {
            var standardMap = BuildBranchingTestMap();
            var mapNoCycles = new MapCycleReducer(standardMap.RoomConnectionGraph.Edges);
            var mapH = new MapHeuristics(mapNoCycles, 1);

            var expectedConnections = new Dictionary<int, List<Connection>> {
                {0, new List<Connection>(new Connection[]{ 
                    new Connection(1, 2),
                    new Connection(5, 6),
                    new Connection(13, 14),
                    new Connection(13, 15),
                    new Connection(16, 17),
                    new Connection(12, 18)
                    })},
                {1, new List<Connection>(new Connection[]{
                    new Connection(3, 5),
                    new Connection(11, 12),
                    new Connection(4, 16)})},
                {2, new List<Connection>(new Connection[]{
                    new Connection(3, 4)})}
            };

            var terminalConnectionsFound = mapH.GetTerminalBranchConnections();

            CollectionAssert.AreEquivalent(expectedConnections[0], terminalConnectionsFound[0]);
            CollectionAssert.AreEquivalent(expectedConnections[1], terminalConnectionsFound[1]);
            CollectionAssert.AreEquivalent(expectedConnections[2], terminalConnectionsFound[2]);
            CollectionAssert.AreEquivalent(expectedConnections.Keys, terminalConnectionsFound.Keys);
        }

        [TestMethod]
        public void DeadEndNodesAreFoundInSinglePathGraphs()
        {
            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            var mapNoCycles = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);
            var mapH = new MapHeuristics(mapNoCycles, 1);

            var expectedConnections = new Dictionary<int, List<Connection>> {
                {0, new List<Connection>(new Connection[]{ 
                    new Connection(1, 2),
                    new Connection(2, 3)})}
            };

            var terminalConnectionsFound = mapH.GetTerminalBranchConnections();

            //TODO: We have slightly pathological behaviour that all non-terminal node connections
            //will be double counted in the 
            CollectionAssert.AreEquivalent(expectedConnections[0], terminalConnectionsFound[0]);
            CollectionAssert.AreEquivalent(expectedConnections.Keys, terminalConnectionsFound.Keys);
        }


        private MapHeuristics BuildMapHeuristics()
        {
            var standardMap = BuildStandardTestMap();
            var mapNoCycles = new MapCycleReducer(standardMap.RoomConnectionGraph.Edges);
            return new MapHeuristics(mapNoCycles, 1);
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

        private ConnectivityMap BuildBranchingTestMap()
        {
            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Branch

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 16);
            newMap.AddRoomConnection(16, 17);

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
            newMap.AddRoomConnection(12, 18);
        
            //2-way branches
            newMap.AddRoomConnection(11, 13);
            newMap.AddRoomConnection(13, 14);
            newMap.AddRoomConnection(13, 15);

            //Save to disk
            GraphvizExport.OutputUndirectedGraph(newMap.RoomConnectionGraph, "branch-test-map");

            return newMap;
        }
    }
}
