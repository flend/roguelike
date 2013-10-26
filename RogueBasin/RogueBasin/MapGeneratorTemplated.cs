using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    class MapGeneratorTemplated
    {
        Dictionary<RoomTemplateTerrain, MapTerrain> terrainMapping;

        public MapGeneratorTemplated()
        {
            terrainMapping = new Dictionary<RoomTemplateTerrain, MapTerrain>();
            terrainMapping[RoomTemplateTerrain.Wall] = MapTerrain.Wall;
            terrainMapping[RoomTemplateTerrain.Floor] = MapTerrain.Empty;
            terrainMapping[RoomTemplateTerrain.Transparent] = MapTerrain.Void;
        }

        
        /** Build a map using templated rooms */
        public Map GenerateMap()
        {

            //Load sample template
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("vaults.vault1.room", StandardTemplateMapping.terrainMapping);

            Map baseMap = new Map(room1.Width, room1.Height);

            for (int x = 0; x < room1.Width; x++)
            {
                for (int y = 0; y < room1.Height; y++) {
                    baseMap.mapSquares[x, y].Terrain = terrainMapping[room1.terrainMap[x, y]];
                }
            }

            baseMap.PCStartLocation = new Point(room1.Width / 2, room1.Height / 2);

            return baseMap;
        }
    }
}
