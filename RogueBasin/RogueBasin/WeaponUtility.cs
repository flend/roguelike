using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public class WeaponUtility
    {
        private Dungeon dungeon;

        public WeaponUtility(Dungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        public List<Point> CalculateTrajectorySameLevel(Creature creature, Point target)
        {
            //Get the points along the line of where we are firing
            CreatureFOV currentFOV = dungeon.CalculateCreatureFOV(creature);
            List<Point> trajPoints = currentFOV.GetPathLinePointsInFOV(creature.LocationMap, target);

            //Also exclude unwalkable points (since we will use this to determine where our item falls
            List<Point> walkableSq = new List<Point>();
            foreach (Point p in trajPoints)
            {
                if (dungeon.MapSquareIsWalkable(creature.Location))
                    walkableSq.Add(p);
            }

            return walkableSq;
        }

        public Monster FirstMonsterInTrajectory(int level, List<Point> squares)
        {
            //Hit the first monster only
            Monster monster = null;
            foreach (Point p in squares)
            {
                //Check there is a monster at target
                SquareContents squareContents = dungeon.MapSquareContents(level, p);

                //Hit the monster if it's there
                if (squareContents.monster != null)
                {
                    monster = squareContents.monster;
                    break;
                }
            }

            return monster;
        }

    }
}
