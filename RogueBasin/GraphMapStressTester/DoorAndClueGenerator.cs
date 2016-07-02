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

        private MapModel model;
        private DoorAndClueManager doorAndClueManager;

        public DoorAndClueGenerator(MapModel mapModel, DoorAndClueManager doorAndClueManager, Random rand)
        {
            this.random = rand;
            this.doorAndClueManager = doorAndClueManager;
            this.model = mapModel;
        }

        public MapModel Model { get { return model; } }
        public DoorAndClueManager DoorAndClueManager { get { return doorAndClueManager; } }

        public void AddDoorsAndClues(int numberDoorsToAdd, int cluesPerDoorMax)
        {
            var manager = doorAndClueManager;
            var numberDoors = 0;

            while (numberDoors < numberDoorsToAdd)
            {
                var doorName = "door" + numberDoors.ToString();
                var cluesToPlace = cluesPerDoorMax;
                var edgeToPlaceDoor = GetRandomEdgeInMap(model.FullMap);

                var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(edgeToPlaceDoor);
                
                manager.PlaceDoor(new DoorRequirements(edgeToPlaceDoor, doorName, cluesToPlace));
                
                for(int i = 0; i < cluesToPlace; i++)
                    manager.AddCluesToExistingDoor(doorName, new List<int> { allowedRoomsForClues.RandomElementUsing(random) });

                numberDoors++;
            }
        }

        private Connection GetRandomEdgeInMap(ConnectivityMap generatedMap) {
            return generatedMap.GetAllConnections().RandomElementUsing(random);
        }
    }
}
