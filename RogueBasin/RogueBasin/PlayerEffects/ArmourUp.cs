using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    public class ArmourUp : PlayerEffectSimpleDuration
    {
        public int duration { get; set; }

        public int armourUpAmount { get; set; }

        public ArmourUp() { }

        public ArmourUp(int duration, int armourUpAmount)
        {

            this.duration = duration;
            this.armourUpAmount = armourUpAmount;
        }

        /// <summary>
        /// Just strings
        /// </summary>
        public override void OnStart(Player target)
        {
            LogFile.Log.LogEntry("ArmourUp start");
            Game.MessageQueue.AddMessage("A blue shimmer surrounds you.");
            Game.Dungeon.Player.RecalculateCombatStatsRequired = true;
        }

        /// <summary>
        /// Just strings
        /// </summary>
        public override void OnEnd(Player target)
        {
            LogFile.Log.LogEntry("ArmourUp ended");
            Game.MessageQueue.AddMessage("The blue shimmer around you fades.");
            Game.Dungeon.Player.RecalculateCombatStatsRequired = true;
        }

        protected override int GetDuration()
        {
            return duration;
        }

        public override int ArmourClassModifier()
        {
            return armourUpAmount;
        }
    }
}