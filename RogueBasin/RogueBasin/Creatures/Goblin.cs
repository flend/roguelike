using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Low threat. Stupid missile troop.
    /// </summary>
    public class Goblin : MonsterThrowAndRunAI
    {
        const int classDeltaHitpoints = 5;
        const int classMinHitpoints = 5;

        public Goblin()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Goblin();
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
            return 7;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 2;
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
            return 1;
        }

        protected override double GetMissileRange()
        {
            return 3.0;
        }

        protected override int GetChanceToBackAway()
        {
            return 0;
        }

        protected override string GetWeaponName()
        {
            return "throws a dagger";
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "goblin"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription  { get { return "goblins"; } }

        protected override char GetRepresentation()
        {
            return 'g';
        }

        protected override int RelaxDirectionAt()
        {
            return 0;
        }

        protected override int GetTotalFleeLoops()
        {
            return 5;
        }
        public override int CreatureCost()
        {
            return 20;
        }

        public override int CreatureLevel()
        {
            return 2;
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.DarkGreen;
        }

        public override int GetMagicXP()
        {
            return 20;
        }

        public override int GetCombatXP()
        {
            return 20;
        }

        public override int GetMagicRes()
        {
            return 0;
        }

        public override int GetCharmRes()
        {
            return 10;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }
    }
}