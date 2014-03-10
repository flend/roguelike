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
        //bool CanBeEquippedInSlot(EquipmentSlot slot);

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

        /// <summary>
        /// AC modifier +1 -1 etc. 0 if none.
        /// </summary>
        /// <returns></returns>
        int ArmourClassModifier();

        /// <summary>
        /// Damage base 1d(return value). Highest one will be picked of all equipped items. 0 if not a weapon type.
        /// </summary>
        /// <returns></returns>
        int DamageBase();

        /// <summary>
        /// Damage modifier +1 -1 etc. 0 if none.
        /// </summary>
        /// <returns></returns>
        int DamageModifier();

        /// <summary>
        /// Hit modifier +1 -1 etc. 0 if none.
        /// </summary>
        /// <returns></returns>
        int HitModifier();

        //FLATLINERL ACTIONS

        /// <summary>
        /// Can be used in melee
        /// </summary>
        /// <returns></returns>
        bool HasMeleeAction();

        /// <summary>
        /// Can be fired
        /// </summary>
        /// <returns></returns>
        bool HasFireAction();
        /// <summary>
        /// Can be thrown
        /// </summary>
        /// <returns></returns>
        bool HasThrowAction();

        /// <summary>
        /// Can be operated
        /// </summary>
        /// <returns></returns>
        bool HasOperateAction();

        int MaxAmmo();
        int RemainingAmmo();

        /// <summary>
        /// Fires the item - probably should be a method
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        bool FireItem(Point target);

        /// <summary>
        /// Throws the item - check if we can't pull this out.
        /// Returns where the item lands
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        Point ThrowItem(Point target);

        /// <summary>
        /// Operates the item - definitely a method
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        bool OperateItem();

        /// <summary>
        /// What type of targetting reticle is needed? [for throwing]
        /// </summary>
        /// <returns></returns>
        TargettingType TargetTypeThrow();

        /// <summary>
        /// What type of targetting reticle is needed? [for firing]
        /// </summary>
        /// <returns></returns>
        TargettingType TargetTypeFire();

        /// <summary>
        /// Throwing range
        /// </summary>
        /// <returns></returns>
        int RangeThrow();

        /// <summary>
        /// Firing range
        /// </summary>
        /// <returns></returns>
        int RangeFire();

        /// <summary>
        /// Noise mag of this weapon on firing
        /// </summary>
        /// <returns></returns>
        double FireSoundMagnitude();

        /// <summary>
        /// Noise mag of this weapon on throwing
        /// </summary>
        /// <returns></returns>
        double ThrowSoundMagnitude();

        /// <summary>
        /// Destroyed on throw
        /// </summary>
        /// <returns></returns>
        bool DestroyedOnThrow();

        /// <summary>
        /// Destroyed on throw
        /// </summary>
        /// <returns></returns>
        bool DestroyedOnUse();

        /// <summary>
        /// Items that have MeleeAction override this
        /// </summary>
        /// <returns></returns>
        int MeleeDamage();

        /// <summary>
        /// For the shotgun targets, what's the half-spread angle
        /// </summary>
        /// <returns></returns>
        double ShotgunSpreadAngle();

        int GetEnergyDrain();
    }
}
