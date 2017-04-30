﻿namespace RogueBasin.Creatures
{
    /// <summary>
    /// Passive target for player.
    /// Just sits there
    /// </summary>
    /// 
    public class ComputerNode : MonsterNullAI
    {

        public ComputerNode()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            NormalSightRadius = 0;

            Unique = true;

            //Always passive
            Passive = true;
            UnpassifyOnAttacked = false;
            WakesOnAttacked = false;
        }

        protected override int ClassMaxHitpoints()
        {
            return 10;
        }

        public override int DamageBase()
        {
            return 0;
        }


        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Base;
        }


        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "Computer Node"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Computer Nodes"; } }

        protected override char GetRepresentation()
        {
            return (char)268;
        }


        public override int CreatureCost()
        {
            return 10;
        }

        public override int CreatureLevel()
        {
            return 1;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new ComputerNode();
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.WhiteSmoke;
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
            return 0;
        }

        public override int GetCharmRes()
        {
            return 5;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }

        

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return 5;
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

        public override string QuestId
        {
            get
            {
                return "computer-node";
            }
        }
    }
}
