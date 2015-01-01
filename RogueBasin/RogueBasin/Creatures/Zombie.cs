using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Medium threat. Slow. Hurts when it hits.
    /// </summary>
    public class Zombie : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 15;
        const int classMinHitpoints = 10;

        public Zombie()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Zombie();
        }

        public override int BaseSpeed()
        {
            return 75;
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
        public override string SingleDescription { get { return "zombie"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "zombies"; } }

        protected override char GetRepresentation()
        {
            return 'Z';
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
            return 40;
        }

        public override int CreatureLevel()
        {
            return 4;
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.Cornsilk;
        }

        public override int GetMagicXP()
        {
            return 50;
        }

        public override int GetCombatXP()
        {
            return 50;
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
