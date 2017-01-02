﻿using GraphMap;
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
        public MapState MapState { get { return mapState; } }

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
        
        /** Build a map using templated rooms */
        public void GenerateTraumaLevels(bool retry)
        {
            //We catch exceptions on generation and keep looping

            //Pick main quests

            //Generate levels needed in the game from quests
            //Add any filler levels
            
            //List of (level id [arbitrary], "level type")
            //Graph of difficulties

            //Generate the overall level tree structure
            var levelTreeBuilder = new LevelTreeBuilder(quickLevelGen);
            levelLinks = levelTreeBuilder.GenerateLevelLinks();
            var gameLevels = levelLinks.GetAllConnections().SelectMany(c => new List<int> { c.Source, c.Target }).Distinct().OrderBy(c => c).ToList();

            //Build each level individually
            TraumaLevelBuilder levelBuilder = new TraumaLevelBuilder(gameLevels, levelLinks, quickLevelGen);
            levelBuilder.GenerateLevels();
            
            //Initialise mapState that supports map mutations
            mapState = new MapState();
            Dictionary<int, LevelInfo> levelInfo = levelBuilder.LevelInfo;
            SetupMapState(levelBuilder, levelInfo);

            //Generate quests (includes map mutations)
            var mapQuestBuilder = new QuestMapBuilder();
            GenerateQuests(mapState, mapQuestBuilder);
           
            //Add non-interactable features
            var decorator = new DungeonDecorator(mapState, mapQuestBuilder);
            decorator.AddDecorationFeatures();

            //Add elevator features to link the maps
            //Note that since the absolute-coords of level maps can now change due to the addition of new rooms (mutations)
            //It's only safe to add elevator features after all map changes have taken place
            if (!quickLevelGen)
                AddElevatorFeatures(MapState.MapInfo, levelInfo);

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
            Game.Dungeon.MonsterPlacement.CreateMonstersForLevels(mapState, mapState.LevelGraph.GameLevels, mapState.LevelGraph.LevelDifficulty);

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
            mapState.BuildLevelMaps(levelLinks, levelInfo, startLevel, startVertex);

            //Feels like there will be a more dynamic way of getting this state in future
            mapState.ConnectionStore["escapePodConnection"] = levelBuilder.EscapePodsConnection;
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

            foreach (var level in mapState.LevelGraph.GameLevels)
            {
                Game.Dungeon.Levels[level].LightLevel = 0;
            }
        }


        private void GenerateQuests(MapState mapState, QuestMapBuilder questMapBuilder)
        {
            var mapInfo = mapState.MapInfo;
            var levelInfo = mapState.LevelGraph.LevelInfo;

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
            var mapExpandQuest = new Quests.MapExpandQuest(questMapBuilder, logGen, level);
            mapExpandQuest.SetupQuest(mapState);
        }

        private void BuildRandomElevatorQuests(MapState mapState, QuestMapBuilder builder, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            var noLevelsToBlock = 1 + Game.Random.Next(1);

            var lowerAtriumLevel = mapState.LevelGraph.LevelIds["lowerAtrium"];
            var medicalLevel = mapState.LevelGraph.LevelIds["medical"];

            var candidateLevels = mapState.LevelGraph.GameLevels.Except(new List<int> { lowerAtriumLevel, medicalLevel }).Where(l => mapState.LevelGraph.LevelInfo[l].ConnectionsToOtherLevels.Count() > 1);
            LogFile.Log.LogEntryDebug("Candidates for elevator quests: " + candidateLevels, LogDebugLevel.Medium);
            var chosenLevels = candidateLevels.RandomElements(noLevelsToBlock);

            foreach (var level in chosenLevels)
            {
                try
                {
                    var blockElevatorQuest = new Quests.BlockElevatorQuest(builder, logGen, level, roomConnectivityMap);
                    blockElevatorQuest.ClueOnElevatorLevel = Game.Random.Next(2) > 0;
                    blockElevatorQuest.SetupQuest(mapState);
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
                var lowerAtriumLevel = mapState.LevelGraph.LevelIds["lowerAtrium"];
                var blockElevatorQuest = new Quests.BlockElevatorQuest(builder, logGen, lowerAtriumLevel, roomConnectivityMap);
                blockElevatorQuest.SetupQuest(mapState);
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntryDebug("Atrium Elevator Exception: " + ex, LogDebugLevel.High);
            }
        }


        private void BuildGoodyQuests(MapState mapState, QuestMapBuilder builder, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            var armoryQuest = new Quests.ArmoryQuest(builder, logGen);
            armoryQuest.SetupQuest(mapState);
        }
        
        private void BuildMedicalLevelQuests(MapState mapState, QuestMapBuilder builder)
        {
            //var cameraQuest = new Quests.MedicalCameraQuest(mapState, builder, logGen);
            var cameraQuest = new Quests.MedicalTurretTrapQuest(builder, logGen);
            cameraQuest.SetupQuest(mapState);
        }

        private void BuildMainQuest(MapState mapState, QuestMapBuilder questMapBuilder)
        {
            var escapePod = new Quests.EscapePodQuest(questMapBuilder, logGen);
            escapePod.SetupQuest(mapState);
    
            var mainQuest = new Quests.CaptainIdQuest(questMapBuilder, logGen);
            mainQuest.SetupQuest(mapState);

            var arcologyQuest = new Quests.ArcologyQuest(questMapBuilder, logGen);
            arcologyQuest.SetupQuest(mapState);
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
                var elevatorRoomRelativePoint = sourceToTargetElevatorPoint - mapInfo.Room(sourceToTargetElevatorRoomId).Location;

                mapInfo.Populator.AddFeatureToRoom(mapInfo, sourceToTargetElevatorRoomId, elevatorRoomRelativePoint, elevatorFeature);
                
                LogFile.Log.LogEntryDebug("Adding elevator connection " + sourceLevel + ":" + targetLevel + " via points" +
                    sourceToTargetElevator + "->" + targetToSourceElevator, LogDebugLevel.Medium);
            }
        }
    }
}
