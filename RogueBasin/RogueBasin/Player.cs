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
        List<PlayerEffect> effects;

        public List<Monster> Kills { get; set;}

        public string Name { get; set; }

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
        /// Number of times we get knocked out
        /// </summary>
        public int NumDeaths { get; set; }

        public int MaxCharmedCreatures { get; set; }

        public int CurrentCharmedCreatures { get; set; }

        /// <summary>
        /// Combat stat calculated from training stat and items
        /// </summary>
        public int CharmPoints { get; set; }

        /// <summary>
        /// PrincessRL has a maximum no of equipped items when using EquipItemNoSlots
        /// </summary>
        public int MaximumEquippedItems { get; set; }

        public int CurrentEquippedItems { get; set; }

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
            effects = new List<PlayerEffect>();
            Kills = new List<Monster>();

            Level = 1;

            Representation = '@';

            MaxCharmedCreatures = 0;
            CurrentCharmedCreatures = 0;
            MaximumEquippedItems = 2;

            NumDeaths = 0;

            //Add default equipment slots
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Body));
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));

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
            SpeedStat = 10;
            AttackStat = 10;
            CharmStat = 2;
            MagicStat = 2;

            //Debug
            AttackStat = 1000;
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

        /// <summary>
        /// Calculate the player's combat stats based on level and equipment
        /// </summary>
        public void CalculateCombatStats()
        {

            
            /*            
                        armourClass = 12;
                                        damageBase = 4;
                                        damageModifier = 0;
                                        hitModifier = 0;
                                        maxHitpoints = 15;
                                        MaxCharmedCreatures = 1;
             */

            Inventory inv = Inventory;

            //Armour class
            ArmourClassAccess = 12;

            //Charm points
            CharmPoints = CharmStat;

            //Max charmed creatures
            if (inv.ContainsItem(new Items.SparklingEarrings()))
            {
                MaxCharmedCreatures = 2;
            }
            else
                MaxCharmedCreatures = 1;

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
            if (AttackStat > 100)
            {
                damageBase = 10;
            }
            else if (AttackStat > 60)
            {
                damageBase = 8;
            }
            else if (AttackStat > 30)
            {
                damageBase = 6;
            }
            else
                damageBase = 4;

            DamageBaseAccess = damageBase;
            Screen.Instance.PCColor = ColorPresets.White;

            //Consider equipped clothing items (only 1 will work)
            if (inv.ContainsItem(new Items.MetalArmour()))
            {
                ArmourClassAccess += 4;
                Screen.Instance.PCColor = ColorPresets.SteelBlue;
            }
            else if (inv.ContainsItem(new Items.LeatherArmour()))
            {
                ArmourClassAccess += 2;
                Screen.Instance.PCColor = ColorPresets.BurlyWood;
            }
            else if (inv.ContainsItem(new Items.PrettyDress()))
            {
                CharmPoints += 20;
                Screen.Instance.PCColor = ColorPresets.BlueViolet;
            }

            //Consider equipped weapons (only 1 will work)
            if (inv.ContainsItem(new Items.GodSword()))
            {
                DamageModifierAccess += 4;
            }
            else if (inv.ContainsItem(new Items.LongSword()))
            {
                DamageModifierAccess += 2;
            }
            else if (inv.ContainsItem(new Items.ShortSword()))
            {
                DamageModifierAccess += 1;
            }


            /*


            //Defaults (not necessary)
            armourClass = 10;
            damageBase = 4;
            damageModifier = 0;
            hitModifier = 0;

            //Check level
            switch (Game.Dungeon.Difficulty)
            {
                case GameDifficulty.Easy:

                    switch (Level)
                    {
                        case 1:
                            armourClass = 12;
                            damageBase = 4;
                            damageModifier = 0;
                            hitModifier = 0;
                            maxHitpoints = 15;
                            MaxCharmedCreatures = 1;
                            break;
                        case 2:
                            armourClass = 13;
                            damageBase = 4;
                            damageModifier = 1;
                            hitModifier = 1;
                            maxHitpoints = 25;
                            break;
                        case 3:
                            armourClass = 13;
                            damageBase = 6;
                            damageModifier = 0;
                            hitModifier = 2;
                            maxHitpoints = 35;
                            break;
                        case 4:
                            armourClass = 14;
                            damageBase = 6;
                            damageModifier = 1;
                            hitModifier = 3;
                            maxHitpoints = 45;
                            break;
                        case 5:
                            armourClass = 15;
                            damageBase = 8;
                            damageModifier = 0;
                            hitModifier = 4;
                            maxHitpoints = 55;
                            break;
                        case 6:
                            armourClass = 16;
                            damageBase = 8;
                            damageModifier = 1;
                            hitModifier = 5;
                            maxHitpoints = 65;
                            break;
                        case 7:
                        default:
                            armourClass = 17;
                            damageBase = 12;
                            damageModifier = 0;
                            hitModifier = 6;
                            maxHitpoints = 75;
                            break;
                    }
                    break;
                case GameDifficulty.Medium:

                    switch (Level)
                    {
                        case 1:
                            armourClass = 12;
                            damageBase = 4;
                            damageModifier = 0;
                            hitModifier = 0;
                            maxHitpoints = 15;
                            break;
                        case 2:
                            armourClass = 13;
                            damageBase = 4;
                            damageModifier = 1;
                            hitModifier = 1;
                            maxHitpoints = 25;
                            break;
                        case 3:
                            armourClass = 13;
                            damageBase = 6;
                            damageModifier = 0;
                            hitModifier = 2;
                            maxHitpoints = 35;
                            break;
                        case 4:
                            armourClass = 14;
                            damageBase = 6;
                            damageModifier = 1;
                            hitModifier = 3;
                            maxHitpoints = 45;
                            break;
                        case 5:
                            armourClass = 15;
                            damageBase = 8;
                            damageModifier = 0;
                            hitModifier = 4;
                            maxHitpoints = 55;
                            break;
                        case 6:
                            armourClass = 16;
                            damageBase = 8;
                            damageModifier = 1;
                            hitModifier = 5;
                            maxHitpoints = 65;
                            break;
                        case 7:
                        default:
                            armourClass = 17;
                            damageBase = 12;
                            damageModifier = 0;
                            hitModifier = 6;
                            maxHitpoints = 75;
                            break;
                    }
                    break;
                case GameDifficulty.Hard:

                    switch (Level)
                    {
                        case 1:
                            armourClass = 10;
                            damageBase = 4;
                            damageModifier = 0;
                            hitModifier = 0;
                            maxHitpoints = 10;
                            break;
                        case 2:
                            armourClass = 10;
                            damageBase = 4;
                            damageModifier = 1;
                            hitModifier = 1;
                            maxHitpoints = 15;
                            break;
                        case 3:
                            armourClass = 11;
                            damageBase = 6;
                            damageModifier = 0;
                            hitModifier = 2;
                            maxHitpoints = 25;
                            break;
                        case 4:
                            armourClass = 11;
                            damageBase = 6;
                            damageModifier = 1;
                            hitModifier = 3;
                            maxHitpoints = 30;
                            break;
                        case 5:
                            armourClass = 12;
                            damageBase = 8;
                            damageModifier = 0;
                            hitModifier = 4;
                            maxHitpoints = 35;
                            break;
                        case 6:
                            armourClass = 13;
                            damageBase = 8;
                            damageModifier = 1;
                            hitModifier = 5;
                            maxHitpoints = 40;
                            break;
                        case 7:
                        default:
                            armourClass = 14;
                            damageBase = 12;
                            damageModifier = 0;
                            hitModifier = 6;
                            maxHitpoints = 50;
                            break;
                    }
                    break;
            }

            //Set overdrive HP
            OverdriveHitpoints = (int)Math.Ceiling(maxHitpoints * 1.5);

            //Check equipped items
            
            foreach (Item item in Inventory.Items)
            {
                if (!item.IsEquipped)
                    continue;

                IEquippableItem equipItem = item as IEquippableItem;
                
                //Error if non-equippable item is equipped
                if(equipItem == null) {
                    LogFile.Log.LogEntry("Item " + item.SingleItemDescription + " is non-equippable but is equipped!");
                    //Just skip
                    continue;
                }

                armourClass += equipItem.ArmourClassModifier();
                damageModifier += equipItem.DamageModifier();
                hitModifier += equipItem.HitModifier();

                if (equipItem.DamageBase() > damageBase)
                {
                    damageBase = equipItem.DamageBase();
                }
            }
            */
            //Check effects

            bool toHitEffectOn = false;
            bool toDamageEffectOn = false;
            //bool speedEffectOn = false;

            foreach (PlayerEffect effect in effects)
            {

                armourClass += effect.ArmourClassModifier();

                if (effect.DamageModifier() > 0 && !toDamageEffectOn)
                {
                    damageModifier += effect.DamageModifier();
                    toDamageEffectOn = true;
                }

                if (effect.HitModifier() > 0 && !toHitEffectOn)
                {

                    hitModifier += effect.HitModifier();
                    toHitEffectOn = true;
                }

                if (effect.DamageBase() > damageBase)
                {
                    damageBase = effect.DamageBase();
                }
            }
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
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return armourClass;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return damageBase;
        }

        /// <summary>
        /// Creature damage modifier.  Set by type of creature.
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


        int toHitRoll; //just so we can use it in debug

        private int AttackWithModifiers(Monster monster, int hitMod, int damBase, int damMod, int ACmod)
        {
            int attackToHit = hitModifier + hitMod;
            int attackDamageMod = damageModifier + damMod;
            
            int attackDamageBase;

            if(damBase > damageBase)
                attackDamageBase = damBase;
            else
                attackDamageBase = damageBase;

            int monsterAC = monster.ArmourClass() + ACmod;
            toHitRoll = Utility.d20() + attackToHit;

            if (toHitRoll >= monsterAC)
            {
                //Hit - calculate damage
                int totalDamage = Utility.DamageRoll(attackDamageBase) + attackDamageMod;

                return totalDamage;
            }

            //Miss
            return 0;
        }

        public bool CastSpell(Spell toCast, Point target)
        {
            //Check MP

            //Actually cast the spell
            bool success = toCast.DoSpell(target);

            //Remove MP if successful

            return success;
        }

         /// <summary>
        /// Normal attack on a monster. Takes care of killing them off if required.
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        public CombatResults AttackMonster(Monster monster)
        {
            return AttackMonsterWithModifiers(monster, 0, 0, 0, 0);
        }

        /// <summary>
        /// Attack a monster with modifiers. Takes care of killing them off if required.
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        public CombatResults AttackMonsterWithModifiers(Monster monster, int hitModifierMod, int damageBaseMod, int damageModifierMod, int enemyACMod)
        {
            //Do we need to recalculate combat stats?
            if (this.RecalculateCombatStatsRequired)
                this.CalculateCombatStats();

            if (monster.RecalculateCombatStatsRequired)
                monster.CalculateCombatStats();

            //Set the attacked by marker
            monster.LastAttackedBy = this;

            //Was this a passive creature? It loses that flag
            if(monster.Passive)
                monster.UnpassifyCreature();

            //Calculate damage from a normal attack
            int damage = AttackWithModifiers(monster, hitModifierMod, damageBaseMod, damageModifierMod, enemyACMod);

            //Do we hit the monster?
            if (damage > 0)
            {
                int monsterOrigHP = monster.Hitpoints;

                monster.Hitpoints -= damage;

                //Fairly evil switch case for special attack types. Sorry, no time to do it well
                bool monsterDead = monster.Hitpoints <= 0;
                SpecialCombatEffectsOnMonster(monster, damage, monsterDead);

                //Evil: check to see if the monster is fleeing and if so, give it a chance not to
                MonsterFightAndRunAI aiMonster = monster as MonsterFightAndRunAI;

                if (aiMonster != null)
                {
                    aiMonster.RecoverOnBeingHit();
                }

                //Is the monster dead, if so kill it?
                if (monsterDead)
                {
                    Game.Dungeon.KillMonster(monster);

                    //Add it to our list of kills (simply adding the whole object here)
                    Kills.Add(monster);

                    //Debug string
                    string combatResultsMsg = "PvM ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " killed";
                    string playerMsg = "You knocked out the " + monster.SingleDescription + ".";
                    Game.MessageQueue.AddMessage(playerMsg);
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    return CombatResults.DefenderDied;
                }

                //Debug string
                string combatResultsMsg3 = "PvM ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " injured";
                string playerMsg2 = "You hit the " + monster.SingleDescription + ".";
                Game.MessageQueue.AddMessage(playerMsg2);
                LogFile.Log.LogEntryDebug(combatResultsMsg3, LogDebugLevel.Medium);

                return CombatResults.NeitherDied;
            }

            //Miss
            string combatResultsMsg2 = "PvM ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monster.Hitpoints + " miss";
            string playerMsg3 = "You missed the " + monster.SingleDescription + ".";
            Game.MessageQueue.AddMessage(playerMsg3);
            LogFile.Log.LogEntryDebug(combatResultsMsg2, LogDebugLevel.Medium);

            return CombatResults.NeitherDied;
        }

        /// <summary>
        /// List of special combat effects that might happen to a HIT monster
        /// </summary>
        /// <param name="monster"></param>
        private void SpecialCombatEffectsOnMonster(Monster monster, int damage, bool isDead)
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
                if (item as Items.Glove != null)
                {
                    glove = item as Items.Glove;
                    break;
                }
            }

            if (glove != null)
            {
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
                }
            }
        }

        /// <summary>
        /// Increase HP from leech attack up to overdrive limit
        /// </summary>
        /// <param name="numHP"></param>
        internal void GainHPFromLeech(int numHP)
        {
            hitpoints += numHP;

            if (hitpoints > OverdriveHitpoints)
                hitpoints = OverdriveHitpoints;

            LogFile.Log.LogEntryDebug("Gain " + numHP + " hp from leech.", LogDebugLevel.Medium);
        }

        /// <summary>
        /// Increment time on all player events. Events that expire will run their onExit() routines and then delete themselves from the list
        /// </summary>
        internal void IncrementEventTime()
        {
            //Increment time on events and remove finished ones
            List<PlayerEffect> finishedEffects = new List<PlayerEffect>();

            foreach (PlayerEffect effect in effects)
            {
                effect.IncrementTime();

                if (effect.HasEnded())
                {
                    finishedEffects.Add(effect);
                }
            }

            //Remove finished effects
            foreach (PlayerEffect effect in finishedEffects)
            {
                effects.Remove(effect);
            }
        }

        /// <summary>
        /// Increment time on all player events then use the base class to increment time on the player's turn counter
        /// </summary>
        /// <returns></returns>
        internal override bool IncrementTurnTime()
        {
            IncrementEventTime();

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

            effect.OnStart();

            //Check if it altered our combat stats
            //CalculateCombatStats();
            //Should be done in effect itself or optionally each time we attack
        }

        protected override char GetRepresentation()
        {
            return '@';
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
            Game.MessageQueue.AddMessage(itemToUse.SingleItemDescription + " equipped in " + StringEquivalent.EquipmentSlots[matchingEquipSlots[0].slotType]);

            return true;
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
                Inventory.RemoveItem(itemToUse);
                
                //This permanently deletes it from the game
                //Game.Dungeon.RemoveItem(itemToUse);

                //If the line above is commented, the item will be returned to town. Will want to un-use it in this case
                useableItem.UsedUp = false;
                
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
                Game.MessageQueue.AddMessage("You can't carry anymore items. Press 'd' to drop your current items.");
                LogFile.Log.LogEntryDebug("Max number of items reached", LogDebugLevel.Medium);

                return false;
            }
            else
            {
                //If not, and we're not in town, set it as inInventory so it won't be drawn. It'll get returned to town on when go back
                item.InInventory = true;

                //Play the video
                equipItem.Equip(this);

                Game.MessageQueue.AddMessage("You place the" + item.SingleItemDescription + " in your backpack.");
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

        internal void RemoveCharmedCreature()
        {
            CurrentCharmedCreatures--;

            if (CurrentCharmedCreatures < 0)
            {
                LogFile.Log.LogEntryDebug("tried to remove a charmed creature when there were 0", LogDebugLevel.High);
                CurrentCharmedCreatures = 0;
            }
        }
    }
}
