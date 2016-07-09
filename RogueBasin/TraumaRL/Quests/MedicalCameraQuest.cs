using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL.Quests
{
    class MedicalCameraQuest : Quest
    {
        public MedicalCameraQuest(MapState mapState, QuestMapBuilder builder, LogGenerator logGen)
            : base(mapState, builder, logGen)
        {

        }

        public override void SetupQuest()
        {
            var mapInfo = MapState.MapInfo;
            var medicalLevel = MapState.LevelIds["medical"];
            var lowerAtriumLevel = MapState.LevelIds["lowerAtrium"];

            //Lock the door to the elevator and require a certain number of monsters to be killed
            var elevatorConnection = MapState.LevelInfo[medicalLevel].ConnectionsToOtherLevels.First().Value;

            var doorId = "medical-security";
            int objectsToPlace = 15;
            int objectsToDestroy = 10;

            //Place door
            Builder.PlaceMovieDoorOnMap(MapState, doorId, doorId, objectsToDestroy, System.Drawing.Color.Red, "t_medicalsecurityunlocked", "t_medicalsecuritylocked", elevatorConnection);

            //This will be restricted to the medical level since we cut off the door
            var manager = MapState.DoorAndClueManager;

            var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            allowedRoomsForClues = mapInfo.FilterOutCorridors(allowedRoomsForClues);

            var roomsForMonsters = Builder.GetRandomRoomsForClues(MapState, objectsToPlace, allowedRoomsForClues);
            var clues = manager.AddCluesToExistingDoor(doorId, roomsForMonsters);

            Builder.PlaceCreatureClues<RogueBasin.Creatures.Camera>(MapState, clues, true, false, true);

            //Place log entries explaining the puzzle
            //These will not be turned into in-engine clue items, so they can't be used to open the door
            //They are added though, to ensure that they are readable before the door is opened

            var roomsForLogs = Builder.GetRandomRoomsForClues(MapState, 2, allowedRoomsForClues);
            var logClues = manager.AddCluesToExistingDoor(doorId, roomsForLogs);

            var log1 = new Tuple<LogEntry, Clue>(LogGen.GenerateElevatorLogEntry(MapState, medicalLevel, lowerAtriumLevel), logClues[0]);
            var log2 = new Tuple<LogEntry, Clue>(LogGen.GenerateArbitaryLogEntry("qe_medicalsecurity"), logClues[1]);
            Builder.PlaceLogClues(MapState, new List<Tuple<LogEntry, Clue>> { log1, log2 }, true, true);
        }
    }
}
