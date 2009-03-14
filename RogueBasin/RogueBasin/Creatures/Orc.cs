using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Medium brawler. Will run
    /// </summary>
    class Orc : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 12;
        const int classMinHitpoints = 8;

        public Orc()
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
        public override string SingleDescription { get { return "orc"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "orcs"; } }

        protected override char GetRepresentation()
        {
            return 'o';
        }

        protected override int GetChanceToRecover()
        {
            return 20;
        }

        protected override int GetChanceToFlee()
        {
            return 40;
        }

        protected override int GetMaxHPWillFlee()
        {
            return 8;
        }
        public override int CreatureCost()
        {
            return 30;
        }

        public override int CreatureLevel()
        {
            return 3;
        }
    }
}
