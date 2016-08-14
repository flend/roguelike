using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL.Quests
{
    class BlockElevatorQuest : Quest
    {
        int questLevel;
        private Dictionary<int, List<Connection>> roomConnectivityMap;
        public bool ClueOnElevatorLevel { get; set; }
        public int MaxDoorsToMake { get; set; }

        public BlockElevatorQuest(MapState mapState, QuestMapBuilder builder, LogGenerator logGen, int questLevel, Dictionary<int, List<Connection>> roomConnectivityMap)
            : base(mapState, builder, logGen)
        {
            this.questLevel = questLevel;
            this.roomConnectivityMap = roomConnectivityMap;

            ClueOnElevatorLevel = false;
            MaxDoorsToMake = 1;
        }

        public override void SetupQuest()
        {
            try
            {
                BlockElevatorPaths(MapState, Builder, roomConnectivityMap, questLevel, MaxDoorsToMake, ClueOnElevatorLevel);
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntryDebug("Atrium Elevator Exception: " + ex, LogDebugLevel.High);
            }
        }

        private bool BlockElevatorPaths(MapState mapState, QuestMapBuilder builder, Dictionary<int, List<Connection>> roomConnectivityMap,
        int levelForBlocks, int maxDoorsToMake, bool clueOnElevatorLevel)
        {
            var levelInfo = mapState.LevelInfo;
            var connectionsFromThisLevel = levelInfo[levelForBlocks].ConnectionsToOtherLevels;

            var pairs = Utility.GetPermutations<int>(connectionsFromThisLevel.Keys, 2);

            if (pairs.Count() == 0)
            {
                LogFile.Log.LogEntryDebug("Can't find pair of elevators to connection", LogDebugLevel.High);
                return false;
            }

            var pairsLeft = pairs.Select(s => s);

            int doorsMade = 0;
            while (doorsMade < maxDoorsToMake && pairsLeft.Count() > 0)
            {
                var pairToTry = pairsLeft.RandomElement();

                var sourceElevatorConnection = levelInfo[levelForBlocks].ConnectionsToOtherLevels[pairToTry.ElementAt(0)];
                var targetElevatorConnection = levelInfo[levelForBlocks].ConnectionsToOtherLevels[pairToTry.ElementAt(1)];

                var startDoor = sourceElevatorConnection.Source;
                var endDoor = targetElevatorConnection.Source;

                var colorToUse = builder.GetUnusedColor();

                var doorName = colorToUse.Item2 + " key card";
                var doorId = mapState.LevelNames[levelForBlocks] + "-" + doorName + Game.Random.Next();
                var doorColor = colorToUse.Item1;

                LogFile.Log.LogEntryDebug("Blocking elevators " + pairToTry.ElementAt(0) + " to " + pairToTry.ElementAt(1) + " with " + doorId, LogDebugLevel.High);

                BlockPathBetweenRoomsWithSimpleDoor(mapState, builder, roomConnectivityMap,
                    doorId, doorName, doorColor, 1, startDoor, endDoor,
                    0.5, clueOnElevatorLevel, QuestMapBuilder.CluePath.NotOnCriticalPath, true,
                    true, QuestMapBuilder.CluePath.OnCriticalPath, true);

                doorsMade++;
                pairsLeft = pairsLeft.Except(Enumerable.Repeat(pairToTry, 1));
            }

            return true;
        }

        private void BlockPathBetweenRoomsWithSimpleDoor(MapState mapState, QuestMapBuilder builder, Dictionary<int, List<Connection>> roomConnectivityMap,
            string doorId, string doorName, System.Drawing.Color colorToUse, int cluesForDoor, int sourceRoom, int endRoom,
            double distanceFromSourceRatio, bool enforceClueOnDestLevel, QuestMapBuilder.CluePath clueNotOnCriticalPath, bool clueNotInCorridors,
            bool hasLogClue, QuestMapBuilder.CluePath logOnCriticalPath, bool logNotInCorridors)
        {
            var manager = mapState.DoorAndClueManager;
            var mapInfo = mapState.MapInfo;

            var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(sourceRoom, endRoom);
            var criticalConnectionForDoor = criticalPath.ElementAt((int)Math.Min(criticalPath.Count() * distanceFromSourceRatio, criticalPath.Count() - 1));

            criticalConnectionForDoor = MapAnalysisUtilities.FindFreeConnectionOnPath(manager, criticalPath, criticalConnectionForDoor);

            //Place door

            builder.PlaceLockedDoorOnMap(mapState, doorId, doorName, cluesForDoor, colorToUse, criticalConnectionForDoor);

            //Place clues

            var allRoomsForClue = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            var preferredRooms = builder.FilterRoomsByPath(mapState, allRoomsForClue, criticalPath, enforceClueOnDestLevel, clueNotOnCriticalPath, clueNotInCorridors);

            var roomsForClues = builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, cluesForDoor, preferredRooms);
            var clues = manager.AddCluesToExistingDoor(doorId, roomsForClues);

            var cluesAndColors = clues.Select(c => new Tuple<Clue, System.Drawing.Color, string>(c, colorToUse, doorName));

            var clueLocations = builder.PlaceSimpleClueItems(mapState, cluesAndColors, clueNotInCorridors, false);

            //Place log entries explaining the puzzle

            if (hasLogClue)
            {
                //Put major clue on the critical path

                var preferredRoomsForLogs = builder.FilterRoomsByPath(mapState, allRoomsForClue, criticalPath, false, logOnCriticalPath, logNotInCorridors);
                var roomsForLogs = builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, 1, preferredRoomsForLogs);
                var logClues = manager.AddCluesToExistingDoor(doorId, roomsForLogs);

                //Put minor clue somewhere else
                var preferredRoomsForLogsNonCritical = builder.FilterRoomsByPath(mapState, allRoomsForClue, criticalPath, false, QuestMapBuilder.CluePath.Any, logNotInCorridors);

                var roomsForLogsNonCritical = builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, 1, preferredRoomsForLogsNonCritical);
                var logCluesNonCritical = manager.AddCluesToExistingDoor(doorId, roomsForLogsNonCritical);

                var coupledLogs = LogGen.GenerateCoupledDoorLogEntry(mapState, doorName, mapInfo.GetLevelForRoomIndex(criticalConnectionForDoor.Source),
                    clueLocations.First().level);
                var log1 = new Tuple<LogEntry, Clue>(coupledLogs[0], logClues[0]);
                var log2 = new Tuple<LogEntry, Clue>(coupledLogs[1], logCluesNonCritical[0]);
                builder.PlaceLogClues(mapState, new List<Tuple<LogEntry, Clue>> { log1, log2 }, true, true);
            }
        }

    }
}
