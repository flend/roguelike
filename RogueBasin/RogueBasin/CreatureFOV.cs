using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using RogueBasin.LibTCOD;

namespace RogueBasin
{
    /** Lightweight wrapper - stops classes uses Fov from altering the map */
    public class WrappedFOV
    {
        TCODFovWrapper fov;

        public WrappedFOV(TCODFovWrapper fov)
        {
            this.fov = fov;
        }

        public bool CheckTileFOV(int level, Point pointToCheck) {

            return fov.CheckTileFOV(level, pointToCheck);
        }
    }


    /** FOV for querying. No guarantees about keeping state, so just obtain from Dungeon, use, then dispose.
     *  Not serializable */
    public class CreatureFOV
    {
        public enum CreatureFOVType { 
            Base, Triangular
        }

        WrappedFOV fov;
        CreatureFOVType type;
        Creature creature;
        Point overrideLocation = null;

        public CreatureFOV(Creature creature, WrappedFOV fov, CreatureFOVType creatureFOVType)
        {
            this.fov = fov;
            this.type = creatureFOVType;
            this.creature = creature;
        }

        /// <summary>
        /// This uses an overriden location for the creature
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="fov"></param>
        /// <param name="creatureFOVType"></param>
        /// <param name="locationX"></param>
        /// <param name="locationY"></param>
        public CreatureFOV(Creature creature, WrappedFOV fov, CreatureFOVType creatureFOVType, Point overrideLocation)
        {
            this.fov = fov;
            this.type = creatureFOVType;
            this.creature = creature;
            this.overrideLocation = overrideLocation;
        }

        public bool CheckTileFOV(Point p)
        {
            return CheckTileFOV(p.x, p.y);
        }

        public bool CheckTileFOV(int x, int y)
        {
            //Check for overriden origin (creature) location
            Point origin = creature.LocationMap;
            if (overrideLocation != null)
            {
                origin = overrideLocation;
            }

            //Check game-specific FOV

            bool gameFOVPass = true;
            switch (type)
            {
                case CreatureFOVType.Base:
                    break;

                case CreatureFOVType.Triangular:
                    
                    //This allows 0 to still be see everything
                    var sightRadiusToUse = creature.SightRadius;
                    if (creature.SightRadius == 0)
                        sightRadiusToUse = creature.NormalSightRadius;

                    gameFOVPass = TriangularFOV(origin, creature.Heading, sightRadiusToUse, x, y, Math.PI / 3.6);
                    break;
            }

            if (!gameFOVPass)
                return false;
            
            //Check tcod FOV
            return fov.CheckTileFOV(creature.LocationLevel, new Point(x,y));
        }

        /// <summary>
        /// Check a triangular FOV around the creature
        /// fovAngle is the permissiveAngle on both sides
        /// </summary>
        /// <param name="point"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool TriangularFOV(Point origin, double direction, int range, int testPointX, int testPointY, double fovAngle)
        {
            //To avoid problems normalizing 0 length vectors
            if (origin.x == testPointX && origin.y == testPointY)
                return true;

            //Seed vector from direction
            Vector3 directionVector = new Vector3(Math.Cos(direction), Math.Sin(direction), 0);

            //Vector to test point
            Vector3 testPointVec = new Vector3(testPointX - origin.x, testPointY - origin.y, 0);

            //Find angle between directionVec and test point vector

            double dirAngle = Vector3.Angle(testPointVec, directionVector);
            
            //Is angle less than 50 deg? (more permissive than 45)
            if (Math.Abs(dirAngle) > fovAngle)
                return false;

            //Also fail if it's behind us. Extra check seems to be necessary
            if (Vector3.DotProduct(testPointVec, directionVector) < 0)
                return false;

            //Is magnitude of testPointVec < range?
            if (testPointVec.Magnitude > range)
                return false;

            return true;
        }

        private bool LineFOV(Point origin, Direction direction, int range, int width, int testPointX, int testPointY)
        {
            //Seed vector from direction
            Point directionVector = DirectionUtil.VectorFromDirection(direction);

            Point testPointVector = new Point(testPointX, testPointY) - origin;

            //Find shortest perpendicular vector to testPoint, and where it joins line described by directionVector
            //Check length of perpendicular vector, check length of vectormalong directionVector to intersection point

            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the points in a triangular target (i.e. shotgun weapon) from origin to target.
        /// Only returns points within FOV. Moral: If you can see it, you can shoot it.
        /// fovAngle = spread of target
        /// </summary>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public List<Point> GetPointsForTriangularTargetInFOV(Point origin, Point target, Map mapLevel, int range, double fovAngle)
        {
            List<Point> triangularPoints = new List<Point>();

            double angle = DirectionUtil.AngleFromOriginToTarget(origin, target);

            for (int i = origin.x - range; i < origin.x + range; i++)
            {
                for (int j = origin.y - range; j < origin.y + range; j++)
                {
                    //Check for creature's FOV
                    //If OK, check to see if it falls within a TriangularFOV (blast radius)
                    if (i >= 0 && i < mapLevel.width && j >= 0 && j < mapLevel.height)
                    {
                        if (CheckTileFOV(i, j) && CreatureFOV.TriangularFOV(origin, angle, range, i, j, fovAngle))
                        {
                            triangularPoints.Add(new Point(i, j));
                        }
                    }
                }
            }

            return triangularPoints;
        }

        /// <summary>
        /// Get points on a line in order.
        /// /// Only returns points within FOV. Moral: If you can see it, you can shoot it.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<Point> GetPathLinePointsInFOV(Point start, Point end)
        {
            List<Point> pointsToRet = new List<Point>();

            foreach (Point p in Utility.GetPointsOnLine(start, end))
            {
                if (CheckTileFOV(p.x, p.y))
                {
                    pointsToRet.Add(p);
                }
            }           

            return pointsToRet;
        }



    }
}
