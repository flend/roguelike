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

        int width = 80;
        int height = 25;

        //Make n x n rooms
        int subSquareDim = 3;

        int minimumRoomSize = 1;

        RogueRoom[] mapRooms;

        Random rand;

        public MapGeneratorRogue()
        {

        }

        RogueRoom MapRoom(int x, int y)
        {
            return mapRooms[x * subSquareDim + y];
        }

        public Map GenerateMap()
        {
            rand = new Random();

            Map baseMap = new Map(width, height);

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

            while (connectedRooms < totalRooms)
            {

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
            }


            //Make a square
           
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
            
            //Stick the PC in the middle

            baseMap.PCStartLocation = new Point(15, 15);

            return baseMap;
        }

        //Get X Y coords from roomNo
        private void RoomCoords(int roomNo, out int roomX, out int roomY) {

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
