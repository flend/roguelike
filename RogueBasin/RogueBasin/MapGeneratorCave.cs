using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    class MapGeneratorCave
    {
        Map baseMap;

        /// <summary>
        /// Chance of digging surrounding squares
        /// </summary>
        public int DiggingChance { get; set; }
        /// <summary>
        /// When connecting stairs, the chance to expand the corridor
        /// </summary>
        public int MineChance { get; set; }
        /// <summary>
        /// How much of the level must be open (not necessarily connected)
        /// </summary>
        public double PercOpenRequired { get; set; }
        /// <summary>
        /// How far away the stairs are guaranteed to be
        /// </summary>
        public double RequiredStairDistance { get; set; }

        public int Width {get; set;}
        public int Height {get; set;}

        Point upStaircase;
        Point downStaircase;

        Point pcStartLocation;

        public bool DoFillInPass = false;
        public int FillInChance = 33;
        public MapTerrain FillInTerrain = MapTerrain.Empty;

        public Map Map { get {return baseMap;} }


        private List<MapTerrain> closedTerrainType;
        private List<MapTerrain> openTerrainType;

        public MapGeneratorCave()
        {
            closedTerrainType = new List<MapTerrain>();
            openTerrainType = new List<MapTerrain>();

            closedTerrainType.Add(MapTerrain.Wall);
            openTerrainType.Add(MapTerrain.Empty);
        }

        public Map GenerateMap() {

            LogFile.Log.LogEntry("Making cave map");

            if (Width < 1 || Height < 1)
            {
                LogFile.Log.LogEntry("Can't make 0 dimension map");
                throw new ApplicationException("Can't make with 0 dimension");
            }

            DiggingChance = 20;
            MineChance = 15;
            PercOpenRequired = 0.4;
            RequiredStairDistance = 40;

            int maxStairConnectAttempts = 10;

            //Since we can't gaurantee connectivity, we will run the map gen many times until we get a map that meets our criteria

            bool badMap = true;
            int mapsMade = 0;

            do
            {

                baseMap = new Map(Width, Height);
                mapsMade++;

                //Fill map with walls
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        SetSquareClosed(i, j);
                    }
                }

                //Start digging from a random point
                int noDiggingPoints = 4 + Game.Random.Next(4);

                for (int i = 0; i < noDiggingPoints; i++)
                {
                    int x = Game.Random.Next(Width);
                    int y = Game.Random.Next(Height);

                    //Don't dig right to the edge
                    if (x == 0)
                        x = 1;
                    if (x == Width - 1)
                        x = Width - 2;
                    if (y == 0)
                        y = 1;
                    if (y == Height - 1)
                        y = Height - 2;

                    Dig(x, y);
                }

                //Check if we are too small, and add more digs
                while (CalculatePercentageOpen() < PercOpenRequired)
                {
                    int x = Game.Random.Next(Width);
                    int y = Game.Random.Next(Height);

                    //Don't dig right to the edge
                    if (x == 0)
                        x = 1;
                    if (x == Width - 1)
                        x = Width - 2;
                    if (y == 0)
                        y = 1;
                    if (y == Height - 1)
                        y = Height - 2;

                    Dig(x, y);
                }

                //Find places for the stairs

                //We will attempt this several times before we give up and redo the whole map
                int attempts = 0;

                do
                {
                    double stairDistance;
                    
                    do
                    {
                        upStaircase = RandomPoint();
                        downStaircase = RandomPoint();

                        stairDistance = Math.Sqrt(Math.Pow(upStaircase.x - downStaircase.x, 2) + Math.Pow(upStaircase.y - downStaircase.y, 2));

                    } while (stairDistance < RequiredStairDistance);

                    //If the stairs aren't connected, try placing them in different random spots, by relooping
                                       
                    if (ArePointsConnected(upStaircase, downStaircase))
                    {
                        badMap = false;
                        break;
                    }

                    attempts++;
                } while (attempts < maxStairConnectAttempts);

            } while (badMap);

            //If requested do a final pass and fill in with some interesting terrain
            //Fill map with walls
            if (DoFillInPass)
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        if (openTerrainType.Contains(baseMap.mapSquares[i, j].Terrain))
                        {
                            if (Game.Random.Next(100) < FillInChance)
                                baseMap.mapSquares[i, j].Terrain = FillInTerrain;
                        }
                    }
                }
            }
            
            //Caves are not guaranteed connected
            baseMap.GuaranteedConnected = false;

            //Set the player start location to that of the up staircase (only used on the first level)
            pcStartLocation = upStaircase;

            LogFile.Log.LogEntry("Total maps made: " + mapsMade.ToString());

            return baseMap;
        }

        /// <summary>
        /// Add g + rand(noRandom) water features
        /// 15, 4 is good
        /// </summary>
        /// <param name="noGuaranteed"></param>
        /// <param name="noRandom"></param>
        public void AddWaterToCave(int noGuaranteed, int noRandom)
        {
            //Add some water features
            int noWaterFeatures = noGuaranteed + Game.Random.Next(noRandom);

            //Guarantee water features starting from the upstaircase
            for (int i = 0; i < 2; i++)
            {
                int deltaX = Game.Random.Next(3) - 1;
                int deltaY = Game.Random.Next(3) - 1;

                AddWaterFeature(upStaircase.x, upStaircase.y, deltaX, deltaY);
            }

            for (int i = 0; i < noWaterFeatures; i++)
            {
                int x, y;

                //Loop until we find an empty place to start
                do
                {
                    x = Game.Random.Next(Width);
                    y = Game.Random.Next(Height);
                } while (!openTerrainType.Contains( baseMap.mapSquares[x, y].Terrain ));

                int deltaX = Game.Random.Next(3) - 1;
                int deltaY = Game.Random.Next(3) - 1;

                AddWaterFeature(x, y, deltaX, deltaY);
            }
        }

        /// <summary>
        /// Call before adding list of new terrain types
        /// </summary>
        public void ResetClosedSquareTerrainType()
        {
            closedTerrainType.Clear();
        }

        /// <summary>
        /// Call before adding list of new terrain types
        /// </summary>
        public void ResetOpenSquareTerrainType()
        {
            openTerrainType.Clear();
        }

        public void SetClosedSquareTerrainType(MapTerrain type)
        {
            closedTerrainType.Add(type);
        }

        public void SetOpenSquareTerrainType(MapTerrain type)
        {
            openTerrainType.Add(type);
        }

        private void SetSquareClosed(int i, int j)
        {
            int noClosedTerrain = closedTerrainType.Count;

            baseMap.mapSquares[i, j].Terrain = closedTerrainType[Game.Random.Next(noClosedTerrain)];
            baseMap.mapSquares[i, j].BlocksLight = true;
            baseMap.mapSquares[i, j].Walkable = false;
        }

        private void SetSquareOpen(int i, int j)
        {
            int noOpenTerrain = openTerrainType.Count;

            baseMap.mapSquares[i, j].Terrain = openTerrainType[Game.Random.Next(noOpenTerrain)];
            baseMap.mapSquares[i, j].BlocksLight = false;
            baseMap.mapSquares[i, j].Walkable = true;
        }

        private double CalculatePercentageOpen()
        {
            int totalOpen = 0;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (openTerrainType.Contains(baseMap.mapSquares[i, j].Terrain))
                        totalOpen++;
                }
            }

            double percOpen = totalOpen / (double)(Width * Height);

            return percOpen;
        }

        private bool ArePointsConnected(Point firstPoint, Point secondPoint)
        {
            //Build map representations
            PathingMap map = new PathingMap(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    map.setCell(i, j, baseMap.mapSquares[i, j].Walkable ? PathingTerrain.Walkable : PathingTerrain.Unwalkable);
                }
            }

            //Try to walk a path between the 2 staircases
            LibTCOD.TCODPathFindingWrapper pathFinder = new LibTCOD.TCODPathFindingWrapper();
            pathFinder.updateMap(0, map);
            return pathFinder.arePointsConnected(0, firstPoint, secondPoint, Pathing.PathingPermission.Normal);
        }

        private void ConnectPoints(Point upStairsPoint, Point downStairsPoint)
        {
            //First check if the stairs are connected... 
            if (ArePointsConnected(upStaircase, downStaircase))
                return;

            //If not, open a path between the staircases

            foreach (Point p in Utility.GetPointsOnLine(upStairsPoint, downStairsPoint))
            {

                int nextX = p.x;
                int nextY = p.y;

                Random rand = Game.Random;

                SetSquareOpen(nextX, nextY);

                //Chance surrounding squares also get done
                if (nextX - 1 > 0 && nextY - 1 > 0)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX - 1, nextY - 1);
                    }
                }

                if (nextY - 1 > 0)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX, nextY - 1);
                    }
                }

                if (nextX + 1 < Width && nextY - 1 > 0)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX + 1, nextY - 1);
                    }
                }


                if (nextX - 1 > 0)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX - 1, nextY);
                    }
                }

                if (nextX + 1 < Width)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX + 1, nextY);
                    }
                }

                if (nextX - 1 > 0 && nextY + 1 < Height)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX - 1, nextY + 1);
                    }
                }

                if (nextY + 1 < Height)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX, nextY + 1);
                    }
                }

                if (nextX + 1 < Width && nextY + 1 < Height)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX + 1, nextY + 1);
                    }
                }

            }

        }
    
        private Point RandomPoint()
        {
            do
            {
                int x = Game.Random.Next(Width);
                int y = Game.Random.Next(Height);

                if (openTerrainType.Contains(baseMap.mapSquares[x, y].Terrain))
                {
                    return new Point(x, y);
                }
            }
            while (true);
        }

        public void Dig(int x, int y)
        {
            //Check this is a valid square to dig
            if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                return;

            //Already dug
            if (openTerrainType.Contains(baseMap.mapSquares[x, y].Terrain))
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

        private void AddWaterFeature(int x, int y, int deltaX, int deltaY)
        {
            //Calculate this square's coords
            int squareX = x + deltaX;
            int squareY = y + deltaY;

            //Check this is a valid square to operate on
            if (squareX == 0 || squareY == 0 || squareX == Width - 1 || squareY == Height - 1)
                return;

            //Already water
            //if (baseMap.mapSquares[x, y].Terrain == MapTerrain.Empty)
            //    return;

            //If this square is empty it becomes water
            if (baseMap.mapSquares[x, y].Terrain == MapTerrain.Empty)
            {
                baseMap.mapSquares[x, y].Terrain = MapTerrain.Flooded;
            }

            //We are most likely to continue on the way we are going

            int chance = Game.Random.Next(100);

            if (chance < 75)
            {
                x += deltaX;
                y += deltaY;

                AddWaterFeature(x, y, deltaX, deltaY);
                return;
            }

            //Chance we'll change direction
            else if (chance < 98)
            {
                deltaX = Game.Random.Next(3) - 1;
                deltaY = Game.Random.Next(3) - 1;

                AddWaterFeature(x, y, deltaX, deltaY);
                return;
            }
            //Otherwise we stop
            else
            {
                return;
            }
        }

        /// <summary>
        /// Add staircases to the map once it has been added to dungeon
        /// </summary>
        /// <param name="levelNo"></param>
        public void AddStaircases(int levelNo) {

            Game.Dungeon.AddFeature(new Features.StaircaseUp(), levelNo, upStaircase);
            Game.Dungeon.AddFeature(new Features.StaircaseDown(), levelNo, downStaircase);
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
        /// Add staircases to the map once it has been added to dungeon
        /// </summary>
        /// <param name="levelNo"></param>
        public void AddUpStaircaseOnly(int levelNo)
        {
            Game.Dungeon.AddFeature(new Features.StaircaseUp(), levelNo, upStaircase);
            //Game.Dungeon.AddFeature(new Features.StaircaseDown(), levelNo, downStaircase);
        }

        /// <summary>
        /// Add an exit staircase at the up staircase location
        /// </summary>
        internal void AddExitStaircaseOnly(int levelNo)
        {
            Game.Dungeon.AddFeature(new Features.StaircaseExit(levelNo), levelNo, upStaircase);
        }

        /// <summary>
        /// Only used on the first level
        /// </summary>
        /// <returns></returns>
        public Point GetPCStartLocation()
        {
            return pcStartLocation;
        }


    }
}
