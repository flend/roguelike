using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Healer. Quite clever missile troop
    /// </summary>
    public class OrcShamanUnique : MonsterSpecialAI
    {
        const int classDeltaHitpoints = 12;
        const int classMinHitpoints = 30;

        public string UniqueName { get; set; }

        public OrcShamanUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));

            Unique = true;

            UniqueName = "Davos the Orc Shaman King";
        }

        public override Monster NewCreatureOfThisType()
        {
            return new OrcShaman();
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
            return 14;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 4;
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
            return 5;
        }

        protected override int GetUseSpecialChance()
        {
            return 90;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return UniqueName; } }

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
            return 5;
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
            return 120;
        }

        public override int CreatureLevel()
        {
            return 3;
        }

        public override Color CreatureColor()
        {
            return ColorPresets.LimeGreen;
        }

        public override int GetMagicXP()
        {
            return 85;
        }

        public override int GetCombatXP()
        {
            return 85;
        }

        public override int GetMagicRes()
        {
            return 0;
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
