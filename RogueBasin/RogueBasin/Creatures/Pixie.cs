using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Medium threat. Fast but weak missile.
    /// </summary>
    public class Pixie : MonsterThrowAndRunAI
    {
        const int classDeltaHitpoints = 2;
        const int classMinHitpoints = 6;

        public Pixie()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Pixie();
        }

        public override int BaseSpeed()
        {
            return 150;
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
            return 12;
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
            return 3;
        }

        protected override double GetMissileRange()
        {
            return 4.5;
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
            return 15;
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

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.HotPink;
        }

        public override int GetMagicXP()
        {
            return 60;
        }

        public override int GetCombatXP()
        {
            return 40;
        }

        public override int GetMagicRes()
        {
            return 35;
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
