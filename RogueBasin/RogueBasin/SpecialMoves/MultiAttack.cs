using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    /// <summary>
    /// Attack a creature. Move into a square (have to move) adjacent to a different monster. Can't move back in the same direction
    /// </summary>
    public class MultiAttack : SpecialMove
    {
        //Really private, accessors for serialization only

        public int moveCounter { get; set; }

        public int lastDeltaX { get; set; }
        public int lastDeltaY { get; set; }

        Monster target = null; //doesn't need to be serialized
        public int lastSpeedInc { get; set; }
        public int speedInc { get; set; }
        //Point monsterSquare = new Point(-1, -1);

        public MultiAttack()
        {
            moveCounter = 0;
        }

        public override bool CheckAction(bool isMove, Point locationAfterMove)
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            //No interruptions or standing still
            if (!isMove || locationAfterMove == player.LocationMap)
            {
                FailInterrupted();
                return false;
            }

            //First move must be an attack
            if (moveCounter == 0)
            {

                int firstDeltaX = locationAfterMove.x - player.LocationMap.x;
                int firstDeltaY = locationAfterMove.y - player.LocationMap.y;

                //Set lastDeltaX to something unreasonable so we never repetition fail on the first move
                lastDeltaX = -5;
                lastDeltaY = -5;

                //Reset speed counter
                speedInc = 0;

                //Check it is an attack
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x, locationAfterMove.y));

                //Is there a monster here? If so, we will attack it
                if (squareContents.monster != null)
                {
                    //Set move counter to 1 and drop back, the normal code will do the first attack
                    moveCounter = 1;
                    target = squareContents.monster;

                    LogFile.Log.LogEntryDebug("MultiAttack Begins", LogDebugLevel.Medium);
                    return true;
                }
                else
                {
                    //Not an attack
                    moveCounter = 0;
                    return false;
                }
            }

            //Any subsequent move can be an attack if
            //a) it's not the opposite to our last move
            //b) you are adjacent to a new monster

            else {

                int secondXDelta = locationAfterMove.x - player.LocationMap.x;
                int secondYDelta = locationAfterMove.y - player.LocationMap.y;

                //Opposite last move
                if (secondXDelta == -lastDeltaX && secondYDelta == -lastDeltaY)
                {
                    //Reset

                    FailRepetition();
                    return false;
                }

                lastDeltaX = secondXDelta;
                lastDeltaY = secondYDelta;

                //Check this is a valid location to move into
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, locationAfterMove);

                //Monster
                if (squareContents.monster != null)
                {
                    FailBlocked();
                    return false;
                }

                //Bad terrain
                if (!dungeon.MapSquareIsWalkable(player.LocationLevel, locationAfterMove))
                {
                    FailBlocked();
                    return false;
                }

                //Check our surrounding squares and make a list of possible targets
                List<Monster> possibleTargets = new List<Monster>();

                squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x - 1, locationAfterMove.y));
                if (squareContents.monster != null)
                    possibleTargets.Add(squareContents.monster);

                squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x + 1, locationAfterMove.y));
                if (squareContents.monster != null)
                    possibleTargets.Add(squareContents.monster);

                squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x, locationAfterMove.y + 1));
                if (squareContents.monster != null)
                    possibleTargets.Add(squareContents.monster);

                squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x, locationAfterMove.y - 1));
                if (squareContents.monster != null)
                    possibleTargets.Add(squareContents.monster);

                squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x - 1, locationAfterMove.y - 1));
                if (squareContents.monster != null)
                    possibleTargets.Add(squareContents.monster);

                squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x + 1, locationAfterMove.y - 1));
                if (squareContents.monster != null)
                    possibleTargets.Add(squareContents.monster);

                squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x - 1, locationAfterMove.y + 1));
                if (squareContents.monster != null)
                    possibleTargets.Add(squareContents.monster);

                squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x + 1, locationAfterMove.y + 1));
                if (squareContents.monster != null)
                    possibleTargets.Add(squareContents.monster);

                //Does our list of possible targets contain a new monster
                List<Monster> newMonsters = possibleTargets.FindAll(x => x != target);

                //Don't count charmed and passives
                newMonsters = newMonsters.FindAll(x => !x.Passive);
                newMonsters = newMonsters.FindAll(x => !x.Charmed);

                if (newMonsters.Count == 0)
                {
                    FailNoNewMonsters();
                    return false;
                }

                //Otherwise, pick a random monster to attack this turn
                target = newMonsters[Game.Random.Next(newMonsters.Count)];

                moveCounter++;

                //Will attack it during DoMove
                return true;
            }
        }

        private void ResetMove() {
            moveCounter = 0;

            //Remove any speed up effects given to the player
            Game.Dungeon.Player.Speed -= speedInc;
            speedInc = 0;
        }

        private void FailNoNewMonsters()
        {
            ResetMove();
            LogFile.Log.LogEntryDebug("MultiAttack ended - no new monsters", LogDebugLevel.Medium);
        }

        private void FailRepetition()
        {
            ResetMove();
            LogFile.Log.LogEntryDebug("MultiAttack repetition fail", LogDebugLevel.Medium);
        }

        private void FailBlocked()
        {
            LogFile.Log.LogEntryDebug("MultiAttack ended - blocked", LogDebugLevel.Medium);
            ResetMove();
        }

        private void FailInterrupted()
        {
            LogFile.Log.LogEntryDebug("MultiAttack ended - interrupted", LogDebugLevel.Medium);
            ResetMove();
        }

        public override bool MoveComplete()
        {
            //Carry out any bar the 1st move (which is handled by the normal code)
            if (moveCounter > 1)
                return true;
            return false;
        }

        public override void DoMove(Point locationAfterMove)
        {
            //Attack the monster in its square with bonuses
            //Bonus depends on moveNumber
            int bonus = moveCounter - 1;

            if (bonus > 5)
                bonus = 5;

            //Bonus to hit and damage
            Game.MessageQueue.AddMessage("Multi Attack!");
            CombatResults results = Game.Dungeon.Player.AttackMonsterWithModifiers(target, bonus * 2, 0, bonus * 2, 0);
             
            //Give the player a small speed boost
            if (bonus <= 5)
            {
                speedInc += 25;
                Game.Dungeon.Player.Speed += 25;
            }
           

            //Move into destination square (already checked this was OK)
            Game.Dungeon.MovePCAbsoluteSameLevel(locationAfterMove.x, locationAfterMove.y);

            LogFile.Log.LogEntry("MultiAttack free attack: " + bonus);
            
        }

        public override void ClearMove()
        {
            moveCounter = 0;
        }

        public override string MovieRoot()
        {
            return "multiattack";
        }

        public override string MoveName()
        {
            return "Multiple Targets";
        }

        public override string Abbreviation()
        {
            return "Mult";
        }

        /// <summary>
        /// Effectively goes on for ever, but stop getting better at 5
        /// </summary>
        /// <returns></returns>
        public override int TotalStages()
        {
            return 5;
        }

        public override int CurrentStage()
        {
            return moveCounter;
        }
    }
}
