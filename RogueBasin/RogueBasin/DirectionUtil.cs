using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Return angular heading we will be set to after a move
        /// </summary>
        /// <param name="oldPoint"></param>
        /// <param name="newPoint"></param>
        /// <returns></returns>
        public static double AngleFromOriginToTarget(Point origin, Point target) {
 
            Point deltaDir = target - origin;

            int deltaX = deltaDir.x;
            int deltaY = deltaDir.y;

            return Math.Atan2(deltaY, deltaX);

            /*
            //Ensure that single square moves are dealt with correctly

            if(Math.Abs(deltaX) >= -1 && Math.Abs(deltaX) <= 1 &&
                Math.Abs(deltaY) >= -1 && Math.Abs(deltaY) <= 1) {

                return DirectionUtil.DirectionFromMove(deltaX, deltaY);
            }*/

            //Otherwise do by angle

            //double angle = Math.Atan(deltaY / (double) deltaX);

			//if (deltaX < 0)
			//	angle += Math.PI;

           // return angle;
            /*
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

            return thisDirection;*/
		}

        public static double DiagonalCardinalAngleFromRelativePosition(int deltaX, int deltaY) {

            var fullAngle = Math.Atan2(deltaY, deltaX);

            var cardinalRounding = Math.Round(
             (fullAngle / (Math.PI / 4)),
             MidpointRounding.AwayFromZero) * (Math.PI / 4);

            return cardinalRounding;
        }

        /// <summary>
        /// Return a vector (integer direction, length not necessarily 1) in the direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Point VectorFromDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.N:
                    return new Point(0, -1);
                case Direction.NE:
                    return new Point(1, -1);
                case Direction.E:
                    return new Point(1, 0);
                case Direction.SE:
                    return new Point(1, 1);
                case Direction.S:
                    return new Point(0, 1);
                case Direction.SW:
                    return new Point(-1, 1);
                case Direction.W:
                    return new Point(-1, 0);
                case Direction.NW:
                    return new Point(-1, -1);
                default:
                    //Impossible
                    return new Point(0, 1);
            }
        }

        /// <summary>
        /// Find the square adjacent to start at the angle specified. Checked up to 3 points
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static List<Point> SurroundingPointsFromDirection(double angle, Point start, int noOfPoints)
        {

            Vector3 unitVector = new Vector3(Math.Cos(angle), Math.Sin(angle), 0);

            //Build dictionary of the angle of vectors to the surrounding spaces
            List<KeyValue<double, Point>> surroundingSpaces = new List<KeyValue<double,Point>>();

            //Include this point with both normally and with +2pi so that angles like 3pi / 2 pick this as the closest
            surroundingSpaces.Add(new KeyValue<double, Point>(-1 * Math.PI / 2, new Point(0, -1)));
            surroundingSpaces.Add(new KeyValue<double, Point>(-1 * Math.PI / 2 + 2 * Math.PI, new Point(0, -1)));

            surroundingSpaces.Add(new KeyValue<double, Point>(-1 * Math.PI / 4, new Point(1, -1)));
            surroundingSpaces.Add(new KeyValue<double, Point>(0, new Point(1, 0)));
            surroundingSpaces.Add(new KeyValue<double, Point>(Math.PI / 4, new Point(1, 1)));
            surroundingSpaces.Add(new KeyValue<double, Point>(Math.PI / 2, new Point(0, 1)));
            surroundingSpaces.Add(new KeyValue<double, Point>(3 * Math.PI / 4, new Point(-1, 1)));
            surroundingSpaces.Add(new KeyValue<double, Point>(Math.PI, new Point(-1, 0)));

            //Include this point with both normally and with -2pi so that angles like -pi / 2 pick this as the closest
            surroundingSpaces.Add(new KeyValue<double, Point>(5 * Math.PI / 4, new Point(-1, -1)));
            surroundingSpaces.Add(new KeyValue<double, Point>(5 * Math.PI / 4 - 2 * Math.PI, new Point(-1, -1)));

            //LogFile.Log.LogEntryDebug("points for angle: " + angle, LogDebugLevel.High);

            //Sort the dictionary in terms of abs difference from the requested angle

            List<KeyValue<double, Point>> myList = surroundingSpaces;

            myList.Sort( (firstPair, nextPair) =>
            {
                return (Math.Abs(firstPair.Key - angle).CompareTo(Math.Abs(nextPair.Key - angle)));
            });
            
            //Top 3 in dictionary ought to be the closest adjacent points to that direction
            List<Point> retPoints = new List<Point>();

            for (int i = 0; i < noOfPoints; i++)
            {
                retPoints.Add(start + myList[i].Value);
            }
            return retPoints;
        }



        /// <summary>
        /// Rotate heading, ensuring we keep within the - pi / 2 -> 3 pi / 2 angular range used for headings.
        /// All rotation of heading vectors should use this function
        /// </summary>
        /// <param name="heading"></param>
        /// <param name="angleToRotate"></param>
        /// <param name="clockwise"></param>
        /// <returns></returns>
        public static double RotateHeading(double heading, double angleToRotate, bool clockwise)
        {
            if (clockwise)
            {
                heading += angleToRotate;
            }
            else
                heading -= angleToRotate;

            if (heading < -Math.PI / 2)
                heading += 2 * Math.PI;
            if (heading > 3 * Math.PI / 2)
                heading -= 2 * Math.PI;

            return heading;
        }
    }
}
