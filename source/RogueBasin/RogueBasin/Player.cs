using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    public class Player : Creature
    {
        /// <summary>
        /// Effects that are active on the player
        /// </summary>
        List<PlayerEffect> effects;

        /// <summary>
        /// Current hitpoints
        /// </summary>
        int hitpoints;

        /// <summary>
        /// Maximum hitpoints
        /// </summary>
        int maxHitpoints;

        public Player()
        {
            effects = new List<PlayerEffect>();
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

        public CombatResults AttackMonster(Monster monster)
        {
            //The monster always dies
            Game.Dungeon.KillMonster(monster);

            string msg = monster.Representation + " was killed!";
            Game.MessageQueue.AddMessage(msg);
            LogFile.Log.LogEntry(msg);

            return CombatResults.DefenderDied;

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

            return base.IncrementTurnTime();
        }

        /// <summary>
        /// Run an effect on the player. Calls the effect's onStart and adds it to the current effects queue
        /// </summary>
        /// <param name="effect"></param>
        internal void AddEffect(PlayerEffect effect)
        {
            effect.OnStart();

            effects.Add(effect);
        }

        protected override char GetRepresentation()
        {
            return '@';
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

            bool usedSuccessfully = itemToUse.Use(Game.Dungeon.Player);

            if (itemToUse.UsedUp)
            {
                //Remove item from inventory and don't drop on floor
                Inventory.RemoveItem(itemToUse);
            }

            return usedSuccessfully;
        }
    }
}
