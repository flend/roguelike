using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{

    public class Captain : MonsterFightAndRunAI
    {
        public Captain()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            NormalSightRadius = 8;

        }

        protected override int ClassMaxHitpoints()
        {
            return 400;
        }

        public override int DamageBase()
        {
            return 30;
        }

        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Base;
        }

        public override PatrolType GetPatrolType()
        {
            return PatrolType.Rotate;
        }

        protected override bool WillInvestigateSounds()
        {
            return false;
        }

        protected override bool WillPursue()
        {
            return true;
        }

        public override bool CanOpenDoors()
        {
            return true;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "The Captain"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "The Captains"; } }

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
            return 500;
        }

        public override int CreatureLevel()
        {
            return 1;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Captain();
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.Magenta;
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

        internal override char GetCorpseRepresentation()
        {
            return (char)501;
        }

        internal override System.Drawing.Color GetCorpseRepresentationColour()
        {
            return System.Drawing.Color.DarkRed;
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
        public override double DamageModifier()
        {
            return 0;
        }

        public override int HitModifier()
        {
            return 0;
        }

        public override string QuestId
        {
            get
            {
                return "captain";
            }
        }
    }
}
