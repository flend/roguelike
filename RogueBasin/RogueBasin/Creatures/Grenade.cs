using System;using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Swarmer. Light melee with wide FOV. Responds to sounds.
    /// </summary>
    public class Grenade : MonsterThrowAndRunAI
    {
        int damage;
        int timer;
        bool exploded = false;
        private int p1;
        private double p2;
        double range;

        public Grenade(int damage, int timer, double range)
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));

            NormalSightRadius = 100;

            Sleeping = false;

            this.damage = damage;
            this.timer = timer;
            this.range = range;
        }

        protected override void SetupAnimationForObject()
        {
            AnimationDelayMS = 250;
            HasAnimation = true;
            NumberOfFrames = 2;
        }

        protected override bool UseSpecialAbility()
        {
            timer--;

            LogFile.Log.LogEntryDebug("Grenade on timer: " + this.timer, LogDebugLevel.Medium);
            //Avoid exploding if we have already been killed
            if (timer == 0 && Alive)
            {
                Explode();
            }

            //Always use this rather than shoot
            return true;
        }


        private void Explode() {

            //Grenade explosion can cause a chain reaction between 2 grenades, so we set the flag first
            if (!exploded)
            {
                exploded = true;
                Hitpoints = 0;

                LogFile.Log.LogEntryDebug("Grenade explosion at: " + this.LocationMap, LogDebugLevel.Medium);

                Game.Dungeon.AddSoundEffect(0.5, LocationLevel, LocationMap);

                Game.Dungeon.DoGrenadeExplosion(LocationLevel, LocationMap, range, damage, this);

                SoundPlayer.Instance().EnqueueSound("explosion");
            }
        }
        /// <summary>
        /// Only explodes when next to you
        /// </summary>
        /// <returns></returns>
        public override double GetMissileRange()
        {
            //Explodes if anyone is even vaguely nearby
            return 1000;
        }

        protected override string GetWeaponName()
        {
            return "explodes!";
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Grenade(damage, timer, range);
        }

        protected override int ClassMaxHitpoints()
        {
            return 10;
        }

        public override int DamageBase()
        {
            return 4;
        }

        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Base;
        }

        public override PatrolType GetPatrolType()
        {
            return PatrolType.Rotate;
        }

        //Rotation makes them look alive (even though they have base FOV)
        protected override double GetPatrolRotationAngle()
        {
            return Math.PI / 2;
        }

        protected override int GetPatrolRotationSpeed()
        {
            return 2;
        }

        public override bool CanMove()
        {
            return false;
        }
        protected override bool WillInvestigateSounds()
        {
            return false;
        }

        protected override bool WillPursue()
        {
            return false;
        }

        //Makes them more effective swarmers
        public override bool CanOpenDoors()
        {
            return false;
        }


        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "Grenade"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Grenades"; } }

        protected override char GetRepresentation()
        {
            return 'g';
        }

        internal override char GetCorpseRepresentation()
        {
            return (char)503;
        }

        internal override System.Drawing.Color GetCorpseRepresentationColour()
        {
            return System.Drawing.Color.DarkRed;
        }

        protected override int GetChanceToRecover()
        {
            return 20;
        }

        protected override int GetChanceToRecoverOnBeingHit()
        {
            return 50;
        }

        protected override int GetChanceToFlee()
        {
            return 0;
        }

        protected override int GetMaxHPWillFlee()
        {
            return 8;
        }
        public override int CreatureCost()
        {
            return 30;
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
        public override double DamageModifier()
        {
            return 1;
        }

        public override int HitModifier()
        {
            return 3;
        }

        public override int DropChance()
        {
            return 0;
        }

        protected override string GetGameSprite()
        {
            return "livegrenade";
        }

        protected override string GetUISprite()
        {
            return "livegrenade";
        }

        internal override void OnKilledSpecialEffects()
        {
            if(!exploded)
                Explode();
        }
    }
}
