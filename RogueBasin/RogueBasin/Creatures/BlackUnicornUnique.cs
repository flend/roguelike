using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Moderate tough
    /// </summary>
    public class BlackUnicornUnique : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 20;
        const int classMinHitpoints = 25;

        public string UniqueName { get; set; }

        public BlackUnicornUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));

            Unique = true;
            UniqueName = "Illustrous the Black Unicorn";
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
            return 6;
        }

        /// <summary>
        /// Creature damage modifier.  Set by type of creature.
        /// </summary>
        public override int DamageModifier()
        {
            return 2;
        }

        public override int HitModifier()
        {
            return 5;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "black unicorn"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "black unicorn"; } }

        protected override char GetRepresentation()
        {
            return 'U';
        }

        protected override int GetChanceToRecover()
        {
            return 10;
        }

        protected override int GetChanceToFlee()
        {
            return 0;
        }

        protected override int GetMaxHPWillFlee()
        {
            return Hitpoints;
        }

        public override int CreatureCost()
        {
            return 40;
        }

        public override int CreatureLevel()
        {
            return 4;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new BlackUnicorn();
        }

        public override Color CreatureColor()
        {
            return ColorPresets.DarkGray;
        }

        public override int GetCombatXP()
        {
            return 70;
        }

        public override int GetMagicXP()
        {
            return 70;
        }

        public override int GetMagicRes()
        {
            return 20;
        }

        public override int GetCharmRes()
        {
            return 30;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }
    }
}
