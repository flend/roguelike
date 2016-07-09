using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    /// <summary>
    /// Base class for all types of pickup-able items
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(Items.Potion))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionDamUp))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionSpeedUp))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionToHitUp))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionSightUp))]

    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionMajHealing))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionMajDamUp))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionMajSpeedUp))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionMajToHitUp))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionMajSightUp))]

    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionSuperHealing))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionSuperDamUp))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionSuperSpeedUp))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionSuperToHitUp))]

    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionMPRestore))]

    [System.Xml.Serialization.XmlInclude(typeof(Items.HealingPotion))]

    [System.Xml.Serialization.XmlInclude(typeof(Items.SoundGrenade))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.FragGrenade))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.StunGrenade))]

    [System.Xml.Serialization.XmlInclude(typeof(Items.Pistol))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Shotgun))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Laser))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Vibroblade))]

    [System.Xml.Serialization.XmlInclude(typeof(Items.NanoRepair))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.TacticalOverlay))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.StealthCloak))]
    public abstract class Item : MapObject
    {
        System.Drawing.Color defaultItemColor = System.Drawing.Color.Red;

        private string id;

        public Item()
        {
            inInventory = false;
            IsEquipped = false;

            IsFound = false;

            Effects = new List<ItemEffect>();
        }

        /// <summary>
        /// Effects current active on this monster
        /// </summary>
        public List<ItemEffect> Effects { get; private set; }

        /// <summary>
        /// Is this in a creature's inventory
        /// </summary>
        bool inInventory;

        /// <summary>
        /// Is equipped by a creature. This properly is tracked on item so the inventory doesn't have to search through a player or creature's equipped slots when deciding whether to stack items.
        /// Could possibly be placed by a call in Inventory to owner.IsThisItemEquipped()?
        /// </summary>
        public bool IsEquipped { get; set; }

        /// <summary>
        /// Is this item in an inventory and therefore should not be rendered on the map?
        /// Policy is that LocationMap and LocationLevel may contain out-of-date data when InInventory is set
        /// </summary>
        public bool InInventory
        {
            get
            {
                return inInventory;
            }

            set
            {
                inInventory = value;
            }
        }

        /// <summary>
        /// Has the object been found (i.e. picked up) by the player. For PrincessRL
        /// </summary>
        public bool IsFound { get; set; }

        /// <summary>
        /// Return the weight of the object. Set in derived classes
        /// </summary>
        /// <returns></returns>
        public abstract int GetWeight();

        /// <summary>
        /// Single item description, e.g. 'sword'
        /// </summary>
        public abstract string SingleItemDescription
        {
            get;
        }

        /// <summary>
        /// Group item description, e.g. 'swords'
        /// </summary>
        public abstract string GroupItemDescription
        {
            get;
        }

        /// <summary>
        /// A hidden id, for display in developer maps etc.
        /// </summary>
        public virtual string QuestId
        {
            get
            {

                return "";
            }
        }

        /// <summary>
        /// Use hidden name
        /// </summary>
        public virtual bool UseHiddenName { get { return false; } }

        public virtual string HiddenSuffix { get { return ""; } }

        public virtual System.Drawing.Color GetColour() { return defaultItemColor; }

        ///Cost of item to level gen
        public virtual int ItemCost()
        {
            return 0;
        }

        /// <summary>
        /// Create a new copy of this item. Override if memberwise clone isn't good enough
        /// </summary>
        /// <returns></returns>
        public virtual Item CloneItem()
        {
            return this.MemberwiseClone() as Item;
        }

        public virtual bool OnPickup(Creature pickupCreature)
        {
            return false;
        }

        /// <summary>
        /// Item is destroyed on pickup and doesn't reach inventory
        /// </summary>
        /// <returns></returns>
        public virtual bool DestroyedOnPickup()
        {
            return false;
        }

        public virtual bool OnDrop(Creature droppingCreature)
        {
            return false;
        }

        public virtual bool DoNotPickupDuplicates()
        {
            return false;
        }

        internal virtual bool IncrementTurnTime()
        {
            IncrementEventTime();

            //We don't really have an action
            return false;
        }

        /// <summary>
        /// Increment time on all monster events. Events that expire will run their onExit() routines and then delete themselves from the list
        /// </summary>
        public void IncrementEventTime()
        {
            //Increment time on events and remove finished ones
            List<ItemEffect> finishedEffects = new List<ItemEffect>();

            foreach (ItemEffect effect in Effects)
            {
                effect.IncrementTime(this);

                if (effect.HasEnded())
                {
                    finishedEffects.Add(effect);
                }
            }

            //Remove finished effects
            foreach (ItemEffect effect in finishedEffects)
            {
                Effects.Remove(effect);
            }
        }

        /// <summary>
        /// Run an effect on the monster. Calls the effect's onStart and adds it to the current effects queue
        /// </summary>
        /// <param name="effect"></param>
        internal void AddEffect(ItemEffect effect)
        {
            Effects.Add(effect);

            effect.OnStart(this);
        }

        /// <summary>
        /// Remove all existing effects
        /// </summary>
        public void RemoveAllEffects()
        {
            //Increment time on events and remove finished ones
            List<ItemEffect> finishedEffects = new List<ItemEffect>();

            foreach (ItemEffect effect in Effects)
            {
                if (!effect.HasEnded())
                    effect.OnEnd(this);

                finishedEffects.Add(effect);
            }

            //Remove finished effects
            foreach (ItemEffect effect in finishedEffects)
            {
                Effects.Remove(effect);
            }
        }
    }
}
