using GraphMap;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestGraphMap
{
    [TestClass]
    public class MapCycleReducerTest
    {

        [TestMethod]
        public void MapCycleReducerRemovesOneCycleInInputMap()
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

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            var roomMapping = cycleReducer.roomMappingFullToNoCycleMap;

            //Confirm that all the cycle nodes are mapped to the first node
            Assert.AreEqual(roomMapping[3], 3);
            Assert.AreEqual(roomMapping[4], 3);
            Assert.AreEqual(roomMapping[5], 3);
        }

        [TestMethod]
        public void SingleCycleInMapCanBeRetrieved()
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

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            var allCycles = cycleReducer.AllCycles.ToList();
            var expectedCycles = new List<List<Connection>> { new List<Connection> {
                new Connection(3,4),
                new Connection(4,5),
                new Connection(5,3)
            }};

            //Can't know whether the list will be returned in 'clockwise' or 'anticlockwise' ordering
            Assert.AreEqual(allCycles.Count, expectedCycles.Count);
            CollectionAssert.AreEquivalent(allCycles[0], expectedCycles[0]);
        }

        [TestMethod]
        public void SingleCycleInMapCanBeRetrievedWhenCycleSpecifiedInADifferentOrder()
        {
            //Build a graph with one nested cycle

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 5);
            newMap.AddRoomConnection(5, 4);
            newMap.AddRoomConnection(4, 3);

            newMap.AddRoomConnection(5, 6);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            var allCycles = cycleReducer.AllCycles;
            var expectedCycles = new List<List<Connection>> { new List<Connection> {
                new Connection(3,5),
                new Connection(5,4),
                new Connection(4,3)
            }};

            //Can't know whether the list will be returned in 'clockwise' or 'anticlockwise' ordering
            Assert.AreEqual(allCycles.Count, expectedCycles.Count);
            CollectionAssert.AreEquivalent(allCycles[0], expectedCycles[0]);
        }

        [TestMethod]
        public void MapWithTwoNonConnectedCyclesGivesTwoCyclesInAllCycles()
        {
            //Build a graph with one nested cycle

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle 1

            newMap.AddRoomConnection(3, 5);
            newMap.AddRoomConnection(5, 4);
            newMap.AddRoomConnection(4, 3);

            newMap.AddRoomConnection(5, 6);

            //Cycle 2

            newMap.AddRoomConnection(6, 7);
            newMap.AddRoomConnection(7, 8);
            newMap.AddRoomConnection(8, 6);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            var allCycles = cycleReducer.AllCycles;

            Assert.AreEqual(2, allCycles.Count);
        }

        [TestMethod]
        public void MapWithTwoNestedCyclesGivesTwoCyclesInAllCycles()
        {
            //Build a graph with one nested cycle

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle 1
            newMap.AddRoomConnection(3, 5);
            newMap.AddRoomConnection(5, 4);
            newMap.AddRoomConnection(4, 3);

            //Nested cycle
            newMap.AddRoomConnection(4, 6);
            newMap.AddRoomConnection(5, 6);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            var allCycles = cycleReducer.AllCycles;

            Assert.AreEqual(2, allCycles.Count);
        }

        [TestMethod]
        public void MapWithTwoNestedCyclesGivesCorrectTwoCyclesInAllCycles()
        {
            //Build a graph with one nested cycle

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle 1
            newMap.AddRoomConnection(3, 5);
            newMap.AddRoomConnection(5, 4);
            newMap.AddRoomConnection(4, 3);

            //Nested cycle
            newMap.AddRoomConnection(4, 6);
            newMap.AddRoomConnection(5, 6);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            //Relying on the fact that the DFS hits 3,5 first
            var allCycles = cycleReducer.AllCycles;
            var expectedCycles = new List<List<Connection>> {
                new List<Connection> {
                    new Connection(3,5),
                    new Connection(5,4),
                    new Connection(4,3)
                },
                new List<Connection> {
                    new Connection(4,6),
                    new Connection(6,5),
                    new Connection(5,4)
                }
            };
            Assert.AreEqual(2, allCycles.Count);
            CollectionAssert.AreEquivalent(expectedCycles[0], allCycles[0]);
            CollectionAssert.AreEquivalent(expectedCycles[1], allCycles[1]);
        }


        [TestMethod]
        public void RetainedEdgeIsInMapAfterCycleReduction()
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

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            Assert.IsTrue(cycleReducer.IsEdgeInRoomsNoCycles(1, 2));
        }

        [TestMethod]
        public void EdgeBetweenNonCycleAndCycleMapsToBottleneckEdge()
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

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            //In the reduced graph, we have the edge between the first node of the cycle and the next node
            //In the full graph, we have the bottleneck edge between the cycle and the next node

            Assert.AreEqual(cycleReducer.edgeMappingNoCycleToFullMap[new Connection(3, 6)], new Connection(5, 6));
        }

        [TestMethod]
        public void EdgeTraversingCycleIsInMapAfterCycleReduction()
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

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            Assert.IsTrue(cycleReducer.IsEdgeInRoomsNoCycles(3, 6));
        }

        [TestMethod]
        public void SquashedEdgeIsNotInMapAfterCycleReduction()
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

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            Assert.IsFalse(cycleReducer.IsEdgeInRoomsNoCycles(3, 4));
        }


        [ExpectedException(typeof(ApplicationException))]
        [TestMethod]
        public void RemovedEdgeNotFoundInMapCycleReducer()
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

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            cycleReducer.GetEdgeBetweenRoomsNoCycles(4, 5);
        }

        [TestMethod]
        public void MapCycleReducerRemovesMultipleCyclesInInputMap()
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

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            var roomMapping = cycleReducer.roomMappingFullToNoCycleMap;

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
        public void MapCycleReducerMapsContainsAMappingForEachEdgeInReducedMap()
        {
            //Build a graph with one nested cycles

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 5);
            newMap.AddRoomConnection(3, 5);

            //Exterior connections

            newMap.AddRoomConnection(5, 6);
            newMap.AddRoomConnection(4, 7);
            newMap.AddRoomConnection(4, 8);
            newMap.AddRoomConnection(3, 9);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            Assert.AreEqual(6, cycleReducer.edgeMappingNoCycleToFullMap.Count());
        }

        [TestMethod]
        public void MapCycleReducerMapsContainsCorrectMappingForUnchangedEdgesInReducedMap()
        {
            //Build a graph with one nested cycles

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 5);
            newMap.AddRoomConnection(3, 5);

            //Exterior connections

            newMap.AddRoomConnection(5, 6);
            newMap.AddRoomConnection(6, 7);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);
            var edgeMapping = cycleReducer.edgeMappingNoCycleToFullMap;

            Assert.AreEqual(new Connection(1, 2), edgeMapping[new Connection(1, 2)].Ordered);
            Assert.AreEqual(new Connection(2, 3), edgeMapping[new Connection(2, 3)].Ordered);
            Assert.AreEqual(new Connection(6, 7), edgeMapping[new Connection(6, 7)].Ordered);
        }

        [TestMethod]
        public void MapCycleReducerMapsExterorEdgesOfCycleToOriginalConnections()
        {
            //Build a graph with one nested cycles

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 5);
            newMap.AddRoomConnection(3, 5);

            //Exterior connections

            newMap.AddRoomConnection(5, 6);
            newMap.AddRoomConnection(4, 7);
            newMap.AddRoomConnection(4, 8);
            newMap.AddRoomConnection(3, 9);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            var edgeMapping = cycleReducer.edgeMappingNoCycleToFullMap;

            //Exterior connections from start node
            Assert.AreEqual(new Connection(2, 3), edgeMapping[new Connection(2, 3)].Ordered);
            Assert.AreEqual(new Connection(3, 9), edgeMapping[new Connection(3, 9)].Ordered);

            //Exterior connections from collasped nodes
            Assert.AreEqual(new Connection(4, 7), edgeMapping[new Connection(3, 7)].Ordered);
            Assert.AreEqual(new Connection(4, 8), edgeMapping[new Connection(3, 8)].Ordered);

            Assert.AreEqual(new Connection(5, 6), edgeMapping[new Connection(3, 6)].Ordered);
        }

        [TestMethod]
        public void MapCycleReducerMapsReducedCycleNodeBackToOriginalNodes()
        {
            //Build a graph with one nested cycles

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 5);
            newMap.AddRoomConnection(3, 5);

            newMap.AddRoomConnection(5, 6);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            var roomMapping = cycleReducer.roomMappingNoCycleToFullMap;

            //Confirm that all the first node in the cycle maps back to all the collapsed nodes
            CollectionAssert.AreEquivalent(new List<int>{ 3, 4, 5 }, roomMapping[3]);
        }

        [TestMethod]
        public void MapCycleReducerMapsNonReducedNodeBackToThemselves()
        {
            //Build a graph with one nested cycles

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            //Cycle

            newMap.AddRoomConnection(3, 4);
            newMap.AddRoomConnection(4, 5);
            newMap.AddRoomConnection(3, 5);

            newMap.AddRoomConnection(5, 6);

            MapCycleReducer cycleReducer = new MapCycleReducer(newMap.RoomConnectionGraph.Edges);

            var roomMapping = cycleReducer.roomMappingNoCycleToFullMap;

            //Confirm that all the first node in the cycle maps back to all the collapsed nodes
            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1 }), roomMapping[1]);
            CollectionAssert.AreEquivalent(new List<int>(new int[] { 2 }), roomMapping[2]);
            CollectionAssert.AreEquivalent(new List<int>(new int[] { 6 }), roomMapping[6]);
        }
    }
}
