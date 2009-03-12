using System;
using System.Collections.Generic;
using System.Text;

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

            //Calculate our combat stats
            CalculateCombatStats();


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
        /// Run the creature's action AI
        /// </summary>
        public virtual void ProcessTurn()
        {
            //Base monster classes just sit still
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

        public abstract CombatResults AttackPlayer(Player player);

        public abstract CombatResults AttackMonster(Monster monster);

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public abstract string SingleDescription {get;}

        /// <summary>
        /// Rats
        /// </summary>
        public abstract string GroupDescription {get;}

        /// <summary>
        /// Increment time on our events then use the base class to increment time on the monster's turn counter
        /// </summary>
        /// <returns></returns>
        internal override bool IncrementTurnTime()
        {
            IncrementEventTime();

            return base.IncrementTurnTime();
        }

    }
}
