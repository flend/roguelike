using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Fast
    /// </summary>
    public class Bat : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 4;
        const int classMinHitpoints = 4;

        public Bat()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override int BaseSpeed()
        {
            return 120;
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
            return 8;
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
            return 0;
        }

        public override int HitModifier()
        {
            return 2;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "bat"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "bats"; } }

        protected override char GetRepresentation()
        {
            return 'B';
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
            return 25;
        }

        public override int CreatureLevel()
        {
            return 2;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Bat();
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.Gold;
        }

        public override int GetCombatXP()
        {
            return 30;
        }

        public override int GetMagicXP()
        {
            return 30;
        }

        public override int GetMagicRes()
        {
            return 0;
        }

        public override int GetCharmRes()
        {
            return 25;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }
    }
}
