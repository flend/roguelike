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
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(player);

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

                Monster monster = squareContents.monster;

                //Check magic resistance
                bool monsterResisted = CheckMagicResistance(squareContents.monster);

                //Draw attack
                CombatResults results = CombatResults.DefenderDamaged;
                if (monsterResisted)
                {
                    results = CombatResults.DefenderUnhurt;
                }

                Screen.Instance.DrawMissileAttack(player, monster, results, System.Drawing.Color.Violet);

                //If monster resisted no damage                
                if (monsterResisted)
                    return true;

                //Damage base
                
                int damageBase;

                if (player.MagicStat > 120)
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
                player.ApplyDamageToMonsterFromPlayer(squareContents.monster, damage, true, false);
                

                //Graphical effect
                /*
                TCODLineDrawing.InitLine(player.LocationMap.x, player.LocationMap.y, target.x, target.y);
                int firstXStep = 0;
                int firstYStep = 0;

                TCODLineDrawing.StepLine(ref firstXStep, ref firstYStep);*/

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

        public override int GetRange()
        {
            return 5;
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

        internal override int GetRequiredMagic()
        {
            return 20;
        }

        internal override string MovieRoot()
        {
            return "spellmagicmissile";
        }
    }
}
