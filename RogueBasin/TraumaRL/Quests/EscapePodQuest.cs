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

        public EscapePodQuest(MapState mapState, QuestMapBuilder builder, LogGenerator logGen)
            : base(mapState, builder, logGen)
        {
            //Escape pod (flight deck) [requires active self-destruct]
            //Activate self-destruct (bridge) [requires enable self-destruct]
            //Enable self-destruct (reactor) [requires computer cores destroyed]
            //Destroy computer cores (computer-core) [no pre-requisite]
            
        }

        public override void SetupQuest()
        {
            EscapePod();

            //Self destruct
            //Requires priming the reactor
            SelfDestruct();

            //Computer core to destroy
            ComputerCore();
        }

        public void EscapePod()
        {
            var escapePodConnection = MapState.ConnectionStore["escapePodConnection"];
            var escapePodRoom = escapePodConnection.Target;
            Builder.PlaceFeatureInRoom(MapState, new RogueBasin.Features.EscapePod(), new List<int>() { escapePodRoom }, true);

            LogFile.Log.LogEntryDebug("Adding features to escape pod room", LogDebugLevel.Medium);
            var escapePodDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.MedicalAutomat]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Pillar1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Pillar2])
            };
            Builder.AddStandardDecorativeFeaturesToRoom(MapState, escapePodRoom, 20, escapePodDecorations, false);

            //Escape pod door
            //Requires enabling self-destruct

            var colorForEscapePods = Builder.GetUnusedColor();
            var escapedoorName = "escape";
            var escapedoorId = escapedoorName;
            var escapedoorColor = colorForEscapePods.Item1;

            Builder.PlaceMovieDoorOnMap(MapState, escapedoorId, escapedoorName, 1, escapedoorColor, "escapepodunlocked", "escapepodlocked", escapePodConnection);
        }


        private void SelfDestruct()
        {
            var levelInfo = MapState.LevelInfo;
            var mapInfo = MapState.MapInfo;
            var manager = MapState.DoorAndClueManager;

            int selfDestructLevel = MapState.LevelIds["bridge"];
            var replaceableVaultsInBridge = levelInfo[selfDestructLevel].ReplaceableVaultConnections.Except(levelInfo[selfDestructLevel].ReplaceableVaultConnectionsUsed);
            var bridgeRoomsInDistanceOrderFromStart = mapInfo.RoomsInDescendingDistanceFromSource(levelInfo[selfDestructLevel].ConnectionsToOtherLevels.First().Value.Target, replaceableVaultsInBridge.Select(c => c.Target));
            var selfDestructRoom = bridgeRoomsInDistanceOrderFromStart.ElementAt(0);
            var selfDestructConnection = replaceableVaultsInBridge.Where(c => c.Target == selfDestructRoom).First();

            manager.PlaceObjective(new ObjectiveRequirements(selfDestructRoom, "self-destruct", 1, new List<string> { "escape" }));
            var selfDestructObjective = manager.GetObjectiveById("self-destruct");

            var bridgeLocation = Builder.PlaceObjective(MapState, selfDestructObjective, new RogueBasin.Features.SelfDestructObjective(selfDestructObjective, MapState.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(selfDestructObjective)), true, true, true);

            Builder.UseVault(MapState, selfDestructConnection);

            var bridgeDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer3]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Screen1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.HighTechBench])
            };
            Builder.AddStandardDecorativeFeaturesToRoom(MapState, selfDestructRoom, 20, bridgeDecorations, false);

            LogFile.Log.LogEntryDebug("Placing self-destruct on level " + selfDestructLevel + " in room " + selfDestructRoom + " off connection " + selfDestructConnection, LogDebugLevel.Medium);

            //Self destruct objective in reactor
            //Requires destruction of computer core
            var unusedVaultsInReactorLevel = Builder.GetAllAvailableVaults(MapState).Where(c => mapInfo.GetLevelForRoomIndex(c.Target) == MapState.LevelIds["reactor"]);
            var reactorSelfDestructVaultConnection = unusedVaultsInReactorLevel.First();
            var reactorSelfDestructVault = reactorSelfDestructVaultConnection.Target;
            Builder.UseVault(MapState, reactorSelfDestructVaultConnection);

            manager.PlaceObjective(new ObjectiveRequirements(reactorSelfDestructVault, "prime-self-destruct", computerCoresToDestroy, new List<string> { "self-destruct" }));
            var selfDestructPrimeObjective = manager.GetObjectiveById("prime-self-destruct");
            //PlaceObjective(mapInfo, selfDestructPrimeObjective, null, true, true);
            var reactorLocation = Builder.PlaceObjective(MapState, selfDestructPrimeObjective, new RogueBasin.Features.SelfDestructPrimeObjective(selfDestructPrimeObjective, MapState.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(selfDestructPrimeObjective)), true, true, true);

            var reactorDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument3])
            };
            Builder.AddStandardDecorativeFeaturesToRoom(MapState, reactorSelfDestructVault, 100, reactorDecorations, false);

        }

        private void ComputerCore()
        {
            var levelInfo = MapState.LevelInfo;
            var mapInfo = MapState.MapInfo;
            var manager = MapState.DoorAndClueManager;

            var computerCoreLevel = MapState.LevelIds["computerCore"];
            var primeSelfDestructId = "prime-self-destruct";
            var coresToPlace = 20;

            var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForObjective(primeSelfDestructId);
            allowedRoomsForClues = mapInfo.FilterOutCorridors(allowedRoomsForClues);
            var roomsOnComputerCoreLevel = allowedRoomsForClues.Intersect(mapInfo.GetRoomIndicesForLevel(computerCoreLevel));

            var roomsToPlaceMonsters = new List<int>();

            var roomsForMonsters = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(MapState, coresToPlace, roomsOnComputerCoreLevel);
            var clues = manager.AddCluesToExistingObjective(primeSelfDestructId, roomsForMonsters);

            Builder.PlaceCreatureClues<RogueBasin.Creatures.ComputerNode>(MapState, clues, true, false, true);

            //Place log entries explaining the puzzle
            //These will not be turned into in-engine clue items, so they can't be used to open the door
            //They are added though, to ensure that they are readable before the door is opened

            //CC is a dead end
            var sourceElevatorConnection = levelInfo[computerCoreLevel].ConnectionsToOtherLevels.First();
            var connectingLevel = sourceElevatorConnection.Key;
            var elevatorToCC = levelInfo[connectingLevel].ConnectionsToOtherLevels[computerCoreLevel];

            var allowedRoomsForLogs = manager.GetValidRoomsToPlaceClueForObjective(primeSelfDestructId);

            var arcologyLevel = MapState.LevelIds["arcology"];
            var commercialLevel = MapState.LevelIds["commercial"];
            var preferredLevelsForLogs = new List<int> { arcologyLevel, commercialLevel };
            var preferredRooms = preferredLevelsForLogs.SelectMany(l => mapInfo.GetRoomIndicesForLevel(l));

            var preferredRoomsForLogs = allowedRoomsForLogs.Intersect(preferredRooms);

            var criticalPathLog = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, elevatorToCC.Source);

            var preferredRoomsForLogsCritical = Builder.FilterRoomsByPath(MapState, preferredRoomsForLogs, criticalPathLog, false, QuestMapBuilder.CluePath.OnCriticalPath, true);
            var preferredRoomsForLogsNonCritical = Builder.FilterRoomsByPath(MapState, preferredRoomsForLogs, criticalPathLog, false, QuestMapBuilder.CluePath.Any, true);

            var roomsForLogsCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(MapState, 2, preferredRoomsForLogsCritical);
            var roomsForLogsNonCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(MapState, 2, preferredRoomsForLogsNonCritical);

            var logCluesCritical = manager.AddCluesToExistingObjective(primeSelfDestructId, roomsForLogsCritical);
            var logCluesNonCritical = manager.AddCluesToExistingObjective(primeSelfDestructId, roomsForLogsNonCritical);

            var log2 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_computer1", connectingLevel, computerCoreLevel), logCluesCritical[0]);
            var log3 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_computer2", connectingLevel, computerCoreLevel), logCluesCritical[1]);

            var log1 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_computer3", connectingLevel, computerCoreLevel), logCluesNonCritical[0]);
            var log4 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_computer4", connectingLevel, computerCoreLevel), logCluesNonCritical[1]);

            Builder.PlaceLogClues(MapState, new List<Tuple<LogEntry, Clue>> { log1, log2, log3, log4 }, true, true);
        }
    }
}
