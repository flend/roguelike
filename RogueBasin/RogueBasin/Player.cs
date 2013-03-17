using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    public class Player : Creature
    {
        /// <summary>
        /// Effects that are active on the player
        /// </summary>
        public List<PlayerEffect> effects { get; set; }

        public List<Monster> Kills { get; set;}

        public int KillCount = 0;

        public string Name { get; set; }

        public bool ItemHelpMovieSeen = false;
        public bool TempItemHelpMovieSeen = false;

        /// <summary>
        /// How many worldClock ticks do we have to rescue our friend?
        /// </summary>
        public long TimeToRescueFriend { get; set; }


        public int TotalPlotItems { get; set; }
        public int PlotItemsFound { get; set; }

        /// <summary>
        /// Play movies and give plot exerpts for items
        /// </summary>
        public bool PlayItemMovies { get; set; }

        /// <summary>
        /// Current hitpoints
        /// </summary>
        int hitpoints;

        /// <summary>
        /// Maximum hitpoints
        /// </summary>
        int maxHitpoints;

        /// <summary>
        /// Player level
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Which princess RL dungeon are we in?
        /// </summary>
        public int CurrentDungeon { get; set; }

        /// <summary>
        /// Player armour class. Auto-calculated so not serialized
        /// </summary>
        int armourClass;

        /// <summary>
        /// Player damage base. Auto-calculated so not serialized
        /// </summary>
        int damageBase;

        /// <summary>
        /// Player damage modifier. Auto-calculated so not serialized
        /// </summary>
        int damageModifier;

        /// <summary>
        /// Player damage modifier. Auto-calculated so not serialized
        /// </summary>
        int hitModifier;

        /// <summary>
        /// Magic casting points
        /// </summary>
        int magicPoints;

        /// <summary>
        /// Maximum magic points
        /// </summary>
        int maxMagicPoints;


        /// <summary>
        /// Number of times we get knocked out
        /// </summary>
        public int NumDeaths { get; set; }

        public int MaxCharmedCreatures { get; set; }

        public int CurrentCharmedCreatures { get; set; }

        public bool CombatUse { get; set; }
        public bool MagicUse { get; set; }
        public bool CharmUse { get; set; }

        /// <summary>
        /// Combat stat calculated from training stat and items
        /// </summary>
        public int CharmPoints { get; set; }

        /// <summary>
        /// PrincessRL has a maximum no of equipped items when using EquipItemNoSlots
        /// </summary>
        public int MaximumEquippedItems { get; set; }

        public int CurrentEquippedItems { get; set; }

        //XP in dungeons
        public int CombatXP { get; set; }
        public int MagicXP { get; set; }
        public int CharmXP { get; set; }

        //Training stats

        public int MaxHitpointsStat { get; set; }
        public int HitpointsStat { get; set; }
        public int AttackStat { get; set; }
        public int SpeedStat { get; set; }
        public int CharmStat { get; set; }
        public int MagicStat { get; set; }

        public int ArmourClassAccess { get { return armourClass; }  set { armourClass = value; } }
        public int DamageBaseAccess { get { return damageBase; } set { damageBase = value; } }
        public int DamageModifierAccess { get { return damageModifier; } set { damageModifier = value; } }
        public int HitModifierAccess { get { return hitModifier; } set { hitModifier = value; } }


        public Player()
        {
            //Set unique ID to 0 (player)
            UniqueID = 0;

            effects = new List<PlayerEffect>();
            Kills = new List<Monster>();

            Level = 1;

            //Representation = '\xd7';

            MaxCharmedCreatures = 0;
            CurrentCharmedCreatures = 0;
            MaximumEquippedItems = 3;

            NumDeaths = 0;
            CombatUse = false;
            MagicUse = false;
            CharmUse = false;

            CurrentDungeon = -1;

            //Add default equipment slots
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Utility));
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));

            //Set initial HP
            SetupInitialHP();

            //Setup initial training stats
            SetupInitialTrainingStats();

            //Setup combat parameters
            //CalculateCombatStats();

            TurnCount = 0;
        }

        private void SetupInitialTrainingStats()
        {
            MaxHitpointsStat = 50;
            HitpointsStat = 50;
            MagicPoints = 10;
            MaxMagicPoints = 10;
            SpeedStat = 10;
            AttackStat = 10;
            CharmStat = 10;
            MagicStat = 10;
            
            
            //Debug
            /*
            AttackStat = 100;
            MagicStat = 100;
            HitpointsStat = 60;
            MaxHitpointsStat = 60;
            CharmStat = 120;
             */
        }

        private void SetupInitialHP()
        {
            //CalculateCombatStats();
            maxHitpoints = 10;
            hitpoints = maxHitpoints;
        }

        /// <summary>
        /// Remove all our items and reset our equipped items count
        /// </summary>
        public void RemoveAllItems()
        {
            Inventory.RemoveAllItems();
            CurrentEquippedItems = 0;
        }

        public int MagicPoints
        {
            get
            {
                return magicPoints;
            }
            set
            {
                magicPoints = value;
            }
        }

        public int MaxMagicPoints
        {
            get
            {
                return maxMagicPoints;
            }
            set
            {
                maxMagicPoints = value;
            }
        }

        /// <summary>
        /// Function called after an effect is applied or a new item is equipped.
        /// Calculates all derived statistics from bases with modifications from equipment and effects.
        /// </summary>
        public void CalculateCombatStats()
        {

            Inventory inv = Inventory;

            //Armour class
            ArmourClassAccess = 12;

            //Charm points
            CharmPoints = CharmStat;

            //Max charmed creatures
            /*
            if (inv.ContainsItem(new Items.SparklingEarrings()))
            {
                MaxCharmedCreatures = 2;
            }
            else
                MaxCharmedCreatures = 1;
            */
            //Sight

            NormalSightRadius = 0;

            /*
            if(inv.ContainsItem(new Items.Lantern()))
                NormalSightRadius = 7;
            */
            //Speed

            int speedDelta = SpeedStat - 10;

            speedDelta = speedDelta * 2;

            Speed = 100 + speedDelta;

            //To Hit

            int toHit;

            if (AttackStat > 60)
            {
                toHit = (int)Math.Round((AttackStat - 60) / 30.0) + 3;
            }
            else
            {
                toHit = AttackStat / 20;
            }

            HitModifierAccess = toHit;

            //Damage base

            int damageBase;
            if (AttackStat > 120)
            {
                damageBase = 12;
            }
            if (AttackStat > 80)
            {
                damageBase = 10;
            }
            else if (AttackStat > 50)
            {
                damageBase = 8;
            }
            else if (AttackStat > 20)
            {
                damageBase = 6;
            }
            else
                damageBase = 4;

            DamageBaseAccess = damageBase;

            //Armour

            Screen.Instance.PCColor = ColorPresets.White;
            /*
            if (inv.ContainsItem(new Items.MetalArmour()) && AttackStat > 50)
            {
                ArmourClassAccess += 6;
                Screen.Instance.PCColor = ColorPresets.SteelBlue;
            }
            else if (inv.ContainsItem(new Items.LeatherArmour()) && AttackStat > 25)
            {
                ArmourClassAccess += 3;
                Screen.Instance.PCColor = ColorPresets.BurlyWood;
            }
            else if (inv.ContainsItem(new Items.KnockoutDress()))
            {
                CharmPoints += 40;
                ArmourClassAccess += 3;
                Screen.Instance.PCColor = ColorPresets.Yellow;
            }
            else if (inv.ContainsItem(new Items.PrettyDress()))
            {
                CharmPoints += 20;
                ArmourClassAccess += 1;
                Screen.Instance.PCColor = ColorPresets.BlueViolet;
            }*/

            //Consider equipped weapons (only 1 will work)
            DamageModifierAccess = 0;

            /*
            if (inv.ContainsItem(new Items.GodSword()))
            {
                DamageModifierAccess += 8;
            }
            else if (inv.ContainsItem(new Items.LongSword()))
            {
                DamageModifierAccess += 4;
            }
            else if (inv.ContainsItem(new Items.ShortSword()))
            {
                DamageModifierAccess += 2;
            }
            else if (inv.ContainsItem(new Items.Dagger()))
            {
                DamageModifierAccess += 1;
            }*/

            //Check and apply effects

            ApplyEffects();

            //Calculate sight radius (depends on dungeon light level)

            CalculateSightRadius();
        }



        /// <summary>
        /// Calculate the derived (used by other functions) sight radius based on the player's NormalSightRadius and the light level of the dungeon level the player is on
        /// Note that 0 is infinite
        /// </summary>
        public void CalculateSightRadius()
        {
            //Set vision
            double sightRatio = NormalSightRadius / 5.0;
            SightRadius = (int)Math.Ceiling(Game.Dungeon.Levels[LocationLevel].LightLevel * sightRatio);
        }

        /// <summary>
        /// As part of CalculateCombatStats, go through the current effects
        /// find the most-good and most-bad stat modifiers and apply them (only)
        /// </summary>
        private void ApplyEffects()
        {
            int maxDamage = 0;
            int minDamage = 0;

            int maxHit = 0;
            int minHit = 0;

            int maxAC = 0;
            int minAC = 0;

            int maxSpeed = 0;
            int minSpeed = 0;

            int maxSight = 0;
            int minSight = 0;

            //Only the greatest magnitude (+ or -) effects have an effect
            foreach (PlayerEffect effect in effects)
            {
                if(effect.ArmourClassModifier() > maxAC)
                    maxAC = effect.ArmourClassModifier();

                if(effect.ArmourClassModifier() < minAC)
                    minAC = effect.ArmourClassModifier();

                if(effect.HitModifier() > maxHit)
                    maxHit = effect.HitModifier();

                if(effect.HitModifier() < minHit)
                    minHit = effect.HitModifier();

                if(effect.SpeedModifier() > maxSpeed)
                    maxSpeed = effect.SpeedModifier();

                if(effect.SpeedModifier() < minSpeed)
                    minSpeed = effect.SpeedModifier();

                if(effect.DamageModifier() > maxDamage)
                    maxDamage = effect.DamageModifier();

                if(effect.DamageModifier() < minDamage)
                    minDamage = effect.DamageModifier();

                if (effect.SightModifier() < minSight)
                    minSight = effect.SightModifier();

                if (effect.SightModifier() > maxSight)
                    maxSight = effect.SightModifier();
            }

            damageModifier += maxDamage;
            damageModifier += minDamage;

            Speed += maxSpeed;
            Speed += minSpeed;

            hitModifier += maxHit;
            hitModifier += minHit;

            armourClass += maxAC;
            armourClass += minAC;

            NormalSightRadius += maxSight;
            NormalSightRadius += minSight;
        }

        /// <summary>
        /// Current HP
        /// </summary>
        public int Hitpoints
        {
            get
            {
                return hitpoints;
            }
            set
            {
                hitpoints = value;
            }
        }

        /// <summary>
        /// Normal maximum hp
        /// </summary>
        public int MaxHitpoints
        {
            get
            {
                return maxHitpoints;
            }
            set
            {
                maxHitpoints = value;
            }
        }

        /// <summary>
        /// Player can overdrive to 50% of normal hp. This is the max possible with this fact.
        /// </summary>
        public int OverdriveHitpoints { get; set; }

        /// <summary>
        /// Used as accessors only for Player
        /// </summary>
        /// <returns></returns>
        public override int BaseSpeed()
        {
            return Speed;
        }

        /// <summary>
        /// Used as accessors only for Player
        /// </summary>
        public override int ArmourClass()
        {
            return armourClass;
        }

        /// <summary>
        /// Used as accessors only for Player
        /// </summary>
        public override int DamageBase()
        {
            return damageBase;
        }

        /// <summary>
        /// Used as accessors only for Player
        /// </summary>
        public override int DamageModifier()
        {
            return damageModifier;
        }

        public override int HitModifier()
        {
            return hitModifier;
        }

        /// <summary>
        /// Will we have a turn if we IncrementTurnTime()
        /// </summary>
        /// <returns></returns>
        public bool CheckReadyForTurn()
        {
            if(turnClock + speed >= turnClockLimit)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Work out the damage from an attack with the specified one-time modifiers (could be from a special attack etc.)
        /// Note ACmod positive is a bonus to the monster AC
        /// </summary>
        /// <param name="hitMod"></param>
        /// <param name="damBase"></param>
        /// <param name="damMod"></param>
        /// <param name="ACmod"></param>
        /// <returns></returns>


        //int toHitRoll; //just so we can use it in debug

        private int AttackWithModifiers(Monster monster, int hitMod, int damBase, int damMod, int ACmod)
        {
            //Flatline has a rather simple combat system
            IEquippableItem item = GetEquippedWeapon();

            int baseDamage = 2;

            if (item.HasMeleeAction())
            {
                baseDamage = item.MeleeDamage();
            }

            string combatResultsMsg = "PvM " + monster.Representation + " = " + baseDamage;

            return baseDamage;

            /*
            int attackToHit = hitModifier + hitMod;
            int attackDamageMod = damageModifier + damMod;
            
            int attackDamageBase;

            if(damBase > damageBase)
                attackDamageBase = damBase;
            else
                attackDamageBase = damageBase;

            int monsterAC = monster.ArmourClass() + ACmod;
            int toHitRoll = Utility.d20() + attackToHit;

            if (toHitRoll >= monsterAC)
            {
                //Hit - calculate damage
                int totalDamage = Utility.DamageRoll(attackDamageBase) + attackDamageMod;
                string combatResultsMsg = "PvM " + monster.Representation + " ToHit: " + toHitRoll + "[+" + hitModifier + "+" + hitMod + "] AC: " + monsterAC + "(" + monster.ArmourClass() + "+" + ACmod + ") " + " Dam: 1d" + attackDamageBase + "+" + damageModifier + "+" + damMod + " = " + totalDamage;

                //            string combatResultsMsg = "PvM Attack ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monster.Hitpoints + " miss";
                LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                return totalDamage;
            }*/

            //Miss
            //return 0;
        }

        public bool CastSpell(Spell toCast, Point target)
        {
            //Check MP
            if (toCast.MPCost() > MagicPoints)
            {
                Game.MessageQueue.AddMessage("Not enough MP! " + toCast.MPCost().ToString() + " required.");
                LogFile.Log.LogEntryDebug("Not enough MP to cast " + toCast.SpellName(), LogDebugLevel.Medium);

                return false;
            }

            //Check we are in target
            int range = toCast.GetRange();
            /*
            if (Inventory.ContainsItem(new Items.ExtendOrb()))
            {
                range += 1;
            }*/

            if (toCast.NeedsTarget() && Dungeon.GetDistanceBetween(LocationMap, target) > range)
            {
                Game.MessageQueue.AddMessage("Out of range!");
                LogFile.Log.LogEntryDebug("Out of range for " + toCast.SpellName(), LogDebugLevel.Medium);

                return false;
            }


            //Actually cast the spell
            bool success = toCast.DoSpell(target);

            //Remove MP if successful
            if (success)
            {
                MagicPoints -= toCast.MPCost();
                if (MagicPoints < 0)
                    MagicPoints = 0;

                //Using magic is an instrinsic
                MagicUse = true;
            }

            return success;
        }

         /// <summary>
        /// Normal attack on a monster. Takes care of killing them off if required.
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        public CombatResults AttackMonster(Monster monster)
        {
            return AttackMonsterWithModifiers(monster, 0, 0, 0, 0, false);
        }

        /// <summary>
        /// Attack a monster with modifiers. Takes care of killing them off if required.
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        public CombatResults AttackMonsterWithModifiers(Monster monster, int hitModifierMod, int damageBaseMod, int damageModifierMod, int enemyACMod, bool specialMoveUsed)
        {
            //Do we need to recalculate combat stats?
            if (this.RecalculateCombatStatsRequired)
                this.CalculateCombatStats();

            if (monster.RecalculateCombatStatsRequired)
                monster.CalculateCombatStats();

            //Attacking a monster with hand to hand give an instrinsic
            CombatUse = true;

            //Calculate damage from a normal attack

            int damage = AttackWithModifiers(monster, hitModifierMod, damageBaseMod, damageModifierMod, enemyACMod);

            return ApplyDamageToMonster(monster, damage, false, specialMoveUsed);
        }

        public CombatResults AttackMonsterRanged(Monster monster, int damage)
        {
            string combatResultsMsg = "PvM (ranged) " + monster.Representation + " = " + damage;
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            return ApplyDamageToMonster(monster, damage, false, false);
        }

        public CombatResults AttackMonsterThrown(Monster monster, int damage)
        {
            string combatResultsMsg = "PvM (thrown) " + monster.Representation + " = " + damage;
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            return ApplyDamageToMonster(monster, damage, false, false);
        }

        public CombatResults AttackMonsterMelee(Monster monster)
        {
            //Flatline has a rather simple combat system
            IEquippableItem item = GetEquippedWeapon();

            int baseDamage = 2;

            if (item != null && item.HasMeleeAction())
            {
                baseDamage = item.MeleeDamage();
            }

            string combatResultsMsg = "PvM (melee) " + monster.Representation + " = " + baseDamage;
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            return ApplyDamageToMonster(monster, baseDamage, false, false);
        }

        /// <summary>
        /// Only here and the Monster equivalent are places where the player can get damaged. Should unify them.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public CombatResults ApplyDamageToPlayer(int damage)
        {
                //Do we hit the player?
            if (damage > 0)
            {
                Hitpoints -= damage;

                //Is the player dead, if so kill it?
                if (Hitpoints <= 0)
                {
                    Game.Dungeon.SetPlayerDeath("was knocked out by a themselves");

                    //Message queue string
                    string combatResultsMsg = "PvP Damage " + damage;


                    //string playerMsg = "The " + this.SingleDescription + " hits you. You die.";
                    string playerMsg = "You knocked yourself out.";
                    Game.MessageQueue.AddMessage(playerMsg);
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    return CombatResults.DefenderDied;
                }

                //Debug string
                //string combatResultsMsg3 = "MvP ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + player.Hitpoints + " injured";
                //string playerMsg3 = "The " + this.SingleDescription + " hits you.";
                string combatResultsMsg2 = "PvP Damage " + damage;
                Game.MessageQueue.AddMessage("Ouch, you hurt yourself.");
                LogFile.Log.LogEntryDebug(combatResultsMsg2, LogDebugLevel.Medium);

                return CombatResults.DefenderDamaged;
            }

            //Miss
            //string combatResultsMsg2 = "MvP ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + player.Hitpoints + " miss";
            //string playerMsg2 = "The " + this.SingleDescription + " misses you.";
            string combatResultsMsg3 = "PvP Damage " + damage;
            string playerMsg2 = "You avoid damaging yourself";
            Game.MessageQueue.AddMessage(playerMsg2);
            LogFile.Log.LogEntryDebug(combatResultsMsg3, LogDebugLevel.Medium);

            return CombatResults.DefenderUnhurt;
        }

        /// <summary>
        /// Apply stun damage (miss n-turns) to monster. All stun attacks are routed through here
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="stunTurns"></param>
        /// <returns></returns>
        public CombatResults ApplyStunDamageToMonster(Monster monster, int stunTurns)
        {
            //Wake monster up etc.
            AIForMonsterIsAttacked(monster);

            int monsterOrigStunTurns = monster.StunnedTurns;

            //Do we hit the monster?
            if (stunTurns > 0)
            {
                monster.StunnedTurns += stunTurns;

                //Notify the creature that it has taken damage
                //It may activate a special ability or stop running away etc.
                monster.NotifyHitByCreature(this, 0);

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
        /// Apply damage to monster and deal with death. All player attacks are routed through here.
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        public CombatResults ApplyDamageToMonster(Monster monster, int damage, bool magicUse, bool specialMove)
        {
            //Wake monster up etc.
            AIForMonsterIsAttacked(monster);

            //Do we hit the monster?
            if (damage > 0)
            {
                int monsterOrigHP = monster.Hitpoints;

                monster.Hitpoints -= damage;

                bool monsterDead = monster.Hitpoints <= 0;

                //Add HP from the glove if wielded
                SpecialCombatEffectsOnMonster(monster, damage, monsterDead, specialMove);

                //Notify the creature that it has taken damage
                //It may activate a special ability or stop running away etc.
                monster.NotifyHitByCreature(this, damage);

                //Is the monster dead, if so kill it?
                if (monsterDead)
                {
                    Game.Dungeon.KillMonster(monster, false);

                    //Add it to our list of kills (simply adding the whole object here)
                    KillCount++;
                    Kills.Add(monster);

                    //Message string
                    string playerMsg = "You destroyed ";
                    if(!monster.Unique)
                        playerMsg += "the ";
                    playerMsg += monster.SingleDescription + ".";
                    Game.MessageQueue.AddMessage(playerMsg);

                    string debugMsg = "MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " killed";
                    LogFile.Log.LogEntryDebug(debugMsg, LogDebugLevel.Medium);
   
                    //No XP in flatline
                    //Add XP
                    //AddXPPlayerAttack(monster, magicUse);

                    return CombatResults.DefenderDied;
                }

                //Message string
                string playerMsg2 = "You hit ";
                if(!monster.Unique)
                    playerMsg2 += "the ";
                playerMsg2 += monster.SingleDescription + ".";
                Game.MessageQueue.AddMessage(playerMsg2);
                
                string debugMsg2 = "MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " injured";
                LogFile.Log.LogEntryDebug(debugMsg2, LogDebugLevel.Medium);

                return CombatResults.DefenderDamaged;
            }

            //Miss

            string playerMsg3 = "You missed the " + monster.SingleDescription + ".";
            Game.MessageQueue.AddMessage(playerMsg3);
            string debugMsg3 = "MHP: " + monster.Hitpoints + "->" + monster.Hitpoints + " missed";
            LogFile.Log.LogEntryDebug(debugMsg3, LogDebugLevel.Medium);

            return CombatResults.NeitherDied;
        }

        /// <summary>
        /// Monster has been attacked. Wake it up etc.
        /// </summary>
        /// <param name="monster"></param>
        private void AIForMonsterIsAttacked(Monster monster)
        {
            //Set the attacked by marker
            monster.LastAttackedBy = this;
            monster.LastAttackedByID = this.UniqueID;

            //Was this a passive creature? It loses that flag
            if (monster.Passive)
                monster.UnpassifyCreature();

            //Was this a sleeping creature? It loses that flag
            if (monster.Sleeping)
            {
                monster.WakeCreature();

                //All wake on sight creatures should be awake at this point. If it's a non-wake-on-sight tell the player it wakes
                Game.MessageQueue.AddMessage("The " + monster.SingleDescription + " wakes up!");
                LogFile.Log.LogEntryDebug(monster.Representation + " wakes on attack by player", LogDebugLevel.Low);
            }

            //Notify the creature that it has been hit
            monster.NotifyAttackByCreature(this);
        }

        /// <summary>
        /// A monster has been killed by magic or combat. Add XP
        /// </summary>
        /// <param name="magicUse"></param>
        private void AddXPPlayerAttack(Monster monster, bool magicUse)
        {
            //No XP for summonded creatures
            if (monster.WasSummoned)
            {
                LogFile.Log.LogEntryDebug("No XP for summounded creatures.", LogDebugLevel.Medium);
                return;
            }

            //Magic case
            if (magicUse)
            {
                int monsterXP = monster.GetMagicXP();
                double diffDelta = (MagicStat - monsterXP) / (double)MagicStat;
                if (diffDelta < 0)
                    diffDelta = 0;

                double xpUpChance = 1 - diffDelta;
                int xpUpRoll = (int)Math.Floor(xpUpChance * 100.0);
                int xpUpRollActual = Game.Random.Next(100);
                LogFile.Log.LogEntryDebug("MagicXP up. Chance: " + xpUpRoll + " roll: " + xpUpRollActual, LogDebugLevel.Medium);

                if (xpUpRollActual < xpUpRoll)
                {
                    MagicXP++;
                    LogFile.Log.LogEntryDebug("MagicXP up!", LogDebugLevel.Medium);
                    Game.MessageQueue.AddMessage("You feel your magic grow stronger.");
                }
            }
            //Combat use
            else
            {
                int monsterXP = monster.GetCombatXP();
                double diffDelta = (AttackStat - monsterXP) / (double)AttackStat;
                if (diffDelta < 0)
                    diffDelta = 0;

                double xpUpChance = 1 - diffDelta;
                int xpUpRoll = (int)Math.Floor(xpUpChance * 100.0);
                int xpUpRollActual = Game.Random.Next(100);
                LogFile.Log.LogEntryDebug("CombatXP up roll. Chance: " + xpUpRoll + " roll: " + xpUpRollActual, LogDebugLevel.Medium);

                if (xpUpRollActual < xpUpRoll)
                {
                    CombatXP++;
                    LogFile.Log.LogEntryDebug("CombatXP up!", LogDebugLevel.Medium);
                    Game.MessageQueue.AddMessage("You feel your combat skill increase.");
                }
            }
        }

        /// <summary>
        /// List of special combat effects that might happen to a HIT monster
        /// </summary>
        /// <param name="monster"></param>
        private void SpecialCombatEffectsOnMonster(Monster monster, int damage, bool isDead, bool specialMove)
        {
            //If short sword is equipped, do a slow down effect (EXAMPLE)
            /*
            Item shortSword = null;
            foreach (Item item in Inventory.Items)
            {
                if (item as Items.ShortSword != null)
                {
                    shortSword = item as Items.ShortSword;
                    break;
                }
            }

            //If we are using the short sword apply the slow effect
            if (shortSword != null)
            {
                monster.AddEffect(new MonsterEffects.SlowDown(monster, 500, 50));
            }*/

            //If glove is equipped we leech some of the monster HP

            Player player = Game.Dungeon.Player;
            
            Item glove = null;
            foreach (Item item in Inventory.Items)
            {
                /*
                if (item as Items.Glove != null)
                {
                    glove = item as Items.Glove;
                    break;
                }*/
            }

            if (glove != null && specialMove)
            {
                //The glove in PrincessRL only works on special moves

                double hpGain;

              //  if (player.AttackStat < 50)
                    hpGain = damage / 10.0;
              //  else
              //      hpGain = damage / 5.0;

                GainHPFromLeech((int)Math.Ceiling(hpGain));
                


                /*
                //If the monster isn't dead we get 1/5th of the HP done
                if (!isDead)
                {
                    double hpGain = damage / 5.0;

                    if (hpGain > 0.9999)
                    {
                        GainHPFromLeech((int)Math.Ceiling(hpGain));
                    }

                    //If we're become 1 there's only a chance that we gain an HP
                    else
                    {
                        int hpChance = (int) (hpGain * 100.0);
                        if (Game.Random.Next(100) < hpChance)
                            GainHPFromLeech(1);
                    }
                }

                //If monster is dead we get 1/5 of the total HP
                else
                {
                    double hpGain = monster.MaxHitpoints / 10.0;

                    if (hpGain > 0.9999)
                    {
                        GainHPFromLeech((int)Math.Ceiling(hpGain));
                    }

                    //If we're become 1 there's only a chance that we gain an HP
                    else
                    {
                        int hpChance = (int) (hpGain * 100.0);
                        if (Game.Random.Next(100) < hpChance)
                            GainHPFromLeech(1);
                    }
                }*/
            }
        }

        /// <summary>
        /// Increase HP from leech attack up to overdrive limit
        /// </summary>
        /// <param name="numHP"></param>
        internal void GainHPFromLeech(int numHP)
        {
            hitpoints += numHP;

            if (hitpoints > maxHitpoints)
                hitpoints = maxHitpoints;

            LogFile.Log.LogEntryDebug("Gain " + numHP + " hp from leech.", LogDebugLevel.Medium);
        }

        /// <summary>
        /// Increment time on all player events. Events that expire will run their onExit() routines and then delete themselves from the list
        /// </summary>
        internal void IncrementEventTime()
        {
            //Increment time on events and remove finished ones
            List<PlayerEffect> finishedEffects = new List<PlayerEffect>();

            bool eventEnded = false;

            foreach (PlayerEffect effect in effects)
            {
                effect.IncrementTime(this);

                if (effect.HasEnded())
                {
                    finishedEffects.Add(effect);
                    eventEnded = true;
                }
            }

            //Remove finished effects
            foreach (PlayerEffect effect in finishedEffects)
            {
                effects.Remove(effect);
            }

            if(eventEnded)
                CalculateCombatStats();
        }

        /// <summary>
        /// Remove all effects on player
        /// </summary>
        internal void RemoveAllEffects()
        {
            //Increment time on events and remove finished ones
            List<PlayerEffect> finishedEffects = new List<PlayerEffect>();

            foreach (PlayerEffect effect in effects)
            {
                if(!effect.HasEnded())
                    effect.OnEnd(this);

                finishedEffects.Add(effect);
            }

            //Remove finished effects
            foreach (PlayerEffect effect in finishedEffects)
            {
                effects.Remove(effect);
            }

            //Check the effect on our stats
            CalculateCombatStats();
        }

        /// <summary>
        /// Increment time on all player events then use the base class to increment time on the player's turn counter
        /// </summary>
        /// <returns></returns>
        internal override bool IncrementTurnTime()
        {
            IncrementEventTime();

            //Work around for bizarre problem - shouldn't happen any more
            if (speed < 30)
            {
                LogFile.Log.LogEntryDebug("ERROR! Player's speed reduced to <30", LogDebugLevel.High);
                speed = 100;
            }

            OverdriveHitpointDecay();

            return base.IncrementTurnTime();
        }


        int overDriveDecayCounter = 0;

        /// <summary>
        /// If we're over our max hitpoint, they decay slowly
        /// This function is typically called 100 times per turn for a normal speed character
        /// </summary>
        private void OverdriveHitpointDecay()
        {
            overDriveDecayCounter++;

            if (hitpoints <= maxHitpoints)
                return;

            //Lose 1% of overdrive HP rounded up per turn
            if (overDriveDecayCounter > 1000)
            {
                overDriveDecayCounter = 0;

                //Proportional decay
                double hpToLose = (hitpoints - maxHitpoints) / 100.0;
                int hpLoss = (int)Math.Ceiling(hpToLose);

                hitpoints -= hpLoss;

                if (hitpoints < maxHitpoints)
                    hitpoints = maxHitpoints;
            }
        }

        /// <summary>
        /// Run an effect on the player. Calls the effect's onStart and adds it to the current effects queue
        /// </summary>
        /// <param name="effect"></param>
        internal void AddEffect(PlayerEffect effect)
        {
            effects.Add(effect);

            effect.OnStart(this);

            //Check if it altered our combat stats
            CalculateCombatStats();
            
            //Should be done in effect itself or optionally each time we attack
            //I prefer it done here, less to remember
        }

        /// <summary>
        /// Is this class of effect currently active?
        /// Refactor to take a Type not an object
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public bool IsEffectActive(Type effectType)
        {
            PlayerEffect activeEffect = effects.Find(x => x.GetType() == effectType);

            if (activeEffect != null)
                return true;
            
            return false;
        }


        protected override char GetRepresentation()
        {
            return '\x15';
        }

        /// <summary>
        /// Equip an item. Item is removed from the main inventory.
        /// Returns true if item was used successfully.
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        public bool EquipItem(InventoryListing selectedGroup)
        {
            //Select the first item in the stack
            int itemIndex = selectedGroup.ItemIndex[0];
            Item itemToUse = Inventory.Items[itemIndex];

            //Check if this item is equippable
            IEquippableItem equippableItem = itemToUse as IEquippableItem;

            if (equippableItem == null)
            {
                LogFile.Log.LogEntryDebug("Can't equip item, not equippable: " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Can't equip " + itemToUse.SingleItemDescription);
                return false;
            }

            //Find all matching slots available on the player

            List<EquipmentSlot> itemPossibleSlots = equippableItem.EquipmentSlots;
            List<EquipmentSlotInfo> matchingEquipSlots = new List<EquipmentSlotInfo>();

            foreach (EquipmentSlot slotType in itemPossibleSlots)
            {
                matchingEquipSlots.AddRange(this.EquipmentSlots.FindAll(x => x.slotType == slotType));
            }

            //No suitable slots
            if (matchingEquipSlots.Count == 0)
            {
                LogFile.Log.LogEntryDebug("Can't equip item, no valid slots: " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Can't equip " + itemToUse.SingleItemDescription);

                return false;
            }

            //Look for first empty slot

            EquipmentSlotInfo freeSlot = matchingEquipSlots.Find(x => x.equippedItem == null);

            if (freeSlot == null)
            {
                //Not slots free, unequip first slot
                Item oldItem = matchingEquipSlots[0].equippedItem;
                IEquippableItem oldItemEquippable = oldItem as IEquippableItem;

                //Sanity check
                if (oldItemEquippable == null)
                {
                    LogFile.Log.LogEntry("Currently equipped item is not equippable!: " + oldItem.SingleItemDescription);
                    return false;
                }

                //Run unequip routine
                oldItemEquippable.UnEquip(this);
                oldItem.IsEquipped = false;
                
                //Can't do this right now, since not in inventory items appear on the floor

                //This slot is now free
                freeSlot = matchingEquipSlots[0];
            }

            //We now have a free slot to equip in

            //Put new item in first relevant slot and run equipping routine
            matchingEquipSlots[0].equippedItem = itemToUse;
            equippableItem.Equip(this);
            itemToUse.IsEquipped = true;

            //Update the inventory listing since equipping an item changes its stackability
            Inventory.RefreshInventoryListing();

            //Message the user
            LogFile.Log.LogEntryDebug("Item equipped: " + itemToUse.SingleItemDescription, LogDebugLevel.Low);
            Game.MessageQueue.AddMessage(itemToUse.SingleItemDescription + " equipped");

            return true;
        }


        /// <summary>
        /// Drop an item at a specific point. Equippable items never exist in the inventory in FlatlineRL
        /// </summary>
        /// <param name="itemToDrop"></param>
        /// <returns></returns>
        public bool DropEquippableItem(Item itemToDrop, int levelToDropAt, Point locToDropAt)
        {
            //Add back to the dungeon store
            Game.Dungeon.Items.Add(itemToDrop);

            itemToDrop.LocationLevel = levelToDropAt;
            itemToDrop.LocationMap = locToDropAt;
            itemToDrop.InInventory = false;

            return true;
        }

        /// <summary>
        /// Drop an item at current location. Equippable items never exist in the inventory in FlatlineRL
        /// </summary>
        /// <param name="itemToDrop"></param>
        /// <returns></returns>
        public bool DropEquippableItem(Item itemToDrop)
        {
            //Add back to the dungeon store
            return DropEquippableItem(itemToDrop, this.LocationLevel, this.LocationMap);
        }

        /// <summary>
        /// Pick up an item. Equippable items never exist in the inventory in FlatlineRL
        /// </summary>
        /// <param name="itemToDrop"></param>
        /// <returns></returns>
        public bool PickupEquippableItem(Item itemToPickup)
        {
            //Remove from the dungeon store, to avoid duplication on serialization
            Game.Dungeon.Items.Remove(itemToPickup);

            itemToPickup.InInventory = true;

            return true;
        }

        /// <summary>
        /// Equip an item into a relevant slot.
        /// Will unequip and drop an item in the same slot.
        /// Returns true if operation successful
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        public bool EquipAndReplaceItem(Item itemToUse)
        {
            //Check if this item is equippable
            IEquippableItem equippableItem = itemToUse as IEquippableItem;

            if (equippableItem == null)
            {
                LogFile.Log.LogEntryDebug("Can't equip item, not equippable: " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Can't equip " + itemToUse.SingleItemDescription);
                return false;
            }

            //Find all matching slots available on the player

            List<EquipmentSlot> itemPossibleSlots = equippableItem.EquipmentSlots;
            //Is always only 1 slot in FlatlineRL

            EquipmentSlot itemSlot = itemPossibleSlots[0];
            
            //We always have 2 equipment slots, 1 of each type on a player in FlatlineRL
            //So we should match exactly on 1 free slot

            List<EquipmentSlotInfo> matchingEquipSlots = new List<EquipmentSlotInfo>();

            foreach (EquipmentSlot slotType in itemPossibleSlots)
            {
                matchingEquipSlots.AddRange(this.EquipmentSlots.FindAll(x => x.slotType == slotType));
            }

            //No suitable slots
            if (matchingEquipSlots.Count == 0)
            {
                LogFile.Log.LogEntryDebug("Can't equip item, no valid slots: " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Can't equip " + itemToUse.SingleItemDescription);

                return false;
            }

            //Look for first empty slot

            EquipmentSlotInfo freeSlot = matchingEquipSlots.Find(x => x.equippedItem == null);

            if (freeSlot == null)
            {
                //Not slots free, unequip first slot
                Item oldItem = matchingEquipSlots[0].equippedItem;
                IEquippableItem oldItemEquippable = oldItem as IEquippableItem;

                //Sanity check
                if (oldItemEquippable == null)
                {
                    LogFile.Log.LogEntry("Old item did not equip: " + oldItem.SingleItemDescription);
                    return false;
                }

                LogFile.Log.LogEntryDebug("Dropping old item " + oldItem.SingleItemDescription, LogDebugLevel.Medium);

                UnequipAndDropItem(oldItem);
                
                //This slot is now free
                freeSlot = matchingEquipSlots[0];
            }

            //We now have a free slot to equip in

            //Put new item in first relevant slot and run equipping routine
            matchingEquipSlots[0].equippedItem = itemToUse;
            equippableItem.Equip(this);
            itemToUse.IsEquipped = true;

            PickupEquippableItem(itemToUse);

            LogFile.Log.LogEntryDebug("Equipping new item " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);

            //Message the user
            LogFile.Log.LogEntryDebug("Item equipped: " + itemToUse.SingleItemDescription, LogDebugLevel.Low);
            Game.MessageQueue.AddMessage(itemToUse.SingleItemDescription + " equipped");

            return true;
        }
        /// <summary>
        /// FlatlineRL - unequip and item at drop at player loc
        /// </summary>
        public void UnequipAndDropItem(Item item)
        {
            UnequipAndDropItem(item, this.LocationLevel, this.LocationMap);
        }

        /// <summary>
        /// FlatlineRL - unequip and item at drop at the coords given
        /// </summary>
        public void UnequipAndDropItem(Item item, int levelToDrop, Point toDropLoc)
        {
            //Run unequip routine
            IEquippableItem equipItem = item as IEquippableItem;
            equipItem.UnEquip(this);
            item.IsEquipped = false;

            //Locate the slot it was in and empty it
            EquipmentSlotInfo oldSlot = EquipmentSlots.Find(x => x.equippedItem == item);
            if (oldSlot == null)
            {
                LogFile.Log.LogEntryDebug("Error - can't find equipment slot for item " + item.SingleItemDescription, LogDebugLevel.High);
            }
            else
            {
                oldSlot.equippedItem = null;
            }

            //Drop the old item
            DropEquippableItem(item, levelToDrop, toDropLoc);

        }

        public void UnequipAndDestoryAllItems()
        {
            foreach (EquipmentSlotInfo es in EquipmentSlots)
            {
                UnequipAndDestroyItem(es.equippedItem);
            }
        }

        /// <summary>
        /// FlatlineRL - unequip item and remove it from the game
        /// </summary>
        public void UnequipAndDestroyItem(Item item)
        {
            if (item == null)
                return;


            //Run unequip routine
            IEquippableItem equipItem = item as IEquippableItem;
            equipItem.UnEquip(this);
            item.IsEquipped = false;

            //Locate the slot it was in and empty it
            EquipmentSlotInfo oldSlot = EquipmentSlots.Find(x => x.equippedItem == item);
            if (oldSlot == null)
            {
                LogFile.Log.LogEntryDebug("Error - can't find equipment slot for item " + item.SingleItemDescription, LogDebugLevel.High);
            }
            else
            {
                oldSlot.equippedItem = null;
            }

            //Delete the old item
            //There should now be no references to it

        }

        /// <summary>
        /// FlatlineRL - return equipped weapon or null
        /// </summary>
        /// <returns></returns>
        public IEquippableItem GetEquippedWeapon() 
        {
            EquipmentSlotInfo weaponSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Weapon);

            if(weaponSlot == null) {
                LogFile.Log.LogEntryDebug("Can't find weapon slot - bug ", LogDebugLevel.High);
                return null;
            }

            return weaponSlot.equippedItem as IEquippableItem;
        }

        /// <summary>
        /// FlatlineRL - return equipped weapon as item reference (always works)
        /// </summary>
        /// <returns></returns>
        public Item GetEquippedWeaponAsItem()
        {
            EquipmentSlotInfo weaponSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Weapon);

            if(weaponSlot == null) {
                LogFile.Log.LogEntryDebug("Can't find weapon slot - bug ", LogDebugLevel.High);
                return null;
            }

            return weaponSlot.equippedItem;
        }

        /// <summary>
        /// FlatlineRL - return equipped utility or null
        /// </summary>
        /// <returns></returns>
        public IEquippableItem GetEquippedUtility()
        {
            EquipmentSlotInfo weaponSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Utility);

            if(weaponSlot == null) {
                LogFile.Log.LogEntryDebug("Can't find utility slot - bug ", LogDebugLevel.High);
                return null;
            }

            return weaponSlot.equippedItem as IEquippableItem;
        }

        /// <summary>
        /// FlatlineRL - return equipped utility or null
        /// </summary>
        /// <returns></returns>
        public Item GetEquippedUtilityAsItem()
        {
            EquipmentSlotInfo weaponSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Utility);

            if(weaponSlot == null) {
                LogFile.Log.LogEntryDebug("Can't find utility slot - bug ", LogDebugLevel.High);
                return null;
            }

            return weaponSlot.equippedItem;
        }

        /// <summary>
        /// Predicate for matching equipment slot of EquipmentSlot type
        /// </summary>
        private static bool EquipmentSlotMatchesType(EquipmentSlotInfo equipSlot, EquipmentSlot type)
        {
            return (equipSlot.slotType == type);
        }

        /// <summary>
        /// Use the item group. Function is responsible for deleting the item if used up etc. Return true if item was used successfully and time should be advanced.
        /// </summary>
        /// <param name="selectedGroup"></param>
        internal bool UseItem(InventoryListing selectedGroup)
        {
            //For now, we use the first item in any stack only
            int itemIndex = selectedGroup.ItemIndex[0];
            Item itemToUse = Inventory.Items[itemIndex];

            //Check if this is a useable item
            IUseableItem useableItem = itemToUse as IUseableItem;

            if (useableItem == null)
            {
                Game.MessageQueue.AddMessage("Cannot use this type of item!");
                LogFile.Log.LogEntry("Tried to use non-useable item: " + itemToUse.SingleItemDescription);
                return false;
            }

            bool usedSuccessfully = useableItem.Use(this);

            if (useableItem.UsedUp)
            {
                //Remove item from inventory and don't drop on floor
                //Goes back into the global list and will be respawned at town
                //Inventory.RemoveItem(itemToUse);
                
                //This permanently deletes it from the game
                //Game.Dungeon.RemoveItem(itemToUse);

                //If the line above is commented, the item will be returned to town. Will want to un-use it in this case
                
                //Only ditch the non-equippable items
                IEquippableItem equipItem = useableItem as IEquippableItem;
                if (equipItem == null)
                {
                    Inventory.RemoveItem(itemToUse);
                }

                

                //useableItem.UsedUp = false;
                
            }

            return usedSuccessfully;
        }



        /// <summary>
        /// Simpler version of equip item, doesn't care about slots
        /// </summary>
        /// <param name="equipItem"></param>
        internal bool EquipItemNoSlots(IEquippableItem equipItem)
        {
            Item item = equipItem as Item;

            if (item == null)
            {
                //Should never happen
                LogFile.Log.LogEntry("Problem with item equip");
                Game.MessageQueue.AddMessage("You can't equip this item (bug)");
                return false;
            }

            //Play help movie
            if (Game.Dungeon.Player.PlayItemMovies && ItemHelpMovieSeen == false)
            {
                Screen.Instance.PlayMovie("helpitems", true);
                ItemHelpMovieSeen = true;
            }

            //Set the item as found
            item.IsFound = true;

            //If we have room in our equipped slots, equip and add the item to the inventory
            if (CurrentEquippedItems < MaximumEquippedItems)
            {
                //Add the item to our inventory
                item.IsEquipped = true;
                Inventory.AddItem(item);

                CurrentEquippedItems++;

                //Let the item do its equip action
                //This can happen multiple times in PrincessRL since items can be dropped
                //Probably just play a video
                equipItem.Equip(this);

                //Update the player's combat stats which may have been affected

                CalculateCombatStats();

                //Update the inventory listing since equipping an item changes its stackability
                //No longer necessary since no equippable items get displayed in inventory
                //Inventory.RefreshInventoryListing();

                //Message the user
                LogFile.Log.LogEntryDebug("Item equipped: " + item.SingleItemDescription, LogDebugLevel.Medium);
                //Game.MessageQueue.AddMessage(item.SingleItemDescription + " found.");

                return true;
            }
            else if (LocationLevel == 0)
            {
                //If not, and we're in town, don't pick it up
                Game.MessageQueue.AddMessage("You can't carry any more items. Press 'd' to drop your current items.");
                LogFile.Log.LogEntryDebug("Max number of items reached", LogDebugLevel.Medium);

                return false;
            }
            else
            {
                //If not, and we're not in town, set it as inInventory so it won't be drawn. It'll get returned to town on when go back
                item.InInventory = true;

                //Play the video
                equipItem.Equip(this);

                Game.MessageQueue.AddMessage("You place the " + item.SingleItemDescription + " in your backpack.");
                LogFile.Log.LogEntryDebug("Max number of items reached. Item returns to town.", LogDebugLevel.Medium);

                return true;
            }          
        }

        /// <summary>
        /// Level up the player!
        /// </summary>
        internal void LevelUp()
        {
            //Level up!
            Level++;

            int lastMaxHP = maxHitpoints;

            //Recalculate combat stats
            CalculateCombatStats();

            hitpoints += maxHitpoints - lastMaxHP;

            //Calculate HP etc
            //HPOnLevelUP();

            LogFile.Log.LogEntry("Player levels up to: " + Level);
        }

        /// <summary>
        /// Apply level up effect to current hitpoints
        /// </summary>
        private void HPOnLevelUP()
        {
            hitpoints += 10;
            maxHitpoints += 10;
        }

        /// <summary>
        /// Try to add another charmed creature. Will return false if already at max.
        /// </summary>
        internal bool AddCharmCreatureIfPossible()
        {
            if (CurrentCharmedCreatures < MaxCharmedCreatures)
            {
                CurrentCharmedCreatures++;
                return true;
            }

            return false;
        }

        internal bool MoreCharmedCreaturesPossible()
        {
            if (CurrentCharmedCreatures < MaxCharmedCreatures)
                return true;
            return false;
        }

        internal void RemoveCharmedCreature()
        {
            CurrentCharmedCreatures--;

            if (CurrentCharmedCreatures < 0)
            {
                LogFile.Log.LogEntryDebug("tried to remove a charmed creature when there were 0", LogDebugLevel.High);
                CurrentCharmedCreatures = 0;
            }
        }

        /// <summary>
        /// This happens when a charmed creature attacks another or a non-charmed creature fights back
        /// </summary>
        /// <param name="attackingMonster"></param>
        /// <param name="targetMonster"></param>
        internal void AddXPMonsterAttack(Monster attackingMonster, Monster targetMonster)
        {
            //Check this monster was charmed
            if (!attackingMonster.Charmed)
            {
                LogFile.Log.LogEntryDebug("Attacking monster was not charmed, no XP.", LogDebugLevel.Medium);
                return;
            }

          //Add charm XP. Use the target's combat XP against the player's charm stat
            //This also has the advantage that every creature in the game has a combat XP
            int monsterXP = targetMonster.GetCombatXP();
            double diffDelta = (CharmStat - monsterXP) / (double)CharmStat;
            if (diffDelta < 0)
                 diffDelta = 0;

                double xpUpChance = 1 - diffDelta;
                int xpUpRoll = (int)Math.Floor(xpUpChance * 100.0);
                int xpUpRollActual = Game.Random.Next(100);
                LogFile.Log.LogEntryDebug("CharmXP up. Chance: " + xpUpRoll + " roll: " + xpUpRollActual, LogDebugLevel.Medium);

                if (xpUpRollActual < xpUpRoll)
                {
                    CharmXP++;
                    LogFile.Log.LogEntryDebug("CharmXP up!", LogDebugLevel.Medium);
                    Game.MessageQueue.AddMessage("You feel more charming.");
                }
            
        }

        internal void ResetTemporaryPlayerStats()
        {
            CurrentCharmedCreatures = 0;
        }

        /// <summary>
        /// Do setup just before the game starts. Dungeons etc. all ready to go.
        /// </summary>
        internal void StartGameSetup()
        {
            CalculateCombatStats();
        }

        /// <summary>
        /// Important to keep this the only place where the player injures themselves
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual CombatResults AttackPlayer(int damage)
        {
            //Do we hit the player?
            if (damage > 0)
            {
                int monsterOrigHP = Hitpoints;

                Hitpoints -= damage;

                //Is the player dead, if so kill it?
                if (Hitpoints <= 0)
                {

                    //Message queue string
                    string combatResultsMsg = "PvP Dam: " + damage + " HP: " + monsterOrigHP + "->" + Hitpoints + " killed";

                    //string playerMsg = "The " + this.SingleDescription + " hits you. You die.";
                    string playerMsg = "You knock yourself out!";
                    Game.MessageQueue.AddMessage(playerMsg);
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    Game.Dungeon.SetPlayerDeath("was knocked out by themselves");

                    return CombatResults.DefenderDied;
                }

                //Debug string
                string combatResultsMsg3 = "PvP Dam: " + damage + " HP: " + monsterOrigHP + "->" + Hitpoints + " injured";
                //string playerMsg3 = "The " + this.SingleDescription + " hits you.";
                Game.MessageQueue.AddMessage("You damage yourself.");
                LogFile.Log.LogEntryDebug(combatResultsMsg3, LogDebugLevel.Medium);

                return CombatResults.DefenderDamaged;
            }

            //Miss
            string combatResultsMsg2 = "PvP Dam: " + damage + " HP: " + Hitpoints + "->" + Hitpoints + " miss";
            //string playerMsg2 = "The " + this.SingleDescription + " misses you.";
            string playerMsg2 = "You don't damage yourself.";
            Game.MessageQueue.AddMessage(playerMsg2);
            LogFile.Log.LogEntryDebug(combatResultsMsg2, LogDebugLevel.Medium);

            return CombatResults.DefenderUnhurt;
        }

        /// <summary>
        /// Heal the player by a quantity. Won't exceed max HP.
        /// </summary>
        /// <param name="healingQuantity"></param>
        public void HealPlayer(int healingQuantity)
        {
            Hitpoints += healingQuantity;

            if (Hitpoints > MaxHitpoints)
                Hitpoints = MaxHitpoints;

        }
    }
}
