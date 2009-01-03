using System;using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    //Represents a level map
    class Point {
        public int x;
        public int y;

        public Point(int x, int y) {
            this.x = x;
            this.y = y;
        }
    }
    
    class Map
    {
        public MapTerrain [,] mapSquares;
        public Point PCStartLocation;

        public int width;
        public int height;

        public enum MapTerrain {
            Empty = 0,
            Wall = 1
        }

        public Map(int width, int height)
        {
            this.width = width;
            this.height = height;

            mapSquares = new MapTerrain[width, height];
        }

        public void Clear() {
            for(int i=0;i<width;i++) {
                for(int j=0;j<height;j++) {
                mapSquares[i,j] = MapTerrain.Empty;
                }
            }
        }


    }
}
