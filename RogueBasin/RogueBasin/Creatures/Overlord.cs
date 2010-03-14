using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Tough high end demon
    /// </summary>
    public class Overlord : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 15;
        const int classMinHitpoints = 35;

        public string UniqueName { get; set; }

        public Overlord()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
            Speed = 90;

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
            return 10;
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
            return 10;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "overlord"; } }

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
            return 120;
        }

        public override int GetMagicXP()
        {
            return 120;
        }

        public override int GetMagicRes()
        {
            return 60;
        }

        public override int GetCharmRes()
        {
            return 100;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }
    }
}
