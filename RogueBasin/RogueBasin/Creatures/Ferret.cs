﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Low threat, runs away easily. Poor eyesight.
    /// </summary>
    public class Ferret : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 5;
        const int classMinHitpoints = 1;

        public Ferret()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
            NormalSightRadius = 4;
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
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "ferret"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "ferrets"; } }

        protected override char GetRepresentation()
        {
            return 'f';
        }

        protected override int GetChanceToRecover()
        {
            return 10;
        }

        protected override int GetChanceToFlee()
        {
            return 75;
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

        public Ferret NewCreatureOfThisType()
        {
            return new Ferret();
        }

        public override Color CreatureColor()
        {
            return ColorPresets.WhiteSmoke;
        }
    }
}