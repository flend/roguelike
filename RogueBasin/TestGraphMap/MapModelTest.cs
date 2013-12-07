using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;

namespace TestGraphMap
{
    [TestClass]
    public class MapModelTest
    {
        [TestMethod]
        public void MapPuzzleLayerRemovesOneCycleInInputMap()
        {
            //Build a graph with one nested cycle

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 5);
            newMap.AddRoomConnection(3, 5);
            
            newMap.AddRoomConnection(5, 6);

            MapPuzzleLayer puzzleLayer = new MapPuzzleLayer(newMap);

            var roomMapping = puzzleLayer.RoomMappingToNoCycleMap;

            //Confirm that all the cycle nodes are mapped to the first node
            Assert.AreEqual(roomMapping[3], 3);
            Assert.AreEqual(roomMapping[4], 3);
            Assert.AreEqual(roomMapping[5], 3);
        }

        [TestMethod]
        public void MapPuzzleLayerRemovesMultipleCyclesInInputMap()
        {
            //Build a graph with one two nested cycles

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 5);
            newMap.AddRoomConnection(3, 5);

            newMap.AddRoomConnection(5, 6);

            //Cycle

            newMap.AddRoomConnection(6, 7);
            newMap.AddRoomConnection(7, 8);
            newMap.AddRoomConnection(8, 9);
            newMap.AddRoomConnection(9, 10);
            newMap.AddRoomConnection(10, 11);
            newMap.AddRoomConnection(11, 6);
            newMap.AddRoomConnection(9, 6);

            newMap.AddRoomConnection(11, 12);

            MapPuzzleLayer model = new MapPuzzleLayer(newMap);

            var roomMapping = model.RoomMappingToNoCycleMap;

            //Confirm that all the first cycle nodes are mapped to the first node in the cycle
            Assert.AreEqual(3, roomMapping[3]);
            Assert.AreEqual(3, roomMapping[4]);
            Assert.AreEqual(3, roomMapping[5]);

            //Confirm that all the second cycle nodes are mapped to the first node in the second cycle
            Assert.AreEqual(6, roomMapping[6]);
            Assert.AreEqual(6, roomMapping[7]);
            Assert.AreEqual(6, roomMapping[8]);
            Assert.AreEqual(6, roomMapping[9]);
            Assert.AreEqual(6, roomMapping[10]);
            Assert.AreEqual(6, roomMapping[11]);
        }

        [TestMethod]
        public void RoomsOnSideOfOriginInSplitMapAreInSameComponent() {

            ConnectivityMap standardMap = BuildStandardTestMap();
            MapSplitter splitter = new MapSplitter(standardMap.RoomConnectionGraph.Edges, standardMap.GetEdgeBetweenRooms(9, 10), 1);

            Assert.AreEqual(splitter.RoomComponentIndex(9), splitter.OriginComponentIndex);
            Assert.AreEqual(splitter.RoomComponentIndex(10), splitter.NonOriginComponentIndex);

        }

        private ConnectivityMap BuildStandardTestMap() {

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
            newMap.AddRoomConnection(12, 13);

            //2-way branches
            newMap.AddRoomConnection(13, 14);
            newMap.AddRoomConnection(13, 15);

            //Save to disk
            GraphvizExport.OutputUndirectedGraph(newMap.RoomConnectionGraph, "standard-test-map");

            return newMap;
        }
    }
}
