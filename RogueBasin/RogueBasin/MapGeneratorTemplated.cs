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

        ConnectivityMap connectivityMap = null;

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

        /** Build a map using templated rooms */
        public MapInfo GenerateDungeonWithReplacedVaults()
        {

            //Load standard room types
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            //Build level 1

            var l1mapBuilder = new TemplatedMapBuilder(100, 100);
            var l1templateGenerator = new TemplatedMapGenerator(l1mapBuilder);

            PlaceOriginRoom(l1templateGenerator, room1);
            PlaceRandomConnectedRooms(l1templateGenerator, 5, room1, corridor1, 5, 10);

            //Add a place holder room for the elevator
            var l1elevatorConnection = AddRoomToRandomOpenDoor(l1templateGenerator, placeHolderVault, corridor1, 3);
            var l1elevatorIndex = l1elevatorConnection.Target;

            LogFile.Log.LogEntryDebug("Level 1 elevator at index " + l1elevatorIndex, LogDebugLevel.High);

            //Build level 2

            var l2mapBuilder = new TemplatedMapBuilder(100, 100);
            var l2templateGenerator = new TemplatedMapGenerator(l2mapBuilder, 100);

            PlaceOriginRoom(l2templateGenerator, room1);
            PlaceRandomConnectedRooms(l2templateGenerator, 5, room1, corridor1, 5, 10);

            //Add a place holder room for the elevator
            var l2elevatorConnection = AddRoomToRandomOpenDoor(l2templateGenerator, placeHolderVault, corridor1, 3);
            var l2elevatorIndex = l2elevatorConnection.Target;

            LogFile.Log.LogEntryDebug("Level 2 elevator at index " + l2elevatorIndex, LogDebugLevel.High);

            //Replace the placeholder vaults with the actual elevator rooms
            l1templateGenerator.ReplaceRoomTemplate(l1elevatorIndex, l1elevatorConnection, replacementVault, 0);
            l2templateGenerator.ReplaceRoomTemplate(l2elevatorIndex, l2elevatorConnection, replacementVault, 0);

            //Replace spare doors with walls
            l1templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);
            l2templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            //Build the graph containing all the levels

            //Build and add the l1 map

            var mapInfoBuilder = new MapInfoBuilder();
            var startRoom = 0;
            mapInfoBuilder.AddConstructedLevel(0, l1templateGenerator.ConnectivityMap, l1templateGenerator.GetRoomTemplatesInWorldCoords(), l1templateGenerator.GetDoorsInMapCoords(), startRoom);

            //Build and add the l2 map

            mapInfoBuilder.AddConstructedLevel(1, l2templateGenerator.ConnectivityMap, l2templateGenerator.GetRoomTemplatesInWorldCoords(), l2templateGenerator.GetDoorsInMapCoords(),
                new Connection(l1elevatorIndex, l2elevatorIndex));

            MapInfo mapInfo = new MapInfo(mapInfoBuilder);
            var mapHeuristics = new MapHeuristics(mapInfo.Model.GraphNoCycles, startRoom);

            //LOCKS

            //Add a locked door on a dead end, localised to level 0
            var level0Indices = mapInfo.GetRoomIndicesForLevel(0);
            var roomConnectivityMap = mapHeuristics.GetTerminalBranchConnections();

            var deadEnds = roomConnectivityMap[0];
            var deadEndsInLevel0 = deadEnds.Where(c => level0Indices.Contains(c.Source) && level0Indices.Contains(c.Target)).ToList();

            var randomDeadEndToLock = deadEndsInLevel0.RandomElement();

            var allRoomsForClue0 = mapInfo.Model.DoorAndClueManager.GetValidRoomsToPlaceClue(randomDeadEndToLock);
            var roomsForClue0Level0 = allRoomsForClue0.Intersect(level0Indices);
            var roomForClue0 = roomsForClue0Level0.RandomElement();

            LogFile.Log.LogEntryDebug("Lock door " + randomDeadEndToLock + " clue at " + roomForClue0, LogDebugLevel.High);

            mapInfo.Model.DoorAndClueManager.PlaceDoorAndClue(new DoorRequirements(randomDeadEndToLock, "yellow"), roomForClue0);

            //Add a locked door halfway along the critical path between the l0 and l1 elevators
            var l0CriticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(startRoom, l1elevatorIndex);
            var l0CriticalConnection = l0CriticalPath.ElementAt(l0CriticalPath.Count() / 2);

            var allRoomsForCriticalL0Clue = mapInfo.Model.DoorAndClueManager.GetValidRoomsToPlaceClue(l0CriticalConnection);
            var roomForCriticalL0Clue = allRoomsForCriticalL0Clue.RandomElement();

            mapInfo.Model.DoorAndClueManager.PlaceDoorAndClue(new DoorRequirements(l0CriticalConnection, "green"), roomForCriticalL0Clue);

            LogFile.Log.LogEntryDebug("L0 Critical Path, candidates: " + l0CriticalPath.Count() + " lock at: " + l0CriticalConnection + " clue at " + roomForCriticalL0Clue, LogDebugLevel.High);

            //Add a multi-level clue
            var level1Indices = mapInfo.GetRoomIndicesForLevel(1);

            var deadEndsInLevel1 = deadEnds.Where(c => level1Indices.Contains(c.Source) && level1Indices.Contains(c.Target)).ToList();

            var randomDeadEndToLockL1 = deadEndsInLevel1.RandomElement();

            var allRoomsForClue1 = mapInfo.Model.DoorAndClueManager.GetValidRoomsToPlaceClue(randomDeadEndToLockL1);
            var roomsForClue1Level0 = allRoomsForClue1.Intersect(level0Indices);
            var roomForClue1 = roomsForClue0Level0.RandomElement();

            LogFile.Log.LogEntryDebug("Lock door " + randomDeadEndToLock + " clue at " + roomForClue0, LogDebugLevel.High);

            mapInfo.Model.DoorAndClueManager.PlaceDoorAndClue(new DoorRequirements(randomDeadEndToLockL1, "red"), roomForClue1);

            //Add a locked door to a dead end, localised to level 1 and place the clue as far away as possible on that level

            var randomDeadEndToLockL1FarClue = deadEndsInLevel1.RandomElement();

            var allRoomsForFarClue = mapInfo.Model.DoorAndClueManager.GetValidRoomsToPlaceClue(randomDeadEndToLockL1FarClue);
            var roomsForFarClueLevel1 = allRoomsForFarClue.Intersect(level1Indices);
            var distancesBetweenClueAndDoor = mapInfo.Model.GetDistanceOfVerticesFromParticularVertexInReducedMap(randomDeadEndToLockL1.Source, roomsForFarClueLevel1);
            var roomForFarClue = MaxEntry(distancesBetweenClueAndDoor).Key;
            LogFile.Log.LogEntryDebug("Lock door " + randomDeadEndToLockL1FarClue + " clue at " + roomForFarClue, LogDebugLevel.High);

            mapInfo.Model.DoorAndClueManager.PlaceDoorAndClue(new DoorRequirements(randomDeadEndToLockL1FarClue, "magenta"), roomForFarClue);

            //Add maps to the dungeon

            Map masterMap = l1mapBuilder.MergeTemplatesIntoMap(terrainMapping);

            //Set player's start location (must be done before adding items)

            var firstRoom = mapInfo.GetRoom(0);
            masterMap.PCStartLocation = new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);

            //Add terrain and randomize walls

            Dictionary<MapTerrain, List<MapTerrain>> brickTerrainMapping = new Dictionary<MapTerrain,List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.BrickWall1, MapTerrain.BrickWall1, MapTerrain.BrickWall1, MapTerrain.BrickWall2, MapTerrain.BrickWall3, MapTerrain.BrickWall4, MapTerrain.BrickWall5 } }};

            Map randomizedMapL1 = MapTerrainRandomizer.RandomizeTerrainInMap(masterMap, brickTerrainMapping);
            Game.Dungeon.AddMap(randomizedMapL1);

            Map masterMapL2 = l2mapBuilder.MergeTemplatesIntoMap(terrainMapping);
            Dictionary<MapTerrain, List<MapTerrain>> panelTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.PanelWall1, MapTerrain.PanelWall1, MapTerrain.PanelWall1, MapTerrain.PanelWall2, MapTerrain.PanelWall3, MapTerrain.PanelWall4, MapTerrain.PanelWall5 } }};
            Map randomizedMapL2 = MapTerrainRandomizer.RandomizeTerrainInMap(masterMapL2, panelTerrainMapping);
            Game.Dungeon.AddMap(randomizedMapL2);

            //Recalculate walkable to allow placing objects
            Game.Dungeon.RefreshAllLevelPathingAndFOV();

            //Add elevator features to link the maps

            //L1 -> L2
            var elevator1Loc = mapInfo.GetRandomPointInRoomOfTerrain(l1elevatorIndex, RoomTemplateTerrain.Floor);
            var elevator2Loc = mapInfo.GetRandomPointInRoomOfTerrain(l2elevatorIndex, RoomTemplateTerrain.Floor);

            Game.Dungeon.AddFeature(new Features.Elevator(1, elevator2Loc), 0, elevator1Loc);
            Game.Dungeon.AddFeature(new Features.Elevator(0, elevator1Loc), 1, elevator2Loc);


            //Add clues

            //Find a random room corresponding to a vertex with a clue and place a clue there
            foreach (var cluesAtVertex in mapInfo.Model.DoorAndClueManager.ClueMap)
            {
                foreach (var clue in cluesAtVertex.Value)
                {
                    var possibleRooms = clue.PossibleClueRoomsInFullMap;
                    var randomRoom = possibleRooms[Game.Random.Next(possibleRooms.Count)];

                    var pointInRoom = mapInfo.GetRandomPointInRoomOfTerrain(randomRoom, RoomTemplateTerrain.Floor);

                    Game.Dungeon.AddItem(new Items.Clue(clue), mapInfo.GetLevelForRoomIndex(randomRoom), pointInRoom);
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

            //Set map for visualisation
            return mapInfo;

        }

        /** Build a map using templated rooms */
        public MapInfo GenerateDungeonWithStory()
        {

            //Load standard room types
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            bool dungeonCreationSuccessful = false;
            MapInfo mapInfo;

            do
            {

                //Build cargo bay level

                var cargoMapBuilder = new TemplatedMapBuilder(100, 100);
                var cargoTemplateGenerator = new TemplatedMapGenerator(cargoMapBuilder);

                PlaceOriginRoom(cargoTemplateGenerator, room1);
                PlaceRandomConnectedRooms(cargoTemplateGenerator, 4, room1, corridor1, 5, 10);

                //Add mass transit connection
                var cargoTransitConnection = AddRoomToRandomOpenDoor(cargoTemplateGenerator, replacementVault, corridor1, 3);

                //Add escape pods
                var escapePodsConnection = AddRoomToRandomOpenDoor(cargoTemplateGenerator, replacementVault, corridor1, 3);

                //Add a small number of place holder holder rooms for vaults
                var cargoPlaceholders = new List<Connection>();
                int maxPlaceHolders = 2;
                int cargoTotalPlaceHolders = 0;

                do
                {
                    var placeHolderRoom = AddRoomToRandomOpenDoor(cargoTemplateGenerator, placeHolderVault, corridor1, 3);
                    if (placeHolderRoom != null)
                    {
                        cargoPlaceholders.Add(placeHolderRoom);
                        cargoTotalPlaceHolders++;
                    }
                    else
                        break;
                } while (cargoTotalPlaceHolders < maxPlaceHolders);

                cargoTemplateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

                //Build bridge

                var bridgeMapBuilder = new TemplatedMapBuilder(100, 100);
                var bridgeTemplateGenerator = new TemplatedMapGenerator(bridgeMapBuilder, 100);

                PlaceOriginRoom(bridgeTemplateGenerator, room1);
                PlaceRandomConnectedRooms(bridgeTemplateGenerator, 4, room1, corridor1, 5, 10);

                //Add mass transit connection
                var bridgeTransitConnection = AddRoomToRandomOpenDoor(bridgeTemplateGenerator, replacementVault, corridor1, 3);

                //Add main bridge
                var bridgeMainBridgeConnection = AddRoomToRandomOpenDoor(bridgeTemplateGenerator, placeHolderVault, corridor1, 3);

                //Replace spare doors with walls
                bridgeTemplateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

                //Build the graph containing all the levels

                //Build and add the cargo map

                var mapInfoBuilder = new MapInfoBuilder();
                var startRoom = 0;
                mapInfoBuilder.AddConstructedLevel(0, cargoTemplateGenerator.ConnectivityMap, cargoTemplateGenerator.GetRoomTemplatesInWorldCoords(), cargoTemplateGenerator.GetDoorsInMapCoords(), startRoom);

                //Build and add the bridge map, with a connection to the cargo map

                mapInfoBuilder.AddConstructedLevel(1, bridgeTemplateGenerator.ConnectivityMap, bridgeTemplateGenerator.GetRoomTemplatesInWorldCoords(), bridgeTemplateGenerator.GetDoorsInMapCoords(),
                    new Connection(cargoTransitConnection.Target, bridgeTransitConnection.Target));

                mapInfo = new MapInfo(mapInfoBuilder);
                var mapHeuristics = new MapHeuristics(mapInfo.Model.GraphNoCycles, startRoom);

                //LOCKS

                //MAIN QUEST

                //Escape pod door
                //  - bridge self-destruct

                mapInfo.Model.DoorAndClueManager.PlaceDoorAndClue(new DoorRequirements(escapePodsConnection, "escape"), bridgeMainBridgeConnection.Target);

                //MAIN QUEST SUPPORT

                //Level-local lock on bridge level on critical path to main bridge. Place clue a reasonable distance away, not on critical path (if possible)

                var bridgeCriticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(bridgeTransitConnection.Target, bridgeMainBridgeConnection.Target);
                var bridgeCriticalConnection = bridgeCriticalPath.ElementAt(bridgeCriticalPath.Count() / 2);

                var allRoomsForCriticalClue = mapInfo.Model.DoorAndClueManager.GetValidRoomsToPlaceClue(bridgeCriticalConnection);
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

                //Add maps to the dungeon

                Map masterMap = cargoMapBuilder.MergeTemplatesIntoMap(terrainMapping);

                //Set player's start location (must be done before adding items)

                var firstRoom = mapInfo.GetRoom(0);
                masterMap.PCStartLocation = new Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);

                //Add terrain and randomize walls

                Dictionary<MapTerrain, List<MapTerrain>> brickTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.BrickWall1, MapTerrain.BrickWall1, MapTerrain.BrickWall1, MapTerrain.BrickWall2, MapTerrain.BrickWall3, MapTerrain.BrickWall4, MapTerrain.BrickWall5 } }};

                Map randomizedMapL1 = MapTerrainRandomizer.RandomizeTerrainInMap(masterMap, brickTerrainMapping);
                Game.Dungeon.AddMap(randomizedMapL1);

                Map masterMapL2 = bridgeMapBuilder.MergeTemplatesIntoMap(terrainMapping);
                Dictionary<MapTerrain, List<MapTerrain>> panelTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.PanelWall1, MapTerrain.PanelWall1, MapTerrain.PanelWall1, MapTerrain.PanelWall2, MapTerrain.PanelWall3, MapTerrain.PanelWall4, MapTerrain.PanelWall5 } }};
                Map randomizedMapL2 = MapTerrainRandomizer.RandomizeTerrainInMap(masterMapL2, panelTerrainMapping);
                Game.Dungeon.AddMap(randomizedMapL2);

                //Recalculate walkable to allow placing objects
                Game.Dungeon.RefreshAllLevelPathingAndFOV();

                //Add elevator features to link the maps

                //L1 -> L2
                var elevator1Loc = mapInfo.GetRandomPointInRoomOfTerrain(cargoTransitConnection.Target, RoomTemplateTerrain.Floor);
                var elevator2Loc = mapInfo.GetRandomPointInRoomOfTerrain(bridgeTransitConnection.Target, RoomTemplateTerrain.Floor);

                Game.Dungeon.AddFeature(new Features.Elevator(1, elevator2Loc), 0, elevator1Loc);
                Game.Dungeon.AddFeature(new Features.Elevator(0, elevator1Loc), 1, elevator2Loc);

                //Add non-interactable features
                var bridgeRoomOnMap = mapInfo.GetRoom(bridgeMainBridgeConnection.Target);
                var bridgeRouter = new RoomRouting(bridgeRoomOnMap.Room);

                int featuresToPlace = 5;
                for (int i = 0; i < featuresToPlace; i++)
                {
                    int randX = Game.Random.Next(bridgeRoomOnMap.Room.Width);
                    int randY = Game.Random.Next(bridgeRoomOnMap.Room.Height);

                    if (bridgeRouter.SetSquareUnwalkableIfDoorPathingIsPreserved(new Point(randX, randY)))
                    {
                        var featureLocationInMapCoords = new Point(bridgeRoomOnMap.Location.x + randX, bridgeRoomOnMap.Location.y + randY);
                        Game.Dungeon.AddFeatureBlocking(new Features.Corpse(), 1, featureLocationInMapCoords, true);

                        LogFile.Log.LogEntryDebug("Placing feature in room " + bridgeRoomOnMap.RoomIndex + " at location " + featureLocationInMapCoords, LogDebugLevel.Medium);
                    }
                }

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

                dungeonCreationSuccessful = true;

            } while (!dungeonCreationSuccessful);

            //Set map for visualisation
            return mapInfo;
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
            int attempts = 20;
            int thisAttempt = 0;

            do
            {
                try
                {
                    return gen.PlaceRoomTemplateAlignedWithExistingDoor(templateToPlace, corridorTemplate, RandomDoor(gen), 0, distanceFromDoor);
                }
                catch (ApplicationException)
                {
                    thisAttempt++;
                }
            } while (thisAttempt < attempts);

            return null;
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

        private bool PlaceOriginRoom(TemplatedMapGenerator templatedGenerator, RoomTemplate roomToPlace)
        {
            try
            {
                templatedGenerator.PlaceRoomTemplateAtPosition(roomToPlace, new Point(0, 0));
                return true;
            }
            catch (ApplicationException)
            {
                return false;
            }
        }

        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, RoomTemplate roomToPlace, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength)
        {
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, roomToPlace, corridorToPlace, minCorridorLength, maxCorridorLength, null);
        }

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

    public static class ShuffleExtension
    {

        public static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            return RandomElementUsing<T>(enumerable, Game.Random);
        }

        public static T RandomElementUsing<T>(this IEnumerable<T> enumerable, Random rand)
        {
            int index = rand.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }

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
