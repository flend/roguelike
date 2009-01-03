using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    class MapGeneratorRogue
    {

        int width = 80;
        int height = 25;

        public MapGeneratorRogue()
        {

        }


        public Map GenerateMap()
        {
            Map baseMap = new Map(width, height);

            //Make a square

            for (int i = 10; i < 20; i++)
            {
                for (int j = 10; j < 20; j++)
                {
                    baseMap.mapSquares[i,j] = Map.MapTerrain.Wall;
                }
            }

            for (int i = 11; i < 19; i++)
            {
                for (int j = 11; j < 19; j++)
                {
                    baseMap.mapSquares[i,j] = Map.MapTerrain.Empty;
                }
            }
            
            //Stick the PC in the middle

            baseMap.PCStartLocation = new Point(15, 15);

            return baseMap;
        }

        public int Width
        {
            set
            {
                width = value;
            }
        }
        public int Height {
            set
            {
                height = value;
            }
        }


    }
}
