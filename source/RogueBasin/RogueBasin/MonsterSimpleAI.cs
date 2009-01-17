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

            //If we're not moving quit at this point, otherwise the target square will be the one we're in
            if (moveX == 0 && moveY == 0)
            {
                return;
            }

            //Check this is a valid move
            bool validMove = false;
            Point newLocation = new Point(LocationMap.x + moveX, LocationMap.y + moveY);

            validMove = Game.Dungeon.MapSquareCanBeEntered(LocationLevel, newLocation);

            //Give up if this is not a valid move
            if (!validMove)
                return;

            //Check if the square is occupied by a PC or monster
            SquareContents contents = Game.Dungeon.MapSquareContents(LocationLevel, newLocation);
            bool okToMoveIntoSquare = false;

            if(contents.empty) {
                okToMoveIntoSquare = true;
            }

            if (contents.player != null)
            {
                //Attack the player
                CombatResults result = AttackPlayer(contents.player);

                if(result == CombatResults.DefenderDied) {
                    //Bad news for the player here!
                    okToMoveIntoSquare = true;
                }
            }

            if (contents.monster != null)
            {
                //Attack the monster
                CombatResults result = AttackMonster(contents.monster);

                if(result == CombatResults.DefenderDied) {
                    okToMoveIntoSquare = true;
                }
            }

            //Move if allowed
            if (okToMoveIntoSquare)
            {
                LocationMap = newLocation;
            }
        }

        public override CombatResults AttackPlayer(Player player)
        {
            LogFile.Log.LogEntry(this.Representation + " attacks player.");

            return CombatResults.NeitherDied;
        }

        public override CombatResults AttackMonster(Monster monster)
        {
            string msg = this.Representation + " attacked " + monster.Representation;
            LogFile.Log.LogEntry(msg);

            //Defender always dies
            Game.Dungeon.KillMonster(monster);

            msg = this.Representation + " killed " + monster.Representation + " !";
            LogFile.Log.LogEntry(msg);
            Game.MessageQueue.AddMessage(msg);

            return CombatResults.DefenderDied;
        }
    }
}
