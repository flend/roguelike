using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using GraphMap;
using System.Linq;

namespace RogueBasin
{

    /** Immutable Point class */
    public sealed class Point {
        public readonly int x;
        public readonly int y;

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

        public static Point operator *(Point i, int magnitude)
        {
            return new Point(i.x * magnitude, i.y * magnitude);
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

    public enum PathingTerrain
    {
        Walkable,
        ClosedDoor,
        Unwalkable
    }


    public enum FOVTerrain
    {
        NonBlocking,
        Blocking
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

    public class FieldMap<T>
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        T[,] pathingMap;

        public FieldMap(int width, int height)
        {
            pathingMap = new T[width, height];
            Width = width;
            Height = height;
        }

        public T getCell(int x, int y)
        {
            return pathingMap[x, y];
        }

        public void setCell(int x, int y, T terrain)
        {
            pathingMap[x, y] = terrain;
        }
    }

    /** Map of enums that indicate pathing information */
    public class PathingMap : FieldMap<PathingTerrain>
    {

        public PathingMap(int width, int height)
            : base(width, height)
        {

        }
    };

    /** Map of enums that indicate fov information */
    public class FovMap : FieldMap<FOVTerrain>
    {

        public FovMap(int width, int height)
            : base(width, height)
        {

        }
    };

    public class Map
    {
        public MapSquare[,] mapSquares;
        public int[,] roomIdMap;
        public Dictionary<Point, List<Door>> mapSquareLocks;
        public Point PCStartLocation;

        public int width;
        public int height;

        /// <summary>
        /// Only public for serialization. Pathing maps (always next power of 2)
        /// </summary>
        public uint pathWidth;
        public uint pathHeight;

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

        }

        public Dictionary<Point, List<Door>> MapSquareLocks
        {
            get
            {
                return mapSquareLocks;
            }
        }

        public void AddLockedDoorToMap(Point coord, Door newDoor) {
            if(!MapSquareLocks.ContainsKey(coord))
                MapSquareLocks[coord] = new List<Door>();

            MapSquareLocks[coord].Add(newDoor);
        }

        public MapSquare[,] MapSquares
        {
            get
            {
                return mapSquares;
            }
        }

        /// <summary>
        /// Clone map. Does not clone pathing representation, so this should be refreshed manually.
        /// </summary>
        /// <returns></returns>
        public Map Clone() {

            Map newMap = new Map(width, height);
            newMap.PCStartLocation = PCStartLocation;
            newMap.PCStartRoomId = PCStartRoomId;
            newMap.GuaranteedConnected = GuaranteedConnected;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    newMap.mapSquares[i, j] = mapSquares[i, j].Clone();
                    newMap.roomIdMap[i, j] = roomIdMap[i, j];
                    newMap.mapSquareLocks = mapSquareLocks.ToDictionary(entry => entry.Key, entry => new List<Door> (entry.Value));
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
            mapSquareLocks = new Dictionary<Point, List<Door>>();

            GuaranteedConnected = false;

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

        public PathingMap PathRepresentation
        {
            get
            {
                PathingMap pathRepresentation = new PathingMap(width, height);

                for (int j = 0; j < width; j++)
                {
                    for (int k = 0; k < height; k++)
                    {
                        MapTerrain terrainHere = mapSquares[j, k].Terrain;
                        pathRepresentation.setCell(j, k, PathingTerrain.Unwalkable);

                        if (terrainHere == MapTerrain.ClosedDoor)
                            pathRepresentation.setCell(j, k, PathingTerrain.ClosedDoor);
                        else if (Dungeon.IsTerrainWalkable(terrainHere))
                            pathRepresentation.setCell(j, k, PathingTerrain.Walkable);
                    }
                }
                
                return pathRepresentation;
            }
        }

        public FovMap FovRepresentaton
        {
            get
            {
                FovMap newMap = new FovMap(width, height);

                for (int j = 0; j < width; j++)
                {
                    for (int k = 0; k < height; k++)
                    {
                        newMap.setCell(j, k, mapSquares[j, k].BlocksLight ? FOVTerrain.Blocking : FOVTerrain.NonBlocking);
                        //tcodLevel.SetCell(j, k, !level.mapSquares[j, k].BlocksLight, level.mapSquares[j, k].Walkable);
                    }
                }

                return newMap;
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

        /// <summary>
        /// The room id for the player start location
        /// </summary>
        public int PCStartRoomId { get; set; }
    }
}
