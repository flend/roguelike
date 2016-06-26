﻿using GraphMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueBasin;

namespace GraphMapStressTester
{
    class DoorAndObjectiveGenerator
    {
        Random random;
        private MapModel model;
        private DoorAndClueManager doorAndClueManager;

        public DoorAndObjectiveGenerator(MapModel mapModel, DoorAndClueManager doorAndClueManager, Random rand)
        {
            this.random = rand;
            this.doorAndClueManager = doorAndClueManager;
            this.model = mapModel;
        }

        public MapModel Model { get { return model; } }
        public DoorAndClueManager DoorAndClueManager { get { return doorAndClueManager; } }

        public MapModel AddDoorsAndObjectives(MapModel mapModel, int numberDoorsToAdd, int cluesPerDoorMax)
        {
            var manager = doorAndClueManager;
            var numberDoors = 0;

            while (numberDoors < numberDoorsToAdd)
            {
                var doorName = "door" + numberDoors.ToString();
                var cluesToPlace = cluesPerDoorMax;
                var edgeToPlaceDoor = GetRandomEdgeInMap(mapModel.FullMap);

                var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(edgeToPlaceDoor);
                
                //Doors are triggered by a chain of objectives
                manager.PlaceDoor(new DoorRequirements(edgeToPlaceDoor, doorName, 1));

                var cluesPlaced = 0;
                var lastLock = doorName;
                while (cluesPlaced < cluesToPlace)
                {
                    var objectiveName = "obj-" + doorName + "-" + cluesPlaced.ToString();
                    manager.PlaceObjective(new ObjectiveRequirements(allowedRoomsForClues.RandomElementUsing(random), objectiveName, 1, new List<string> { lastLock }));
                    cluesPlaced++;
                    lastLock = objectiveName;
                }

                var roomForObjectiveClue = allowedRoomsForClues.RandomElementUsing(random);
                manager.AddCluesToExistingObjective(lastLock, new List<int> { roomForObjectiveClue });

                numberDoors++;
            }

            return mapModel;
        }

        private Connection GetRandomEdgeInMap(ConnectivityMap generatedMap) {
            return generatedMap.GetAllConnections().RandomElementUsing(random);
        }
    }
}
