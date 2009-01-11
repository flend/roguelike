using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    class MapNode
    {
        int width;
        int height;

        //tl corner in map coords
        int x;
        int y;

        MapNode childLeft;
        MapNode childRight;

        SplitType split;
        int actualSplit;

        const double minimumSplit = 0.2;
        const double maximumSplit = 0.8;

        const double minFill = 0.6;
        const double maxFill = 1.0;

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

        public void Split() {

            Random rand = MapGeneratorBSP.rand;
            split = SplitType.Vertical;

            if(rand.Next(2) == 1) {
                split = SplitType.Horizontal;
            }

            if (split == SplitType.Horizontal)
            {
                int minSplit = (int)(width * minimumSplit);
                int maxSplit = (int)(width * maximumSplit);

                actualSplit = minSplit + rand.Next(maxSplit - minSplit + 1);

                //Define the two child areas, make objects and then recursively split them

                if (actualSplit < MapGeneratorBSP.minimumRoomSize)
                {
                    childLeft = null;
                }
                else
                {
                    childLeft = new MapNode(x, y, actualSplit, height);
                    childLeft.Split();
                }

                if (width - actualSplit < MapGeneratorBSP.minimumRoomSize)
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
                int minSplit = (int)(height * minimumSplit);
                int maxSplit = (int)(height * maximumSplit);

                actualSplit = minSplit + rand.Next(maxSplit - minSplit + 1);

                //Define the two child areas, make objects and then recursively split them

                if (actualSplit < MapGeneratorBSP.minimumRoomSize)
                {
                    childLeft = null;
                }
                else
                {
                    childLeft = new MapNode(x, y, width, actualSplit);
                    childLeft.Split();
                }

                if (height - actualSplit < MapGeneratorBSP.minimumRoomSize)
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
            //Width and height ensures a 1 square border in the BSP square
            int roomWidth = (int)(width * minFill + rand.Next((int) ( (width * maxFill) - (width * minFill) - 1 ) ));
            int roomHeight = (int)(height * minFill + rand.Next((int) ( (height * maxFill) - (height * minFill) - 1 ) ));

            /*if (roomWidth < MapGeneratorBSP.minimumRoomSize)
                roomWidth = MapGeneratorBSP.minimumRoomSize;

            if (roomHeight < MapGeneratorBSP.minimumRoomSize)
                roomHeight = MapGeneratorBSP.minimumRoomSize;*/

            int lx = x + 1 + rand.Next(width - roomWidth);
            int rx = lx + roomWidth - 1;
            int ty = y + 1 + rand.Next(height - roomHeight);
            int by = ty + roomHeight - 1;

            for (int i = lx; i <= rx; i++)
            {
                //Top row
                baseMap.mapSquares[i, ty].Terrain = MapTerrain.Wall;
                //Bottom row
                baseMap.mapSquares[i, by].Terrain = MapTerrain.Wall;
            }

            for (int i = ty; i <= by; i++)
            {
                //Left row
                baseMap.mapSquares[lx, i].Terrain = MapTerrain.Wall;
                //Right row
                baseMap.mapSquares[rx, i].Terrain = MapTerrain.Wall;
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
                        //A corridor 'seen coming' we can short cut too
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
                    int lBendX = leftX + 1 + rand.Next(rightX - leftX - 2);

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
                        if (baseMap.mapSquares[corridorRoute[i].x, corridorRoute[i].y].Terrain != MapTerrain.Empty)
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
                    int lBendY = leftY + 1 + rand.Next(rightY - leftY - 2);

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
                        if (baseMap.mapSquares[corridorRoute[i].x, corridorRoute[i].y].Terrain != MapTerrain.Empty)
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
                }


            }
        }
    }

    class MapGeneratorBSP
    {
        int width = 40;
        int height = 40;

        public static Random rand;
        public const int minimumRoomSize = 6;

        Map baseMap;

        public MapGeneratorBSP()
        {

        }

        static MapGeneratorBSP()
        {
            rand = new Random();
        }

        public Map GenerateMap()
        {
            LogFile.Log.LogEntry(String.Format("Generating BSP dungeon"));

            baseMap = new Map(width, height);

            //Make a BSP tree for the rooms

            MapNode rootNode = new MapNode(0, 0, width, height);
            rootNode.Split();
            
            //Draw a room in each BSP leaf
            rootNode.DrawRoomAtLeaf(baseMap);

            rootNode.DrawCorridorConnectingChildren(baseMap);

            baseMap.PCStartLocation = new Point(0, 0);

            return baseMap;
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
