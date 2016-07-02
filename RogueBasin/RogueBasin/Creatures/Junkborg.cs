using System;using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using System.Linq;

namespace RogueBasin.Creatures
{
    public class Junkborg : MonsterThrowAndRunAI
    {

        public Junkborg(int level): base(level)
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));

            NormalSightRadius = 5;

       }

        public static bool RaiseCorpse(Monster summoner, double range) {
        
            //Look for a nearby corpse
            List<Feature> corpseInRange = new List<Feature>();

            foreach (Feature feature in Game.Dungeon.Features)
            {
                if (summoner.LocationLevel != feature.LocationLevel)
                    continue;

                if (Utility.GetDistanceBetween(summoner, feature) < range + 0.005)
                {
                    if (feature is Features.Corpse)
                    {
                        corpseInRange.Add(feature);
                    }
                }
            }

            if (corpseInRange.Count == 0)
                return false;

            //Pick a corpse at random
            var corpsesByRange = corpseInRange.OrderBy(f => Utility.GetDistanceBetween(summoner, f));

            foreach (Feature actualCorpse in corpseInRange)
            {
                //Check this square is empty
                int corpseLevel = actualCorpse.LocationLevel;
                Point corpseMap = actualCorpse.LocationMap;

                SquareContents contents = Game.Dungeon.MapSquareContents(corpseLevel, corpseMap);

                if (!contents.empty)
                    continue;

                //Raise a creature here

                //For now just raise skeletons I think we might need to make a separate AI for each raisey creature
                Game.Dungeon.Features.Remove(actualCorpse); //should have a helper for this really

                //Spawn a zombie
                var zombieCreature = new Creatures.Zomborg(summoner.Level);
                Game.Dungeon.AddMonsterDynamic(zombieCreature, actualCorpse.LocationLevel, actualCorpse.LocationMap);

                Game.MessageQueue.AddMessage("The " + summoner.SingleDescription + " produces something horrible!");
                LogFile.Log.LogEntryDebug(summoner.SingleDescription + " raises corpse", LogDebugLevel.Medium);
                break;
            }

            return true;
        }

        protected override bool UseSpecialAbility()
        {
            //Raises corpses

            //Look for a nearby corpse
            return Junkborg.RaiseCorpse(this, GetMissileRange());

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
            return "throws a cybernetic part";
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Junkborg(Level);
        }

        protected override int ClassMaxHitpoints()
        {
            return 100;
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
        public override string SingleDescription { get { return "Junkborg"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Junkborgs"; } }

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
            return "junkborg";
        }

        protected override string GetUISprite()
        {
            return "junkborg";
        }

        protected override int GetChanceToBackAway()
        {
            return 100;
        }

        public override int GetCombatXP()
        {
            return 40;
        }

    }
}
