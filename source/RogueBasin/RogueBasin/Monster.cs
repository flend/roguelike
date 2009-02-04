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
    public abstract class Monster : Creature, ITurnAI
    {

        public Monster()
        {
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

        public abstract CombatResults AttackPlayer(Player player);

        public abstract CombatResults AttackMonster(Monster monster);

    }
}
