using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Tough mid range demon
    /// </summary>
    public class Peon : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 10;
        const int classMinHitpoints = 5;

        public Peon()
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
            return 10;
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
        public override string SingleDescription { get { return "peon"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "peons"; } }

        protected override char GetRepresentation()
        {
            return 'P';
        }

        protected override int GetChanceToRecover()
        {
            return 50;
        }

        protected override int GetChanceToFlee()
        {
            return 50;
        }

        protected override int GetMaxHPWillFlee()
        {
            return Hitpoints / 2;
        }

        public override int CreatureCost()
        {
            return 35;
        }

        public override int CreatureLevel()
        {
            return 3;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Peon();
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.GreenYellow;
        }

        public override int GetCombatXP()
        {
            return 80;
        }

        public override int GetMagicXP()
        {
            return 40;
        }

        public override int GetMagicRes()
        {
            return 20;
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
