using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Creatures
{
    public class SkeletalArcher : MonsterThrowAndRunAI
    {
        const int classDeltaHitpoints = 20;
        const int classMinHitpoints = 5;

        public SkeletalArcher()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
            Speed = 100;
        }
        public override Monster NewCreatureOfThisType()
        {
            return new SkeletalArcher();
        }

        protected override double GetMissileRange()
        {
            return 3;
        }

        protected override string GetWeaponName()
        {
            return "shoots an arrow";
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
            return 1;
        }

        public override int HitModifier()
        {
            return 3;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "skeletal archer"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "skeletal archers"; } }

        protected override char GetRepresentation()
        {
            return 'A';
        }

        public override int CreatureCost()
        {
            return 35;
        }

        public override int CreatureLevel()
        {
            return 4;
        }

        public override int GetMagicXP()
        {
            return 50;
        }

        public override int GetCombatXP()
        {
            return 50;
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
