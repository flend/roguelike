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
        List<List<TemplatePositioned>> roomTemplates;
        List<ConnectivityMap> connectivityMap;

        ConnectivityMap fullMap;

        public MapInfo()
        {
            roomTemplates = new List<List<TemplatePositioned>>();
            connectivityMap = new List<ConnectivityMap>();
        }

        /// <summary>
        /// Add second or subsequent level. Must be added in numerical order (and the same as dungeon!)
        /// </summary>
        public void AddConstructedLevel(ConnectivityMap levelMap, List<TemplatePositioned> roomsInLevelCoords, Connection connectionBetweenLevels) {

            if (connectivityMap.Count == 0)
                throw new ApplicationException("Need to add first level before using this method");

            roomTemplates.Add(roomsInLevelCoords);
            connectivityMap.Add(levelMap);

            //Add into full map
            fullMap.AddAllConnections(levelMap);
            fullMap.AddRoomConnection(connectionBetweenLevels);
        }

        /// <summary>
        /// Add first level. Must be added in numerical order (and the same as dungeon!)
        /// </summary>
        public void AddConstructedLevel(ConnectivityMap levelMap, List<TemplatePositioned> roomsInLevelCoords)
        {
            if (connectivityMap.Count > 0)
                throw new ApplicationException("Can't add first level twice");

            roomTemplates.Add(roomsInLevelCoords);
            connectivityMap.Add(levelMap);

            fullMap = levelMap;
        }
        
        public List<TemplatePositioned> RoomTemplatesForLevel(int levelNo) {
            return roomTemplates[levelNo];
        }

        /// <summary>
        /// Returns point in map coords of this level in the required room of terrain
        /// </summary>
        /// <param name="roomIndex"></param>
        /// <param name="terrainToFind"></param>
        /// <returns></returns>
        public Point GetRandomPointInRoomOfTerrain(int levelNo, int roomIndex, RoomTemplateTerrain terrainToFind) {
         
            var roomRelativePoint = RoomTemplateUtilities.GetRandomPointWithTerrain(roomTemplates[levelNo][roomIndex].Room, terrainToFind);

            return new Point(GetRoom(levelNo, roomIndex).Location + roomRelativePoint);
        }

        public TemplatePositioned GetRoom(int levelNo, int roomIndex)
        {
            return roomTemplates[levelNo][roomIndex];
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
