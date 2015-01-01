using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Low threat, fights to the death. Good eyesight
    /// </summary>
    public class RatUnique : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 10;
        const int classMinHitpoints = 20;

        public string UniqueName { get; set; }

        public RatUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            SightRadius = 6;
            Unique = true;

            UniqueName = "Ratkins the Unique Rat";
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new RatUnique();
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
            return 4;
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

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return UniqueName; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "rats"; } }

        protected override char GetRepresentation()
        {
            return 'r';
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
            return 15;
        }

        public override int CreatureLevel()
        {
            return 1;
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.Coral;
        }

        public override int GetCombatXP()
        {
            return 35;
        }

        public override int GetMagicXP()
        {
            return 35;
        }

        public override int GetMagicRes()
        {
            return 0;
        }

        public override int GetCharmRes()
        {
            return 10;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }
    }
}
