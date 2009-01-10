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
            int roomWidth = (int)(width * minFill + rand.Next((int) ( (width * maxFill) - (width * minFill) + 1 ) ));
            int roomHeight = (int)(height * minFill + rand.Next((int) ( (height * maxFill) - (height * minFill) + 1 ) ));

            /*if (roomWidth < MapGeneratorBSP.minimumRoomSize)
                roomWidth = MapGeneratorBSP.minimumRoomSize;

            if (roomHeight < MapGeneratorBSP.minimumRoomSize)
                roomHeight = MapGeneratorBSP.minimumRoomSize;*/

            int lx = x + rand.Next(width - roomWidth + 1);
            int rx = lx + roomWidth - 1;
            int ty = y + rand.Next(height - roomHeight + 1);
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

            if (childLeft == null)
                return;
            childLeft.DrawCorridorConnectingChildren(baseMap);

            if (childRight == null)
                return;
            childRight.DrawCorridorConnectingChildren(baseMap);

            Random rand = MapGeneratorBSP.rand;

            if (split == SplitType.Horizontal)
            {
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
                        MapTerrain terrainNext = baseMap.mapSquares[i,leftY].Terrain;
                        MapTerrain terrainNext2 = baseMap.mapSquares[i - 1,leftY].Terrain;

                        //Any corridor is OK
                        if(terrainNext == MapTerrain.Corridor) {
                           leftX = i;
                        }
                        else if(terrainNext2 == MapTerrain.Corridor) {
                            leftX = i - 1;
                        }
                        //A 1-thick wall is OK
                        else if(terrainNext == MapTerrain.Wall && terrainNext2 == MapTerrain.Empty) {
                            leftX = i;
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
                        MapTerrain terrainNext = baseMap.mapSquares[i,rightY].Terrain;
                        MapTerrain terrainNext2 = baseMap.mapSquares[i + 1,rightY].Terrain;

                        //Any corridor is OK
                        if (terrainNext == MapTerrain.Corridor)
                        {
                            rightX = i;
                        }
                        else if (terrainNext2 == MapTerrain.Corridor)
                        {
                            rightX = i + 1;
                        }
                        //A 1-thick wall is OK
                        else if (terrainNext == MapTerrain.Wall && terrainNext2 == MapTerrain.Empty)
                        {
                            rightX = i;
                        }
                    }
                } while (rightX == -1);


                //Now cast rays to define the 'safe' area for our L shaped corridor to go
                //Left child projecting into right space. Stop if we hit anything other than empty space

                //(ASSUMES THAT THE FURTHER SQUARE TO THE LEFT ON A RIGHT CHILD IS ALWAYS EMPTY
                //MAY NOT BE NECESSARY - SEE BOOK
                int rightSafeX = x + actualSplit;

                //this won't generate safe right up to rightX (which would be in the rest of the wall)
                for (int i = x + actualSplit + 1; i < rightX; i++)
                {
                    MapTerrain checkSquare = baseMap.mapSquares[i, leftY].Terrain;
                    if (checkSquare == MapTerrain.Empty)
                        rightSafeX++;
                    else
                        break;
                }

                //Start on the right hand side of the split which we're assuming is empty
                int leftSafeX = x + actualSplit;

                for (int i = x + actualSplit - 1; i > leftX; i--)
                {
                    MapTerrain checkSquare = baseMap.mapSquares[i, rightY].Terrain;
                    if (checkSquare == MapTerrain.Empty)
                        leftSafeX--;
                    else
                        break;
                }

                //Now route a L corridor from (leftX, leftY) to (rightX, rightY)
                //The L bend can occur within X: leftSafeX -> rightSafeX

                int lBendX = leftSafeX + rand.Next(rightSafeX - leftSafeX + 1);

                for (int i = leftX; i <= lBendX; i++)
                {
                    baseMap.mapSquares[i, leftY].Terrain = MapTerrain.Corridor;
                }
                
                int startY;
                int endY;

                if(leftY > rightY) {
                    //down
                    startY = rightY;
                    endY = leftY;
                }
                else {
                    startY = leftY;
                    endY = rightY;
                }

                for (int j = startY + 1; j < endY; j++)
                {
                    baseMap.mapSquares[lBendX, j].Terrain = MapTerrain.Corridor;
                }

                for (int i = lBendX; i < rightX; i++)
                {
                    baseMap.mapSquares[i, rightY].Terrain = MapTerrain.Corridor;
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
