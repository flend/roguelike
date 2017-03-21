﻿using System;using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{

    public class RollingBomb : MonsterThrowAndRunAI
    {

        public RollingBomb()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));

            NormalSightRadius = 8;
        }

        protected override bool UseSpecialAbility()
        {
            Explode();

            //true to avoid 2 explosions!
            Game.Dungeon.KillMonster(this, true);
            return true;
        }

        private void Explode() {

            Game.Dungeon.AddSoundEffect(1.0, LocationLevel, LocationMap);

            int damage = 50;

            if (this.LocationLevel >= 6)
                damage *= 2;

            Game.Dungeon.Combat.DoGrenadeExplosion(LocationLevel, LocationMap, 3.0, damage, this);
        }
        /// <summary>
        /// Only explodes when next to you
        /// </summary>
        /// <returns></returns>
        public override double GetMissileRange()
        {
            //Can explode diagonally!
            return 1.9;
        }

        protected override string GetWeaponName()
        {
            return "explodes!";
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new RollingBomb();
        }

        protected override int ClassMaxHitpoints()
        {
            return 20;
        }

        public override int DamageBase()
        {
            return 4;
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
        public override string SingleDescription { get { return "Rolling Bomb"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Rolling Bombs"; } }

        protected override char GetRepresentation()
        {
            return (char)266;
        }

        internal override char GetCorpseRepresentation()
        {
            return (char)503;
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
            return 30;
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

        internal override void OnKilledSpecialEffects()
        {
            Explode();
        }

        public override int DropChance()
        {
            return 0;
        }
    }
}
