﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.SpecialMoves
{
    /// <summary>
    /// OpenSpaceAttack. Starts with an attack then you move in a clockwise diagonal box around the monster
    /// </summary>
    public class OpenSpaceAttack : SpecialMove
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

        public OpenSpaceAttack()
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
                FailInterrupted();
                return;
            }

            //First move must be an attack in a square direction
            if (moveCounter == 0)
            {
                firstDeltaX = locationAfterMove.x - player.LocationMap.x;
                firstDeltaY = locationAfterMove.y - player.LocationMap.y;

                //Any non-diagonal move
                if (firstDeltaX != 0 && firstDeltaY != 0)
                {
                    //FailWrongPattern();
                    return;
                }

                //Check it is an attack
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(locationAfterMove.x, locationAfterMove.y));

                //Is there a monster here? If so, we will attack it
                if (squareContents.monster != null)
                {
                    //Set move counter to 1 and drop back, the normal code will do the first attack
                    moveCounter = 1;
                    target = squareContents.monster;
                    monsterSquare = target.LocationMap;

                    LogFile.Log.LogEntryDebug("OpenSpaceAttack Stage: " + moveCounter, LogDebugLevel.Medium);
                }
                else
                {
                    //Not an attack
                    moveCounter = 0;
                }
                return;
            }

            //Move after an attack
            if(moveCounter == 1) {

                //Check this is a valid location to move into
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, locationAfterMove);

                //Monster
                if (squareContents.monster != null)
                {
                    FailBlocked();
                    return;
                }

                //Bad terrain
                if (!dungeon.MapSquareCanBeEntered(player.LocationLevel, locationAfterMove))
                {
                    FailBlocked();
                    return;
                }

                //Check the sequence is correct

                int thisDeltaX = locationAfterMove.x - player.LocationMap.x;
                int thisDeltaY = locationAfterMove.y - player.LocationMap.y;

                //South
                if (firstDeltaX == 0 && firstDeltaY == 1)
                {
                    if (thisDeltaX != 1 || thisDeltaY != 1)
                    {
                        FailWrongPattern();
                        return;
                    }
                }

                //East
                if (firstDeltaX == 1 && firstDeltaY == 0)
                {
                    if (thisDeltaX != 1 || thisDeltaY != -1)
                    {
                        FailWrongPattern();
                        return;
                    }
                }

                //North
                if (firstDeltaX == 0 && firstDeltaY == -1)
                {
                    if (thisDeltaX != -1 || thisDeltaY != -1)
                    {
                        FailWrongPattern();
                        return;
                    }
                }

                //West
                if (firstDeltaX == -1 && firstDeltaY == 0)
                {
                    if (thisDeltaX != -1 || thisDeltaY != 1)
                    {
                        FailWrongPattern();
                        return;
                    }
                }

                //Save this move
                lastDeltaX = thisDeltaX;
                lastDeltaY = thisDeltaY;

                //Check there the monster is still in its square and hasn't died
                if(!target.Alive) {
                    FailTarget();
                    return;
                }

                if (target.LocationMap != monsterSquare)
                {
                    FailTarget();
                    return;
                }

                //Monster is still alive and in right square
                moveCounter = 2;

                LogFile.Log.LogEntryDebug("OpenSpaceAttack Stage: " + moveCounter, LogDebugLevel.Medium);

                //Will attack it during DoMove, and move into its square
                return;
            }

            //Later moves follow a clockwise box
            if (moveCounter > 1)
            {
                //Check this is a valid location to move into
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, locationAfterMove);

                //Monster
                if (squareContents.monster != null)
                {
                    FailBlocked();
                    return;
                }

                //Bad terrain
                if (!dungeon.MapSquareCanBeEntered(player.LocationLevel, locationAfterMove))
                {
                    FailBlocked();
                    return;
                }

                //Check the sequence is correct

                int thisDeltaX = locationAfterMove.x - player.LocationMap.x;
                int thisDeltaY = locationAfterMove.y - player.LocationMap.y;

                //NE
                if (lastDeltaX == 1 && lastDeltaY == -1)
                {
                    //SE
                    if (thisDeltaX != 1 || thisDeltaY != 1)
                    {
                        FailWrongPattern();
                        return;
                    }
                }

                //SE
                if (lastDeltaX == 1 && lastDeltaY == 1)
                {
                    //SW
                    if (thisDeltaX != -1 || thisDeltaY != 1)
                    {
                        FailWrongPattern();
                        return;
                    }
                }

                //SW
                if (lastDeltaX == -1 && lastDeltaY == 1)
                {
                    //NW
                    if (thisDeltaX != -1 || thisDeltaY != -1)
                    {
                        FailWrongPattern();
                        return;
                    }
                }

                //NW
                if (lastDeltaX == -1 && lastDeltaY == -1)
                {
                    //NE
                    if (thisDeltaX != 1 || thisDeltaY != -1)
                    {
                        FailWrongPattern();
                        return;
                    }
                }

                //Save this move
                lastDeltaX = thisDeltaX;
                lastDeltaY = thisDeltaY;

                //Check there the monster is still in its square and hasn't died
                if (!target.Alive)
                {
                    FailTarget();
                    return;
                }

                if (target.LocationMap != monsterSquare)
                {
                    FailTarget();
                    return;
                }

                //Monster is still alive and in right square
                moveCounter++;

                LogFile.Log.LogEntryDebug("OpenSpaceAttack Stage: " + moveCounter, LogDebugLevel.Medium);

                //Will attack it during DoMove, and move into its square
                return;
            }

            LogFile.Log.LogEntry("OpenSpaceAttack: moveCounter wrong");
            return;
        }

        private void FailWrongPattern()
        {
            LogFile.Log.LogEntryDebug("OpenSpaceAttack failed - wrong pattern", LogDebugLevel.Medium);
            ResetStatus();
        }

        private void ResetStatus()
        {
            moveCounter = 0;

        }

        private void FailBlocked()
        {
            LogFile.Log.LogEntryDebug("OpenSpaceAttack failed - blocked", LogDebugLevel.Medium);
            ResetStatus();
        }

        private void FailTarget()
        {
            LogFile.Log.LogEntryDebug("OpenSpaceAttack failed - target moved or died", LogDebugLevel.Medium);
            ResetStatus();
        }

        private void FailInterrupted()
        {
            LogFile.Log.LogEntryDebug("OpenSpaceAttack failed - interrupted", LogDebugLevel.Medium);
            ResetStatus();
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

            //Bonus to hit and damage
            Game.MessageQueue.AddMessage("OpenSpaceAttack!");
            CombatResults results = Game.Dungeon.Player.AttackMonsterWithModifiers(target, bonus, 0, bonus, 0);
             
            //Move into destination square (already check this was OK)
            Game.Dungeon.MovePCAbsoluteSameLevel(locationAfterMove.x, locationAfterMove.y);

            //ResetStatus();

            LogFile.Log.LogEntry("OpenSpaceAttack free attack: " + bonus);
            

            //Fifth move is end and requires another attack to restart
            if (moveCounter == 5)
            {
                LogFile.Log.LogEntry("OpenSpaceAttack finished ");
                ResetStatus();
            }
        }

        public override void ClearMove()
        {
            moveCounter = 0;
        }

        public override string MovieRoot()
        {
            return "openspaceattack";
        }
    }
}