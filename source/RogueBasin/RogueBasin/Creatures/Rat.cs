using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Creatures
{
    class Rat : MonsterSimpleAI
    {
        const int classMaxHitpoints = 10;

        public Rat()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
        }

        protected override int ClassMaxHitpoints()
        {
            return classMaxHitpoints;
        }
    }
}
