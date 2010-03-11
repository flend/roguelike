using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Spells
{
    public class MagicMissile : Spell
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

            //Check there is a monster at target
            SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, target);

            //Is there no monster here? If so, then attack it
            if (squareContents.monster != null)
            {
                LogFile.Log.LogEntryDebug("Firing magic missile", LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Magic Missile!");
                
                //Attack the monster

                //Magic missile always hits

                //Damage is based on Magic Stat (and creature's magic resistance)

                //Damage base
                
                int damageBase;

                if (player.MagicStat > 100)
                {
                    damageBase = 8;
                }
                else if (player.MagicStat > 60)
                {
                    damageBase = 6;
                }
                else if (player.MagicStat > 30)
                {
                    damageBase = 5;
                }
                else
                    damageBase = 4;

                //Damage done is just the base

                int damage = Utility.DamageRoll(damageBase);

                string combatResultsMsg = "PvM Magic Missile: Dam: 1d" + damageBase + " -> " + damage;
                LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                //Apply damage
                player.ApplyDamageToMonster(squareContents.monster, damage);
                
                //Graphical effect

                int deltaX = target.x - player.LocationMap.x;
                int deltaY = target.y - player.LocationMap.y;

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

                //Draw a graphical effect
                int startX = player.LocationMap.x + unitX;
                int startY = player.LocationMap.y + unitY;

                int endY = target.y - unitY;
                int endX = target.x - unitX;

                Screen.Instance.DrawFlashLine(new Point(startX, startY), new Point(endX, endY), ColorPresets.Violet);

                return true;
            }

            Game.MessageQueue.AddMessage("No target for magic missile.");
            LogFile.Log.LogEntryDebug("No monster to target for Magic Missile", LogDebugLevel.Medium);
            return false;
        }

        public override int MPCost()
        {
            return 1;
        }

        public override bool NeedsTarget()
        {
            return true;
        }

        public override string SpellName()
        {
            return "Magic Missile";
        }

        public override string Abbreviation()
        {
            return "MM";
        }
    }
}
