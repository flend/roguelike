using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Medium-hard brawler. Will run
    /// </summary>
    public class Uruk : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 15;
        const int classMinHitpoints = 10;

        public Uruk()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
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
            return 15;
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
        public override double DamageModifier()
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
        public override string SingleDescription { get { return "uruk"; } }

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
            return 20;
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


        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.Lime;
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
            return 10;
        }

        public override int GetCharmRes()
        {
            return 40;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }
    }
}
