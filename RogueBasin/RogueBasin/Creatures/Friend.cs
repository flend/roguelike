using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Creatures
{
    public class Friend : MonsterNullAI
    {
        const int classMaxHitpoints = 100;

        public Friend()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        protected override int ClassMaxHitpoints()
        {
            return classMaxHitpoints;
        }

        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return 10;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 8;
        }

        /// <summary>
        /// Creature damage modifier.  Set by type of creature.
        /// </summary>
        public override int DamageModifier()
        {
            return 0;
        }

        public override int HitModifier()
        {
            return 10;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "poor defenceless friend"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "liches"; } }

        protected override char GetRepresentation()
        {
            return '@';
        }
        public override int CreatureCost()
        {
            return 1;
        }

        public override int CreatureLevel()
        {
            return 1;
        }

        /// <summary>
        /// A creature has attacked us (possibly from out of our view range). Don't just sit there passively
        /// </summary>
        public override void NotifyAttackByCreature(Creature creature)
        {
          //Do nothing
        }
    }
}
