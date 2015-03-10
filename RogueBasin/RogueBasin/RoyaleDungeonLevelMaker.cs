using GraphMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    class RoyaleDungeonLevelMaker
    {
        public class LevelInfo
        {
            public LevelInfo(int levelNo)
            {
                LevelNo = levelNo;

                ConnectionsToOtherLevels = new Dictionary<int, Connection>();
                ReplaceableVaultConnections = new List<Connection>();
                ReplaceableVaultConnectionsUsed = new List<Connection>();
            }

            public int LevelNo { get; private set; }

            public Dictionary<int, Connection> ConnectionsToOtherLevels { get; set; }

            public TemplatedMapGenerator LevelGenerator { get; set; }
            public TemplatedMapBuilder LevelBuilder { get; set; }

            //Replaceable vault at target
            public List<Connection> ReplaceableVaultConnections { get; set; }
            public List<Connection> ReplaceableVaultConnectionsUsed { get; set; }

            public Dictionary<MapTerrain, List<MapTerrain>> TerrainMapping { get; set; }
        }

        /// <summary>
        /// Numeral of the first dungeon in the next choices
        /// </summary>
        public int NextDungeonLevelChoice { get; private set; }

        /// <summary>
        /// Number of dungeons available in next choice
        /// </summary>
        /// <param name="numberOfLevels"></param>
        public int NumberDungeonLevelChoices { get; private set; }

        private MapInfoBuilder mapInfoBuilder;

        private MapInfo mapInfo;

        Dictionary<RoomTemplateTerrain, MapTerrain> terrainMapping = new Dictionary<RoomTemplateTerrain, MapTerrain>();

        private static int vertexOffsetPerLevel = 100;

        public RoyaleDungeonLevelMaker()
        {
            NumberDungeonLevelChoices = 3;
            mapInfoBuilder = new MapInfoBuilder();
            BuildTerrainMapping();
        }

        public void CreateNextDungeonChoices()
        {
            NextDungeonLevelChoice = Game.Dungeon.NoLevels;
            NumberDungeonLevelChoices = NumberDungeonLevelChoices;

            //Construct new levels
            for (int i = NextDungeonLevelChoice; i < NextDungeonLevelChoice + NumberDungeonLevelChoices; i++)
            {
                LogFile.Log.LogEntryDebug("Generating level: " + i, LogDebugLevel.Medium);
                LevelInfo standardLevel = GenerateStandardLevel(i, i * vertexOffsetPerLevel);
                AddLevel(standardLevel);
            }

            //Set maps in engine (needs to be done before placing items and monsters)
            SetupNewMapsInEngine();

            //Setup monsters and items
        }

        public void SetPlayerStartLocation()
        {
            //Set player's start location (must be done before adding items)
            var firstRoom = mapInfo.GetRoom(0);
            Game.Dungeon.Levels[0].PCStartLocation = new RogueBasin.Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);
        }

        private void AddLevel(LevelInfo newLevel)
        {
            //Adding as unconnected maps, all starting at room 0
            mapInfoBuilder.AddConstructedLevel(0, newLevel.LevelGenerator.ConnectivityMap, newLevel.LevelGenerator.GetRoomTemplatesInWorldCoords(),
                newLevel.LevelGenerator.GetDoorsInMapCoords(), newLevel.LevelNo * vertexOffsetPerLevel);

            Map masterMap = newLevel.LevelBuilder.MergeTemplatesIntoMap(terrainMapping);
            Game.Dungeon.AddMap(masterMap);

            //It's not clear if we need this with unconnected levels
            //Rebuild each time
            mapInfo = new MapInfo(mapInfoBuilder);
            Game.Dungeon.MapInfo = mapInfo;
        }

        private void SetupNewMapsInEngine()
        {
            for (int i = NextDungeonLevelChoice; i < NextDungeonLevelChoice + NumberDungeonLevelChoices; i++)
            {
                Game.Dungeon.RefreshLevelPathingAndFOV(i);

                //Set light level
                Game.Dungeon.Levels[i].LightLevel = 0;
            }
        }

        private void BuildTerrainMapping()
        {
            terrainMapping = new Dictionary<RoomTemplateTerrain, MapTerrain>();
            terrainMapping[RoomTemplateTerrain.Wall] = MapTerrain.Wall;
            terrainMapping[RoomTemplateTerrain.Floor] = MapTerrain.Empty;
            terrainMapping[RoomTemplateTerrain.Transparent] = MapTerrain.Void;
            terrainMapping[RoomTemplateTerrain.WallWithPossibleDoor] = MapTerrain.ClosedDoor;
        }

        private LevelInfo GenerateStandardLevel(int levelNo, int startVertexIndex)
        {
            var medicalInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate deadEnd = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber3x3_1door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate medicalBay = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.medical_bay1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, medicalBay);

            int numberOfRandomRooms = 10;

            BuildTXShapedRoomsBig(templateGenerator, numberOfRandomRooms);

            //Add extra corridors
            AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            return medicalInfo;
        }

        private int PlaceOriginRoom(TemplatedMapGenerator templatedGenerator, RoomTemplate roomToPlace)
        {
            return templatedGenerator.PlaceRoomTemplateAtPosition(roomToPlace, new RogueBasin.Point(0, 0));
        }

        private void BuildTXShapedRoomsBig(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate lshapeRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.lshape3.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate lshapeAsymmetric = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.lshape_asymmetric3.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate tshape = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.tshape2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate xshape = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.xshape2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>> { 
                new Tuple<int, RoomTemplate>(100, lshapeRoom),
                new Tuple<int, RoomTemplate>(100, lshapeAsymmetric),
                new Tuple<int, RoomTemplate>(100, tshape),
                new Tuple<int, RoomTemplate>(100, xshape) };

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 4, 6);
        }


        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, RoomTemplate roomToPlace, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength)
        {
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, roomToPlace, corridorToPlace, minCorridorLength, maxCorridorLength, null);
        }


        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, RoomTemplate roomToPlace, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength, Func<int> doorPicker)
        {
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, new List<Tuple<int, RoomTemplate>> { new Tuple<int, RoomTemplate>(1, roomToPlace) }, corridorToPlace, minCorridorLength, maxCorridorLength, doorPicker);
        }

        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, List<Tuple<int, RoomTemplate>> roomToPlaceWithWeights, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength)
        {
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, roomToPlaceWithWeights, corridorToPlace, minCorridorLength, maxCorridorLength, null);
        }


        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, List<RoomTemplate> roomToPlaces, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength)
        {
            var tuples = roomToPlaces.Select(r => new Tuple<int, RoomTemplate>(1, r));
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, tuples, corridorToPlace, minCorridorLength, maxCorridorLength, null);
        }

        /// <summary>
        /// Failure mode is placing fewer rooms than requested
        /// </summary>
        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, IEnumerable<Tuple<int, RoomTemplate>> roomsToPlaceWithWeights, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength, Func<int> doorPicker)
        {
            int roomsPlaced = 0;
            int attempts = 0;

            //This uses random distances and their might be collisions so we should avoid infinite loops
            int maxAttempts = roomsToPlace * 5;

            //Terminate when all rooms placed or no more potential door sites
            do
            {
                //Find a random potential door and try to grow a random room off this

                //Find room using weights
                var roomToPlace = ChooseItemFromWeights<RoomTemplate>(roomsToPlaceWithWeights);

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

                try
                {
                    templatedGenerator.PlaceRoomTemplateAlignedWithExistingDoor(roomToPlace, corridorToPlace, RandomDoor(templatedGenerator), randomNewDoorIndex,
                    corridorLength);

                    roomsPlaced++;
                }
                catch (ApplicationException) { }

                attempts++;

            } while (roomsPlaced < roomsToPlace && attempts < maxAttempts && templatedGenerator.HaveRemainingPotentialDoors());
            return roomsPlaced;
        }

        private T ChooseItemFromWeights<T>(IEnumerable<Tuple<int, T>> itemsWithWeights)
        {
            var totalWeight = itemsWithWeights.Select(t => t.Item1).Sum();
            var randomNumber = Game.Random.Next(totalWeight);

            int weightSoFar = 0;
            T roomToPlace = itemsWithWeights.First().Item2;
            foreach (var t in itemsWithWeights)
            {
                weightSoFar += t.Item1;
                if (weightSoFar > randomNumber)
                {
                    roomToPlace = t.Item2;
                    break;
                }
            }

            return roomToPlace;
        }

        private DoorInfo RandomDoor(TemplatedMapGenerator generator)
        {
            return generator.PotentialDoors[Game.Random.Next(generator.PotentialDoors.Count())];
        }

        private void AddCorridorsBetweenOpenDoors(TemplatedMapGenerator templatedGenerator, int totalExtraConnections, List<RoomTemplate> corridorsToUse)
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

            var allStraightDoorPossibilities = from d1 in allDoors
                                               from d2 in allDoors
                                               where RoomTemplateUtilities.CanBeConnectedWithStraightCorridor(d1.MapCoords, d1.DoorLocation, d2.MapCoords, d2.DoorLocation)
                                                     && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                               select new { origin = d1, target = d2 };

            var allOverlappingDoorPossibilities = from d1 in allDoors
                                                  from d2 in allDoors
                                                  where d1.MapCoords == d2.MapCoords
                                                        && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                                  select new { origin = d1, target = d2 };


            //Materialize for speed

            var allMatchingDoorPossibilities = allBendDoorPossibilities.Union(allLDoorPossibilities).Union(allOverlappingDoorPossibilities).Union(allStraightDoorPossibilities).ToList();

            var shuffleMatchingDoors = allMatchingDoorPossibilities.Shuffle(Game.Random);

            for (int i = 0; i < allMatchingDoorPossibilities.Count; i++)
            {
                //Try a random combination to see if it works
                var doorsToTry = shuffleMatchingDoors.ElementAt(i);

                LogFile.Log.LogEntryDebug("Trying door " + doorsToTry.origin.MapCoords + " to " + doorsToTry.target.MapCoords, LogDebugLevel.Medium);

                bool success = templatedGenerator.JoinDoorsWithCorridor(doorsToTry.origin, doorsToTry.target, corridorsToUse.RandomElement());
                if (success)
                    extraConnections++;

                if (extraConnections > totalExtraConnections)
                    break;
            }
        }

    }

}
