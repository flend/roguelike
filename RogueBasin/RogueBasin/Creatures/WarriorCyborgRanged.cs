using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{

    public class WarriorCyborgRanged : MonsterThrowAndRunAI
    {
        public WarriorCyborgRanged()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            NormalSightRadius = 8;

        }

        protected override int ClassMaxHitpoints()
        {
            return 100;
        }

        public override int DamageBase()
        {
            return 20;
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
            return true;
        }

        protected override bool WillPursue()
        {
            return true;
        }

        protected override double GetMissileRange()
        {
            return 7.0;
        }

        protected override int GetChanceToBackAway()
        {
            return 0;
        }

        protected override string GetWeaponName()
        {
            return "fires a carbine";
        }

        public override bool CanOpenDoors()
        {
            return true;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "Warrior Cyborg"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Warrior Cyborgs"; } }

        protected override char GetRepresentation()
        {
            return (char)627;
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

        public override Monster NewCreatureOfThisType()
        {
            return new WarriorCyborgRanged();
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.Chartreuse;
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
