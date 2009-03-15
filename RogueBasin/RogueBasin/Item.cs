using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Base class for all types of pickup-able items
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(Items.Potion))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionDamUp))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionSpeedUp))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionToHitUp))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.PotionSpeedUp))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.ShortSword))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Bracelet))]

    [System.Xml.Serialization.XmlInclude(typeof(Items.Badge))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Band))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Boots))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Bracer))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.GlassGem))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Greaves))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.LeadRing))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Lockpicks))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Sash))]

    [System.Xml.Serialization.XmlInclude(typeof(Items.Glove))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Backpack))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Book))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Stone))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Medal))]
    [System.Xml.Serialization.XmlInclude(typeof(Items.Flint))]
    
    public abstract class Item : MapObject
    {
        public Item()
        {
            inInventory = false;
            IsEquipped = false;
        }

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
        /// Use hidden name
        /// </summary>
        public virtual bool UseHiddenName { get { return false; } }

        public virtual string HiddenSuffix { get { return ""; } }
    }
}
