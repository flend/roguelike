using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Chasing monster that doesn't attack and can be used as a shield???
    /// </summary>
    public class Drone : MonsterThrowAndRunAI
    {
        const int classDeltaHitpoints = 5;
        const int classMinHitpoints = 5;

        public Drone()
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
            return new Drone();
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
        public override double DamageModifier()
        {
            return 0;
        }

        public override int HitModifier()
        {
            return 1;
        }

        public override double GetMissileRange()
        {
            return 3.0;
        }

        protected override int GetChanceToBackAway()
        {
            return 0;
        }

        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Triangular;
        }

        public override PatrolType GetPatrolType()
        {
            return PatrolType.RandomWalk;
        }

        protected override bool WillAttack()
        {
            return false;
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
            return 'd';
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

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.DarkGreen;
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
