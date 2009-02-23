using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Items
{
    class ShortSword : Item, IEquippableItem
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

        public bool Equip(Creature user)
        {
            return true;
        }

        public bool UnEquip(Creature user)
        {
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
            return '!';
        }
    }
}
