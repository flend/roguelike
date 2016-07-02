using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{

    public class ExplosiveBarrel : MonsterNullAI
    {
        int level;

        public ExplosiveBarrel(int level)
        {
            this.level = level;
            this.Passive = true;
            this.UnpassifyOnAttacked = false;
            this.WakesOnAttacked = false;
        }

        /// <summary>
        /// Bombs explode as grenades
        /// </summary>
        protected void Explode()
        {
            Game.Dungeon.AddSoundEffect(0.5, LocationLevel, LocationMap);

            Game.Dungeon.DoGrenadeExplosion(LocationLevel, LocationMap, 2.5, this.ScaleRangedDamage(DamageBase()), this);
        }

        public override Monster NewCreatureOfThisType()
        {
            return new ExplosiveBarrel(Level);
        }

        public override Feature GenerateCorpse()
        {
            return null;
        }

        protected override int ClassMaxHitpoints()
        {
            return 20;
        }

        public override int DamageBase()
        {
            return 20;
        }

        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Base;
        }


        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "Explosive Barrel"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Explosive Barrels"; } }

        protected override char GetRepresentation()
        {
            return (char)304;
        }

        internal override char GetCorpseRepresentation()
        {
            return (char)569;
        }

        internal override System.Drawing.Color GetCorpseRepresentationColour()
        {
            return System.Drawing.Color.DarkRed;
        }


        public override int CreatureCost()
        {
            return 10;
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
            return 0;
        }

        internal override void OnKilledSpecialEffects()
        {
            Explode();
        }

        public override int DropChance()
        {
            return 0;
        }

        public override bool CanMove()
        {
            return false;
        }

        protected override string GetGameSprite()
        {
            return "barrel";
        }

        protected override string GetUISprite()
        {
            return "barrel";
        }

    }
}
