﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    class MapNode
    {
        //BSP tl corner in map coords
        int x;
        int y;

        //BSP square dimensions
        int width;
        int height;

        //Room coords
        int roomX;
        int roomY;

        int roomWidth;
        int roomHeight;

        //Square split parameters
        SplitType split;
        int actualSplit;

        //Children references
        MapNode childLeft;
        MapNode childRight;

        //TUNING PARAMETERS

        //Minimum BSP square sizes
        const int minBSPSquareWidth = 8;
        const int minBSPSquareHeight = 5;

        //Minimum room sizes. Below 3 will break the algorithm (and make unwalkable rooms)
        const int minRoomWidth = 4;
        const int minRoomHeight = 4;

        //Smaller numbers make larger areas more likely
        //Numbers 5 or below make a significant difference
        const int noSplitChance = 5;
        //Multiple of BSPwidth above which we must split
        const int mustSplitSize = 3;

        //How the BSP squares are split as a ratio
        const double minimumSplit = 0.4;
        const double maximumSplit = 0.6;

        //How much of the BSP square is filled by a room
        const double minFill = 0.7;
        const double maxFill = 1.0;
        
        //Tree depth counter
        int treeDepth;
        bool newConnectionMade;

        public MapNode(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        enum SplitType {
            Horizontal, Vertical
        }

        //Find a random point within a room
        public Point RandomRoomPoint()
        {
            return FindRandomRoomPoint();
        }

        private Point FindRandomRoomPoint()
        {
            //Go down the tree to a random left. When there find a random point in the room and return it
            Random rand = MapGeneratorBSP.rand;
            
            Point retPoint = new Point(-1, -1);

            //If we have both children choose one
            if (childLeft != null && childRight != null)
            {

                if (rand.Next(2) < 1)
                {
                    retPoint = childLeft.RandomRoomPoint();
                }
                else
                {
                    retPoint = childRight.RandomRoomPoint();
                }
            }
            //Otherwise do the one we have
            else
            {
                if (childLeft != null)
                {
                    retPoint = childLeft.RandomRoomPoint();
                }

                else
                {
                    if (childRight != null)
                    {
                        retPoint = childRight.RandomRoomPoint();
                    }
                }
            }

            //If it's the first leaf we've come to 
            //Find point in the room
            if (retPoint.x == -1)
            {

                int x = roomX + 1 + rand.Next(roomWidth - 2);
                int y = roomY + 1 + rand.Next(roomHeight - 2);

                return new Point(x, y);
            }

            return retPoint;
        }

        public void Split() {

            Random rand = MapGeneratorBSP.rand;
            split = SplitType.Vertical;

            //Long thin areas are more likely to be split widthways and vice versa
            if(rand.Next(width + height) < width) {
                split = SplitType.Horizontal;
            }

            if (split == SplitType.Horizontal)
            {
                //Small chance that we don't recurse any further
                int chanceNoSplitHoriz = mustSplitSize - (width / minBSPSquareWidth);
                if (rand.Next(noSplitChance) < chanceNoSplitHoriz)
                {
                    childLeft = null;
                    childRight = null;
                    return;
                }

                int minSplit = (int)(width * minimumSplit);
                int maxSplit = (int)(width * maximumSplit);

                actualSplit = minSplit + rand.Next(maxSplit - minSplit + 1);

                //Define the two child areas, make objects and then recursively split them

                if (actualSplit < minBSPSquareWidth)
                {
                    childLeft = null;
                }
                else
                {
                    childLeft = new MapNode(x, y, actualSplit, height);
                    childLeft.Split();
                }

                if (width - actualSplit < minBSPSquareWidth)
                {
                    childRight = null;
                }
                else
                {
                    childRight = new MapNode(x + actualSplit, y, width - actualSplit, height);
                    childRight.Split();
                }
            }
            else {
                //SplitType.Vertical

                //Small chance that we don't recurse any further
                int chanceNoSplitVert = mustSplitSize - (height / minBSPSquareHeight);
                if (rand.Next(noSplitChance) < chanceNoSplitVert)
                {
                    childLeft = null;
                    childRight = null;
                }

                int minSplit = (int)(height * minimumSplit);
                int maxSplit = (int)(height * maximumSplit);

                actualSplit = minSplit + rand.Next(maxSplit - minSplit + 1);

                //Define the two child areas, make objects and then recursively split them

                if (actualSplit < minBSPSquareHeight)
                {
                    childLeft = null;
                }
                else
                {
                    childLeft = new MapNode(x, y, width, actualSplit);
                    childLeft.Split();
                }

                if (height - actualSplit < minBSPSquareHeight)
                {
                    childRight = null;
                }
                else
                {
                    childRight = new MapNode(x, y + actualSplit, width, height - actualSplit);
                    childRight.Split();
                }
            }
        }
            public void DrawRoomAtLeaf(Map baseMap) {

                //Check we are a leaf
                if(childLeft != null) {
                    childLeft.DrawRoomAtLeaf(baseMap);
                }
                if(childRight != null) {
                    childRight.DrawRoomAtLeaf(baseMap);
                }

                //Only actually draw the room if both childLeft and childRight are null
                if(childLeft == null && childRight == null) {
                    DrawRoom(baseMap);
                }
            }
            
        public void DrawRoom(Map baseMap) {
            
            Random rand = MapGeneratorBSP.rand;
            //Width and height are reduced by 1 from max filling to ensure there is always a free column / row for an L-shaped corridor
            roomWidth = (int)(width * minFill + rand.Next((int) ( (width * maxFill) - (width * minFill)) ));
            roomHeight = (int)(height * minFill + rand.Next((int) ( (height * maxFill) - (height * minFill) ) ));

            if(width <= minRoomWidth) {
                throw new ApplicationException("BSP too small for room");
            }
            if(height <= minRoomHeight) {
                throw new ApplicationException("BSP too small for room");
            }

            if(roomWidth < minRoomWidth)
                roomWidth = minRoomWidth;

            if (roomHeight < minRoomHeight)
                roomHeight = minRoomHeight;

            /*if (roomWidth < MapGeneratorBSP.minimumRoomSize)
                roomWidth = MapGeneratorBSP.minimumRoomSize;

            if (roomHeight < MapGeneratorBSP.minimumRoomSize)
                roomHeight = MapGeneratorBSP.minimumRoomSize;*/

            roomX = x + 1 + rand.Next(width - roomWidth);
            int rx = roomX + roomWidth - 1;
            roomY = y + 1 + rand.Next(height - roomHeight);
            int by = roomY + roomHeight - 1;

            //Walls

            for (int i = roomX; i <= rx; i++)
            {
                //Top row
                baseMap.mapSquares[i, roomY].Terrain = MapTerrain.Wall;
                //Bottom row
                baseMap.mapSquares[i, by].Terrain = MapTerrain.Wall;
            }

            for (int i = roomY; i <= by; i++)
            {
                //Left row
                baseMap.mapSquares[roomX, i].Terrain = MapTerrain.Wall;
                //Right row
                baseMap.mapSquares[rx, i].Terrain = MapTerrain.Wall;
            }

            //Inside
            //Set as empty
            for (int i = roomX + 1; i < rx; i++)
            {
                for (int j = roomY + 1; j < by; j++)
                {
                    baseMap.mapSquares[i, j].Terrain = MapTerrain.Empty;
                    baseMap.mapSquares[i, j].SetOpen();
                }
            }
        }

        /// <summary>
        /// Add an extra connecting corridor somewhere on the map. Returns whether a new connection was drawn (may fail due to randomness, in which case retry)
        /// </summary>
        /// <param name="baseMap"></param>
        public bool AddRandomConnection(Map baseMap)
        {
            treeDepth = 0;
            newConnectionMade = false;
            AddExtraConnectionBetweenChildren(baseMap);

            return newConnectionMade;
        }

        public void AddExtraConnectionBetweenChildren(Map baseMap) {

            //Wander down the tree to a leaf, keeping track of the number of nodes with 2 children (that could be possible connections)
            //When we come back down the tree, try to get one of the 2 children nodes to draw a connection

            if (childLeft != null && childRight != null)
            {
                treeDepth++;

                //Pick one node at random
                if(MapGeneratorBSP.rand.Next(2) < 1) {
                    childLeft.AddExtraConnectionBetweenChildren(baseMap);
                }
                else {
                    childRight.AddExtraConnectionBetweenChildren(baseMap);
                }

                //We reach here after we have hit the leaf
                //If we haven't made a connection yet there's a chance we will connect our children

                if (newConnectionMade == false &&
                MapGeneratorBSP.rand.Next(treeDepth) < 1)
                {
                    newConnectionMade = true;

                    //Draw a connecting corridor between our two children
                    DrawConnectingCorriderBetweenChildren(baseMap);
                }
            }
            else if(childLeft != null) {
                childLeft.AddExtraConnectionBetweenChildren(baseMap);
            }
            else if(childRight != null) {
                childRight.AddExtraConnectionBetweenChildren(baseMap);
            }
        }

        public void DrawCorridorConnectingChildren(Map baseMap)
        {
            //Children should do their own drawing first
            //However, if we don't have children, return to our parent

            if (childLeft != null)
                childLeft.DrawCorridorConnectingChildren(baseMap);

            if (childRight != null) 
                childRight.DrawCorridorConnectingChildren(baseMap);

            //If we only have 1 child we can't connect them, but our parent will connect to them
            if (childLeft == null || childRight == null)
                return;

            //Draw a connecting corridor between our two children
            DrawConnectingCorriderBetweenChildren(baseMap);
        }

        private void DrawConnectingCorriderBetweenChildren(Map baseMap)
        {
            Random rand = MapGeneratorBSP.rand;

            if (split == SplitType.Horizontal)
            {
                //We are guaranteed that rays from the split into the left and right child will not hit an obstruction
                //(from any y)
                //However, routing a L shaped corridor between the rays is only guaranteed to be possible down the edge of the BSP tile

                //So after finding start and end Y from projected ray we just guess an L and check that it works before doing

                //Cast rays from the split into the left child
                //Look for a corridor or accessable room

                int leftY;
                int leftX = -1;
                do
                {
                    //Random y coord
                    leftY = y + rand.Next(height);

                    //Don't go quite to the left extent (x) - the 2 look ahead will fix this
                    for (int i = x + actualSplit - 1; i > x; i--)
                    {
                        //Look ahead 2
                        MapTerrain terrainNext = baseMap.mapSquares[i, leftY].Terrain;
                        MapTerrain terrainNext2 = baseMap.mapSquares[i - 1, leftY].Terrain;

                        //A corridor is OK
                        if (terrainNext == MapTerrain.Corridor)
                        {
                            leftX = i;
                            break;
                        }
                        //A wall with empty or corridor behind it is OK
                        else if (terrainNext == MapTerrain.Wall && terrainNext2 != MapTerrain.Wall)
                        {
                            leftX = i;
                            break;
                        }
                        //A corridor 'seen coming' we can short cut to
                        else if (terrainNext2 == MapTerrain.Corridor)
                        {
                            leftX = i - 1;
                            break;
                        }
                        //A 1-thick wall is OK
                        else if (terrainNext == MapTerrain.Wall && terrainNext2 == MapTerrain.Wall)
                        {
                            //No good
                            break;
                        }
                    }
                } while (leftX == -1);

                //Cast ray into right child
                int rightY;
                int rightX = -1;

                do
                {
                    //Random y coord
                    rightY = y + rand.Next(height);

                    //Don't go quite to the right extent (x) - the 2 look ahead will fix this
                    for (int i = x + actualSplit; i < x + width - 1; i++)
                    {
                        //Look ahead 2
                        MapTerrain terrainNext = baseMap.mapSquares[i, rightY].Terrain;
                        MapTerrain terrainNext2 = baseMap.mapSquares[i + 1, rightY].Terrain;

                        //Any corridor is OK
                        if (terrainNext == MapTerrain.Corridor)
                        {
                            rightX = i;
                            break;
                        }
                        //A wall with empty or corridor behind it is OK
                        else if (terrainNext == MapTerrain.Wall && terrainNext2 != MapTerrain.Wall)
                        {
                            rightX = i;
                            break;
                        }
                        //A corridor 'seen coming' we can short cut too
                        else if (terrainNext2 == MapTerrain.Corridor)
                        {
                            rightX = i + 1;
                            break;
                        }
                        //A 1-thick wall is OK
                        else if (terrainNext == MapTerrain.Wall && terrainNext2 == MapTerrain.Wall)
                        {
                            //No good
                            break;
                        }
                    }
                } while (rightX == -1);

                //Screen.Instance.DrawMapDebugHighlight(baseMap, leftX, leftY, rightX, rightY);

                //Now route a L corridor from (leftX, leftY) to (rightX, rightY)
                //The L bend can occur within X: leftSafeX -> rightSafeX

                List<Point> corridorRoute = new List<Point>(rightX - leftX + Math.Abs(rightY - leftY));
                bool notValidPath = false;

                //Keep trying until we get a valid path (at least one is guaranteed)

                do
                {
                    corridorRoute.Clear();
                    notValidPath = false;

                    //L bend set randonly
                    int lBendX = leftX + 1 + rand.Next(rightX - leftX);

                    for (int i = leftX; i <= lBendX; i++)
                    {
                        corridorRoute.Add(new Point(i, leftY));
                    }

                    int startY;
                    int endY;

                    if (leftY > rightY)
                    {
                        //down
                        startY = rightY;
                        endY = leftY;
                    }
                    else
                    {
                        startY = leftY;
                        endY = rightY;
                    }

                    for (int j = startY + 1; j < endY; j++)
                    {
                        corridorRoute.Add(new Point(lBendX, j));
                    }

                    for (int i = lBendX; i <= rightX; i++)
                    {
                        corridorRoute.Add(new Point(i, rightY));
                    }

                    //Check this path for validity
                    //Look for walls but ignore the first and last squares
                    for (int i = 1; i < corridorRoute.Count - 1; i++)
                    {
                        if (baseMap.mapSquares[corridorRoute[i].x, corridorRoute[i].y].Terrain == MapTerrain.Wall)
                        {
                            notValidPath = true;
                            break;
                        }
                    }
                } while (notValidPath);

                //We now have a valid path so draw it
                foreach (Point sq in corridorRoute)
                {
                    baseMap.mapSquares[sq.x, sq.y].Terrain = MapTerrain.Corridor;
                    baseMap.mapSquares[sq.x, sq.y].SetOpen();
                }
            }
            else
            {
                //Vertical

                //We are guaranteed that rays from the split into the left and right child will not hit an obstruction
                //(from any y)
                //However, routing a L shaped corridor between the rays is only guaranteed to be possible down the edge of the BSP tile

                //So after finding start and end Y from projected ray we just guess an L and check that it works before doing

                //Cast rays from the split into the left child
                //Look for a corridor or accessable room

                int leftX;
                int leftY = -1;
                do
                {
                    //Random x coord
                    leftX = x + rand.Next(width);

                    //Don't go quite to the left extent (y) - the 2 look ahead will fix this
                    for (int i = y + actualSplit - 1; i > y; i--)
                    {
                        //Look ahead 2
                        MapTerrain terrainNext = baseMap.mapSquares[leftX, i].Terrain;
                        MapTerrain terrainNext2 = baseMap.mapSquares[leftX, i - 1].Terrain;

                        //Any corridor is OK
                        if (terrainNext == MapTerrain.Corridor)
                        {
                            leftY = i;
                            break;
                        }
                        //A wall with empty or corridor behind it is OK
                        else if (terrainNext == MapTerrain.Wall && terrainNext2 != MapTerrain.Wall)
                        {
                            leftY = i;
                            break;
                        }
                        //A corridor 'seen coming' we can short cut too
                        else if (terrainNext2 == MapTerrain.Corridor)
                        {
                            leftY = i - 1;
                            break;
                        }
                        //A 1-thick wall is OK
                        else if (terrainNext == MapTerrain.Wall && terrainNext2 == MapTerrain.Wall)
                        {
                            //No good
                            break;
                        }
                    }
                } while (leftY == -1);

                //Cast ray into right child
                int rightX;
                int rightY = -1;

                do
                {
                    //Random y coord
                    rightX = x + rand.Next(width);

                    //Don't go quite to the right extent (x) - the 2 look ahead will fix this
                    for (int i = y + actualSplit; i < y + height - 1; i++)
                    {
                        //Look ahead 2
                        MapTerrain terrainNext = baseMap.mapSquares[rightX, i].Terrain;
                        MapTerrain terrainNext2 = baseMap.mapSquares[rightX, i + 1].Terrain;

                        //Any corridor is OK
                        if (terrainNext == MapTerrain.Corridor)
                        {
                            rightY = i;
                            break;
                        }
                        //A wall with empty or corridor behind it is OK
                        else if (terrainNext == MapTerrain.Wall && terrainNext2 != MapTerrain.Wall)
                        {
                            rightY = i;
                            break;
                        }
                        //A corridor 'seen coming' we can short cut too
                        else if (terrainNext2 == MapTerrain.Corridor)
                        {
                            rightY = i + 1;
                            break;
                        }
                        //A 1-thick wall is OK
                        else if (terrainNext == MapTerrain.Wall && terrainNext2 == MapTerrain.Wall)
                        {
                            //No good
                            break;
                        }
                    }
                } while (rightY == -1);


                //Now route a L corridor from (leftX, leftY) to (rightX, rightY)
                //The L bend can occur within X: leftSafeX -> rightSafeX

                List<Point> corridorRoute = new List<Point>(Math.Abs(rightX - leftX) + rightY - leftY);
                bool notValidPath = false;

                //Keep trying until we get a valid path (at least one is guaranteed)

                do
                {
                    corridorRoute.Clear();
                    notValidPath = false;

                    //L bend set randonly
                    int lBendY = leftY + 1 + rand.Next(rightY - leftY);

                    for (int i = leftY; i <= lBendY; i++)
                    {
                        corridorRoute.Add(new Point(leftX, i));
                    }

                    int startX;
                    int endX;

                    if (leftX > rightX)
                    {
                        //down
                        startX = rightX;
                        endX = leftX;
                    }
                    else
                    {
                        startX = leftX;
                        endX = rightX;
                    }

                    for (int j = startX + 1; j < endX; j++)
                    {
                        corridorRoute.Add(new Point(j, lBendY));
                    }

                    for (int i = lBendY; i <= rightY; i++)
                    {
                        corridorRoute.Add(new Point(rightX, i));
                    }

                    //Check this path for validity
                    //Look for walls but ignore the first and last squares
                    for (int i = 1; i < corridorRoute.Count - 1; i++)
                    {
                        if (baseMap.mapSquares[corridorRoute[i].x, corridorRoute[i].y].Terrain == MapTerrain.Wall)
                        {
                            notValidPath = true;
                            break;
                        }
                    }
                } while (notValidPath);

                //We now have a valid path so draw it
                foreach (Point sq in corridorRoute)
                {
                    baseMap.mapSquares[sq.x, sq.y].Terrain = MapTerrain.Corridor;
                    baseMap.mapSquares[sq.x, sq.y].SetOpen();
                }


            }
        }
    }

    class MapGeneratorBSP
    {
        int width = 40;
        int height = 40;

        public static Random rand;

        Map baseMap;

        MapNode rootNode;

        public MapGeneratorBSP()
        {

        }

        static MapGeneratorBSP()
        {
            rand = new Random();
        }

        public Map GenerateMap(int extraConnections)
        {
            LogFile.Log.LogEntry(String.Format("Generating BSP dungeon"));

            baseMap = new Map(width, height);

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

            //Set which squares are light blocking
            //Now done during creation
            //SetLightBlocking(baseMap);

            //Set the PC start location in a random room
            baseMap.PCStartLocation = rootNode.RandomRoomPoint();

            return baseMap;
        }

        /// <summary>
        /// Add doors at the end of single corridors into rooms (don't do so with double or triple corridors,
        /// so we have to do this in a final pass)
        /// </summary>
        private void AddDoors()
        {
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
                                baseMap.mapSquares[i, j].Terrain = MapTerrain.ClosedDoor;
                                baseMap.mapSquares[i, j].SetBlocking();
                            }
                        }
                        //South door
                        if (j + 1 < height && i - 1 >= 0 && i + 1 < width)
                        {
                            if (baseMap.mapSquares[i, j + 1].Terrain == MapTerrain.Empty  &&
                                baseMap.mapSquares[i - 1, j].Terrain == MapTerrain.Wall &&
                                baseMap.mapSquares[i + 1, j].Terrain == MapTerrain.Wall)
                            {
                                baseMap.mapSquares[i, j].Terrain = MapTerrain.ClosedDoor;
                                baseMap.mapSquares[i, j].SetBlocking();
                            }
                        }

                        //West door
                        if (i - 1 >= 0 && j - 1 >= 0 && j + 1 < height)
                        {
                            if (baseMap.mapSquares[i - 1, j].Terrain == MapTerrain.Empty &&
                                baseMap.mapSquares[i, j + 1].Terrain == MapTerrain.Wall &&
                                baseMap.mapSquares[i, j - 1].Terrain == MapTerrain.Wall)
                            {
                                baseMap.mapSquares[i, j].Terrain = MapTerrain.ClosedDoor;
                                baseMap.mapSquares[i, j].SetBlocking();
                            }
                        }

                        //East door
                        if (i + 1 < width && j - 1 >= 0 && j + 1 < height)
                        {
                            if (baseMap.mapSquares[i + 1, j].Terrain == MapTerrain.Empty &&
                                baseMap.mapSquares[i, j + 1].Terrain == MapTerrain.Wall &&
                                baseMap.mapSquares[i, j - 1].Terrain == MapTerrain.Wall)
                            {
                                baseMap.mapSquares[i, j].Terrain = MapTerrain.ClosedDoor;
                                baseMap.mapSquares[i, j].SetBlocking();
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
        public Point RandomPointInRoom()
        {
            return rootNode.RandomRoomPoint();
        }

        /// <summary>
        /// Returns a point anywhere the terrain is empty
        /// </summary>
        /// <returns></returns>
        public Point RandomWalkablePoint()
        {
            do
            {
                int x = Game.Random.Next(width);
                int y = Game.Random.Next(height);

                if (baseMap.mapSquares[x, y].Terrain == MapTerrain.Empty)
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

    }
}