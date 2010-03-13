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
        AttackerDied, DefenderDied, BothDied, NeitherDied
    }

    /// <summary>
    /// Simplest class for a Monster.
    /// Subclasses will have things like better AI.
    /// Real monsters will inherit off whichever subclass they like
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Rat))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Lich))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Friend))]
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
    public abstract class Monster : Creature, ITurnAI
    {
        /// <summary>
        /// Effects current active on this monster
        /// </summary>
        List<MonsterEffect> effects;

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
        }

        /// <summary>
        /// Current hitpoints
        /// </summary>
        int hitpoints;

        /// <summary>
        /// Maximum hitpoints
        /// </summary>
        int maxHitpoints;

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
        /// Called when the creature is killed. Can be used to drop treasure.
        /// </summary>
        /// <returns></returns>
        abstract public void InventoryDrop();

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

            foreach (MonsterEffect effect in effects)
            {
                armourClass += effect.ArmourClassModifier();
                damageModifier += effect.DamageModifier();
                hitModifier += effect.HitModifier();

                if (effect.DamageBase() > damageBase)
                {
                    damageBase = effect.DamageBase();
                }
            }
        }

        /// <summary>
        /// Run an effect on the monster. Calls the effect's onStart and adds it to the current effects queue
        /// </summary>
        /// <param name="effect"></param>
        internal void AddEffect(MonsterEffect effect)
        {
            effects.Add(effect);

            effect.OnStart();
        }

        /// <summary>
        /// Increment time on all monster events. Events that expire will run their onExit() routines and then delete themselves from the list
        /// </summary>
        internal void IncrementEventTime()
        {
            //Increment time on events and remove finished ones
            List<MonsterEffect> finishedEffects = new List<MonsterEffect>();

            foreach (MonsterEffect effect in effects)
            {
                effect.IncrementTime();

                if (effect.HasEnded())
                {
                    finishedEffects.Add(effect);
                }
            }

            //Remove finished effects
            foreach (MonsterEffect effect in finishedEffects)
            {
                effects.Remove(effect);
            }
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

        //Could be in Monster
        protected virtual int AttackCreatureWithModifiers(Creature player, int hitMod, int damBase, int damMod, int ACmod)
        {
            int attackToHit = hitModifier + hitMod;
            int attackDamageMod = damageModifier + damMod;

            int attackDamageBase;

            if (damBase > damageBase)
                attackDamageBase = damBase;
            else
                attackDamageBase = damageBase;

            int playerAC = player.ArmourClass() + ACmod;
            toHitRoll = Utility.d20() + attackToHit;

            if (toHitRoll >= playerAC)
            {
                //Hit - calculate damage
                int totalDamage = Utility.DamageRoll(attackDamageBase) + attackDamageMod;

                return totalDamage;
            }

            //Miss
            return 0;
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
                    Game.Dungeon.PlayerDeath("was knocked out by a " + this.SingleDescription);

                    //Message queue string
                    string combatResultsMsg = "MvP ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + player.Hitpoints + " killed";
                    
                    
                    //string playerMsg = "The " + this.SingleDescription + " hits you. You die.";
                    string playerMsg = HitsPlayerCombatString() + " You are knocked out.";
                    Game.MessageQueue.AddMessage(playerMsg);
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    return CombatResults.DefenderDied;
                }

                //Debug string
                string combatResultsMsg3 = "MvP ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + player.Hitpoints + " injured";
                //string playerMsg3 = "The " + this.SingleDescription + " hits you.";
                string playerMsg3 = HitsPlayerCombatString();
                Game.MessageQueue.AddMessage(playerMsg3);
                LogFile.Log.LogEntryDebug(combatResultsMsg3, LogDebugLevel.Medium);

                return CombatResults.NeitherDied;
            }

            //Miss
            string combatResultsMsg2 = "MvP ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + player.Hitpoints + " miss";
            //string playerMsg2 = "The " + this.SingleDescription + " misses you.";
            string playerMsg2 = MissesPlayerCombatString();
            Game.MessageQueue.AddMessage(playerMsg2);
            LogFile.Log.LogEntryDebug(combatResultsMsg2, LogDebugLevel.Medium);

            return CombatResults.NeitherDied;
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

            //Notify the creature that it has been hit
            monster.NotifyAttackByCreature(this);

            //Calculate damage from a normal attack
            int damage = AttackCreatureWithModifiers(monster, 0, 0, 0, 0);
            
            string playerMsg;
            
            //Do we hit the player?
            if (damage > 0)
            {
                int monsterOrigHP = monster.Hitpoints;

                monster.Hitpoints -= damage;

                //Is the player dead, if so kill it?
                if (monster.Hitpoints <= 0)
                {

                    Game.Dungeon.KillMonster(monster);

                    //Add charm XP if appropriate
                    Game.Dungeon.Player.AddXPMonsterAttack(this, monster);

                    //Debug string
                    string combatResultsMsg = "MvM ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " killed";
                    playerMsg = HitsMonsterCombatString(monster) + " It's knocked out.";
                    Game.MessageQueue.AddMessage(playerMsg);
                    //Game.MessageQueue.AddMessage(combatResultsMsg);
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    return CombatResults.DefenderDied;
                }

                //Debug string
                string combatResultsMsg3 = "MvM ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " injured";
                playerMsg = HitsMonsterCombatString(monster);
                Game.MessageQueue.AddMessage(playerMsg);
                LogFile.Log.LogEntryDebug(combatResultsMsg3, LogDebugLevel.Medium);

                return CombatResults.NeitherDied;
            }

            //Miss
            string combatResultsMsg2 = "MvM ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monster.Hitpoints + " miss";
            //Game.MessageQueue.AddMessage(combatResultsMsg2);
            playerMsg = MissesMonsterCombatString(monster);
            Game.MessageQueue.AddMessage(playerMsg);
            LogFile.Log.LogEntryDebug(combatResultsMsg2, LogDebugLevel.Medium);

            return CombatResults.NeitherDied;
        }

        virtual public Color CreatureColor()
        {
            return ColorPresets.White;
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

        abstract public void NotifyAttackByCreature(Creature creature);

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

    }
}
