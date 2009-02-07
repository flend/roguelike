using System;using System.Collections.Generic;
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

        public static bool operator ==(Point i, Point j)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(i, j))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)i == null) || ((object)j == null))
            {
                return false;
            }

            // Return true if the fields match:
            if (i.x == j.x && i.y == j.y)
                return true;
            return false;
        }

        public static bool operator !=(Point i, Point j)
        {
            return !(i == j);
        }

        public override bool Equals(object obj)
        {
            //Value-wise comparison ensured by the cast
            return this == (Point)obj;
        }

        public override int GetHashCode()
        {
            return x ^ y;
        }
    }
    
    public enum MapTerrain
    {
        Void,    //non-walkable
        Empty,   //walkable
        Wall,    //non-walkable
        Corridor //walkable
    }

    public class MapSquare
    {
        MapTerrain terrain = MapTerrain.Empty;
        
        /// <summary>
        /// Is the square walkable. This is recalculated based on creature positions etc. each turn
        /// </summary>
        bool walkable = false;

        /// <summary>
        /// Does the square block light. Shouldn't change after the map is made
        /// </summary>
        bool blocksLight = true;

        /// <summary>
        /// Has this square ever been in the FOV?
        /// </summary>
        bool seenByPlayer = false;

        /// <summary>
        /// In player's FOV - recalculated each turn
        /// </summary>
        bool inPlayerFOV = false;

        /// <summary>
        /// In a creature's FOV (may be debug only)
        /// </summary>
        bool inMonsterFOV = false;

        public MapTerrain Terrain
        {
            set
            {
                terrain = value;
                /*if (terrain == MapTerrain.Wall)
                    walkable = false;
                else
                    walkable = true;*/
            }
            get
            {
                return terrain;
            }
        }

        /// <summary>
        /// Is the square walkable. This is recalculated based on creature positions etc. each turn
        /// </summary>
        public bool Walkable
        {
            get
            {
                return walkable;
            }
            set
            {
                walkable = value;
            }
        }

        /// <summary>
        /// Does the square block light. Shouldn't change after the map is made
        /// </summary>
        public bool BlocksLight
        {
            get
            {
                return blocksLight;
            }
            set
            {
                blocksLight = value;
            }
        }

        /// <summary>
        /// Has this square ever been in the FOV?
        /// </summary>
        public bool SeenByPlayer
        {
            get
            {
                return seenByPlayer;
            }
            set
            {
                seenByPlayer = value;
            }
        }

        /// <summary>
        /// In player's FOV - recalculated each turn
        /// </summary>
        public bool InPlayerFOV
        {
            get
            {
                return inPlayerFOV;
            }
            set
            {
                inPlayerFOV = value;
            }
        }

        /// <summary>
        /// In a creature's FOV (may be debug only)
        /// </summary>
        public bool InMonsterFOV
        {
            get
            {
                return inMonsterFOV;
            }
            set
            {
                inMonsterFOV = value;
            }
        }

        //Sets walkable and non-light blocking
        public void SetOpen() {
            BlocksLight = false;
            Walkable = true;
        }
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
                }
            }

            Clear();
        }

        public void Clear()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    mapSquares[i, j].Terrain = MapTerrain.Void;
                }
            }
        }


    }
}
