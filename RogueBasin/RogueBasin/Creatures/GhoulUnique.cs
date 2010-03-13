using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    public class GhoulUnique : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 50;
        const int classMinHitpoints = 50;

        public string UniqueName { get; set; }

        public GhoulUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
            Speed = 40;
            Unique = true;
            UniqueName = "Terrance the Brick";
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Ghoul();
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
            return 1;
        }

        public override int HitModifier()
        {
            return 4;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return UniqueName; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "ghouls"; } }

        protected override char GetRepresentation()
        {
            return 'G';
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
            return 60;
        }

        public override int CreatureLevel()
        {
            return 5;
        }

        public override Color CreatureColor()
        {
            return ColorPresets.YellowGreen;
        }

        public override int GetMagicXP()
        {
            return 100;
        }

        public override int GetCombatXP()
        {
            return 100;
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