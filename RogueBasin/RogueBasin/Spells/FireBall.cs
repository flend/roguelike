using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Spells
{
    /// <summary>
    /// Penetrating magic attack
    /// </summary>
    public class FireBall : Spell
    {
        int spellRange = 2;

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

            //Hit monsters around the target

            Point targetSquare = target;

            List<Point> splashSquares = new List<Point>();

            for (int i = targetSquare.x - spellRange; i < targetSquare.x + spellRange; i++)
            {
                for (int j = targetSquare.y - spellRange; j < targetSquare.y + spellRange; j++)
                {
                    if (Math.Pow(i - targetSquare.x, 2) + Math.Pow(j - targetSquare.y, 2) < Math.Pow(spellRange, 2))
                    {
                        splashSquares.Add(new Point(i, j));
                        LogFile.Log.LogEntryDebug("FireBall Added square x: " + i + " y: " + j, LogDebugLevel.Low);
                    }
                }
            }


            //This one is always cast, even without a target

            Game.MessageQueue.AddMessage("Fire Ball!");


            foreach (Point p in splashSquares)
            {
                //Is there a monster here? If so, then attack it

                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, new Point(p.x, p.y));

                if (squareContents.monster != null)
                {
                    HitMonster(player, squareContents.monster);
                }

                if (squareContents.player != null)
                {
                    HitPlayer();
                }
            }

            Screen.Instance.DrawFlashSquares(splashSquares, ColorPresets.Red);
            
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

            if (player.MagicStat > 100)
            {
                damageBase = 12;
                damageMod = 2;
            }
            else if (player.MagicStat > 60)
            {
                damageBase = 8;
                damageMod = 2;
            }
            else if (player.MagicStat > 30)
            {
                damageBase = 8;
                damageMod = 1;
            }
            else
            {
                damageBase = 6;
                damageMod = 1;
            }

            //Damage done is just the base

            int damage = Utility.DamageRoll(damageBase) + damageMod;

            string combatResultsMsg = "PvM Fire Ball: Dam: 1d" + damageBase + " mod " + damageMod + " -> " + damage;
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            //Apply damage
            player.ApplyDamageToMonster(monster, damage, true, false);
        }

        private void HitPlayer()
        {
            Player player = Game.Dungeon.Player;
            //Attack the monster

            //Magic missile always hits

            //Damage is based on Magic Stat (and creature's magic resistance)

            //Damage base

            int damageBase;
            int damageMod;

            if (player.MagicStat > 100)
            {
                damageBase = 12;
                damageMod = 2;
            }
            else if (player.MagicStat > 60)
            {
                damageBase = 8;
                damageMod = 2;
            }
            else if (player.MagicStat > 30)
            {
                damageBase = 8;
                damageMod = 1;
            }
            else
            {
                damageBase = 6;
                damageMod = 1;
            }

            //Damage done is just the base

            int damage = Utility.DamageRoll(damageBase) + damageMod;

            string combatResultsMsg = "PvP Fire Ball: Dam: 1d" + damageBase + " mod " + damageMod + " -> " + damage;
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            //Apply damage
            player.ApplyDamageToPlayer(damage);
        }
        override public int MPCost()
        {
            return 5;
        }

        public override int GetRange()
        {
            return 6;
        }

        public override bool NeedsTarget()
        {
            return true;
        }

        public override string SpellName()
        {
            return "Fire Ball";
        }

        public override string Abbreviation()
        {
            return "FB";
        }

        internal override int GetRequiredMagic()
        {
            return 40;
        }

        internal override string MovieRoot()
        {
            return "spellfireball";
        }
    }
}
