using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Items
{
    public class ShortSword : Item, IEquippableItem
    {
        public ShortSword()
        {

        }

        public bool CanBeEquippedInSlot(EquipmentSlot slot)
        {
            if (slot == EquipmentSlot.RightHand)
                return true;

            return false;
        }

        public List<EquipmentSlot> EquipmentSlots
        {
            get
            {
                List<EquipmentSlot> retList = new List<EquipmentSlot>();
                retList.Add(EquipmentSlot.RightHand);

                return retList;
            }
        }

        public bool Equip(Creature user)
        {
            LogFile.Log.LogEntryDebug("Short sword equipped", LogDebugLevel.Low);
            return true;
        }

        public bool UnEquip(Creature user)
        {
            LogFile.Log.LogEntryDebug("Short sword unequipped", LogDebugLevel.Low);
            return true;
        }

        public override int GetWeight()
        {
            return 50;
        }

        public override string SingleItemDescription
        {
            get { return "short sword"; }
        }

        public override string GroupItemDescription
        {
            get { return "short swords"; }
        }

        protected override char GetRepresentation()
        {
            return '\\';
        }

        public int ArmourClassModifier()
        {
            return 0;
        }

        public int DamageBase()
        {
            //1d6
            return 6;
        }

        public int DamageModifier()
        {
            return 1;
        }

        public int HitModifier()
        {
            return 1;
        }
    }
}
