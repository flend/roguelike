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
            SplitType split = SplitType.Vertical;

            if(rand.Next(2) == 1) {
                split = SplitType.Horizontal;
            }

            if (split == SplitType.Horizontal)
            {
                int minSplit = (int)(width * minimumSplit);
                int maxSplit = (int)(width * maximumSplit);

                int actualSplit = minSplit + rand.Next(maxSplit - minSplit + 1);

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

                int actualSplit = minSplit + rand.Next(maxSplit - minSplit + 1);

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
            //For now just draw a room filling the BSP

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
