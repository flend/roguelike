using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Medium threat. Slow. Hurts when it hits.
    /// </summary>
    public class Zombie : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 13;
        const int classMinHitpoints = 12;

        public Zombie()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
            SightRadius = 5;
            Speed = 75;
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
            return 3;
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
            return 'S';
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
    }
}
