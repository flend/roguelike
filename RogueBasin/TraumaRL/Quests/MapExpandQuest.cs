using GraphMap;
using RogueBasin;
using RogueBasin.Items;
using System;
using System.Linq;

namespace TraumaRL.Quests
{
    class MapExpandQuest : Quest
    {
        private int level;

        public MapExpandQuest(QuestMapBuilder builder, LogGenerator logGen, int level)
            : base(builder, logGen)
        {
            this.level = level;
        }

        public override void SetupQuest(MapState mapState)
        {
            var newRoomConnection = AddRoomToMap(mapState, level);

            //Required before adding to the new room
            mapState.RefreshLevelMaps();

            AddTreasure(mapState, newRoomConnection);

            AddLogClue(mapState, newRoomConnection, level);
        }

        private void AddLogClue(MapState mapState, Connection newRoomConnection, int levelToPlace)
        {
            var thisRoom = newRoomConnection.Target;

            //Put it on the critical path from the start vertex
            var criticalPath = mapState.MapInfo.Model.GetPathBetweenVerticesInFullMap(mapState.MapInfo.StartRoom, thisRoom);
            var roomsOnLevel = mapState.MapInfo.GetRoomIndicesForLevel(levelToPlace);

            var filteredRooms = Builder.FilterRoomsByPath(mapState, roomsOnLevel, criticalPath, true, QuestMapBuilder.CluePath.OnCriticalPath, true);
            var roomToPlaceLog = filteredRooms.Skip(1).Take(filteredRooms.Count() / 2).RandomElement();

            var logEntry = LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_stash1", levelToPlace, levelToPlace);
            var log = new Log(logEntry, "mapexpandquest-" + Game.Random.Next());
            Builder.PlaceItems(mapState, log, roomToPlaceLog, false);
        }

        private void AddTreasure(MapState mapState, Connection newRoomConnection)
        {
            var grenade = new FragGrenade();

            Builder.PlaceItems(mapState, grenade, newRoomConnection.Target, false);
        }

        private Connection AddRoomToMap(MapState mapState, int level)
        {
            RoomTemplate roomTemplate = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.tshape1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            var doorIndex = 0; //top door

            var levelGenerator = mapState.LevelGraph.LevelInfo[level].LevelGenerator;

            var sourceDoors = levelGenerator.PotentialDoors.Shuffle();

            Connection placedRoomConnection = null;

            foreach (var door in sourceDoors)
            {
                try
                {
                    placedRoomConnection = levelGenerator.PlaceRoomTemplateAlignedWithExistingDoor(roomTemplate, door, doorIndex);
                    break;
                }
                catch (ApplicationException) { }
            }

            if (placedRoomConnection == null)
            {
                throw new ApplicationException("Unable to place a room for MapExpandQuest.");
            }

            return placedRoomConnection;
        }

        public override void RegisterLevels(LevelRegister register)
        {
            //This quest does not require a specific level
        }
    }
}
