﻿using GraphMap;
using RogueBasin.TerrainSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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

        public void CreateNextDungeonChoices(int basePlayerLevel)
        {
            NextDungeonLevelChoice = Game.Dungeon.NoLevels;
            NumberDungeonLevelChoices = NumberDungeonLevelChoices;

            var playerLevelsForDungeons = Enumerable.Range(basePlayerLevel, NumberDungeonLevelChoices);
            var levelNumbers = Enumerable.Range(NextDungeonLevelChoice, NumberDungeonLevelChoices);

            //Construct new levels
            for (int i = NextDungeonLevelChoice; i < NextDungeonLevelChoice + NumberDungeonLevelChoices; i++)
            {
                LogFile.Log.LogEntryDebug("Generating level: " + i, LogDebugLevel.Medium);
                LevelInfo standardLevel = GenerateStandardLevel(i, i * vertexOffsetPerLevel);
                AddLevel(standardLevel);
            }

            //Set maps in engine (needs to be done before placing items and monsters)
            SetupNewMapsInEngine();

            for (int i = NextDungeonLevelChoice; i < NextDungeonLevelChoice + NumberDungeonLevelChoices; i++)
            {
                //Setup entry and exit
                SetEntryLocation(levels[i]);
                PlaceEntryAndExitDoors(levels[i]);

                //Add intra-room terrain
                AddDecorationFeatures(mapInfo, levels[i]);
            }

            RoyaleMonsterPlacement monPlacement = new RoyaleMonsterPlacement();

            //Add monsters
            var playerEntryLocations = levelNumbers.Select(n => levels[n].EntryLocation);
            monPlacement.CreateMonstersForLevels(mapInfo, levelNumbers.Zip(playerLevelsForDungeons, Tuple.Create), playerEntryLocations);

            //hacks
            for (int i = NextDungeonLevelChoice; i < NextDungeonLevelChoice + NumberDungeonLevelChoices; i++)
            {
                List<Monster> punks = new List<Monster> { //new Creatures.Punk(1), new Creatures.Punk(1), new Creatures.Punk(1), new Creatures.Thug(1), 
                new Creatures.ArenaMaster(21) };
                //AddMonstersToRoom(mapInfo, i, 0, punks);
               

                List<Item> items = new List<Item> { new Items.FragGrenade(), new Items.FragGrenade(), new Items.FragGrenade(), new Items.FragGrenade(), new Items.FragGrenade(),
                //new Items.StunGrenade(), new Items.StunGrenade(), new Items.StunGrenade(), new Items.StunGrenade(), new Items.StunGrenade(),
                new Items.RocketLauncher(), new Items.RocketLauncher(), new Items.RocketLauncher(), new Items.AcidGrenade(), new Items.AcidGrenade(), new Items.AcidGrenade(), new Items.AcidGrenade(), new Items.AcidGrenade(), new Items.AcidGrenade()};
                //AddItemsToRoom(mapInfo, i, 0, items);
            }

        }

        private void SetEntryLocation(LevelInfo levelInfo)
        {
            var entryRoom = mapInfo.Room(levelInfo.EntryBoothConnection.Target);
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
            mapInfo = new MapInfo(mapInfoBuilder, new MapPopulator());
            Game.Dungeon.MapInfo = mapInfo;
        }

        private void SetupNewMapsInEngine()
        {
            for (int i = NextDungeonLevelChoice; i < NextDungeonLevelChoice + NumberDungeonLevelChoices; i++)
            {
                Game.Dungeon.RefreshLevelPathingAndFOV(i);

                //Set light level
                Game.Dungeon.Levels[i].LightLevel = 8;
            }
            Game.Dungeon.Player.CalculateSightRadius();
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
            RoomTemplate verylargeOvalArena = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.basic_large_arena.room", StandardTemplateMapping.terrainMapping);

            //Entry/exit booth
            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            levelInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            levelInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, verylargeOvalArena);

            //Connections are from the level TO the booth
            int distanceToConnectingDoor = 0;
            levelInfo.EntryBoothConnection = AddVaults(templateGenerator, corridor1, distanceToConnectingDoor, replacementVault, 0).First();

            //int numberOfRandomRooms = 3;

            //BuildCircularRooms(templateGenerator, numberOfRandomRooms);

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

            //Place trigger to close door - to do

            
            LogFile.Log.LogEntryDebug("Placing entry booth door for: level: " + levelInfo.LevelNo + " at: " + entryConnection, LogDebugLevel.Medium);

            //Place exit door
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
            // RoomTemplate mediumOval = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_vault_oval1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate smallOval = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.oval_vault_small1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>> { 
                new Tuple<int, RoomTemplate>(200, smallOval) };
            //,
            //new Tuple<int, RoomTemplate>(100, mediumOval) };

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

        public int AddItemsToRoom(MapInfo mapInfo, int level, int roomId, IEnumerable<Item> items)
        {
            var candidatePointsInRoom = mapInfo.GetAllPointsInRoomOfTerrain(roomId, RoomTemplateTerrain.Floor).Shuffle();

            int ItemsPlaced = 0;

            Item mon = items.ElementAt(ItemsPlaced);

            foreach (Point p in candidatePointsInRoom)
            {
                bool placedSuccessfully = Game.Dungeon.AddItem(mon, level, p);

                if (placedSuccessfully)
                {
                    ItemsPlaced++;

                    if (ItemsPlaced == items.Count())
                        break;

                    mon = items.ElementAt(ItemsPlaced);
                }
            }
            return ItemsPlaced;
        }


        private static void AddExistingBlockingFeaturesToRoomFiller(int level, TemplatePositioned positionedRoom, RoomFilling bridgeRouter)
        {
            var floorPointsInRoom = RoomTemplateUtilities.GetPointsInRoomWithTerrain(positionedRoom.Room, RoomTemplateTerrain.Floor).Select(p => p + positionedRoom.Location);
            foreach (var roomPoint in floorPointsInRoom)
            {
                if (Game.Dungeon.BlockingFeatureAtLocation(level, roomPoint))
                {
                    var stillWalkable = bridgeRouter.SetSquareUnWalkableIfMaintainsConnectivity(roomPoint - positionedRoom.Location);

                    if (!stillWalkable)
                    {
                        LogFile.Log.LogEntryDebug("Room " + positionedRoom.RoomIndex + " appears unconnected.", LogDebugLevel.High);
                    }
                }
            }
        }

        T RandomItem<T>(IEnumerable<T> items)
        {
            var totalItems = items.Count();
            if (totalItems == 0)
                throw new ApplicationException("Empty list for randomization");

            return items.ElementAt(Game.Random.Next(totalItems));
        }

        private void AddNonBlockingFeaturesToLevel(int level, IEnumerable<Point> mapPoints, Func<Feature> featureGenerator)
        {
            foreach (Point mapPoint in mapPoints)
            {
                var featureToPlace = featureGenerator();
                if (featureToPlace.IsBlocking)
                {
                    throw new ApplicationException("Can't use this function for blocking features");
                }

                bool result = Game.Dungeon.AddFeature(featureToPlace, level, mapPoint);

                if (result)
                    LogFile.Log.LogEntryDebug("Placing feature " + featureToPlace.Description + " at location " + mapPoint, LogDebugLevel.Medium);
            }
        }

        private void AddFeaturesToRoom(int level, TemplatePositioned positionedRoom, IEnumerable<Point> pointsToPlaceTerrain, Func<Feature> featureGenerator)
        {
            var roomFiller = new RoomFilling(positionedRoom.Room);

            //Need to account for all current blocking features in room
            AddExistingBlockingFeaturesToRoomFiller(level, positionedRoom, roomFiller);

            foreach (Point roomPoint in pointsToPlaceTerrain)
            {
                try
                {
                    var featureLocationInMapCoords = positionedRoom.Location + roomPoint;
                    var featureToPlace = featureGenerator();

                    if (!featureToPlace.IsBlocking)
                    {
                        bool result = Game.Dungeon.AddFeature(featureToPlace, level, featureLocationInMapCoords);

                        if (result)
                            LogFile.Log.LogEntryDebug("Placing feature " + featureToPlace.Description + " in room " + positionedRoom.RoomIndex + " at location " + featureLocationInMapCoords, LogDebugLevel.Medium);
                    }
                    else if (roomFiller.SetSquareUnWalkableIfMaintainsConnectivity(roomPoint))
                    {
                        bool result = Game.Dungeon.AddFeatureBlocking(featureToPlace, level, featureLocationInMapCoords, false);

                        if (result)
                        {
                            LogFile.Log.LogEntryDebug("Placing blocking feature in room " + positionedRoom.RoomIndex + " at location " + featureLocationInMapCoords + " room wise " + roomPoint, LogDebugLevel.Medium);
                        }
                    }
                }
                catch (ApplicationException ex)
                {
                    LogFile.Log.LogEntryDebug("Unable to add feature in room " + ex.Message, LogDebugLevel.Medium);
                }
            }
        }

        private void AddStandardDecorativeFeaturesToRoom(int level, TemplatePositioned positionedRoom, IEnumerable<Point> pointsToPlaceTerrain, IEnumerable<Tuple<int, DecorationFeatureDetails.Decoration>> decorationDetails, bool blocksLight, bool isSoft)
        {
            var roomFiller = new RoomFilling(positionedRoom.Room);

            //Need to account for all current blocking features in room
            AddExistingBlockingFeaturesToRoomFiller(level, positionedRoom, roomFiller);

            foreach (Point roomPoint in pointsToPlaceTerrain)
            {
                try
                {
                    var featureToPlace = ChooseItemFromWeights<DecorationFeatureDetails.Decoration>(decorationDetails);
                    var featureLocationInMapCoords = positionedRoom.Location + roomPoint;

                    if (isSoft)
                    {
                        featureToPlace.isBlocking = false;
                    }

                    if (!featureToPlace.isBlocking)
                    {
                        bool result = Game.Dungeon.AddFeature(new RogueBasin.Features.StandardDecorativeFeature(featureToPlace.representation, featureToPlace.colour, false), level, featureLocationInMapCoords);

                        if (result)
                            LogFile.Log.LogEntryDebug("Placing feature in room " + positionedRoom.RoomIndex + " at location " + featureLocationInMapCoords, LogDebugLevel.Medium);
                    }
                    else if (roomFiller.SetSquareUnWalkableIfMaintainsConnectivity(roomPoint))
                    {
                        bool result = Game.Dungeon.AddFeatureBlocking(new RogueBasin.Features.StandardDecorativeFeature(featureToPlace.representation, featureToPlace.colour, true), level, featureLocationInMapCoords, blocksLight);

                        if (result)
                        {
                            LogFile.Log.LogEntryDebug("Placing blocking feature in room " + positionedRoom.RoomIndex + " at location " + featureLocationInMapCoords + " room wise " + roomPoint, LogDebugLevel.Medium);
                        }
                    }
                }
                catch (ApplicationException ex)
                {
                    LogFile.Log.LogEntryDebug("Unable to add feature in room " + ex.Message, LogDebugLevel.Medium);
                }
            }
        }

        private void AddDecorationFeatures(MapInfo mapInfo, LevelInfo levelInfo)
        {
            var roomsInThisLevel = mapInfo.GetRoomIndicesForLevel(levelInfo.LevelNo);
            roomsInThisLevel = mapInfo.FilterOutCorridors(roomsInThisLevel);

            double avConcentration = 0.1;
            double stdConcentration = 0.02;

            foreach (var room in roomsInThisLevel)
            {
                var thisRoom = mapInfo.Room(room);
                var thisRoomArea = thisRoom.Room.Width * thisRoom.Room.Height;

                var numberOfFeatures = (int)Math.Abs(Gaussian.BoxMuller(thisRoomArea * avConcentration, thisRoomArea * stdConcentration));
                var decorationWeights = new List<Tuple<int, DecorationFeatureDetails.Decoration>> {
                    new Tuple<int, DecorationFeatureDetails.Decoration>(100, new DecorationFeatureDetails.Decoration((char)557, System.Drawing.Color.Yellow, true))
                };

                //only big rooms
                if(thisRoomArea < 40)
                  continue;

                //var regularGridOfCentres = RoomTemplateUtilities.GetPointsInRoomWithTerrain(thisRoom.Room, RoomTemplateTerrain.Floor)
                var regularGridOfCentres = DivideRoomIntoCentres(thisRoom.Room, 3, 3, 0.3, new Point(2,2));
                
                foreach (Point centre in regularGridOfCentres)
                {
                    int random = Game.Random.Next(3);
                    if (random < 2)
                    {
                        //Cross piece

                        var crossPiece = new CrossPiece(centre, 4 + Game.Random.Next(3), 4 + Game.Random.Next(3), Math.PI / Game.Random.NextDouble());
                        var crossPoints = crossPiece.Generate();
                        var isLightBlockingRoll = Game.Random.Next(20);
                        bool isLightBlocking = isLightBlockingRoll > 10 ? true : false;
                        var softCoverRoll = Game.Random.Next(20);
                        bool isSoftCover = softCoverRoll > 15 ? true : false;
                        LogFile.Log.LogEntryDebug("Picking soft cover: " + isSoftCover + " from roll " + softCoverRoll, LogDebugLevel.Medium);
                        AddStandardDecorativeFeaturesToRoom(levelInfo.LevelNo, thisRoom, crossPoints, decorationWeights, isLightBlocking, isSoftCover);
                    }
                    else
                    {
                        //Acid Pond
                        AddAcidPondToLevel(levelInfo.LevelNo, thisRoom.Location + centre, 6, 6);
                    }
                }


            }
        }

        public void AddAcidPondToLevel(int level, Point origin, int width, int height)
        {
            var pond = new Pond(origin, width, height);
            var pondPoints = pond.Generate();

            AddNonBlockingFeaturesToLevel(level, pondPoints, () => new Features.Acid());
        }


        /// <summary>
        /// Return a list of the centres of grid squares for room subdivision
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<Point> DivideRoomIntoCentres(RoomTemplate room, int xSubdivisons, int ySubdivisions, double gaussianJitter, Point interiorOffset)
        {
            List<Point> pointsToReturn = new List<Point>();

            double xStart = interiorOffset.x;
            double yStart = interiorOffset.y;

            double xStep = (room.Width - interiorOffset.x * 2)/ (double)xSubdivisons;
            double yStep = (room.Height - interiorOffset.y * 2) / (double)ySubdivisions;

            List<Point> xCentres = new List<Point>();

            for (int i = 0; i < xSubdivisons; i++)
            {
                for (int j = 0; j < ySubdivisions; j++)
                {
                    double xCentre = xStart + Math.Round((i + 0.5) * xStep);
                    double yCentre = yStart + Math.Round((j + 0.5) * yStep);
                    
                    double xJitter = Gaussian.BoxMuller(0, gaussianJitter * xStep);
                    double yJitter = Gaussian.BoxMuller(0, gaussianJitter * yStep);

                    pointsToReturn.Add(new Point((int)Math.Round(xCentre + xJitter), (int)Math.Round(yCentre + yJitter)));
                }
            }

            /*
            LogFile.Log.LogEntryDebug("Room centres: " + room.Width + "/" + room.Height, LogDebugLevel.Medium);
            foreach (Point p in pointsToReturn)
            {
                LogFile.Log.LogEntryDebug("Centre: " + p, LogDebugLevel.Medium);
            }*/
            return pointsToReturn;
        }

    }


}
