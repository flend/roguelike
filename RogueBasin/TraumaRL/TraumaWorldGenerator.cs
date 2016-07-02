using GraphMap;
using libtcodWrapper;
using RogueBasin;
using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TraumaRL
{
    public partial class TraumaWorldGenerator
    {
        /// <summary>
        /// Mapping from template terrain to real terrain on the map
        /// </summary>
        Dictionary<RoomTemplateTerrain, MapTerrain> terrainMapping;

        Dictionary<MapTerrain, List<MapTerrain>> brickTerrainMapping;

        ConnectivityMap levelLinks;

        //For development, skip making most of the levels
        bool quickLevelGen = false;

        LogGenerator logGen = new LogGenerator();

        MapState mapState;

        public ConnectivityMap LevelLinks { get { return levelLinks; } }

        public const int medicalLevel = 0;
        public const int lowerAtriumLevel = 1;
        public const int scienceLevel = 2;
        public const int storageLevel = 3;
        public const int flightDeck = 4;
        public const int reactorLevel = 5;
        public const int arcologyLevel = 6;
        public const int commercialLevel = 7;
        public const int computerCoreLevel = 8;
        public const int bridgeLevel = 9;

        public TraumaWorldGenerator()
        {
            BuildTerrainMapping();
            
        }

        private void BuildTerrainMapping()
        {
            terrainMapping = new Dictionary<RoomTemplateTerrain, MapTerrain>();
            terrainMapping[RoomTemplateTerrain.Wall] = MapTerrain.Wall;
            terrainMapping[RoomTemplateTerrain.Floor] = MapTerrain.Empty;
            terrainMapping[RoomTemplateTerrain.Transparent] = MapTerrain.Void;
            terrainMapping[RoomTemplateTerrain.WallWithPossibleDoor] = MapTerrain.ClosedDoor;

            brickTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {

                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.BrickWall1, MapTerrain.BrickWall1, MapTerrain.BrickWall1, MapTerrain.BrickWall2, MapTerrain.BrickWall3, MapTerrain.BrickWall4, MapTerrain.BrickWall5 } }};

        }

        static TraumaWorldGenerator() { 
            
        }

        /// <summary>
        /// Build a level->level map showing how the levels are connected
        /// </summary>
        private ConnectivityMap GenerateLevelLinks()
        {
            var levelLinks = new ConnectivityMap();

            //Player starts in medical which links to the lower atrium
            levelLinks.AddRoomConnection(new Connection(medicalLevel, lowerAtriumLevel));

            if (!quickLevelGen)
            {
                var standardLowerLevels = new List<int> { scienceLevel, storageLevel, flightDeck, arcologyLevel, commercialLevel };

                //3 of these branch from the lower atrium
                var directLinksFromLowerAtrium = standardLowerLevels.RandomElements(3);

                foreach (var level in directLinksFromLowerAtrium)
                    levelLinks.AddRoomConnection(lowerAtriumLevel, level);

                //The remainder branch from other levels (except the arcology)
                var leafLevels = directLinksFromLowerAtrium.Select(x => x);
                leafLevels = leafLevels.Except(new List<int> { arcologyLevel });

                var allLowerLevelsToPlace = standardLowerLevels.Except(directLinksFromLowerAtrium).Union(new List<int> { reactorLevel });
                foreach (var level in allLowerLevelsToPlace)
                {
                    levelLinks.AddRoomConnection(leafLevels.RandomElement(), level);
                }

                //Bridge and computer core are also leaves
                var allLaterLevels = standardLowerLevels.Except(directLinksFromLowerAtrium);
                var finalLevelsToPlace = new List<int> { computerCoreLevel, bridgeLevel };
                foreach (var level in finalLevelsToPlace)
                {
                    levelLinks.AddRoomConnection(allLaterLevels.RandomElement(), level);
                }
            }

            //Calculate some data about the levels
            var gameLevels = levelLinks.GetAllConnections().SelectMany(c => new List<int> { c.Source, c.Target }).Distinct().OrderBy(c => c).ToList();

            return levelLinks;
        }

        public MapState MapState { get { return mapState; } }

        
        /** Build a map using templated rooms */
        public void GenerateTraumaLevels(bool retry)
        {
            //We catch exceptions on generation and keep looping

            //Generate the overall level structure
            levelLinks = GenerateLevelLinks();
            var gameLevels = levelLinks.GetAllConnections().SelectMany(c => new List<int> { c.Source, c.Target }).Distinct().OrderBy(c => c).ToList();

            //Build each level individually
            TraumaLevelBuilder levelBuilder = new TraumaLevelBuilder(gameLevels, levelLinks, quickLevelGen);
            levelBuilder.GenerateLevels();
            
            Dictionary<int, LevelInfo> levelInfo = levelBuilder.LevelInfo;

            //Create the state object which will hold the map state in the generation phase

            var startVertex = 0;
            var startLevel = 0;
            mapState = new MapState();
            mapState.UpdateWithNewLevelMaps(levelLinks, levelInfo, startLevel);
            mapState.InitialiseDoorAndClueManager(startVertex);
                        
            //Feels like there will be a more dynamic way of getting this state in future
            mapState.ConnectionStore["escapePodConnection"] = levelBuilder.EscapePodsConnection;
            mapState.AllReplaceableVaults = levelBuilder.AllReplaceableVaults;

            MapInfo mapInfo = mapState.MapInfo;
            
            //Add maps to the dungeon (must be ordered)
            AddLevelMapsToDungeon(levelInfo);

            //Add elevator features to link the maps
            if (!quickLevelGen)
                AddElevatorFeatures(mapInfo, levelInfo);

            //Generate quests
            var mapQuestBuilder = new QuestMapBuilder();
            GenerateQuests(mapState, mapQuestBuilder);

            //Place loot
            CalculateLevelDifficulty(mapState);

            if (!quickLevelGen)
                PlaceLootInArmory(mapState, mapQuestBuilder);

            if (!quickLevelGen)
                AddGoodyQuestLogClues(mapState, mapQuestBuilder);

            //Add non-interactable features
            var decorator = new DungeonDecorator(mapState, mapQuestBuilder);
            decorator.AddDecorationFeatures();

            //Add debug stuff in the first room
            AddDebugItems(mapInfo);

            //Set player's start location (must be done before adding items)
            SetPlayerStartLocation(mapInfo);

            //Set maps in engine (needs to be done before placing items and monsters)
            SetupMapsInEngine();

            //Quests is being refactored to store information in MapInfo, rather than in the Dungeon
            //Need to add here the code which transfers the completed MapInfo creatures, features, items and locks into the Dungeon
            AddMapObjectsToDungeon(mapInfo);
            
            //Add monsters
            Game.Dungeon.MonsterPlacement.CreateMonstersForLevels(mapState, mapState.GameLevels, mapState.LevelDifficulty);

            //Check we are solvable
            AssertMapIsSolveable(mapInfo, mapState.DoorAndClueManager);

            if (retry)
            {
                throw new ApplicationException("It happened!");
            }
        }

        /// <summary>
        /// RoomPlacements currently contain absolute co-ordinates. I would prefer them to have relative coordinates, and those to get
        /// mapped to absolute coordinates here
        /// </summary>
        /// <param name="mapInfo"></param>
        private void AddMapObjectsToDungeon(MapInfo mapInfo)
        {
            var rooms = mapInfo.Populator.AllRoomsInfo();

            foreach (RoomInfo roomInfo in rooms)
            {
                foreach (MonsterRoomPlacement monsterPlacement in roomInfo.Monsters)
                {
                    bool monsterResult = Game.Dungeon.AddMonster(monsterPlacement.monster, monsterPlacement.location);

                    if (!monsterResult) {
                        LogFile.Log.LogEntryDebug("Cannot add monster to dungeon: " + monsterPlacement.monster.SingleDescription + " at: " + monsterPlacement.location, LogDebugLevel.Medium);
                    }
                }

                foreach (ItemRoomPlacement itemPlacement in roomInfo.Items)
                {
                    bool monsterResult = Game.Dungeon.AddItem(itemPlacement.item, itemPlacement.location);

                    if (!monsterResult)
                    {
                        LogFile.Log.LogEntryDebug("Cannot add item to dungeon: " + itemPlacement.item.SingleItemDescription + " at: " + itemPlacement.location, LogDebugLevel.Medium);
                    }
                }

                foreach (FeatureRoomPlacement featurePlacement in roomInfo.Features)
                {
                    if (featurePlacement.feature.IsBlocking)
                    {
                        bool featureResult = Game.Dungeon.AddFeatureBlocking(featurePlacement.feature, featurePlacement.location.Level, featurePlacement.location.MapCoord, featurePlacement.feature.BlocksLight);

                        if (!featureResult)
                        {
                            LogFile.Log.LogEntryDebug("Cannot add blocking feature to dungeon: " + featurePlacement.feature.Description + " at: " + featurePlacement.location, LogDebugLevel.Medium);
                        }
                    }
                    else
                    {
                        bool featureResult = Game.Dungeon.AddFeature(featurePlacement.feature, featurePlacement.location.Level, featurePlacement.location.MapCoord);

                        if (!featureResult)
                        {
                            LogFile.Log.LogEntryDebug("Cannot add feature to dungeon: " + featurePlacement.feature.Description + " at: " + featurePlacement.location, LogDebugLevel.Medium);
                        }
                    }
                }
            }

            foreach (var doorInfo in mapInfo.Populator.DoorInfo)
            {
                var door = doorInfo.Value;

                foreach(var doorLock in door.Locks) {
                    Game.Dungeon.AddLock(doorLock);
                }
            }
        }

        private void AssertMapIsSolveable(MapInfo mapInfo, DoorAndClueManager doorAndClueManager)
        {
            var graphSolver = new GraphSolver(mapInfo.Model, doorAndClueManager);
            if (!graphSolver.MapCanBeSolved())
            {
                LogFile.Log.LogEntryDebug("MAP CAN'T BE SOLVED!", LogDebugLevel.High);
                throw new ApplicationException("It's all over - map can't be solved.");
            }
            else
            {
                LogFile.Log.LogEntryDebug("Phew - map can be solved", LogDebugLevel.High);
            }

            if (!quickLevelGen && !RoutabilityUtilities.CheckItemRouteability())
            {
                throw new ApplicationException("Item is not connected to elevator, aborting.");
            }

            if (!quickLevelGen && !RoutabilityUtilities.CheckFeatureRouteability())
            {
                //throw new ApplicationException("Feature is not connected to elevator, aborting.");
            }
        }

        private static void SetPlayerStartLocation(MapInfo mapInfo)
        {
            var firstRoom = mapInfo.Room(0);
            Game.Dungeon.Levels[0].PCStartLocation = new RogueBasin.Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);
        }


        private void AddLevelMapsToDungeon(Dictionary<int, LevelInfo> levelInfo)
        {
            foreach (var kv in levelInfo.OrderBy(kv => kv.Key))
            {
                var thisLevelInfo = kv.Value;

                Map masterMap = thisLevelInfo.LevelBuilder.MergeTemplatesIntoMap(terrainMapping);

                Dictionary<MapTerrain, List<MapTerrain>> terrainSubstitution = brickTerrainMapping;
                if (thisLevelInfo.TerrainMapping != null)
                    terrainSubstitution = thisLevelInfo.TerrainMapping;

                Map randomizedMapL1 = MapTerrainRandomizer.RandomizeTerrainInMap(masterMap, terrainSubstitution);
                Game.Dungeon.AddMap(randomizedMapL1);
            }
        }


        private void AddDebugItems(MapInfo mapInfo)
        {
            
        }

        private void SetupMapsInEngine()
        {
            //Comment for faster UI check
            Game.Dungeon.RefreshAllLevelPathingAndFOV();

            foreach (var level in mapState.GameLevels)
            {
                Game.Dungeon.Levels[level].LightLevel = 0;
            }
        }


        private void GenerateQuests(MapState mapState, QuestMapBuilder questMapBuilder)
        {
            var mapInfo = mapState.MapInfo;
            var levelInfo = mapState.LevelInfo;

            var mapHeuristics = new MapHeuristics(mapInfo.Model.GraphNoCycles, mapInfo.StartRoom);
            var roomConnectivityMap = mapHeuristics.GetTerminalBranchConnections();

            if (!quickLevelGen)
            {
                BuildMainQuest(mapState, questMapBuilder);
            }
            BuildMedicalLevelQuests(mapState, questMapBuilder);

            if (!quickLevelGen)
            {
                BuildAtriumLevelQuests(mapState, questMapBuilder, roomConnectivityMap);

                BuildRandomElevatorQuests(mapState, questMapBuilder, roomConnectivityMap);

                BuildGoodyQuests(mapState, questMapBuilder, roomConnectivityMap);
            }
        }

        private void BuildRandomElevatorQuests(MapState mapState, QuestMapBuilder builder, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            var noLevelsToBlock = 1 + Game.Random.Next(1);

            var candidateLevels = mapState.GameLevels.Except(new List<int> { lowerAtriumLevel, medicalLevel }).Where(l => mapState.LevelInfo[l].ConnectionsToOtherLevels.Count() > 1);
            LogFile.Log.LogEntryDebug("Candidates for elevator quests: " + candidateLevels, LogDebugLevel.Medium);
            var chosenLevels = candidateLevels.RandomElements(noLevelsToBlock);

            foreach (var level in chosenLevels)
            {
                try
                {
                    BlockElevatorPaths(mapState, builder, roomConnectivityMap, level, 1, Game.Random.Next(2) > 0);
                }
                catch (Exception ex)
                {
                    LogFile.Log.LogEntryDebug("Random Elevator Exception (level " + level + "): " + ex, LogDebugLevel.High);
                }
            }
        }

        private void BuildAtriumLevelQuests(MapState mapState, QuestMapBuilder builder, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            try
            {
                BlockElevatorPaths(mapState, builder, roomConnectivityMap, lowerAtriumLevel, 1, false);
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntryDebug("Atrium Elevator Exception: " + ex, LogDebugLevel.High);
            }
        }

        private bool BlockElevatorPaths(MapState mapState, QuestMapBuilder builder, Dictionary<int, List<Connection>> roomConnectivityMap,
            int levelForBlocks, int maxDoorsToMake, bool clueOnElevatorLevel)
        {
            var levelInfo = mapState.LevelInfo;
            var connectionsFromThisLevel = levelInfo[levelForBlocks].ConnectionsToOtherLevels;

            var pairs = Utility.GetPermutations<int>(connectionsFromThisLevel.Keys, 2);

            if (pairs.Count() == 0)
            {
                LogFile.Log.LogEntryDebug("Can't find pair of elevators to connection", LogDebugLevel.High);
                return false;
            }

            var pairsLeft = pairs.Select(s => s);

            int doorsMade = 0;
            while (doorsMade < maxDoorsToMake && pairsLeft.Count() > 0)
            {
                var pairToTry = pairsLeft.RandomElement();

                var sourceElevatorConnection = levelInfo[levelForBlocks].ConnectionsToOtherLevels[pairToTry.ElementAt(0)];
                var targetElevatorConnection = levelInfo[levelForBlocks].ConnectionsToOtherLevels[pairToTry.ElementAt(1)];

                var startDoor = sourceElevatorConnection.Source;
                var endDoor = targetElevatorConnection.Source;

                var colorToUse = builder.GetUnusedColor();

                var doorName = colorToUse.Item2 + " key card";
                var doorId = mapState.LevelNames[levelForBlocks] + "-" + doorName + Game.Random.Next();
                var doorColor = colorToUse.Item1;

                LogFile.Log.LogEntryDebug("Blocking elevators " + pairToTry.ElementAt(0) + " to " + pairToTry.ElementAt(1) + " with " + doorId, LogDebugLevel.High);

                BlockPathBetweenRoomsWithSimpleDoor(mapState, builder, roomConnectivityMap,
                    doorId, doorName, doorColor, 1, startDoor, endDoor,
                    0.5, clueOnElevatorLevel, QuestMapBuilder.CluePath.NotOnCriticalPath, true,
                    true, QuestMapBuilder.CluePath.OnCriticalPath, true);

                doorsMade++;
                pairsLeft = pairsLeft.Except(Enumerable.Repeat(pairToTry, 1));
            }

            return true;
        }

        Dictionary<int, int> goodyRooms;
        Dictionary<int, string> goodyRoomKeyNames;

        private void BuildGoodyQuests(MapState mapState, QuestMapBuilder builder, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            //Ensure that we have a goody room on every level that will support it
            var levelInfo = mapState.LevelInfo;
            var replaceableVaultsForLevels = levelInfo.ToDictionary(kv => kv.Key, kv => kv.Value.ReplaceableVaultConnections.Except(kv.Value.ReplaceableVaultConnectionsUsed));
            goodyRooms = new Dictionary<int,int>();
            goodyRoomKeyNames = new Dictionary<int, string>();

            var manager = mapState.DoorAndClueManager;

            foreach (var kv in replaceableVaultsForLevels)
            {
                if (kv.Value.Count() == 0)
                {
                    LogFile.Log.LogEntryDebug("No vaults left for armory on level " + kv.Key, LogDebugLevel.High);
                    continue;
                }

                var thisLevel = kv.Key;
                var thisConnection = kv.Value.RandomElement();
                var thisRoom = thisConnection.Target;

                LogFile.Log.LogEntryDebug("Placing goody room at: level: " + thisLevel + " room: " + thisRoom, LogDebugLevel.Medium);

                //Place door
                var doorReadableId = mapState.LevelNames[thisLevel] + " armory";
                var doorId = doorReadableId;
                
                var unusedColor = builder.GetUnusedColor();
                var clueName = unusedColor.Item2 + " key card";

                builder.PlaceLockedDoorOnMap(mapState, doorId, clueName, 1, unusedColor.Item1, thisConnection);

                goodyRooms[thisLevel] = thisRoom;

                //Clue
                var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(doorId);

                //Assume a critical path from the lower level elevator
                var lowerLevelFloor = levelInfo[thisLevel].ConnectionsToOtherLevels.Min(level => level.Key);
                var elevatorFromLowerLevel = levelInfo[thisLevel].ConnectionsToOtherLevels[lowerLevelFloor].Target;
                var criticalPath = mapState.MapInfo.Model.GetPathBetweenVerticesInReducedMap(elevatorFromLowerLevel, thisRoom);

                var filteredRooms = builder.FilterClueRooms(mapState, allowedRoomsForClues, criticalPath, true, QuestMapBuilder.CluePath.NotOnCriticalPath, true);
                var roomsToPlaceMonsters = new List<int>();

                var roomsForMonsters = builder.GetRandomRoomsForClues(mapState, 1, filteredRooms);
                var clues = manager.AddCluesToExistingDoor(doorId, roomsForMonsters);

                
                goodyRoomKeyNames[thisLevel] = clueName;
                var cluesAndColors = clues.Select(c => new Tuple<Clue, System.Drawing.Color, string>(c, unusedColor.Item1, clueName));
                builder.PlaceSimpleClueItems(mapState, cluesAndColors, true, false);

                //Vault is used
                levelInfo[thisLevel].ReplaceableVaultConnectionsUsed.Add(thisConnection);
            }
        
        }

        private void AddGoodyQuestLogClues(MapState mapState, QuestMapBuilder builder)
        {
            //Ensure that we have a goody room on every level that will support it
            var manager = mapState.DoorAndClueManager;
            var mapInfo = mapState.MapInfo;
            var levelInfo = mapState.LevelInfo;

            foreach (var kv in goodyRooms)
            {
                var thisLevel = kv.Key;
                var thisRoom = kv.Value;
                
                var doorId = mapState.LevelNames[thisLevel] + " armory";

                //Clue
                var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(doorId);

                //Assume a critical path from the lower level elevator
                var lowerLevelFloor = levelInfo[thisLevel].ConnectionsToOtherLevels.Min(level => level.Key);
                var elevatorFromLowerLevel = levelInfo[thisLevel].ConnectionsToOtherLevels[lowerLevelFloor].Target;
                var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(elevatorFromLowerLevel, thisRoom);

                //Logs - try placing them on the critical path from the start of the game!

                var criticalPathFromStart = mapInfo.Model.GetPathBetweenVerticesInReducedMap(0, thisRoom);
                var preferredRoomsForLogsNonCritical = builder.FilterClueRooms(mapState, allowedRoomsForClues, criticalPath, false, QuestMapBuilder.CluePath.OnCriticalPath, true);

                var roomsForLogsNonCritical = builder.GetRandomRoomsForClues(mapState, 1, preferredRoomsForLogsNonCritical);

                var logClues = manager.AddCluesToExistingDoor(doorId, roomsForLogsNonCritical);
                var clueName = goodyRoomKeyNames[thisLevel];
                var log1 = new Tuple<LogEntry, Clue>(logGen.GenerateGoodyRoomLogEntry(clueName, thisLevel, itemsInArmory[thisLevel]), logClues[0]);
                builder.PlaceLogClues(mapState, new List<Tuple<LogEntry, Clue>> { log1 }, true, true);
            }

        }
        
        private void BuildMedicalLevelQuests(MapState mapState, QuestMapBuilder builder)
        {
            var mapInfo = mapState.MapInfo;

            //Lock the door to the elevator and require a certain number of monsters to be killed
            var elevatorConnection = mapState.LevelInfo[medicalLevel].ConnectionsToOtherLevels.First().Value;

            var doorId = "medical-security";
            int objectsToPlace = 15;
            int objectsToDestroy = 10;

            //Place door
            builder.PlaceMovieDoorOnMap(mapState, doorId, doorId, objectsToDestroy, System.Drawing.Color.Red, "t_medicalsecurityunlocked", "t_medicalsecuritylocked", elevatorConnection);

            //This will be restricted to the medical level since we cut off the door
            var manager = mapState.DoorAndClueManager;

            var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            allowedRoomsForClues = mapInfo.FilterOutCorridors(allowedRoomsForClues);

            var roomsForMonsters = builder.GetRandomRoomsForClues(mapState, objectsToPlace, allowedRoomsForClues);
            var clues = manager.AddCluesToExistingDoor(doorId, roomsForMonsters);

            builder.PlaceCreatureClues<RogueBasin.Creatures.Camera>(mapState, clues, true, false);

            //Place log entries explaining the puzzle
            //These will not be turned into in-engine clue items, so they can't be used to open the door
            //They are added though, to ensure that they are readable before the door is opened

            var roomsForLogs = builder.GetRandomRoomsForClues(mapState, 2, allowedRoomsForClues);
            var logClues = manager.AddCluesToExistingDoor(doorId, roomsForLogs);

            var log1 = new Tuple<LogEntry, Clue>(logGen.GenerateElevatorLogEntry(medicalLevel, lowerAtriumLevel), logClues[0]);
            var log2 = new Tuple<LogEntry, Clue>(logGen.GenerateArbitaryLogEntry("qe_medicalsecurity"), logClues[1]);
            builder.PlaceLogClues(mapState, new List<Tuple<LogEntry, Clue>> { log1, log2 }, true, true);
        }

        private void BlockPathBetweenRoomsWithSimpleDoor(MapState mapState, QuestMapBuilder builder, Dictionary<int, List<Connection>> roomConnectivityMap,
            string doorId, string doorName, System.Drawing.Color colorToUse, int cluesForDoor, int sourceRoom, int endRoom,
            double distanceFromSourceRatio, bool enforceClueOnDestLevel, QuestMapBuilder.CluePath clueNotOnCriticalPath, bool clueNotInCorridors,
            bool hasLogClue, QuestMapBuilder.CluePath logOnCriticalPath, bool logNotInCorridors)
        {
            var manager = mapState.DoorAndClueManager;
            var mapInfo = mapState.MapInfo;

            var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(sourceRoom, endRoom);
            var criticalConnectionForDoor = criticalPath.ElementAt((int)Math.Min(criticalPath.Count() * distanceFromSourceRatio, criticalPath.Count() - 1));

            criticalConnectionForDoor = MapAnalysisUtilities.FindFreeConnectionOnPath(manager, criticalPath, criticalConnectionForDoor);

            //Place door

            builder.PlaceLockedDoorOnMap(mapState, doorId, doorName, cluesForDoor, colorToUse, criticalConnectionForDoor);

            //Place clues

            var allRoomsForClue = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            var preferredRooms = builder.FilterClueRooms(mapState, allRoomsForClue, criticalPath, enforceClueOnDestLevel, clueNotOnCriticalPath, clueNotInCorridors);

            var roomsForClues = builder.GetRandomRoomsForClues(mapState, cluesForDoor, preferredRooms);
            var clues = manager.AddCluesToExistingDoor(doorId, roomsForClues);

            var cluesAndColors = clues.Select(c => new Tuple<Clue, System.Drawing.Color, string>(c, colorToUse, doorName));

            var clueLocations = builder.PlaceSimpleClueItems(mapState, cluesAndColors, clueNotInCorridors, false);

            //Place log entries explaining the puzzle

            if (hasLogClue)
            {
                //Put major clue on the critical path

                var preferredRoomsForLogs = builder.FilterClueRooms(mapState, allRoomsForClue, criticalPath, false, logOnCriticalPath, logNotInCorridors);
                var roomsForLogs = builder.GetRandomRoomsForClues(mapState, 1, preferredRoomsForLogs);
                var logClues = manager.AddCluesToExistingDoor(doorId, roomsForLogs);

                //Put minor clue somewhere else
                var preferredRoomsForLogsNonCritical = builder.FilterClueRooms(mapState, allRoomsForClue, criticalPath, false, QuestMapBuilder.CluePath.Any, logNotInCorridors);

                var roomsForLogsNonCritical = builder.GetRandomRoomsForClues(mapState, 1, preferredRoomsForLogsNonCritical);
                var logCluesNonCritical = manager.AddCluesToExistingDoor(doorId, roomsForLogsNonCritical);

                var coupledLogs = logGen.GenerateCoupledDoorLogEntry(doorName, mapInfo.GetLevelForRoomIndex(criticalConnectionForDoor.Source),
                    clueLocations.First().level);
                var log1 = new Tuple<LogEntry, Clue>(coupledLogs[0], logClues[0]);
                var log2 = new Tuple<LogEntry, Clue>(coupledLogs[1], logCluesNonCritical[0]);
                builder.PlaceLogClues(mapState, new List<Tuple<LogEntry, Clue>> { log1, log2 }, true, true);
            }
        }

        private void BuildMainQuest(MapState mapState, QuestMapBuilder questMapBuilder)
        {
            //Escape pod end game
            var escapePod = new MainQuest(mapState, questMapBuilder, logGen);
            escapePod.SetupQuest();
        }

        private void AddElevatorFeatures(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            var elevatorLocations = new Dictionary<Tuple<int, int>, Tuple<int, RogueBasin.Point>>();

            foreach (var kv in levelInfo)
            {
                var thisLevelNo = kv.Key;
                var thisLevelInfo = kv.Value;

                foreach (var connectionToOtherLevel in thisLevelInfo.ConnectionsToOtherLevels)
                {
                    var elevatorLoc = mapInfo.GetUnoccupiedPointsInRoom(connectionToOtherLevel.Value.Target).Shuffle().First();
                    elevatorLocations[new Tuple<int, int>(thisLevelNo, connectionToOtherLevel.Key)] =
                        new Tuple<int, RogueBasin.Point>(connectionToOtherLevel.Value.Target, elevatorLoc);
                }
            }

            foreach (var kv in elevatorLocations)
            {
                var sourceLevel = kv.Key.Item1;
                var targetLevel = kv.Key.Item2;

                var sourceToTargetElevator = kv.Value;
                var targetToSourceElevator = elevatorLocations[new Tuple<int, int>(targetLevel, sourceLevel)];

                var sourceToTargetElevatorRoomId = kv.Value.Item1;
                var sourceToTargetElevatorPoint = kv.Value.Item2;

                var elevatorFeature = new RogueBasin.Features.Elevator(targetLevel, targetToSourceElevator.Item2);

                mapInfo.Populator.AddFeatureToRoom(mapInfo, sourceToTargetElevatorRoomId, sourceToTargetElevatorPoint, elevatorFeature);
                
                LogFile.Log.LogEntryDebug("Adding elevator connection " + sourceLevel + ":" + targetLevel + " via points" +
                    sourceToTargetElevator + "->" + targetToSourceElevator, LogDebugLevel.Medium);
            }
        }
        
        


    }
}
