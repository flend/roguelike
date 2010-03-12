using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Spells
{
    public class EnergyBlast : Spell
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
                LogFile.Log.LogEntryDebug("Firing energy blast", LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Energy Blast!");
                
                //Attack the monster

                //Magic missile always hits

                //Damage is based on Magic Stat (and creature's magic resistance)

                //Damage base
                
                int damageBase;

                if (player.MagicStat > 140)
                {
                    damageBase = 12;
                }

                if (player.MagicStat > 120)
                {
                    damageBase = 10;
                }

                if (player.MagicStat > 100)
                {
                    damageBase = 8;
                }
                else
                {
                    damageBase = 6;
                }

                //Damage done is just the base

                int damage = Utility.DamageRoll(damageBase) + Utility.DamageRoll(damageBase) + Utility.DamageRoll(damageBase) + damageBase / 2;

                string combatResultsMsg = "PvM Energy Blast: Dam: 3d" + damageBase + " -> " + (damageBase / 2);
                LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                //Apply damage
                player.ApplyDamageToMonster(squareContents.monster, damage);
                

                //Graphical effect
                /*
                TCODLineDrawing.InitLine(player.LocationMap.x, player.LocationMap.y, target.x, target.y);
                int firstXStep = 0;
                int firstYStep = 0;

                TCODLineDrawing.StepLine(ref firstXStep, ref firstYStep);*/

                Screen.Instance.DrawFlashLine(new Point(player.LocationMap.x, player.LocationMap.y), new Point(target.x, target.y), ColorPresets.Crimson);

                return true;
            }

            Game.MessageQueue.AddMessage("No target for energy blast.");
            LogFile.Log.LogEntryDebug("No monster to target for Energy Blast", LogDebugLevel.Medium);
            return false;
        }

        public override int MPCost()
        {
            return 6;
        }

        public override bool NeedsTarget()
        {
            return true;
        }

        public override string SpellName()
        {
            return "Energy Blast";
        }

        public override string Abbreviation()
        {
            return "EB";
        }

        internal override int GetRequiredMagic()
        {
            return 70;
        }

        internal override string MovieRoot()
        {
            return "spellenergyblast";
        }
    }
}
