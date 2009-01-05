using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    class RogueRoom
    {
        public bool connected = false;
        public List<int> connectedTo;
        
        public int roomWidth = 1;
        public int roomHeight = 1;
        
        //Coords of room centre
        public int roomX;
        public int roomY;

        public RogueRoom()
        {
            connectedTo = new List<int>();
        }

    }

    class MapGeneratorRogue
    {

        int width = 60;
        int height = 60;

        //Make n x n rooms
        int subSquareDim = 3;

        int minimumRoomSize = 3;

        int corridorRedos;

        RogueRoom[] mapRooms;

        static Random rand;
        Map baseMap;

        public MapGeneratorRogue()
        {

        }

        static MapGeneratorRogue()
        {
            rand = new Random();
        }

        RogueRoom MapRoom(int x, int y)
        {
            return mapRooms[x * subSquareDim + y];
        }

        public Map GenerateMap()
        {
            LogFile.Log.LogEntry(String.Format("Generating Rogue-like dungeon {0}x{0}", subSquareDim));
            corridorRedos = 0;
            

            baseMap = new Map(width, height);

            //Divide the grid up into subsquares
            int subsquareWidth = width / subSquareDim;
            int subsquareHeight = height / subSquareDim;

            int totalRooms = subSquareDim * subSquareDim;

            //Setup room array
            mapRooms = new RogueRoom[totalRooms];
            for(int i=0;i<totalRooms;i++) {
                mapRooms[i] = new RogueRoom();
            }

            //Give rooms random sizes
            //-2 is to ensure there are same columns at the extremes of the squares
            //-1 would probably be OK too
            //The maxroom size possible is 1 below filling the allowed area completely.

            int roomSizeWidthVariation = subsquareWidth - minimumRoomSize - 1;
            int roomSizeHeightVariation = subsquareHeight - minimumRoomSize - 1;

            //int roomSizeWidthVariation = 2;
            //int roomSizeHeightVariation = 2;

            foreach (RogueRoom room in mapRooms)
            {
                room.roomWidth = minimumRoomSize + rand.Next(roomSizeWidthVariation);
                room.roomHeight = minimumRoomSize + rand.Next(roomSizeHeightVariation);
            }

            //Calculate room centres
            for (int i = 0; i < subSquareDim; i++)
            {
                for (int j = 0; j < subSquareDim; j++)
                {
                    //Available space
                    //x: SubsquareWidth - 2 (safe columns) - roomSizeWidth
                    //+1 used in Rand so we can generate the maximum
                    int squareCentreX = subsquareWidth / 2 + subsquareWidth * i;
                    int squareCentreY = subsquareHeight / 2 + subsquareHeight * j;

                    int roomWidth = MapRoom(i, j).roomWidth;
                    int roomHeight = MapRoom(i, j).roomHeight;

                    int availableX = subsquareWidth - 2 - roomWidth;
                    int availableY = subsquareHeight - 2 - roomHeight;

                    int offsetX = rand.Next(availableX + 1);
                    int offsetY = rand.Next(availableY + 1);

                    offsetX -= availableX / 2;
                    offsetY -= availableY / 2;

                    MapRoom(i, j).roomX = squareCentreX + offsetX;
                    MapRoom(i, j).roomY = squareCentreY + offsetY;
                }
            }

            

            //Connect rooms
            //First stage is to snake through the rooms until we reach a 'snake' corner

            //(we could snake more than once for big maps but unnecessary for small maps)

            int connectedRooms = 0;

            //Current processing room

            int startRoom = rand.Next(totalRooms);

            //Link each room to an unconnected neighbour until we run out of unconnected neighbours

            bool unconnectedNeighbourFound = false;

            
            do
            {
                //Say this room is connected (it is at least connected to the room we came from)
                mapRooms[startRoom].connected = true;
                connectedRooms++;

                //Connect to a random unconnected neighbour and then make that the current processing room
                List<int> unconnectedNeighbours = FindConnectedNeighbours(startRoom, false);

                if (unconnectedNeighbours.Count > 0)
                {
                    int neighbourRand = rand.Next(unconnectedNeighbours.Count);
                    int neighbourToConnectTo = unconnectedNeighbours[neighbourRand];

                    mapRooms[startRoom].connectedTo.Add(neighbourToConnectTo);

                    startRoom = neighbourToConnectTo;
                    unconnectedNeighbourFound = true;
                }
                else
                {
                    unconnectedNeighbourFound = false;
                }
            }
            while (unconnectedNeighbourFound);
            

            /*
            //The snake above can be commented out if you like and replaced with these two lines
            mapRooms[startRoom].connected = true;
            connectedRooms++;
            */
             
            //Cycle through all unconnected rooms
            //Connect each to a connected neighbour

            startRoom = 0;

            int fixingIterations = 0; //profiling

            while (connectedRooms < totalRooms)
            {
                fixingIterations++;

                //Find next unconnected room
             
                while (mapRooms[startRoom].connected == true)
                {
                    startRoom++;

                    //Allow cycle to reset back to 0
                    if (startRoom == totalRooms)
                    {
                        startRoom = 0;
                    }
                }

                //Find connected neighbours

                List<int> connectedNeighbours = FindConnectedNeighbours(startRoom, true);

                //Connect to a random connected neighbour
                if (connectedNeighbours.Count > 0)
                {
                    int neighbourRand = rand.Next(connectedNeighbours.Count);
                    int neighbourToConnectTo = connectedNeighbours[neighbourRand];

                    mapRooms[startRoom].connectedTo.Add(neighbourToConnectTo);

                    //Mark this room as connected
                    mapRooms[startRoom].connected = true;
                    connectedRooms++;
                }
                //If we don't find a connected neighbour this time, we'll return here on the cycle
                startRoom++;
                
                //Allow cycle to reset back to 0
                if (startRoom == totalRooms)
                {
                    startRoom = 0;
                }

            }

            //Add some more random connections

            int totalRandomConnections = rand.Next(subSquareDim);
            int noRandomConnections = 0;

            while (noRandomConnections < totalRandomConnections)
            {
                //Pick a room at random
                int randomRoom = rand.Next(totalRooms);

                //Pick a random neighbour to connect to
                List<int> allNeighbours = FindAllNeighbours(randomRoom);

                if (allNeighbours.Count > 0)
                {
                    int neighbourRand = rand.Next(allNeighbours.Count);
                    int neighbourToConnectTo = allNeighbours[neighbourRand];

                    //If we are not already connected, add a connection
                    if (!mapRooms[randomRoom].connectedTo.Contains(neighbourToConnectTo) && !mapRooms[neighbourToConnectTo].connectedTo.Contains(randomRoom))
                    {
                        mapRooms[randomRoom].connectedTo.Add(neighbourToConnectTo);
                        noRandomConnections++;
                    }
                }

            }


            LogFile.Log.LogEntry(String.Format("Generation complete, fixing iterations {0}, corridor redos {1}", fixingIterations, corridorRedos));

            //Draw rooms on map

            foreach (RogueRoom room in mapRooms)
            {
                DrawRoomOnMap(room);
            }

            //Draw corridors
            
            for(int i=0;i<totalRooms;i++) {

                foreach (int dest in mapRooms[i].connectedTo)
                {
                    DrawMapCorridor(i, dest);
                }
            }
            
            //Place the PC in room 1
            baseMap.PCStartLocation = new Point(mapRooms[0].roomX, mapRooms[0].roomY);


            //Make a square
           /*
            for (int i = 10; i < 20; i++)
            {
                for (int j = 10; j < 20; j++)
                {
                    baseMap.mapSquares[i,j] = Map.MapTerrain.Wall;
                }
            }

            for (int i = 11; i < 19; i++)
            {
                for (int j = 11; j < 19; j++)
                {
                    baseMap.mapSquares[i,j] = Map.MapTerrain.Empty;
                }
            }
            */
            //Stick the PC in the middle

            //baseMap.PCStartLocation = new Point(15, 15);

            return baseMap;
        }

        enum Direction { North, East, South, West };

        private void DrawMapCorridor(int startRoomCoord, int destRoomCoord)
        {
            //Draw an L-shaped corridor from centre to centre

            //Providing we don't have room centres on the extreme top or left of squares then there will always be a route

            //Establish what sort of connection this is, horizontal or vertical
            //We have the room numbers (this logic won't work on 1 row / column maps where some types of connection are not possible
            int startRoomX, startRoomY;
            RoomCoords(startRoomCoord, out startRoomX, out startRoomY);

            int destRoomX, destRoomY;
            RoomCoords(destRoomCoord, out destRoomX, out destRoomY);

            Direction corridorDir;

            if (startRoomY == destRoomY)
            {
                if (startRoomX < destRoomX)
                {
                    //East
                    corridorDir = Direction.East;
                }
                else
                {
                    //West
                    corridorDir = Direction.West;
                }
            }
            else
            {
                if (startRoomY < destRoomY)
                {
                    //North
                    corridorDir = Direction.South;
                }
                else
                {
                    //South
                    corridorDir = Direction.North;
                }
            }

            //Get room objects
            RogueRoom startRoom = mapRooms[startRoomCoord];
            RogueRoom destRoom = mapRooms[destRoomCoord];
            
            int startX, startY;
            int endX, endY;

            //Rooms are drawn from
            //centre - roomWidth / 2
            //to
            //(centre - roomWidth / 2) + roomWidth - 1 (inclusive)
            //right wall: centre + ceil(roomWidth / 2) (inclusive)
            //left wall: centre - floor(roomWidth / 2)

            //start/end X/Y are the wall spaces which will become doors

            if (corridorDir == Direction.East)
            {
                startX = startRoom.roomX - startRoom.roomWidth / 2 + startRoom.roomWidth - 1;
                startY = startRoom.roomY;
                endX = destRoom.roomX - destRoom.roomWidth / 2;
                endY = destRoom.roomY;
            }
            
            else if (corridorDir == Direction.West)
            {
                //Draw the corridor the other way round (east)
                startX = destRoom.roomX - destRoom.roomWidth / 2 + destRoom.roomWidth - 1;
                startY = destRoom.roomY;
                endX = startRoom.roomX - startRoom.roomWidth / 2;
                endY = startRoom.roomY;

                corridorDir = Direction.East;
            }

            else if (corridorDir == Direction.South)
            {
                startY = startRoom.roomY - startRoom.roomHeight / 2 + startRoom.roomHeight - 1;
                startX = startRoom.roomX;
                endY = destRoom.roomY - destRoom.roomHeight / 2;
                endX = destRoom.roomX;
            }

            else
            {
                //Direction.North
                //Turn this into a south corridor
                startY = destRoom.roomY - destRoom.roomHeight / 2 + destRoom.roomHeight - 1;
                startX = destRoom.roomX;
                endY = startRoom.roomY - startRoom.roomHeight / 2;
                endX = startRoom.roomX;

                corridorDir = Direction.South;
            }

            //Corridor spaces (not including doors)
            int corridorLengthX = endX - startX - 2;
            int corridorLengthY = endY - startY - 2;

            //Try to draw a random corridor. It may intersect with a pre-existing corridor in which case we need to start again
            bool corridorFailed = false;

            //Where to put the bend is the random variable
            int lBendCoord;

            do
            {
                
                //Pick a random point for the L bend
                //+1 so we can get the largest value
                if (corridorDir == Direction.East)
                {
                    lBendCoord = rand.Next(corridorLengthX + 1);
                }
                else
                {
                    //Direction.South
                    lBendCoord = rand.Next(corridorLengthY + 1);
                }

                //Trace out the corridor - except the doors
                //Look for non-empty squares - a collision
                //In that case restart

                //East
                //Go right: (startX + 1) to (startX + 1 + lBendCoord)
                //Go up/down: (startY + 1) to (endY)
                //Go left: (startX + 1 + lBendCoord) to (endX - 1)

                if (corridorDir == Direction.East)
                {
                    //right
                    for (int i = startX + 1; i <= startX + 1 + lBendCoord; i++)
                    {
                        if (baseMap.mapSquares[i, startY].terrain != MapTerrain.Empty)
                        {
                            corridorRedos++;
                            continue;

                        }
                    }

                    //up / down
                    int xCoord = startX + 1 + lBendCoord;

                    int corridorYStart;
                    int corridorYEnd;

                    if (endY > startY)
                    {
                        corridorYStart = startY;
                        corridorYEnd = endY;
                    }
                    else
                    {
                        corridorYStart = endY;
                        corridorYEnd = startY;
                    }

                    for (int j = corridorYStart; j <= corridorYEnd; j++)
                    {
                        if (baseMap.mapSquares[xCoord, j].terrain != MapTerrain.Empty) {
                            corridorRedos++; 
                            continue;

                        }

                    }

                    //right
                    for (int i = xCoord + 1; i <= endX - 1; i++)
                    {
                        if (baseMap.mapSquares[i, endY].terrain != MapTerrain.Empty) {
                            corridorRedos++;
                            continue;

                        }

                    }
                }
                else
                {
                    //Direction.South
                    //down
                    for (int i = startY + 1; i <= startY + 1 + lBendCoord; i++)
                    {
                        if (baseMap.mapSquares[startX, i].terrain != MapTerrain.Empty) {
                            corridorRedos++; 
                            continue;

                        }

                    }

                    //left / right
                    int yCoord = startY + 1 + lBendCoord;

                    int corridorXEnd;
                    int corridorXStart;

                    if (endX > startX)
                    {
                        corridorXStart = startX;
                        corridorXEnd = endX;
                    }
                    else
                    {
                        corridorXStart = endX;
                        corridorXEnd = startX;
                    }

                    for (int j = corridorXStart; j <= corridorXEnd; j++)
                    {
                        if (baseMap.mapSquares[j, yCoord].terrain != MapTerrain.Empty) {
                            corridorRedos++; 
                            continue;

                        }

                    }

                    //down
                    for (int i = yCoord + 1; i <= endY - 1; i++)
                    {
                        if (baseMap.mapSquares[endX, i].terrain != MapTerrain.Empty) {
                            corridorRedos++;
                            continue;
                        }

                    }
                }
            } while (corridorFailed);

            //We now have a successful corridor so draw it
            if (corridorDir == Direction.East)
            {
                //right
                for (int i = startX; i <= startX + 1 + lBendCoord; i++)
                {
                    baseMap.mapSquares[i, startY].terrain = MapTerrain.Corridor;
                }

                //up / down
                int xCoord = startX + 1 + lBendCoord;

                int corridorYStart;
                int corridorYEnd;

                if (endY > startY)
                {
                    corridorYStart = startY;
                    corridorYEnd = endY;
                }
                else
                {
                    corridorYStart = endY;
                    corridorYEnd = startY;
                }

                for (int j = corridorYStart; j <= corridorYEnd; j++)
                {
                    baseMap.mapSquares[xCoord, j].terrain = MapTerrain.Corridor;
                }

                //right
                for (int i = xCoord + 1; i <= endX; i++)
                {
                    baseMap.mapSquares[i, endY].terrain = MapTerrain.Corridor;
                }
            }
            else
            {
                //Direction.South
                //down
                for (int i = startY; i <= startY + 1 + lBendCoord; i++)
                {
                    baseMap.mapSquares[startX, i].terrain = MapTerrain.Corridor;
                }

                //left / right
                int yCoord = startY + 1 + lBendCoord;

                int corridorXEnd;
                int corridorXStart;

                if (endX > startX)
                {
                    corridorXStart = startX;
                    corridorXEnd = endX;
                }
                else
                {
                    corridorXStart = endX;
                    corridorXEnd = startX;
                }

                for (int j = corridorXStart; j <= corridorXEnd; j++)
                {
                    baseMap.mapSquares[j, yCoord].terrain = MapTerrain.Corridor;
                }

                //down
                for (int i = yCoord + 1; i <= endY; i++)
                {
                    baseMap.mapSquares[endX, i].terrain = MapTerrain.Corridor;
                }
            }
        }

        //Draw squares for map
        private void DrawRoomOnMap(RogueRoom room)
        {
            int lx = room.roomX - room.roomWidth / 2;
            int rx = lx + room.roomWidth - 1;
            int ty = room.roomY - room.roomHeight / 2;
            int by = ty + room.roomHeight - 1;

            for (int i = lx; i <= rx; i++)
            {
                //Top row
                baseMap.mapSquares[i, ty].terrain = MapTerrain.Wall;
                //Bottom row
                baseMap.mapSquares[i, by].terrain = MapTerrain.Wall;
            }

            for (int i = ty; i <= by; i++)
            {
                //Left row
                baseMap.mapSquares[lx, i].terrain = MapTerrain.Wall;
                //Right row
                baseMap.mapSquares[rx, i].terrain = MapTerrain.Wall;
            }
        }

        //Get X Y coords from roomNo
        private void RoomCoords(int roomNo, out int roomX, out int roomY)
        {

            roomX = roomNo / subSquareDim;
            roomY = roomNo - (roomX * subSquareDim);
        }

        //Get roomNo from X Y coords
        private int MapRoomNum(int roomX, int roomY)
        {
            return roomX * subSquareDim + roomY;
        }


        //Find connected (param==true) neighbours of a room
        //Find unconnected (param==false) neighbours of a room
        private List<int> FindConnectedNeighbours(int startRoom, bool connected)
        {
            List<int> unconnectedN = new List<int>();

            int roomX, roomY;

            RoomCoords(startRoom, out roomX, out roomY);

            //Neighbours are in compass directions only

            //E neighbour
            if (roomX != subSquareDim - 1)
            {
                if (MapRoom(roomX + 1, roomY).connected == connected)
                {
                    unconnectedN.Add(MapRoomNum(roomX + 1, roomY));
                }
            }

            //W neighbour
            if (roomX != 0)
            {
                if (MapRoom(roomX - 1, roomY).connected == connected)
                {
                    unconnectedN.Add(MapRoomNum(roomX - 1, roomY));
                }
            }

            //N neighbour
            if (roomY != 0)
            {
                if (MapRoom(roomX, roomY - 1).connected == connected)
                {
                    unconnectedN.Add(MapRoomNum(roomX, roomY - 1));
                }
            }

            //S neighbour
            if (roomY != subSquareDim - 1)
            {
                if (MapRoom(roomX, roomY + 1).connected == connected)
                {
                    unconnectedN.Add(MapRoomNum(roomX, roomY + 1));
                }
            }

            return unconnectedN;
        }

        private List<int> FindAllNeighbours(int startRoom)
        {
            List<int> neighbourRooms = new List<int>();

            int roomX, roomY;

            RoomCoords(startRoom, out roomX, out roomY);

            //Neighbours are in compass directions only

            //E neighbour
            if (roomX != subSquareDim - 1)
            {
                neighbourRooms.Add(MapRoomNum(roomX + 1, roomY));
            }

            //W neighbour
            if (roomX != 0)
            {
                    neighbourRooms.Add(MapRoomNum(roomX - 1, roomY));
            }

            //N neighbour
            if (roomY != 0)
            {
                    neighbourRooms.Add(MapRoomNum(roomX, roomY - 1));
            }

            //S neighbour
            if (roomY != subSquareDim - 1)
            {
                    neighbourRooms.Add(MapRoomNum(roomX, roomY + 1));
            }

            return neighbourRooms;
        }

        void RandomRoom(out int roomX, out int roomY)
        {

             int roomNo = rand.Next(subSquareDim * subSquareDim);
             roomX = roomNo % subSquareDim;
             roomY = roomNo - roomX;
        }

        public int Width
        {
            set
            {
                width = value;
            }
        }
        public int Height {
            set
            {
                height = value;
            }
        }


    }
}
