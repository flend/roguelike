using GraphMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    /// <summary>
    /// Class that constructs the full map out of discrete level graphs and room sets
    /// </summary>
    public class MapInfoBuilder
    {
        Dictionary<int, TemplatePositioned> roomTemplates;
        Dictionary<int, int> roomLevels;
        List<ConnectivityMap> connectivityMap;

        ConnectivityMap fullMap;

        int startRoom;

        public MapInfoBuilder()
        {
            roomTemplates = new Dictionary<int, TemplatePositioned>();
            connectivityMap = new List<ConnectivityMap>();
            roomLevels = new Dictionary<int, int>();
        }

        /// <summary>
        /// Add second or subsequent level.
        /// </summary>
        public void AddConstructedLevel(int levelNo, ConnectivityMap levelMap, List<TemplatePositioned> roomsInLevelCoords, Connection connectionBetweenLevels) {

            if (connectivityMap.Count == 0)
                throw new ApplicationException("Need to add first level before using this method");

            foreach(var r in roomsInLevelCoords) {
                roomTemplates.Add(r.RoomIndex, r);
                roomLevels.Add(r.RoomIndex, levelNo);
            }
            connectivityMap.Add(levelMap);

            //Combine into full map
            fullMap.AddAllConnections(levelMap);
            fullMap.AddRoomConnection(connectionBetweenLevels);
        }

        /// <summary>
        /// Add first level, or level without other connections
        /// </summary>
        public void AddConstructedLevel(int levelNo, ConnectivityMap levelMap, List<TemplatePositioned> roomsInLevelCoords, int startRoom)
        {
            foreach (var r in roomsInLevelCoords)
            {
                roomTemplates.Add(r.RoomIndex, r);
                roomLevels.Add(r.RoomIndex, levelNo);
            }
            connectivityMap.Add(levelMap);

            fullMap = levelMap;

            this.startRoom = startRoom;
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
    }

    /// <summary>
    /// Holds state about the multi-level map
    /// </summary>
    public class MapInfo
    {
        Dictionary<int, TemplatePositioned> rooms;
        Dictionary<int, int> roomLevels;
        ConnectivityMap map;
        int startRoom;

        MapModel model;
        
        public MapInfo(MapInfoBuilder builder) {

            rooms = builder.Rooms;
            roomLevels = builder.RoomLevelMapping;
            map = builder.FullConnectivityMap;
            startRoom = builder.StartRoom;

            model = new MapModel(map, startRoom);
        }

        /// <summary>
        /// Returns point in map coords of this level in the required room of terrain
        /// </summary>
        /// <param name="roomIndex"></param>
        /// <param name="terrainToFind"></param>
        /// <returns></returns>
        public Point GetRandomPointInRoomOfTerrain(int levelNo, int roomIndex, RoomTemplateTerrain terrainToFind)
        {
            var roomRelativePoint = RoomTemplateUtilities.GetRandomPointWithTerrain(rooms[roomIndex].Room, terrainToFind);

            return new Point(rooms[roomIndex].Location + roomRelativePoint);
        }

        public IEnumerable<int> GetRoomIndicesForLevel(int level)
        {
            return roomLevels.Where(kv => kv.Value == level).Select(kv => kv.Key);
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
    }
}
