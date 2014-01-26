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

        public MapInfo()
        {
            roomTemplates = new List<List<TemplatePositioned>>();
            connectivityMap = new List<ConnectivityMap>();
        }

        /// <summary>
        /// Add level. Must be added in numerical order (and the same as dungeon!)
        /// </summary>
        /// <param name="levelMap"></param>
        /// <param name="roomsInLevelCoords"></param>
        public void AddConstructedLevel(ConnectivityMap levelMap, List<TemplatePositioned> roomsInLevelCoords, Connection connectionBetweenLevels) {
            
            roomTemplates.Add(roomsInLevelCoords);
            connectivityMap.Add(levelMap);
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

            return new Point(roomTemplates[levelNo][roomIndex].Location + roomRelativePoint);
        }
    }
}
