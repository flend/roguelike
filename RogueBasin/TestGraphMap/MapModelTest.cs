using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;
using System.Collections.Generic;
using QuickGraph;
using System.Linq;

namespace TestGraphMap
{
    [TestClass]
    public class MapModelTest
    {
        [TestMethod]
        public void UndirectedEdgesMayBeFoundInEitherOrientation()
        {
            var mapNoCycles = new UndirectedGraph<int, TaggedEdge<int, string>>();

            mapNoCycles.AddVerticesAndEdge(new TaggedEdge<int, string>(1, 2, ""));

            TaggedEdge<int, string> possibleEdge = null;

            mapNoCycles.TryGetEdge(2, 1, out possibleEdge);
            Assert.IsNotNull(possibleEdge);

            TaggedEdge<int, string> possibleEdge2 = null;

            mapNoCycles.TryGetEdge(1, 2, out possibleEdge2);
            Assert.IsNotNull(possibleEdge2);
        }


        [TestMethod]
        public void AddingSameEdgeTwiceDoesntMakeTwoEdges()
        {
            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(1, 2);

            Assert.AreEqual(1, newMap.RoomConnectionGraph.EdgeCount);
        }


        [TestMethod]
        public void RoomsOnSideOfOriginInSplitMapAreInSameComponentAndViceVersa() {

            ConnectivityMap standardMap = BuildStandardTestMap();
            MapSplitter splitter = new MapSplitter(standardMap.RoomConnectionGraph.Edges, standardMap.GetEdgeBetweenRooms(9, 10), 1);

            Assert.AreEqual(splitter.RoomComponentIndex(9), splitter.OriginComponentIndex);
            Assert.AreEqual(splitter.RoomComponentIndex(10), splitter.NonOriginComponentIndex);

        }

