using System;using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    public class Mine : MonsterThrowAndRunAI
    {
        int damage;
        bool exploded = false;

        public Mine(int damage)
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));

            NormalSightRadius = 1000;

            Sleeping = false;
           
            this.damage = damage;
        }

        protected override bool UseSpecialAbility()
        {
            //Avoid exploding if we have already been killed
            if (Alive)
            {
                var adjacentSquares = Game.Dungeon.GetWalkableAdjacentSquares(this.LocationLevel, this.LocationMap);

                bool willExplode = false;
                foreach (Point sq in adjacentSquares)
                {
                    //Check square has nothing else on it
                    SquareContents contents = Game.Dungeon.MapSquareContents(this.LocationLevel, sq);

                    if (contents.monster != null)
                    {
                        willExplode = true;
                        break;
                    }
                }

                if(willExplode)
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

                LogFile.Log.LogEntryDebug("Mine explosion at: " + this.LocationMap, LogDebugLevel.Medium);

                Game.Dungeon.AddSoundEffect(1.0, LocationLevel, LocationMap);

                Game.Dungeon.DoGrenadeExplosion(LocationLevel, LocationMap, 3.0, damage, this);

                SoundPlayer.Instance().EnqueueSound("explosion");
            }
        }

        public override Feature GenerateCorpse()
        {
            return null;
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
            return new Mine(damage);
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
        public override string SingleDescription { get { return "Mine"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Mines"; } }

        protected override char GetRepresentation()
        {
            return 'm';
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
            return "mine";
        }

        protected override string GetUISprite()
        {
            return "mine";
        }

        internal override void OnKilledSpecialEffects()
        {
            if(!exploded)
                Explode();
        }
    }
}
