using GraphMap;
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

        //Wall mappings
        Dictionary<MapTerrain, List<MapTerrain>> brickTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> panelTerrainMapping;

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
            AddCorridorsBetweenOpenDoors(templatedGenerator, totalExtraConnections);

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
            AddCorridorsBetweenOpenDoors(templatedGenerator, totalExtraConnections);

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
            AddCorridorsBetweenOpenDoors(templatedGenerator, 10);

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
            }

            public int LevelNo { get; private set; }

            public Dictionary<int, Connection> ConnectionsToOtherLevels { get; set; }

            public TemplatedMapGenerator LevelGenerator { get; set; }
            public TemplatedMapBuilder LevelBuilder { get; set; }

            //Replaceable vault at target
            public List<Connection> ReplaceableVaultConnections { get; set; }
        }

        /** Build a map using templated rooms */
        public MapInfo GenerateDungeonWithStory()
        {
            //We catch exceptions on generation and keep looping
            MapInfo mapInfo;
            
            do
            {
                try
                {
                    //Generate the overall level structure
                    GenerateLevelLinks();

                    //Build each level individually

                    Dictionary<int, LevelInfo> levelInfo = new Dictionary<int, LevelInfo>();

                    //var medicalInfo = GenerateMedicalLevel(medicalLevel);
                    //levelInfo[medicalLevel] = medicalInfo;

                    foreach (var level in gameLevels)
                    {
                        var thisLevelInfo = GenerateStandardLevel(level, level * 100);
                        levelInfo[level] = thisLevelInfo;
                    }

                    //Build the room graph containing all levels

                    //Build and add the start level

                    var mapInfoBuilder = new MapInfoBuilder();
                    var startRoom = 0;
                    var startLevelInfo = levelInfo[medicalLevel];
                    mapInfoBuilder.AddConstructedLevel(0, startLevelInfo.LevelGenerator.ConnectivityMap, startLevelInfo.LevelGenerator.GetRoomTemplatesInWorldCoords(),
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
                    var mapHeuristics = new MapHeuristics(mapInfo.Model.GraphNoCycles, startRoom);

                    /*

                    //MAIN QUEST

                    //Escape pod door
                    //  - bridge self-destruct

                    mapInfo.Model.DoorAndClueManager.PlaceDoorAndClue(new DoorRequirements(escapePodsConnection, "escape"), bridgeMainBridgeConnection.Target);

                    //MAIN QUEST SUPPORT

                    //Level-local lock on bridge level on critical path to main bridge. Place clue a reasonable distance away, not on critical path (if possible)

                    var bridgeCriticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(bridgeTransitConnection.Target, bridgeMainBridgeConnection.Target);
                    var bridgeCriticalConnection = bridgeCriticalPath.ElementAt(bridgeCriticalPath.Count() / 2);

                    var allRoomsForCriticalClue = mapInfo.Model.DoorAndClueManager.GetValidRoomsToPlaceClueForDoor(bridgeCriticalConnection);
                    var bridgeRooms = mapInfo.GetRoomIndicesForLevel(1);
                    var bridgeCriticalPathRooms = bridgeCriticalPath.Select(c => c.Source).Union(bridgeCriticalPath.Select(c => c.Target));

                    var allowedBridgeRoomsNotOnCriticalPath = allRoomsForCriticalClue.Intersect(bridgeRooms).Except(bridgeCriticalPathRooms);

                    int roomForCriticalBridgeClue;
                    if (allowedBridgeRoomsNotOnCriticalPath.Count() > 0)
                    {
                        var distancesBetweenClueAndDoor = mapInfo.Model.GetDistanceOfVerticesFromParticularVertexInReducedMap(bridgeCriticalConnection.Source, allowedBridgeRoomsNotOnCriticalPath);

                        //Get room that is half maximum distance from door
                        var verticesByDistance = distancesBetweenClueAndDoor.OrderByDescending(kv => kv.Value).Select(kv => kv.Key);
                        roomForCriticalBridgeClue = verticesByDistance.ElementAt(verticesByDistance.Count() / 2);

                        //Or as far away as possible
                        roomForCriticalBridgeClue = MaxEntry(distancesBetweenClueAndDoor).Key;
                    }
                    else
                        roomForCriticalBridgeClue = allRoomsForCriticalClue.RandomElement();

                    mapInfo.Model.DoorAndClueManager.PlaceDoorAndClue(new DoorRequirements(bridgeCriticalConnection, "green"), roomForCriticalBridgeClue);

                    LogFile.Log.LogEntryDebug("L0 Critical Path, candidates: " + allowedBridgeRoomsNotOnCriticalPath.Count() + " lock at: " + bridgeCriticalConnection + " clue at " + roomForCriticalBridgeClue, LogDebugLevel.High);

                    */
                    
                    //Add maps to the dungeon
                    foreach (var kv in levelInfo)
                    {
                        var thisLevelInfo = kv.Value;

                        Map masterMap = thisLevelInfo.LevelBuilder.MergeTemplatesIntoMap(terrainMapping);

                        Map randomizedMapL1 = MapTerrainRandomizer.RandomizeTerrainInMap(masterMap, brickTerrainMapping);
                        Game.Dungeon.AddMap(randomizedMapL1);
                    }

                    //Set player's start location (must be done before adding items)

                    var firstRoom = mapInfo.GetRoom(0);
                    Game.Dungeon.Levels[0].PCStartLocation = new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);
                    
                    //Recalculate walkable to allow placing objects
                    Game.Dungeon.RefreshAllLevelPathingAndFOV();

                    //Add elevator features to link the maps

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

                    foreach(var kv in elevatorLocations) {

                        var sourceLevel = kv.Key.Item1;
                        var targetLevel = kv.Key.Item2;

                        var sourceToTargetElevator = kv.Value;
                        var targetToSourceElevator = elevatorLocations[new Tuple<int, int>(targetLevel, sourceLevel)];

                        Game.Dungeon.AddFeature(new Features.Elevator(targetLevel, targetToSourceElevator), sourceLevel, sourceToTargetElevator);

                        LogFile.Log.LogEntryDebug("Adding elevator connection " + sourceLevel + ":" + targetLevel + " via points" +
                            sourceToTargetElevator + "->" + targetToSourceElevator, LogDebugLevel.Medium);
                    }

                    //Add non-interactable features
                    /*
                    var bridgeRoomOnMap = mapInfo.GetRoom(bridgeMainBridgeConnection.Target);
                    AddStandardDecorativeFeaturesToRoom(1, bridgeRoomOnMap, 50, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Machine]);
                    var escapePodsRoom = mapInfo.GetRoom(escapePodsConnection.Target);
                    AddStandardDecorativeFeaturesToRoom(0, escapePodsRoom, 50, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Machine]);
                    */

                    AddCluesAndLocks(mapInfo);

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

        private static void AddCluesAndLocks(MapInfo mapInfo)
        {

            //Add clues

            //Find a random room corresponding to a vertex with a clue and place a clue there
            foreach (var cluesAtVertex in mapInfo.Model.DoorAndClueManager.ClueMap)
            {
                foreach (var clue in cluesAtVertex.Value)
                {
                    var possibleRooms = clue.PossibleClueRoomsInFullMap;
                    var randomRoom = possibleRooms[Game.Random.Next(possibleRooms.Count)];

                    bool placedItem = false;
                    do
                    {
                        var pointInRoom = mapInfo.GetRandomPointInRoomOfTerrain(randomRoom, RoomTemplateTerrain.Floor);

                        placedItem = Game.Dungeon.AddItem(new Items.Clue(clue), mapInfo.GetLevelForRoomIndex(randomRoom), pointInRoom);
                    } while (!placedItem);
                }

            }

            //Add locks to dungeon as simple doors

            foreach (var door in mapInfo.Model.DoorAndClueManager.DoorMap.Values)
            {
                var lockedDoor = new Locks.SimpleLockedDoor(door);
                var doorInfo = mapInfo.GetDoorForConnection(door.DoorConnectionFullMap);
                lockedDoor.LocationLevel = doorInfo.LevelNo;
                lockedDoor.LocationMap = doorInfo.MapLocation;

                LogFile.Log.LogEntryDebug("Lock door level " + lockedDoor.LocationLevel + " loc: " + doorInfo.MapLocation, LogDebugLevel.High);

                Game.Dungeon.AddLock(lockedDoor);
            }
        }

        private LevelInfo GenerateMedicalLevel(int levelNo)
        {
            var medicalInfo = new LevelInfo(levelNo);
            
            //Load standard room types

            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var cargoMapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = cargoMapBuilder;
            var cargoTemplateGenerator = new TemplatedMapGenerator(cargoMapBuilder);
            medicalInfo.LevelGenerator = cargoTemplateGenerator;

            PlaceOriginRoom(cargoTemplateGenerator, room1);
            PlaceRandomConnectedRooms(cargoTemplateGenerator, 4, room1, corridor1, 5, 10);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, cargoTemplateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 2;

            medicalInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(cargoTemplateGenerator, corridor1, placeHolderVault, maxPlaceHolders));

            //Add escape pods
            escapePodsConnection = AddRoomToRandomOpenDoor(cargoTemplateGenerator, placeHolderVault, corridor1, 3);

            //Tidy terrain
            cargoTemplateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            return medicalInfo;
        }

        private LevelInfo GenerateStandardLevel(int levelNo, int startVertexIndex)
        {
            var medicalInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var cargoMapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = cargoMapBuilder;
            var cargoTemplateGenerator = new TemplatedMapGenerator(cargoMapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = cargoTemplateGenerator;

            PlaceOriginRoom(cargoTemplateGenerator, room1);
            PlaceRandomConnectedRooms(cargoTemplateGenerator, 4, room1, corridor1, 5, 10);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, cargoTemplateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 2;

            medicalInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(cargoTemplateGenerator, corridor1, placeHolderVault, maxPlaceHolders));

            //Tidy terrain
            cargoTemplateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            return medicalInfo;
        }

        private List<Tuple<int, Connection>> AddConnectionsToOtherLevels(int levelNo, LevelInfo medicalInfo, RoomTemplate corridor1, RoomTemplate replacementVault, TemplatedMapGenerator cargoTemplateGenerator)
        {
            var otherLevelConnections = LevelLinks.GetAllConnections().Where(c => c.IncludesVertex(levelNo)).Select(c => c.Source == levelNo ? c.Target : c.Source);
            var connectionsToReturn = new List<Tuple<int, Connection>>();

            foreach (var otherLevel in otherLevelConnections)
            {
                var connectingRoom = AddRoomToRandomOpenDoor(cargoTemplateGenerator, replacementVault, corridor1, 3);
                connectionsToReturn.Add(new Tuple<int, Connection>(otherLevel, connectingRoom));
            }

            return connectionsToReturn;
        }

        private List<Connection> AddReplaceableVaults(TemplatedMapGenerator cargoTemplateGenerator, RoomTemplate corridor1, RoomTemplate placeHolderVault, int maxPlaceHolders)
        {
            var vaultsToReturn = new List<Connection>();
            int cargoTotalPlaceHolders = 0;
            do
            {
                var placeHolderRoom = AddRoomToRandomOpenDoor(cargoTemplateGenerator, placeHolderVault, corridor1, 3);
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


        private void AddCorridorsBetweenOpenDoors(TemplatedMapGenerator templatedGenerator, int totalExtraConnections)
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

        /// <summary>
        /// Failure mode is placing fewer rooms than requested
        /// </summary>
        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, RoomTemplate roomToPlace, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength, Func<int> doorPicker)
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
