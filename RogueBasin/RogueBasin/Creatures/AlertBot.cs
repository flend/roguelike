using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Fast randomly moving robot with long range. Will make a loud sound if sees player.
    /// Will not pursue or respond to sounds itself.
    /// </summary>
    public class AlertBot : MonsterThrowAndRunAI
    {
        const int classDeltaHitpoints = 4;
        const int classMinHitpoints = 1;

        long lastAlertTime = -1;

        public AlertBot()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            NormalSightRadius = 7;
        }

        public override int BaseSpeed()
        {
            return 200;
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        protected override int ClassMaxHitpoints()
        {
            return 10;
        }

        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return 5;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 2;
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

        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Base;
        }

        public override PatrolType GetPatrolType()
        {
            return PatrolType.RandomWalk;
        }

        protected override bool WillPursue()
        {
 	        return false;
        }

        protected override bool  WillInvestigateSounds()
        {
 	        return false;
        }

        /// <summary>
        /// The bot keeps moving around. Particularly useful since it doens't fire it's special ability each time
        /// </summary>
        /// <returns></returns>
        protected override bool WillAlwaysPatrol()
        {
            return true;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "Alert bot"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Alert bots"; } }

        protected override char GetRepresentation()
        {
            return (char)259;
        }

        protected override int GetChanceToRecover()
        {
            return 10;
        }

        protected override int GetChanceToFlee()
        {
            return 0;
        }

        protected override int GetMaxHPWillFlee()
        {
            return Hitpoints;
        }

        public override int CreatureCost()
        {
            return 20;
        }

        public override int CreatureLevel()
        {
            return 1;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new AlertBot();
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.GreenYellow;
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

        /// <summary>
        /// Alert bot has a long range to use its alerting ability
        /// </summary>
        /// <returns></returns>
        public override double GetMissileRange()
        {
            return 10.0;
        }

        protected override string GetWeaponName()
        {
            return "sounds the alert!";
        }

        internal override char GetCorpseRepresentation()
        {
            return (char)504;
        }

        internal override System.Drawing.Color GetCorpseRepresentationColour()
        {
            return System.Drawing.Color.DarkRed;
        }

        protected override bool UseSpecialAbility()
        {
            //Don't make too many sounds otherwise it will slow the game down
            //Return true to stop the bot firing
            if (lastAlertTime != -1 && lastAlertTime + 100 > Game.Dungeon.WorldClock)
            {
                return true;
            }

 	        //Alert bot makes a loud sound at its location
            string playerMsg = "The alert bot sounds the alert!";
            Game.MessageQueue.AddMessage(playerMsg);
            
            SoundEffect effect = Game.Dungeon.AddSoundEffect(1.0, this.LocationLevel, Game.Dungeon.Player.LocationMap);
            lastAlertTime = Game.Dungeon.WorldClock;
            LogFile.Log.LogEntryDebug("Alert bot makes sound: " + effect + " at time: " + Game.Dungeon.WorldClock, LogDebugLevel.Medium);

            List<Point> grenadeAffects = Game.Dungeon.GetPointsForGrenadeTemplate(Game.Dungeon.Player.LocationMap, Game.Dungeon.Player.LocationLevel, 4);
            Screen.Instance.DrawAreaAttackAnimation(grenadeAffects, System.Drawing.Color.Yellow);

            return true;
        }

        public override int DropChance()
        {
            return 0;
        }

    }
}
