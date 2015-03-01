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
                LogFile.Log.LogEntryDebug("Firing energy blast", LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Energy Blast!");
                
                //Attack the monster

                //Check magic resistance
                bool monsterResisted = CheckMagicResistance(squareContents.monster);
                
                //Draw attack
                CombatResults results = CombatResults.DefenderDamaged;
                if (monsterResisted)
                {
                    results = CombatResults.DefenderUnhurt;
                }

                Screen.Instance.DrawMissileAttack(player, squareContents.monster, results, System.Drawing.Color.Crimson);

                //If monster resisted no damage                
                if (monsterResisted)
                    return true;

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

                int damage = Utility.DamageRoll(damageBase) + Utility.DamageRoll(damageBase) + Utility.DamageRoll(damageBase);// +damageBase / 2;

                string combatResultsMsg = "PvM Energy Blast: Dam: 3d" + damageBase + " -> " + (damage);
                LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                //Apply damage
                player.ApplyDamageToMonster(squareContents.monster, damage, true, false);
                
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

        public override int GetRange()
        {
            return 15;
        }

        public override bool NeedsTarget()
        {
            return true;
        }

        public override TargettingType TargetType()
        {
            return TargettingType.Shotgun;
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
            return 100;
        }

        internal override string MovieRoot()
        {
            return "spellenergyblast";
        }
    }
}
