using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    public class RoomCoords
    {
        public int RoomX { get; set; }
        public int RoomY { get; set; }
        public int RoomWidth { get; set; }
        public int RoomHeight { get; set; }

        protected int roomFreeArea;

        /// <summary>
        /// How much free space is in the room
        /// </summary>
        public int RoomFreeArea { get { return roomFreeArea; } }

        /// <summary>
        /// roomX, roomY should be TL wall, roomX + roomWidth, roomY + roomHeight should be BR wall
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="roomX"></param>
        /// <param name="roomY"></param>
        /// <param name="roomWidth"></param>
        /// <param name="roomHeight"></param>
        public RoomCoords(int roomX, int roomY, int roomWidth, int roomHeight) {

            this.RoomX = roomX;
            this.RoomY = roomY;
            this.RoomWidth = roomWidth;
            this.RoomHeight = roomHeight;

            this.roomFreeArea = (roomHeight - 2) * (roomWidth - 2);
        }

        public RoomCoords(PointInRoom room)
        {
            this.RoomX = room.RoomX;
            this.RoomY = room.RoomY;
            this.RoomWidth = room.RoomWidth;
            this.RoomHeight = room.RoomHeight;

        }

        public Point RandomPointInRoom()
        {
            int randX = RoomX + 1 + Game.Random.Next(RoomWidth - 2);
            int randY = RoomY + 1 + Game.Random.Next(RoomHeight - 2);

            return new Point(randX, randY);
        }


    }


    public class PointInRoom {

        public int X { get; set; }
        public int Y { get; set; }
        public int RoomX { get; set; }
        public int RoomY { get; set; }
        public int RoomWidth { get; set; }
        public int RoomHeight { get; set; }
        public int RoomId { get; set; }

        /// <summary>
        /// roomX, roomY should be TL wall, roomX + roomWidth, roomY + roomHeight should be BR wall
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="roomX"></param>
        /// <param name="roomY"></param>
        /// <param name="roomWidth"></param>
        /// <param name="roomHeight"></param>
        public PointInRoom(int x, int y, int roomX, int roomY, int roomWidth, int roomHeight, int roomId)
        {
            this.X = x;
            this.Y = y;
            this.RoomX = roomX;
            this.RoomY = roomY;
            this.RoomWidth = roomWidth;
            this.RoomHeight = roomHeight;
            this.RoomId = roomId;
        }

        /// <summary>
        /// Return the x,y coords of the point in the room
        /// </summary>
        /// <returns></returns>
        public Point GetPointInRoomOnly()
        {
            return new Point(X, Y);
        }
    }

    public class CreaturePatrol
    {
        public Point StartPos { get; set; }
        public RoomCoords StartRoom { get; set; }
        public List<Point> Waypoints { get; set; }

        public CreaturePatrol(Point startPos, RoomCoords roomCoords, List<Point> waypoints)
        {
            this.StartPos = startPos;
            this.Waypoints = waypoints;
            this.StartRoom = roomCoords;
        }

        public CreaturePatrol(Point startPos, List<Point> waypoints)
        {
            this.StartPos = startPos;
            this.Waypoints = waypoints;
        }

    }
    
    /// <summary>
    /// All random generators in FlatlineRL should return this, so that we can intelligently place creatures etc.
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(MapGeneratorBSP))]
    public abstract class MapGenerator
    {
        /// <summary>
        /// Returns a random point in a room and a description of the room
        /// </summary>
        /// <returns></returns>
        public abstract PointInRoom RandomPointInRoom();

        /// <summary>
        /// Returns a random walkable point
        /// </summary>
        /// <returns></returns>
        public abstract Point RandomWalkablePoint();

        /// <summary>
        /// Produce a random start location & list of creature waypoints for patrol (around a room).
        /// </summary>
        /// <returns></returns>
        public abstract CreaturePatrol CreatureStartPosAndWaypoints(bool clockwisePatrol);

        /// <summary>
        /// Produce a random start location & list of creature waypoints for patrol (to the centre of a number of rooms).
        /// </summary>
        /// <returns></returns>
        public abstract CreaturePatrol CreatureStartPosAndWaypointsSisterRooms(bool clockwisePatrol, int numberOfWayPoints);

        public abstract Point GetPlayerStartLocation();

        public abstract List<Point> GetEntryDoor();

        /// <summary>
        /// Return coords of all rooms, for sensible / gauranteed distribution of creatures
        /// </summary>
        /// <returns></returns>
        public abstract List<RoomCoords> GetAllRooms();

        /// <summary>
        /// Returns a copy of the original map
        /// </summary>
        public abstract Map GetOriginalMap();

    }
}
