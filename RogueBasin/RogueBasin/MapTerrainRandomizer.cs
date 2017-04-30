using System.Collections.Generic;

namespace RogueBasin
{
    static public class MapTerrainRandomizer
    {
        /** Produce a new map with randomized terrain. Its pathing and sight may need to be recalculated */
        static public Map RandomizeTerrainInMap(Map mapToRandomize, Dictionary<MapTerrain, List<MapTerrain>> randomizeMapping)
        {
            var outputMap = mapToRandomize.Clone();

            for (int i = 0; i < mapToRandomize.width; i++)
            {
                for (int j = 0; j < mapToRandomize.height; j++)
                {
                    var thisTerrain = mapToRandomize.mapSquares[i, j].Terrain;

                    if (randomizeMapping.ContainsKey(thisTerrain))
                    {
                        outputMap.mapSquares[i, j].Terrain = randomizeMapping[thisTerrain].RandomElement();
                    }
                }
            }

            return outputMap;
        }
    }
}
