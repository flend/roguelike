using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Creatures
{
    class Goblin : MonsterThrowAndRunAI
    {
        const int classMaxHitpoints = 15;

        public Goblin()
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
            return 2;
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
            return 0;
        }

        protected override double GetMissileRange()
        {
            return 3.0;
        }

        protected override string GetWeaponName()
        {
            return "dagger";
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "goblin"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription  { get { return "goblins"; } }

        protected override char GetRepresentation()
        {
            return 'g';
        }
    }
}