using System;
using System.Collections.Generic;
using System.Linq;

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
            CreatureFOV currentFOV = dungeon.CalculateCreatureFOV(dungeon.Player);

            if (!Utility.TestRangeFOVForWeapon(dungeon.Player, target.MapCoord, range, currentFOV))
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

            CreatureFOV currentFOV = dungeon.CalculateCreatureFOV(dungeon.Player);

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

            if (!Utility.TestRangeFOVForWeapon(dungeon.Player, target.MapCoord, range, currentFOV))
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

                    dungeon.KillMonster(monster, false);

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

        /// <summary>
        /// Generic throw method for most grenade items
        /// </summary>
        public Point ThrowItemGrenadeLike(IEquippableItem item, int level, Point target, int damage, bool stunning = false)
        {
            Item itemAsItem = item as Item;

            LogFile.Log.LogEntryDebug("Throwing " + itemAsItem.SingleItemDescription, LogDebugLevel.Medium);

            Player player = dungeon.Player;

            //Find target

            List<Point> targetSquares = dungeon.WeaponUtility.CalculateTrajectorySameLevel(player, target);
            Monster monster = dungeon.WeaponUtility.FirstMonsterInTrajectory(player.LocationLevel, targetSquares);

            //Find where it landed

            //Destination will be the last square in trajectory
            Point destination;
            if (targetSquares.Count > 0)
                destination = targetSquares[targetSquares.Count - 1];
            else
                //Threw it on themselves!
                destination = player.LocationMap;


            //Stopped by a monster
            if (monster != null)
            {
                destination = monster.LocationMap;
            }

            //Make throwing sound AT target location
            dungeon.AddSoundEffect(item.ThrowSoundMagnitude(), dungeon.Player.LocationLevel, destination);

            //if (Player.LocationLevel >= 6)
            //    damage *= 2;

            //Work out grenade splash and damage
            if (stunning)
            {
                DoGrenadeExplosionStun(item, level, target, damage, dungeon.Player);
            }
            else
            {
                DoGrenadeExplosion(item, level, target, damage, dungeon.Player);
            }

            return destination;
        }

        public void DoGrenadeExplosion(int level, Point locationMap, double size, int damage, Creature originMonster, int animationDelay = 0)
        {
            Player player = dungeon.Player;

            List<Point> grenadeAffects = dungeon.GetPointsForGrenadeTemplate(locationMap, level, size);

            //Use FOV from point of explosion (this means grenades don't go round corners or through walls)
            WrappedFOV grenadeFOV = dungeon.CalculateAbstractFOV(player.LocationLevel, locationMap, 0);

            var grenadeAffectsFiltered = grenadeAffects.Where(sq => grenadeFOV.CheckTileFOV(player.LocationLevel, sq));

            DoGrenadeExplosion(level, damage, originMonster, animationDelay, grenadeAffectsFiltered);
        }

        public void DoGrenadeExplosion(IEquippableItem item, int level, Point locationMap, int damage, Creature originMonster, int animationDelay = 0)
        {
            //Work out grenade splash and damage
            var grenadeAffectsFiltered = item.TargettingInfo().TargetPoints(dungeon.Player, dungeon, new Location(level, locationMap));
            DoGrenadeExplosion(level, damage, originMonster, animationDelay, grenadeAffectsFiltered);
        }

        private void DoGrenadeExplosion(int level, int damage, Creature originMonster, int animationDelay, IEnumerable<Point> grenadeAffectsFiltered)
        {
            Player player = dungeon.Player;

            //Draw attack
            Screen.Instance.DrawAreaAttackAnimation(grenadeAffectsFiltered, Screen.AttackType.Explosion, false, animationDelay);

            foreach (Point sq in grenadeAffectsFiltered)
            {
                SquareContents squareContents = dungeon.MapSquareContents(level, sq);

                Monster m = squareContents.monster;

                //Hit the monster if it's there
                if (m != null && m.Alive)
                {
                    string combatResultsMsg = "PvM (" + m.Representation + ") Grenade: Dam: " + damage;
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    //Apply damage
                    if (originMonster != null && originMonster != dungeon.Player)
                    {
                        dungeon.Combat.ApplyDamageToMonster(originMonster, m, damage);
                    }
                    else
                    {
                        dungeon.Combat.PlayerAttackMonsterRanged(squareContents.monster, damage);
                    }
                }

                dungeon.AddDecorationFeature(new Features.Scorch(), level, sq);
            }

            //And the player

            if (grenadeAffectsFiltered.Where(p => p.x == dungeon.Player.LocationMap.x && p.y == dungeon.Player.LocationMap.y).Any())
            {
                if (originMonster != null)
                {
                    string combatResultsMsg = "MvP (" + originMonster.Representation + ") Grenade: Dam: " + damage;
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);
                }
                //Apply damage (uses damage base)
                if (originMonster != null && originMonster != dungeon.Player)
                {
                    player.ApplyCombatDamageToPlayer((originMonster as Monster), damage, true);
                    player.NotifyMonsterEvent(new MonsterEvent(MonsterEvent.MonsterEventType.MonsterAttacksPlayer, originMonster as Monster));
                }
                else
                    player.ApplyCombatDamageToPlayer(damage);
            }
        }

        public void DoGrenadeExplosionStun(IEquippableItem item, int level, Point locationMap, int stunDamage, Creature originMonster)
        {
            //Work out grenade splash and damage
            var grenadeAffectsFiltered = item.TargettingInfo().TargetPoints(dungeon.Player, dungeon, new Location(level, locationMap));

            //Draw attack
            Screen.Instance.DrawAreaAttackAnimation(grenadeAffectsFiltered, Screen.AttackType.Stun);

            foreach (Point sq in grenadeAffectsFiltered)
            {
                SquareContents squareContents = dungeon.MapSquareContents(level, sq);

                Monster m = squareContents.monster;

                //Hit the monster if it's there
                if (m != null && m.Alive)
                {
                    string combatResultsMsg = "PvM (" + m.Representation + ") Stun Grenade: Dam: " + stunDamage;
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    //Apply damage
                    if (originMonster != null)
                    {
                        dungeon.Combat.ApplyStunDamageToMonster(m, originMonster, stunDamage);
                    }
                }
            }

            //And the player

            if (grenadeAffectsFiltered.Where(p => p.x == dungeon.Player.LocationMap.x && p.y == dungeon.Player.LocationMap.y).Any())
            {
                if (originMonster != null)
                {
                    string combatResultsMsg = "MvP (" + originMonster.Representation + ") Stun Grenade: Dam: " + stunDamage;
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);
                }

                //No stun damage for players right now
            }
        }


        public bool FireShotgunWeapon(Point target, Items.ShotgunTypeWeapon item, int damageBase, int damageDropWithRange, int damageDropWithInterveningMonster)
        {
            IEquippableItem equipItem = item as IEquippableItem;
            return FireShotgunWeapon(dungeon.Player, target, damageBase, item.FireSoundMagnitude(), item.ShotgunSpreadAngle(), damageDropWithRange, damageDropWithInterveningMonster);
        }

        public bool FireShotgunWeapon(Creature gunner, Point target, int damageBase, double fireSoundMagnitude, double spreadAngle, int damageDropWithRange, int damageDropWithInterveningMonster)
        {
            Player player = dungeon.Player;

            //The shotgun fires towards its target and does less damage with range

            LogFile.Log.LogEntryDebug("---SHOTGUN FIRE---", LogDebugLevel.Medium);

            //Get all squares in range and within FOV (shotgun needs a straight line route to fire)

            CreatureFOV currentFOV = dungeon.CalculateNoRangeCreatureFOV(gunner);
            List<Point> targetSquares = currentFOV.GetPointsForTriangularTargetInFOV(gunner.LocationMap, target, dungeon.Levels[gunner.LocationLevel], 10, spreadAngle);

            //Draw attack
            Screen.Instance.DrawAreaAttackAnimation(targetSquares, Screen.AttackType.Bullet);

            //Make firing sound
            if (gunner is Player)
                dungeon.AddSoundEffect(fireSoundMagnitude, player.LocationLevel, player.LocationMap);

            //Attack all monsters in the area

            foreach (Point sq in targetSquares)
            {
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, sq);

                Monster m = squareContents.monster;

                //Hit the monster if it's there
                if (m != null)
                {
                    int damage = ShotgunDamage(gunner, m, damageBase, damageDropWithRange, damageDropWithInterveningMonster);

                    string combatMessage = "MvM";
                    if (gunner is Player)
                        combatMessage = "PvM";

                    string combatResultsMsg = combatMessage + " (" + m.Representation + ") Shotgun: Dam: " + damage;
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    //Apply damage to monsters
                    if (damage > 0)
                    {
                        if (gunner is Player)
                            //Cancels some player statuses
                            dungeon.Combat.PlayerAttackMonsterRanged(squareContents.monster, damage);
                        else
                            dungeon.Combat.ApplyDamageToMonster(gunner, m, damage);
                    }
                }

                if (squareContents.player != null && !(gunner is Player))
                {

                    int damage = ShotgunDamage(gunner, player, damageBase, damageDropWithRange, damageDropWithInterveningMonster);

                    string combatResultsMsg = "MvP (" + gunner.Representation + ") Shotgun: Dam: " + damage;
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    //Apply damage to player
                    if (damage > 0)
                    {
                        player.ApplyCombatDamageToPlayer(gunner as Monster, damage, true);
                        player.NotifyMonsterEvent(new MonsterEvent(MonsterEvent.MonsterEventType.MonsterAttacksPlayer, gunner as Monster));
                    }
                }
            }

            return true;
        }

        private int ShotgunDamage(Creature gunner, Creature target, int damageBase, int damageDropWithRange, int damageDropWithInterveningMonster)
        {
            //Calculate range
            int rangeToMonster = (int)Math.Floor(Utility.GetDistanceBetween(gunner.LocationMap, target.LocationMap));

            //How many monsters between monster and gunner
            var pointsFromPlayerToMonster = Utility.GetPointsOnLine(gunner.LocationMap, target.LocationMap);
            //(exclude player and monster itself)
            var numInterveningMonsters = Math.Max(pointsFromPlayerToMonster.Skip(1).Where(p => dungeon.MapSquareContents(gunner.LocationLevel, p).monster != null).Count() - 1, 0);

            LogFile.Log.LogEntryDebug("Shotgun. Gunner: " + gunner.Representation + " Target at " + target.LocationMap + " intervening monsters: " + numInterveningMonsters, LogDebugLevel.Medium);

            int damage = damageBase - rangeToMonster * damageDropWithRange;
            damage = Math.Max(damage - numInterveningMonsters * damageDropWithInterveningMonster, 0);
            return damage;
        }


        public bool FireLaserLineWeapon(Point target, RangedWeapon item, int damage)
        {
            Player player = dungeon.Player;

            Point lineEnd = dungeon.GetEndOfLine(player.LocationMap, target, player.LocationLevel);

            WrappedFOV fovForWeapon = dungeon.CalculateAbstractFOV(dungeon.Player.LocationLevel, dungeon.Player.LocationMap, 80);
            List<Point> targetSquares = dungeon.GetPathLinePointsInFOV(dungeon.Player.LocationLevel, dungeon.Player.LocationMap, lineEnd, fovForWeapon);

            //Draw attack
            var targetSquaresToDraw = targetSquares.Count() > 1 ? targetSquares.GetRange(1, targetSquares.Count - 1) : targetSquares;
            Screen.Instance.DrawAreaAttackAnimation(targetSquaresToDraw, Screen.AttackType.Laser);

            //Make firing sound
            dungeon.AddSoundEffect(item.FireSoundMagnitude(), player.LocationLevel, player.LocationMap);

            //Attack all monsters in the area

            foreach (Point sq in targetSquares)
            {
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, sq);

                Monster m = squareContents.monster;

                //Hit the monster if it's there
                if (m != null)
                {
                    string combatResultsMsg = "PvM (" + m.Representation + ") Laser: Dam: " + damage;
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    //Apply damage
                    dungeon.Combat.PlayerAttackMonsterRanged(squareContents.monster, damage);
                }
            }

            //Remove 1 ammo
            item.Ammo--;

            return true;
        }

        public bool FirePistolLineWeapon(Point target, RangedWeapon item, int damageBase)
        {
            Player player = dungeon.Player;

            //Make firing sound
            dungeon.AddSoundEffect(item.FireSoundMagnitude(), player.LocationLevel, player.LocationMap);

            //Find monster target

            var targetSquares = dungeon.WeaponUtility.CalculateTrajectorySameLevel(player, target);
            Monster monster = dungeon.WeaponUtility.FirstMonsterInTrajectory(player.LocationLevel, targetSquares);
        
            if (monster == null)
            {
                LogFile.Log.LogEntryDebug("No monster in target for pistol-like weapon. Ammo used anyway.", LogDebugLevel.Medium);
                return true;
            }

            var targetSquaresToDraw = targetSquares.Count() > 1 ? targetSquares.GetRange(1, targetSquares.Count - 1) : targetSquares;

            //Draw attack

            Screen.Instance.DrawAreaAttackAnimation(targetSquaresToDraw, Screen.AttackType.Bullet, true);

            //Apply damage
            dungeon.Combat.PlayerAttackMonsterRanged(monster, damageBase);

            return true;
        }

    }
}
