using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// An item which can be equipped. Currently this is inherited off Item which has the Use() method. In future I might made Equippable and Useable interfaces
    /// </summary>
    public interface IEquippableItem
    {
        /// <summary>
        /// Returns true if this object can be equipped in the slot specified
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        bool CanBeEquippedInSlot(EquipmentSlot slot);

        /// <summary>
        /// Returns a list of possible equipment slots that the item can be equipped in
        /// </summary>
        /// <returns></returns>
        List<EquipmentSlot> EquipmentSlots
        {
            get;
        }

        /// <summary>
        /// Apply the equipped effect to the user. Returns true on successfully equipped. May want to consider a hooking interface as well (events).
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
       bool Equip(Creature user);

        /// <summary>
        /// Unequip the object and remove its effect from the user. Returns true on successfully unequipped.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool UnEquip(Creature user);
    }
}
