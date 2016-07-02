﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Medium threat, faster than normal
    /// </summary>
    public class Skeleton : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 20;
        const int classMinHitpoints = 5;

        public Skeleton()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Skeleton();
        }

        public override int BaseSpeed()
        {
            return 110;
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
            return 12;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 6;
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

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "skeleton"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "skeleton"; } }

        protected override char GetRepresentation()
        {
            return 'S';
        }

        protected override int GetChanceToRecover()
        {
            return 0;
        }

        protected override int GetChanceToFlee()
        {
            return 0;
        }

        protected override int GetMaxHPWillFlee()
        {
            return 0;
        }
        public override int CreatureCost()
        {
            return 35;
        }

        public override int CreatureLevel()
        {
            return 4;
        }

        public override int GetMagicXP()
        {
            return 50;
        }

        public override int GetCombatXP()
        {
            return 50;
        }

        public override int GetMagicRes()
        {
            return 0;
        }

        public override int GetCharmRes()
        {
            return 0;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }
    }
}
