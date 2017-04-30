using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumaRL.Quests
{
    class CaptainIdQuest : Quest
    {
        LevelIdData bridgeLevelData = null;

        public CaptainIdQuest(QuestMapBuilder builder, LogGenerator logGen) : base(builder, logGen)
        {
            //Order of the main quest (in future, this will be generic)

            //ESCAPE POD QUEST
            //Escape pod (flight deck) [requires active self-destruct]
            //Activate self-destruct (bridge) [requires enable self-destruct]
            //Enable self-destruct (reactor) [requires computer cores destroyed]
            //Destroy computer cores (computer-core) [no pre-requisite]
            
            //Bridge lock (any level place captain's cabin) [no pre-requisite]
            //Computer core lock (arcology) [no pre-requisite]
            //Arcology lock (any level - place bioware) [no pre-requisite]
            //Arcology lock (any level) [antennae disabled]
            //Antennae (science / storage) [no pre-requisite]

            //To chain quests
            //Figure out the list of quests, from last to first
            //From first to last, pass on the result-clues to the next quest
            //(e.g. door key, 20 monsters killed clues) + possibly some meta-data to inform the logs
            //Then create quests from last to first, referring to the inherited clues

            //[or never write quests with pre-requisites! lock doors to levels with a complete quest as a single quest object]
        }

        public override void SetupQuest(MapState mapState)
        {
            //Bridge lock
            //Requires captain's id

            BridgeLock(mapState);
        }

        private void BridgeLock(MapState mapState)
        {
            var bridgeLevel = bridgeLevelData.id;

            //This should be replaced by a query solely based on difficulty / start levels
            //rather than hard-coding levels we know to be easier
            var lowerAtriumLevel = mapState.LevelGraph.LevelIds["lowerAtrium"];
            var medicalLevel = mapState.LevelGraph.LevelIds["medical"];
            var storageLevel = mapState.LevelGraph.LevelIds["storage"];
            var scienceLevel = mapState.LevelGraph.LevelIds["science"];

            var levelInfo = mapState.LevelGraph.LevelInfo;
            var mapInfo = mapState.MapInfo;

            //bridge is a dead end
            var sourceElevatorConnection = levelInfo[bridgeLevel].ConnectionsToOtherLevels.First();
            var connectingLevel = sourceElevatorConnection.Key;
            var elevatorToBridge = levelInfo[connectingLevel].ConnectionsToOtherLevels[bridgeLevel];

            var doorName = "captain's id bridge";
            var doorId = doorName;
            var colorForCaptainId = Builder.GetUnusedColor();
            var doorColor = colorForCaptainId.Item1;

            Builder.PlaceMovieDoorOnMap(mapState, doorId, doorName, 1, doorColor, "bridgelocked", "bridgeunlocked", elevatorToBridge);

            //Captain's id
            var captainIdIdealLevel = mapState.LevelGraph.LevelDepths.Where(kv => kv.Value >= 1).Select(kv => kv.Key).Except(new List<int> { lowerAtriumLevel, medicalLevel, storageLevel, scienceLevel });
            var captainsIdRoom = Builder.PlaceClueForDoorInVault(mapState, levelInfo, doorId, doorColor, doorName, captainIdIdealLevel);

            //Add monsters - nice to put ID on captain but not for now
            var captainsIdLevel = mapInfo.GetLevelForRoomIndex(captainsIdRoom);
            var monstersToPlace = new List<Monster> { new RogueBasin.Creatures.HeavyTurret(), new RogueBasin.Creatures.HeavyTurret(), new RogueBasin.Creatures.AssaultCyborgRanged(), new RogueBasin.Creatures.Captain() };
            Builder.PlaceCreaturesInRoom(mapState, captainsIdLevel, captainsIdRoom, monstersToPlace, false);

            var decorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Skeleton]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Plant2]),
                new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Plant3])};
            Builder.AddStandardDecorativeFeaturesToRoom(mapState, captainsIdRoom, 10, decorations, false);

            //Logs

            var manager = mapState.DoorAndClueManager;

            var allowedRoomsForLogs = manager.GetValidRoomsToPlaceClueForDoor(doorId);

            var criticalPathLog = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, elevatorToBridge.Source);

            var preferredRoomsForLogsCritical = Builder.FilterRoomsByPath(mapState, allowedRoomsForLogs, criticalPathLog, false, QuestMapBuilder.CluePath.OnCriticalPath, true);
            var preferredRoomsForLogsNonCritical = Builder.FilterRoomsByPath(mapState, allowedRoomsForLogs, criticalPathLog, false, QuestMapBuilder.CluePath.Any, true);

            var roomsForLogsCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, 2, preferredRoomsForLogsCritical);
            var roomsForLogsNonCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, 2, preferredRoomsForLogsNonCritical);

            var logCluesCritical = manager.AddCluesToExistingDoor(doorId, roomsForLogsCritical);
            var logCluesNonCritical = manager.AddCluesToExistingDoor(doorId, roomsForLogsNonCritical);

            var log2 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_captain1", connectingLevel, captainsIdLevel), logCluesCritical[0]);
            var log3 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_captain2", connectingLevel, captainsIdLevel), logCluesCritical[1]);

            var log1 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_captain3", connectingLevel, captainsIdLevel), logCluesNonCritical[0]);
            var log4 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_captain4", connectingLevel, captainsIdLevel), logCluesNonCritical[1]);

            Builder.PlaceLogClues(mapState, new List<Tuple<LogEntry, Clue>> { log1, log2, log3, log4 }, true, true);
        }

        public override void RegisterLevels(LevelRegister register)
        {
            //Almost certainly another quest has already registered the bridge, this quest just locks it
            bridgeLevelData = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.BridgeLevel));
        }
    }
}
