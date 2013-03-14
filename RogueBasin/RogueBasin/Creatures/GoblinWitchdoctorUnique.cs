using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Raiser. Stupid.
    /// </summary>
    public class GoblinWitchdoctorUnique : MonsterSpecialAI
    {
        const int classDeltaHitpoints = 15;
        const int classMinHitpoints = 25;

        public string UniqueName { get; set; }

        public GoblinWitchdoctorUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            Unique = true;
            UniqueName = "Damastrals the Goblin Witchdoctor";
        }

        public override Monster NewCreatureOfThisType()
        {
            return new GoblinWitchdoctorUnique();
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
            return 8;
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
            return 1;
        }

        protected override int GetUseSpecialChance()
        {
            return 100;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return UniqueName; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "goblin witch"; } }

        protected override char GetRepresentation()
        {
            return 'G';
        }

        protected override SpecialAIType GetSpecialAIType()
        {
            return SpecialAIType.Raiser;
        }

        protected override int RelaxDirectionAt()
        {
            return 0;
        }

        protected override int GetTotalFleeLoops()
        {
            return 10;
        }

        protected override double GetMissileRange()
        {
            return 3.5;
        }

        protected override string GetWeaponName()
        {
            return "fires a crackling bolt of energy";
        }

        protected override bool RaiseCorpse(int level, Point locationMap)
        {
            bool raisedSuccess;

            if (Game.Random.Next(10) < 6)
            {
                raisedSuccess = Game.Dungeon.AddMonsterDynamic(new Creatures.Ferret(), level, locationMap);
            }
            else
                raisedSuccess = Game.Dungeon.AddMonsterDynamic(new Creatures.Goblin(), level, locationMap);

            return raisedSuccess;
        }
        
        public override int CreatureCost()
        {
            return 70;
        }

        public override int CreatureLevel()
        {
            return 3;
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.Green;
        }
        
        public override int GetCombatXP()
        {
            return 60;
        }

        public override int GetMagicXP()
        {
            return 60;
        }

        public override int GetMagicRes()
        {
            return 10;
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
