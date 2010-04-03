using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Healer. Quite clever missile troop
    /// </summary>
    public class Meddler : MonsterSpecialAI
    {
        const int classDeltaHitpoints = 15;
        const int classMinHitpoints = 10;

        public Meddler()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
            Speed = 110;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Meddler();
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
            return 6;
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
            return 4;
        }

        protected override int GetUseSpecialChance()
        {
            return 35;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "meddler"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "meddlers"; } }

        protected override char GetRepresentation()
        {
            return 'M';
        }

        protected override SpecialAIType GetSpecialAIType()
        {
            return SpecialAIType.Healer;
        }

        protected override int RelaxDirectionAt()
        {
            return 50;
        }

        protected override int GetTotalFleeLoops()
        {
            return 50;
        }

        protected override double GetMissileRange()
        {
            return 4;
        }

        protected override string GetWeaponName()
        {
            return "fires a glob of acid";
        }

        public override int CreatureCost()
        {
            return 180;
        }

        public override int CreatureLevel()
        {
            return 5;
        }

        public override Color CreatureColor()
        {
            return ColorPresets.Orange;
        }

        public override int GetMagicXP()
        {
            return 70;
        }

        public override int GetCombatXP()
        {
            return 70;
        }

        public override int GetMagicRes()
        {
            return 70;
        }

        public override int GetCharmRes()
        {
            return 0;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }
    }
}
