using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using GraphMap;
using System.Collections.Generic;
using System.Linq;

namespace DDRogueTest
{
    [TestClass]
    public class MapInfoTest
    {
        [TestMethod]
        public void MapsFromDifferentLevelsCanBeConnected()
        {
            var mapInfo = new MapInfoBuilder();

            var l1ConnectivityMap = new ConnectivityMap();
            l1ConnectivityMap.AddRoomConnection(1, 2);
            l1ConnectivityMap.AddRoomConnection(2, 3);

            var l2ConnectivityMap = new ConnectivityMap();
            l2ConnectivityMap.AddRoomConnection(5, 6);
            l2ConnectivityMap.AddRoomConnection(6, 7);

            mapInfo.AddConstructedLevel(0, l1ConnectivityMap, new List<TemplatePositioned>(), 0);
            mapInfo.AddConstructedLevel(1, l2ConnectivityMap, new List<TemplatePositioned>(), new Connection(3, 5));

            ConnectivityMap fullMap = mapInfo.FullConnectivityMap;

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(1, 2), new Connection(2, 3),
                new Connection(3, 5), new Connection(5, 6), new Connection(6, 7)}), fullMap.GetAllConnections().ToList());
        }

        [TestMethod]
        public void RoomsCanBeRetrievedByIndex()
        {
            var newTemplate = new TemplatePositioned(9, 9, 0, null, 100);
            var mapInfoBuilder = new MapInfoBuilder();

            var templateList = new List<TemplatePositioned>();
            templateList.Add(newTemplate);

            var map = new ConnectivityMap();
            map.AddRoomConnection(new Connection(100, 101));

            mapInfoBuilder.AddConstructedLevel(0, map, templateList, 100);

            var mapInfo = new MapInfo(mapInfoBuilder);

            Assert.AreEqual(new Point(9,9), mapInfo.GetRoom(100).Location);
        }

        [TestMethod]
        public void RoomsInALevelCanBeReturned()
        {
            var mapInfo = new MapInfo(GetStandardMapInfoBuilder());

            var level1Nodes = mapInfo.GetRoomIndicesForLevel(1);

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 5, 6, 7 }), level1Nodes.ToList());
        }

        [TestMethod]
        public void ConnectionsInALevelCanBeReturned()
        {
            var mapInfo = new MapInfo(GetStandardMapInfoBuilder());

            var level1Nodes = mapInfo.GetConnectionsOnLevel(0);

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(1,2), new Connection(2,3), new Connection(3,5) }), level1Nodes.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void AddConstructedLevelCantBeCalledForFirstLevel()
        {
            var mapInfo = new MapInfoBuilder();
            mapInfo.AddConstructedLevel(0, new ConnectivityMap(), null, new Connection(1, 2));
        }

        private MapInfoBuilder GetStandardMapInfoBuilder() {
            var builder = new MapInfoBuilder();

            var l1ConnectivityMap = new ConnectivityMap();
            l1ConnectivityMap.AddRoomConnection(1, 2);
            l1ConnectivityMap.AddRoomConnection(2, 3);

            var l1RoomList = new List<TemplatePositioned>();
            l1RoomList.Add(new TemplatePositioned(1, 1, 0, null, 1));
            l1RoomList.Add(new TemplatePositioned(1, 1, 0, null, 2));
            l1RoomList.Add(new TemplatePositioned(1, 1, 0, null, 3));
            
            var l2ConnectivityMap = new ConnectivityMap();
            l2ConnectivityMap.AddRoomConnection(5, 6);
            l2ConnectivityMap.AddRoomConnection(6, 7);

            var l2RoomList = new List<TemplatePositioned>();
            l2RoomList.Add(new TemplatePositioned(1, 1, 0, null, 5));
            l2RoomList.Add(new TemplatePositioned(1, 1, 0, null, 6));
            l2RoomList.Add(new TemplatePositioned(1, 1, 0, null, 7));

            builder.AddConstructedLevel(0, l1ConnectivityMap, l1RoomList, 1);
            builder.AddConstructedLevel(1, l2ConnectivityMap, l2RoomList, new Connection(3, 5));

            return builder;
        }
    }
}
