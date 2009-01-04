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

        int width = 180;
        int height = 50;

        //Make n x n rooms
        int subSquareDim = 5;

        int minimumRoomSize = 4;

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

            //Calculate room centres
            for (int i = 0; i < subSquareDim; i++)
            {
                for (int j = 0; j < subSquareDim; j++)
                {
                    MapRoom(i, j).roomX = subsquareWidth / 2 + subsquareWidth * i;
                    MapRoom(i, j).roomY = subsquareHeight / 2 + subsquareHeight * j;
                }
            }

            //Give rooms random sizes

            int roomSizeWidthVariation = subsquareWidth - minimumRoomSize;
            int roomSizeHeightVariation = subsquareHeight - minimumRoomSize;

            foreach (RogueRoom room in mapRooms)
            {
                room.roomWidth = minimumRoomSize + rand.Next(roomSizeWidthVariation);
                room.roomHeight = minimumRoomSize + rand.Next(roomSizeHeightVariation);
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


            LogFile.Log.LogEntry(String.Format("Generation complete, fixing iterations {0}", fixingIterations));

            //Draw rooms on map

            foreach (RogueRoom room in mapRooms)
            {
                DrawRoomOnMap(room);
            }

            //Draw corridors

            foreach (RogueRoom room in mapRooms)
            {
                foreach (int dest in room.connectedTo)
                {
                    DrawMapCorridor(room, mapRooms[dest]);
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

        private void DrawMapCorridor(RogueRoom startRoom, RogueRoom destRoom)
        {
            //Run a straight ray from the centre of start room to dest room
            //When we hit the first wall we start drawing corridor
            //When we hit the second wall we stop drawing corridor

            int startX, startY;
            int endX, endY;

            bool lineHoriz; //false == vertical

            if (startRoom.roomX > destRoom.roomX)
            {
                startX = destRoom.roomX;
                endX = startRoom.roomX;
            }
            else
            {
                startX = startRoom.roomX;
                endX = destRoom.roomX;
            }

            if (startRoom.roomY > destRoom.roomY)
            {
                startY = destRoom.roomY;
                endY = startRoom.roomY;
            }
            else
            {
                startY = startRoom.roomY;
                endY = destRoom.roomY;
            }

            int startCoord, endCoord;

            //Either the Xs or Ys ought to be the same
            if (startX == endX)
            {
                //Draw a vertical line
                lineHoriz = false;
                startCoord = startY;
                endCoord = endY;
            }
            else
            {
                lineHoriz = true;
                startCoord = startX;
                endCoord = endX;
            }

            bool drawingCorridor = false;
            bool endDrawing = false;

            int i = startCoord;

            do {
                //Find the terrain in this square
                MapTerrain terrainType;
                int squareX, squareY;

                if (lineHoriz)
                {
                    squareX = i;
                    squareY = startY;
                }
                else
                {
                    squareX = startX;
                    squareY = i;
                }

                terrainType = baseMap.mapSquares[squareX, squareY].terrain;

                if (terrainType == MapTerrain.Wall)
                {
                    //Drawing corridor already - this is the last iteration
                    if (drawingCorridor == true)
                        endDrawing = true;

                    //Not drawing corridor - start
                    drawingCorridor = true;
                }

                //Place this square with corridor
                if (drawingCorridor)
                {
                    baseMap.mapSquares[squareX, squareY].terrain = MapTerrain.Corridor;
                }

                i++;

            } while(!endDrawing);

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
