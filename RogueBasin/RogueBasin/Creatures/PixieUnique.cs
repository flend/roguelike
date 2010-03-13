using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Medium threat. Fast but weak missile.
    /// </summary>
    public class PixieUnique : MonsterThrowAndRunAI
    {
        const int classDeltaHitpoints = 10;
        const int classMinHitpoints = 30;

        public string UniqueName { get; set; }

        public PixieUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
            Speed = 200;

            Unique = true;
            UniqueName = "Nixie the Pixie";
        }

        public override Monster NewCreatureOfThisType()
        {
            return new PixieUnique();
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
            return 3;
        }

        protected override double GetMissileRange()
        {
            return 5.5;
        }

        protected override string GetWeaponName()
        {
            return "throws a dart";
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "pixie"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription  { get { return "pixies"; } }

        protected override char GetRepresentation()
        {
            return 'p';
        }

        protected override int RelaxDirectionAt()
        {
            return 5;
        }

        protected override int GetTotalFleeLoops()
        {
            return 20;
        }
        public override int CreatureCost()
        {
            return 35;
        }

        public override int CreatureLevel()
        {
            return 2;
        }

        public override Color CreatureColor()
        {
            return ColorPresets.HotPink;
        }

        public override int GetMagicXP()
        {
            return 80;
        }

        public override int GetCombatXP()
        {
            return 80;
        }

        public override int GetMagicRes()
        {
            return 30;
        }

        public override int GetCharmRes()
        {
            return 50;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }
    }
}