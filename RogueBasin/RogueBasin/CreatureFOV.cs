﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    /** FOV for querying. No guarantees about keeping state, so just obtain from Dungeon, use, then dispose.
     *  Not serializable */
    public class CreatureFOV
    {
        public enum CreatureFOVType { 
            Base, Triangular
        }

        TCODFov tcodFOV;
        CreatureFOVType type;
        Creature creature;
        Point overrideLocation = null;

        public CreatureFOV(Creature creature, TCODFov fov, CreatureFOVType creatureFOVType)
        {
            this.tcodFOV = fov;
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
        public CreatureFOV(Creature creature, TCODFov fov, CreatureFOVType creatureFOVType, Point overrideLocation)
        {
            this.tcodFOV = fov;
            this.type = creatureFOVType;
            this.creature = creature;
            this.overrideLocation = overrideLocation;
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
                    gameFOVPass = TriangularFOV(origin, creature.Heading, creature.SightRadius, x, y);
                    break;
            }

            if (!gameFOVPass)
                return false;
            
            //Check tcod FOV
            return tcodFOV.CheckTileFOV(x, y);
        }

        /// <summary>
        /// Check a triangular FOV around the creature
        /// </summary>
        /// <param name="point"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool TriangularFOV(Point origin, double direction, int range, int testPointX, int testPointY)
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
            if (Math.Abs(dirAngle) > Math.PI / 3.6)
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



    }
}
