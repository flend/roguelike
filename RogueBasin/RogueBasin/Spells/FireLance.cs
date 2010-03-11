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
        public override bool DoSpell(Point target)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //Check the target is within FOV

            //Get the FOV from Dungeon (this also updates the map creature FOV state)
            TCODFov currentFOV = Game.Dungeon.CalculateCreatureFOV(player);

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
            }

            //Extend this line until it hits an edge
            int finalX = player.LocationMap.x;
            int finalY = player.LocationMap.y;

            while(finalX < dungeon.Levels[player.LocationLevel].width &&
                finalY < dungeon.Levels[player.LocationLevel].height && finalX >0 && finalY > 0) {
                finalX += deltaX;
                finalY += deltaY;
            }

            //Cast a line between the start and end
            TCODLineDrawing.InitLine(targetSquare.x, targetSquare.y, finalX, finalY);
            
            int currentX = targetSquare.x;
            int currentY = targetSquare.y;

            bool finishedLine = false;

            //This one is always cast, even without a target

            Game.MessageQueue.AddMessage("Fire Lance!");

            do {

                LogFile.Log.LogEntryDebug("FireLance: Attacking square: x: " + currentX + " y:" + currentY, LogDebugLevel.Low);

                //Finish conditions - run out of line or hit wall (should always happen)

                if(!dungeon.MapSquareIsWalkable(player.LocationLevel, new Point(currentX, currentY))) {

                    //Finish
                    LogFile.Log.LogEntryDebug("Finished line at: x: " + currentX + " y:" + currentY, LogDebugLevel.Low);
                    break;
                }

                //Is there a monster here? If so, then attack it
                
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(currentX, currentY));

                if (squareContents.monster != null)
                {
                    HitMonster(player, squareContents.monster);
                }

                finishedLine = TCODLineDrawing.StepLine(ref currentX, ref currentY);

                if(finishedLine) {
                    LogFile.Log.LogEntryDebug("Finished line at: x: " + currentX + " y:" + currentY, LogDebugLevel.Low);
                }
            } while(finishedLine == false);

            //Draw a graphical effect
            int startX = player.LocationMap.x + unitX;
            int startY = player.LocationMap.y + unitY;

            int endY = currentY - unitY;
            int endX = currentX - unitX;

            Screen.Instance.DrawFlashLine(new Point(startX, startY), new Point(endX, endY), ColorPresets.Yellow);

            return true;
            
        }

        private void HitMonster(Player player, Monster monster)
        {

            //Attack the monster

            //Magic missile always hits

            //Damage is based on Magic Stat (and creature's magic resistance)

            //Damage base

            int damageBase;
            int damageMod;

            if (player.MagicStat > 100)
            {
                damageBase = 10;
                damageMod = 2;
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
            player.ApplyDamageToMonster(monster, damage);
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
    }
}
