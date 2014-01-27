using GraphMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    /// <summary>
    /// Class that returns useful information about the map in world coordinates
    /// </summary>
    public class MapInfo
    {
        Dictionary<int, TemplatePositioned> roomTemplates;
        Dictionary<int, int> roomLevels;
        List<ConnectivityMap> connectivityMap;

        ConnectivityMap fullMap;

        public MapInfo()
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
        public void AddConstructedLevel(int levelNo, ConnectivityMap levelMap, List<TemplatePositioned> roomsInLevelCoords)
        {
            foreach (var r in roomsInLevelCoords)
            {
                roomTemplates.Add(r.RoomIndex, r);
                roomLevels.Add(r.RoomIndex, levelNo);
            }
            connectivityMap.Add(levelMap);

            fullMap = levelMap;
        }
        
        /// <summary>
        /// Returns point in map coords of this level in the required room of terrain
        /// </summary>
        /// <param name="roomIndex"></param>
        /// <param name="terrainToFind"></param>
        /// <returns></returns>
        public Point GetRandomPointInRoomOfTerrain(int levelNo, int roomIndex, RoomTemplateTerrain terrainToFind) {
         
            var roomRelativePoint = RoomTemplateUtilities.GetRandomPointWithTerrain(roomTemplates[roomIndex].Room, terrainToFind);

            return new Point(roomTemplates[roomIndex].Location + roomRelativePoint);
        }

        public TemplatePositioned GetRoom(int roomIndex)
        {
            return roomTemplates[roomIndex];
        }

        public ConnectivityMap FullConnectivityMap
        {
            get
            {
                return fullMap;
            }
        }
    }
}
