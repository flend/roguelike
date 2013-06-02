using System;using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    //Represents a point
    //This should be immutable (make x and y readonly)
    public class Point {
        public int x;
        public int y;

        public Point()
        {
            x = 0;
            y = 0;
        }

        public Point(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public Point(Point oldP)
        {
            this.x = oldP.x;
            this.y = oldP.y;
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

        public static Point operator-(Point i, Point j)
        {
            return new Point(i.x - j.x, i.y - j.y);
        }

        public static Point operator +(Point i, Point j)
        {
            return new Point(i.x + j.x, i.y + j.y);
        }

        public override string ToString()
        {
            return "(x: " + this.x + ", y: " + this.y + ")";
        }
    }
    
    public enum MapTerrain
    {
        Void,    //non-walkable
        Empty,   //walkable
        Wall,    //non-walkable
        Corridor, //walkable
        ClosedDoor, //non-walkable
        OpenDoor, //walkable
        Flooded, //walkable
        River, //non-walkable
        Mountains, //non-walkable
        SkeletonWall, //non-walkable
        SkeletonWallWhite, //non-walkable
        Volcano, //non-walkable
        Trees, //non-walkable
        Road, //walkable
        Grass, //walkable
        Literal, //no walkable
        Gravestone, //walkable
        Rubble, //walkable
        Forest, //not walkable
        BarDoor,//not walkable
        HellWall,//not walkable
        DockWall//not walkable
     }

    public class MapSquare
    {
        MapTerrain terrain = MapTerrain.Empty;
        //Used for textual terrain
        public char terrainLiteral {get; set;}

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
        /// Has this square been seen this adventure (i.e. since left school)
        /// </summary>
        bool seenByPlayerThisRun = false;

        /// <summary>
        /// In player's FOV - recalculated each turn
        /// </summary>
        bool inPlayerFOV = false;

        /// <summary>
        /// In a creature's FOV (may be debug only)
        /// </summary>
        bool inMonsterFOV = false;

        public MapSquare Clone() {

            return this.MemberwiseClone() as MapSquare;
        }

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
        /// Get: Has this square ever been in the FOV?
        /// Set: Player sees this square so record as ever in FOV
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
                seenByPlayerThisRun = value;
            }
        }

        /// <summary>
        /// Has the square been seen by the player this adventure (since left school)?
        /// </summary>
        public bool SeenByPlayerThisRun
        {
            get
            {
                return seenByPlayerThisRun;
            }
            set
            {
                seenByPlayerThisRun = value;
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

        /// <summary>
        /// Sound magnitude at this square (debug only)
        /// </summary>
        public double SoundMag { get; set; }

        /// <summary>
        /// Sets walkable and non-light blocking
        /// </summary>
        public void SetOpen() {
            BlocksLight = false;
            Walkable = true;
        }

        /// <summary>
        /// Sets non-walkable and light blocking
        /// </summary>
        public void SetBlocking()
        {
            BlocksLight = true;
            Walkable = false;
        }
    }

    public class Map
    {
        public MapSquare[,] mapSquares;
        public int[,] roomIdMap;
        public Point PCStartLocation;

        public int width;
        public int height;

        /// <summary>
        /// Only public for serialization. Pathing maps (always next power of 2)
        /// </summary>
        public uint pathWidth;
        public uint pathHeight;

        /// <summary>
        /// Byte array representation for pathing
        /// </summary>
        public byte[,] pathRepresentation;

        /// <summary>
        /// Byte array representation for pathing. Closed doors appear open (i.e. for creatures which can open doors)
        /// </summary>
        public byte[,] pathRepresentationNoClosedDoors;

        public double LightLevel;

        /// <summary>
        /// Are we guaranteed to be connected?
        /// </summary>
        public bool GuaranteedConnected { get; set; }

        /// <summary>
        /// Constructor for serialization
        /// </summary>
        Map()
        {
            //LightLevel = 1.0;
        }

        /// <summary>
        /// Clone map. Does not clone pathing representation, so this should be refreshed manually.
        /// </summary>
        /// <returns></returns>
        public Map Clone() {

            Map newMap = new Map(width, height);
            newMap.PCStartLocation = PCStartLocation;
            newMap.GuaranteedConnected = GuaranteedConnected;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    newMap.mapSquares[i, j] = mapSquares[i, j].Clone();
                    newMap.roomIdMap[i, j] = roomIdMap[i, j];
                }
            }

            return newMap;
        }


        public Map(int width, int height)
        {
            this.width = width;
            this.height = height;

            LightLevel = 1.0;

            mapSquares = new MapSquare[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    mapSquares[i, j] = new MapSquare();
                }
            }

            roomIdMap = new int[width, height];

            GuaranteedConnected = false;

            //Pad path representations out to nearest power of 2
            pathWidth = Utility.nextPowerOf2(Convert.ToUInt32(width));
            pathHeight = Utility.nextPowerOf2(Convert.ToUInt32(height));

            pathRepresentation = new byte[pathWidth, pathHeight];
            pathRepresentationNoClosedDoors = new byte[pathWidth, pathHeight];

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

        public byte[,] PathRepresentation
        {
            get { return pathRepresentation; }
        }


        public byte[,] PathRepresentationNoClosedDoors
        {
            get { return pathRepresentationNoClosedDoors; }
        }


        /// <summary>
        /// Recalculates the representation of the map used for pathing
        /// </summary>
        public void RecalculatePathingRepresentation()
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < height; k++)
                {
                    pathRepresentation[j, k] = mapSquares[j, k].Walkable ? (byte)1 : (byte)0;
                }
            }

            //Add 0 padding
            for (int j = width; j < pathWidth; j++)
            {
                for (int k = height; k < pathHeight; k++)
                {
                    pathRepresentation[j, k] = 0;
                }
            }

            //Ignoring closed doors

            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < height; k++)
                {
                    MapTerrain terrainHere = mapSquares[j, k].Terrain;

                    pathRepresentationNoClosedDoors[j, k] = mapSquares[j, k].Walkable || terrainHere == MapTerrain.ClosedDoor ? (byte)1 : (byte)0;
                }
            }

            //Add 0 padding
            for (int j = width; j < pathWidth; j++)
            {
                for (int k = height; k < pathHeight; k++)
                {
                    pathRepresentationNoClosedDoors[j, k] = 0;
                }
            }

        }

        /// <summary>
        /// Set walkable flag on map squares. Must be called before RecalculatePathingRepresentation()
        /// </summary>
        public void RecalculateWalkable()
        {

            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < height; k++)
                {

                    //Terrain

                    bool walkable = true;

                    //Use new function

                    if (!Dungeon.IsTerrainWalkable(mapSquares[j, k].Terrain))
                        walkable = false;

                    mapSquares[j, k].Walkable = walkable;
                }
            }
        }
        
    }
}
