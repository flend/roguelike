using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Medium-hard brawler. Will run
    /// </summary>
    public class UrukUnique : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 25;
        const int classMinHitpoints = 30;

        public string UniqueName { get; set; }

        public UrukUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
            Unique = true;

            UniqueName = "Halkot the Uruk";
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Uruk();
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
            return 13;
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
            return 6;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return UniqueName; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "uruks"; } }

        protected override char GetRepresentation()
        {
            return 'R';
        }

        protected override int GetChanceToRecover()
        {
            return 30;
        }

        protected override int GetChanceToRecoverOnBeingHit()
        {
            return 60;
        }

        protected override int GetChanceToFlee()
        {
            return 0;
        }

        protected override int GetMaxHPWillFlee()
        {
            return MaxHitpoints / 2;
        }
        public override int CreatureCost()
        {
            return 50;
        }

        public override int CreatureLevel()
        {
            return 4;
        }


        public override Color RepresentationColor()
        {
            return ColorPresets.Lime;
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
            return 30;
        }

        public override int GetCharmRes()
        {
            return 40;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }
    }
}
