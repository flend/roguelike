using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Tough mid range demon
    /// </summary>
    public class MaleficarumUnique : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 50;
        const int classMinHitpoints = 50;

        public string UniqueName { get; set; }

        public MaleficarumUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            Unique = true;
            UniqueName = "Alcelchior the Thrice Damned";
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
            return 18;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 12;
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
            return 10;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return UniqueName; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "maleficarum"; } }

        protected override char GetRepresentation()
        {
            return 'M';
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
            return 200;
        }

        public override int CreatureLevel()
        {
            return 5;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Creatures.Maleficarum();
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.Crimson;
        }

        public override int GetCombatXP()
        {
            return 150;
        }

        public override int GetMagicXP()
        {
            return 150;
        }

        public override int GetMagicRes()
        {
            return 40;
        }

        public override int GetCharmRes()
        {
            return 90;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }
    }
}
