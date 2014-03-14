using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Swarmer. Light melee with wide FOV. Responds to sounds.
    /// </summary>
    public class ExplosiveBarrel : MonsterNullAI
    {

        public ExplosiveBarrel()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));

            //8 is good for triangular
            NormalSightRadius = 8;
        }

        /// <summary>
        /// Bombs explode as grenades
        /// </summary>
        protected void Explode()
        {
            double size = 4.0;
            int damage = 50;

            //Make explosion sound AT target location
            Game.Dungeon.AddSoundEffect(1, LocationLevel, LocationMap);

            Game.Dungeon.DoGrenadeExplosion(LocationLevel, LocationMap, size, damage, this);
        }



        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new ExplosiveBarrel();
        }

        protected override int ClassMaxHitpoints()
        {
            return 2;
        }

        public override int DamageBase()
        {
            return 4;
        }

        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Triangular;
        }


        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "Explosive Barrel"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Explosive Barrel"; } }

        protected override char GetRepresentation()
        {
            return (char)304;
        }

        public override int CreatureCost()
        {
            return 20;
        }

        public override int CreatureLevel()
        {
            return 3;
        }


        public override Color RepresentationColor()
        {
            return ColorPresets.Gold;
        }

        public override int GetCombatXP()
        {
            return 40;
        }

        public override int GetMagicXP()
        {
            return 40;
        }

        public override int GetMagicRes()
        {
            return 0;
        }

        public override int GetCharmRes()
        {
            return 30;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }


        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return 12;
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
            return 3;
        }

        internal override void OnKilledSpecialEffects()
        {
            Explode();
        }

    }
}
