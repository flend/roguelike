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

        public RotatingTurret()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));

            NormalSightRadius = 10;
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
            return 40;
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
            return 15;
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
            return 10.0;
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

        public override PatrolType GetPatrolType()
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

        /// <summary>
        /// Set to false to ignore sounds. Can't move already ignore sounds
        /// </summary>
        /// <returns></returns>
        protected override bool WillInvestigateSounds()
        {
            return false;
        }

        protected override string GetWeaponName()
        {
            return "shoots a laser";
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "Rotating turret"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Rotating turrets"; } }

        protected override char GetRepresentation()
        {
            return (char)320;
        }

        internal override char GetCorpseRepresentation()
        {
            return (char)322;
        }

        internal override System.Drawing.Color GetCorpseRepresentationColour()
        {
            return System.Drawing.Color.DarkRed;
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
            return System.Drawing.Color.OrangeRed;
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
