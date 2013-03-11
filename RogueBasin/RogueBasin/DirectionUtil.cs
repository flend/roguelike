using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// A direction, for facing etc.
    /// </summary>
    public enum Direction
    {
        N, NE, E, SE, S, SW, W, NW
    }

    static public class DirectionUtil
    {

        public static Direction DirectionBetweenPoints(Point oldPoint, Point newPoint) {
 
            Point deltaDir = newPoint - oldPoint;

            int deltaX = deltaDir.x;
            int deltaY = deltaDir.y;

            //Ensure that single square moves are dealt with correctly

            if(Math.Abs(deltaX) >= -1 && Math.Abs(deltaX) <= 1 &&
                Math.Abs(deltaY) >= -1 && Math.Abs(deltaY) <= 1) {

                return DirectionUtil.DirectionFromMove(deltaX, deltaY);
            }

            //Otherwise do by angle

            double angle = Math.Atan(deltaY / (double) deltaX);

			if (deltaX < 0)
				angle += Math.PI;

			// NE: -3p/8 -> -pi/8
			// E: -pi/8 -> pi/8
			// SE: pi/8 -> 3pi/8
			// S: 3pi/8 -> 5pi/8
			// SW: 5pi/8 -> 7pi/8
			// W: 7pi/8 -> 9pi/8
			// NW: 9pi/8 -> 11pi/8
			// N: 11pi/8 -> 3pi/2 & -p/2 -> -3p/8

			// >= start < end

            Direction thisDirection;

			if (angle < -1.1781) {
				thisDirection = Direction.N;
			} else if (angle < -0.3927) {
				thisDirection = Direction.NE;
			} else if (angle < 0.3927) {
				thisDirection = Direction.E;
			} else if (angle < 1.1781) {
				thisDirection = Direction.SE;
			} else if (angle < 1.9635) {
				thisDirection = Direction.S;
			} else if (angle < 2.7489) {
				thisDirection = Direction.SW;
			} else if (angle < 3.5343) {
				thisDirection = Direction.W;
			} else if (angle < 4.3197) {
				thisDirection = Direction.NW;
			} else {
				thisDirection = Direction.N;
			}

            return thisDirection;
		}

        /// <summary>
        /// Direction from a movement direction. Currently only handles 1 square moves (max offset 1 -1)
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        /// <returns></returns>
        public static Direction DirectionFromMove(int deltaX, int deltaY)
        {
            if(Math.Abs(deltaX) > 1 || Math.Abs(deltaY) > 1) {

                string error = "DirectionFromMove: Deltas too big. Throwing exception - can implement this feature";
                LogFile.Log.LogEntry(error);
                throw new ApplicationException(error);
            }

            if(deltaX == 0 && deltaY == -1)
                return Direction.N;
            if(deltaX == 1 && deltaY == -1)
                return Direction.NE;
            if(deltaX == 1 && deltaY == 0)
                return Direction.E;
            if(deltaX == 1 && deltaY == 1)
                return Direction.SE;
            if(deltaX == 0 && deltaY == 1)
                return Direction.S;
            if(deltaX == -1 && deltaY == 1)
                return Direction.SW;
            if(deltaX == -1 && deltaY == 0)
                return Direction.W;
            if(deltaX == -1 && deltaY == -1)
                return Direction.NW;

            string error2 = "DirectionFromMove: Can't find heading (BUG) returning North";
            LogFile.Log.LogEntry(error2);
            return Direction.N;

        }

        /// <summary>
        /// Offset start by one square in direction of direction
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static Point NextPointFromDirection(Direction direction, Point start)
        {
            switch (direction)
            {
                case Direction.N:
                    return start + new Point(0, -1);
                case Direction.NE:
                    return start + new Point(1, -1);
                case Direction.E:
                    return start + new Point(1, 0);
                case Direction.SE:
                    return start + new Point(1, 1);
                case Direction.S:
                    return start + new Point(0, 1);
                case Direction.SW:
                    return start + new Point(-1, 1);
                case Direction.W:
                    return start + new Point(-1, 0);
                case Direction.NW:
                    return start + new Point(-1, -1);

            }

            //Impossible
            return start;
        }
    }
}
