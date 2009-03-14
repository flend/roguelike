using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Raiser. Stupid.
    /// </summary>
    class GoblinWitchdoctor : MonsterSpecialAI
    {
        const int classDeltaHitpoints = 3;
        const int classMinHitpoints = 3;


        public GoblinWitchdoctor()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
            SightRadius = 5;
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
            return 8;
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
            return 1;
        }

        public override int HitModifier()
        {
            return 1;
        }

        protected override int GetUseSpecialChance()
        {
            return 25;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "goblin witch"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "goblin witch"; } }

        protected override char GetRepresentation()
        {
            return 'G';
        }

        protected override SpecialAIType GetSpecialAIType()
        {
            return SpecialAIType.Raiser;
        }

        protected override int RelaxDirectionAt()
        {
            return 0;
        }

        protected override int GetTotalFleeLoops()
        {
            return 10;
        }

        protected override double GetMissileRange()
        {
            return 3.5;
        }

        protected override string GetWeaponName()
        {
            return "fires a crackling bolt of energy";
        }
    }
}
