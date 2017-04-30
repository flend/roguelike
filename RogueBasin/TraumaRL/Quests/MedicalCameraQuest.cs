using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumaRL.Quests
{
    class MedicalCameraQuest : Quest
    {
        LevelIdData medicalLevel = null;
        LevelIdData lowerAtriumLevel = null;

        public MedicalCameraQuest(QuestMapBuilder builder, LogGenerator logGen)
            : base(builder, logGen)
        {

        }

        public override void SetupQuest(MapState mapState)
        {
            var mapInfo = mapState.MapInfo;
            var medicalLevelId = mapState.LevelGraph.LevelIds[medicalLevel.name];
            var lowerAtriumLevelId = mapState.LevelGraph.LevelIds[lowerAtriumLevel.name];

            //Lock the door to the elevator and require a certain number of monsters to be killed
            var elevatorConnection = mapState.LevelGraph.LevelInfo[medicalLevelId].ConnectionsToOtherLevels.First().Value;

            var doorId = "medical-security";
            int objectsToPlace = 15;
            int objectsToDestroy = 10;

            //Place door
            Builder.PlaceMovieDoorOnMap(mapState, doorId, doorId, objectsToDestroy, System.Drawing.Color.Red, "t_medicalsecurityunlocked", "t_medicalsecuritylocked", elevatorConnection);

            //This will be restricted to the medical level since we cut off the door
            var manager = mapState.DoorAndClueManager;

            var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            allowedRoomsForClues = mapInfo.FilterOutCorridors(allowedRoomsForClues);

            var roomsForMonsters = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, objectsToPlace, allowedRoomsForClues);
            var clues = manager.AddCluesToExistingDoor(doorId, roomsForMonsters);

            Builder.PlaceCreatureClues<RogueBasin.Creatures.Camera>(mapState, clues, true, false, true);

            //Place log entries explaining the puzzle
            //These will not be turned into in-engine clue items, so they can't be used to open the door
            //They are added though, to ensure that they are readable before the door is opened

            var roomsForLogs = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, 2, allowedRoomsForClues);
            var logClues = manager.AddCluesToExistingDoor(doorId, roomsForLogs);

            var log1 = new Tuple<LogEntry, Clue>(LogGen.GenerateElevatorLogEntry(mapState, medicalLevelId, lowerAtriumLevelId), logClues[0]);
            var log2 = new Tuple<LogEntry, Clue>(LogGen.GenerateArbitaryLogEntry("qe_medicalsecurity"), logClues[1]);
            Builder.PlaceLogClues(mapState, new List<Tuple<LogEntry, Clue>> { log1, log2 }, true, true);
        }

        public override void RegisterLevels(LevelRegister register)
        {
            medicalLevel = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.MedicalLevel));
            lowerAtriumLevel = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.LowerAtriumLevel));
            register.RegisterAscendingDifficultyRelationship(medicalLevel.id, lowerAtriumLevel.id);
        }
    }
}
