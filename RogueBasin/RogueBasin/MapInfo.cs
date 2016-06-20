using GraphMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    /// <summary>
    /// Info about a door's location. We don't know the levelNo when this is created in TemplatedMapGenerator
    /// </summary>
    public class DoorLocationInfo
    {
        public Point MapLocation { get; private set; }
        public int LevelNo { get; private set; }

        public DoorLocationInfo() { }

        public DoorLocationInfo(Point mapLocation, int levelNo) {
            MapLocation = mapLocation;
            LevelNo = levelNo;
        }
    }


    /// <summary>
    /// Class that constructs the full map out of discrete level graphs and room sets
    /// </summary>
    public class MapInfoBuilder
    {
        Dictionary<int, TemplatePositioned> roomTemplates;
        Dictionary<Connection, DoorLocationInfo> doors;
        Dictionary<int, int> roomLevels;
        Dictionary<int, List<int>> roomForLevel;
        List<ConnectivityMap> connectivityMap;

        ConnectivityMap fullMap;

        int startRoom;

        public MapInfoBuilder()
        {
            roomTemplates = new Dictionary<int, TemplatePositioned>();
            connectivityMap = new List<ConnectivityMap>();
            roomLevels = new Dictionary<int, int>();
            roomForLevel = new Dictionary<int, List<int>>();
            doors = new Dictionary<Connection, DoorLocationInfo>();
        }

        /// <summary>
        /// Add second or subsequent level.
        /// </summary>
        public void AddConstructedLevel(int levelNo, ConnectivityMap levelMap, List<TemplatePositioned> roomsInLevelCoords, Dictionary<Connection, Point> doorsInLevel, Connection connectionBetweenLevels)
        {
            if (connectivityMap.Count == 0)
                throw new ApplicationException("Need to add first level before using this method");

            AddConstructedLevelItems(levelNo, levelMap, roomsInLevelCoords, doorsInLevel);

            //Combine into full map
            fullMap.AddAllConnections(levelMap);
            fullMap.AddRoomConnection(connectionBetweenLevels);
        }

        /// <summary>
        /// Add first level, or level without other connections
        /// </summary>
        public void AddConstructedLevel(int levelNo, ConnectivityMap levelMap, List<TemplatePositioned> roomsInLevelCoords, Dictionary<Connection, Point> doorsInLevel, int startRoom)
        {
            AddConstructedLevelItems(levelNo, levelMap, roomsInLevelCoords, doorsInLevel);

            fullMap = levelMap;

            this.startRoom = startRoom;
        }

        private void AddConstructedLevelItems(int levelNo, ConnectivityMap levelMap, List<TemplatePositioned> roomsInLevelCoords, Dictionary<Connection, Point> doorsInLevel)
        {
            foreach(var r in roomsInLevelCoords) {
                roomTemplates.Add(r.RoomIndex, r);
                roomLevels.Add(r.RoomIndex, levelNo);
            }
            connectivityMap.Add(levelMap);

            //Add all new doors
            foreach (var p in doorsInLevel)
            {
                doors.Add(p.Key, new DoorLocationInfo(p.Value, levelNo));
            }
        }
        

        public ConnectivityMap FullConnectivityMap
        {
            get
            {
                return fullMap;
            }
        }

        public Dictionary<int, int> RoomLevelMapping
        {
            get
            {
                return roomLevels;
            }
        }

        public Dictionary<int, TemplatePositioned> Rooms
        {
            get
            {
                return roomTemplates;
            }
        }

        public int StartRoom
        {
            get
            {
                return startRoom;
            }
        }

        public Dictionary<Connection, DoorLocationInfo> Doors
        {
            get
            {
                return doors;
            }
        }
    }

    public class MonsterRoomPlacement
    {
        public readonly Monster monster;
        public readonly Location location;

        public MonsterRoomPlacement(Monster m, Location loc)
        {
            monster = m;
            location = loc;
        }
    }

    public class FeatureRoomPlacement
    {
        public readonly Feature feature;
        public readonly Location location;

        public FeatureRoomPlacement(Feature f, Location loc)
        {
            feature = f;
            location = loc;
        }
    }

    public class RoomInfo {

        private int id;
        private List<FeatureRoomPlacement> features = new List<FeatureRoomPlacement>();
        private List<MonsterRoomPlacement> monsters = new List<MonsterRoomPlacement>();
        private List<Item> items = new List<Item>();
        
        public RoomInfo(int roomId) {
            this.id = roomId;
        }

        public void AddFeature(FeatureRoomPlacement feature)
        {
            features.Add(feature);
        }

        public IEnumerable<FeatureRoomPlacement> Features
        {
            get {
                return features;
            }
        }

        public void AddMonster(Monster creature, Location loc)
        {
            monsters.Add(new MonsterRoomPlacement(creature, loc));
        }

        public IEnumerable<MonsterRoomPlacement> Monsters
        {
            get
            {
                return monsters;
            }
        }

        public void AddItem(Item item)
        {
            items.Add(item);
        }

        public IEnumerable<Item> Items
        {
            get
            {
                return items;
            }
        }
    }

    public class DoorContentsInfo
    {

        private string id;
        private List<Lock> locks = new List<Lock>();

        public DoorContentsInfo(string doorId)
        {
            this.id = doorId;
        }

        public void AddLock(Lock newLock)
        {
            locks.Add(newLock);
        }

        public IEnumerable<Lock> Locks
        {
            get
            {
                return locks;
            }
        }
    }

    /// <summary>
    /// Holds state about the multi-level map
    /// </summary>
    public class MapInfo
    {
        Dictionary<int, TemplatePositioned> rooms;
        Dictionary<int, int> roomToLevelMapping;
        Dictionary<int, List<int>> roomListForLevel;
        Dictionary<int, RoomInfo> roomInfo;
        Dictionary<string, DoorContentsInfo> doorInfo;

        ConnectivityMap map;
        int startRoom;
        Dictionary<Connection, DoorLocationInfo> doors;

        MapModel model;
        private const int corridorHeight = 3;
        private const int corridorWidth = 3;
        
        public MapInfo(MapInfoBuilder builder) {

            rooms = builder.Rooms;
            roomToLevelMapping = builder.RoomLevelMapping;
            map = builder.FullConnectivityMap;
            startRoom = builder.StartRoom;
            doors = builder.Doors;

            model = new MapModel(map, startRoom);

            roomInfo = new Dictionary<int, RoomInfo>();
            doorInfo = new Dictionary<string, DoorContentsInfo>();

            BuildRoomIndices();
        }

        public int StartRoom { get { return startRoom; } }

        private void BuildRoomIndices()
        {
            roomListForLevel = new Dictionary<int,List<int>>();

            foreach(var level in roomToLevelMapping.Values.Distinct()) {
                var roomsInThisLevel = roomToLevelMapping.Where(kv => kv.Value == level).Select(kv => kv.Key).ToList();
                roomListForLevel[level] = roomsInThisLevel;
            }
        }

        /// <summary>
        /// Returns point in map coords of this level in the required room of terrain
        /// </summary>
        /// <param name="roomIndex"></param>
        /// <param name="terrainToFind"></param>
        /// <returns></returns>
        public Point GetRandomPointInRoomOfTerrain(int roomIndex, RoomTemplateTerrain terrainToFind)
        {
            var roomRelativePoint = RoomTemplateUtilities.GetRandomPointWithTerrain(rooms[roomIndex].Room, terrainToFind);

            return new Point(rooms[roomIndex].Location + roomRelativePoint);
        }

        public IEnumerable<Point> GetAllPointsInRoomOfTerrain(int roomIndex, RoomTemplateTerrain terrainToFind)
        {
            var roomRelativePoints = RoomTemplateUtilities.GetPointsInRoomWithTerrain(rooms[roomIndex].Room, terrainToFind);

            return roomRelativePoints.Select(p => new Point(rooms[roomIndex].Location + p));
        }

        public IEnumerable<Point> GetFreePointsToPlaceCreatureInRoom(int roomIndex)
        {
            var roomRelativePoints = RoomTemplateUtilities.GetPointsInRoomWithTerrain(rooms[roomIndex].Room, RoomTemplateTerrain.Floor);
            var roomAbsolutePoints = roomRelativePoints.Select(p => new Point(rooms[roomIndex].Location + p));

            var unoccupiedAbsolutePoints = roomAbsolutePoints.Except(GetOccupiedRoomPoints(roomIndex));

            return unoccupiedAbsolutePoints;
        }

        private IEnumerable<Point> GetOccupiedRoomPoints(int roomIndex)
        {
            var roomInfo = RoomInfo(roomIndex);

            var occupiedFeaturePoints = roomInfo.Features.Where(f => f.feature.IsBlocking).Select(f => f.location.MapCoord);
            var occupiedMonsterPoints = roomInfo.Monsters.Select(m => m.location.MapCoord);

            return occupiedFeaturePoints.Concat(occupiedMonsterPoints);
        }

        public IEnumerable<Point> GetBoundaryFloorPointsInRoom(int roomIndex)
        {
            var roomRelativePoints = RoomTemplateUtilities.GetBoundaryFloorPointsInRoom(rooms[roomIndex].Room);

            return roomRelativePoints.Select(p => new Point(rooms[roomIndex].Location + p));
        }

        public IEnumerable<int> FilterOutCorridors(IEnumerable<int> roomIndices)
        {
            return roomIndices.Where(r => (rooms[r].Room.Height > corridorHeight && rooms[r].Room.Width > corridorWidth) && !rooms[r].Room.IsCorridor);
        }

        public IEnumerable<int> FilterRoomsByLevel(IEnumerable<int> roomIndices, IEnumerable<int> levels)
        {
            return roomIndices.Where(r => levels.Contains(GetLevelForRoomIndex(r)));
        }

        public IEnumerable<int> GetRoomIndicesForLevel(int level)
        {
            return roomListForLevel[level];
        }

        public int GetLevelForRoomIndex(int roomIndex)
        {
            return roomToLevelMapping[roomIndex];
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

        public DoorContentsInfo DoorInfo(string doorIndex)
        {
            DoorContentsInfo thisDoorInfo;
            doorInfo.TryGetValue(doorIndex, out thisDoorInfo);

            if (thisDoorInfo == null)
            {
                doorInfo[doorIndex] = new DoorContentsInfo(doorIndex);
            }

            return doorInfo[doorIndex];
        }

        public IEnumerable<int> RoomsInDescendingDistanceFromSource(int sourceRoom, IEnumerable<int> testRooms)
        {
            var deadEndDistancesFromStartRoom = Model.GetDistanceOfVerticesFromParticularVertexInFullMap(sourceRoom, testRooms);
            var verticesByDistance = deadEndDistancesFromStartRoom.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).Select(kv => kv.Key);

            return verticesByDistance;
        }

        /// <summary>
        /// Get all connections on level. Also includes connections from this level to other levels
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public IEnumerable<Connection> GetConnectionsOnLevel(int level)
        {
            var roomIndicesForLevel = GetRoomIndicesForLevel(level);

            //This may be slow and better done by the underlying map representation
            return map.GetAllConnections().Where(c => roomIndicesForLevel.Contains(c.Source) || roomIndicesForLevel.Contains(c.Target));
        }

        public IEnumerable<List<Connection>> GetCyclesOnLevel(int level)
        {
            var allCycles = Model.GraphNoCycles.AllCycles;

            return allCycles.Where(cycle =>
                cycle.Select(c => c.Source).Union(cycle.Select(c => c.Target)).Intersect(roomListForLevel[level]).Any()
            );
        }

        public MapModel Model
        {
            get
            {
                return model;
            }
        }

        public TemplatePositioned GetRoom(int roomIndex)
        {
            return rooms[roomIndex];
        }

        public DoorLocationInfo GetDoorForConnection(Connection connection)
        {
            return doors[connection];
        }
    }
}
