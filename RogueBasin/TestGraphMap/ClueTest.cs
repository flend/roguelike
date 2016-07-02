﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;
using System.Collections.Generic;

namespace TestGraphMap
{
    [TestClass]
    public class ClueTest
    {
        [TestMethod]
        public void ClueKnowsWhichRoomsItCanBePlacedIn()
        {
            var map = BuildStandardTestMap();
            var startVertex = 1;

            var mapModel = new MapModel(map, startVertex);
            var doorManager = new DoorAndClueManager(mapModel);
            var clue = doorManager.PlaceDoorAndClue(new DoorRequirements(new Connection(2, 10), "lock0"), 2);

            CollectionAssert.AreEquivalent(new List<int>(new int []{2, 7, 8, 9}), clue.PossibleClueRoomsInFullMap);
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
