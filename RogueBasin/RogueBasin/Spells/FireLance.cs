using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Spells
{
    /// <summary>
    /// Penetrating magic attack
    /// </summary>
    public class FireLance : Spell
    {


        int fireRange = 6;

        public override bool DoSpell(Point target)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;


            //Check the target is within FOV

            //Get the FOV from Dungeon (this also updates the map creature FOV state)
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(player);

            //Is the target in FOV
            if (!currentFOV.CheckTileFOV(target.x, target.y))
            {
                LogFile.Log.LogEntryDebug("Target out of FOV", LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Target is out of sight.");

                return false;
            }

            //Keep hitting monsters until we hit a wall

            Point targetSquare = target;

            int deltaX = targetSquare.x - player.LocationMap.x;
            int deltaY = targetSquare.y - player.LocationMap.y;

            if (deltaX == 0 && deltaY == 0)
            {
                LogFile.Log.LogEntryDebug("No target for fireland", LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("No target for Fire Lance.");

                return false;
            }

            /*
            int unitX = 0;
            int unitY = 0;

            if (deltaX < 0 && deltaY < 0)
            {
                unitX = -1;
                unitY = -1;
            }
            else if (deltaX < 0 && deltaY > 0)
            {
                unitX = -1;
                unitY = 1;
            }
            else if (deltaX > 0 && deltaY < 0)
            {
                unitX = 1;
                unitY = -1;
            }
            else if (deltaX > 0 && deltaY > 0)
            {
                unitX = 1;
                unitY = 1;
            }
            else if (deltaX == 0 && deltaY > 0)
            {
                unitX = 0;
                unitY = 1;
            }
            else if (deltaX == 0 && deltaY < 0)
            {
                unitX = 0;
                unitY = -1;
            }
            else if (deltaY == 0 && deltaX < 0)  
            {
                unitX = -1;
                unitY = 0;
            }
            else if (deltaY == 0 && deltaX > 0)
            {
                unitX = 1;
                unitY = 0;
            }*/

            //Extend this line until it hits an edge
            int finalX = player.LocationMap.x;
            int finalY = player.LocationMap.y;

            while(finalX < dungeon.Levels[player.LocationLevel].width &&
                finalY < dungeon.Levels[player.LocationLevel].height && finalX >0 && finalY > 0) {
                finalX += deltaX;
                finalY += deltaY;
            }

            //int currentX = targetSquare.x;
           // int currentY = targetSquare.y;

            int lastX = targetSquare.x;
            int lastY = targetSquare.y;

            bool finishedLine = false;

            //This one is always cast, even without a target

            Game.MessageQueue.AddMessage("Fire Lance!");

            //Cast in 2 stages. The first stage from us to target 1. The next stage from target 1 to the end of the map

            TCODLineDrawing.InitLine(player.LocationMap.x, player.LocationMap.y, finalX, finalY);
            int firstXStep = 0;
            int firstYStep = 0;

            TCODLineDrawing.StepLine(ref firstXStep, ref firstYStep);

            int currentX = firstXStep;
            int currentY = firstYStep;

            //Cast a line between the start and end
            TCODLineDrawing.InitLine(currentX, currentY, targetSquare.x, targetSquare.y);

            do
            {

                LogFile.Log.LogEntryDebug("FireLance: Attacking square: x: " + currentX + " y:" + currentY, LogDebugLevel.Medium);

                //Finish conditions - run out of line or hit wall (should always happen)

                if (!dungeon.MapSquareIsWalkable(player.LocationLevel, new Point(currentX, currentY)))
                {

                    //Finish
                    LogFile.Log.LogEntryDebug("hit wall Finished line 1 at: x: " + currentX + " y:" + currentY, LogDebugLevel.Medium);
                    break;
                }

                if (dungeon.GetDistanceBetween(player.LocationMap, new Point(currentX, currentY)) > fireRange)
                {
                    //Finish - range
                    LogFile.Log.LogEntryDebug("(range) Finished line 1 at: x: " + currentX + " y:" + currentY, LogDebugLevel.Medium);
                    break;
                }

                //Is there a monster here? If so, then attack it

                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(currentX, currentY));

                if (squareContents.monster != null)
                {
                    HitMonster(player, squareContents.monster);
                }

                lastX = currentX; lastY = currentY;

                finishedLine = TCODLineDrawing.StepLine(ref currentX, ref currentY);

                if (finishedLine)
                {
                    LogFile.Log.LogEntryDebug("Finished line 1 at: x: " + currentX + " y:" + currentY, LogDebugLevel.Low);
                }
            } while (finishedLine == false);


            //The second stage from one square away from target 1 to the end

            TCODLineDrawing.InitLine(targetSquare.x, targetSquare.y, finalX, finalY);
            TCODLineDrawing.StepLine(ref currentX, ref currentY);

            //Cast a line between the start and end
            TCODLineDrawing.InitLine(currentX, currentY, finalX, finalY);

            do
            {

                LogFile.Log.LogEntryDebug("FireLance: Attacking square: x: " + currentX + " y:" + currentY, LogDebugLevel.Low);

                //Finish conditions - run out of line or hit wall (should always happen)

                if (!dungeon.MapSquareIsWalkable(player.LocationLevel, new Point(currentX, currentY)))
                {

                    //Finish
                    LogFile.Log.LogEntryDebug("Finished line 2 at: x: " + currentX + " y:" + currentY, LogDebugLevel.Medium);
                    break;
                }

                if (dungeon.GetDistanceBetween(player.LocationMap, new Point(currentX, currentY)) > fireRange)
                {
                    //Finish - range
                    LogFile.Log.LogEntryDebug("(range) Finished line 1 at: x: " + currentX + " y:" + currentY, LogDebugLevel.Medium);
                    break;
                }

                //Is there a monster here? If so, then attack it

                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(currentX, currentY));

                if (squareContents.monster != null)
                {
                    HitMonster(player, squareContents.monster);
                }

                lastX = currentX; lastY = currentY;

                finishedLine = TCODLineDrawing.StepLine(ref currentX, ref currentY);

                if (finishedLine)
                {
                    LogFile.Log.LogEntryDebug("Finished line 2 at: x: " + currentX + " y:" + currentY, LogDebugLevel.Low);
                }
            } while (finishedLine == false);

            //Find the first square away from us
            //Cast the ray from here


            Screen.Instance.DrawFlashLine(new Point(player.LocationMap.x, player.LocationMap.y), new Point(lastX, lastY), ColorPresets.Yellow);

            return true;
            
            }
        

        private void HitMonster(Player player, Monster monster)
        {

            //Attack the monster

            //Check magic resistance
            bool monsterResisted = CheckMagicResistance(monster);
            if (monsterResisted)
                return;

            //Damage is based on Magic Stat (and creature's magic resistance)

            //Damage base

            int damageBase;
            int damageMod;

            if (player.MagicStat > 130)
            {
                damageBase = 12;
                damageMod = 1;
            }

            if (player.MagicStat > 100)
            {
                damageBase = 10;
                damageMod = 1;
            }
            else if (player.MagicStat > 60)
            {
                damageBase = 8;
                damageMod = 1;
            }
            else if (player.MagicStat > 30)
            {
                damageBase = 6;
                damageMod = 1;
            }
            else
            {
                damageBase = 4;
                damageMod = 1;
            }

            //Damage done is just the base

            int damage = Utility.DamageRoll(damageBase) + damageMod;

            string combatResultsMsg = "PvM Fire Lance: Dam: 1d" + damageBase + " mod " + damageMod + " -> " + damage;
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            //Apply damage
            player.ApplyDamageToMonster(monster, damage, true, false);
        }

        public override int GetRange()
        {
            return fireRange;
        }

        override public int MPCost()
        {
            return 3;
        }

        public override bool NeedsTarget()
        {
            return true;
        }

        public override string SpellName()
        {
            return "Fire Lance";
        }

        public override string Abbreviation()
        {
            return "FL";
        }

        internal override int GetRequiredMagic()
        {
            return 50;
        }

        internal override string MovieRoot()
        {
            return "spellfirelance";
        }
    }
}
