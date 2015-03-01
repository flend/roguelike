using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Passive target for player.
    /// Just sits there
    /// </summary>
    /// 
    public class Camera : MonsterNullAI
    {
        public Camera()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            NormalSightRadius = 0;

            //Start in the passive state
            this.Passive = true;
        }

        protected override int ClassMaxHitpoints()
        {
            return 2;
        }

        public override int DamageBase()
        {
            return 0;
        }


        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Triangular;
        }


        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "Camera"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Cameras"; } }

        protected override char GetRepresentation()
        {
            return (char)271;
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
            return new Camera();
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.PaleVioletRed;
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
        public override int DamageModifier()
        {
            return 0;
        }

        public override int HitModifier()
        {
            return 0;
        }

        internal override char GetCorpseRepresentation()
        {
            return (char)270;
        }

        internal override System.Drawing.Color GetCorpseRepresentationColour()
        {
            return System.Drawing.Color.DarkRed;
        }

        internal override void OnKilledSpecialEffects()
        {
            //Game.MessageQueue.AddMessage("Level security now: " + Game.Dungeon.CalculateLevelSecurity(LocationLevel) + "%%");
        }
    }
}
