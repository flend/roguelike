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
            
            double range = Utility.GetDistanceBetween(this, newTarget);

            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(this);
            CreatureFOV playerFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            bool backAwayFromTarget = false;

            //Back away if we are too close & can see the target
            //If we can't see the target, don't back away
            if(range < GetMissileRange() / 2.0 && currentFOV.CheckTileFOV(newTarget.LocationMap.x, newTarget.LocationMap.y)) {

                //Enforce a symmetry FOV
                if (newTarget != Game.Dungeon.Player ||
                    (newTarget == Game.Dungeon.Player && playerFOV.CheckTileFOV(this.LocationMap)))
                {

                    //Check our chance to back away. For a hard/clever creature this is 100. For a stupid creature it is 0.
                    if (Game.Random.Next(100) < GetChanceToBackAway())
                    {
                        backAwayFromTarget = true;
                    }
                }
            }

            if(backAwayFromTarget && CanMove() && WillPursue()) {

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
                    Pathing.PathingPermission permission = Pathing.PathingPermission.Normal;
                    if (CanOpenDoors())
                        permission = Pathing.PathingPermission.IgnoreDoors;

                    nextStep = Game.Dungeon.Pathing.GetPathToPoint(this.LocationLevel, this.LocationMap, new Point(fleeX, fleeY), PathingType(), permission).MonsterFinalLocation;

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
                    MoveIntoSquare(nextStep);
                    SetHeadingToTarget(newTarget);
                }
                else if(WillAttack())
                {
                    //If not, don't back away and attack
                    //(target in FOV)
                    CombatResults result;

                    //Set heading to target
                    SetHeadingToTarget(newTarget);

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
            else if (Utility.TestRange(this, newTarget, GetMissileRange()) && WillAttack())
            {
                //In range

                //Check FOV. If not in FOV, chase the player.
                if (!currentFOV.CheckTileFOV(newTarget.LocationMap.x, newTarget.LocationMap.y))
                {
                    ContinueChasing(newTarget);
                    return;
                }

                //Enforce a symmetry FOV
                if (newTarget == Game.Dungeon.Player && !playerFOV.CheckTileFOV(this.LocationMap))
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

                    //Set heading to target (only if we are a Pursuing creature, capable of adapting our heading)
                    if(WillPursue())
                        SetHeadingToTarget(newTarget);

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

        private void ContinueChasing(Creature newTarget)
        {
            //Chase the player
            //They are either out of range or out of FOV
            //For now, pursuing creatures know how to move their FOV to get the player

            //Return if we can't move or won't pursue
            if (!CanMove() || !WillPursue())
            {
                //Return to patrol mode. This allows creatures to go back to patrolling if the PC moves out of range
                AIState = SimpleAIStates.Patrol;
                LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Out of range but can't pursue, returning to patrol ", LogDebugLevel.Medium);
                return;
            }
             
            //Find location of next step on the path towards them
            Pathing.PathingResult pathingResult;

            Pathing.PathingPermission permission = Pathing.PathingPermission.Normal;
            if (CanOpenDoors())
                permission = Pathing.PathingPermission.IgnoreDoors;

            pathingResult = Game.Dungeon.Pathing.GetPathToCreature(this, newTarget, PathingType(), permission);

            //If this is the same as the target creature's location, we are adjacent. TODO: change our FOV instead of moving. 
            //We are allowed to attack in this case. TODO: more fun if not?
            if (pathingResult.MoveIsInteractionWithTarget)
            {
                LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Adjacent to target so changing FOV only ", LogDebugLevel.Low);
                
                //This does appear to happen now due to the unusual shaped FOVs (i.e. adjacent but out of FOV)
                SetHeadingToTarget(newTarget);

                if (WillAttack())
                {
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

                    //Missile animation
                    Screen.Instance.DrawMissileAttack(this, newTarget, result, GetWeaponColor());
                }
            }

            //If we are permanently blocked, return to patrol state
            if (pathingResult.TerminallyBlocked)
            {
                LogFile.Log.LogEntryDebug(this.Representation + " permanently blocked (door), returning to patrol ", LogDebugLevel.Medium);
                AIState = SimpleAIStates.Patrol;
                return;
            }

            //Update position
            MoveIntoSquare(pathingResult.MonsterFinalLocation);
            SetHeadingToTarget(newTarget);
            
        }

    }
}
