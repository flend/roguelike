using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Moderate tough
    /// </summary>
    public class BlackUnicorn : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 10;
        const int classMinHitpoints = 10;

        public BlackUnicorn()
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
            return 3;
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

        public BlackUnicorn NewCreatureOfThisType()
        {
            return new BlackUnicorn();
        }

        public override Color CreatureColor()
        {
            return ColorPresets.DarkGray;
        }

        public override int GetCombatXP()
        {
            return 50;
        }

        public override int GetMagicXP()
        {
            return 70;
        }

        public override int GetMagicRes()
        {
            return 50;
        }

        public override int GetCharmRes()
        {
            return 60;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }
    }
}
