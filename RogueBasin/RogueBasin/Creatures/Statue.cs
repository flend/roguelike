using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    public class Statue : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 25;
        const int classMinHitpoints = 15;

        public Statue()
        {
            Speed = 60;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Statue();
        }

        public override int BaseSpeed()
        {
            return 60;
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
            return 14;
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
        public override int DamageModifier()
        {
            return 1;
        }

        public override int HitModifier()
        {
            return 5;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "statue"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "statues"; } }

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
            return 60;
        }

        public override int CreatureLevel()
        {
            return 5;
        }

        public override Color CreatureColor()
        {
            return ColorPresets.RosyBrown;
        }

        public override int GetMagicXP()
        {
            return 60;
        }

        public override int GetCombatXP()
        {
            return 60;
        }

        public override int GetMagicRes()
        {
            return 30;
        }

        public override int GetCharmRes()
        {
            return 0;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }

        protected override bool WakesOnSight()
        {
            return false;
        }
    }
}
