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

            SetupMapState(levelBuilder, levelInfo);
            MapInfo mapInfo = mapState.MapInfo;
            
            //Add elevator features to link the maps
            if (!quickLevelGen)
                AddElevatorFeatures(mapInfo, levelInfo);

            //Generate quests
            var mapQuestBuilder = new QuestMapBuilder();
            GenerateQuests(mapState, mapQuestBuilder);
           
            //Add non-interactable features
            var decorator = new DungeonDecorator(mapState, mapQuestBuilder);
            decorator.AddDecorationFeatures();

            //Add debug stuff in the first room
            AddDebugItems(mapInfo);

            //Close off any unused doors etc.
            levelBuilder.CompleteLevels();
            
            //Add maps to the dungeon (must be ordered)
            AddLevelMapsToDungeon(levelInfo);

            //Set player's start location (must be done before adding items)
            SetPlayerStartLocation(mapInfo);

            //Set maps in engine (needs to be done before placing items and monsters)
            SetupMapsInEngine();

            //Quests is being refactored to store information in MapInfo, rather than in the Dungeon
            //Need to add here the code which transfers the completed MapInfo creatures, features, items and locks into the Dungeon
            Game.Dungeon.AddMapObjectsToDungeon(mapInfo);
            
            //Add monsters
            Game.Dungeon.MonsterPlacement.CreateMonstersForLevels(mapState, mapState.GameLevels, mapState.LevelDifficulty);

            //Check we are solvable
            AssertMapIsSolveable(mapInfo, mapState.DoorAndClueManager);

            if (retry)
            {
                throw new ApplicationException("It happened!");
            }
        }

        private void SetupMapState(TraumaLevelBuilder levelBuilder, Dictionary<int, LevelInfo> levelInfo)
        {
            var startVertex = 0;
            var startLevel = 0;
            mapState = new MapState();
            mapState.UpdateWithNewLevelMaps(levelLinks, levelInfo, startLevel);
            mapState.InitialiseDoorAndClueManager(startVertex);

            //Feels like there will be a more dynamic way of getting this state in future
            mapState.ConnectionStore["escapePodConnection"] = levelBuilder.EscapePodsConnection;
            mapState.AllReplaceableVaults = levelBuilder.AllReplaceableVaults;

            CalculateLevelDifficulty(mapState);
        }

        private void UpdateMapStateAfterAddition(TraumaLevelBuilder levelBuilder, Dictionary<int, LevelInfo> levelInfo)
        {
            var startLevel = 0;
            mapState.UpdateWithNewLevelMaps(levelLinks, levelInfo, startLevel);

            mapState.AllReplaceableVaults = levelBuilder.AllReplaceableVaults;
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

            BuildMapExpandQuest(mapState, questMapBuilder);

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

        private void BuildMapExpandQuest(MapState mapState, QuestMapBuilder questMapBuilder)
        {
            //Try to add a new room the medical level
            //var mapExpandQuest = new Quests.MapExpandQuest(mapState, questMapBuilder, logGen);
            //mapExpandQuest.SetupQuest();

            //Requires a map update
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
                    var blockElevatorQuest = new Quests.BlockElevatorQuest(mapState, builder, logGen, level, roomConnectivityMap);
                    blockElevatorQuest.ClueOnElevatorLevel = Game.Random.Next(2) > 0;
                    blockElevatorQuest.SetupQuest();
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
                var blockElevatorQuest = new Quests.BlockElevatorQuest(mapState, builder, logGen, lowerAtriumLevel, roomConnectivityMap);
                blockElevatorQuest.SetupQuest();
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntryDebug("Atrium Elevator Exception: " + ex, LogDebugLevel.High);
            }
        }


        private void BuildGoodyQuests(MapState mapState, QuestMapBuilder builder, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            var armoryQuest = new Quests.ArmoryQuest(mapState, builder, logGen);
            armoryQuest.SetupQuest();
        }
        
        private void BuildMedicalLevelQuests(MapState mapState, QuestMapBuilder builder)
        {
            var cameraQuest = new Quests.MedicalCameraQuest(mapState, builder, logGen);
            cameraQuest.SetupQuest();
        }

        private void BuildMainQuest(MapState mapState, QuestMapBuilder questMapBuilder)
        {
            var escapePod = new Quests.MainQuest(mapState, questMapBuilder, logGen);
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

        private void CalculateLevelDifficulty(MapState mapState)
        {
            var levelsToHandleSeparately = new List<int> { medicalLevel, arcologyLevel, computerCoreLevel, bridgeLevel };

            var levelDifficulty = mapState.LevelDifficulty;

            foreach (var kv in mapState.LevelDepths)
            {
                levelDifficulty[kv.Key] = kv.Value;
            }

            levelDifficulty[reactorLevel] = 4;
            levelDifficulty[arcologyLevel] = 4;
            levelDifficulty[computerCoreLevel] = 5;
            levelDifficulty[bridgeLevel] = 5;
        }
    }
}
