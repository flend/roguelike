﻿using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueBasin
{
    public enum PlayerClass
    {
        Athlete, Gunner, Sneaker
    }

    public class Player : Creature
    {
        /// <summary>
        /// Effects that are active on the player
        /// </summary>
        public List<PlayerEffect> effects { get; private set; }

        public List<Monster> Kills { get; set;}

        private IEnumerable<Monster> monstersInLastFoV = Enumerable.Empty<Monster>();

        public int KillCount = 0;

        public string Name { get; set; }

        public bool ItemHelpMovieSeen = false;
        public bool TempItemHelpMovieSeen = false;

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

        public int Shield { get; set; }

        public int MaxShield { get; set; }
        
        public int Energy { get; set; }

        public int MaxEnergy { get; set; }

        public bool ShieldWasDamagedThisTurn { get; set; }

        public bool HitpointsWasDamagedThisTurn { get; set; }

        public bool EnergyWasDamagedThisTurn { get; set; }

        public bool ShieldIsDisabled { get; private set; }

        public bool EnergyRechargeIsDisabled { get; private set; }

        public bool DoesShieldRecharge { get; private set; }

        public bool DoHitpointsRecharge { get; private set; }

        private int TurnsSinceShieldDisabled { get; set; }

        private int TurnsSinceEnergyRechargeDisabled { get; set; }

        private const int TurnsForShieldToTurnBackOn = 20;

        private const int TurnsForEnergyRechargeToTurnBackOn = 5;

        private const int TurnsToRegenerateShield = 20;

        private const int TurnsToRegenerateHP = 20;

        private const int TurnsToRegenerateEnergy = 10;

        private Dictionary<Item, int> wetwareDisabledTurns = new Dictionary<Item, int>();

        //4 ticks per turn currently
        private const int turnsToDisableStealthWareAfterAttack = 80;
        private const int turnsToDisableBoostWareAfterAttack = 80;

        private const int turnsToDisableStealthWareAfterUnequip = 20;
        private const int turnsToDisableBoostWareAfterUnequip = 20;

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

        public PlayerClass PlayerClass { get; private set; }

        /// <summary>
        /// Is the player currently in the running state (changes interactions)
        /// </summary>
        public bool Running { get; set; }

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

        private int UtilityInventoryPosition
        {
            get;
            set;
        }

        private int WetwareInventoryPosition
        {
            get;
            set;
        }

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
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Melee));
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Wetware));

            //Set initial HP
            SetupInitialStats();

            SightRadius = 0;

            TurnCount = 0;
        }

        private void SetupInitialStats()
        {
            //CalculateCombatStats();
            maxHitpoints = 50;
            hitpoints = maxHitpoints;

            MaxShield = 100;
            Shield = MaxShield;

            MaxEnergy = 200;
            Energy = MaxEnergy;

            DoesShieldRecharge = true;
            DoHitpointsRecharge = false;
        }

        public void SetPlayerClass(PlayerClass thisClass)
        {
            PlayerClass = thisClass;

            switch (PlayerClass)
            {
                case RogueBasin.PlayerClass.Athlete:
                    GameSprite = "lance";
                    //Perma dodge
                    AddEffect(new PlayerEffects.Dodge());
                    break;
                case RogueBasin.PlayerClass.Gunner:
                    GameSprite = "crack";
                    //Perma aim
                    AddEffect(new PlayerEffects.Aim());
                    break;
                case RogueBasin.PlayerClass.Sneaker:
                    GameSprite = "nerd";
                    GiveItemNotFromDungeonIfTypeNotInInventory(new Items.StealthWare());
                    break;
            }
        }

        internal bool IsWeaponTypeAvailable(Type weaponType)
        {
            return IsInventoryTypeAvailable(weaponType);
        }

        internal bool IsWetwareTypeAvailable(Type wetWareType)
        {

            return IsInventoryTypeAvailable(wetWareType);
        }

        internal bool IsInventoryTypeAvailable(Type wetWareType)
        {
            var wetwareInInventory = Inventory.GetItemsOfType(wetWareType);

            if (wetwareInInventory.Count() == 0)
            {
                return false;
            }

            return true;
        }

        internal bool IsWetwareTypeDisabled(Type wetwareType)
        {
            var wetwareInInventory = Inventory.GetItemsOfType(wetwareType);

            if (wetwareInInventory.Count() == 0)
            {
                return false;
            }

            var thisWetware = wetwareInInventory.First();

            if (wetwareDisabledTurns.ContainsKey(thisWetware) && wetwareDisabledTurns[thisWetware] > 0)
            {
                return true;
            }
            return false;
        }


        public void DisableWetware(Type wetwareToDisable, int turnsToDisable)
        {
            var wetwareInInventory = Inventory.GetItemsOfType(wetwareToDisable);

            if (wetwareInInventory.Count() == 0)
            {
                LogFile.Log.LogEntryDebug("Can't disable wetware " + wetwareToDisable + " not in inventory", LogDebugLevel.Medium);
                return;
            }

            var thisWetware = wetwareInInventory.First();

            var currentlyDisableTurns = 0;

            if (wetwareDisabledTurns.ContainsKey(thisWetware))
                currentlyDisableTurns = wetwareDisabledTurns[thisWetware];

            if (currentlyDisableTurns > turnsToDisable)
                return;

            wetwareDisabledTurns[thisWetware] = turnsToDisable;
        }

        /// <summary>
        /// Remove all our items and reset our equipped items count
        /// </summary>
        public void RemoveAllItems()
        {
            Inventory.RemoveAllItems();
            CurrentEquippedItems = 0;
        }


        public int GetNoItemsOfSameType(Item itemType)
        {
            if (itemType == null)
                return 0;

            return Inventory.Items.Where(i => i.GetType() == itemType.GetType()).Count();
        }

        public void EquipNextUtilityInventoryItem(int inventoryPositionChange)
        {
            var allUtilityItems = Inventory.Items.Where(i => (i as IEquippableItem) != null && (i as IEquippableItem).EquipmentSlots.Contains(EquipmentSlot.Utility));
            var orderedUtilityItemsTypes = allUtilityItems.DistinctBy(i => i.SingleItemDescription).OrderBy(i => i.SingleItemDescription);
            
            if (orderedUtilityItemsTypes.Count() == 0)
            {
                UtilityInventoryPosition = 0;
                LogFile.Log.LogEntryDebug("No next utility item to equip", LogDebugLevel.Medium);
                return;
            }

            if (inventoryPositionChange > 0)
                UtilityInventoryPosition++;
            else if(inventoryPositionChange < 0)
                UtilityInventoryPosition--;

            if (UtilityInventoryPosition >= orderedUtilityItemsTypes.Count())
            {
                UtilityInventoryPosition = 0;
            }
            if (UtilityInventoryPosition < 0)
            {
                UtilityInventoryPosition = orderedUtilityItemsTypes.Count() - 1;
            }

            EquipAndReplaceItem(orderedUtilityItemsTypes.ElementAt(UtilityInventoryPosition));
        }

        public void SelectNextWetware()
        {
            SelectNextWetwareInventoryItem(0);
        }

        public void SelectNextWetwareInventoryItem(int inventoryPositionChange)
        {
            var allUtilityItems = Inventory.Items.Where(i => (i as IEquippableItem) != null && (i as IEquippableItem).EquipmentSlots.Contains(EquipmentSlot.Wetware));
            var orderedUtilityItemsTypes = allUtilityItems.DistinctBy(i => i.SingleItemDescription).OrderBy(i => i.SingleItemDescription);

            var originalSelectedWetware = GetSelectedWetware();

            if (orderedUtilityItemsTypes.Count() == 0)
            {
                WetwareInventoryPosition = 0;
                LogFile.Log.LogEntryDebug("No next wetware item to equip", LogDebugLevel.Medium);
                return;
            }

            if (inventoryPositionChange > 0)
                WetwareInventoryPosition++;
            else if (inventoryPositionChange < 0)
                WetwareInventoryPosition--;

            if (WetwareInventoryPosition >= orderedUtilityItemsTypes.Count())
            {
                WetwareInventoryPosition = 0;
            }
            if (WetwareInventoryPosition < 0)
            {
                WetwareInventoryPosition = orderedUtilityItemsTypes.Count() - 1;
            }

            if (originalSelectedWetware != GetSelectedWetware())
            {
                //Changing the selected wetware while it is active will unequip
                UnequipWetware();
            }
        }

        public Item GetSelectedWetware()
        {
            var allUtilityItems = Inventory.Items.Where(i => (i as IEquippableItem) != null && (i as IEquippableItem).EquipmentSlots.Contains(EquipmentSlot.Wetware));
            var orderedUtilityItemsTypes = allUtilityItems.DistinctBy(i => i.SingleItemDescription).OrderBy(i => i.SingleItemDescription);

            if (orderedUtilityItemsTypes.Count() == 0)
            {
                return null;
            }

            if (WetwareInventoryPosition >= orderedUtilityItemsTypes.Count())
            {
                LogFile.Log.LogEntryDebug("Wetware inventory position " + WetwareInventoryPosition + " higher than item count", LogDebugLevel.Medium);
                return null;
            }

            return orderedUtilityItemsTypes.ElementAt(WetwareInventoryPosition);
        }

        public int GetTurnsDisabledForSelectedWetware()
        {
            var selectedWetware = GetSelectedWetware();

            if (GetSelectedWetware() == null)
            {
                return 0;
            }
            else
            {
                return GetDisabledTurnsForWetware(selectedWetware);
            }

        }

        private int GetDisabledTurnsForWetware(Item wetware) {
            if (wetwareDisabledTurns.ContainsKey(wetware))
            {
                return wetwareDisabledTurns[wetware];
            }
            return 0;
        }
         
        public void EquipSelectedWetware()
        {
            var selectedWetware = GetSelectedWetware();

            if (selectedWetware != null)
            {
                ToggleEquipWetware(selectedWetware.GetType());
            }
            else 
            {
                LogFile.Log.LogEntryDebug("No selected wetware to equip", LogDebugLevel.Medium);
            }
        }

        public void EquipNextUtility()
        {
            EquipNextUtilityInventoryItem(0);
        }

        public IEnumerable<Monster> NewMonstersinFoV()
        {
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(this);
            var monstersInFoV = Game.Dungeon.FindAllHostileCreaturesInFoV(currentFOV);
            var difference = monstersInFoV.Except(monstersInLastFoV);
            monstersInLastFoV = monstersInFoV;
            return difference;
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

            //NormalSightRadius = 0;

            /*
            if(inv.ContainsItem(new Items.Lantern()))
                NormalSightRadius = 7;
            */
            //Speed

            //int speedDelta = SpeedStat - 10;

            //speedDelta = speedDelta * 2;

            Speed = 100;// +speedDelta;

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

            Screen.Instance.PCColor = System.Drawing.Color.White;
            /*
            if (inv.ContainsItem(new Items.MetalArmour()) && AttackStat > 50)
            {
                ArmourClassAccess += 6;
                Screen.Instance.PCColor = System.Drawing.Color.SteelBlue;
            }
            else if (inv.ContainsItem(new Items.LeatherArmour()) && AttackStat > 25)
            {
                ArmourClassAccess += 3;
                Screen.Instance.PCColor = System.Drawing.Color.BurlyWood;
            }
            else if (inv.ContainsItem(new Items.KnockoutDress()))
            {
                CharmPoints += 40;
                ArmourClassAccess += 3;
                Screen.Instance.PCColor = System.Drawing.Color.Yellow;
            }
            else if (inv.ContainsItem(new Items.PrettyDress()))
            {
                CharmPoints += 20;
                ArmourClassAccess += 1;
                Screen.Instance.PCColor = System.Drawing.Color.BlueViolet;
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

            //CalculateSightRadius();
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
                    maxDamage = (int)effect.DamageModifier();

                if(effect.DamageModifier() < minDamage)
                    minDamage = (int)effect.DamageModifier();

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
        public override double DamageModifier()
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

            if (toCast.NeedsTarget() && Utility.GetDistanceBetween(LocationMap, target) > range)
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

        public double CalculateAimBonus()
        {
            /*
            var aimBonus = 0.1;

            var aimEffect = GetActiveEffects(typeof(PlayerEffects.AimEnhance));

            if(aimEffect.Count() > 0)
            {
                aimBonus = ((PlayerEffects.AimEnhance)aimEffect.First()).aimEnhanceAmount * 0.3;
            }

            var stationaryBonus = Math.Min(TurnsInactive, 3) * aimBonus;

            var nonFireBonus = Math.Min(TurnsSinceAction, 3) * aimBonus / 2;

            return stationaryBonus + nonFireBonus;
             * */

            if (IsAimActive())
            {
                return 0.5;
            }

            return 0.0;
        }

        public double CalculateRangedAttackModifiersOnMonster(Monster target)
        {
            var damageModifier = 1.0;

            //Aiming
            damageModifier += CalculateAimBonus();

            //Enemy moving
            /*if (target != null && target.TurnsMoving > 0)
            {
                damageModifier -= 0.2;
            }*/

            return damageModifier;
        }

        public double CalculateMeleeAttackModifiersOnMonster(Monster target)
        {
            var meleeEffect = GetActiveEffects(typeof(PlayerEffects.SpeedBoost));

            var meleeMultiplier = 1.0;

            if (meleeEffect.Count() > 0)
            {
                meleeMultiplier = ((PlayerEffects.SpeedBoost)meleeEffect.First()).Level * 0.5 + 1;
            }

            return meleeMultiplier;
        }

        public double CalculateDamageModifierForAttacksOnPlayer(Monster target, bool ranged)
        {
            var damageModifier = 1.0;
            /*
            var speedEffect = GetActiveEffects(typeof(PlayerEffects.SpeedBoost));

            var speedModifier = 1.0;

            if (speedEffect.Count() > 0)
            {
                speedModifier += ((PlayerEffects.SpeedBoost)speedEffect.First()).Level;
            }*/

            if (IsDodgeActive() && ranged)
            {
                //Straight 50% damage reduction for moving
                damageModifier -= 0.5;
            }

            if (IsAimActive() && ranged)
            {
                //Straight 50% damage reduction for not moving
                damageModifier -= 0.5;
            }

            if (target != null && ranged)
            {
                //Test cover
                var coverItems = GetPlayerCover(target);
                var hardCover = coverItems.Item1;
                var softCover = coverItems.Item2;

                if (hardCover > 0)
                    damageModifier -= 0.5;
                if (softCover > 0)
                    damageModifier -= 0.25;
            }

            return Math.Max(0.1, damageModifier);
        }

        public Tuple<int, int> GetPlayerCover()
        {
            var nearestMonster = Game.Dungeon.FindClosestHostileCreatureInFOV(this) as Monster;
            if (nearestMonster == null)
                return new Tuple<int, int>(0, 0);

            return GetPlayerCover(nearestMonster);
        }

        public Tuple<int, int> GetPlayerCover(Monster target)
        {
            if (target == null)
                return new Tuple<int, int>(0, 0);

            var coverItems = Game.Dungeon.GetNumberOfCoverItemsBetweenPoints(target.LocationLevel, target.LocationMap, LocationLevel, LocationMap);
            return coverItems;
        }

        public void CancelStealthDueToAttack()
        {
            if (Game.Dungeon.PlayerCheating)
                return;

            //Forceably unequip any StealthWare and disable for some time
            if (IsWetwareTypeEquipped(typeof(Items.StealthWare)))
            {
                UnequipWetware();
                DisableWetware(typeof(Items.StealthWare), turnsToDisableStealthWareAfterAttack);
            }
        }

        public void CancelBoostDueToAttack()
        {
            if (Game.Dungeon.PlayerCheating)
                return;

            //Forceably unequip any SpeedWare and disable for some time
            if (IsWetwareTypeEquipped(typeof(Items.BoostWare)))
            {
                UnequipWetware();
                DisableWetware(typeof(Items.BoostWare), turnsToDisableBoostWareAfterAttack);
            }
        }

        public void CancelStealthDueToUnequip()
        {

            DisableWetware(typeof(Items.StealthWare), turnsToDisableStealthWareAfterUnequip);
        }

        public void CancelBoostDueToUnequip()
        {

            DisableWetware(typeof(Items.BoostWare), turnsToDisableBoostWareAfterUnequip);

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

            DecreaseWetwareDisabledCounts();

            return base.IncrementTurnTime();
        }

        private void DecreaseWetwareDisabledCounts()
        {
            //yuck
            var allKeys = wetwareDisabledTurns.Keys.ToList();
            for (int i = 0; i < allKeys.Count; i++)
            {
                wetwareDisabledTurns[allKeys[i]] = Math.Max(wetwareDisabledTurns[allKeys[i]] - 1, 0);
            }
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

        /// <summary>
        /// Is this class of effect currently active?
        /// Refactor to take a Type not an object
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public IEnumerable<PlayerEffect> GetActiveEffects(Type effectType)
        {
            return effects.FindAll(x => x.GetType() == effectType);
        }

        protected override char GetRepresentation()
        {
            /*
            var weapon = GetEquippedRangedWeapon();

            if (weapon != null)
            {
                if (weapon.GetType() == typeof(Items.Fists))
                    return (char)257;

                if (weapon.GetType() == typeof(Items.Pistol))
                    return (char)513;

                if (weapon.GetType() == typeof(Items.HeavyPistol))
                    return (char)512;

                if (weapon.GetType() == typeof(Items.Shotgun))
                    return (char)514;

                if (weapon.GetType() == typeof(Items.AssaultRifle))
                    return (char)515;

                if (weapon.GetType() == typeof(Items.HeavyShotgun))
                    return (char)516;

                if (weapon.GetType() == typeof(Items.Laser))
                    return (char)517;

                if (weapon.GetType() == typeof(Items.HeavyLaser))
                    return (char)517;

                if (weapon.GetType() == typeof(Items.Vibroblade))
                    return (char)518;

                if (weapon.GetType() == typeof(Items.FragGrenade))
                    return (char)521;

                if (weapon.GetType() == typeof(Items.StunGrenade))
                    return (char)522;

                if (weapon.GetType() == typeof(Items.SoundGrenade))
                    return (char)520;
            }
            return (char)257;*/

            return '@';
        }

        public bool ToggleEquipWetware(Type wetwareTypeToEquip)
        {
            if(!IsWetwareTypeAvailable(wetwareTypeToEquip)) {
                LogFile.Log.LogEntryDebug("Do not have wetware of type: " + wetwareTypeToEquip.ToString(), LogDebugLevel.Medium);
                return false;
            }

            var justUnequip = false;
            var currentlyEquippedWetware = GetEquippedWetware();
            if (currentlyEquippedWetware != null && currentlyEquippedWetware.GetType() == wetwareTypeToEquip)
            {
                justUnequip = true;
            }

            UnequipWetware();

            if (justUnequip)
                return true;

            var equipTime = EquipWetware(wetwareTypeToEquip);
            return equipTime;
        }

        internal bool EquipWetware(Type wetwareTypeToEquip)
        {
            //Check if we have this item
            var wetwareOfTypeInInventory = Inventory.GetItemsOfType(wetwareTypeToEquip);

            if (wetwareOfTypeInInventory.Count == 0)
            {
                LogFile.Log.LogEntryDebug("Do not have wetware of type: " + wetwareTypeToEquip.ToString(), LogDebugLevel.Medium);
                return false;
            }

            Item wetwareToEquip = wetwareOfTypeInInventory[0];
            IEnumerable<Item> wetwareToFind;

            if (wetwareTypeToEquip == typeof(Items.ShieldWare))
            {
                wetwareToFind = wetwareOfTypeInInventory.Cast<Items.ShieldWare>().Where(s => s.level == 3);
                if (!wetwareToFind.Any())
                    wetwareToFind = wetwareOfTypeInInventory.Cast<Items.ShieldWare>().Where(s => s.level == 2);
                if (!wetwareToFind.Any())
                    wetwareToFind = wetwareOfTypeInInventory.Cast<Items.ShieldWare>().Where(s => s.level == 1);

                wetwareToEquip = wetwareToFind.First();
            }

            if (wetwareTypeToEquip == typeof(Items.BoostWare))
            {
                wetwareToFind = wetwareOfTypeInInventory.Cast<Items.BoostWare>().Where(s => s.level == 3);
                if (!wetwareToFind.Any())
                    wetwareToFind = wetwareOfTypeInInventory.Cast<Items.BoostWare>().Where(s => s.level == 2);
                if (!wetwareToFind.Any())
                    wetwareToFind = wetwareOfTypeInInventory.Cast<Items.BoostWare>().Where(s => s.level == 1);

                wetwareToEquip = wetwareToFind.First();
            }

            if (wetwareTypeToEquip == typeof(Items.AimWare))
            {
                wetwareToFind = wetwareOfTypeInInventory.Cast<Items.AimWare>().Where(s => s.level == 3);
                if (!wetwareToFind.Any())
                    wetwareToFind = wetwareOfTypeInInventory.Cast<Items.AimWare>().Where(s => s.level == 2);
                if (!wetwareToFind.Any())
                    wetwareToFind = wetwareOfTypeInInventory.Cast<Items.AimWare>().Where(s => s.level == 1);

                wetwareToEquip = wetwareToFind.First();
            }

            //Check if it is disabled

            var disabledTurnsForWetware = GetDisabledTurnsForWetware(wetwareToEquip);
            if (disabledTurnsForWetware > 0)
            {
                Game.MessageQueue.AddMessage("Can't enable wetware - it's disabled for " + disabledTurnsForWetware + " turns");
                LogFile.Log.LogEntryDebug("Can't enable wetware, is disabled for " + disabledTurnsForWetware + " turns", LogDebugLevel.Medium);
                return false;
            }

            //Equip the new wetware
            EquipmentSlotInfo wetwareSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Wetware);

            if (wetwareSlot == null)
            {
                LogFile.Log.LogEntryDebug("Can't find wetware slot - bug ", LogDebugLevel.High);
                return false;
            }
            
            var wetwareToEquipAsEquippable = wetwareToEquip as IEquippableItem;
            wetwareToEquip.IsEquipped = true;
            wetwareSlot.equippedItem = wetwareToEquip;
            wetwareToEquipAsEquippable.Equip(this);

            return true;
        }

        private void UnequipWetware()
        {
            var currentlyEquippedWetware = GetEquippedWetware();
            if (currentlyEquippedWetware != null)
            {
                var currentlyEquippedWetwareItem = currentlyEquippedWetware as Item;
                currentlyEquippedWetware.UnEquip(this);
                currentlyEquippedWetwareItem.IsEquipped = false;

                EquipmentSlotInfo wetwareSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Wetware);

                if (wetwareSlot == null)
                {
                    LogFile.Log.LogEntryDebug("Can't find wetware slot - bug ", LogDebugLevel.High);
                    return;
                }

                wetwareSlot.equippedItem = null;

            }

            DisableEnergyRecharge();

            if (currentlyEquippedWetware is Items.StealthWare)
                CancelStealthDueToUnequip();

            if (currentlyEquippedWetware is Items.BoostWare)
                CancelBoostDueToUnequip();

            CalculateCombatStats();
        }


        /// <summary>
        /// Drop an item at a specific point. Equippable items never exist in the inventory in FlatlineRL
        /// </summary>
        /// <param name="itemToDrop"></param>
        /// <returns></returns>
        public bool DropEquippableItem(Item itemToDrop, int levelToDropAt, Point locToDropAt)
        {
            //Remove from inventory
            Inventory.RemoveItem(itemToDrop);

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
            return DropEquippableItem(itemToDrop, this.LocationLevel, this.LocationMap);
        }



        internal Type HeavyWeaponTranslation(Type itemType)
        {

            //Do weapon translations
            if (itemType == typeof(Items.Pistol) && IsInventoryTypeAvailable(typeof(Items.HeavyPistol)))
                return typeof(Items.HeavyPistol);

            if (itemType == typeof(Items.Shotgun) && IsInventoryTypeAvailable(typeof(Items.HeavyShotgun)))
                return typeof(Items.HeavyShotgun);

            if (itemType == typeof(Items.Laser) && IsInventoryTypeAvailable(typeof(Items.HeavyLaser)))
                return typeof(Items.HeavyLaser);

            if (itemType == typeof(Items.Fists) && IsInventoryTypeAvailable(typeof(Items.Vibroblade)))
                return typeof(Items.Vibroblade);

            return itemType;
        }

        internal bool EquipInventoryItemType(Type itemType, bool reequip=false)
        {
            itemType = HeavyWeaponTranslation(itemType);

            var invAvailable = IsInventoryTypeAvailable(itemType);
            if (!invAvailable)
            {
                LogFile.Log.LogEntryDebug("Can't equip inventory type " + itemType + " - not in inventory", LogDebugLevel.Medium);

            }

            var equipSuccess = false;
            if(invAvailable)
                equipSuccess = EquipAndReplaceItem(Inventory.GetItemsOfType(itemType).First());

            if (equipSuccess == false && reequip == false)
            {
                //Try to reequip melee
                //EquipBestMeleeWeapon();
            }
            return false;
        }
        
        public virtual bool PickUpItem(Item itemToPickUp)
        {
            bool pickedUp = base.PickUpItem(itemToPickUp);

            if (pickedUp && AutoequipItem(itemToPickUp))
            {
                EquipAndReplaceItem(itemToPickUp);
            }

            return true;
        }

        private bool AutoequipItem(Item itemToPickUp)
        {
            if (itemToPickUp is Items.Fists)
                return true;

            if (itemToPickUp is Items.Vibroblade)
                return true;

            if (itemToPickUp is Items.Pistol)
                return true;

            if (itemToPickUp is Items.Shotgun)
                return true;

            if (itemToPickUp is Items.AssaultRifle)
                return true;

            if (itemToPickUp is Items.Laser)
                return true;

            if (itemToPickUp is Items.HeavyPistol)
                return true;

            if (itemToPickUp is Items.HeavyShotgun)
                return true;

            if (itemToPickUp is Items.HeavyLaser)
                return true;

            //Just equip everything
            return false;
        }

        /// <summary>
        /// Equip an item into a relevant slot.
        /// Will unequip and drop an item in the same slot.
        /// Returns true if operation successful
        /// Should be called after picking item up
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
            
            //Check item is in inventory
            if (!Inventory.ContainsItem(itemToUse))
            {
                LogFile.Log.LogEntryDebug("Can't equip item, not in inventory: " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);
                return false;
            };

            //Find all matching slots available on the player

            List<EquipmentSlot> itemPossibleSlots = equippableItem.EquipmentSlots;
            
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

                //We destroy obselete ware
                if (IsObselete(oldItem))
                {
                    LogFile.Log.LogEntryDebug("Item discarded: " + oldItem.SingleItemDescription, LogDebugLevel.Medium);
                    //Game.MessageQueue.AddMessage("Discarding obselete " + oldItem.SingleItemDescription + ".");

                    UnequipAndDestroyItem(oldItem);
                }
                else
                {
                    UnequipItem(oldItem);
                }
                    
                //This slot is now free
                freeSlot = matchingEquipSlots[0];
            }

            //We now have a free slot to equip in

            //Put new item in first relevant slot and run equipping routine
            matchingEquipSlots[0].equippedItem = itemToUse;
            equippableItem.Equip(this);
            itemToUse.IsEquipped = true;

            LogFile.Log.LogEntryDebug("Equipping new item " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);

            //Message the user
            LogFile.Log.LogEntryDebug("Item equipped: " + itemToUse.SingleItemDescription, LogDebugLevel.Low);
            Game.MessageQueue.AddMessage(itemToUse.SingleItemDescription + " equipped.");
            
            //Implies that this would be better wrapped into a PlayerAction - leaving this code to handle player state only
            Game.Base.InputHandler.UpdateMapTargetting();

            return true;
        }

        private bool IsObselete(Item oldItem)
        {
            //Pistol is never dropped
            /* 
           if (oldItem is Items.Pistol)
               return true;

           if (oldItem is Items.Fists)
               return true;

           if (oldItem is Items.Pistol && IsWeaponTypeAvailable(typeof(Items.HeavyPistol)))
               return true;

           if (oldItem is Items.Shotgun && IsWeaponTypeAvailable(typeof(Items.HeavyShotgun)))
               return true;

           if (oldItem is Items.Laser && IsWeaponTypeAvailable(typeof(Items.HeavyLaser)))
               return true;*/
            return false;
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
            UnequipItem(item);

            //Drop the old item
            DropEquippableItem(item, levelToDrop, toDropLoc);
        }

        private void UnequipItem(Item item)
        {
            //Run unequip routine
            IEquippableItem equipItem = item as IEquippableItem;

            if (equipItem == null)
            {
                LogFile.Log.LogEntryDebug("UnequipItem - item not equippable " + item.SingleItemDescription, LogDebugLevel.High);
                return;
            }

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
            Inventory.RemoveItemAndDestroy(item);
            //There should now be no references to it

        }

        /// <summary>
        /// TraumaRL - return equipped wetware or null
        /// </summary>
        /// <returns></returns>
        public IEquippableItem GetEquippedWetware()
        {
            return GetEquippedWetwareAsItem() as IEquippableItem;
        }

        public Item GetEquippedWetwareAsItem()
        {
            EquipmentSlotInfo weaponSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Wetware);

            if (weaponSlot == null)
            {
                LogFile.Log.LogEntryDebug("Can't find wetware slot - bug ", LogDebugLevel.High);
                return null;
            }

            return weaponSlot.equippedItem;
        }

        public bool IsWetwareTypeEquipped(Type wetwareType)
        {
            var equippedWetware = GetEquippedWetware();

            if (equippedWetware != null && equippedWetware.GetType() == wetwareType)
                return true;

            return false;
        }

        public IEquippableItem GetEquippedRangedWeapon() 
        {
            return GetEquippedRangedWeaponAsItem() as IEquippableItem;
        }

        public IEquippableItem GetEquippedMeleeWeapon()
        {
            return GetEquippedMeleeWeaponAsItem() as IEquippableItem;
        }

        public bool HasMeleeWeaponEquipped()
        {
            var currentWeapon = GetEquippedRangedWeapon();

            if (currentWeapon == null)
                return false;

            if (currentWeapon.GetType() == typeof(Items.Fists) || currentWeapon.GetType() == typeof(Items.Vibroblade))
                return true;

            return false;
        }

        public bool HasThrownWeaponEquipped()
        {
            var currentWeapon = GetEquippedRangedWeapon();

            if (currentWeapon == null)
                return false;

            if (currentWeapon.GetType() == typeof(Items.FragGrenade) || currentWeapon.GetType() == typeof(Items.StunGrenade) || currentWeapon.GetType() == typeof(Items.SoundGrenade))
                return true;

            return false;
        }


        public Item GetEquippedRangedWeaponAsItem()
        {
            EquipmentSlotInfo weaponSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Weapon);

            if(weaponSlot == null) {
                LogFile.Log.LogEntryDebug("Can't find weapon slot - bug ", LogDebugLevel.High);
                return null;
            }

            return weaponSlot.equippedItem;
        }

        public IEnumerable<Item> GetRangedWeaponsOrdered()
        {
            return Inventory.Items.Where(i => i is RangedWeapon).OrderBy(i => (i as RangedWeapon).Index());
        }

        public Item GetEquippedMeleeWeaponAsItem()
        {
            EquipmentSlotInfo weaponSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Melee);

            if (weaponSlot == null)
            {
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

        public void GiveAllWetware(int level)
        {
            if (level == 3)
            {
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.StealthWare());
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.ShieldWare(3));
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.AimWare(3));
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.BoostWare(3));
            }

            if (level == 2)
            {
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.StealthWare());
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.ShieldWare(2));
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.AimWare(2));
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.BoostWare(2));
            }

            if (level == 1)
            {
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.StealthWare());
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.ShieldWare(1));
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.AimWare(1));
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.BoostWare(1));
            }

            GiveItemNotFromDungeonIfTypeNotInInventory(new Items.BioWare());
        }

        public void GiveItemNotFromDungeonIfTypeNotInInventory(Item item)
        {
            if (!Inventory.ContainsItemOfType(item.GetType()))
            {
                Inventory.AddItemNotFromDungeon(item);
            }
        }

        public void GiveAllWeapons(int level)
        {
            if (level == 1)
            {
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.Vibroblade());
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.AssaultRifle());
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.Pistol());
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.Shotgun());
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.Laser());

                EquipInventoryItemType(typeof(Items.Vibroblade));
                
            }

            if (level == 2)
            {
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.HeavyPistol());
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.HeavyShotgun());
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.HeavyLaser());
            }

            for (int i = 0; i < 5; i++)
            {
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.FragGrenade());
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.StunGrenade());
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.SoundGrenade());
                GiveItemNotFromDungeonIfTypeNotInInventory(new Items.NanoRepair());
            }
                        
        }
        public void EquipStartupWeapons() {

            //Melee - Start with fists equipped
            GiveItemNotFromDungeonIfTypeNotInInventory(new Items.Fists());
            Game.Dungeon.Player.EquipInventoryItemType(typeof(Items.Fists));

            //Ranged - Start with pistol equipped
            GiveItemNotFromDungeonIfTypeNotInInventory(new Items.Pistol());
            Game.Dungeon.Player.EquipInventoryItemType(typeof(Items.Pistol));
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
        /// Level up the player!
        /// </summary>
        internal void LevelUp()
        {
            //Level up!
            Level++;

            //int lastMaxHP = maxHitpoints;

            //Recalculate combat stats
            CalculateCombatStats();

            //hitpoints += maxHitpoints - lastMaxHP;

            //Calculate HP etc
            HPOnLevelUP();

            LogFile.Log.LogEntry("Player levels up to: " + Level);
        }

        /// <summary>
        /// Apply level up effect to current hitpoints
        /// </summary>
        private void HPOnLevelUP()
        {
            MaxHitpoints = Game.Dungeon.LevelScalingCalculation(100, Level);
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
        public void StartGameSetup()
        {
            CalculateCombatStats();

            //keep this
            EquipStartupWeapons();
        }

        /*
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

                //bypasses cover etc.
                //var modifiedDamaged = (int)Math.Floor(CalculateDamageModifierForAttacksOnPlayer(this) * damage);

                ApplyDamageToPlayer(damage);

                //Hitpoints -= damage;

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
        }*/

        public IEquippableItem GetBestMeleeWeapon()
        {
            Type bestMeleeType = HeavyWeaponTranslation(typeof(Items.Fists));

            var meleeWeapon = Inventory.GetItemsOfType(bestMeleeType).First() as IEquippableItem;

            return meleeWeapon;
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

        internal bool isStealthed()
        {
            if (IsEffectActive(typeof(PlayerEffects.StealthBoost)) || IsEffectActive(typeof(PlayerEffects.StealthField)))
                return true;

            return false;
        }

        internal void RemoveEffect(Type effectType)
        {

            //Increment time on events and remove finished ones
            List<PlayerEffect> finishedEffects = effects.FindAll(x => x.GetType() == effectType);

            //Remove these effects
            
            foreach (PlayerEffect effect in finishedEffects)
            {
                if(!effect.HasEnded())
                    effect.OnEnd(this);

                effects.Remove(effect);
            }

        }

        /// <summary>
        /// Generic throw method for most normal items
        /// </summary>
        /// <param name="item"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public Point ThrowItemGeneric(IEquippableItem item, Point target, int damageOrStunTurns, bool stunDamage)
        {
            Item itemAsItem = item as Item;

            LogFile.Log.LogEntryDebug("Throwing " + itemAsItem.SingleItemDescription, LogDebugLevel.Medium);

            //Find target

            List<Point> targetSquares = Game.Dungeon.WeaponUtility.CalculateTrajectorySameLevel(this, target);
            Monster monster = Game.Dungeon.WeaponUtility.FirstMonsterInTrajectory(LocationLevel, targetSquares);

            //Find where it landed

            //Destination will be the last square in trajectory
            Point destination;
            if (targetSquares.Count > 0)
                destination = targetSquares[targetSquares.Count - 1];
            else
                //Threw it on themselves!
                destination = LocationMap;

            //Stopped by a monster
            if (monster != null)
            {
                destination = monster.LocationMap;
            }

            //Make throwing sound AT target location
            Game.Dungeon.AddSoundEffect(item.ThrowSoundMagnitude(), LocationLevel, destination);

            //Draw throw
            Screen.Instance.DrawAreaAttackAnimation(targetSquares, Screen.AttackType.Bullet);

            if (stunDamage)
            {
                if (monster != null && damageOrStunTurns > 0)
                {
                    Game.Dungeon.Combat.ApplyStunDamageToMonster(monster, this, damageOrStunTurns);
                }
            }
            else
            {
                if (monster != null && damageOrStunTurns > 0)
                {
                    Game.Dungeon.Combat.PlayerAttackMonsterThrown(monster, damageOrStunTurns);
                }
            }

            return destination;
        }

        /// <summary>
        /// All combat attacks go through here, since we apply modifiers
        /// </summary>
        /// <param name="damage"></param>
        public CombatResults ApplyCombatDamageToPlayer(Monster attacker, int damage, bool ranged)
        {
            var modifiedDamaged = (int)Math.Floor(CalculateDamageModifierForAttacksOnPlayer(attacker, ranged) * damage);
            return ApplyDamageToPlayer(modifiedDamaged);
        }

        /// <summary>
        /// All combat attacks go through here, since we apply modifiers
        /// </summary>
        /// <param name="damage"></param>
        public CombatResults ApplyCombatDamageToPlayer(int damage)
        {
            return ApplyDamageToPlayer(damage);
        }

        public CombatResults ApplyDamageToPlayer(int damage)
        {
            var remainingDamage = damage;
            int shieldAbsorbs = 0;
            int hpAbsorbs = 0;
            int origHP = Hitpoints;

            //Shield absorbs damage first
            if (Shield > 0)
            {
                var shieldEffect = GetActiveEffects(typeof(PlayerEffects.ShieldEnhance));

                int shieldEnhance = 1;
                if (shieldEffect.Count() > 0)
                {
                     shieldEnhance += (shieldEffect.First() as PlayerEffects.ShieldEnhance).shieldEnhanceAmount;
                }

                shieldAbsorbs = Math.Min(Shield * shieldEnhance, remainingDamage);
                Shield -= shieldAbsorbs / shieldEnhance;
                remainingDamage -= shieldAbsorbs;

                ShieldWasDamagedThisTurn = true;
            }

            if (Shield == 0)
            {
                ShieldIsDisabled = true;
            }

            //Through to health
            //Unless we lost our shield this turn, in which case we get a 1 turn grace
            if (!ShieldWasDamagedThisTurn)
            {
                hpAbsorbs = ApplyDamageToPlayerHitpoints(remainingDamage);
            }

            LogFile.Log.LogEntryDebug("Player takes " + shieldAbsorbs + " damage " + hpAbsorbs + " hitpoint damage.", LogDebugLevel.Medium);
            string hpMessage = "HP: " + origHP + "->" + Hitpoints;

            LogFile.Log.LogEntryDebug(hpMessage, LogDebugLevel.Medium);

            if (Hitpoints <= 0)
                return CombatResults.DefenderDied;
            else
                return CombatResults.NeitherDied;
        }

        public int ApplyDamageToPlayerHitpoints(int damage)
        {
            int hpAbsorbs = 0;

            if (damage > 0)
            {
                hpAbsorbs = Math.Min(Hitpoints, damage);
                Hitpoints -= damage;

                HitpointsWasDamagedThisTurn = true;
            }

            if (Hitpoints <= 0)
            {
                //Player died
                Game.Dungeon.SetPlayerDeath("Took damage");

                LogFile.Log.LogEntry("Player takes " + damage + " damage and dies.");
            }
            return hpAbsorbs;
        }

        /// <summary>
        /// Carry out all pre-turn checks and sets
        /// </summary>
        internal void PreTurnActions()
        {
            UseEnergyForWetware();
            RegenerateStatsPerTurn();

            ShieldWasDamagedThisTurn = false;
            HitpointsWasDamagedThisTurn = false;
            EnergyWasDamagedThisTurn = false;
            
            if (Game.Dungeon.Player.RecalculateCombatStatsRequired)
                Game.Dungeon.Player.CalculateCombatStats();

        }

        private void UseEnergyForWetware()
        {
            var equippedWetware = GetEquippedWetware();

            if (equippedWetware != null)
            {
                var energyDrain = equippedWetware.GetEnergyDrain();
                Energy -= Math.Min(energyDrain, Energy);
                if (energyDrain > 0)
                    EnergyWasDamagedThisTurn = true;

                if (Energy == 0)
                {
                    UnequipWetware();
                    DisableEnergyRecharge();
                }
            }
        }

        public void DisableEnergyRecharge() {
            EnergyRechargeIsDisabled = true;
        }

        public void HealCompletely()
        {
            Hitpoints = MaxHitpoints;
            Shield = MaxShield;
            Energy = MaxEnergy;
        }

        public bool NeedsHealing()
        {
            if (Hitpoints < MaxHitpoints)
                return true;
            if (Shield < MaxShield)
                return true;
            if(Energy < MaxEnergy)
                return true;
            return false;
        }

        private void RegenerateStatsPerTurn()
        {
            if (ShieldIsDisabled)
            {
                TurnsSinceShieldDisabled++;

                if (TurnsSinceShieldDisabled >= TurnsForShieldToTurnBackOn)
                {
                    ShieldIsDisabled = false;
                    TurnsSinceShieldDisabled = 0;
                }
            }

            if (!ShieldIsDisabled && !ShieldWasDamagedThisTurn && DoesShieldRecharge)
            {
                double shieldRegenRate = MaxShield / (double)TurnsToRegenerateShield;
                AddShield((int)Math.Ceiling(shieldRegenRate));

                if (Shield > MaxShield)
                    Shield = MaxShield;
            }

            if (!HitpointsWasDamagedThisTurn && DoHitpointsRecharge)
            {
                double hpRegenRate = MaxHitpoints / (double)TurnsToRegenerateHP;
                Hitpoints += (int)Math.Ceiling(hpRegenRate);
                if (Hitpoints > MaxHitpoints)
                    Hitpoints = MaxHitpoints;
            }

            if (EnergyRechargeIsDisabled)
            {
                TurnsSinceEnergyRechargeDisabled++;

                if (TurnsSinceEnergyRechargeDisabled >= TurnsForEnergyRechargeToTurnBackOn)
                {
                    EnergyRechargeIsDisabled = false;
                    TurnsSinceEnergyRechargeDisabled = 0;
                }
            }

            if (!EnergyRechargeIsDisabled && !EnergyWasDamagedThisTurn)
            {
                double energyRegenRate = MaxEnergy / (double)TurnsToRegenerateEnergy;
                Energy += (int)Math.Ceiling(energyRegenRate);
                if (Energy > MaxEnergy)
                    Energy = MaxEnergy;
            }
        }

        internal void ResetAfterDeath()
        {
            SetupInitialStats();
        }



        internal void AddShield(int shieldBonus)
        {
            Shield += shieldBonus;

            if (Shield > MaxShield)
                Shield = MaxShield;
        }


        internal void AddAmmoToCurrentWeapon()
        {
            var equippedWeapon = GetEquippedRangedWeaponAsItem() as RangedWeapon;

            if (equippedWeapon != null && equippedWeapon.RemainingAmmo() < equippedWeapon.MaxAmmo())
            {
                equippedWeapon.Ammo = equippedWeapon.MaxAmmo();
                Game.MessageQueue.AddMessage(equippedWeapon.SingleItemDescription + " reloaded.");
                LogFile.Log.LogEntryDebug("Giving ammo to " + equippedWeapon.SingleItemDescription, LogDebugLevel.Medium);
            }
            else
            {
                //Apply to a random weapon item
                foreach (var i in Inventory.Items)
                {
                    var item = i as RangedWeapon;

                    if (item != null && item.RemainingAmmo() < item.MaxAmmo())
                    {
                        item.Ammo = item.MaxAmmo();
                        Game.MessageQueue.AddMessage(item.SingleItemDescription + " reloaded.");
                        LogFile.Log.LogEntryDebug("Giving ammo to " + item.SingleItemDescription, LogDebugLevel.Medium);
                        break;
                    }
                }
            }
        }

        internal void NotifyMonsterEvent(MonsterEvent monsterEvent)
        {
            switch (monsterEvent.EventType)
            {
                case MonsterEvent.MonsterEventType.MonsterAttacksPlayer:

                    Game.Base.Running.StopRunning();

                    if (!Screen.Instance.TargetSelected())
                    {
                        Screen.Instance.CreatureToView = monsterEvent.Monster;
                    }

                    break;

                case MonsterEvent.MonsterEventType.MonsterSeenByPlayer:

                    Game.Base.Running.StopRunning();

                    Game.MessageQueue.AddMessage("You see a " + monsterEvent.Monster.SingleDescription + ".");

                    break;
            }
        }
    

        internal void RefillWeapons()
        {
            foreach (var i in Inventory.Items)
            {
                var item = i as RangedWeapon;

                if (item != null && item.RemainingAmmo() < item.MaxAmmo())
                {
                    item.Ammo = item.MaxAmmo();
                    Game.MessageQueue.AddMessage(item.SingleItemDescription + " reloaded.");
                    LogFile.Log.LogEntryDebug("Giving ammo to " + item.SingleItemDescription, LogDebugLevel.Medium);
                }
            }
        }

        protected override string GetGameSprite()
        {
            return "lance";
        }

        internal void AddKill(Monster monster)
        {
            KillCount++;
            Kills.Add(monster);
        }

        internal int GetHealXPCost()
        {
            return 75;//(int)Math.Floor(75 * (1 + 0.5 * (Level - 1)));
        }

        internal int GetLevelXPCost()
        {
            return 150;//(int)Math.Floor(150 * (1 + 0.5 * (Level - 1)));
        }

        internal void LevelUpWithXP()
        {

            var levelUpCost = GetLevelXPCost();
            if (CombatXP >= levelUpCost)
            {
               
                LevelUp();
                CombatXP -= levelUpCost;
                LogFile.Log.LogEntryDebug("Levelled up at cost of  " + GetLevelXPCost() + " XP", LogDebugLevel.Medium);
            }
        }

        internal void HealWithXP()
        {
            var healCost = GetHealXPCost();
            if (CombatXP >= healCost)
            {
                HealCompletely();
                CombatXP -= healCost;
                LogFile.Log.LogEntryDebug("Healed completely at cost of " + GetHealXPCost() + " XP", LogDebugLevel.Medium);
            }
        }

        internal int ScaleRangedDamage(IEquippableItem item, int damageBase)
        {
            return Game.Dungeon.LevelScalingCalculation(damageBase, Level);
        }

        internal int ScaleMeleeDamage(Item item, int damageBase)
        {
            var scaledDamage = Game.Dungeon.LevelScalingCalculation(damageBase, Level);
            //Get a boost for dodge
            if(IsDodgeActive())
                return scaledDamage * 2;
            else
                return scaledDamage;
        }

        public bool LastMoveWasMeleeAttack { get; set; }

        internal bool IsDodgeActive()
        {
            return TurnsMoving > 0 && IsEffectActive(typeof(PlayerEffects.Dodge));
        }

        internal bool IsAimActive()
        {
            return TurnsMoving == 0 && TurnsInactive > 0 && IsEffectActive(typeof(PlayerEffects.Aim));
        }
    }
}
