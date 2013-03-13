using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    class MapGeneratorBSPCave : MapGenerator
    {
        int width = 40;
        int height = 40;

        int DiggingChance { get; set; }

        public static Random rand;

        Map baseMap;

        MapNode rootNode;

        List<MapTerrain> rubbleType;
        List<MapTerrain> possibleEmptyTypes;

        Point upStaircase;
        Point downStaircase;

        /// <summary>
        /// Alternative wall types. Normal walls get converted into these in a final pass
        /// </summary>
        List<MapTerrain> wallType;

        public MapGeneratorBSPCave()
        {
            rubbleType = new List<MapTerrain>();
            possibleEmptyTypes = new List<MapTerrain>();
            possibleEmptyTypes.Add(MapTerrain.Corridor);

            wallType = new List<MapTerrain>();
        }

        static MapGeneratorBSPCave()
        {
            rand = new Random();
        }

        public void ClearWallType()
        {
            wallType.Clear();
        }

        public void AddWallType(MapTerrain terrain)
        {
            wallType.Add(terrain);
        }

        public void AddRubbleType(MapTerrain terrain)
        {
            rubbleType.Add(terrain);
            possibleEmptyTypes.Add(terrain);
        }

        public void ClearRubbleType()
        {
            rubbleType.Clear();
            possibleEmptyTypes.Clear();
            possibleEmptyTypes.Add(MapTerrain.Corridor);
        }

        public Map GenerateMap(int extraConnections)
        {
            LogFile.Log.LogEntry(String.Format("Generating BSPCave dungeon"));

            baseMap = new Map(width, height);

            //BSP is always connected
            baseMap.GuaranteedConnected = true;

            //Make a BSP tree for the rooms

            rootNode = new MapNode(0, 0, width, height);
            rootNode.Split();
            
            //Draw a room in each BSP leaf
            rootNode.DrawRoomAtLeaf(baseMap);

            //debug
            //Screen.Instance.DrawMapDebug(baseMap);

            //Draw connecting corridors
            rootNode.DrawCorridorConnectingChildren(baseMap);

            //Add any extra connecting corridors as specified

            for (int i = 0; i < extraConnections; i++)
            {
                rootNode.AddRandomConnection(baseMap);
            }

            //Add doors where single corridors terminate into rooms
            AddDoors();

            //Turn corridors into normal squares and surround with walls
            CorridorsIntoRooms();

            //Now fill all void with walls

            //Fill the map with walls
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (baseMap.mapSquares[i, j].Terrain == MapTerrain.Void)
                    {
                        baseMap.mapSquares[i, j].Terrain = MapTerrain.Wall;
                    }
                }
            }

            //Work out where the staircases will be

            //We just want 2 places that aren't too close to each other. The map is guaranteed connected
            double RequiredStairDistance = (width * 0.5);
            double stairDistance;

            do
            {
                upStaircase = RandomWalkablePoint();
                downStaircase = RandomWalkablePoint();

                stairDistance = Math.Sqrt(Math.Pow(upStaircase.x - downStaircase.x, 2) + Math.Pow(upStaircase.y - downStaircase.y, 2));

            } while (stairDistance < RequiredStairDistance);

            //Set which squares are light blocking
            //Now done during creation
            //SetLightBlocking(baseMap);

            //Set the PC start location in a random room
            baseMap.PCStartLocation = rootNode.RandomRoomPoint().GetPointInRoomOnly();

            //Now we use the cave algorithm to eat the map
            //Instead of setting this Empty like in cave, set them to Corridor temporarily (so the algo knows where it's been)

            DiggingChance = 22;

            //Start digging from a random point
            int noDiggingPoints = 6 + Game.Random.Next(2);

            for (int i = 0; i < noDiggingPoints; i++)
            {
                int x = Game.Random.Next(width);
                int y = Game.Random.Next(height);

                //Don't dig right to the edge
                if (x == 0)
                    x = 1;
                if (x == width - 1)
                    x = width - 2;
                if (y == 0)
                    y = 1;
                if (y == height - 1)
                    y = height - 2;

                Dig(x, y);
            }

            //Turn the corridors into empty
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (baseMap.mapSquares[i, j].Terrain == MapTerrain.Corridor)
                    {
                        baseMap.mapSquares[i, j].Terrain = MapTerrain.Empty;
                    }
                }
            }

            //Do a final pass to convert Wall into something more exciting
            if (wallType.Count > 0)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (baseMap.mapSquares[i, j].Terrain == MapTerrain.Wall)
                        {
                            baseMap.mapSquares[i, j].Terrain = wallType[rand.Next(wallType.Count)];
                        }
                    }
                }
            }

            return baseMap;
        }

        public int RubbleChance = 100;

        /// <summary>
        /// Used by dig to open up a square
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void SetSquareOpen(int i, int j)
        {
            if (rubbleType.Count > 0 && rand.Next(100) < RubbleChance)
            {
                baseMap.mapSquares[i, j].Terrain = rubbleType[rand.Next(rubbleType.Count)];
            }
            else
            {
                baseMap.mapSquares[i, j].Terrain = MapTerrain.Corridor;
            }
            baseMap.mapSquares[i, j].BlocksLight = false;
            baseMap.mapSquares[i, j].Walkable = true;
        }

        public void Dig(int x, int y)
        {
            //Check this is a valid square to dig
            if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                return;

            //Already dug
            if (possibleEmptyTypes.Contains(baseMap.mapSquares[x, y].Terrain))
                return;

            //Set this as open
            SetSquareOpen(x, y);

            //Did in all the directions

            Random rand = Game.Random;

            //TL
            if (rand.Next(100) < DiggingChance)
                Dig(x - 1, y - 1);
            //T
            if (rand.Next(100) < DiggingChance)
                Dig(x, y - 1);
            //TR
            if (rand.Next(100) < DiggingChance)
                Dig(x + 1, y - 1);
            //CL
            if (rand.Next(100) < DiggingChance)
                Dig(x - 1, y);
            //CR
            if (rand.Next(100) < DiggingChance)
                Dig(x + 1, y);
            //BL
            if (rand.Next(100) < DiggingChance)
                Dig(x - 1, y + 1);
            //B
            if (rand.Next(100) < DiggingChance)
                Dig(x, y + 1);
            //BR
            if (rand.Next(100) < DiggingChance)
                Dig(x + 1, y + 1);
        }

        /// <summary>
        /// Add doors at the end of single corridors into rooms (don't do so with double or triple corridors,
        /// so we have to do this in a final pass)
        /// </summary>
        private void AddDoors()
        {
            int doorChanceMax = 5;
            int closedDoorChance = 2;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (baseMap.mapSquares[i, j].Terrain == MapTerrain.Corridor)
                    {
                        //North door
                        //Check there is room first
                        if (j - 1 >= 0 && i - 1 >= 0 && i + 1 < width)
                        {
                            //We need to check for the surrounding walls as well as the gap to avoid multi-corridors having multi-doors
                            if (baseMap.mapSquares[i, j - 1].Terrain == MapTerrain.Empty &&
                                baseMap.mapSquares[i - 1, j].Terrain == MapTerrain.Wall &&
                                baseMap.mapSquares[i + 1, j].Terrain == MapTerrain.Wall)
                            {
                                if (Game.Random.Next(doorChanceMax) < closedDoorChance)
                                {
                                    baseMap.mapSquares[i, j].Terrain = MapTerrain.ClosedDoor;
                                    baseMap.mapSquares[i, j].SetBlocking();
                                }
                                else
                                {
                                    baseMap.mapSquares[i, j].Terrain = MapTerrain.OpenDoor;
                                    baseMap.mapSquares[i, j].SetOpen();
                                }
                            }
                        }
                        //South door
                        if (j + 1 < height && i - 1 >= 0 && i + 1 < width)
                        {
                            if (baseMap.mapSquares[i, j + 1].Terrain == MapTerrain.Empty &&
                                baseMap.mapSquares[i - 1, j].Terrain == MapTerrain.Wall &&
                                baseMap.mapSquares[i + 1, j].Terrain == MapTerrain.Wall)
                            {
                                if (Game.Random.Next(doorChanceMax) < closedDoorChance)
                                {
                                    baseMap.mapSquares[i, j].Terrain = MapTerrain.ClosedDoor;
                                    baseMap.mapSquares[i, j].SetBlocking();
                                }
                                else
                                {
                                    baseMap.mapSquares[i, j].Terrain = MapTerrain.OpenDoor;
                                    baseMap.mapSquares[i, j].SetOpen();
                                }
                            }
                        }

                        //West door
                        if (i - 1 >= 0 && j - 1 >= 0 && j + 1 < height)
                        {
                            if (baseMap.mapSquares[i - 1, j].Terrain == MapTerrain.Empty &&
                                baseMap.mapSquares[i, j + 1].Terrain == MapTerrain.Wall &&
                                baseMap.mapSquares[i, j - 1].Terrain == MapTerrain.Wall)
                            {
                                if (Game.Random.Next(doorChanceMax) < closedDoorChance)
                                {
                                    baseMap.mapSquares[i, j].Terrain = MapTerrain.ClosedDoor;
                                    baseMap.mapSquares[i, j].SetBlocking();
                                }
                                else
                                {
                                    baseMap.mapSquares[i, j].Terrain = MapTerrain.OpenDoor;
                                    baseMap.mapSquares[i, j].SetOpen();
                                }
                            }
                        }

                        //East door
                        if (i + 1 < width && j - 1 >= 0 && j + 1 < height)
                        {
                            if (baseMap.mapSquares[i + 1, j].Terrain == MapTerrain.Empty &&
                                baseMap.mapSquares[i, j + 1].Terrain == MapTerrain.Wall &&
                                baseMap.mapSquares[i, j - 1].Terrain == MapTerrain.Wall)
                            {
                                if (Game.Random.Next(doorChanceMax) < closedDoorChance)
                                {
                                    baseMap.mapSquares[i, j].Terrain = MapTerrain.ClosedDoor;
                                    baseMap.mapSquares[i, j].SetBlocking();
                                }
                                else
                                {
                                    baseMap.mapSquares[i, j].Terrain = MapTerrain.OpenDoor;
                                    baseMap.mapSquares[i, j].SetOpen();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Turn corridors into little rooms
        /// </summary>
        private void CorridorsIntoRooms()
        {
            //Surround each corridor edge with walls
            //i.e. fill any adjacent (inc diagonals) void with walls
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (baseMap.mapSquares[i, j].Terrain == MapTerrain.Corridor)
                    {
                        int tx = i - 1;
                        int ty = j - 1;
                        int bx = i + 1;
                        int by = j + 1;

                        if (tx >= 0 && ty >= 0 &&
                            baseMap.mapSquares[tx, ty].Terrain == MapTerrain.Void)
                        {
                            baseMap.mapSquares[tx, ty].Terrain = MapTerrain.Wall;
                        }
                        if (ty >= 0 &&
                            baseMap.mapSquares[i, ty].Terrain == MapTerrain.Void)
                        {
                            baseMap.mapSquares[i, ty].Terrain = MapTerrain.Wall;
                        }
                        if (ty >= 0 && bx < width &&
                            baseMap.mapSquares[bx, ty].Terrain == MapTerrain.Void)
                        {
                            baseMap.mapSquares[bx, ty].Terrain = MapTerrain.Wall;
                        }
                        if (tx >= 0 &&
                            baseMap.mapSquares[tx, j].Terrain == MapTerrain.Void)
                        {
                            baseMap.mapSquares[tx, j].Terrain = MapTerrain.Wall;
                        }
                        if (bx < width &&
                            baseMap.mapSquares[bx, j].Terrain == MapTerrain.Void)
                        {
                            baseMap.mapSquares[bx, j].Terrain = MapTerrain.Wall;
                        }
                        if (by < height && tx >= 0 &&
                            baseMap.mapSquares[tx, by].Terrain == MapTerrain.Void)
                        {
                            baseMap.mapSquares[tx, by].Terrain = MapTerrain.Wall;
                        }
                        if (by < height &&
                            baseMap.mapSquares[i, by].Terrain == MapTerrain.Void)
                        {
                            baseMap.mapSquares[i, by].Terrain = MapTerrain.Wall;
                        }
                        if (by < height && bx < width &&
                            baseMap.mapSquares[bx, by].Terrain == MapTerrain.Void)
                        {
                            baseMap.mapSquares[bx, by].Terrain = MapTerrain.Wall;
                        }
                    }
                }
            }

            //Now turn all corridors into normal floor
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (baseMap.mapSquares[i, j].Terrain == MapTerrain.Corridor)
                    {
                        baseMap.mapSquares[i, j].Terrain = MapTerrain.Empty;
                    }
                }
            }
        }

        private void SetLightBlocking(Map baseMap)
        {
            foreach (MapSquare square in baseMap.mapSquares)
            {
                if (square.Terrain == MapTerrain.Empty)
                {
                    square.BlocksLight = false;
                }
                else
                {
                    square.BlocksLight = true;
                }
            }
        }

        /// <summary>
        /// Returns only points in rooms (not corridors)
        /// </summary>
        /// <returns></returns>
        public override PointInRoom RandomPointInRoom()
        {
            return rootNode.RandomRoomPoint();
        }

        /// <summary>
        /// Returns a point anywhere the terrain is empty
        /// </summary>
        /// <returns></returns>
        public override Point RandomWalkablePoint()
        {
            do
            {
                int x = Game.Random.Next(width);
                int y = Game.Random.Next(height);

                if (baseMap.mapSquares[x, y].Terrain == MapTerrain.Empty || baseMap.mapSquares[x, y].Terrain == MapTerrain.Corridor)
                {
                    return new Point(x, y);
                }
            }
            while (true);
        }

        public int Width
        {
            set
            {
                width = value;
            }
        }
        public int Height
        {
            set
            {
                height = value;
            }
        }
        
        public Point GetUpStaircaseLocation()
        {
            return upStaircase;
        }

        /// <summary>
        /// Add staircases to the map once it has been added to dungeon
        /// </summary>
        /// <param name="levelNo"></param>
        public void AddStaircases(int levelNo)
        {
            Game.Dungeon.AddFeature(new Features.StaircaseUp(), levelNo, upStaircase);
            Game.Dungeon.AddFeature(new Features.StaircaseDown(), levelNo, downStaircase);
        }

        /// <summary>
        /// Add staircases to the map once it has been added to dungeon
        /// </summary>
        /// <param name="levelNo"></param>
        public void AddUpStaircaseOnly(int levelNo)
        {
            Game.Dungeon.AddFeature(new Features.StaircaseUp(), levelNo, upStaircase);
            //Game.Dungeon.AddFeature(new Features.StaircaseDown(), levelNo, downStaircase);
        }

        /// <summary>
        /// Add staircases to the map once it has been added to dungeon
        /// </summary>
        /// <param name="levelNo"></param>
        public void AddDownStaircaseOnly(int levelNo)
        {
            //Game.Dungeon.AddFeature(new Features.StaircaseUp(), levelNo, upStaircase);
            Game.Dungeon.AddFeature(new Features.StaircaseDown(), levelNo, downStaircase);
        }
        
        /// <summary>
        /// Add an exit staircase at the up staircase location
        /// </summary>
        internal void AddExitStaircaseOnly(int levelNo)
        {
            Game.Dungeon.AddFeature(new Features.StaircaseExit(levelNo), levelNo, upStaircase);
        }
    }
}
