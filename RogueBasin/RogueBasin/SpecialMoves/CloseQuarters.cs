using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    public class CloseQuarters : SpecialMove
    {
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        public int xDelta { get; set; }
        public int yDelta { get; set; }

        public int firstDeltaX { get; set; }
        public int firstDeltaY { get; set; }

        public int lastDeltaX { get; set; }
        public int lastDeltaY { get; set; }

        Monster target = null; //doesn't need to be serialized
        Point monsterSquare = new Point(-1, -1);

        public CloseQuarters()
        {
            moveCounter = 0;
        }

        public override void CheckAction(bool isMove, Point locationAfterMove)
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            //No interruptions
            if (!isMove)
            {
                //FailInterrupted();
                return;
            }

            //1 step attack, when single enemy in contact and 3 cardinal directions of target are walls or other unwalkable things
            //and attack is in cardinal direction


            firstDeltaX = locationAfterMove.x - player.LocationMap.x;
            firstDeltaY = locationAfterMove.y - player.LocationMap.y;

            //Any non-diagonal move
            //if (firstDeltaX != 0 && firstDeltaY != 0)
            //{
                //FailWrongPattern();
                //return;
           // }

            //Check it is an attack
            SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x, locationAfterMove.y));

            //Is there a monster here?
            if (squareContents.monster != null)
            {
                //Are 3 cardinal directions around the monster unwalkable?
                Point monsterLoc = squareContents.monster.LocationMap;

                int noCardinals = 0;

                if (!dungeon.MapSquareCanBeEntered(squareContents.monster.LocationLevel, new Point(monsterLoc.x - 1, monsterLoc.y)))
                    noCardinals++;

                if (!dungeon.MapSquareCanBeEntered(squareContents.monster.LocationLevel, new Point(monsterLoc.x + 1, monsterLoc.y)))
                    noCardinals++;

                if (!dungeon.MapSquareCanBeEntered(squareContents.monster.LocationLevel, new Point(monsterLoc.x, monsterLoc.y + 1)))
                    noCardinals++;

                if (!dungeon.MapSquareCanBeEntered(squareContents.monster.LocationLevel, new Point(monsterLoc.x, monsterLoc.y - 1)))
                    noCardinals++;

                if (noCardinals > 2)
                {
                    target = squareContents.monster;
                    moveCounter = 1;
                    LogFile.Log.LogEntryDebug("CloseQuarters OK", LogDebugLevel.Low);
                    return;
                }
                LogFile.Log.LogEntryDebug("CloseQuarters: not enough cardinals", LogDebugLevel.Low);
            }

            //Otherwise it's not a close quarters attack
            return;
        }

        public override bool MoveComplete()
        {
            //Carry out any bar the 1st move (which is handled by the normal code)
            if (moveCounter > 0)
                return true;
            return false;
        }

        public override void DoMove(Point locationAfterMove)
        {
            //Attack the monster in its square with bonuses

            //Bonus to hit and damage
            Game.MessageQueue.AddMessage("CloseQuarters!");
            CombatResults results = Game.Dungeon.Player.AttackMonsterWithModifiers(target, 2 , 0, 2, 0);

            moveCounter = 0;

            //Standard move into square, copied from PCMove

            bool okToMoveIntoSquare = false;

            if (results == CombatResults.DefenderDied)
            {
                okToMoveIntoSquare = true;
            }


            if (okToMoveIntoSquare)
            {
                Game.Dungeon.MovePCAbsoluteSameLevel(locationAfterMove.x, locationAfterMove.y);

                //Tell the player if there are multiple items in the square
                if (Game.Dungeon.MultipleItemAtSpace(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap))
                {
                    Game.MessageQueue.AddMessage("There are multiple items here.");
                }

                //If there is a feature and an item (feature will be hidden)
                if (Game.Dungeon.FeatureAtSpace(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap) != null &&
                    Game.Dungeon.ItemAtSpace(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap) != null)
                {
                    Game.MessageQueue.AddMessage("There is a staircase here.");
                }
            }
        }

        public override void ClearMove()
        {
            moveCounter = 0;
        }

        public override string MovieRoot()
        {
            return "closequarters";
        }
    }
}
