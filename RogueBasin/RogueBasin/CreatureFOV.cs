using System;
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

        public CreatureFOV(Creature creature, TCODFov fov, CreatureFOVType creatureFOVType)
        {
            this.tcodFOV = fov;
            this.type = creatureFOVType;
            this.creature = creature;
        }


        public bool CheckTileFOV(int x, int y)
        {
            //Check game-specific FOV

            bool gameFOVPass = true;
            switch (type)
            {
                case CreatureFOVType.Base:
                    break;

                case CreatureFOVType.Triangular:
                    //gameFOVPass = TriangularFOV(creature.LocationMap, x, y);
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
        private bool TriangularFOV(Point origin, Direction direction, int range, int testPointX, int testPointY)
        {
            //Seed vector from direction
            Point directionVector = DirectionUtil.VectorFromDirection(direction);

            //Find angle between directionVector and test point vector
            //Is angle less than 45 deg?

            //Find range of test point vector (pythag)
            //Is range less than range?

            //Need C# vector class

            throw new NotImplementedException();
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
