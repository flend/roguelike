using System;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Swarmer. Light melee with wide FOV. Responds to sounds.
    /// </summary>
    public class MaintBot : MonsterFightAndRunAI
    {

        public MaintBot()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));

            //Start in the passive state
            this.Passive = true;
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new MaintBot();
        }

        protected override int ClassMaxHitpoints()
        {
            return 20; //Not a 1-hit kill
        }

        public override int DamageBase()
        {
            return 10;
        }

        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Base;
        }

        public override PatrolType GetPatrolType()
        {
            return PatrolType.RandomWalk;
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
            return false;
        }

        protected override bool WillPursue()
        {
            return false;
        }

        //Makes them more effective swarmers
        public override bool CanOpenDoors()
        {
            return false;
        }


        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "Maint Bot"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Maint Bots"; } }

        protected override char GetRepresentation()
        {
            return (char)258;
        }

        internal override char GetCorpseRepresentation()
        {
            return (char)496;
        }

        internal override System.Drawing.Color GetCorpseRepresentationColour()
        {
            return System.Drawing.Color.DarkRed;
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
            return 20;
        }

        public override int CreatureLevel()
        {
            return 3;
        }


        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.Gold;
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
        public override double DamageModifier()
        {
            return 1;
        }

        public override int HitModifier()
        {
            return 3;
        }

    }
}
