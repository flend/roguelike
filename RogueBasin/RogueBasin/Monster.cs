using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    /// <summary>
    /// What happened during a combat
    /// </summary>
    public enum CombatResults
    {
        AttackerDied, DefenderDied, BothDied, NeitherDied, DefenderDamaged, DefenderUnhurt
    }

    /// <summary>
    /// Simplest class for a Monster.
    /// Subclasses will have things like better AI.
    /// Real monsters will inherit off whichever subclass they like
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Bat))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.BlackUnicorn))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.BlackUnicornUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Demon))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.DragonUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Drainer))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Faerie))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.FaerieUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.FerretUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.GoblinWitchdoctorUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Imp))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Maleficarum))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.MaleficarumUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Meddler))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.NecromancerUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Nymph))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Ogre))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.OgreUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.OrcShamanUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Overlord))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.OverlordUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Peon))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Pixie))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.PixieUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.RatUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.SkeletonUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.SpiderUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Uruk))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.UrukUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Whipper))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Rat))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Goblin))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.GoblinWitchdoctor))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Necromancer))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Orc))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.OrcShaman))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Skeleton))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Spider))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Zombie))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Ferret))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Bugbear))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Ghoul))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.SkeletalArcher))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.GhoulUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Mushroom))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Statue))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Drone))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.RotatingTurret))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.PatrolBot))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.PatrolBotArea))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.AlertBot))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.PerimeterBot))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Swarmer))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.ComputerNode))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.RollingBomb))]
    public abstract class Monster : Creature, ITurnAI
    {
        /// <summary>
        /// Effects current active on this monster
        /// </summary>
        public List<MonsterEffect> effects { get; set; }

        public Monster()
        {
            effects = new List<MonsterEffect>();
            
            //Set up attributes from class start values
            maxHitpoints = ClassMaxHitpoints();

            hitpoints = maxHitpoints;

            WasSummoned = false;
            //Calculate our combat stats
            CalculateCombatStats();

            Charmed = false;
            Passive = false;
            
            //In FlatlineRL monsters default to awake
            Sleeping = false;

            Unique = false;
        }

        /// <summary>
        /// Used for spawning
        /// </summary>
        /// <returns></returns>
        public abstract Monster NewCreatureOfThisType();

        public bool Unique { get; set; }

        /// <summary>
        /// Current hitpoints
        /// </summary>
        int hitpoints;

        /// <summary>
        /// Maximum hitpoints
        /// </summary>
        int maxHitpoints;

        /// <summary>
        /// The monster is stunned for this many turns, and will miss this many AI turns
        /// </summary>
        public int StunnedTurns
        {
            get;
            set;
        }

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
        /// Special effect that means the monster fights for the player. Could be a real effect in future, and use this as an accessor
        /// </summary>
        public bool Charmed { get; set; }

        /// <summary>
        /// Special effect that means the monster won't fight be player. Could be a real effect in future, and use this as an accessor
        /// </summary>
        public bool Passive { get; set; }

        /// <summary>
        /// Is the creature currently asleep. Defaults to true to avoid creatures wandering around the dungeon without a player.
        /// </summary>
        public bool Sleeping { get; set; }


        /// <summary>
        /// Player armour class. Auto-calculated so not serialized
        /// </summary>
        protected int armourClass;

        /// <summary>
        /// Player damage base. Auto-calculated so not serialized
        /// </summary>
        protected int damageBase;

        /// <summary>
        /// Player damage modifier. Auto-calculated so not serialized
        /// </summary>
        protected int damageModifier;

        /// <summary>
        /// Player damage modifier. Auto-calculated so not serialized
        /// </summary>
        protected int hitModifier;

        /// <summary>
        /// Get the max hitpoints for this class of creature
        /// </summary>
        /// <returns></returns>
        abstract protected int ClassMaxHitpoints();


        /// <summary>
        /// Will the creature wake on sight of the player or another wakened creature?
        /// Override as false for classic sleep-until-attacked behaviour
        /// </summary>
        /// <returns></returns>
        virtual protected bool WakesOnSight()
        {
            return true;
        }

        /// <summary>
        /// Monster isn't processed until it's been seen by the player
        /// Used so that monsters don't wander around until necessary
        /// Override as false for wake-on-sight or wake-on-attacked behaviour
        /// </summary>
        /// <returns></returns>
        virtual protected bool WakesOnBeingSeen()
        {
            return true;
        }

        /// <summary>
        /// Called when the creature is killed. Can be used to drop treasure. Not used at the moment.
        /// </summary>
        /// <returns></returns>
        virtual public void InventoryDrop() { }

        /// <summary>
        /// Was the creature summoned or raised?
        /// </summary>
        public bool WasSummoned { get; set; }

        /// <summary>
        /// Run the creature's action AI
        /// </summary>
        public virtual void ProcessTurn()
        {
            //Base monster classes just sit still
        }

        /// <summary>
        /// Set creature's status to charmed. Check if it worked at a higher level
        /// </summary>
        public void CharmCreature()
        {
            LogFile.Log.LogEntryDebug(this.SingleDescription + "charmed", LogDebugLevel.Medium);
            Charmed = true;
        }
        /// <summary>
        /// Set creature's status to uncharmed (normal). Check if it worked at a higher level
        /// </summary>
        public void UncharmCreature()
        {
            LogFile.Log.LogEntryDebug(this.SingleDescription + "uncharmed", LogDebugLevel.Medium);
            Charmed = false;
        }

        /// <summary>
        /// Set creature's status to passive. Check if it worked at a higher level
        /// </summary>
        public void PassifyCreature()
        {
            LogFile.Log.LogEntryDebug(this.SingleDescription + "passified", LogDebugLevel.Medium);
            Passive = true;
        }

        /// <summary>
        /// Set creature's status to non-passive (normal). Check if it worked at a higher level
        /// </summary>
        public void UnpassifyCreature()
        {
            LogFile.Log.LogEntryDebug(this.SingleDescription + "unpassified", LogDebugLevel.Medium);
            Passive = false;
        }

        public void CalculateCombatStats()
        {
            //Get defaults from class
            armourClass = ArmourClass();
            damageBase = DamageBase();
            damageModifier = DamageModifier();
            hitModifier = HitModifier();
            speed = BaseSpeed();

            //Check equipped items - unlikely to be implemented

            foreach (Item item in Inventory.Items)
            {
                if (!item.IsEquipped)
                    continue;

                IEquippableItem equipItem = item as IEquippableItem;

                //Error if non-equippable item is equipped
                if (equipItem == null)
                {
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

            //Check effects

            ApplyEffects();
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

            //Only the greatest magnitude (+ or -) effects have an effect
            foreach (MonsterEffect effect in effects)
            {
                if (effect.ArmourClassModifier() > maxAC)
                    maxAC = effect.ArmourClassModifier();

                if (effect.ArmourClassModifier() < minAC)
                    minAC = effect.ArmourClassModifier();

                if (effect.HitModifier() > maxHit)
                    maxHit = effect.HitModifier();

                if (effect.HitModifier() < minHit)
                    minHit = effect.HitModifier();

                if (effect.SpeedModifier() > maxSpeed)
                    maxSpeed = effect.SpeedModifier();

                if (effect.SpeedModifier() < minSpeed)
                    minSpeed = effect.SpeedModifier();

                if (effect.DamageModifier() > maxDamage)
                    maxDamage = effect.DamageModifier();

                if (effect.DamageModifier() < minDamage)
                    minDamage = effect.DamageModifier();
            }

            damageModifier += maxDamage;
            damageModifier += minDamage;

            Speed += maxSpeed;
            Speed += minSpeed;

            hitModifier += maxHit;
            hitModifier += minHit;

            armourClass += maxAC;
            armourClass += minAC;
        }

        /// <summary>
        /// Run an effect on the monster. Calls the effect's onStart and adds it to the current effects queue
        /// </summary>
        /// <param name="effect"></param>
        internal void AddEffect(MonsterEffect effect)
        {
            effects.Add(effect);

            effect.OnStart(this);

            CalculateCombatStats();
        }

        /// <summary>
        /// Remove all existing effects
        /// </summary>
        public void RemoveAllEffects()
        {
            //Increment time on events and remove finished ones
            List<MonsterEffect> finishedEffects = new List<MonsterEffect>();

            foreach (MonsterEffect effect in effects)
            {
                if (!effect.HasEnded())
                    effect.OnEnd(this);

                finishedEffects.Add(effect);
            }

            //Remove finished effects
            foreach (MonsterEffect effect in finishedEffects)
            {
                effects.Remove(effect);
            }

            CalculateCombatStats();
        }

        /// <summary>
        /// Increment time on all monster events. Events that expire will run their onExit() routines and then delete themselves from the list
        /// </summary>
        internal void IncrementEventTime()
        {
            //Increment time on events and remove finished ones
            List<MonsterEffect> finishedEffects = new List<MonsterEffect>();

            bool eventEnded = false;

            foreach (MonsterEffect effect in effects)
            {
                effect.IncrementTime(this);

                if (effect.HasEnded())
                {
                    finishedEffects.Add(effect);
                    eventEnded = true;
                }
            }

            //Remove finished effects
            foreach (MonsterEffect effect in finishedEffects)
            {
                effects.Remove(effect);
            }

            //Recalculate stats if an event ended
            if(eventEnded)
                CalculateCombatStats();
        }

        /// <summary>
        /// Can this creature move?
        /// </summary>
        public virtual bool CanMove() {
            return true;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public abstract string SingleDescription {get;}

        /// <summary>
        /// Rats
        /// </summary>
        public abstract string GroupDescription { get; }

        /// <summary>
        /// Increment time on our events then use the base class to increment time on the monster's turn counter
        /// </summary>
        /// <returns></returns>
        internal override bool IncrementTurnTime()
        {
            IncrementEventTime();

            return base.IncrementTurnTime();
        }

        protected int toHitRoll;

        protected virtual int AttackCreatureWithModifiers(Creature creature, int hitMod, int damBase, int damMod, int ACmod)
        {
            //Just do damage base
            return damageBase;

            /*
            int attackToHit = hitModifier + hitMod;
            int attackDamageMod = damageModifier + damMod;

            int attackDamageBase;

            if (damBase > damageBase)
                attackDamageBase = damBase;
            else
                attackDamageBase = damageBase;

            int targetAC = creature.ArmourClass() + ACmod;
            toHitRoll = Utility.d20() + attackToHit;

            if (toHitRoll >= targetAC)
            {
                //Hit - calculate damage
                int totalDamage = Utility.DamageRoll(attackDamageBase) + attackDamageMod;

                return totalDamage;
            }

            //Miss
            return 0;*/


        }

        protected abstract string HitsPlayerCombatString();

        protected abstract string MissesPlayerCombatString();

        protected abstract string HitsMonsterCombatString(Monster target);

        protected abstract string MissesMonsterCombatString(Monster target);

        /// <summary>
        /// Can the creature be charmed by the charm power? Put in to exclude classes of monsters by the AI they have. Some AIs don't have charm and passive rules
        /// </summary>
        public virtual bool CanBeCharmed() {
            return false;
        }

        /// <summary>
        /// Can the creature be passified by the charm power? Put in to exclude classes of monsters by the AI they have. Some AIs don't have charm and passive rules
        /// </summary>
        public virtual bool CanBePassified()
        {
            return false;
        }

        /// <summary>
        /// Important to keep this the only place where the player gets injured
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual CombatResults AttackPlayer(Player player)
        {
            //Recalculate combat stats if required
            if (this.RecalculateCombatStatsRequired)
                this.CalculateCombatStats();

            if (player.RecalculateCombatStatsRequired)
                player.CalculateCombatStats();

            //Calculate damage from a normal attack
            int damage = AttackCreatureWithModifiers(player, 0, 0, 0, 0);

            //Do we hit the player?
            if (damage > 0)
            {
                int monsterOrigHP = player.Hitpoints;

                player.Hitpoints -= damage;

                //Is the player dead, if so kill it?
                if (player.Hitpoints <= 0)
                {
                    
                    //Message queue string
                    string combatResultsMsg = "MvP " + this.Representation + " ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + player.Hitpoints + " killed";
                    
                    
                    //string playerMsg = "The " + this.SingleDescription + " hits you. You die.";
                    string playerMsg = HitsPlayerCombatString() + " You are knocked out.";
                    Game.MessageQueue.AddMessage(playerMsg);
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    Game.Dungeon.SetPlayerDeath("was knocked out by a " + this.SingleDescription);

                    return CombatResults.DefenderDied;
                }

                //Debug string
                string combatResultsMsg3 = "MvP " + this.Representation + " ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + player.Hitpoints + " injured";
                //string playerMsg3 = "The " + this.SingleDescription + " hits you.";
                string playerMsg3 = HitsPlayerCombatString();
                Game.MessageQueue.AddMessage(playerMsg3);
                LogFile.Log.LogEntryDebug(combatResultsMsg3, LogDebugLevel.Medium);

                return CombatResults.DefenderDamaged;
            }

            //Miss
            string combatResultsMsg2 = "MvP "  + this.Representation + " ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + player.Hitpoints + " miss";
            //string playerMsg2 = "The " + this.SingleDescription + " misses you.";
            string playerMsg2 = MissesPlayerCombatString();
            Game.MessageQueue.AddMessage(playerMsg2);
            LogFile.Log.LogEntryDebug(combatResultsMsg2, LogDebugLevel.Medium);

            return CombatResults.DefenderUnhurt;
        }

        public virtual CombatResults AttackMonster(Monster monster)
        {
            //Recalculate combat stats if required
            if (this.RecalculateCombatStatsRequired)
                this.CalculateCombatStats();

            if (monster.RecalculateCombatStatsRequired)
                monster.CalculateCombatStats();

            //Set the attacked by marker
            monster.LastAttackedBy = this;
            monster.LastAttackedByID = this.UniqueID;

            //Notify the creature that it has been attacked
            monster.NotifyAttackByCreature(this);

            //Wake a sleeping creature
            if (monster.Sleeping)
            {
                monster.WakeCreature();

                //All wake on sight creatures should be awake at this point. If it's a non-wake-on-sight tell the player it wakes
                Game.MessageQueue.AddMessage("The " + monster.SingleDescription + " wakes up!");
                LogFile.Log.LogEntryDebug(monster.Representation + " wakes on attack by monster " + this.Representation, LogDebugLevel.Low);
            }

            //Calculate damage from a normal attack
            int damage = AttackCreatureWithModifiers(monster, 0, 0, 0, 0);
            
            string messageStr;
            
            //Do we hit the monster?
            if (damage > 0)
            {
                //Notify the creature that it has been hit
                monster.NotifyHitByCreature(this, damage);

                int monsterOrigHP = monster.Hitpoints;

                monster.Hitpoints -= damage;

                //Is the player dead, if so kill it?
                if (monster.Hitpoints <= 0)
                {
                    Game.Dungeon.KillMonster(monster, false);
                    
                    //Debug string
                    string combatResultsMsg = "MvM " + this.Representation + " vs " + monster.Representation + " ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " killed";
                    messageStr = HitsMonsterCombatString(monster) + " It's knocked out.";
                    Game.MessageQueue.AddMessage(messageStr);
                    //Game.MessageQueue.AddMessage(combatResultsMsg);
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    //Add charm XP if appropriate
                    Game.Dungeon.Player.AddXPMonsterAttack(this, monster);

                    return CombatResults.DefenderDied;
                }

                //Debug string
                string combatResultsMsg3 = "MvM " + this.Representation + " vs " + monster.Representation + " ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " injured";
                messageStr = HitsMonsterCombatString(monster);
                Game.MessageQueue.AddMessage(messageStr);
                LogFile.Log.LogEntryDebug(combatResultsMsg3, LogDebugLevel.Medium);

                return CombatResults.DefenderDamaged;
            }

            //Miss
            string combatResultsMsg2 = "MvM " + this.Representation + " vs " + monster.Representation + " ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monster.Hitpoints + " miss";
            //Game.MessageQueue.AddMessage(combatResultsMsg2);
            messageStr = MissesMonsterCombatString(monster);
            Game.MessageQueue.AddMessage(messageStr);
            LogFile.Log.LogEntryDebug(combatResultsMsg2, LogDebugLevel.Medium);

            return CombatResults.DefenderUnhurt;
        }

        /// <summary>
        /// Calculate the derived (used by other functions) sight radius based on the player's NormalSightRadius and the light level of the dungeon level the monster is on
        /// </summary>
        public void CalculateSightRadius()
        {
            //Set vision
            double sightRatio = NormalSightRadius / 5.0;
            SightRadius = (int)Math.Ceiling(Game.Dungeon.Levels[LocationLevel].LightLevel * sightRatio);
        }

        /// <summary>
        /// Creature cost for level gen
        /// </summary>
        /// <returns></returns>
        abstract public int CreatureCost();

        /// <summary>
        /// Creature level for level gen
        /// </summary>
        /// <returns></returns>
        abstract public int CreatureLevel();

        /// <summary>
        /// We have be attacked (but not necessarily damaged by) creature
        /// </summary>
        /// <param name="creature"></param>
        abstract public void NotifyAttackByCreature(Creature creature);

        /// <summary>
        /// We have been hit by creature
        /// </summary>
        /// <param name="creature"></param>
        abstract public void NotifyHitByCreature(Creature creature, int damage);

        /// <summary>
        /// We have been killed. Do special game effects. Do some clean up of internal structure
        /// </summary>
        /// <param name="creature"></param>
        abstract public void NotifyMonsterDeath();

        /// <summary>
        /// The monster's combat challenge rating
        /// </summary>
        /// <returns></returns>
        abstract public int GetCombatXP();

        /// <summary>
        /// The monster's magic challenge rating
        /// </summary>
        /// <returns></returns>
        abstract public int GetMagicXP();

        /// <summary>
        /// The monster's magic resistance
        /// </summary>
        /// <returns></returns>
        virtual public int GetMagicRes() { return 0; }
        
        /// <summary>
        /// The monster's charm resistance
        /// </summary>
        /// <returns></returns>
        virtual public int GetCharmRes() { return 0; }

        /// <summary>
        /// Wake a creature (remove sleeping flag)
        /// </summary>
        internal void WakeCreature()
        {
            Sleeping = false;
        }

        /// <summary>
        /// Are we on patrol (i.e. not attacking?) ? Highlight on the map
        /// </summary>
        /// <returns></returns>
        virtual public bool OnPatrol()
        {
            return false;
        }

        /// <summary>
        /// Are we on patrol (i.e. not attacking?) ? Highlight on the map
        /// </summary>
        /// <returns></returns>
        virtual public bool InPursuit()
        {
            return false;
        }

        public virtual bool ShowHeading() {
            return true;
        }

        /// <summary>
        /// Do on Killed special effects
        /// </summary>
        internal virtual void OnKilledSpecialEffects()
        {
            return;
        }
    }
}
