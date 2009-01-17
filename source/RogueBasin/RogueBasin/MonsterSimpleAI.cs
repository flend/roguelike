using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    abstract class MonsterSimpleAI : Monster
    {
        public MonsterSimpleAI()
        {

        }

        public override void ProcessTurn()
        {
            //Monster AI will do something interesting like move randomly
            Random rand = Game.Random;

            int direction = rand.Next(9);

            int moveX = 0;
            int moveY = 0;

            moveX = direction / 3 - 1;
            moveY = direction % 3 - 1;

            //If this is a valid move, then move
            Point newLocation = new Point(LocationMap.x + moveX, LocationMap.y + moveY);
            if (Game.Dungeon.MapSquareCanBeEntered(LocationLevel, newLocation))
            {
                LocationMap = newLocation;
            }
            
        }
    }
}
