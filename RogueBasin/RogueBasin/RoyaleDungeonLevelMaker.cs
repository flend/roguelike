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

            public Connection EntryBoothConnection { get; set; }
            public Connection ExitBoothConnection { get; set; }

            public Dictionary<MapTerrain, List<MapTerrain>> TerrainMapping { get; set; }

            public Point EntryLocation { get; set; }
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

        Dictionary<int, LevelInfo> levels = new Dictionary<int, LevelInfo>();

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

            //Setup entry and exit
            for (int i = NextDungeonLevelChoice; i < NextDungeonLevelChoice + NumberDungeonLevelChoices; i++)
            {
                SetEntryLocation(levels[i]);
                PlaceEntryAndExitDoors(levels[i]);
            }

            //Add monsters
            for (int i = NextDungeonLevelChoice; i < NextDungeonLevelChoice + NumberDungeonLevelChoices; i++)
            {
                List<Monster> punks = new List<Monster> { new Creatures.Punk(1), new Creatures.Punk(1), new Creatures.Punk(1) };
                AddMonstersToRoom(mapInfo, i, 0, punks);
            }
            
        }

        private void SetEntryLocation(LevelInfo levelInfo)
        {
            var entryRoom = mapInfo.GetRoom(levelInfo.EntryBoothConnection.Target);
            levelInfo.EntryLocation = new RogueBasin.Point(entryRoom.X + entryRoom.Room.Width / 2, entryRoom.Y + entryRoom.Room.Height / 2); 
        }

        public Point GetEntryLocationOnLevel(int levelNo)
        {
            return levels[levelNo].EntryLocation;
        }

        public void SetPlayerStartLocation()
        {
            //Set player's start location (must be done before adding items)
            Game.Dungeon.Levels[0].PCStartLocation = GetEntryLocationOnLevel(0);
        }

        private void AddLevel(LevelInfo newLevel)
        {
            levels[newLevel.LevelNo] = newLevel;

            //Adding as unconnected maps, all starting at room 0
            mapInfoBuilder.AddConstructedLevel(newLevel.LevelNo, newLevel.LevelGenerator.ConnectivityMap, newLevel.LevelGenerator.GetRoomTemplatesInWorldCoords(),
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
            var levelInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate xshape = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.xshape2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate largeOval = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.oval_vault1.room", StandardTemplateMapping.terrainMapping);

            //Entry/exit booth
            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            levelInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            levelInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, largeOval);

            //Connections are from the level TO the booth
            int distanceToConnectingDoor = 1;
            levelInfo.EntryBoothConnection = AddVaults(templateGenerator, corridor1, distanceToConnectingDoor, replacementVault, 0).First();
            
            int numberOfRandomRooms = 3;

            BuildCircularRooms(templateGenerator, numberOfRandomRooms);

            //Add exit booth
            
            levelInfo.ExitBoothConnection = AddVaults(templateGenerator, corridor1, distanceToConnectingDoor, replacementVault, 0).First();

            //Add extra corridors
            //AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            return levelInfo;
        }

        private List<Connection> AddVaults(TemplatedMapGenerator templateGenerator, RoomTemplate corridor1, int distanceToConnectingDoor, RoomTemplate placeHolderVault, int maxPlaceHolders)
        {
            return AddVaults(templateGenerator, corridor1, distanceToConnectingDoor, new List<RoomTemplate> { placeHolderVault }, maxPlaceHolders);
        }

        private List<Connection> AddVaults(TemplatedMapGenerator templateGenerator, RoomTemplate corridor1, int distanceToConnectingDoor, List<RoomTemplate> placeHolderVault, int maxVaults)
        {
            var vaultsToReturn = new List<Connection>();
            int cargoTotalPlaceHolders = 0;
            do
            {
                var placeHolderRoom = AddRoomToRandomOpenDoor(templateGenerator, placeHolderVault.RandomElement(), corridor1, distanceToConnectingDoor);
                if (placeHolderRoom != null)
                {
                    vaultsToReturn.Add(placeHolderRoom);
                    cargoTotalPlaceHolders++;
                }
                else
                    break;
            } while (cargoTotalPlaceHolders < maxVaults);
            return vaultsToReturn;
        }

        Connection AddRoomToRandomOpenDoor(TemplatedMapGenerator gen, RoomTemplate templateToPlace, RoomTemplate corridorTemplate, int distanceFromDoor)
        {
            var doorsToTry = gen.PotentialDoors.Shuffle();

            foreach (var door in doorsToTry)
            {
                try
                {
                    return gen.PlaceRoomTemplateAlignedWithExistingDoor(templateToPlace, corridorTemplate, RandomDoor(gen), 0, distanceFromDoor);
                }
                catch (ApplicationException)
                {
                    //No good, continue
                }
            }

            throw new ApplicationException("No applicable doors left");
        }

        private void PlaceEntryAndExitDoors(LevelInfo levelInfo)
        {
            var entryConnection = levelInfo.EntryBoothConnection;
            var exitConnection = levelInfo.ExitBoothConnection;
            
            //Place entry door
            var doorInfo = mapInfo.GetDoorForConnection(entryConnection);

            var entryDoor = new RogueBasin.Locks.EntryLock(levelInfo.LevelNo);
            entryDoor.LocationLevel = doorInfo.LevelNo;
            entryDoor.LocationMap = doorInfo.MapLocation;

            Game.Dungeon.AddLock(entryDoor);

            LogFile.Log.LogEntryDebug("Placing entry booth door for: level: " + levelInfo.LevelNo + " at: " + entryConnection, LogDebugLevel.Medium);

            //Place entry door
            doorInfo = mapInfo.GetDoorForConnection(exitConnection);

            var exitDoor = new RogueBasin.Locks.ExitLock(levelInfo.LevelNo);
            exitDoor.LocationLevel = doorInfo.LevelNo;
            exitDoor.LocationMap = doorInfo.MapLocation;

            Game.Dungeon.AddLock(exitDoor);

            LogFile.Log.LogEntryDebug("Placing exit booth door for: level: " + levelInfo.LevelNo + " at: " + entryConnection, LogDebugLevel.Medium);

        }

        private int PlaceOriginRoom(TemplatedMapGenerator templatedGenerator, RoomTemplate roomToPlace)
        {
            return templatedGenerator.PlaceRoomTemplateAtPosition(roomToPlace, new RogueBasin.Point(0, 0));
        }

        private void BuildCircularRooms(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate mediumOval = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_vault_oval1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate smallOval = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.oval_vault_small1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>> { 
                new Tuple<int, RoomTemplate>(200, smallOval),
                new Tuple<int, RoomTemplate>(100, mediumOval) };

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 0, 1);
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

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 0, 1);
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

        public int AddMonstersToRoom(MapInfo mapInfo, int level, int roomId, IEnumerable<Monster> monsters)
        {
            var candidatePointsInRoom = mapInfo.GetAllPointsInRoomOfTerrain(roomId, RoomTemplateTerrain.Floor).Shuffle();

            int monstersPlaced = 0;

            Monster mon = monsters.ElementAt(monstersPlaced);

            foreach (Point p in candidatePointsInRoom)
            {
                bool placedSuccessfully = Game.Dungeon.AddMonster(mon, level, p);

                if (placedSuccessfully)
                {
                    monstersPlaced++;

                    if (monstersPlaced == monsters.Count())
                        break;

                    mon = monsters.ElementAt(monstersPlaced);
                }
            }
            return monstersPlaced;
        }
    }

}
