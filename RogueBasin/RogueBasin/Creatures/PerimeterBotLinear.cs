using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Patrolling Robot. Will not break off patrol but will fire at enemies within FOV
    /// </summary>
    public class PerimeterBotLinear : MonsterThrowAndRunAI
    {
        const int classDeltaHitpoints = 4;
        const int classMinHitpoints = 1;

        bool rotationClockwise = true;

        public PerimeterBotLinear()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            NormalSightRadius = 4;

            //Randomize which way we rotate (should be serialized)
            if (Game.Random.Next(2) > 0)
            {
                rotationClockwise = false;
            }
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
            return 5;
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
            return 0;
        }

        /// <summary>
        /// Will not leave patrol route.
        /// </summary>
        /// <returns></returns>
        protected override bool WillPursue()
        {
            return false;
        }

        /// <summary>
        /// If true, will not leave route even when firing
        /// </summary>
        /// <returns></returns>
        protected override bool WillAlwaysPatrol()
        {
            return false;
        }

        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Triangular;
        }

        public override PatrolType GetPatrolType()
        {
            return PatrolType.Waypoints;
        }

        public override bool GetPatrolRotationClockwise()
        {
            return rotationClockwise;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "Perimeter bot"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Perimeter bots"; } }

        protected override char GetRepresentation()
        {
            return 'r';
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
            return 10;
        }

        public override int CreatureLevel()
        {
            return 1;
        }

        protected override int GetChanceToBackAway()
        {
            return 0;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new PerimeterBotLinear();
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.Tomato;
        }

        public override int GetCombatXP()
        {
            return 10;
        }

        public override int GetMagicXP()
        {
            return 10;
        }

        public override int GetMagicRes()
        {
            return 0;
        }

        public override int GetCharmRes()
        {
            return 5;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }

        protected override double GetMissileRange()
        {
            return 5.0;
        }

        protected override string GetWeaponName()
        {
            return "fires a railgun";
        }

        //Can open doors on patrol
        public override bool CanOpenDoors()
        {
            return true;
        }

        public override bool HasSquarePatrol()
        {
            return false;
        }


    }
}
