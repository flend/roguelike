using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Healer. Clever missile troop
    /// </summary>
    public class OrcShaman : MonsterSpecialAI
    {
        const int classDeltaHitpoints = 12;
        const int classMinHitpoints = 8;

        public OrcShaman()
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
            return 12;
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
            return 3;
        }

        protected override int GetUseSpecialChance()
        {
            return 25;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "orc shaman"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "orc shamen"; } }

        protected override char GetRepresentation()
        {
            return 'O';
        }

        protected override SpecialAIType GetSpecialAIType()
        {
            return SpecialAIType.Healer;
        }

        protected override int RelaxDirectionAt()
        {
            return 10;
        }

        protected override int GetTotalFleeLoops()
        {
            return 50;
        }

        protected override double GetMissileRange()
        {
            return 3.5;
        }

        protected override string GetWeaponName()
        {
            return "fires a bolt of flame";
        }

        public override int CreatureCost()
        {
            return 75;
        }

        public override int CreatureLevel()
        {
            return 3;
        }
    }
}
