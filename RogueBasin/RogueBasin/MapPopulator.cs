using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    public class MapPopulator
    {
        MapInfo mapInfo;

        public MapPopulator(MapInfo mapInfo)
        {
            this.mapInfo = mapInfo;
        }

        public bool AddFeatureToRoom(MapInfo mapInfo, int roomId, RogueBasin.Point roomPoint, Feature feature)
        {
            var featuresPlaced = mapInfo.Populator.AddFeaturesToRoom(roomId, new List<Tuple<RogueBasin.Point, Feature>>() { new Tuple<RogueBasin.Point, Feature>(roomPoint, feature) });

            if (featuresPlaced > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddMonsterToRoom(Monster monster, int roomId, Location absoluteLocation)
        {
            mapInfo.RoomInfo(roomId).AddMonster(new MonsterRoomPlacement(monster, absoluteLocation));
        }

        public void AddItemToRoom(Item item, int roomId, Location absoluteLocation)
        {
            mapInfo.RoomInfo(roomId).AddItem(new ItemRoomPlacement(item, absoluteLocation));
        }

        public int AddFeaturesToRoom(int roomId, IEnumerable<Tuple<RogueBasin.Point, Feature>> featurePoints)
        {
            var thisRoom = mapInfo.Room(roomId);
            var roomFiller = new RoomFilling(thisRoom.Room);

            //Need to account for all current blocking features in room
            InitialiseRoomFillerWithRoomState(mapInfo, roomFiller, roomId);

            int featuresPlaced = 0;

            foreach (var featurePoint in featurePoints)
            {
                var p = featurePoint.Item1;
                var featureToPlace = featurePoint.Item2;

                var pointInRoom = p - thisRoom.Location;

                if (!featureToPlace.IsBlocking ||
                    featureToPlace.IsBlocking && roomFiller.SetSquareUnWalkableIfMaintainsConnectivity(pointInRoom))
                {
                    mapInfo.RoomInfo(roomId).AddFeature(new FeatureRoomPlacement(featureToPlace, new Location(mapInfo.GetLevelForRoomIndex(roomId), p)));
                    featuresPlaced++;
                }
            }

            return featuresPlaced;
        }

        private void InitialiseRoomFillerWithRoomState(MapInfo mapInfo, RoomFilling roomFiller, int roomId)
        {
            var room = mapInfo.Room(roomId);

            var floorPointsInRoom = RoomTemplateUtilities.GetPointsInRoomWithTerrain(room.Room, RoomTemplateTerrain.Floor);
            var roomInfo = mapInfo.RoomInfo(roomId);

            //Items squares must be connected
            foreach (var item in roomInfo.Items)
            {
                roomFiller.SetSquareAsUnfillableMustBeConnected(item.location.MapCoord - room.Location);
            }

            //Non-blocking features must be connected
            foreach (var feature in roomInfo.Features)
            {
                if (feature.feature.IsBlocking)
                {
                    roomFiller.SetSquareUnwalkable(feature.location.MapCoord - room.Location);
                }
                else
                {
                    roomFiller.SetSquareAsUnfillableMustBeConnected(feature.location.MapCoord - room.Location);
                }
            }

            //Monsters must be connected
            foreach (var monster in roomInfo.Monsters)
            {
                roomFiller.SetSquareAsUnfillableMustBeConnected(monster.location.MapCoord - room.Location);
            }
        }
    }
}
