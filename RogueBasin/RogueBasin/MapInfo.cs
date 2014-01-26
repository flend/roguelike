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
        ConnectivityMap connectivityMap;
        List<TemplatePositioned> roomTemplates;

        public MapInfo(ConnectivityMap connectivityMap, List<TemplatePositioned> roomsInWorldCoords)
        {
            this.connectivityMap = connectivityMap;
            this.roomTemplates = roomsInWorldCoords;
        }
    }
}
