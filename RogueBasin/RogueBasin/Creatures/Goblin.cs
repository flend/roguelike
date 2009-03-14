using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Low threat. Stupid missile troop.
    /// </summary>
    class Goblin : MonsterThrowAndRunAI
    {
        const int classDeltaHitpoints = 3;
        const int classMinHitpoints = 3;

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
            return classMinHitpoints + Game.Random.Next(classDeltaHitpoints) + 1;
        }

        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return 7;
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
            return 1;
        }

        protected override double GetMissileRange()
        {
            return 3.0;
        }

        protected override string GetWeaponName()
        {
            return "throws a dagger";
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

        protected override int RelaxDirectionAt()
        {
            return 0;
        }

        protected override int GetTotalFleeLoops()
        {
            return 10;
        }
        public override int CreatureCost()
        {
            return 25;
        }

        public override int CreatureLevel()
        {
            return 2;
        }
    }
}