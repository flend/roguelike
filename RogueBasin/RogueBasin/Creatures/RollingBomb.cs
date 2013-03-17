using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Swarmer. Light melee with wide FOV. Responds to sounds.
    /// </summary>
    public class RollingBomb : MonsterThrowAndRunAI
    {

        public RollingBomb()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));

            //8 is good for triangular
            NormalSightRadius = 8;
        }

        protected override bool UseSpecialAbility()
        {
            Explode();

            //true to avoid 2 explosions!
            Game.Dungeon.KillMonster(this, true);
            return true;
        }
        /// <summary>
        /// Only explodes when next to you
        /// </summary>
        /// <returns></returns>
        protected override double GetMissileRange()
        {
            return 1.0;
        }

        protected override string GetWeaponName()
        {
            return "explodes!";
        }

        /// <summary>
        /// Bombs explode as grenades
        /// </summary>
        protected void Explode()
        {

            int size = 3;
            int damage = 3;

            //Make explosion sound AT target location
            Game.Dungeon.AddSoundEffect(1, LocationLevel, LocationMap);

            //Work out grenade splash and damage

            List<Point> grenadeAffects = Game.Dungeon.GetPointsForGrenadeTemplate(LocationMap, LocationLevel, size);

            //Draw attack
            Screen.Instance.DrawAreaAttack(grenadeAffects, ColorPresets.Chocolate);

            //Attack all monsters in the area

            foreach (Point sq in grenadeAffects)
            {
                SquareContents squareContents = Game.Dungeon.MapSquareContents(LocationLevel, sq);

                Monster m = squareContents.monster;

                //Don't attack ourself -will loop
                if (m == this)
                    continue;

                if (m!=null && !m.Alive)
                    continue;

                //Hit the monster if it's there
                if (m != null)
                {
                    string combatResultsMsg = "MvM (" + m.Representation + ") Grenade: Dam: " + damage;
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    //Apply damage
                    //make this a player attack to avoid AI being confused by monster attack that disappears
                    Game.Dungeon.Player.AttackMonsterRanged(squareContents.monster, damage);
                }
            }

            //And the player

            if (grenadeAffects.Find(p => p.x == Game.Dungeon.Player.LocationMap.x && p.y == Game.Dungeon.Player.LocationMap.y) != null)
            {
                //Apply damage (uses damage base)
                AttackPlayer(Game.Dungeon.Player);
            }
        }



        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new RollingBomb();
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

        protected override PatrolType GetPatrolType()
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


        protected override bool WillInvestigateSounds()
        {
            return true;
        }

        protected override bool WillPursue()
        {
            return true;
        }

        //Makes them more effective swarmers
        public override bool CanOpenDoors()
        {
            return true;
        }


        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "Rolling Bomb"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Rolling Bombs"; } }

        protected override char GetRepresentation()
        {
            return 'b';
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
            return 20;
        }

        public override int CreatureLevel()
        {
            return 3;
        }


        public override Color RepresentationColor()
        {
            return ColorPresets.Magenta;
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
