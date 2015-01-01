using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Equipment slot and current contents
    /// </summary>
    public class EquipmentSlotInfo
    {
        /// <summary>
        /// The type of this slot
        /// </summary>
        public EquipmentSlot slotType;

        /// <summary>
        /// The item currently equipped in this slot. Can be null to show nothing is equipped
        /// </summary>
        public Item equippedItem = null;

        //For serialization
        EquipmentSlotInfo()
        {

        }

        public EquipmentSlotInfo(EquipmentSlot type) {
            slotType = type;
        }
    }
}
