using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;
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
            var deadEndNodes = mapH.GetTerminalBranchNodes(0).ToList();

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 4, 6, 12, 14, 15 }), deadEndNodes);
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
    }
}
