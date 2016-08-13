using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL.Quests
{
    class MedicalTurretTrapQuest : Quest
    {
        public MedicalTurretTrapQuest(MapState mapState, QuestMapBuilder builder, LogGenerator logGen)
            : base(mapState, builder, logGen)
        {

        }

        public override void SetupQuest()
        {
            var mapInfo = MapState.MapInfo;
            var medicalLevel = MapState.LevelIds["medical"];
            var lowerAtriumLevel = MapState.LevelIds["lowerAtrium"];

            //Lock the door to the next level elevator with a key card which is stashed in a trap room full of turrets
            //Make sure a stun grenade is accessible before the door
            
            var elevatorConnection = MapState.LevelInfo[medicalLevel].ConnectionsToOtherLevels.First().Value;

            var doorId = "medical-turret-security";
            
            //Place locked door
            var totalClues = 2;

            Builder.PlaceMovieDoorOnMap(MapState, doorId, doorId, totalClues, System.Drawing.Color.Red, "t_medicalturretunlocked", "t_medicalturretlocked", elevatorConnection);

            //Place clues
            var manager = MapState.DoorAndClueManager;

            var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            allowedRoomsForClues = mapInfo.FilterOutCorridors(allowedRoomsForClues);

            var roomsForClues = Builder.PickExpandedRoomsFromReducedRoomsList(MapState, totalClues, allowedRoomsForClues);
            var clues = manager.AddCluesToExistingDoor(doorId, roomsForClues);

            //Keycard

            var colorForKeycard = Builder.GetUnusedColor();
            var keycardItem = new RogueBasin.Items.Clue(clues.ElementAt(0), colorForKeycard.Item1, colorForKeycard.Item2);
            var keycardItemAndClue = new Tuple<Clue, Item>(clues.ElementAt(0), keycardItem);

            Builder.PlaceClueItems(MapState, Enumerable.Repeat(keycardItemAndClue, 1), false, true, false);

            //Grenade

            var grenadeItem = new RogueBasin.Items.StunGrenade();
            //since this is not a simple clue item, we need to manually set it as associated with this quest to appear in the drawn graph
            grenadeItem.QuestId = "grenade " + "(" + doorId + ")";
            var grenade = new Tuple<Clue, Item>(clues.ElementAt(1), grenadeItem);

            Builder.PlaceClueItems(MapState, Enumerable.Repeat(grenade, 1), false, true, false);

            //Place log entries explaining the puzzle
            //These will not be turned into in-engine clue items, so they can't be used to open the door
            //They are added though, to ensure that they are readable before the door is opened

            var roomsForLogs = Builder.PickExpandedRoomsFromReducedRoomsList(MapState, 2, allowedRoomsForClues);
            var logClues = manager.AddCluesToExistingDoor(doorId, roomsForLogs);

            var log1 = new Tuple<LogEntry, Clue>(LogGen.GenerateElevatorLogEntry(MapState, medicalLevel, lowerAtriumLevel), logClues[0]);
            var log2 = new Tuple<LogEntry, Clue>(LogGen.GenerateArbitaryLogEntry("qe_medicalturretsecurity"), logClues[1]);
            Builder.PlaceLogClues(MapState, new List<Tuple<LogEntry, Clue>> { log1, log2 }, true, true);
        }
    }
}
