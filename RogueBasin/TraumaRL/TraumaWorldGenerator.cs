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

        private class LevelAndDifficulty
        {
            public readonly int level;
            public readonly int difficulty;

            public LevelAndDifficulty(int level, int difficulty)
            {
                this.level = level;
                this.difficulty = difficulty;
            }

            public static bool operator ==(LevelAndDifficulty i, LevelAndDifficulty j)
            {
                // If both are null, or both are same instance, return true.
                if (System.Object.ReferenceEquals(i, j))
                {
                    return true;
                }

                // If one is null, but not both, return false.
                if (((object)i == null) || ((object)j == null))
                {
                    return false;
                }

                // Return true if the fields match:
                if (i.level == j.level && i.difficulty == j.difficulty)
                    return true;
                return false;
            }

            public static bool operator !=(LevelAndDifficulty i, LevelAndDifficulty j)
            {
                return !(i == j);
            }
            
            public override bool Equals(object obj)
            {
                //Value-wise comparison ensured by the cast
                return this == (LevelAndDifficulty)obj;
            }

            public override int GetHashCode()
            {
                return level + 17 * difficulty;
            }
        }

        /// <summary>
        /// Build a level->level map showing how the levels are connected
        /// </summary>
        private ConnectivityMap GenerateLevelLinks()
        {
            var levelLinks = new ConnectivityMap();

            //Order of the main quest (in future, this will be generic)

            //Escape pod (flight deck) [requires active self-destruct]
            //Activate self-destruct (bridge) [requires enable self-destruct]
            //Enable self-destruct (reactor) [requires computer cores destroyed]
            //Destroy computer cores (computer-core) [no pre-requisite]
            //Bridge lock (any level place captain's cabin) [no pre-requisite]
            //Computer core lock (arcology) [no pre-requisite]
            //Arcology lock (any level - place bioware) [no pre-requisite]
            //Arcology lock (any level) [antennae disabled]
            //Antennae (science / storage) [no pre-requisite]
            
            //Lower Atrium (may be out-of-sequence)
            //Medical

            //Level order (last to first)

            //flight deck
            //bridge
            //reactor
            //computer-core
            //arcology
            //science
            //storage

            //lower atrium
            //medical

            //non-difficulty sequenced:

            //commercial

            //Player starts in medical which links to the lower atrium
            levelLinks.AddRoomConnection(new Connection(medicalLevel, lowerAtriumLevel));

            if (!quickLevelGen)
            {
                //Create levels in order of difficulty
                var levelsAndDifficulties = new List<LevelAndDifficulty> {
                    new LevelAndDifficulty(flightDeck, 1),
                    new LevelAndDifficulty(bridgeLevel, 2),
                    new LevelAndDifficulty(reactorLevel, 3),
                    new LevelAndDifficulty(computerCoreLevel, 4),
                    new LevelAndDifficulty(arcologyLevel, 5),
                    new LevelAndDifficulty(scienceLevel, 6),
                    new LevelAndDifficulty(storageLevel, 7),
                    new LevelAndDifficulty(commercialLevel, 6),
                    new LevelAndDifficulty(lowerAtriumLevel, 8)
                };

                //Pick terminuses (all levels except most difficult and lower atrium)
                //Note that the toList() is essential here, otherwise the list keeps getting lazily reshuffled which breaks the algorithm
                var terminusShuffle = levelsAndDifficulties.Skip(1).Take(7).Shuffle().ToList();

                var numberOfTerminii = Game.Random.Next(2) + 2;
                var terminusNodes = terminusShuffle.Take(numberOfTerminii);

                //Add most difficult level as terminus
                terminusNodes = terminusNodes.Union(Enumerable.Repeat(levelsAndDifficulties.ElementAt(0), 1));

                var remainingNodes = levelsAndDifficulties.Except(terminusNodes).Except(Enumerable.Repeat(new LevelAndDifficulty(lowerAtriumLevel, 8), 1));

                foreach (var level in remainingNodes)
                {
                    //Pick a parent from current terminusNodes, which is less difficult
                    var parentLevel = terminusNodes.Where(parent => parent.difficulty < level.difficulty).Shuffle().First();
                    levelLinks.AddRoomConnection(new Connection(level.level, parentLevel.level));

                    //Remove parent from terminii and add this level
                    var terminusNodesExceptParent = terminusNodes.Except(Enumerable.Repeat(parentLevel, 1));
                    terminusNodes = terminusNodesExceptParent.Union(Enumerable.Repeat(level, 1));
                }

                //Connect all terminii to lower atrium
                foreach (var level in terminusNodes)
                {
                    levelLinks.AddRoomConnection(new Connection(lowerAtriumLevel, level.level));
                }

                //TODO: try to balance the tree a bit, otherwise pathological situations (one long branch) are quite likely
            }

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
            
            //Add elevator features to link the maps
            if (!quickLevelGen)
                AddElevatorFeatures(MapState.MapInfo, levelInfo);

            //Generate quests
            var mapQuestBuilder = new QuestMapBuilder();
            GenerateQuests(mapState, mapQuestBuilder);
           
            //Add non-interactable features
            var decorator = new DungeonDecorator(mapState, mapQuestBuilder);
            decorator.AddDecorationFeatures();

            //Add debug stuff in the first room
            AddDebugItems(MapState.MapInfo);

            //Close off any unused doors etc.
            levelBuilder.CompleteLevels();
            
            //Add maps to the dungeon (must be ordered)
            AddLevelMapsToDungeon(levelInfo);

            //Set player's start location (must be done before adding items)
            SetPlayerStartLocation(MapState.MapInfo);

            //Set maps in engine (needs to be done before placing items and monsters)
            SetupMapsInEngine();

            //Pause here to attach the debugger
            //MessageBox.Show("post engine");

            Game.Dungeon.AddMapObjectsToDungeon(MapState.MapInfo);
            
            //Add monsters
            Game.Dungeon.MonsterPlacement.CreateMonstersForLevels(mapState, mapState.GameLevels, mapState.LevelDifficulty);

            //Check we are solvable
            AssertMapIsSolveable(MapState.MapInfo, mapState.DoorAndClueManager);

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
            mapState.BuildLevelMaps(levelLinks, levelInfo, startLevel, startVertex);

            //Feels like there will be a more dynamic way of getting this state in future
            mapState.ConnectionStore["escapePodConnection"] = levelBuilder.EscapePodsConnection;
            mapState.AllReplaceableVaults = levelBuilder.AllReplaceableVaults;

            CalculateLevelDifficulty(mapState);
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

            //Example has been surpassed by the MedicalTurretTrapQuest
            //BuildMapExpandQuest(mapState, questMapBuilder, medicalLevel);

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

        private void BuildMapExpandQuest(MapState mapState, QuestMapBuilder questMapBuilder, int level)
        {
            var mapExpandQuest = new Quests.MapExpandQuest(mapState, questMapBuilder, logGen, level);
            mapExpandQuest.SetupQuest();
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
            //var cameraQuest = new Quests.MedicalCameraQuest(mapState, builder, logGen);
            var cameraQuest = new Quests.MedicalTurretTrapQuest(mapState, builder, logGen);
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
