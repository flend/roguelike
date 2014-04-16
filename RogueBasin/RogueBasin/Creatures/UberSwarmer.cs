using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Swarmer. Light melee with wide FOV. Responds to sounds.
    /// </summary>
    public class UberSwarmer : MonsterFightAndRunAI
    {

        public UberSwarmer()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));

            //More fun to move these guys around with a lower radius
            NormalSightRadius = 5;

            //Fast though
            Speed = 200;
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new UberSwarmer();
        }

        protected override int ClassMaxHitpoints()
        {
            return 40; //Not a 1-hit kill
        }

        public override int DamageBase()
        {
            return 10;
        }

        public override Pathing.PathingType PathingType()
        {
            return Pathing.PathingType.CreaturePass;
        }

        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Base;
        }

        public override PatrolType GetPatrolType()
        {
            return PatrolType.Rotate;
        }

        //Rotation makes them look alive (even though they have base FOV)
        protected override double GetPatrolRotationAngle()
        {
            return Math.PI / 2;
        }

        protected override int GetPatrolRotationSpeed()
        {
            return 2;
        }


        protected override bool WillInvestigateSounds()
        {
            return true;
        }

        protected override bool WillPursue()
        {
            return true;
        }

        //Makes them more effective swarmers
        public override bool CanOpenDoors()
        {
            return true;
        }


        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "UberSwarmer"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "UberSwarmers"; } }

        protected override char GetRepresentation()
        {
            return (char)261;
        }

        internal override char GetCorpseRepresentation()
        {
            return (char)498;
        }

        internal override Color GetCorpseRepresentationColour()
        {
            return ColorPresets.DarkRed;
        }

        protected override int GetChanceToRecover()
        {
            return 20;
        }

        protected override int GetChanceToRecoverOnBeingHit()
        {
            return 50;
        }

        protected override int GetChanceToFlee()
        {
            return 0;
        }

        protected override int GetMaxHPWillFlee()
        {
            return 8;
        }
        public override int CreatureCost()
        {
            return 40;
        }

        public override int CreatureLevel()
        {
            return 3;
        }


        public override Color RepresentationColor()
        {
            return ColorPresets.DeepPink;
        }

        public override int GetCombatXP()
        {
            return 40;
        }

        public override int GetMagicXP()
        {
            return 40;
        }

        public override int GetMagicRes()
        {
            return 0;
        }

        public override int GetCharmRes()
        {
            return 30;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }


        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return 12;
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

    }
}
