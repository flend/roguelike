using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumaRL
{
    public partial class TraumaWorldGenerator
    {

        //For development, skip making most of the levels
        bool quickLevelGen = false;

        MapState mapState;
        
        /** Build a map using templated rooms */
        public void GenerateTraumaLevels(bool retry)
        {
            //We catch exceptions on generation and keep looping

            //Pick main quests, registers required levels & difficulty relationship
            var mapQuestBuilder = new QuestMapBuilder();
            var logGen = new LogGenerator();
            var levelRegister = new LevelRegister();

            var questManager = new TraumaQuestManager(mapQuestBuilder, logGen, quickLevelGen);
            questManager.RegisterQuests(levelRegister);
            var startLevel = questManager.StartLevel.id;

            GraphVisualizer.VisualiseDirectedGraph(levelRegister.DifficultyGraph, "diff-graph");

            //Levels are now registered inside levelRegister and level ids are generated
            //These level ids are used inside the quests

            //Generate the overall level tree structure and difficulties
            var levelTreeBuilder = new LevelTreeBuilder(startLevel, levelRegister, quickLevelGen);
            var levelLinks = levelTreeBuilder.LevelLinks;
            var levelDifficulities = levelTreeBuilder.LevelDifficulties;
            
            //Build each level individually
            TraumaLevelBuilder levelBuilder = new TraumaLevelBuilder(levelRegister, levelLinks, startLevel, quickLevelGen);
            levelBuilder.GenerateLevels();
            
            //Initialise mapState that supports map mutations
            mapState = new MapState();
            SetupMapState(levelBuilder, levelTreeBuilder, startLevel);
            var levelInfo = levelBuilder.LevelInfo;

            GraphVisualizer.VisualiseLevelConnectivityGraph(levelLinks, mapState.LevelGraph.LevelNames);
            GraphVisualizer.VisualiseClueDoorGraph(mapState.MapInfo, mapState.DoorAndClueManager, "prequest");
            GraphVisualizer.VisualiseFullMapGraph(mapState.MapInfo, mapState.DoorAndClueManager, "prequest");

            //Generate quests (includes map mutations)
            questManager.GenerateQuests(mapState);

            GraphVisualizer.VisualiseFullMapGraph(mapState.MapInfo, mapState.DoorAndClueManager, "postquest");
            GraphVisualizer.VisualiseClueDoorGraph(mapState.MapInfo, mapState.DoorAndClueManager, "postquest");
        
            //Add non-interactable features
            var decorator = new DungeonDecorator(mapState, mapQuestBuilder);
            decorator.AddDecorationFeatures();

            //Add monsters (note - right now does not take into account where decorations are placed - fix)
            Game.Dungeon.MonsterPlacement.CreateMonstersForLevels(mapState, Game.Dungeon.Difficulty, mapState.LevelGraph.GameLevels, mapState.LevelGraph.LevelDifficulty);

            //Add elevator features to link the maps
            //Note that since the absolute-coords of level maps can now change due to the addition of new rooms (mutations)
            //It's only safe to add elevator features after all map changes have taken place
            //(absolute coords are necessary since the features need to know where to teleport the user to)
            if (!quickLevelGen)
                AddElevatorFeatures(mapState.MapInfo, levelInfo);

            //Add debug stuff in the first room
            AddDebugItems(mapState.MapInfo);

            //Close off any unused doors etc.
            levelBuilder.CompleteLevels();
            
            //Add maps to the dungeon (must be ordered)
            var dungeonMapSetup = new DungeonMapSetup();
            dungeonMapSetup.AddLevelMapsToDungeon(levelInfo);

            //Set player's start location
            dungeonMapSetup.SetPlayerStartLocation(mapState);

            //Set maps in engine (needs to be done before placing items and monsters)
            dungeonMapSetup.SetupMapsInEngine(mapState);

            //Add items/monsters/features from room model into dungeon
            dungeonMapSetup.AddMapObjectsToDungeon(mapState.MapInfo);
            
            dungeonMapSetup.AddMapStatePropertiesToDungeon(mapState);

            //Check we are solvable
            AssertMapIsSolveable(mapState.MapInfo, mapState.DoorAndClueManager);

            if (retry)
            {
                throw new ApplicationException("It happened!");
            }
        }

        private void SetupMapState(TraumaLevelBuilder levelBuilder, LevelTreeBuilder levelTreeBuilder, int startLevel)
        {
            mapState.BuildLevelMaps(levelTreeBuilder.LevelLinks, levelBuilder.LevelInfo, levelTreeBuilder.LevelDifficulties, startLevel);

            //Feels like there will be a more dynamic way of getting this state in future
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
                throw new ApplicationException("Feature is not connected to elevator, aborting.");
            }
        }
        
        private void AddDebugItems(MapInfo mapInfo)
        {
            
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
