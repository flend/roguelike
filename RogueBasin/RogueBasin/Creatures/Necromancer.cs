using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Clever, fast, raiser. Long range. Bad news!
    /// </summary>
    public class Necromancer : MonsterSpecialAI
    {
        const int classDeltaHitpoints = 8;
        const int classMinHitpoints = 10;


        public Necromancer()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Necromancer();
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
            return 2;
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
            return 8;
        }

        protected override int GetUseSpecialChance()
        {
            return 50;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "necromancer"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "necromancers"; } }

        protected override char GetRepresentation()
        {
            return 'N';
        }

        protected override SpecialAIType GetSpecialAIType()
        {
            return SpecialAIType.Raiser;
        }

        protected override int RelaxDirectionAt()
        {
            return 100;
        }

        protected override int GetTotalFleeLoops()
        {
            return 100;
        }

        public override double GetMissileRange()
        {
            return 4.5;
        }

        protected override string GetWeaponName()
        {
            return "launches a dark bolt of the void";
        }

        protected override bool RaiseCorpse(int level, Point locationMap)
        {
            bool raisedSuccess;

            if (Game.Random.Next(10) < 5)
            {
                raisedSuccess = Game.Dungeon.AddMonsterDynamic(new Creatures.Zombie(), level, locationMap);
            }
            else
                raisedSuccess = Game.Dungeon.AddMonsterDynamic(new Creatures.Skeleton(), level, locationMap);

            return raisedSuccess;
        }

        public override int CreatureCost()
        {
            return 150;
        }

        public override int CreatureLevel()
        {
            return 5;
        }


        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.Gray;
        }

        public override int GetMagicXP()
        {
            return 80;
        }

        public override int GetCombatXP()
        {
            return 60;
        }

        public override int GetMagicRes()
        {
            return 40;
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
