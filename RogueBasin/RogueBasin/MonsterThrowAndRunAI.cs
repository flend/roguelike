using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    public abstract class MonsterThrowAndRunAI : MonsterSimpleThrowingAI
    {

        /// <summary>
        /// Override the following code from the simple throwing AI to include backing away
        /// </summary>
        /// <param name="newTarget"></param>
        protected override void FollowAndAttack(Creature newTarget) {
            
            double range = Game.Dungeon.GetDistanceBetween(this, newTarget);

            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(this);

            bool backAwayFromTarget = false;

            //Back away if we are too close & can see the target
            //If we can't see the target, don't back away
            if(range < GetMissileRange() / 2.0 && currentFOV.CheckTileFOV(newTarget.LocationMap.x, newTarget.LocationMap.y)) {
                
                 //Check our chance to back away. For a hard/clever creature this is 100. For a stupid creature it is 0.
                 if (Game.Random.Next(100) < GetChanceToBackAway())
                 {
                     backAwayFromTarget = true;
                 }
            }

            if(backAwayFromTarget && CanMove()) {

                //Target is too close, so back away before firing

                int deltaX = newTarget.LocationMap.x - this.LocationMap.x;
                int deltaY = newTarget.LocationMap.y - this.LocationMap.y;

                //Find a point in the dungeon to flee to
                int fleeX = 0;
                int fleeY = 0;

                int counter = 0;

                bool relaxDirection = false;
                bool goodPath = false;

                Point nextStep = new Point(0,0);

                int totalFleeLoops = GetTotalFleeLoops();
                int relaxDirectionAt = RelaxDirectionAt();

                do
                {
                    //This performs badly when there are few escape options and you are close to the edge of the map
                    fleeX = Game.Random.Next(Game.Dungeon.Levels[this.LocationLevel].width);
                    fleeY = Game.Random.Next(Game.Dungeon.Levels[this.LocationLevel].height);

                    //Relax conditions if we are having a hard time
                    if (counter > relaxDirectionAt)
                        relaxDirection = true;

                    //Find the square to move to
                    nextStep = Game.Dungeon.GetPathFromCreatureToPoint(this.LocationLevel, this, new Point(fleeX, fleeY));

                    //Check the square is pathable to
                    if (nextStep.x == LocationMap.x && nextStep.y == LocationMap.y)
                    {
                        counter++;
                        //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail unpathable", LogDebugLevel.Low);
                        continue;
                    }

                    //Check that the next square is in a direction away from the attacker
                    int deltaFleeX = nextStep.x - this.LocationMap.x;
                    int deltaFleeY = nextStep.y - this.LocationMap.y;

                    if (!relaxDirection)
                    {
                        if (deltaFleeX > 0 && deltaX > 0)
                        {
                            counter++;
                            //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail direction", LogDebugLevel.Low);
                            continue;
                        }
                        if (deltaFleeX < 0 && deltaX < 0)
                        {
                            counter++;
                            //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail direction", LogDebugLevel.Low);
                            continue;
                        }
                        if (deltaFleeY > 0 && deltaY > 0)
                        {
                            counter++;
                            //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail direction", LogDebugLevel.Low);
                            continue;
                        }
                        if (deltaFleeY < 0 && deltaY < 0)
                        {
                            counter++;
                            //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail direction", LogDebugLevel.Low);
                            continue;
                        }
                    }

                    //Check the square is empty
                    bool isEnterable = Game.Dungeon.MapSquareIsWalkable(this.LocationLevel, new Point(fleeX, fleeY));
                    if (!isEnterable)
                    {
                        counter++;
                        //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail enterable", LogDebugLevel.Low);
                        continue;
                    }

                    //Check the square is empty of creatures
                    SquareContents contents = Game.Dungeon.MapSquareContents(this.LocationLevel, new Point(fleeX, fleeY));
                    if (contents.monster != null)
                    {
                        counter++;
                        //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail blocked", LogDebugLevel.Low);
                        continue;
                    }


                    //Check that the target is visible from the square we are fleeing to
                    //This may prove to be too expensive

                    CreatureFOV projectedFOV = Game.Dungeon.CalculateCreatureFOV(this, nextStep);

                    if (!projectedFOV.CheckTileFOV(newTarget.LocationMap.x, newTarget.LocationMap.y))
                    {
                        counter++;
                        //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail fov", LogDebugLevel.Low);
                        continue;
                    }

                    //Otherwise we found it
                    goodPath = true;
                    break;
                } while (counter < totalFleeLoops);

                LogFile.Log.LogEntryDebug("Back away results. Count: " + counter + " Direction at: " + relaxDirectionAt + " Total: " + totalFleeLoops, LogDebugLevel.Low);

                //If we found a good path, walk it
                if (goodPath)
                {
                    LocationMap = nextStep;
                }
                else
                {
                    //If not, don't back away and attack
                    //(target in FOV)
                    CombatResults result;

                    if (newTarget == Game.Dungeon.Player)
                    {
                        result = AttackPlayer(newTarget as Player);
                    }
                    else
                    {
                        //It's a normal creature
                        result = AttackMonster(newTarget as Monster);
                    }

                    //Missile animation
                    Screen.Instance.DrawMissileAttack(this, newTarget, result, GetWeaponColor());
                }
            }

            //Close enough to fire. Not backing away (either far enough away or chose not to)
            else if (range < GetMissileRange() + 0.005)
            {
                //In range

                //Check FOV. If not in FOV, chase the player.
                if (!currentFOV.CheckTileFOV(newTarget.LocationMap.x, newTarget.LocationMap.y))
                {
                    ContinueChasing(newTarget);
                    return;
                }

                //In preference they will use their special ability rather than fighting
                //If they don't have a special ability or choose not to use it will return false

                //Special abilities are defined in derived classes

                bool usingSpecial = UseSpecialAbility();

                if (!usingSpecial)
                {
                    //In FOV - fire at the player
                    CombatResults result;

                    if (newTarget == Game.Dungeon.Player)
                    {
                        result = AttackPlayer(newTarget as Player);
                    }
                    else
                    {
                        //It's a normal creature
                        result = AttackMonster(newTarget as Monster);
                    }

                    //Missile animation
                    Screen.Instance.DrawMissileAttack(this, newTarget, result, GetWeaponColor());
                }
            }

            //Not in range, chase the target
            else
            {
                ContinueChasing(newTarget);
            }
        }

        /// <summary>
        /// Does the creature have a special ability that they use instead of missiles?
        /// </summary>
        /// <returns>If the creature used special ability. Return false if no special ability.</returns>
        protected virtual bool UseSpecialAbility()
        {
            return false;
        }

        /// <summary>
        /// Chance that monster backs away from player before attacking. Default 100.
        /// </summary>
        /// <returns></returns>
        protected virtual int GetChanceToBackAway()
        {
            return 100;
        }

        //Could be replaced with the SimpleThrowingAI one since the debug check should never happen
        private void ContinueChasing(Creature newTarget)
        {
            //If not, move towards the player

            //Return if we can't move
            if (!CanMove())
                return;

            //Find location of next step on the path towards them
            Point nextStep = Game.Dungeon.GetPathTo(this, newTarget);

            bool moveIntoSquare = true;

            //If this is the same as the target creature's location, we are adjacent. Something is wrong, but attack anyway
            if (nextStep.x == newTarget.LocationMap.x && nextStep.y == newTarget.LocationMap.y)
            {
                LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Adjacent to target and still moving towards", LogDebugLevel.High);
                //This does appear to happen now due to the unusual shaped FOVs.
                //Setting this flag stops us moving on top of another creature
                moveIntoSquare = false;

                //Fire at the player
                CombatResults result;

                if (newTarget == Game.Dungeon.Player)
                {
                    result = AttackPlayer(newTarget as Player);
                }
                else
                {
                    //It's a normal creature
                    result = AttackMonster(newTarget as Monster);
                }
            }

            //Otherwise (or if the creature died), move towards it (or its corpse)
            if (moveIntoSquare)
            {
                LocationMap = nextStep;
            }
        }
    }
}
