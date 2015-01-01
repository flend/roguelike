using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;
using QuickGraph;
using System.Collections.Generic;
using System.Linq;

namespace TestGraphMap
{
    [TestClass]
    public class ConnectivityMapTest
    {
        [TestMethod]
        public void ConnectionCanBeAddedFromAnotherMap()
        {
            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            ConnectivityMap newMap2 = new ConnectivityMap();

            newMap2.AddRoomConnection(3, 4);
            newMap2.AddRoomConnection(4, 5);

            newMap.AddAllConnections(newMap2);

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(1, 2), new Connection(2, 3),
                new Connection(3, 4), new Connection(4, 5)}), newMap.GetAllConnections().ToList());
        }

        [TestMethod]
        public void ContainEdgeInMap()
        {
            //Build a simple graph

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);

            var edge = newMap.GetEdgeBetweenRooms(1, 2);

            Assert.AreEqual(edge.Source, 1);
            Assert.AreEqual(edge.Target, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException), "Edge not in map")]
        public void DoesNotContainEdgeNotInMap()
        {
            //Build a simple graph

            ConnectivityMap newMap = new ConnectivityMap();

            newMap.AddRoomConnection(1, 2);
            newMap.AddRoomConnection(2, 3);
            newMap.AddRoomConnection(3, 4);

            newMap.GetEdgeBetweenRooms(2, 4);
        }
    }
}
