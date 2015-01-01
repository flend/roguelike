using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Medium threat, fights to the death. Fast.
    /// </summary>
    public class SpiderUnique : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 10;
        const int classMinHitpoints = 20;

        public string UniqueName { get; set; }

        public SpiderUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            Unique = true;

            UniqueName = "Kal'lot the Spider";
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Spider();
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
            return 10;
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
            return 3;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return UniqueName; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "spiders"; } }

        protected override char GetRepresentation()
        {
            return 's';
        }

        protected override int GetChanceToRecover()
        {
            return 0;
        }

        protected override int GetChanceToFlee()
        {
            return 0;
        }

        protected override int GetMaxHPWillFlee()
        {
            return 0;
        }
        public override int CreatureCost()
        {
            return 30;
        }

        public override int CreatureLevel()
        {
            return 2;
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.DarkOrange;
        }

        public override int GetCombatXP()
        {
            return 60;
        }

        public override int GetMagicXP()
        {
            return 60;
        }

        public override int GetMagicRes()
        {
            return 0;
        }

        public override int GetCharmRes()
        {
            return 20;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }
    }
}
