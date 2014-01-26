using GraphMap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{

    public class MapGeneratorTemplated
    {


        /// <summary>
        /// Mapping from template terrain to real terrain on the map
        /// </summary>
        Dictionary<RoomTemplateTerrain, MapTerrain> terrainMapping;

        List<RoomTemplate> roomTemplates = new List<RoomTemplate>();
        List<RoomTemplate> corridorTemplates = new List<RoomTemplate>();

        TemplatedMapGenerator templatedGenerator;

        public MapGeneratorTemplated()
        {
            terrainMapping = new Dictionary<RoomTemplateTerrain, MapTerrain>();
            terrainMapping[RoomTemplateTerrain.Wall] = MapTerrain.Wall;
            terrainMapping[RoomTemplateTerrain.Floor] = MapTerrain.Empty;
            terrainMapping[RoomTemplateTerrain.Transparent] = MapTerrain.Void;
            terrainMapping[RoomTemplateTerrain.WallWithPossibleDoor] = MapTerrain.ClosedDoor;
        }

        private RoomTemplate RandomRoom()
        {
            return roomTemplates[Game.Random.Next(roomTemplates.Count)];
        }

        private RoomTemplate RandomCorridor()
        {
            return corridorTemplates[Game.Random.Next(corridorTemplates.Count)];
        }

        private TemplatedMapGenerator.DoorInfo RandomDoor(TemplatedMapGenerator generator)
        {
            return generator.PotentialDoors[Game.Random.Next(generator.PotentialDoors.Count())];
        }

        public ConnectivityMap ConnectivityMap
        {
            get
            {
                return templatedGenerator.ConnectivityMap;
            }
        }

        /** Build a map using templated rooms */
        public Map GenerateMap()
        {
            //Load sample templates
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            //Add to stores
            roomTemplates.Add(room1);
            corridorTemplates.Add(corridor1);

            //Create generator (guess initial cache size)
            var mapBuilder = new TemplatedMapBuilder(100, 100);
            templatedGenerator = new TemplatedMapGenerator(mapBuilder);

            int roomsToPlace = 20;
            int maxRoomDistance = 10;

            var roomsPlaced = PlaceRandomConnectedRooms(roomsToPlace, maxRoomDistance);

            //Add some extra connections, if doors are available
            var totalExtraConnections = 500;
            AddCorridorsBetweenOpenDoors(totalExtraConnections);

            Map masterMap = mapBuilder.MergeTemplatesIntoMap(terrainMapping);

            var firstRoom = mapBuilder.GetTemplate(0);
            masterMap.PCStartLocation = new Point(firstRoom.X - mapBuilder.MasterMapTopLeft.x + firstRoom.Room.Width / 2, firstRoom.Y - mapBuilder.MasterMapTopLeft.y + firstRoom.Room.Height / 2);

            LogFile.Log.LogEntryDebug("Player start location (map gen coords) " + new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2), LogDebugLevel.High);

            return masterMap;
        }

        /** Build a map using templated rooms */
        public Map GenerateMap2()
        {
            //Load sample templates
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            //Add to stores
            roomTemplates.Add(room1);
            corridorTemplates.Add(corridor1);

            //Create generator (guess initial cache size)
            var mapBuilder = new TemplatedMapBuilder(100, 100);
            templatedGenerator = new TemplatedMapGenerator(mapBuilder);

            int roomsToPlace = 100;
            int maxRoomDistance = 1;

            var roomsPlaced = PlaceRandomConnectedRooms(roomsToPlace, maxRoomDistance);

            //Add some extra connections, if doors are available
            var totalExtraConnections = 500;
            AddCorridorsBetweenOpenDoors(totalExtraConnections);

            //Replace spare doors with walls
            templatedGenerator.ReplaceDoorsWithTerrain(RoomTemplateTerrain.Wall);

            Map masterMap = mapBuilder.MergeTemplatesIntoMap(terrainMapping);

            var firstRoom = mapBuilder.GetTemplate(0);
            masterMap.PCStartLocation = new Point(firstRoom.X - mapBuilder.MasterMapTopLeft.x + firstRoom.Room.Width / 2, firstRoom.Y - mapBuilder.MasterMapTopLeft.y + firstRoom.Room.Height / 2);

            LogFile.Log.LogEntryDebug("Player start location (map gen coords) " + new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2), LogDebugLevel.High);

            return masterMap;
        }

        /** Build a map using templated rooms */
        public Map GenerateMapBranchRooms()
        {
            //Load sample templates
            RoomTemplate branchRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.branchroom.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber1Doors = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber7x3_1door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber2Doors = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber7x3_2door.room", StandardTemplateMapping.terrainMapping);
            
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            //Build a network of branched corridors

            //Add to stores
            roomTemplates.Add(branchRoom);
            corridorTemplates.Add(corridor1);

            //Create generator (guess initial cache size)
            var mapBuilder = new TemplatedMapBuilder(100, 100);
            templatedGenerator = new TemplatedMapGenerator(mapBuilder);

            //Place branch rooms to form the initial structure, joined on long axis
            PlaceOriginRoom(branchRoom);

            PlaceRandomConnectedRooms(3, branchRoom, corridor1, 0, 0, () => Game.Random.Next(1) > 0 ? 3 : 4);
           
            //Add some 2-door rooms
            PlaceRandomConnectedRooms(10, chamber2Doors, corridor1, 0, 0);

            //Add some 1-door deadends
            PlaceRandomConnectedRooms(10, chamber1Doors, corridor1, 0, 0);

            //Add some extra connections, if doors are available
            AddCorridorsBetweenOpenDoors(10);

            //Replace spare doors with walls
            templatedGenerator.ReplaceDoorsWithTerrain(RoomTemplateTerrain.Wall);

            /*
            //Add some extra connections, if doors are available
            var totalExtraConnections = 500;
            AddCorridorsBetweenOpenDoors(totalExtraConnections);

            //Replace spare doors with walls
            templatedGenerator.ReplaceDoorsWithTerrain(RoomTemplateTerrain.Wall);
            */
            Map masterMap = mapBuilder.MergeTemplatesIntoMap(terrainMapping);

            var firstRoom = mapBuilder.GetTemplate(0);
            masterMap.PCStartLocation = new Point(firstRoom.X - mapBuilder.MasterMapTopLeft.x + firstRoom.Room.Width / 2, firstRoom.Y - mapBuilder.MasterMapTopLeft.y + firstRoom.Room.Height / 2);

            LogFile.Log.LogEntryDebug("Player start location (map gen coords) " + new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2), LogDebugLevel.High);

            return masterMap;
        }

        private void AddCorridorsBetweenOpenDoors(int totalExtraConnections)
        {
            var extraConnections = 0;

            var allDoors = templatedGenerator.PotentialDoors;

            //Find all possible doors matches that aren't in the same room
            var allBendDoorPossibilities = from d1 in allDoors
                                           from d2 in allDoors
                                           where RoomTemplateUtilities.CanBeConnectedWithBendCorridor(d1.MapCoords, d1.DoorLocation, d2.MapCoords, d2.DoorLocation)
                                                 && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                           select new { origin = d1, target = d2 };

            var allLDoorPossibilities = from d1 in allDoors
                                        from d2 in allDoors
                                        where RoomTemplateUtilities.CanBeConnectedWithLShapedCorridor(d1.MapCoords, d1.DoorLocation, d2.MapCoords, d2.DoorLocation)
                                              && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                        select new { origin = d1, target = d2 };

            var allOverlappingDoorPossibilities = from d1 in allDoors
                                        from d2 in allDoors
                                        where d1.MapCoords == d2.MapCoords
                                              && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                        select new { origin = d1, target = d2 };


            //Materialize for speed

            var allMatchingDoorPossibilities = allBendDoorPossibilities.Union(allLDoorPossibilities).Union(allOverlappingDoorPossibilities).ToList();
            //var allMatchingDoorPossibilities = allLDoorPossibilities;
            //var allMatchingDoorPossibilities = allBendDoorPossibilities;

            var shuffleMatchingDoors = allMatchingDoorPossibilities.Shuffle(Game.Random);
            
            for (int i = 0; i < allMatchingDoorPossibilities.Count; i++)
            {
                //Try a random combination to see if it works
                var doorsToTry = shuffleMatchingDoors.ElementAt(i);

                LogFile.Log.LogEntryDebug("Trying door " + doorsToTry.origin.MapCoords + " to " + doorsToTry.target.MapCoords, LogDebugLevel.Medium);

                bool success = templatedGenerator.JoinDoorsWithCorridor(doorsToTry.origin, doorsToTry.target, RandomCorridor());
                if (success)
                    extraConnections++;

                if (extraConnections > totalExtraConnections)
                    break;
            }

            //Previous code (was super-slow!)
            //while (allMatchingDoorPossibilities.Any() && extraConnections < totalExtraConnections)
            //In any case, remove this attempt
            //var doorsToTry = allMatchingDoorPossibilities.ElementAt(Game.Random.Next(allMatchingDoorPossibilities.Count()));
            //allMatchingDoorPossibilities = allMatchingDoorPossibilities.Except(Enumerable.Repeat(doorsToTry, 1)); //order n - making it slow?
                
        }

        private int PlaceRandomConnectedRooms(int roomsToPlace, int maxRoomDistance)
        {
            int roomsPlaced = 0;
            int attempts = 0;

            //This uses random distances and their might be collisions so we should avoid infinite loops
            int maxAttempts = roomsToPlace * 5;

            //Terminate when all rooms placed or no more potential door sites
            do
            {
                if (roomsPlaced == 0)
                {
                    //Place a random room at a location near the origin
                    bool success = templatedGenerator.PlaceRoomTemplateAtPosition(RandomRoom(), new Point(Game.Random.Next(maxRoomDistance), Game.Random.Next(maxRoomDistance)));
                    if (success)
                        roomsPlaced++;
                }
                else
                {
                    //Find a random potential door and try to grow a random room off this
                    if(templatedGenerator.PlaceRoomTemplateAlignedWithExistingDoor(RandomRoom(), corridorTemplates[0], RandomDoor(templatedGenerator),
                        Game.Random.Next(maxRoomDistance)))
                        roomsPlaced++;

                        attempts++;
                }
            } while (roomsPlaced < roomsToPlace && attempts < maxAttempts && templatedGenerator.HaveRemainingPotentialDoors());
            return roomsPlaced;
        }

        private bool PlaceOriginRoom(RoomTemplate roomToPlace)
        {
            return templatedGenerator.PlaceRoomTemplateAtPosition(roomToPlace, new Point(0, 0));
        }

        private int PlaceRandomConnectedRooms(int roomsToPlace, RoomTemplate roomToPlace, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength)
        {
            return PlaceRandomConnectedRooms(roomsToPlace, roomToPlace, corridorToPlace, minCorridorLength, maxCorridorLength, null);
        }

        private int PlaceRandomConnectedRooms(int roomsToPlace, RoomTemplate roomToPlace, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength, Func<int> doorPicker)
        {
            int roomsPlaced = 0;
            int attempts = 0;

            //This uses random distances and their might be collisions so we should avoid infinite loops
            int maxAttempts = roomsToPlace * 5;

            //Terminate when all rooms placed or no more potential door sites
            do
            {

                //Find a random potential door and try to grow a random room off this
                
                //Use a random door, or the function passed in
                int randomNewDoorIndex;
                if (doorPicker == null)
                {
                    //Random door
                    randomNewDoorIndex = Game.Random.Next(roomToPlace.PotentialDoors.Count);
                }
                else
                    randomNewDoorIndex = doorPicker();

                int corridorLength = Game.Random.Next(minCorridorLength, maxCorridorLength);

                if (templatedGenerator.PlaceRoomTemplateAlignedWithExistingDoor(roomToPlace, corridorToPlace, RandomDoor(templatedGenerator), randomNewDoorIndex,
                    corridorLength))
                    roomsPlaced++;

                attempts++;

            } while (roomsPlaced < roomsToPlace && attempts < maxAttempts && templatedGenerator.HaveRemainingPotentialDoors());
            return roomsPlaced;
        }

        
    }

    public static class ShuffleExtension
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                // ... except we don't really need to swap it fully, as we can
                // return it immediately, and afterwards it's irrelevant.
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }
    }
}
