using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{

    public class PointInRoom {

        public int X { get; set; }
        public int Y { get; set; }
        public int RoomX { get; set; }
        public int RoomY { get; set; }
        public int RoomWidth { get; set; }
        public int RoomHeight { get; set; }

        /// <summary>
        /// roomX, roomY should be TL wall, roomX + roomWidth, roomY + roomHeight should be BR wall
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="roomX"></param>
        /// <param name="roomY"></param>
        /// <param name="roomWidth"></param>
        /// <param name="roomHeight"></param>
        public PointInRoom(int x, int y, int roomX, int roomY, int roomWidth, int roomHeight)
        {
            this.X = x;
            this.Y = y;
            this.RoomX = roomX;
            this.RoomY = roomY;
            this.RoomWidth = roomWidth;
            this.RoomHeight = roomHeight;
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
        public List<Point> Waypoints { get; set; }

        public CreaturePatrol(Point startPos, List<Point> waypoints)
        {
            this.StartPos = startPos;
            this.Waypoints = waypoints;
        }

    }
    
    /// <summary>
    /// All random generators in FlatlineRL should return this, so that we can intelligently place creatures etc.
    /// </summary>
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
    }
}
