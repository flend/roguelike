using graphtestc;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    public class MapNode
    {
        //unique room id
        public int Id {
            get; set;
        }

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
        int noSplitChance = 3;
        //Multiple of BSPwidth above which we must split
        int mustSplitSize = 3;

        //How the BSP squares are split as a ratio
        const double minimumSplit = 0.4;
        const double maximumSplit = 0.6;

        //How much of the BSP square is filled by a room
        const double minFill = 0.9;
        const double maxFill = 1.0;

        bool twoWideCorridor = false;

        //Tree depth counter
        int treeDepth;
        bool newConnectionMade;

        //Reference to the MapGeneratorBSP which stores the room numbering
        MapGeneratorBSP parentGenerator;

        public MapNode(MapGeneratorBSP parentGen, int x, int y, int width, int height)
        {
            this.parentGenerator = parentGen;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        enum SplitType {
            Horizontal, Vertical
        }

        //Find a random point within a room
        public PointInRoom RandomRoomPoint()
        {
            return FindRandomRoomPoint();
        }

        public List<PointInRoom> GetRandomSisterRooms(int totalRooms)
        {
            return FindRandomSisterRooms(new List<PointInRoom>(), totalRooms);
        }

        /// <summary>
        /// Find rooms with a near-sister node relationship.
        /// Must be passed an
        /// </summary>
        /// <returns></returns>
        public List<PointInRoom> FindRandomSisterRooms(List<PointInRoom> sisterRoomPoints, int totalRooms)
        {
            //Go down the tree to a random left. When there find a random point in the room and return it
            Random rand = MapGeneratorBSP.rand;

            //If we have both children choose one to do first
            if (childLeft != null && childRight != null)
            {

                if (rand.Next(2) < 1)
                {
                    sisterRoomPoints = childLeft.FindRandomSisterRooms(sisterRoomPoints, totalRooms);
                    sisterRoomPoints = childRight.FindRandomSisterRooms(sisterRoomPoints, totalRooms);
                }
                else
                {
                    sisterRoomPoints = childRight.FindRandomSisterRooms(sisterRoomPoints, totalRooms);
                    sisterRoomPoints = childLeft.FindRandomSisterRooms(sisterRoomPoints, totalRooms);
                }
            }
            //Otherwise do the one we have
            else
            {
                if (childLeft != null)
                {
                    sisterRoomPoints = childLeft.FindRandomSisterRooms(sisterRoomPoints, totalRooms);
                }

                else
                {
                    if (childRight != null)
                    {
                        sisterRoomPoints = childRight.FindRandomSisterRooms(sisterRoomPoints, totalRooms);
                    }
                }
            }

            //roomwidth detects leaf
            if (sisterRoomPoints.Count < totalRooms && roomWidth > 0)
            {
                //If it's the first leaf we've come to 
                //Find point in the room

                int x = roomX + 1 + rand.Next(roomWidth - 2);
                int y = roomY + 1 + rand.Next(roomHeight - 2);

                sisterRoomPoints.Add(new PointInRoom(x, y, roomX, roomY, roomWidth, roomHeight, Id));
            }
          
            return sisterRoomPoints;
        }

        private PointInRoom FindRandomRoomPoint()
        {
            //Go down the tree to a random left. When there find a random point in the room and return it
            Random rand = MapGeneratorBSP.rand;

            PointInRoom retPoint = null;

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
            if (retPoint == null)
            {

                int x = roomX + 1 + rand.Next(roomWidth - 2);
                int y = roomY + 1 + rand.Next(roomHeight - 2);

                return new PointInRoom(x, y, roomX, roomY, roomWidth, roomHeight, Id);
            }

            return retPoint;
        }

        /// <summary>
        /// Find a random point in a room by Id.
        /// DFS all nodes until we find the right one by id
        /// </summary>
        /// <returns></returns>
        public PointInRoom FindRandomRoomPointById(int destId)
        {
            //Go down the tree to a random left. When there find a random point in the room and return it
            Random rand = MapGeneratorBSP.rand;

            PointInRoom retPoint = null;

            if (childLeft != null)
            {
                retPoint = childLeft.FindRandomRoomPointById(destId);
            }

            //Avoid overwriting the real answer
            if (childRight != null && retPoint == null)
            {
                retPoint = childRight.FindRandomRoomPointById(destId);
            }

            //If it's a leaf, check the Id. Find a random point in this room

            //If it's the first leaf we've come to 
            //Find point in the room
            if (childLeft == null && childRight == null && Id == destId)
            {

                int x = roomX + 1 + rand.Next(roomWidth - 2);
                int y = roomY + 1 + rand.Next(roomHeight - 2);

                return new PointInRoom(x, y, roomX, roomY, roomWidth, roomHeight, Id);
            }

            return retPoint;
        }

        public List<RoomCoords> FindAllRooms()
        {
            return FindAllRoomsRecurse(new List<RoomCoords>());
        }

        private List<RoomCoords> FindAllRoomsRecurse(List<RoomCoords> allRooms)
        {
            //Go down the tree to a random left. When there find a random point in the room and return it
            Random rand = MapGeneratorBSP.rand;

            if (childLeft != null)
            {
                allRooms = childLeft.FindAllRoomsRecurse(allRooms);
            }

            if (childRight != null)
            {
                allRooms = childRight.FindAllRoomsRecurse(allRooms);
            }

            if (childRight == null && childLeft == null)
            {
                //We are leaf
                allRooms.Add(new RoomCoords(roomX, roomY, roomWidth, roomHeight));
            }

            return allRooms;

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
                    childLeft = new MapNode(parentGenerator, x, y, actualSplit, height);
                    childLeft.Split();
                }

                if (width - actualSplit < minBSPSquareWidth)
                {
                    childRight = null;
                }
                else
                {
                    childRight = new MapNode(parentGenerator, x + actualSplit, y, width - actualSplit, height);
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
                    childLeft = new MapNode(parentGenerator, x, y, width, actualSplit);
                    childLeft.Split();
                }

                if (height - actualSplit < minBSPSquareHeight)
                {
                    childRight = null;
                }
                else
                {
                    childRight = new MapNode(parentGenerator, x, y + actualSplit, width, height - actualSplit);
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
            
            //Associate this area with a new room id
            Id = parentGenerator.GetNextRoomIdAndIncrease();

            parentGenerator.AssociateAreaWithId(Id, x, y, width, height);

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

                //Inform the generator of this corridor, so it can build the graph representation
                parentGenerator.AddConnection(leftX, leftY, rightX, rightY);

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

                    //Try 2 width
                    if (twoWideCorridor)
                    {
                        if (sq.y - 1 > 0 && sq.y - 1 < height)
                        {
                            baseMap.mapSquares[sq.x, sq.y - 1].Terrain = MapTerrain.Corridor;
                            baseMap.mapSquares[sq.x, sq.y - 1].SetOpen();
                        }
                    }
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

                //Inform the generator of this corridor, so it can build the graph representation
                parentGenerator.AddConnection(leftX, leftY, rightX, rightY);

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

                    //Try 2 width
                    if (twoWideCorridor)
                    {
                        if (sq.x - 1 > 0 && sq.x - 1 < width)
                        {
                            baseMap.mapSquares[sq.x - 1, sq.y].Terrain = MapTerrain.Corridor;
                            baseMap.mapSquares[sq.x - 1, sq.y].SetOpen();
                        }
                    }
                }


            }
        }
    }

    public class MapGeneratorBSP : MapGenerator
    {
        int width = 40;
        int height = 40;

        //Room Ids
        public int NextRoomId { get; set; }

        //For serialization
        public static Random rand;

        public Map baseMap;

        public MapNode rootNode;


        public Point upStaircase;
        public Point downStaircase;


        public MapGeneratorBSP()
        {
            //0 is the default in the array, so good to distinguish
            NextRoomId = 1;
        }

        static MapGeneratorBSP()
        {
            rand = new Random();
        }

        public Map GenerateMap(int extraConnections)
        {
            LogFile.Log.LogEntry(String.Format("Generating BSP dungeon"));

            do
            {
                baseMap = new Map(width, height);
                connectivityGraph = new UndirectedGraph<int, TaggedEdge<int, string>>();

                //BSP is always connected
                baseMap.GuaranteedConnected = true;

                //Make a BSP tree for the rooms

                rootNode = new MapNode(this, 0, 0, width, height);
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
                AddDoorsGraph();

                //Turn corridors into normal squares and surround with walls
                CorridorsIntoRooms();

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
                //baseMap.PCStartLocation = AddEntryRoomForPlayer();

                
            } while (false); //legacy 

            //Save out the graph
            GraphvizExport.OutputUndirectedGraph(connectivityGraph, "bsptree-base");

            //Build the map model for the graph, based on the PC's true starting position (useful for locking doors)
            
            graphModel = new MapModel(connectivityGraph);
            graphModel.EliminateCyclesInMap();

            //Decide on a player start location 
            PointInRoom randomRoom = RandomPointInRoom();
            baseMap.PCStartLocation = randomRoom.GetPointInRoomOnly();
            baseMap.PCStartRoomId = randomRoom.RoomId;

            //Need to 
            
            //Save out a copy of the graph with no cycles

            GraphvizExport.OutputUndirectedGraph(graphModel.GraphNoCycles, "bsptree-nocycles");

            return baseMap.Clone();
        }

        public override Map GetOriginalMap() {
            return baseMap.Clone();
        }

        public override Point GetPlayerStartLocation()
        {
            return baseMap.PCStartLocation;
        }

        public int GetNextRoomIdAndIncrease()
        {
            int nextId = NextRoomId;
            NextRoomId++;
            return nextId;
        }

        private List<Point> EntryDoorLocation { get; set; }

        private Point AddEntryRoomForPlayer()
        {
            bool stillTrying = true;
            int noOfLoops = 0;
            int maxLoops = 100;
            do
            {
                noOfLoops++;
                if (noOfLoops == maxLoops)
                {
                    //No room need to recreate level
                    return new Point(-1, -1);
                }

                //Cast a ray from a side of the map.
                int side = Game.Random.Next(4);

                int roomX = 0;
                int roomY = 0;
                int roomWidth = 3;
                int roomHeight = 3;

                side = Game.Random.Next(4);

                Point doorLoc = new Point(0, 0);
                Point playerLoc = new Point(0, 0);

                switch (side)
                {

                    case 0:
                        {
                            //North

                            int xOrigin = Game.Random.Next(width);

                            int y;
                            for (y = 0; y < height; y++)
                            {

                                if (baseMap.mapSquares[xOrigin, y].Terrain == MapTerrain.Empty)
                                {
                                    break;
                                }
                            }

                            //Failed to find anywhere to break into?
                            if (y == baseMap.height)
                            {
                                LogFile.Log.LogEntryDebug("Failed to find intersection with level, retrying", LogDebugLevel.Medium);
                                continue;
                            }

                            doorLoc = new Point(xOrigin, y - 1);
                            playerLoc = new Point(xOrigin, y - 2);

                            roomX = xOrigin - 1;
                            roomY = doorLoc.y - 2;
                        }
                        break;

                    case 1:
                        {
                            //South

                            int xOrigin = Game.Random.Next(width);

                            int y;
                            for (y = height - 1; y >= 0; y--)
                            {

                                if (baseMap.mapSquares[xOrigin, y].Terrain == MapTerrain.Empty)
                                {
                                    break;
                                }
                            }

                            //Failed to find anywhere to break into?
                            if (y == -1)
                            {
                                LogFile.Log.LogEntryDebug("Failed to find intersection with level, retrying", LogDebugLevel.Medium);
                                continue;
                            }

                            doorLoc = new Point(xOrigin, y + 1);
                            playerLoc = new Point(xOrigin, y + 2);

                            roomX = xOrigin - 1;
                            roomY = doorLoc.y;
                        }
                        break;

                    case 2:
                        {
                            //West

                            int yOrigin = Game.Random.Next(height);

                            int x;
                            for (x = 0; x < width; x++)
                            {

                                if (baseMap.mapSquares[x, yOrigin].Terrain == MapTerrain.Empty)
                                {
                                    break;
                                }
                            }

                            //Failed to find anywhere to break into?
                            if (x == width)
                            {
                                LogFile.Log.LogEntryDebug("Failed to find intersection with level, retrying", LogDebugLevel.Medium);
                                continue;
                            }

                            doorLoc = new Point(x - 1, yOrigin);
                            playerLoc = new Point(x - 2, yOrigin);

                            roomX = x - 3;
                            roomY = yOrigin - 1;
                        }
                        break;

                    case 3:
                        {
                            //East

                            int yOrigin = Game.Random.Next(height);

                            int x;
                            for (x = width - 1; x >= 0; x--)
                            {

                                if (baseMap.mapSquares[x, yOrigin].Terrain == MapTerrain.Empty)
                                {
                                    break;
                                }
                            }

                            //Failed to find anywhere to break into?
                            if (x == -1)
                            {
                                LogFile.Log.LogEntryDebug("Failed to find intersection with level, retrying", LogDebugLevel.Medium);
                                continue;
                            }

                            doorLoc = new Point(x + 1, yOrigin);
                            playerLoc = new Point(x + 2, yOrigin);

                            roomX = doorLoc.x;
                            roomY = yOrigin - 1;
                        }
                        break;

                }

                //Check there is room for the 3x3 room

                bool enoughRoom = true;

                for (int i = roomX; i < roomX + roomWidth; i++)
                {
                    for (int j = roomY; j < roomY + roomHeight; j++)
                    {
                        if (i < 0 || j < 0)
                        {
                            enoughRoom = false;
                            continue;
                        }
                        if (i >= width || j >= height)
                        {
                            enoughRoom = false;
                            continue;
                        }
                    }
                }

                if (!enoughRoom)
                    continue;

                //if (baseMap.mapSquares[i, j].Terrain != MapTerrain.Void && baseMap.mapSquares[i, j].Terrain != MapTerrain.Wall)
                //{
                //    enoughRoom = false;
                //    continue;
                //}

                //Draw room 

                List<Point> dockSurround = new List<Point>();

                bool notAllWall = false;

                for (int i = roomX; i < roomX + roomWidth; i++)
                {
                    for (int j = roomY; j < roomY + roomHeight; j++)
                    {
                        
                        if (baseMap.mapSquares[i, j].Terrain == MapTerrain.Void || baseMap.mapSquares[i, j].Terrain == MapTerrain.Wall)
                        {

                        }
                        else
                        {

                            notAllWall = true;
                        }
                    }
                }

                //Yucky hack to avoid weird positions
                if (notAllWall)
                    continue;

                for (int i = roomX; i < roomX + roomWidth; i++)
                {
                    for (int j = roomY; j < roomY + roomHeight; j++)
                    {
                        if (!(i == playerLoc.x && j == playerLoc.y))
                            dockSurround.Add(new Point(i, j));

                        //Only block out existing walls to avoid making unrouteable maps
                        if (baseMap.mapSquares[i, j].Terrain == MapTerrain.Void || baseMap.mapSquares[i, j].Terrain == MapTerrain.Wall)
                        {
                            baseMap.mapSquares[i, j].Terrain = MapTerrain.DockWall;
                            baseMap.mapSquares[i, j].SetBlocking();
                        }
                    }
                }

                

                //Draw entry room
                baseMap.mapSquares[doorLoc.x, doorLoc.y].Terrain = MapTerrain.ClosedDoor;

                //Returned to set triggers
                EntryDoorLocation = dockSurround;

                //Draw player space
                baseMap.mapSquares[playerLoc.x, playerLoc.y].Terrain = MapTerrain.Empty;
                baseMap.mapSquares[playerLoc.x, playerLoc.y].SetOpen();

                return (playerLoc);

            } while (stillTrying);

            return null;
        }

        public override List<Point> GetEntryDoor()
        {
            return EntryDoorLocation;
        }

        /// <summary>
        /// Add doors based on the connectivity graph stored when the rooms were made
        /// </summary>
        private void AddDoorsGraph()
        {
            foreach (var connectionInfo in edgeInfo.Values)
            {
                baseMap.mapSquares[connectionInfo.LeftX, connectionInfo.LeftY].Terrain = MapTerrain.ClosedDoor;
                baseMap.mapSquares[connectionInfo.LeftX, connectionInfo.LeftY].SetBlocking();

                baseMap.mapSquares[connectionInfo.RightX, connectionInfo.RightY].Terrain = MapTerrain.ClosedDoor;
                baseMap.mapSquares[connectionInfo.RightX, connectionInfo.RightY].SetBlocking();
            }
        }

        /// <summary>
        /// Add doors at the end of single corridors into rooms (don't do so with double or triple corridors,
        /// so we have to do this in a final pass)
        /// </summary>
        private void AddDoors()
        {
            int doorChanceMax = 5;
            int closedDoorChance = 3;

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
                            if (baseMap.mapSquares[i, j + 1].Terrain == MapTerrain.Empty  &&
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
        /// Returns only points in rooms (not corridors) by id of room
        /// </summary>
        /// <returns></returns>
        public PointInRoom RandomPointInRoomById(int destId)
        {
            return rootNode.FindRandomRoomPointById(destId);
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

                if (baseMap.mapSquares[x, y].Terrain == MapTerrain.Empty)
                {
                    return new Point(x, y);
                }
            }
            while (true);
        }

        /// <summary>
        /// Initial implementation at leaf nodes only. TODO: try parents & also redo map for waypoints
        /// </summary>
        /// <returns></returns>
        public override CreaturePatrol CreatureStartPosAndWaypoints(bool clockwise)
        {
            //Find a leaf room
            PointInRoom room = RandomPointInRoom();

            //Calculate a patrol around the room
            int freeSpaceX = (int)Math.Max(0, room.RoomWidth - 2);
            int freeSpaceY = (int)Math.Max(0, room.RoomHeight - 2);

            int patrolIndentX = Game.Random.Next(freeSpaceX / 4);
            int patrolIndentY = Game.Random.Next(freeSpaceY / 4);

            //Waypoints
            List<Point> waypointsBase = new List<Point>();

            Point tl = new Point(room.RoomX + 1 + patrolIndentX, room.RoomY + 1 + patrolIndentY);
            Point tr = new Point(room.RoomX + room.RoomWidth - 2 - patrolIndentX, room.RoomY + 1 + patrolIndentY);
            Point bl = new Point(room.RoomX + 1 + patrolIndentX, room.RoomY + room.RoomHeight - 2 - patrolIndentY);
            
            Point br = new Point(room.RoomX + room.RoomWidth - 2 - patrolIndentX, room.RoomY + room.RoomHeight - 2 - patrolIndentY);

            waypointsBase.Add(tl);
            waypointsBase.Add(tr);
            waypointsBase.Add(br);
            waypointsBase.Add(bl);

            //Start position is a random spot on this square
            //Pair: side index, Point
            List<KeyValuePair<int, Point>> startPos = new List<KeyValuePair<int, Point>>();

            for (int i = tl.x; i <= tr.x; i++)
            {
                //Top
                startPos.Add(new KeyValuePair<int, Point>(0, new Point(i, tl.y)));
                startPos.Add(new KeyValuePair<int, Point>(2, new Point(i, bl.y)));
            }

            for (int j = tl.y + 1; j <= bl.y - 1; j++)
            {
                startPos.Add(new KeyValuePair<int, Point>(3, new Point(tl.x, j)));
                startPos.Add(new KeyValuePair<int, Point>(1, new Point(tr.x, j)));
            }

            KeyValuePair<int, Point> startLoc = startPos[Game.Random.Next(startPos.Count)];

            //Depending on the startLoc, re-order the waypoints so we to a suitable first point
            List<Point> waypointsReordered = new List<Point>();

            if (clockwise == true)
            {
                for (int i = 1; i < 5; i++)
                {
                    waypointsReordered.Add(waypointsBase[(startLoc.Key + i) % 4]);
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    int wayPointNo = startLoc.Key - i;
                    if (wayPointNo < 0)
                        wayPointNo += 4;

                    waypointsReordered.Add(waypointsBase[wayPointNo]);
                }
            }

            return new CreaturePatrol(startLoc.Value, new RoomCoords(room), waypointsReordered);

        }

        /*
        /// <summary>
        /// Find a route between sistering rooms
        /// </summary>
        /// <returns></returns>
        public override CreaturePatrol CreatureStartPosAndWaypointsSisterRooms(bool clockwise)
        {
            //Find a leaf room
            List<PointInRoom> sisterRoomPoints = rootNode.GetRandomSisterRooms(4);

            //use reordered for the 2nd room (where start pos is) and base for first room
            List<List<Point>> waypointReorderedAll = new List<List<Point>>();
            List<List<Point>> waypointBaseAll = new List<List<Point>>();

            //(arbitary use the 2nd startloc)
            KeyValuePair<int, Point> startLoc = new KeyValuePair<int,Point>();

            //Calculate waypoints for each room then pick 2 from each
            for (int r = 0; r < 2; r++)
            {

                PointInRoom room = sisterRoomPoints[r];

                //Calculate a patrol around the room
                int freeSpaceX = (int)Math.Max(0, room.RoomWidth - 2);
                int freeSpaceY = (int)Math.Max(0, room.RoomHeight - 2);

                int patrolIndentX = Game.Random.Next(freeSpaceX / 4);
                int patrolIndentY = Game.Random.Next(freeSpaceY / 4);

                //Waypoints
                List<Point> waypointsBase = new List<Point>();

                Point tl = new Point(room.RoomX + 1 + patrolIndentX, room.RoomY + 1 + patrolIndentY);
                Point tr = new Point(room.RoomX + room.RoomWidth - 2 - patrolIndentX, room.RoomY + 1 + patrolIndentY);
                Point bl = new Point(room.RoomX + 1 + patrolIndentX, room.RoomY + room.RoomHeight - 2 - patrolIndentY);

                Point br = new Point(room.RoomX + room.RoomWidth - 2 - patrolIndentX, room.RoomY + room.RoomHeight - 2 - patrolIndentY);

                waypointsBase.Add(tl);
                waypointsBase.Add(tr);
                waypointsBase.Add(br);
                waypointsBase.Add(bl);

                waypointBaseAll.Add(waypointsBase);

                //Start position is a random spot on this square
                //Pair: side index, Point
                List<KeyValuePair<int, Point>> startPos = new List<KeyValuePair<int, Point>>();

                for (int i = tl.x; i <= tr.x; i++)
                {
                    //Top
                    startPos.Add(new KeyValuePair<int, Point>(0, new Point(i, tl.y)));
                    startPos.Add(new KeyValuePair<int, Point>(2, new Point(i, bl.y)));
                }

                for (int j = tl.y + 1; j <= bl.y - 1; j++)
                {
                    startPos.Add(new KeyValuePair<int, Point>(3, new Point(tl.x, j)));
                    startPos.Add(new KeyValuePair<int, Point>(1, new Point(tr.x, j)));
                }

                startLoc = startPos[Game.Random.Next(startPos.Count)];

                //Depending on the startLoc, re-order the waypoints so we to a suitable first point
                List<Point> waypointsReordered = new List<Point>();

                if (clockwise == true)
                {
                    for (int i = 1; i < 5; i++)
                    {
                        waypointsReordered.Add(waypointsBase[(startLoc.Key + i) % 4]);
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int wayPointNo = startLoc.Key - i;
                        if (wayPointNo < 0)
                            wayPointNo += 4;

                        waypointsReordered.Add(waypointsBase[wayPointNo]);
                    }
                }

                waypointReorderedAll.Add(waypointsReordered);
            }

            //Pick the first 2 waypoints from the first room and the last 2 from the 2nd
            List<Point> newWaypoints = new List<Point>();
            
            newWaypoints.Add(waypointReorderedAll[1][0]);
            //newWaypoints.Add(waypointReorderedAll[1][1]);
           // newWaypoints.Add(waypointReorderedAll[1][2]);
           // newWaypoints.Add(waypointReorderedAll[1][3]);
           // newWaypoints.Add(waypointReorderedAll[1][0]);
            newWaypoints.Add(waypointBaseAll[0][0]);
          //  newWaypoints.Add(waypointBaseAll[0][1]);
          ///  newWaypoints.Add(waypointBaseAll[0][2]);
//newWaypoints.Add(waypointBaseAll[0][3]);
         //   newWaypoints.Add(waypointBaseAll[0][0]);

            return new CreaturePatrol(startLoc.Value, newWaypoints);

        }
        */

        public class toSort
        {
            public PointInRoom index;
            public int coord;

            public toSort(PointInRoom index, int coord)
            {
                this.index = index;
                this.coord = coord;
            }

        }

        /// <summary>
        /// Find a route between center of sistering rooms
        /// </summary>
        /// <returns></returns>
        public override CreaturePatrol CreatureStartPosAndWaypointsSisterRooms(bool clockwise, int noOfRooms)
        {
            //Find a leaf room
            int totalRoomsToVisit = noOfRooms;

            List<PointInRoom> sisterRoomPoints = rootNode.GetRandomSisterRooms(totalRoomsToVisit);

            //Fully random
            /*
            List<PointInRoom> sisterRoomPoints = new List<PointInRoom>();
            for (int i = 0; i < noOfRooms; i++)
            {
                sisterRoomPoints.Add(rootNode.RandomRoomPoint());
            }*/

            List<List<Point>> waypointReorderedAll = new List<List<Point>>();

            //Find a simple path between a point in each of the rooms

            List<toSort> allWaypointsIndices = new List<toSort>();

            for (int r = 0; r < sisterRoomPoints.Count; r++)
            {
                if (clockwise)
                {
                    allWaypointsIndices.Add(new toSort(sisterRoomPoints[r], sisterRoomPoints[r].X));
                }
                else
                {
                    allWaypointsIndices.Add(new toSort(sisterRoomPoints[r], sisterRoomPoints[r].Y));
                }
            }

            //Sort by relevant value
            allWaypointsIndices.Sort((a, b) => a.coord.CompareTo(b.coord));

            List<Point> allWaypoints = new List<Point>();
            List<RoomCoords> allRooms = new List<RoomCoords>();
            foreach (toSort s in allWaypointsIndices)
            {
                allWaypoints.Add(new Point(s.index.RoomX + s.index.RoomWidth / 2, s.index.RoomY + s.index.RoomHeight / 2));
                allRooms.Add(new RoomCoords(s.index));
            }

            return new CreaturePatrol(allWaypoints[0], allRooms[0], allWaypoints);



            /*
            //Remove duplicates (useful if fully random points are used

            for (int i = 0; i < allWaypointsIndices.Count; i++)
            {
                Point newWaypoint = new Point(allWaypointsIndices[i].index.RoomX + allWaypointsIndices[i].index.RoomWidth / 2, allWaypointsIndices[i].index.RoomY + allWaypointsIndices[i].index.RoomHeight / 2);

                if(lastPoint != null) {
                    if (newWaypoint == lastPoint)
                        continue;
                }

                allWaypoints.Add(newWaypoint);
                lastPoint = newWaypoint;
            }
            */



            /*

            
            foreach (toSort s in allWaypointsIndices)
            {
                allWaypoints.Add(new Point(s.index.RoomX + s.index.RoomWidth / 2, s.index.RoomY + s.index.RoomHeight / 2));
            }*/

            //return new CreaturePatrol(allWaypoints[0], allWaypoints);

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

        public override List<RoomCoords> GetAllRooms()
        {
            return rootNode.FindAllRooms();
        }

        /// <summary>
        /// Associate this area with a room id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        internal void AssociateAreaWithId(int id, int x, int y, int width, int height)
        {
            for (int i = x; i < x + width; i++)
            {
                for (int j = y; j < y + height; j++)
                {
                    //Should be no overlaps
                    if (baseMap.roomIdMap[i, j] != 0)
                    {
                        LogFile.Log.LogEntryDebug("Error, area already associated with room", LogDebugLevel.High);
                    }

                    baseMap.roomIdMap[i, j] = id;
                }
            }
        }

        /// <summary>
        /// There is a connecting corridor between these two points on the map. Add this as an edge on the graph
        /// </summary>
        /// <param name="leftX"></param>
        /// <param name="leftY"></param>
        /// <param name="rightX"></param>
        /// <param name="rightY"></param>

        ///Information about what the edge looks like on the map. Could / should be stored in the TaggedEdge itself
        public class ConnectionInfo
        {
            public int LeftX { get; set; }
            public int LeftY { get; set; }
            public int RightX { get; set; }
            public int RightY { get; set; }

            public ConnectionInfo(int leftX, int leftY, int rightX, int rightY)
            {
                LeftX = leftX;
                LeftY = leftY;
                RightX = rightX;
                RightY = rightY;
            }
        }

        private UndirectedGraph<int, TaggedEdge<int, string>> connectivityGraph;
        private MapModel graphModel;
        private Dictionary<TaggedEdge<int, string>, ConnectionInfo> edgeInfo = new Dictionary<TaggedEdge<int, string>, ConnectionInfo>();
        
        internal void AddConnection(int leftX, int leftY, int rightX, int rightY)
        {
            //Lookup the room ids for the origin and dest
            int originId = baseMap.roomIdMap[leftX, leftY];
            int destId = baseMap.roomIdMap[rightX, rightY];

            LogFile.Log.LogEntryDebug("From room: " + originId + " to room: " + destId, LogDebugLevel.High);

            var newGraphEdge = new TaggedEdge<int, string>(originId, destId, "");

            connectivityGraph.AddVerticesAndEdge(newGraphEdge);

            //Add the door locations to the edge lookup
            edgeInfo.Add(newGraphEdge, new ConnectionInfo(leftX, leftY, rightX, rightY));
        }

        /// <summary>
        /// Lock a connection (set its doors to locked with a particular id)
        /// </summary>
        /// <param name="edge"></param>
        internal void LockConnection(TaggedEdge<int, string> edge, string lockId)
        {
            ConnectionInfo connectionInfo = edgeInfo[edge];

            baseMap.mapSquareLocks[connectionInfo.LeftX, connectionInfo.LeftY] = lockId;
            baseMap.mapSquareLocks[connectionInfo.RightX, connectionInfo.RightY] = lockId;
        }

        public UndirectedGraph<int, TaggedEdge<int, string>> ConnectivityGraph
        {
            get
            {
                return connectivityGraph;
            }
        }

        public MapModel GraphModel
        {
            get
            {
                return graphModel;
            }
        }

        
    }
}