        [TestMethod]
        public void RoomsOnSideOfOriginInMultiplySplitMapAreInOneComponent()
        {

            ConnectivityMap standardMap = BuildStandardTestMap();
            var edgesToSplit = new List<TaggedEdge<int, string>>();
            edgesToSplit.Add(standardMap.GetEdgeBetweenRooms(10, 11));
            edgesToSplit.Add(standardMap.GetEdgeBetweenRooms(3, 5));

            MapSplitter splitter = new MapSplitter(standardMap.RoomConnectionGraph.Edges, edgesToSplit, 1);

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 1, 2, 3, 4, 7, 8, 9, 10 }), splitter.MapComponent(splitter.OriginComponentIndex).ToList());
        }

        [TestMethod]
        public void IntegrationTestMapModelCanBeBuiltWithAStartVertexInACycle()
        {
            var connectivityMap = BuildStandardTestMap();
            var mapModel = new MapModel(connectivityMap, 7);

            Assert.AreEqual(7, mapModel.StartVertex);
            Assert.AreEqual(2, mapModel.StartVertexNoCycleMap);
        }

        
        [TestMethod]
        public void ConnectionObjectsWithSameValueAreEqual()
        {
            var connection1 = new Connection(1, 2);
            var connection2 = new Connection(1, 2);

            Assert.AreEqual(connection1, connection2);
        
        }

        [TestMethod]
        public void NewConnectionObjectsCanBeUsedAsKeysInDictionaries()
        {
            var dict = new Dictionary<Connection, int>();

            dict.Add(new Connection(1, 2), 1);
            Assert.AreEqual(1, dict[new Connection(1, 2)]);
        }

        [TestMethod]
        public void ShortestPathBetweenVerticesCanBeFoundWhenItExists()
        {
            var connectivityMap = BuildStandardTestMap();
            var model = new MapModel(connectivityMap, 1);

            var shortestPath = model.GetPathBetweenVerticesInReducedMap(1, 15).ToList();

            var expectedPath = new List<Connection>(new Connection[] {
                new Connection(1, 2),
                new Connection(2, 10),
                new Connection(10, 11),
                new Connection(11, 13),
                new Connection(13, 15)
            });

            CollectionAssert.AreEqual(shortestPath, expectedPath);
        }

        [TestMethod]
        public void ShortestPathBetweenVerticesCanBeFoundWhenItExistsOnFullMap()
        {
            var connectivityMap = BuildStandardTestMap();
            var model = new MapModel(connectivityMap, 1);

            var shortestPath = model.GetPathBetweenVerticesInFullMap(1, 4).ToList();

            var expectedPath = new List<Connection>(new Connection[] {
                new Connection(1, 2),
                new Connection(2, 3),
                new Connection(3, 4)
            });

            CollectionAssert.AreEqual(shortestPath, expectedPath);
        }

        [TestMethod]
        public void ShortestPathBetweenVerticesCanBeFoundWhenItExistsOnFullMapThroughCycle()
        {
            var connectivityMap = BuildStandardTestMap();
            var model = new MapModel(connectivityMap, 1);

            //Only test the length since we don't know which way it will go
            var shortestPath = model.GetPathBetweenVerticesInFullMap(1, 11).ToList();

            Assert.AreEqual(5, shortestPath.Count());
        }

        [TestMethod]
        public void PathBetweenVerticesIsEquivalentWhenTravelledInEitherDirection()
        {
            var connectivityMap = BuildStandardTestMap();
            var model = new MapModel(connectivityMap, 1);

            var shortestPath = model.GetPathBetweenVerticesInReducedMap(15, 1).ToList();

            var expectedPath = new List<Connection>(new Connection[] {
                new Connection(1, 2),
                new Connection(2, 10),
                new Connection(10, 11),
                new Connection(11, 13),
                new Connection(13, 15)
            });

            //Expect to get back the same connections in a different order
            CollectionAssert.AreEquivalent(shortestPath, expectedPath);
        }

        [TestMethod]
        public void VertexDistanceFromSourceVertexCanBeFoundInReducedMap()
        {
            var connectivityMap = BuildStandardTestMap();
            var model = new MapModel(connectivityMap, 1);

            //Use the keys to get all vertices in the graph
            var allVerticesInReducedGraph = model.GraphNoCycles.roomMappingNoCycleToFullMap.Keys.ToList();
            var verticesAndDistances = model.GetDistanceOfVerticesFromParticularVertexInReducedMap(11, allVerticesInReducedGraph);

            var expectedDistance = new Dictionary<int, int>();

            expectedDistance.Add(1, 3);
            expectedDistance.Add(2, 2);
            expectedDistance.Add(3, 3);
            expectedDistance.Add(4, 4);
            expectedDistance.Add(5, 4);

            expectedDistance.Add(6, 5);

            expectedDistance.Add(10, 1);
            expectedDistance.Add(11, 0);
            expectedDistance.Add(12, 1);
            expectedDistance.Add(13, 1);
            expectedDistance.Add(14, 2);
            expectedDistance.Add(15, 2);

            CollectionAssert.AreEquivalent(verticesAndDistances, expectedDistance);
        }

        [TestMethod]
        public void VertexDistanceFromSourceVertexCanBeFound()
        {
            var connectivityMap = BuildStandardTestMap();
            var model = new MapModel(connectivityMap, 1);

            //Use the keys to get all vertices in the graph
            var allVerticesInFullGraph = model.BaseGraph.Vertices.ToList();
            var verticesAndDistances = model.GetDistanceOfVerticesFromParticularVertexInFullMap(11, allVerticesInFullGraph);

            var expectedDistance = new Dictionary<int, int>();

            expectedDistance.Add(1, 5);
            expectedDistance.Add(2, 4);
            expectedDistance.Add(3, 5);
            expectedDistance.Add(4, 6);
            expectedDistance.Add(5, 6);

            expectedDistance.Add(6, 7);

            expectedDistance.Add(7, 3);
            expectedDistance.Add(8, 3);
            expectedDistance.Add(9, 2);

            expectedDistance.Add(10, 1);
            expectedDistance.Add(11, 0);
            expectedDistance.Add(12, 1);
            expectedDistance.Add(13, 1);
            expectedDistance.Add(14, 2);
            expectedDistance.Add(15, 2);

            CollectionAssert.AreEquivalent(verticesAndDistances, expectedDistance);
        }

        [TestMethod]
        public void EmptyPathReturnedWhenNonExistentVertexUsed()
        {
            var connectivityMap = BuildStandardTestMap();
            var model = new MapModel(connectivityMap, 1);

            var shortestPath = model.GetPathBetweenVerticesInReducedMap(1, 20);

            CollectionAssert.AreEqual(shortestPath.ToList(), new List<Connection>());
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
