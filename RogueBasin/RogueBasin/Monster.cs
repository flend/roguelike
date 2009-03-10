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
        /// Get the max hitpoints for this class of creature
        /// </summary>
        /// <returns></returns>
        abstract protected int ClassMaxHitpoints();

        /// <summary>
        /// Run the creature's action AI
        /// </summary>
        public virtual void ProcessTurn()
        {
            //Base monster classes just sit still
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
