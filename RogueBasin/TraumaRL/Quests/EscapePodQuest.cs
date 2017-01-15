using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL.Quests
{
    /// <summary>
    /// This quest is self-contained, in that it generates all the clues required to complete the quest
    /// </summary>
    class EscapePodQuest : Quest
    {
        private const int computerCoresToDestroy = 15;

        //Quest-critical levels
        LevelIdData computerCoreLevelData = null;
        LevelIdData bridgeLevelData = null;
        LevelIdData flightDeckLevelData = null;
        LevelIdData reactorLevelData = null;

        //Mostly used as filler
        LevelIdData commercialLevelData = null;
        LevelIdData arcologyLevelData = null;

        public EscapePodQuest(QuestMapBuilder builder, LogGenerator logGen)
            : base(builder, logGen)
        {
            //Escape pod (flight deck) [requires active self-destruct]
            //Activate self-destruct (bridge) [requires enable self-destruct]
            //Enable self-destruct (reactor) [requires computer cores destroyed]
            //Destroy computer cores (computer-core) [no pre-requisite]
            
        }

        public override void SetupQuest(MapState mapState)
        {
            EscapePod(mapState);

            //Self destruct
            //Requires priming the reactor
            SelfDestruct(mapState);

            //Computer core to destroy
            ComputerCore(mapState);
        }

        public void EscapePod(MapState mapState)
        {
            var escapePodConnection = mapState.ConnectionStore["escapePodConnection"];
            var escapePodRoom = escapePodConnection.Target;
            Builder.PlaceFeatureInRoom(mapState, new RogueBasin.Features.EscapePod(), new List<int>() { escapePodRoom }, true);

            LogFile.Log.LogEntryDebug("Adding features to escape pod room", LogDebugLevel.Medium);
            var escapePodDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.MedicalAutomat]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Pillar1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Pillar2])
            };
            Builder.AddStandardDecorativeFeaturesToRoom(mapState, escapePodRoom, 20, escapePodDecorations, false);

            //Escape pod door
            //Requires enabling self-destruct

            var colorForEscapePods = Builder.GetUnusedColor();
            var escapedoorName = "escape";
            var escapedoorId = escapedoorName;
            var escapedoorColor = colorForEscapePods.Item1;

            Builder.PlaceMovieDoorOnMap(mapState, escapedoorId, escapedoorName, 1, escapedoorColor, "escapepodunlocked", "escapepodlocked", escapePodConnection);
        }


        private void SelfDestruct(MapState mapState)
        {
            var levelInfo = mapState.LevelGraph.LevelInfo;
            var mapInfo = mapState.MapInfo;
            var manager = mapState.DoorAndClueManager;

            int selfDestructLevel = mapState.LevelGraph.LevelIds["bridge"];
            var replaceableVaultsInBridge = levelInfo[selfDestructLevel].ReplaceableVaultConnections.Except(levelInfo[selfDestructLevel].ReplaceableVaultConnectionsUsed);
            var bridgeRoomsInDistanceOrderFromStart = mapInfo.RoomsInDescendingDistanceFromSource(levelInfo[selfDestructLevel].ConnectionsToOtherLevels.First().Value.Target, replaceableVaultsInBridge.Select(c => c.Target));
            var selfDestructRoom = bridgeRoomsInDistanceOrderFromStart.ElementAt(0);
            var selfDestructConnection = replaceableVaultsInBridge.Where(c => c.Target == selfDestructRoom).First();

            manager.PlaceObjective(new ObjectiveRequirements(selfDestructRoom, "self-destruct", 1, new List<string> { "escape" }));
            var selfDestructObjective = manager.GetObjectiveById("self-destruct");

            var bridgeLocation = Builder.PlaceObjective(mapState, selfDestructObjective, new RogueBasin.Features.SelfDestructObjective(selfDestructObjective, mapState.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(selfDestructObjective)), true, true, true);

            Builder.UseVault(mapState, selfDestructConnection);

            var bridgeDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer3]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Screen1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.HighTechBench])
            };
            Builder.AddStandardDecorativeFeaturesToRoom(mapState, selfDestructRoom, 20, bridgeDecorations, false);

            LogFile.Log.LogEntryDebug("Placing self-destruct on level " + selfDestructLevel + " in room " + selfDestructRoom + " off connection " + selfDestructConnection, LogDebugLevel.Medium);

            //Self destruct objective in reactor
            //Requires destruction of computer core
            var unusedVaultsInReactorLevel = Builder.GetAllAvailableVaults(mapState).Where(c => mapInfo.GetLevelForRoomIndex(c.Target) == mapState.LevelGraph.LevelIds["reactor"]);
            var reactorSelfDestructVaultConnection = unusedVaultsInReactorLevel.First();
            var reactorSelfDestructVault = reactorSelfDestructVaultConnection.Target;
            Builder.UseVault(mapState, reactorSelfDestructVaultConnection);

            manager.PlaceObjective(new ObjectiveRequirements(reactorSelfDestructVault, "prime-self-destruct", computerCoresToDestroy, new List<string> { "self-destruct" }));
            var selfDestructPrimeObjective = manager.GetObjectiveById("prime-self-destruct");
            //PlaceObjective(mapInfo, selfDestructPrimeObjective, null, true, true);
            var reactorLocation = Builder.PlaceObjective(mapState, selfDestructPrimeObjective, new RogueBasin.Features.SelfDestructPrimeObjective(selfDestructPrimeObjective, mapState.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(selfDestructPrimeObjective)), true, true, true);

            var reactorDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument3])
            };
            Builder.AddStandardDecorativeFeaturesToRoom(mapState, reactorSelfDestructVault, 100, reactorDecorations, false);

        }

        private void ComputerCore(MapState mapState)
        {
            var levelInfo = mapState.LevelGraph.LevelInfo;
            var mapInfo = mapState.MapInfo;
            var manager = mapState.DoorAndClueManager;

            var computerCoreLevel = mapState.LevelGraph.LevelIds["computerCore"];
            var primeSelfDestructId = "prime-self-destruct";
            var coresToPlace = 20;

            var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForObjective(primeSelfDestructId);
            allowedRoomsForClues = mapInfo.FilterOutCorridors(allowedRoomsForClues);
            var roomsOnComputerCoreLevel = allowedRoomsForClues.Intersect(mapInfo.GetRoomIndicesForLevel(computerCoreLevel));

            var roomsToPlaceMonsters = new List<int>();

            var roomsForMonsters = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, coresToPlace, roomsOnComputerCoreLevel);
            var clues = manager.AddCluesToExistingObjective(primeSelfDestructId, roomsForMonsters);

            Builder.PlaceCreatureClues<RogueBasin.Creatures.ComputerNode>(mapState, clues, true, false, true);

            //Place log entries explaining the puzzle
            //These will not be turned into in-engine clue items, so they can't be used to open the door
            //They are added though, to ensure that they are readable before the door is opened

            //CC is a dead end
            var sourceElevatorConnection = levelInfo[computerCoreLevel].ConnectionsToOtherLevels.First();
            var connectingLevel = sourceElevatorConnection.Key;
            var elevatorToCC = levelInfo[connectingLevel].ConnectionsToOtherLevels[computerCoreLevel];

            var allowedRoomsForLogs = manager.GetValidRoomsToPlaceClueForObjective(primeSelfDestructId);

            var arcologyLevel = mapState.LevelGraph.LevelIds["arcology"];
            var commercialLevel = mapState.LevelGraph.LevelIds["commercial"];
            var preferredLevelsForLogs = new List<int> { arcologyLevel, commercialLevel };
            var preferredRooms = preferredLevelsForLogs.SelectMany(l => mapInfo.GetRoomIndicesForLevel(l));

            var preferredRoomsForLogs = allowedRoomsForLogs.Intersect(preferredRooms);

            var criticalPathLog = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, elevatorToCC.Source);

            var preferredRoomsForLogsCritical = Builder.FilterRoomsByPath(mapState, preferredRoomsForLogs, criticalPathLog, false, QuestMapBuilder.CluePath.OnCriticalPath, true);
            var preferredRoomsForLogsNonCritical = Builder.FilterRoomsByPath(mapState, preferredRoomsForLogs, criticalPathLog, false, QuestMapBuilder.CluePath.Any, true);

            var roomsForLogsCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, 2, preferredRoomsForLogsCritical);
            var roomsForLogsNonCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, 2, preferredRoomsForLogsNonCritical);

            var logCluesCritical = manager.AddCluesToExistingObjective(primeSelfDestructId, roomsForLogsCritical);
            var logCluesNonCritical = manager.AddCluesToExistingObjective(primeSelfDestructId, roomsForLogsNonCritical);

            var log2 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_computer1", connectingLevel, computerCoreLevel), logCluesCritical[0]);
            var log3 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_computer2", connectingLevel, computerCoreLevel), logCluesCritical[1]);

            var log1 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_computer3", connectingLevel, computerCoreLevel), logCluesNonCritical[0]);
            var log4 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_computer4", connectingLevel, computerCoreLevel), logCluesNonCritical[1]);

            Builder.PlaceLogClues(mapState, new List<Tuple<LogEntry, Clue>> { log1, log2, log3, log4 }, true, true);
        }

        public override void RegisterLevels(LevelRegister register)
        {
            computerCoreLevelData = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.ComputerCoreLevel));
            arcologyLevelData = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.ArcologyLevel));
            bridgeLevelData = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.BridgeLevel));
            flightDeckLevelData = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.FlightDeck));
            reactorLevelData = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.ReactorLevel));

            commercialLevelData = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.CommercialLevel));
            arcologyLevelData = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.ArcologyLevel));

            register.RegisterAscendingDifficultyRelationship(bridgeLevelData.id, flightDeckLevelData.id);
            
            register.RegisterAscendingDifficultyRelationship(reactorLevelData.id, bridgeLevelData.id);
            register.RegisterAscendingDifficultyRelationship(computerCoreLevelData.id, reactorLevelData.id);

            register.RegisterAscendingDifficultyRelationship(commercialLevelData.id, computerCoreLevelData.id);
            register.RegisterAscendingDifficultyRelationship(arcologyLevelData.id, computerCoreLevelData.id);
        }
    }
}
