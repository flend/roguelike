﻿using System;using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    //Represents a level map
    public class Point {
        public int x;
        public int y;

        public Point(int x, int y) {
            this.x = x;
            this.y = y;
        }
    }
    
    public enum MapTerrain
    {
        Empty = 0,
        Wall = 1,
        Corridor = 2
    }

    public class MapSquare
    {
        public MapTerrain terrain;
    }

    public class Map
    {
        public MapSquare[,] mapSquares;
        public Point PCStartLocation;

        public int width;
        public int height;

        public Map(int width, int height)
        {
            this.width = width;
            this.height = height;

            mapSquares = new MapSquare[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    mapSquares[i, j] = new MapSquare();
                    mapSquares[i, j].terrain = MapTerrain.Empty;
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    mapSquares[i, j].terrain = MapTerrain.Empty;
                }
            }
        }


    }
}