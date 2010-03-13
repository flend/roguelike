using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Tough high end demon
    /// </summary>
    public class OverlordUnique : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 50;
        const int classMinHitpoints = 50;

        public string UniqueName { get; set; }

        public OverlordUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
            Speed = 100;

            Unique = true;
            UniqueName = "Derang the Unkillable";
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
            return 22;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 15;
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
            return 15;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return UniqueName; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "overlords"; } }

        protected override char GetRepresentation()
        {
            return 'O';
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
            return 250;
        }

        public override int CreatureLevel()
        {
            return 6;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Creatures.Overlord();
        }

        public override Color CreatureColor()
        {
            return ColorPresets.Yellow;
        }

        public override int GetCombatXP()
        {
            return 200;
        }

        public override int GetMagicXP()
        {
            return 200;
        }

        public override int GetMagicRes()
        {
            return 40;
        }

        public override int GetCharmRes()
        {
            return 100;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }
    }
}
