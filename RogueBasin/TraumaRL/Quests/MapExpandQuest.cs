using GraphMap;
using RogueBasin;
using RogueBasin.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraumaRL.Quests
{
    class MapExpandQuest : Quest
    {
        private int level;

        public MapExpandQuest(MapState mapState, QuestMapBuilder builder, LogGenerator logGen, int level)
            : base(mapState, builder, logGen)
        {
            this.level = level;
        }

        public override void SetupQuest()
        {
            var newRoomConnection = AddRoomToMap(level);

            //Required before adding to the new room
            MapState.RefreshLevelMaps();

            AddTreasure(newRoomConnection);

            AddLogClue(newRoomConnection, level);
        }

        private void AddLogClue(Connection newRoomConnection, int levelToPlace)
        {
            var thisRoom = newRoomConnection.Target;

            //Put it on the critical path from the start vertex
            var criticalPath = MapState.MapInfo.Model.GetPathBetweenVerticesInFullMap(MapState.MapInfo.StartRoom, thisRoom);
            var roomsOnLevel = MapState.MapInfo.GetRoomIndicesForLevel(levelToPlace);

            var filteredRooms = Builder.FilterRoomsByPath(MapState, roomsOnLevel, criticalPath, true, QuestMapBuilder.CluePath.OnCriticalPath, true);
            var roomToPlaceLog = filteredRooms.Skip(1).Take(filteredRooms.Count() / 2).RandomElement();

            var logEntry = LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_stash1", levelToPlace, levelToPlace);
            var log = new Log(logEntry, "mapexpandquest-" + Game.Random.Next());
            Builder.PlaceItems(MapState, log, roomToPlaceLog, false);
        }

        private void AddTreasure(Connection newRoomConnection)
        {
            var grenade = new FragGrenade();

            Builder.PlaceItems(MapState, grenade, newRoomConnection.Target, false);
        }

        private Connection AddRoomToMap(int level)
        {
            RoomTemplate roomTemplate = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.tshape1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            var doorIndex = 0; //top door

            var levelGenerator = MapState.LevelInfo[level].LevelGenerator;

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
    }
}
