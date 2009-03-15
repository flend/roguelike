using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    public class Lich : MonsterLichAI
    {
        const int classMaxHitpoints = 100;

        public Lich()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
            NormalSightRadius = 0;
            /*
            if (Game.Dungeon.Difficulty == GameDifficulty.Easy)
            {
                MaxSummons = 8;
            }
           else if (Game.Dungeon.Difficulty == GameDifficulty.Medium)
            {
                MaxSummons = 12;
            }
            else
            {
                MaxSummons = 20;
            }*/
            MaxSummons = 8;

            Speed = 150;
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        protected override int ClassMaxHitpoints()
        {
            return 50;
            /*
            if (Game.Dungeon.Difficulty == GameDifficulty.Easy)
            {
                return 50;
            }
            else if (Game.Dungeon.Difficulty == GameDifficulty.Medium)
            {
                return 100;
            }
            else
            {
                return 150;
            }*/
        }

        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return 15;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 4;
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
            return 12;
        }
        
        protected override double GetMissileRange()
        {
            return 5;
        }

        protected override int RelaxDirectionAt()
        {
            return 100;
        }

        protected override int GetTotalFleeLoops()
        {
            return 500;
        }

        protected override string GetWeaponName()
        {
            return "launches a dark sphere of the void";
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "lich"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "liches"; } }



        protected override char GetRepresentation()
        {
            return 'L';
        }

        public override int CreatureCost()
        {
            return 1;
        }

        public override int CreatureLevel()
        {
            return 1;
        }


        public override Color CreatureColor()
        {
            return ColorPresets.Yellow;
        }
    }
}
