using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Tough mid range demon
    /// </summary>
    public class Demon : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 20;
        const int classMinHitpoints = 10;

        public Demon()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
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
            return 8;
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
            return 6;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "demon"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "demons"; } }

        protected override char GetRepresentation()
        {
            return '&';
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
            return 60;
        }

        public override int CreatureLevel()
        {
            return 5;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Demon();
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.Red;
        }

        public override int GetCombatXP()
        {
            return 80;
        }

        public override int GetMagicXP()
        {
            return 80;
        }

        public override int GetMagicRes()
        {
            return 40;
        }

        public override int GetCharmRes()
        {
            return 70;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }
    }
}
