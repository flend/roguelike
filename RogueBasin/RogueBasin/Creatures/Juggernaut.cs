using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Patrolling Robot. Linear patrol.
    /// Won't chase. Attack at medium range.
    /// Doesn't respond to sounds
    /// </summary>
    public class Juggernaut : MonsterThrowAndRunAI
    {
        bool rotationClockwise = true;

        public Juggernaut()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            NormalSightRadius = 8;

            //Randomize which way we rotate (should be serialized)
            if (Game.Random.Next(2) > 0)
            {
                rotationClockwise = false;
            }
        }

        protected override int ClassMaxHitpoints()
        {
            return 15;
        }

        public override int DamageBase()
        {
            return 2;
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

        protected override bool WillInvestigateSounds()
        {
            return false;
        }

        protected override bool WillPursue()
        {
            return false;
        }

        protected override double GetMissileRange()
        {
            return 5.0;
        }

        protected override int GetChanceToBackAway()
        {
            return 0;
        }

        protected override string GetWeaponName()
        {
            return "fires a cannon";
        }

        public override bool CanOpenDoors()
        {
            return true;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "Juggernaut"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Juggernaut"; } }

        protected override char GetRepresentation()
        {
            return (char)265;
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
            return 40;
        }

        public override int CreatureLevel()
        {
            return 1;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Juggernaut();
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.Fuchsia;
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

        

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return 5;
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
    }
}
