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

        public Map Map { get {return baseMap;} }

        public MapGeneratorCave()
        {

        }

        public Map GenerateMap(bool isFirstLevel) {

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

            for (int i = 0; i < noWaterFeatures; i++)
            {
                int x, y;

                //Loop until we find an empty place to start
                do
                {
                    x = Game.Random.Next(Width);
                    y = Game.Random.Next(Height);
                } while (baseMap.mapSquares[x, y].Terrain != MapTerrain.Empty);

                int deltaX = Game.Random.Next(3) - 1;
                int deltaY = Game.Random.Next(3) - 1;

                AddWaterFeature(x, y, deltaX, deltaY);
            }
        }

        private void SetSquareClosed(int i, int j)
        {
            baseMap.mapSquares[i, j].Terrain = MapTerrain.Wall;
            baseMap.mapSquares[i, j].BlocksLight = true;
            baseMap.mapSquares[i, j].Walkable = false;
        }

        private void SetSquareOpen(int i, int j)
        {
            baseMap.mapSquares[i, j].Terrain = MapTerrain.Empty;
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
                    if (baseMap.mapSquares[i, j].Terrain == MapTerrain.Empty)
                        totalOpen++;
                }
            }

            double percOpen = totalOpen / (double)(Width * Height);

            return percOpen;
        }

        private bool ArePointsConnected(Point firstPoint, Point secondPoint)
        {
            //Build tcodmap
            TCODFov tcodMap = new TCODFov(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    tcodMap.SetCell(i, j, !baseMap.mapSquares[i, j].BlocksLight, baseMap.mapSquares[i, j].Walkable);
                }
            }

            //Try to walk the path between the 2 staircases
            TCODPathFinding path = new TCODPathFinding(tcodMap, 1.0);
            path.ComputePath(firstPoint.x, firstPoint.y, secondPoint.x, secondPoint.y);

            //Find the first step. We need to load x and y with the origin of the path
            int x = firstPoint.x;
            int y = firstPoint.y;

            bool obstacleHit = false;

            //If there's no routeable path
            if (path.IsPathEmpty())
            {
                obstacleHit = true;
            }

            path.Dispose();
            tcodMap.Dispose();

            return (!obstacleHit);
        }

        private void ConnectPoints(Point upStairsPoint, Point downStairsPoint)
        {
            //First check if the stairs are connected... 
            if (ArePointsConnected(upStaircase, downStaircase))
                return;

            //If not, open a path between the staircases

            TCODLineDrawing.InitLine(upStairsPoint.x, upStairsPoint.y, downStairsPoint.x, downStairsPoint.y);

            int nextX = upStairsPoint.x;
            int nextY = upStairsPoint.y;

            Random rand = Game.Random;

            do
            {
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

            } while (!TCODLineDrawing.StepLine(ref nextX, ref nextY));

        }
    
        private Point RandomPoint()
        {
            do
            {
                int x = Game.Random.Next(Width);
                int y = Game.Random.Next(Height);

                if (baseMap.mapSquares[x, y].Terrain == MapTerrain.Empty)
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
            if (baseMap.mapSquares[x, y].Terrain == MapTerrain.Empty)
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
        /// Only used on the first level
        /// </summary>
        /// <returns></returns>
        public Point GetPCStartLocation()
        {
            return pcStartLocation;
        }
    }
}
