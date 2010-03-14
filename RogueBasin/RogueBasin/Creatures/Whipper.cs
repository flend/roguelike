using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    public class Whipper : MonsterThrowAndRunAI
    {
        const int classDeltaHitpoints = 15;
        const int classMinHitpoints = 10;

        public Whipper()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Whipper();
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
            return 8;
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
            return 5;
        }

        protected override double GetMissileRange()
        {
            return 4;
        }

        protected override string GetWeaponName()
        {
            return "lashes out with a tentacle";
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "whipper"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "whippers"; } }

        protected override char GetRepresentation()
        {
            return 'W';
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
            return 70;
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
            return 70;
        }

        public override int GetCombatXP()
        {
            return 70;
        }

        public override int GetMagicRes()
        {
            return 40;
        }

        public override int GetCharmRes()
        {
            return 80;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }
    }
}