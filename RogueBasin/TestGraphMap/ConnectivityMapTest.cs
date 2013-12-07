using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;
using QuickGraph;

namespace TestGraphMap
{
    [TestClass]
    public class ConnectivityMapTest
    {
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
