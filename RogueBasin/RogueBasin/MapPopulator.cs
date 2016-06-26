using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    public class MapPopulator
    {
        Dictionary<int, RoomInfo> roomInfo = new Dictionary<int,RoomInfo>();
        Dictionary<string, DoorContentsInfo> doorInfo = new Dictionary<string,DoorContentsInfo>();

        public MapPopulator()
        {
        }

        public bool AddFeatureToRoom(MapInfo mapInfo, int roomId, RogueBasin.Point roomPoint, Feature feature)
        {
            var featuresPlaced = AddFeaturesToRoom(mapInfo, roomId, new List<Tuple<RogueBasin.Point, Feature>>() { new Tuple<RogueBasin.Point, Feature>(roomPoint, feature) });

            if (featuresPlaced > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public RoomInfo RoomInfo(int roomIndex)
        {
            RoomInfo thisRoomInfo;
            roomInfo.TryGetValue(roomIndex, out thisRoomInfo);

            if (thisRoomInfo == null)
            {
                roomInfo[roomIndex] = new RoomInfo(roomIndex);
            }

            return roomInfo[roomIndex];
        }

        public IEnumerable<RoomInfo> AllRoomsInfo()
        {
            return roomInfo.Select(r => r.Value);
        }

        public DoorContentsInfo GetDoorInfo(string doorIndex)
        {
            DoorContentsInfo thisDoorInfo;
            doorInfo.TryGetValue(doorIndex, out thisDoorInfo);

            if (thisDoorInfo == null)
            {
                doorInfo[doorIndex] = new DoorContentsInfo(doorIndex);
            }

            return doorInfo[doorIndex];
        }

        public Dictionary<string, DoorContentsInfo> DoorInfo
        {
            get
            {
                return doorInfo;
            }
        }

        public void AddMonsterToRoom(Monster monster, int roomId, Location absoluteLocation)
        {
            RoomInfo(roomId).AddMonster(new MonsterRoomPlacement(monster, absoluteLocation));
        }

        public void AddItemToRoom(Item item, int roomId, Location absoluteLocation)
        {
            RoomInfo(roomId).AddItem(new ItemRoomPlacement(item, absoluteLocation));
        }

        public int AddFeaturesToRoom(MapInfo mapInfo, int roomId, IEnumerable<Tuple<RogueBasin.Point, Feature>> featurePoints)
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
                    RoomInfo(roomId).AddFeature(new FeatureRoomPlacement(featureToPlace, new Location(mapInfo.GetLevelForRoomIndex(roomId), p)));
                    featuresPlaced++;
                }
            }

            return featuresPlaced;
        }

        private void InitialiseRoomFillerWithRoomState(MapInfo mapInfo, RoomFilling roomFiller, int roomId)
        {
            var room = mapInfo.Room(roomId);

            var floorPointsInRoom = RoomTemplateUtilities.GetPointsInRoomWithTerrain(room.Room, RoomTemplateTerrain.Floor);
            var roomInfo = RoomInfo(roomId);

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
