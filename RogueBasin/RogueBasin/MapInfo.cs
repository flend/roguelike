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
    /// Contains absolute (per-level) mapCoords and the roomId
    /// </summary>
    public class RoomPoint
    {
        public readonly int level;
        public readonly RogueBasin.Point mapLocation;
        public readonly int roomId;

        public RoomPoint(int level, int roomId, RogueBasin.Point mapLocation)
        {
            this.level = level;
            this.roomId = roomId;
            this.mapLocation = mapLocation;
        }

        public Location ToLocation()
        {
            return new Location(level, mapLocation);
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

    public class ItemRoomPlacement
    {
        public readonly Item item;
        public readonly Location location;

        public ItemRoomPlacement(Item i, Location loc)
        {
            item = i;
            location = loc;
        }
    }

    public class RoomInfo {

        private int id;
        private List<FeatureRoomPlacement> features = new List<FeatureRoomPlacement>();
        private List<MonsterRoomPlacement> monsters = new List<MonsterRoomPlacement>();
        private List<ItemRoomPlacement> items = new List<ItemRoomPlacement>();
        
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

        public void AddMonster(MonsterRoomPlacement monster)
        {
            monsters.Add(monster);
        }

        public IEnumerable<MonsterRoomPlacement> Monsters
        {
            get
            {
                return monsters;
            }
        }

        public void AddItem(ItemRoomPlacement item)
        {
            items.Add(item);
        }

        public IEnumerable<ItemRoomPlacement> Items
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

        ConnectivityMap map;
        int startRoom;
        Dictionary<Connection, DoorLocationInfo> doors;

        MapModel model;
        private const int corridorHeight = 3;
        private const int corridorWidth = 3;

        MapPopulator populator;

        public MapInfo(MapInfoBuilder builder, MapPopulator populator) {

            rooms = builder.Rooms;
            roomToLevelMapping = builder.RoomLevelMapping;
            map = builder.FullConnectivityMap;
            startRoom = builder.StartRoom;
            doors = builder.Doors;

            model = new MapModel(map, startRoom);

            this.populator = populator;

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

        /// <summary>
        /// To have an event distribution of items in rooms, we need to place items based on the number of actual rooms that
        /// make up a room in the no-cycles map.
        /// </summary>
        /// <param name="roomNodes"></param>
        /// <returns></returns>
        public IEnumerable<int> RepeatRoomNodesByNumberOfRoomsInCollapsedCycles(IEnumerable<int> roomNodes)
        {
            return roomNodes.SelectMany(r => Enumerable.Repeat(r, Model.GraphNoCycles.roomMappingNoCycleToFullMap[r].Count()));
        }

        public IEnumerable<Point> GetAllPointsInRoomOfTerrain(int roomIndex, RoomTemplateTerrain terrainToFind)
        {
            var roomRelativePoints = RoomTemplateUtilities.GetPointsInRoomWithTerrain(rooms[roomIndex].Room, terrainToFind);

            return roomRelativePoints.Select(p => new Point(rooms[roomIndex].Location + p));
        }

        public IEnumerable<Point> GetUnoccupiedPointsInRoom(int roomIndex)
        {
            var roomRelativePoints = RoomTemplateUtilities.GetPointsInRoomWithTerrain(rooms[roomIndex].Room, RoomTemplateTerrain.Floor);
            var unoccupiedAbsolutePoints = roomRelativePoints.Except(GetOccupiedRoomPointsInRelativeCoords(roomIndex));

            var roomAbsolutePoints = unoccupiedAbsolutePoints.Select(p => new Point(rooms[roomIndex].Location + p));

            return roomAbsolutePoints;
        }

        private IEnumerable<Point> GetOccupiedRoomPointsInRelativeCoords(int roomIndex)
        {
            var roomInfo = Populator.RoomInfo(roomIndex);

            var occupiedFeaturePoints = roomInfo.Features.Where(f => f.feature.IsBlocking).Select(f => f.location.MapCoord);
            var occupiedMonsterPoints = roomInfo.Monsters.Select(m => m.location.MapCoord);

            return occupiedFeaturePoints.Concat(occupiedMonsterPoints);
        }

        public IEnumerable<Point> GetBoundaryFloorPointsInRoom(int roomIndex)
        {
            var roomRelativePoints = RoomTemplateUtilities.GetBoundaryFloorPointsInRoom(rooms[roomIndex].Room);

            return roomRelativePoints.Select(p => new Point(rooms[roomIndex].Location + p));
        }

        public IEnumerable<RoomPoint> GetAllUnoccupiedRoomPoints(IEnumerable<int> rooms)
        {
            var allWalkablePoints = new List<RoomPoint>();

            foreach (var room in rooms)
            {
                var level = GetLevelForRoomIndex(room);
                var allPossiblePoints = GetAllPointsInRoomOfTerrain(room, RoomTemplateTerrain.Floor);
                var allUnoccupiedPoints = allPossiblePoints.Except(GetOccupiedPointsInRoom(room));
                var allUnoccupiedRoomPoints = allUnoccupiedPoints.Select(p => new RoomPoint(level, room, p));

                allWalkablePoints.AddRange(allUnoccupiedRoomPoints);
            }

            return allWalkablePoints.Shuffle();
        }

        public IEnumerable<RoomPoint> GetAllUnoccupiedRoomPointsBoundariesOnly(IEnumerable<int> rooms)
        {
            var allWalkablePoints = new List<RoomPoint>();

            foreach (var room in rooms)
            {
                var level = GetLevelForRoomIndex(room);
                var allPossiblePoints = GetBoundaryFloorPointsInRoom(room);
                var allUnoccupiedPoints = allPossiblePoints.Except(GetOccupiedPointsInRoom(room));
                var allUnoccupiedRoomPoints = allUnoccupiedPoints.Select(p => new RoomPoint(level, room, p));

                allWalkablePoints.AddRange(allUnoccupiedRoomPoints);
            }

            return allWalkablePoints.Shuffle();
        }

        public IEnumerable<RoomPoint> GetAllUnoccupiedRoomPoints(int room, bool preferBoundaries)
        {
            return GetAllUnoccupiedRoomPoints(Enumerable.Repeat(room, 1), preferBoundaries);
        }

        public IEnumerable<RoomPoint> GetAllUnoccupiedRoomPoints(IEnumerable<int> rooms, bool preferBoundaries)
        {
            if (!preferBoundaries)
            {
                return GetAllUnoccupiedRoomPoints(rooms);
            }

            var pointsAtBoundaries = GetAllUnoccupiedRoomPointsBoundariesOnly(rooms);

            if (!pointsAtBoundaries.Any())
            {
                return GetAllUnoccupiedRoomPoints(rooms);
            }

            return pointsAtBoundaries;
        }

        public IEnumerable<int> FilterOutCorridors(IEnumerable<int> roomIndices)
        {
            return roomIndices.Where(r => (rooms[r].Room.Height > corridorHeight && rooms[r].Room.Width > corridorWidth) && !rooms[r].Room.IsCorridor);
        }

        public IEnumerable<Point> GetOccupiedPointsInRoom(int roomIndex)
        {
            var relativeOccupiedPoints = GetOccupiedRoomPointsInRelativeCoords(roomIndex);
            return relativeOccupiedPoints.Select(p => new Point(rooms[roomIndex].Location + p));
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

        public TemplatePositioned Room(int roomIndex)
        {
            return rooms[roomIndex];
        }

        public DoorLocationInfo GetDoorForConnection(Connection connection)
        {
            return doors[connection];
        }

        public MapPopulator Populator
        {
            get
            {
                return populator;
            }
        }
    }
}
