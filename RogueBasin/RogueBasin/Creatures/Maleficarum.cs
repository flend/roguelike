﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Tough mid range demon
    /// </summary>
    public class Maleficarum : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 15;
        const int classMinHitpoints = 25;

        public Maleficarum()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
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
            return 16;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 10;
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
            return 7;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "maleficarum"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "maleficarum"; } }

        protected override char GetRepresentation()
        {
            return 'M';
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
            return 200;
        }

        public override int CreatureLevel()
        {
            return 5;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Creatures.Maleficarum();
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.Crimson;
        }

        public override int GetCombatXP()
        {
            return 100;
        }

        public override int GetMagicXP()
        {
            return 100;
        }

        public override int GetMagicRes()
        {
            return 60;
        }

        public override int GetCharmRes()
        {
            return 90;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }
    }
}
