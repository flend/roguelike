using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Rotating turret. Can't move, but will attack when in player in FOV.
    /// </summary>
    public class RotatingTurret : MonsterThrowAndRunAI
    {
        const int classDeltaHitpoints = 5;
        const int classMinHitpoints = 5;

        public RotatingTurret()
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
            return new RotatingTurret();
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
            return 5.0;
        }

        protected override int GetChanceToBackAway()
        {
            return 0;
        }

        public override bool CanMove()
        {
            return false;
        }

        //Can't alter its rotation to lock-on
        protected override bool WillPursue()
        {
            return false;
        }

        //Will always rotate, even if engaged
        protected override bool WillAlwaysPatrol()
        {
            return true;
        }

        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Triangular;
        }

        protected override PatrolType GetPatrolType()
        {
            return PatrolType.Rotate;
        }

        protected override double GetPatrolRotationAngle()
        {
            return Math.PI / 4;
        }

        protected override int GetPatrolRotationSpeed()
        {
            return 1;
        }

        protected override string GetWeaponName()
        {
            return "shoots a laser";
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "rotating turret"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "rotating turrets"; } }

        protected override char GetRepresentation()
        {
            return 'R';
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