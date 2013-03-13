using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    public class PointInRoom {

        public int X { get; set; }
        int Y  { get; set; }
        int RoomX  { get; set; }
        int RoomY  { get; set; }
        int RoomWidth  { get; set; }
        int RoomHeight { get; set; }

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
    }
}
