using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Low threat, sleeps until attacked, can't move
    /// </summary>
    public class Mushroom : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 10;
        const int classMinHitpoints = 5;

        public Mushroom()
        {
            NormalSightRadius = 4;
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
        public override double DamageModifier()
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
        public override string SingleDescription { get { return "mushroom"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "mushrooms"; } }

        protected override char GetRepresentation()
        {
            return 'm';
        }

        public override int CreatureCost()
        {
            return 5;
        }

        public override int CreatureLevel()
        {
            return 1;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Mushroom();
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.Peru;
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
            return 10;
        }

        public override bool CanMove()
        {
            return false;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }
    }
}
