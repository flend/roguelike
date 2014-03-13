﻿using GraphMap;
using libtcodWrapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{

    public class TraumaWorldGenerator
    {
        /// <summary>
        /// Mapping from template terrain to real terrain on the map
        /// </summary>
        Dictionary<RoomTemplateTerrain, MapTerrain> terrainMapping;

        List<RoomTemplate> roomTemplates = new List<RoomTemplate>();
        List<RoomTemplate> corridorTemplates = new List<RoomTemplate>();

        ConnectivityMap connectivityMap = null;

        ConnectivityMap levelLinks;
        List<int> gameLevels;
        static Dictionary<int, string> levelNaming;

        HashSet<Clue> placedClues = new HashSet<Clue>();
        HashSet<Objective> placedObjectives = new HashSet<Objective>();
        HashSet<Door> placedDoors = new HashSet<Door>();

        LogGenerator logGen = new LogGenerator();

        public TraumaWorldGenerator()
        {
            BuildTerrainMapping();
        }

        static TraumaWorldGenerator() { 
            
            BuildLevelNaming();
        }

        public const int medicalLevel = 0;
        public const int lowerAtriumLevel = 1;
        public const int scienceLevel = 2;
        public const int storageLevel = 3;
        public const int flightDeck = 4;
        public const int reactorLevel = 5;
        public const int arcologyLevel = 6;
        public const int commercialLevel = 7;

        //Quest important rooms / vaults
        Connection escapePodsConnection;
        int escapePodsLevel;

        //Wall mappings
        Dictionary<MapTerrain, List<MapTerrain>> brickTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> panelTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> securityTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> irisTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> bioTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> lineTerrainMapping;

        private static void BuildLevelNaming()
        {
            levelNaming = new Dictionary<int, string>();
            levelNaming[medicalLevel] = "Medical";
            levelNaming[lowerAtriumLevel] = "Lower Atrium";
            levelNaming[scienceLevel] = "Science";
            levelNaming[storageLevel] = "Storage";
            levelNaming[flightDeck] = "Flight deck";
            levelNaming[reactorLevel] = "Reactor";
            levelNaming[arcologyLevel] = "Arcology";
            levelNaming[commercialLevel] = "Commercial";
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

            panelTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.PanelWall1, MapTerrain.PanelWall1, MapTerrain.PanelWall1, MapTerrain.PanelWall2, MapTerrain.PanelWall3, MapTerrain.PanelWall4, MapTerrain.PanelWall5 } }};

            bioTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.BioWall1, MapTerrain.BioWall1, MapTerrain.BioWall1, MapTerrain.BioWall2, MapTerrain.BioWall3, MapTerrain.BioWall4, MapTerrain.BioWall5 } }};

            securityTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.SecurityWall1, MapTerrain.SecurityWall1, MapTerrain.SecurityWall1, MapTerrain.SecurityWall2, MapTerrain.SecurityWall3, MapTerrain.SecurityWall4, MapTerrain.SecurityWall5 } }};

            lineTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.LineWall1, MapTerrain.LineWall1, MapTerrain.LineWall1, MapTerrain.LineWall2, MapTerrain.LineWall3, MapTerrain.LineWall4, MapTerrain.LineWall5 } }};

            irisTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.IrisWall1, MapTerrain.IrisWall1, MapTerrain.IrisWall1, MapTerrain.IrisWall2, MapTerrain.IrisWall3, MapTerrain.IrisWall4, MapTerrain.IrisWall5 } }};

        }

        private RoomTemplate RandomRoom()
        {
            return roomTemplates[Game.Random.Next(roomTemplates.Count)];
        }

        private RoomTemplate RandomCorridor()
        {
            return corridorTemplates[Game.Random.Next(corridorTemplates.Count)];
        }

        private DoorInfo RandomDoor(TemplatedMapGenerator generator)
        {
            return generator.PotentialDoors[Game.Random.Next(generator.PotentialDoors.Count())];
        }

        public ConnectivityMap ConnectivityMap
        {
            get
            {
                return connectivityMap;
            }
        }

        public ConnectivityMap LevelLinks { get { return levelLinks; } }

        public static Dictionary<int, string> LevelNaming { get { return levelNaming; } }

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
            var templatedGenerator = new TemplatedMapGenerator(mapBuilder);

            int roomsToPlace = 20;
            int maxRoomDistance = 10;

            var roomsPlaced = PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, maxRoomDistance);

            //Add some extra connections, if doors are available
            var totalExtraConnections = 500;
            AddCorridorsBetweenOpenDoors(templatedGenerator, totalExtraConnections, new List<RoomTemplate>{corridor1});

            Map masterMap = mapBuilder.MergeTemplatesIntoMap(terrainMapping);

            var mapRooms = templatedGenerator.GetRoomTemplatesInWorldCoords();

            var firstRoom = mapRooms[0];
            masterMap.PCStartLocation = new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);

            LogFile.Log.LogEntryDebug("Player start location (map gen coords) " + new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2), LogDebugLevel.High);

            connectivityMap = templatedGenerator.ConnectivityMap;

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
            var templatedGenerator = new TemplatedMapGenerator(mapBuilder);

            int roomsToPlace = 100;
            int maxRoomDistance = 1;

            var roomsPlaced = PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, maxRoomDistance);

            //Add some extra connections, if doors are available
            var totalExtraConnections = 500;
            AddCorridorsBetweenOpenDoors(templatedGenerator, totalExtraConnections, new List<RoomTemplate> { corridor1 });

            //Replace spare doors with walls
            templatedGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            Map masterMap = mapBuilder.MergeTemplatesIntoMap(terrainMapping);

            var mapRooms = templatedGenerator.GetRoomTemplatesInWorldCoords();

            var firstRoom = mapRooms[0];
            masterMap.PCStartLocation = new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);

            LogFile.Log.LogEntryDebug("Player start location (map coords) " + new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2), LogDebugLevel.High);

            connectivityMap = templatedGenerator.ConnectivityMap;

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
            var templatedGenerator = new TemplatedMapGenerator(mapBuilder);

            //Place branch rooms to form the initial structure, joined on long axis
            PlaceOriginRoom(templatedGenerator, branchRoom);

            PlaceRandomConnectedRooms(templatedGenerator, 3, branchRoom, corridor1, 0, 0, () => Game.Random.Next(1) > 0 ? 3 : 4);

            //Add some 2-door rooms
            PlaceRandomConnectedRooms(templatedGenerator, 10, chamber2Doors, corridor1, 0, 0);

            //Add some 1-door deadends
            PlaceRandomConnectedRooms(templatedGenerator, 10, chamber1Doors, corridor1, 0, 0);

            //Add some extra connections, if doors are available
            AddCorridorsBetweenOpenDoors(templatedGenerator, 10, new List<RoomTemplate> { corridor1 });

            //Replace spare doors with walls
            templatedGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            Map masterMap = mapBuilder.MergeTemplatesIntoMap(terrainMapping);

            var mapRooms = templatedGenerator.GetRoomTemplatesInWorldCoords();

            var firstRoom = mapRooms[0];

            masterMap.PCStartLocation = new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);

            LogFile.Log.LogEntryDebug("Player start location (map coords) " + new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2), LogDebugLevel.High);

            connectivityMap = templatedGenerator.ConnectivityMap;

            return masterMap;
        }

        /// <summary>
        /// Build a level->level map showing how the levels are connected
        /// </summary>
        private void GenerateLevelLinks()
        {
            levelLinks = new ConnectivityMap();

            //Player starts in medical which links to the lower atrium
            levelLinks.AddRoomConnection(new Connection(medicalLevel, lowerAtriumLevel));
            
            var standardLowerLevels = new List<int> { scienceLevel, storageLevel, flightDeck, arcologyLevel, commercialLevel };

            //3 of these branch from the lower atrium
            var directLinksFromLowerAtrium = standardLowerLevels.RandomElements(3);

            foreach (var level in directLinksFromLowerAtrium)
                levelLinks.AddRoomConnection(lowerAtriumLevel, level);

            //The remainder branch from other levels (except the arcology)
            var leafLevels = directLinksFromLowerAtrium.Select(x => x);
            leafLevels.Except(new List<int> { arcologyLevel });

            var allLowerLevelsToPlace = standardLowerLevels.Except(directLinksFromLowerAtrium).Union(new List<int> { reactorLevel });
            foreach (var level in allLowerLevelsToPlace)
            {
                levelLinks.AddRoomConnection(leafLevels.RandomElement(), level);
            }

            gameLevels = levelLinks.GetAllConnections().SelectMany(c => new List<int> { c.Source, c.Target }).Distinct().OrderBy(c => c).ToList();
        }

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

        /** Build a map using templated rooms */
        public MapInfo GenerateTraumaLevels()
        {
            //We catch exceptions on generation and keep looping
            MapInfo mapInfo;
            
            do
            {
                //Reset shared state
                placedClues = new HashSet<Clue>();
                placedDoors = new HashSet<Door>();
                placedObjectives = new HashSet<Objective>();

                try
                {
                    //Generate the overall level structure
                    GenerateLevelLinks();

                    //Build each level individually

                    Dictionary<int, LevelInfo> levelInfo = new Dictionary<int, LevelInfo>();

                    var medicalInfo = GenerateMedicalLevel(medicalLevel);
                    levelInfo[medicalLevel] = medicalInfo;

                    var scienceInfo = GenerateScienceLevel(scienceLevel, scienceLevel * 100);
                    levelInfo[scienceLevel] = scienceInfo;

                    var storageInfo = GenerateStorageLevel(storageLevel, storageLevel * 100);
                    levelInfo[storageLevel] = storageInfo;

                    var reactorInfo = GenerateReactorLevel(reactorLevel, reactorLevel * 100);
                    levelInfo[reactorLevel] = reactorInfo;

                    var archologyInfo = GenerateArcologyLevel(arcologyLevel, arcologyLevel * 100);
                    levelInfo[arcologyLevel] = archologyInfo;

                    //Make other levels generically

                    var standardGameLevels = gameLevels.Except(new List<int> { medicalLevel, storageLevel, reactorLevel, arcologyLevel, scienceLevel });

                    foreach (var level in standardGameLevels)
                    {
                        var thisLevelInfo = GenerateStandardLevel(level, level * 100);
                        levelInfo[level] = thisLevelInfo;
                    }

                    //Build the room graph containing all levels

                    //Build and add the start level

                    var mapInfoBuilder = new MapInfoBuilder();
                    var startRoom = 0;
                    var startLevelInfo = levelInfo[medicalLevel];
                    mapInfoBuilder.AddConstructedLevel(medicalLevel, startLevelInfo.LevelGenerator.ConnectivityMap, startLevelInfo.LevelGenerator.GetRoomTemplatesInWorldCoords(),
                        startLevelInfo.LevelGenerator.GetDoorsInMapCoords(), startRoom);

                    //Build and add each connected level
                    var levelsAdded = new HashSet<int> { medicalLevel };

                    foreach (var kv in levelInfo)
                    {
                        var thisLevel = kv.Key;
                        var thisLevelInfo = kv.Value;

                        //Since links to other levels are bidirectional, ensure we only add each level once
                        foreach (var connectionToOtherLevel in thisLevelInfo.ConnectionsToOtherLevels)
                        {
                            var otherLevel = connectionToOtherLevel.Key;
                            var otherLevelInfo = levelInfo[otherLevel];

                            var thisLevelElevator = connectionToOtherLevel.Value.Target;
                            var otherLevelElevator = otherLevelInfo.ConnectionsToOtherLevels[thisLevel].Target;

                            var levelConnection = new Connection(thisLevelElevator, otherLevelElevator);

                            if (!levelsAdded.Contains(otherLevel))
                            {
                                mapInfoBuilder.AddConstructedLevel(otherLevel, otherLevelInfo.LevelGenerator.ConnectivityMap, otherLevelInfo.LevelGenerator.GetRoomTemplatesInWorldCoords(),
                                otherLevelInfo.LevelGenerator.GetDoorsInMapCoords(), levelConnection);

                                LogFile.Log.LogEntryDebug("Adding level connection " + thisLevelInfo.LevelNo + ":" + connectionToOtherLevel.Key + " via nodes" +
                                    thisLevelElevator + "->" + otherLevelElevator, LogDebugLevel.Medium);

                                levelsAdded.Add(otherLevel);
                            }
                        }
                    }

                    mapInfo = new MapInfo(mapInfoBuilder);

                    //Add maps to the dungeon (must be ordered)
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

                    //Set player's start location (must be done before adding items)

                    var firstRoom = mapInfo.GetRoom(0);
                    Game.Dungeon.Levels[0].PCStartLocation = new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);
                    
                    //Recalculate walkable to allow placing objects
                    Game.Dungeon.RefreshAllLevelPathingAndFOV();

                    //Add elevator features to link the maps
                    AddElevatorFeatures(mapInfo, levelInfo);

                    //Generate quests at mapmodel level
                    //GenerateQuests(mapInfo, levelInfo, startRoom);

                    //Add clues and locks at dungeon engine level
                    //AddSimpleCluesAndLocks(mapInfo);

                    //Add non-interactable features
                    //var escapePodsRoom = mapInfo.GetRoom(escapePodsConnection.Target);
                    //AddStandardDecorativeFeaturesToRoom(escapePodsLevel, escapePodsRoom, 50, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Machine]);

                    break;
                }
                //This should be all exceptions, we're just failing fast for now
                catch (OutOfMemoryException ex)
                {
                    LogFile.Log.LogEntryDebug("Failed to create dungeon: " + ex.Message, LogDebugLevel.High);
                    //Try again
                }

            } while (true);

            return mapInfo;
        }


        /*private IEnumerable<Connection> GetCriticalRouteBetweenElevators(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, int startLevel, int endLevel)
        {
            var 

            return mapInfo.Model.GetPathBetweenVerticesInReducedMap(bridgeTransitConnection.Target, bridgeMainBridgeConnection.Target);
        }*/

        private IEnumerable<int> RoomsInConnectionSet(IEnumerable<int> testRooms, IEnumerable<Connection> connectionSet)
        {
            return connectionSet.Where(c => testRooms.Contains(c.Source) && testRooms.Contains(c.Target)).SelectMany(c => new List<int>{c.Source, c.Target}).Distinct();
        }

        private IEnumerable<Connection> ConnectionsWithinRoomSet(IEnumerable<int> testRooms, IEnumerable<Connection> connectionSet)
        {
            return connectionSet.Where(c => testRooms.Contains(c.Source) && testRooms.Contains(c.Target));
        }

        private IEnumerable<int> RoomsInDescendingDistanceFromSource(MapInfo mapInfo, int sourceRoom, IEnumerable<int> testRooms)
        {
            var deadEndDistancesFromStartRoom = mapInfo.Model.GetDistanceOfVerticesFromParticularVertexInFullMap(sourceRoom, testRooms);
            var verticesByDistance = deadEndDistancesFromStartRoom.OrderByDescending(kv => kv.Value).Select(kv => kv.Key);

            return verticesByDistance;
        }

        private void GenerateQuests(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, int startRoom)
        {
            var mapHeuristics = new MapHeuristics(mapInfo.Model.GraphNoCycles, startRoom);
            var roomConnectivityMap = mapHeuristics.GetTerminalBranchConnections();

            BuildMainQuest(mapInfo, levelInfo, startRoom, roomConnectivityMap);

            BuildMedicalLevelQuests(mapInfo, levelInfo, startRoom, roomConnectivityMap);
        }
        
        private void BuildMedicalLevelQuests(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, int startRoom, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            //Lock the door to the elevator and require a certain number of monsters to be killed
            var elevatorConnection = levelInfo[medicalLevel].ConnectionsToOtherLevels.First().Value;

            var manager = mapInfo.Model.DoorAndClueManager;

            var doorId = "medical-security";
            int objectsToPlace = 20;
            int objectsToDestroy = 2;

            //Place door
            manager.PlaceDoor(new DoorRequirements(elevatorConnection, doorId, objectsToDestroy));
            var door = manager.GetDoorById(doorId);

            var lockedDoor = new Locks.SimpleLockedDoorWithMovie(door, "t_medicalsecurityunlocked", "t_medicalsecuritylocked");
            var doorInfo = mapInfo.GetDoorForConnection(door.DoorConnectionFullMap);
            lockedDoor.LocationLevel = doorInfo.LevelNo;
            lockedDoor.LocationMap = doorInfo.MapLocation;

            Game.Dungeon.AddLock(lockedDoor);

            placedDoors.Add(door);

            //Place monsters (not in corridors)

            //This will be restricted to the medical level since we cut off the door
            var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            allowedRoomsForClues = mapInfo.FilterOutCorridors(allowedRoomsForClues);
            var roomsToPlaceMonsters = new List<int>();

            var roomsForMonsters = GetRandomRoomsForClues(mapInfo, objectsToPlace, allowedRoomsForClues);
            var clues = manager.AddCluesToExistingDoor(doorId, roomsForMonsters);

            PlaceCreatureClues<Creatures.Camera>(mapInfo, clues, true);

            //Place log entries explaining the puzzle
            //These will not be turned into in-engine clue items, so they can't be used to open the door
            //They are added though, to ensure that they are readable before the door is opened

            var roomsForLogs = GetRandomRoomsForClues(mapInfo, 2, allowedRoomsForClues);
            var logClues = manager.AddCluesToExistingDoor(doorId, roomsForLogs);

            //try
            //{
                var log1 = new Tuple<LogEntry, Clue>(logGen.GenerateElevatorLogEntry(medicalLevel, lowerAtriumLevel), logClues[0]);
                var log2 = new Tuple<LogEntry, Clue>(logGen.GenerateArbitaryLogEntry("qe_medicalsecurity"), logClues[1]);
                PlaceLogClues(mapInfo, new List<Tuple<LogEntry, Clue>> { log1, log2 });
            //}
           // catch (Exception)
            //{
                //Ignore log problems
            //}
        }

        private List<int> GetRandomRoomsForClues(MapInfo info, int objectsToPlace, IEnumerable<int> allowedRoomsForClues)
        {
            if (allowedRoomsForClues.Count() == 0)
                throw new ApplicationException("Not enough rooms to place clues");

            //To get an even distribution we need to take into account how many nodes are in each group node
            var expandedAllowedRoomForClues = allowedRoomsForClues.SelectMany(r => Enumerable.Repeat(r, info.Model.GraphNoCycles.roomMappingNoCycleToFullMap[r].Count()));

            var roomsToPlaceMonsters = new List<int>();

            while (roomsToPlaceMonsters.Count() < objectsToPlace)
            {
                foreach (var room in expandedAllowedRoomForClues.Shuffle())
                {
                    roomsToPlaceMonsters.Add(room);
                    if (roomsToPlaceMonsters.Count() == objectsToPlace)
                        break;
                }
            }

            return roomsToPlaceMonsters;
        }

        private void BuildMainQuest(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, int startRoom, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            var deadEndRooms = roomConnectivityMap[0];
            //MAIN QUEST

            //Escape pod door
            //Requires enabling self-destruct and fueling pods

            escapePodsConnection = levelInfo[flightDeck].ReplaceableVaultConnections[0];
            escapePodsLevel = flightDeck;
            levelInfo[flightDeck].ReplaceableVaultConnectionsUsed.Add(escapePodsConnection);

            //TODO: replace vault in map

            mapInfo.Model.DoorAndClueManager.PlaceDoor(new DoorRequirements(escapePodsConnection, "escape", 2));

            //Self destruct requires captain's id
            int selfDestructLevel = medicalLevel;
            var selfDestructLevelIndices = mapInfo.GetRoomIndicesForLevel(selfDestructLevel);
            var deadEndsInMedical = RoomsInConnectionSet(selfDestructLevelIndices, deadEndRooms);
            var roomsInDistanceOrderFromStart = RoomsInDescendingDistanceFromSource(mapInfo, startRoom, deadEndsInMedical);
            var roomsFarFromStart = roomsInDistanceOrderFromStart.ElementAt(0);

            mapInfo.Model.DoorAndClueManager.PlaceObjective(new ObjectiveRequirements(roomsFarFromStart, "self-destruct", 1, new List<string> { "escape" }));

            //Captain's id
            int captainIdLevel = lowerAtriumLevel;
            var captainIdLevelIndices = mapInfo.GetRoomIndicesForLevel(captainIdLevel);
            var randomRoomForCaptainId = captainIdLevelIndices.RandomElement();

            mapInfo.Model.DoorAndClueManager.AddCluesToExistingObjective("self-destruct", new List<int> { randomRoomForCaptainId });

            //Fueling system
            int fuelingLevel = lowerAtriumLevel;
            var fuelingLevelIndices = mapInfo.GetRoomIndicesForLevel(fuelingLevel);
            var randomRoomForFueling = fuelingLevelIndices.RandomElement();

            mapInfo.Model.DoorAndClueManager.AddCluesToExistingDoor("escape", new List<int> { randomRoomForFueling });
        }

        private static void AddElevatorFeatures(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            var elevatorLocations = new Dictionary<Tuple<int, int>, Point>();

            foreach (var kv in levelInfo)
            {
                var thisLevelNo = kv.Key;
                var thisLevelInfo = kv.Value;

                foreach (var connectionToOtherLevel in thisLevelInfo.ConnectionsToOtherLevels)
                {
                    var elevatorLoc = mapInfo.GetRandomPointInRoomOfTerrain(connectionToOtherLevel.Value.Target, RoomTemplateTerrain.Floor);
                    elevatorLocations[new Tuple<int, int>(thisLevelNo, connectionToOtherLevel.Key)] = elevatorLoc;
                }
            }

            foreach (var kv in elevatorLocations)
            {
                var sourceLevel = kv.Key.Item1;
                var targetLevel = kv.Key.Item2;

                var sourceToTargetElevator = kv.Value;
                var targetToSourceElevator = elevatorLocations[new Tuple<int, int>(targetLevel, sourceLevel)];

                Game.Dungeon.AddFeature(new Features.Elevator(targetLevel, targetToSourceElevator), sourceLevel, sourceToTargetElevator);

                LogFile.Log.LogEntryDebug("Adding elevator connection " + sourceLevel + ":" + targetLevel + " via points" +
                    sourceToTargetElevator + "->" + targetToSourceElevator, LogDebugLevel.Medium);
            }
        }

        private void PlaceCreatureClues<T>(MapInfo mapInfo, List<Clue> monsterCluesToPlace, bool autoPickup) where T : Monster, new()
        {
            foreach (var clue in monsterCluesToPlace)
            {
                if (placedClues.Contains(clue))
                    continue;

                var roomsForClue = GetAllWalkablePointsToPlaceClueBoundariesOnly(mapInfo, clue, true);

                if (!roomsForClue.Item2.Any())
                    roomsForClue = GetAllWalkablePointsToPlaceClue(mapInfo, clue, true);

                var levelForClue = roomsForClue.Item1;
                var allWalkablePoints = roomsForClue.Item2;

                bool placedItem = false;

                var newMonster = new T();
                Item clueItem;
                if (autoPickup)
                    clueItem = new Items.ClueAutoPickup(clue);
                else
                    clueItem = new Items.Clue(clue);

                newMonster.PickUpItem(clueItem);
                
                foreach (Point p in allWalkablePoints)
                {
                    placedItem = Game.Dungeon.AddMonster(newMonster, levelForClue, p);

                    if (placedItem)
                        break;
                }

                if (!placedItem)
                    throw new ApplicationException("Nowhere to place monster");

                placedClues.Add(clue);
            }
        }

        private void PlaceLogClues(MapInfo mapInfo, List<Tuple<LogEntry, Clue>> logCluesToPlace)
        {
            foreach (var t in logCluesToPlace)
            {
                var clue = t.Item2;
                var logEntry = t.Item1;

                if (placedClues.Contains(clue))
                    continue;

                var roomsForClue = GetAllWalkablePointsToPlaceClueBoundariesOnly(mapInfo, clue, true);

                if (!roomsForClue.Item2.Any())
                    roomsForClue = GetAllWalkablePointsToPlaceClue(mapInfo, clue, true);

                var levelForClue = roomsForClue.Item1;
                var allWalkablePoints = roomsForClue.Item2;

                bool placedItem = false;

                var logItem = new Items.Log(logEntry);

                foreach (Point p in allWalkablePoints)
                {
                    placedItem = Game.Dungeon.AddItem(logItem, levelForClue, p);

                    if (placedItem)
                        break;
                }

                if (!placedItem)
                    throw new ApplicationException("Nowhere to place item");

                placedClues.Add(clue);
            }
        }

        private Tuple<int, IEnumerable<Point>> GetAllWalkablePointsToPlaceClue(MapInfo mapInfo, Clue clue, bool filterCorridors)
        {
            var possibleRooms = clue.PossibleClueRoomsInFullMap;
            IEnumerable<int> candidateRooms = possibleRooms;
            if (filterCorridors)
                candidateRooms = mapInfo.FilterOutCorridors(possibleRooms);

            //Must be on the same level
            var levelForRandomRoom = mapInfo.GetLevelForRoomIndex(candidateRooms.First());

            var allWalkablePoints = new List<Point>();

            //Hmm, could be quite expensive
            foreach (var room in candidateRooms)
            {
                var allPossiblePoints = mapInfo.GetAllPointsInRoomOfTerrain(room, RoomTemplateTerrain.Floor);
                allWalkablePoints.AddRange(Game.Dungeon.GetWalkablePointsFromSet(levelForRandomRoom, allPossiblePoints));
            }
            
            return new Tuple<int, IEnumerable<Point>>(levelForRandomRoom, allWalkablePoints.Shuffle());
        }

        private Tuple<int, IEnumerable<Point>> GetAllWalkablePointsToPlaceClueBoundariesOnly(MapInfo mapInfo, Clue clue, bool filterCorridors)
        {
            var possibleRooms = clue.PossibleClueRoomsInFullMap;
            IEnumerable<int> candidateRooms = possibleRooms;
            if (filterCorridors)
                candidateRooms = mapInfo.FilterOutCorridors(possibleRooms);

            //Must be on the same level
            var levelForRandomRoom = mapInfo.GetLevelForRoomIndex(candidateRooms.First());

            var allWalkablePoints = new List<Point>();

            //Hmm, could be quite expensive
            foreach (var room in candidateRooms)
            {
                var allPossiblePoints = mapInfo.GetBoundaryPointsInRoomOfTerrain(room);
                allWalkablePoints.AddRange(Game.Dungeon.GetWalkablePointsFromSet(levelForRandomRoom, allPossiblePoints));
            }

            return new Tuple<int, IEnumerable<Point>>(levelForRandomRoom, allWalkablePoints.Shuffle());
        }

        private Tuple<int, IEnumerable<Point>> GetAllWalkablePointsToPlaceObjective(MapInfo mapInfo, Objective clue)
        {
            var possibleRooms = clue.PossibleClueRoomsInFullMap;
            var randomRoom = possibleRooms.RandomElement();
            var levelForRandomRoom = mapInfo.GetLevelForRoomIndex(randomRoom);

            var allPossiblePoints = mapInfo.GetAllPointsInRoomOfTerrain(randomRoom, RoomTemplateTerrain.Floor);
            var allWalkablePoints = Game.Dungeon.GetWalkablePointsFromSet(levelForRandomRoom, allPossiblePoints);

            return new Tuple<int, IEnumerable<Point>>(levelForRandomRoom, allWalkablePoints);
        }

        /// <summary>
        /// Add any remaining clues, locks and objectives as simple types (to ensure we don't miss anything)
        /// </summary>
        /// <param name="mapInfo"></param>
        private void AddSimpleCluesAndLocks(MapInfo mapInfo)
        {
            //Add clues

            //Find a random room corresponding to a vertex with a clue and place a clue there
            foreach (var cluesAtVertex in mapInfo.Model.DoorAndClueManager.ClueMap)
            {
                foreach (var clue in cluesAtVertex.Value)
                {
                    if (placedClues.Contains(clue))
                        continue;

                    var roomsForClue = GetAllWalkablePointsToPlaceClue(mapInfo, clue, false);
                    var levelForRandomRoom = roomsForClue.Item1;
                    var allWalkablePoints = roomsForClue.Item2;

                    bool placedItem = false;
                    foreach (Point p in allWalkablePoints)
                    {
                        placedItem = Game.Dungeon.AddItem(new Items.Clue(clue), levelForRandomRoom, p);
                        
                        if (placedItem)
                            break;
                    }

                    if (!placedItem)
                    {
                        var str = "Can't place clue " + clue.OpenLockIndex;
                        LogFile.Log.LogEntryDebug(str, LogDebugLevel.High);
                        throw new ApplicationException(str);
                    }

                    placedClues.Add(clue);
                }

            }

            //Add locks to dungeon as simple doors

            foreach (var door in mapInfo.Model.DoorAndClueManager.DoorMap.Values)
            {
                if (placedDoors.Contains(door))
                    continue;

                var lockedDoor = new Locks.SimpleLockedDoor(door);
                var doorInfo = mapInfo.GetDoorForConnection(door.DoorConnectionFullMap);
                lockedDoor.LocationLevel = doorInfo.LevelNo;
                lockedDoor.LocationMap = doorInfo.MapLocation;

                LogFile.Log.LogEntryDebug("Lock door level " + lockedDoor.LocationLevel + " loc: " + doorInfo.MapLocation, LogDebugLevel.High);

                Game.Dungeon.AddLock(lockedDoor);

                placedDoors.Add(door);
            }

            //Add objectives to dungeon as simple objectives

            foreach (var objAtVertex in mapInfo.Model.DoorAndClueManager.ObjectiveRoomMap)
            {
                foreach (var obj in objAtVertex.Value)
                {

                    if (placedObjectives.Contains(obj))
                        continue;

                    var possibleRooms = obj.PossibleClueRoomsInFullMap;
                    var randomRoom = possibleRooms.RandomElement();
                    var levelForRandomRoom = mapInfo.GetLevelForRoomIndex(randomRoom);

                    var allPossiblePoints = mapInfo.GetAllPointsInRoomOfTerrain(randomRoom, RoomTemplateTerrain.Floor);
                    var allWalkablePoints = Game.Dungeon.GetWalkablePointsFromSet(levelForRandomRoom, allPossiblePoints);

                    bool placedItem = false;
                    foreach (Point p in allWalkablePoints)
                    {
                        var objectiveFeature = new Features.SimpleObjective(obj, mapInfo.Model.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(obj));
                        placedItem = Game.Dungeon.AddFeature(objectiveFeature, levelForRandomRoom, p);

                        if (placedItem)
                            break;
                    }

                    if (!placedItem)
                    {
                        var str = "Can't place objective " + obj.Id;
                        LogFile.Log.LogEntryDebug(str, LogDebugLevel.High);
                        throw new ApplicationException(str);
                    }

                    placedObjectives.Add(obj);
                }
            }
        }

        private LevelInfo GenerateMedicalLevel(int levelNo)
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
            var templateGenerator = new TemplatedMapGenerator(mapBuilder);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, medicalBay);

            int numberOfRandomRooms = 10;

            BuildTXShapedRooms(templateGenerator, numberOfRandomRooms);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 2;

            medicalInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(templateGenerator, corridor1, placeHolderVault, maxPlaceHolders));

            //Add extra corridors
            AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            //Wall type
            medicalInfo.TerrainMapping = irisTerrainMapping;

            return medicalInfo;
        }

        private LevelInfo GenerateScienceLevel(int levelNo, int startVertexIndex)
        {
            var levelInfo = new LevelInfo(levelNo);

            //Load standard room types
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            levelInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            levelInfo.LevelGenerator = templateGenerator;

            //Load sample templates
            RoomTemplate branchRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.branchroom.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate branchRoom2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.branchroom2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber1Doors = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber7x3_1door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber2Doors = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber7x3_2door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber1Doors2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber6x4_1door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber2Doors2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber6x4_2door.room", StandardTemplateMapping.terrainMapping);

            //Build a network of branched corridors

            //Place branch rooms to form the initial structure, joined on long axis
            PlaceOriginRoom(templateGenerator, branchRoom);

            PlaceRandomConnectedRooms(templateGenerator, 3, branchRoom, corridor1, 0, 0, () => Game.Random.Next(1) > 0 ? 3 : 4);
            PlaceRandomConnectedRooms(templateGenerator, 3, branchRoom2, corridor1, 0, 0, () => Game.Random.Next(1) > 0 ? 2 : 3);

            //Add some 2-door rooms
            var twoDoorDistribution = new List<Tuple<int, RoomTemplate>> {
                new Tuple<int, RoomTemplate>(2, chamber1Doors),
                new Tuple<int, RoomTemplate>(2, chamber1Doors2)
            };

            PlaceRandomConnectedRooms(templateGenerator, 10, twoDoorDistribution, corridor1, 0, 0);

            //Add some 1-door deadends

            var oneDoorDistribution = new List<Tuple<int, RoomTemplate>> {
                new Tuple<int, RoomTemplate>(2, chamber2Doors),
                new Tuple<int, RoomTemplate>(2, chamber2Doors2)
            };
            PlaceRandomConnectedRooms(templateGenerator, 10, oneDoorDistribution, corridor1, 0, 0);
            
            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, levelInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                levelInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 2;

            levelInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(templateGenerator, corridor1, placeHolderVault, maxPlaceHolders));

            //Add extra corridors
            //AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            //templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            //Wall type
            levelInfo.TerrainMapping = lineTerrainMapping;

            return levelInfo;
        }

        private LevelInfo GenerateStorageLevel(int levelNo, int startVertexIndex)
        {
            var medicalInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate originRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, originRoom);

            int numberOfRandomRooms = 12;

            GenerateLargeRooms(templateGenerator, numberOfRandomRooms);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 2;

            medicalInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(templateGenerator, corridor1, placeHolderVault, maxPlaceHolders));

            //Add extra corridors
            AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            return medicalInfo;
        }

        private LevelInfo GenerateReactorLevel(int levelNo, int startVertexIndex)
        {
            var medicalInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate originRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.reactor1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate deadEnd = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way_1door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, originRoom);

            int numberOfRandomRooms = 20;

            GenerateClosePackedSquareRooms(templateGenerator, numberOfRandomRooms);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 2;

            medicalInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(templateGenerator, corridor1, placeHolderVault, maxPlaceHolders));

            //If we have any more doors, add a couple of dead ends
            PlaceRandomConnectedRooms(templateGenerator, 3, deadEnd, corridor1, 0, 0);

            //Add extra corridors
            AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            //Wall type
            medicalInfo.TerrainMapping = securityTerrainMapping;

            return medicalInfo;
        }

        private LevelInfo GenerateArcologyLevel(int levelNo, int startVertexIndex)
        {
            var medicalInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate originRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_special1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate arcologyBig = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_vault_big1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate arcologySmall = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_vault_small1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate arcologyTiny = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_vault_tiny1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate arcologyOval = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_vault_oval1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, originRoom);

            int numberOfRandomRooms = 12;

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>> { 
                new Tuple<int, RoomTemplate>(100, arcologyBig),
                new Tuple<int, RoomTemplate>(100, arcologySmall),
                new Tuple<int, RoomTemplate>(50, arcologyOval)};

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 4, 6);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 2;

            medicalInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(templateGenerator, corridor1, placeHolderVault, maxPlaceHolders));

            //If we have any more doors, add a couple of dead ends
            PlaceRandomConnectedRooms(templateGenerator, 3, arcologyTiny, corridor1, 0, 0);

            //Add extra corridors
            AddCorridorsBetweenOpenDoors(templateGenerator, 1, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            //Wall type
            medicalInfo.TerrainMapping = bioTerrainMapping;

            return medicalInfo;
        }

        private void GenerateLargeRooms(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate largeRoom1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.largeconnectingvault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate largeRoom2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.largeconnectingvault2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);
            
            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>>();

            for (int i = 0; i < 10; i++)
            {
                allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(2, RoomTemplateUtilities.BuildRandomRectangularRoom(6, 14, 6, 14, 4, 4)));
            }

            allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom1));
            allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom2));

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 0, 0);
        }

        private void GenerateClosePackedSquareRooms(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate largeRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way3.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate smallRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate tinyRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>>();

            //allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom1));
            //allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom2));

            int numberOfLargeRooms = (int)Math.Ceiling(numberOfRandomRooms / 2.0);
            int numberOfMediumRooms = (int)Math.Ceiling(numberOfRandomRooms / 6.0);
            int numberOfSmallRooms = numberOfRandomRooms - numberOfLargeRooms - numberOfMediumRooms;

            PlaceRandomConnectedRooms(templateGenerator, numberOfLargeRooms, largeRoom, corridor1, 0, 0);
            PlaceRandomConnectedRooms(templateGenerator, numberOfMediumRooms, smallRoom, corridor1, 0, 0);
            PlaceRandomConnectedRooms(templateGenerator, numberOfSmallRooms, tinyRoom, corridor1, 0, 0);
        }

        private void BuildTXShapedRooms(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate lshapeRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.lshape2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate lshapeAsymmetric = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.lshape_asymmetric2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate tshape = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.tshape1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate xshape = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.xshape1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>> { 
                new Tuple<int, RoomTemplate>(100, lshapeRoom),
                new Tuple<int, RoomTemplate>(100, lshapeAsymmetric),
                new Tuple<int, RoomTemplate>(100, tshape),
                new Tuple<int, RoomTemplate>(100, xshape) };

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 4, 6);
        }

        private LevelInfo GenerateStandardLevel(int levelNo, int startVertexIndex)
        {
            var medicalInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, room1);
            PlaceRandomConnectedRooms(templateGenerator, 4, room1, corridor1, 5, 10);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 2;

            medicalInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(templateGenerator, corridor1, placeHolderVault, maxPlaceHolders));

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            return medicalInfo;
        }

        private List<Tuple<int, Connection>> AddConnectionsToOtherLevels(int levelNo, LevelInfo medicalInfo, RoomTemplate corridor1, RoomTemplate elevatorVault, TemplatedMapGenerator templateGenerator)
        {
            var otherLevelConnections = LevelLinks.GetAllConnections().Where(c => c.IncludesVertex(levelNo)).Select(c => c.Source == levelNo ? c.Target : c.Source);
            var connectionsToReturn = new List<Tuple<int, Connection>>();

            foreach (var otherLevel in otherLevelConnections)
            {
                var connectingRoom = AddRoomToRandomOpenDoor(templateGenerator, elevatorVault, corridor1, 3);
                connectionsToReturn.Add(new Tuple<int, Connection>(otherLevel, connectingRoom));
            }

            return connectionsToReturn;
        }

        private List<Connection> AddReplaceableVaults(TemplatedMapGenerator templateGenerator, RoomTemplate corridor1, RoomTemplate placeHolderVault, int maxPlaceHolders)
        {
            var vaultsToReturn = new List<Connection>();
            int cargoTotalPlaceHolders = 0;
            do
            {
                var placeHolderRoom = AddRoomToRandomOpenDoor(templateGenerator, placeHolderVault, corridor1, 3);
                if (placeHolderRoom != null)
                {
                    vaultsToReturn.Add(placeHolderRoom);
                    cargoTotalPlaceHolders++;
                }
                else
                    break;
            } while (cargoTotalPlaceHolders < maxPlaceHolders);
            return vaultsToReturn;
        }

        private LevelInfo GenerateLowerAtriumLevel(int levelNo)
        {
            var medicalInfo = new LevelInfo(levelNo);

            return medicalInfo;
        }

        private static void AddFeaturesToRoom<T>(int level, TemplatePositioned positionedRoom, int featuresToPlace) where T: Feature, new()
        {
            var bridgeRouter = new RoomFilling(positionedRoom.Room);

            var floorPoints = RoomTemplateUtilities.GetPointsInRoomWithTerrain(positionedRoom.Room, RoomTemplateTerrain.Floor);

            for (int i = 0; i < featuresToPlace; i++)
            {
                var randomPoint = floorPoints.RandomElement();
                floorPoints.Remove(randomPoint);

                if (bridgeRouter.SetSquareUnWalkableIfMaintainsConnectivity(randomPoint))
                {
                    var featureLocationInMapCoords = positionedRoom.Location + randomPoint;
                    Game.Dungeon.AddFeatureBlocking(new T(), level, featureLocationInMapCoords, true);

                    LogFile.Log.LogEntryDebug("Placing feature in room " + positionedRoom.RoomIndex + " at location " + featureLocationInMapCoords, LogDebugLevel.Medium);
                }

                if (floorPoints.Count() == 0)
                    break;
            }
        }

        private static void AddStandardDecorativeFeaturesToRoom(int level, TemplatePositioned positionedRoom, int featuresToPlace, DecorationFeatureDetails.Decoration decorationDetails)
        {
            var bridgeRouter = new RoomFilling(positionedRoom.Room);

            var floorPoints = RoomTemplateUtilities.GetPointsInRoomWithTerrain(positionedRoom.Room, RoomTemplateTerrain.Floor);

            for (int i = 0; i < featuresToPlace; i++)
            {
                var randomPoint = floorPoints.RandomElement();
                floorPoints.Remove(randomPoint);

                if (bridgeRouter.SetSquareUnWalkableIfMaintainsConnectivity(randomPoint))
                {
                    var featureLocationInMapCoords = positionedRoom.Location + randomPoint;
                    Game.Dungeon.AddFeatureBlocking(new Features.StandardDecorativeFeature(decorationDetails.representation, decorationDetails.colour), level, featureLocationInMapCoords, true);

                    LogFile.Log.LogEntryDebug("Placing feature in room " + positionedRoom.RoomIndex + " at location " + featureLocationInMapCoords, LogDebugLevel.Medium);
                }

                if (floorPoints.Count() == 0)
                    break;
            }
        }

        T RandomItem<T>(IEnumerable<T> items)
        {
            var totalItems = items.Count();
            if (totalItems == 0)
                throw new ApplicationException("Empty list for randomization");

            return items.ElementAt(Game.Random.Next(totalItems));
        }

        Connection AddRoomToRandomOpenDoor(TemplatedMapGenerator gen, RoomTemplate templateToPlace, RoomTemplate corridorTemplate, int distanceFromDoor)
        {
            var doorsToTry = gen.PotentialDoors.Shuffle();
            
            foreach(var door in doorsToTry) {
                try {
                    return gen.PlaceRoomTemplateAlignedWithExistingDoor(templateToPlace, corridorTemplate, RandomDoor(gen), 0, distanceFromDoor);
                }
                catch (ApplicationException)
                {
                    //No good, continue
                }
            }

            throw new ApplicationException("No applicable doors left");
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
            //var allMatchingDoorPossibilities = allLDoorPossibilities;
            //var allMatchingDoorPossibilities = allBendDoorPossibilities;

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

            //Previous code (was super-slow!)
            //while (allMatchingDoorPossibilities.Any() && extraConnections < totalExtraConnections)
            //In any case, remove this attempt
            //var doorsToTry = allMatchingDoorPossibilities.ElementAt(Game.Random.Next(allMatchingDoorPossibilities.Count()));
            //allMatchingDoorPossibilities = allMatchingDoorPossibilities.Except(Enumerable.Repeat(doorsToTry, 1)); //order n - making it slow?

        }

        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, int maxRoomDistance)
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
                    try
                    {
                        templatedGenerator.PlaceRoomTemplateAtPosition(RandomRoom(), new Point(Game.Random.Next(maxRoomDistance), Game.Random.Next(maxRoomDistance)));
                        roomsPlaced++;
                    }
                    catch (ApplicationException) { }

                    attempts++;
                }
                else
                {
                    //Find a random potential door and try to grow a random room off this
                    var randomRoom = RandomRoom();
                    var randomDoorInRoom = Game.Random.Next(randomRoom.PotentialDoors.Count);
                    try
                    {
                        templatedGenerator.PlaceRoomTemplateAlignedWithExistingDoor(randomRoom, corridorTemplates[0], RandomDoor(templatedGenerator),
                        randomDoorInRoom,
                        Game.Random.Next(maxRoomDistance));
                        roomsPlaced++;
                    }
                    catch (ApplicationException) { }

                    attempts++;
                }
            } while (roomsPlaced < roomsToPlace && attempts < maxAttempts && templatedGenerator.HaveRemainingPotentialDoors());
            return roomsPlaced;
        }

        private void PlaceOriginRoom(TemplatedMapGenerator templatedGenerator, RoomTemplate roomToPlace)
        {
             templatedGenerator.PlaceRoomTemplateAtPosition(roomToPlace, new Point(0, 0));
        }

        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, RoomTemplate roomToPlace, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength)
        {
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, roomToPlace, corridorToPlace, minCorridorLength, maxCorridorLength, null);
        }
        
        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, List<RoomTemplate> roomToPlaces, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength)
        {
            var tuples = roomToPlaces.Select(r => new Tuple<int, RoomTemplate>(1, r));
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, tuples, corridorToPlace, minCorridorLength, maxCorridorLength, null);
        }

        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, List<Tuple<int,RoomTemplate>> roomToPlaceWithWeights, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength)
        {
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, roomToPlaceWithWeights, corridorToPlace, minCorridorLength, maxCorridorLength, null);
        }


        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, RoomTemplate roomToPlace, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength, Func<int> doorPicker)
        {
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, new List<Tuple<int,RoomTemplate>> { new Tuple<int, RoomTemplate>(1, roomToPlace) }, corridorToPlace, minCorridorLength, maxCorridorLength, doorPicker);
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
                var totalWeight = roomsToPlaceWithWeights.Select(t => t.Item1).Sum();
                var randomNumber = Game.Random.Next(totalWeight);

                int weightSoFar = 0;
                RoomTemplate roomToPlace = roomsToPlaceWithWeights.First().Item2;
                foreach (var t in roomsToPlaceWithWeights)
                {
                    weightSoFar += t.Item1;
                    if (weightSoFar > randomNumber)
                    {
                        roomToPlace = t.Item2;
                        break;
                    }
                }

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

        /** Build a map using templated rooms */
        public MapInfo GenerateTestGraphicsDungeon()
        {

            //Load standard room types
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.largetestvault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            //Build level 1

            var l1mapBuilder = new TemplatedMapBuilder(100, 100);
            var l1templateGenerator = new TemplatedMapGenerator(l1mapBuilder);

            PlaceOriginRoom(l1templateGenerator, room1);
            PlaceRandomConnectedRooms(l1templateGenerator, 1, room1, corridor1, 0, 0, () => 0);

            //Build the graph containing all the levels

            //Build and add the l1 map

            var mapInfoBuilder = new MapInfoBuilder();
            var startRoom = 0;
            mapInfoBuilder.AddConstructedLevel(0, l1templateGenerator.ConnectivityMap, l1templateGenerator.GetRoomTemplatesInWorldCoords(), l1templateGenerator.GetDoorsInMapCoords(), startRoom);

            MapInfo mapInfo = new MapInfo(mapInfoBuilder);

            //Add maps to the dungeon

            Map masterMap = l1mapBuilder.MergeTemplatesIntoMap(terrainMapping);
            Game.Dungeon.AddMap(masterMap);

            //Recalculate walkable to allow placing objects
            Game.Dungeon.RefreshAllLevelPathingAndFOV();

            //Set player's start location (must be done before adding items)

            //Set PC start location

            var firstRoom = mapInfo.GetRoom(0);
            masterMap.PCStartLocation = new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);

            //Add items
            var dungeon = Game.Dungeon;

            dungeon.AddItem(new Items.Pistol(), 0, new Point(1, 1));
            dungeon.AddItem(new Items.Shotgun(), 0, new Point(2, 1));
            dungeon.AddItem(new Items.Laser(), 0, new Point(3, 1));
            dungeon.AddItem(new Items.Vibroblade(), 0, new Point(4, 1));

            //Set map for visualisation
            return mapInfo;
        }

        public static KeyValuePair<int, int> MaxEntry(Dictionary<int, int> dict)
        {
            return dict.Aggregate((a, b) => a.Value > b.Value ? a : b);
        }
    }
}
