using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public class Movement
    {
        private Dungeon dungeon;

        public Movement(Dungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        public enum MoveResults
        {
            StoppedByObstacle,
            InteractedWithObstacle,
            OpenedDoor,
            AttackedMonster,
            SwappedWithMonster,
            StoppedByMonster,
            InteractedWithFeature,
            NormalMove
        }

        /// <summary>
        /// Process a relative PC move, from a keypress
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal MoveResults PCMoveRelative(Point relativeMove)
        {
            Player player = dungeon.Player;

            Point newPCMapPoint = new Point(player.LocationMap.x + relativeMove.x, player.LocationMap.y + relativeMove.y);
            Location newPCLocation = new Location(player.LocationLevel, newPCMapPoint);
            return PCMoveWithInteractions(newPCLocation);
        }

        /// <summary>
        /// Process a relative PC move, from a keypress
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal MoveResults PCMoveWithInteractions(Location target)
        {
            Player player = dungeon.Player;

            //Relative moves only make sense if this is on the same level
            Point deltaMove = new Point(0, 0);
            if (target.Level == player.LocationLevel)
            {
                deltaMove = target.MapCoord - player.LocationMap;
            }

            MoveResults moveResults = MoveResults.NormalMove;

            //Moves off the map don't work

            if (!dungeon.IsValidLocationInWorld(target))
            {
                return MoveResults.StoppedByObstacle;
            }

            //Check special moves. These take precidence over normal moves. Only if no special move is ready do we do normal resolution here
            SpecialMove moveDone = dungeon.DoSpecialMove(target.MapCoord);

            bool okToMoveIntoSquare = true;

            //Apply environmental effects
            /*
            if (player.LocationLevel < dungeonInfo.LevelNaming.Count && dungeonInfo.LevelNaming[player.LocationLevel] == "Arcology")
            {
                if (!player.IsEffectActive(typeof(PlayerEffects.BioProtect)))
                {
                    player.ApplyDamageToPlayerHitpoints(5);
                }
            }*/

            bool stationaryAction = false;
            bool attackAction = false;

            //If there's no special move, do a conventional move
            if (moveDone == null)
            {
                //If square is not walkable exit, except in special conditions
                if (!dungeon.MapSquareIsWalkable(target))
                {
                    //Is there a closed door? This is a move, so return
                    if (dungeon.GetTerrainAtLocation(target) == MapTerrain.ClosedDoor)
                    {
                        dungeon.OpenDoor(target);
                        stationaryAction = true;
                        okToMoveIntoSquare = false;
                        moveResults = MoveResults.OpenedDoor;
                    }
                    else if (dungeon.GetTerrainAtLocation(target) == MapTerrain.ClosedLock)
                    {
                        //Is there a lock at the new location? Interact
                        var locksAtLocation = dungeon.LocksAtLocation(target);

                        //Try to open each lock
                        foreach (var thisLock in locksAtLocation)
                        {
                            bool thisSuccess = true;
                            if (!thisLock.IsOpen())
                            {
                                thisSuccess = thisLock.OpenLock(player);
                                if (thisSuccess)
                                    dungeon.SetTerrainAtPoint(target, MapTerrain.OpenLock);
                            }
                        }

                        stationaryAction = true;
                        okToMoveIntoSquare = false;
                        moveResults = MoveResults.InteractedWithObstacle;
                    }
                    else
                    {
                        okToMoveIntoSquare = false;
                        moveResults = MoveResults.StoppedByObstacle;
                    }
                }

                //Check for monsters in the square
                SquareContents contents = dungeon.MapSquareContents(target);

                //Monster - check for charm / passive / normal status
                if (contents.monster != null)
                {
                    Monster monster = contents.monster;

                    if (monster.Charmed)
                    {
                        //Switch monster to PC position
                        monster.LocationMap = player.LocationMap;

                        //PC will move to monster's old location
                        okToMoveIntoSquare = true;
                        moveResults = MoveResults.SwappedWithMonster;

                    }
                    else if (monster.Passive)
                    {
                        if (!player.Running)
                        {
                            //Attack the passive creature.
                            dungeon.DoMeleeAttackOnMonster(deltaMove, target.MapCoord);
                            okToMoveIntoSquare = false;

                            attackAction = true;
                            stationaryAction = true;
                            moveResults = MoveResults.AttackedMonster;
                        }
                        else
                        {
                            stationaryAction = true;
                            okToMoveIntoSquare = false;
                            moveResults = MoveResults.StoppedByMonster;
                        }
                    }
                    else
                    {
                        //Monster hostile 

                        dungeon.DoMeleeAttackOnMonster(deltaMove, target.MapCoord);

                        okToMoveIntoSquare = false;

                        stationaryAction = true;
                        attackAction = true;
                        moveResults = MoveResults.AttackedMonster;
                    }
                }

                //Ranged melee weapons - yuck
                if (player.GetEquippedMeleeWeapon() is Items.Pole)
                {
                    //Check 2 squares ahead
                    for (int i = 0; i < 2; i++)
                    {
                        Point p = target.MapCoord + deltaMove * (i + 1);

                        SquareContents poleContents = dungeon.MapSquareContents(player.LocationLevel, p);
                        if (poleContents.monster != null && !poleContents.monster.Charmed)
                        {
                            //Pole will start from the origin anyway
                            dungeon.DoMeleeAttackOnMonster(deltaMove, target.MapCoord);

                            stationaryAction = true;
                            attackAction = true;
                            moveResults = MoveResults.AttackedMonster;
                            break;
                        }
                    }
                }

                //Apply movement effects to counters
                if (stationaryAction)
                {
                    if (attackAction && !player.LastMoveWasMeleeAttack)
                    {

                    }
                    else
                    {
                        player.ResetTurnsMoving();
                    }

                    player.ResetTurnsSinceAction();
                    player.AddTurnsInactive();

                    if (attackAction)
                        player.LastMoveWasMeleeAttack = true;
                }
                else
                {
                    player.LastMoveWasMeleeAttack = false;
                    player.AddTurnsSinceAction();
                    if (deltaMove == new Point(0, 0))
                    {
                        player.ResetTurnsMoving();
                        player.AddTurnsInactive();
                    }
                    else
                    {
                        player.ResetTurnsInactive();
                        player.AddTurnsMoving();
                    }
                }

                //If not OK to move, return here
                if (!okToMoveIntoSquare)
                    return moveResults;

                MovePCAbsoluteNoIteractions(target, false);

                dungeon.CheckForNewMonstersInFoV();

                //Auto-pick up any items
                if (contents.items.Count > 0)
                {
                    //Pick up first item only
                    //Might help if the player makes a massive pile
                    dungeon.PickUpItemInSpace();
                }

                //If there is an active feature, auto interact
                bool activeFeatureInteract = dungeon.InteractWithActiveFeature();

                //If there is a useable feature, auto interact
                bool useableFeatureInteract = dungeon.InteractWithUseableFeature();

                if (activeFeatureInteract || useableFeatureInteract)
                {
                    moveResults = MoveResults.InteractedWithFeature;
                }
            }

            //Run any entering square messages
            //Happens for both normal and special moves

            //Tell the player if there are items here
            //Don't tell the player again if they haven't moved

            Item itemAtSpace = dungeon.ItemAtSpace(player.LocationLevel, player.LocationMap);
            if (itemAtSpace != null)
            {
                Game.MessageQueue.AddMessage("There is a " + itemAtSpace.SingleItemDescription + " here.");
            }

            //Tell the player if there are multiple items in the square
            if (dungeon.MultipleItemAtSpace(player.LocationLevel, player.LocationMap))
            {
                Game.MessageQueue.AddMessage("There are multiple items here.");
            }

            return moveResults;
        }

        internal bool MoveMonsterAbsolute(Monster monsterToMove, int level, Point location)
        {
            monsterToMove.LocationLevel = level;
            monsterToMove.LocationMap = location;

            //Do anything needed with the AI, not needed right now

            return true;
        }
        internal bool MovePCAbsoluteNoInteractions(int level, Point location)
        {
            return MovePCAbsoluteNoIteractions(new Location(level, location), false);
        }

        internal bool MovePCAbsoluteNoInteractions(Location location)
        {
            return MovePCAbsoluteNoIteractions(location, false);
        }

        /// <summary>
        /// Move PC to an absolute square (doesn't check the contents). Runs triggers.
        /// Doesn't do any checking at the mo, should return false if there's a problem.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool MovePCAbsoluteNoIteractions(Location location, bool runTriggersAlways)
        {
            Player player = dungeon.Player;

            if (!dungeon.IsValidLocationInWorld(location))
            {
                return false;
            }

            //Don't run triggers if we haven't moved
            if (player.Location == location && !runTriggersAlways)
            {
                return true;
            }

            //Update player location
            player.Location = location;

            //Kill monsters if they are going to be under the PC
            foreach (Monster monster in dungeon.Monsters)
            {
                if (monster.InSameSpace(player))
                {
                    dungeon.KillMonster(monster, false);
                }
            }

            dungeon.RunDungeonTriggers(player.LocationLevel, player.LocationMap);

            return true;
        }

        public enum RunToTargetStatus
        {
            OK, Unwalkable, CantRunToSelf, CantRunBetweenLevels, UnwalkableDestination, CantRouteToDestination
        }

        public RunToTargetStatus CanRunToTarget(Location target)
        {
            Player player = dungeon.Player;
            
            if (target.Level != player.Location.Level)
            {
                return RunToTargetStatus.CantRunBetweenLevels;
            }

            if (target.MapCoord == player.Location.MapCoord)
            {
                return RunToTargetStatus.CantRunToSelf;
            }

            if(!dungeon.MapSquareIsWalkableOrInteractable(target))
            {
                return RunToTargetStatus.UnwalkableDestination;
            }

            if(GetPlayerRunningPath(target.MapCoord).IsEmpty())
            {
                return RunToTargetStatus.CantRouteToDestination;
            }

            return RunToTargetStatus.OK;
        }

        public IEnumerable<Point> GetPlayerRunningPath(Point destination)
        {
            IEnumerable<Point> path = dungeon.Pathing.GetPathToSquare(dungeon.Player.LocationLevel, dungeon.Player.LocationMap, destination, Pathing.PathingPermission.IgnoreDoorsAndLocks, true);
            if (path == null || !path.Skip(1).Any())
            {
                return Enumerable.Empty<Point>();
            }
            return path.Skip(1);
        }

        public bool MeleeTargetAtMovementLocation(Location target)
        {
            var sq = dungeon.MapSquareContents(target);

            if (sq.monster != null)
            {
                return true;
            }

            return false;
        }

    }
}
