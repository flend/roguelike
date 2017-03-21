using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public class Combat
    {
        private Dungeon dungeon;

        public Combat(Dungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        public enum FireOnTargetStatus
        {
            NoWeapon, CantTargetSelf, CantFireBetweenLevels, NotEnoughAmmo, OutOfRange, OK
        }

        public FireOnTargetStatus CanFireOnTargetWithEquippedWeapon(Location target)
        {
            Player player = dungeon.Player;

            IEquippableItem weapon = player.GetEquippedRangedWeapon();
            Item weaponI = player.GetEquippedRangedWeaponAsItem();

            if (weapon == null)
            {
                return FireOnTargetStatus.NoWeapon;
            }

            if (target.Level != player.Location.Level)
            {
                return FireOnTargetStatus.CantFireBetweenLevels;
            }

            if (target.MapCoord == player.Location.MapCoord)
            {
                return FireOnTargetStatus.CantTargetSelf;
            }

            if (weapon.RemainingAmmo() < 1)
            {
                return FireOnTargetStatus.NotEnoughAmmo;
            }

            //Check we are in range of target (not done above)
            int range = weapon.RangeFire();
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            if (!Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, target.MapCoord, range, currentFOV))
            {
                return FireOnTargetStatus.OutOfRange;
            }

            return FireOnTargetStatus.OK;
        }

        public enum ThrowToTargetStatus
        {
            NoUtility, CantThrowBetweenLevels, OutOfRange, OK
        }

        public ThrowToTargetStatus CanThrowToTargetWithEquippedUtility(Location target)
        {
            Player player = dungeon.Player;

            IEquippableItem toThrow = player.GetEquippedUtility();
            Item toThrowItem = player.GetEquippedUtilityAsItem();

            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            if (toThrow == null)
            {
                return ThrowToTargetStatus.NoUtility;
            }

            if (target.Level != player.Location.Level)
            {
                return ThrowToTargetStatus.CantThrowBetweenLevels;
            }

            //Check we are in range of target (not done above)

            int range = toThrow.RangeThrow();

            if (!Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, target.MapCoord, range, currentFOV))
            {
                return ThrowToTargetStatus.OutOfRange;
            }

            return ThrowToTargetStatus.OK;
        }


        /// <summary>
        /// From PCMove, where we do our melee attack on the monster in the square entered
        /// </summary>
        /// <param name="monster"></param>
        public void DoMeleeAttackOnMonster(Point moveDelta, Point newPCLocation)
        {
            //Attack at pc's direct move location
            Player player = dungeon.Player;

            var monstersToAttack = new List<Monster>();
            SquareContents contents = dungeon.MapSquareContents(player.LocationLevel, newPCLocation);

            if (contents.monster != null)
            {
                if (!contents.monster.Charmed)
                {
                    monstersToAttack.Add(contents.monster);
                }
            }

            //Should be rewritten with item properties

            if (player.GetEquippedMeleeWeapon() is Items.Axe)
            {
                //Also attack the neighbours
                var neighbours = dungeon.GetNeighbourPointsToDelta(moveDelta).Select(p => newPCLocation - moveDelta + p);

                foreach (var point in neighbours)
                {
                    contents = dungeon.MapSquareContents(player.LocationLevel, point);

                    if (contents.monster != null)
                    {
                        if (!contents.monster.Charmed)
                        {
                            monstersToAttack.Add(contents.monster);
                        }
                    }
                }
            }

            if (player.GetEquippedMeleeWeapon() is Items.Pole)
            {
                //Also attack the points behind
                var neighbours = new List<Point> { new Point(newPCLocation + moveDelta), new Point(newPCLocation + moveDelta + moveDelta) };

                foreach (var point in neighbours)
                {
                    contents = dungeon.MapSquareContents(player.LocationLevel, point);

                    if (contents.monster != null)
                    {
                        if (!contents.monster.Charmed)
                        {
                            monstersToAttack.Add(contents.monster);
                        }
                    }
                }
            }

            foreach (Monster m in monstersToAttack)
            {
                CombatResults results = PlayerAttackMonsterMelee(m);
                Screen.Instance.DrawMeleeAttack(player, m, results);
                Screen.Instance.CreatureToView = m;
            }
        }

        /// <summary>
        /// Used by old special moves code, although combat system no longer observes modifiers
        /// </summary>
        public CombatResults PlayerMeleeAttackMonsterWithModifiers(Monster monster, int hitModifierMod, int damageBaseMod, int damageModifierMod, int enemyACMod, bool specialMoveUsed)
        {
            Player player = dungeon.Player;

            //Do we need to recalculate combat stats?
            if (player.RecalculateCombatStatsRequired)
                player.CalculateCombatStats();

            if (monster.RecalculateCombatStatsRequired)
                monster.CalculateCombatStats();

            //Attacking a monster with hand to hand give an instrinsic
            player.CombatUse = true;

            //Calculate damage from a normal attack

            int damage = PlayerMeleeAttackMonsterWithModifiers(monster, hitModifierMod, damageBaseMod, damageModifierMod, enemyACMod);

            return ApplyDamageToMonsterFromPlayer(monster, damage, false, specialMoveUsed);
        }

        private int PlayerMeleeAttackMonsterWithModifiers(Monster monster, int hitMod, int damBase, int damMod, int ACmod)
        {
            Player player = dungeon.Player;

            //Flatline has a rather simple combat system
            IEquippableItem item = player.GetEquippedMeleeWeapon();

            int baseDamage = 2;

            if (item != null && item.HasMeleeAction())
            {
                baseDamage = item.MeleeDamage();
            }

            string combatResultsMsg = "PvM " + monster.Representation + " = " + baseDamage;

            return baseDamage;
        }

        public CombatResults PlayerAttackMonsterRanged(Monster monster, int damage)
        {
            Player player = dungeon.Player;

            var modifiedDamage = (int)Math.Ceiling(player.CalculateRangedAttackModifiersOnMonster(monster) * damage);
            string combatResultsMsg = "PvM (ranged) " + monster.Representation + "base " + damage + " modified " + modifiedDamage;
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            player.CancelStealthDueToAttack();
            player.CancelBoostDueToAttack();

            return ApplyDamageToMonsterFromPlayer(monster, modifiedDamage, false, false);
        }

        public CombatResults PlayerAttackMonsterThrown(Monster monster, int damage)
        {
            Player player = dungeon.Player;

            string combatResultsMsg = "PvM (thrown) " + monster.Representation + " = " + damage;
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            player.CancelStealthDueToAttack();
            player.CancelBoostDueToAttack();

            return ApplyDamageToMonsterFromPlayer(monster, damage, false, false);
        }

        private CombatResults PlayerAttackMonsterMelee(Monster monster)
        {
            Player player = dungeon.Player;

            IEquippableItem item = player.GetEquippedMeleeWeapon();

            int baseDamage = 2;

            if (item != null && item.HasMeleeAction())
            {
                baseDamage = item.MeleeDamage();
            }

            var modifiedDamage = player.ScaleMeleeDamage(player.GetEquippedMeleeWeapon() as Item, baseDamage);
            //(int)Math.Ceiling(CalculateMeleeAttackModifiersOnMonster(monster) * baseDamage);

            string combatResultsMsg = "PvM (melee) " + monster.Representation + " base " + baseDamage + " mod " + modifiedDamage;
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            player.CancelStealthDueToAttack();
            //ResetTurnsMoving();

            //Play sound
            SoundPlayer.Instance().EnqueueSound("punch");

            return ApplyDamageToMonsterFromPlayer(monster, modifiedDamage, false, false);
        }

        public CombatResults ApplyDamageToMonsterFromPlayer(Monster monster, int damage, bool magicUse, bool specialMove)
        {
            return ApplyDamageToMonster(dungeon.Player, monster, damage);
        }


        /// <summary>
        /// Apply stun damage (miss n-turns) to monster. All stun attacks are routed through here
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="stunTurns"></param>
        /// <returns></returns>
        public CombatResults ApplyStunDamageToMonster(Monster monster, Creature attackingCreature, int stunTurns)
        {
            //Wake monster up etc.
            monster.AIForMonsterIsAttacked(monster, attackingCreature);

            int monsterOrigStunTurns = monster.StunnedTurns;

            //Do we hit the monster?
            if (stunTurns > 0)
            {
                monster.StunnedTurns += stunTurns;

                //Notify the creature that it has taken damage
                //It may activate a special ability or stop running away etc.
                monster.NotifyHitByCreature(monster, 0);

                //Message string
                string playerMsg2 = "";
                if (!monster.Unique)
                    playerMsg2 += "The ";
                playerMsg2 += monster.SingleDescription + " is stunned!";
                Game.MessageQueue.AddMessage(playerMsg2);

                string debugMsg2 = "MStun: " + monsterOrigStunTurns + "->" + monster.StunnedTurns;
                LogFile.Log.LogEntryDebug(debugMsg2, LogDebugLevel.Medium);

                return CombatResults.NeitherDied;
            }

            //Miss

            string playerMsg3 = "";
            if (!monster.Unique)
                playerMsg3 += "The ";
            playerMsg3 += monster.SingleDescription + " shrugs off the attack.";
            Game.MessageQueue.AddMessage(playerMsg3);
            string debugMsg3 = "MStun: " + monsterOrigStunTurns + "->" + monster.StunnedTurns;
            LogFile.Log.LogEntryDebug(debugMsg3, LogDebugLevel.Medium);

            return CombatResults.NeitherDied;

        }

        /// <summary>
        /// Apply damage to monster and deal with death. All damaging attacks are routed through here.
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        public CombatResults ApplyDamageToMonster(Creature attackingCreature, Monster monster, int damage)
        {
            //Wake monster up etc.
            monster.AIForMonsterIsAttacked(monster, attackingCreature);

            //Do we hit the monster?
            if (damage > 0)
            {
                int monsterOrigHP = monster.Hitpoints;

                monster.Hitpoints -= damage;

                bool monsterDead = monster.Hitpoints <= 0;

                //Notify the creature that it has taken damage
                //It may activate a special ability or stop running away etc.
                if (attackingCreature != null)
                {
                    monster.NotifyHitByCreature(attackingCreature, damage);
                }

                //Is the monster dead, if so kill it?
                if (monsterDead)
                {
                    //Add it to our list of kills (simply adding the whole object here)
                    dungeon.Player.AddKill(monster);

                    //Message string
                    if (attackingCreature != null)
                    {
                        string attackerStr = AttackerString(attackingCreature);
                        string playerMsg = attackerStr + " destroyed the ";
                        playerMsg += monster.SingleDescription + ".";
                        Game.MessageQueue.AddMessage(playerMsg);

                    }
                    else
                    {
                        Game.MessageQueue.AddMessage("The " + monster.SingleDescription + " dies.");
                    }

                    if (attackingCreature != null)
                    {
                        string mvm = AttackerMvMString(attackingCreature);
                        string debugMsg4 = mvm + " " + attackingCreature == null ? "" : attackingCreature.Representation + " attacks " + monster.Representation;
                        LogFile.Log.LogEntryDebug(debugMsg4, LogDebugLevel.Medium);
                    }
                    string debugMsg = "MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " killed";
                    LogFile.Log.LogEntryDebug(debugMsg, LogDebugLevel.Medium);

                    Game.Dungeon.KillMonster(monster, false);

                    return CombatResults.DefenderDied;
                }

                //Message string
                string playerMsg2 = AttackerString(attackingCreature);
                playerMsg2 += " hit the ";
                playerMsg2 += monster.SingleDescription + ".";
                Game.MessageQueue.AddMessage(playerMsg2);
                if (attackingCreature != null)
                {
                    string mvm2 = AttackerMvMString(attackingCreature);
                    string debugMsg6 = mvm2 + " " + attackingCreature == null ? "" : attackingCreature.Representation + " attacks " + monster.Representation;
                    LogFile.Log.LogEntryDebug(debugMsg6, LogDebugLevel.Medium);
                }
                string debugMsg2 = "MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " injured";
                LogFile.Log.LogEntryDebug(debugMsg2, LogDebugLevel.Medium);

                return CombatResults.DefenderDamaged;
            }

            //Miss

            string playerMsg3 = AttackerString(attackingCreature) + " missed the " + monster.SingleDescription + ".";
            Game.MessageQueue.AddMessage(playerMsg3);
            string debugMsg3 = "MHP: " + monster.Hitpoints + "->" + monster.Hitpoints + " missed";
            LogFile.Log.LogEntryDebug(debugMsg3, LogDebugLevel.Medium);

            return CombatResults.NeitherDied;
        }


        public string AttackerString(Creature attackingCreature)
        {
            String attackerStr = "";
            if (attackingCreature is Monster)
            {
                attackerStr = (attackingCreature as Monster).SingleDescription;
            }
            if (attackingCreature is Player)
            {
                attackerStr = "You";
            }
            return attackerStr;
        }

        private string AttackerMvMString(Creature attackingCreature)
        {
            String attackerStr = "MvM";
            if (attackingCreature == null)
            {
                return "TvM";
            }
            if (attackingCreature is Player)
            {
                attackerStr = "MvP";
            }

            return attackerStr;
        }

    }
}
