using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    public class Bugbear : MonsterThrowAndRunAI
    {
        const int classDeltaHitpoints = 5;
        const int classMinHitpoints = 10;

        public Bugbear()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Bugbear();
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

        protected override double GetMissileRange()
        {
            return 3;
        }

        protected override string GetWeaponName()
        {
            return "throws a knife";
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "bugbear"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "bugbears"; } }

        protected override char GetRepresentation()
        {
            return 'b';
        }

        protected override int RelaxDirectionAt()
        {
            return 0;
        }

        protected override int GetTotalFleeLoops()
        {
            return 10;
        }
        public override int CreatureCost()
        {
            return 30;
        }

        public override int CreatureLevel()
        {
            return 3;
        }

        public override Color CreatureColor()
        {
            return ColorPresets.DarkGreen;
        }

        public override int GetMagicXP()
        {
            return 30;
        }

        public override int GetCombatXP()
        {
            return 30;
        }

        public override int GetMagicRes()
        {
            return 0;
        }

        public override int GetCharmRes()
        {
            return 20;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }
    }
}