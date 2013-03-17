using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    public class MapGeneratorBSPCave : MapGenerator
    {
        int width = 40;
        int height = 40;

        int DiggingChance { get; set; }

        public static Random rand;

        public Map baseMap;

        public MapNode rootNode;

        public List<MapTerrain> rubbleType;
        public List<MapTerrain> possibleEmptyTypes;

        public Point upStaircase;
        public Point downStaircase;

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

            return baseMap.Clone();
        }

        public override Map GetOriginalMap() {
            return baseMap.Clone();
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

        private Point EntryDoorLocation { get; set; }

        private Point AddEntryRoomForPlayer()
        {
            bool stillTrying = true;

            do
            {

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
                //It's almost impossible that we will block off an area, and this check fails for maps with rooms close to the edge


                //Draw room 
                for (int i = roomX; i < roomX + roomWidth; i++)
                {
                    for (int j = roomY; j < roomY + roomHeight; j++)
                    {
                        baseMap.mapSquares[i, j].Terrain = MapTerrain.Wall;
                        baseMap.mapSquares[i, j].SetBlocking();
                    }
                }

                //Draw entry room
                baseMap.mapSquares[doorLoc.x, doorLoc.y].Terrain = MapTerrain.ClosedDoor;

                //Returned to set triggers
                EntryDoorLocation = doorLoc;

                //Draw player space
                baseMap.mapSquares[playerLoc.x, playerLoc.y].Terrain = MapTerrain.Empty;
                baseMap.mapSquares[playerLoc.x, playerLoc.y].SetOpen();

                return (playerLoc);

            } while (stillTrying);

            return null;
        }

        public override Point GetEntryDoor()
        {
            return EntryDoorLocation;
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

        class toSort
        {
            public PointInRoom index;
            public int coord;

            public toSort(PointInRoom index, int coord)
            {
                this.index = index;
                this.coord = coord;
            }

        }

        public override Point GetPlayerStartLocation()
        {
            return baseMap.PCStartLocation;
        }

        /// <summary>
        /// Find a route between sistering rooms
        /// </summary>
        /// <returns></returns>
        public override CreaturePatrol CreatureStartPosAndWaypointsSisterRooms(bool clockwise, int noOfRooms)
        {
            //Find a leaf room
            int totalRoomsToVisit = noOfRooms;

            List<PointInRoom> sisterRoomPoints = rootNode.GetRandomSisterRooms(totalRoomsToVisit);

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

        public override List<RoomCoords> GetAllRooms()
        {
            return rootNode.FindAllRooms();
        }
    }
}
