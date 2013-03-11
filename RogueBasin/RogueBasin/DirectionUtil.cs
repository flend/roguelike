﻿using System;
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

        /// <summary>
        /// Return angular heading we will be set to after a move
        /// </summary>
        /// <param name="oldPoint"></param>
        /// <param name="newPoint"></param>
        /// <returns></returns>
        public static double DirectionBetweenPoints(Point oldPoint, Point newPoint) {
 
            Point deltaDir = newPoint - oldPoint;

            int deltaX = deltaDir.x;
            int deltaY = deltaDir.y;

            /*
            //Ensure that single square moves are dealt with correctly

            if(Math.Abs(deltaX) >= -1 && Math.Abs(deltaX) <= 1 &&
                Math.Abs(deltaY) >= -1 && Math.Abs(deltaY) <= 1) {

                return DirectionUtil.DirectionFromMove(deltaX, deltaY);
            }*/

            //Otherwise do by angle

            double angle = Math.Atan(deltaY / (double) deltaX);

			if (deltaX < 0)
				angle += Math.PI;

            return angle;
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

        /// <summary>
        /// Direction from a movement direction. Currently only handles 1 square moves (max offset 1 -1)
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        /// <returns></returns>
        public static double DirectionFromMove(int deltaX, int deltaY)
        {
            if(Math.Abs(deltaX) > 1 || Math.Abs(deltaY) > 1) {

                string error = "DirectionFromMove: Deltas too big. Throwing exception - can implement this feature";
                LogFile.Log.LogEntry(error);
                throw new ApplicationException(error);
            }

            // NE: -3p/8 -> -pi/8
            // E: -pi/8 -> pi/8
            // SE: pi/8 -> 3pi/8
            // S: 3pi/8 -> 5pi/8
            // SW: 5pi/8 -> 7pi/8
            // W: 7pi/8 -> 9pi/8
            // NW: 9pi/8 -> 11pi/8
            // N: 11pi/8 -> 13pi/2 & -p/2 -> -3p/8

            if(deltaX == 0 && deltaY == -1)
                return -1 * Math.PI / 2;
            if(deltaX == 1 && deltaY == -1)
                return -1 * Math.PI / 4;
            if(deltaX == 1 && deltaY == 0)
                return 0;
            if(deltaX == 1 && deltaY == 1)
                return Math.PI / 4;
            if(deltaX == 0 && deltaY == 1)
                return Math.PI / 2;
            if(deltaX == -1 && deltaY == 1)
                return 3 * Math.PI / 4;
            if(deltaX == -1 && deltaY == 0)
                return Math.PI;
            if(deltaX == -1 && deltaY == -1)
                return 5 * Math.PI / 4;

            string error2 = "DirectionFromMove: Can't find heading (BUG) returning North";
            LogFile.Log.LogEntry(error2);
            return -1 * Math.PI / 2;

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
        /// Find the square adjacent to start at the angle specified
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static List<Point> SurroundingPointsFromDirection(double angle, Point start)
        {

            Vector3 unitVector = new Vector3(Math.Cos(angle), Math.Sin(angle), 0);

            //Build dictionary of the angle of vectors to the surrounding spaces
            List<KeyValuePair<double, Point>> surroundingSpaces = new List<KeyValuePair<double,Point>>();

            surroundingSpaces.Add(new KeyValuePair<double, Point>(-1 * Math.PI / 2, new Point(0, -1)));
            surroundingSpaces.Add(new KeyValuePair<double, Point>(-1 * Math.PI / 4, new Point(1, -1)));
            surroundingSpaces.Add(new KeyValuePair<double, Point>(0, new Point(1, 0)));
            surroundingSpaces.Add(new KeyValuePair<double, Point>(Math.PI / 4, new Point(1, 1)));
            surroundingSpaces.Add(new KeyValuePair<double, Point>(Math.PI / 2, new Point(0, 1)));
            surroundingSpaces.Add(new KeyValuePair<double, Point>(3 * Math.PI / 4, new Point(-1, 1)));
            surroundingSpaces.Add(new KeyValuePair<double, Point>(Math.PI, new Point(-1, 0)));
            surroundingSpaces.Add(new KeyValuePair<double, Point>(5 * Math.PI / 4, new Point(-1, -1)));

            //Sort the dictionary in terms of abs difference from the requested angle

            List<KeyValuePair<double, Point>> myList = surroundingSpaces;

            myList.Sort( (firstPair, nextPair) =>
            {
                return (Math.Abs(firstPair.Key - angle).CompareTo(Math.Abs(nextPair.Key - angle)));
            });
            
            //Top 3 in dictionary ought to be the closest adjacent points to that direction
            List<Point> retPoints = new List<Point>();

            retPoints.Add(start + myList[0].Value);
            retPoints.Add(start + myList[1].Value);
            retPoints.Add(start + myList[2].Value);

            return retPoints;
        }
    }
}
