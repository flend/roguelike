using System;
namespace RogueBasin.Creatures
{
    public class Grenadier : MonsterThrowAndRunAI
    {

        public Grenadier(int level): base(level)
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));

            NormalSightRadius = 5;

       }

        protected override bool UseSpecialAbility()
        {
            //Throw a timed grenade at the player

            //Find an adjacent square to the target
            var adjacentSquares = Game.Dungeon.GetWalkableAdjacentSquaresFreeOfCreatures(currentTarget.LocationLevel, currentTarget.LocationMap);

            if(adjacentSquares.Count > 0) {
                LogFile.Log.LogEntryDebug("Grenadier throwing grenade at " + currentTarget.Representation, LogDebugLevel.Medium);

                var grenadeCreature = new Creatures.Grenade(this.ScaleRangedDamage(30), 2, 2.0);
                var grenadeSquare = adjacentSquares.RandomElement();
                Game.Dungeon.AddMonsterDynamic(grenadeCreature, new Location(currentTarget.LocationLevel, grenadeSquare));

                var targetSquares = Game.Dungeon.WeaponUtility.CalculateTrajectorySameLevel(this, grenadeSquare);
                Screen.Instance.DrawAreaAttackAnimationProgressive(targetSquares, grenadeCreature.GameSprite);
            }
            else {
                LogFile.Log.LogEntryDebug("Grenadier failed to throw grenade at " + currentTarget.Representation, LogDebugLevel.Medium);
            }

            //Always use this rather than shoot
            return true;
        }

        public override double GetMissileRange()
        {
            return 5.0;
        }

        internal override int ReloadTurnsRequired()
        {
            return 2;
        }

        protected override string GetWeaponName()
        {
            return "throws a grenade!";
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Grenadier(Level);
        }

        protected override int ClassMaxHitpoints()
        {
            return 45;
        }

        public override int DamageBase()
        {
            return 0;
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
        public override string SingleDescription { get { return "Grenadier"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Grenadier"; } }

        protected override char GetRepresentation()
        {
            return 'G';
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
            return "dwarf";
        }

        protected override string GetUISprite()
        {
            return "dwarf";
        }

        public override int GetCombatXP()
        {
            return 20;
        }

    }
}
