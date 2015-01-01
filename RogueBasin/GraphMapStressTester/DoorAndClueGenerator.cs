using GraphMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueBasin;

namespace GraphMapStressTester
{
    class DoorAndClueGenerator
    {
        Random random;

        public DoorAndClueGenerator(Random rand)
        {
            this.random = rand;
        }

        public MapModel AddDoorsAndClues(MapModel mapModel, int numberDoorsToAdd, int cluesPerDoorMax)
        {
            var manager = mapModel.DoorAndClueManager;
            var numberDoors = 0;

            while (numberDoors < numberDoorsToAdd)
            {
                var doorName = "door" + numberDoors.ToString();
                var cluesToPlace = cluesPerDoorMax;
                var edgeToPlaceDoor = GetRandomEdgeInMap(mapModel.FullMap);

                var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(edgeToPlaceDoor);
                
                manager.PlaceDoor(new DoorRequirements(edgeToPlaceDoor, doorName, cluesToPlace));
                
                for(int i = 0; i < cluesToPlace; i++)
                    manager.AddCluesToExistingDoor(doorName, new List<int> { allowedRoomsForClues.RandomElementUsing(random) });

                numberDoors++;
            }

            return mapModel;
        }

        private Connection GetRandomEdgeInMap(ConnectivityMap generatedMap) {
            return generatedMap.GetAllConnections().RandomElementUsing(random);
        }
    }
}
