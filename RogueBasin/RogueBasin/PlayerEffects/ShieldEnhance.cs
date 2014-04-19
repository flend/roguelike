using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    public class ShieldEnhance : PlayerEffectNoDuration
    {
        public int shieldEnhanceAmount { get; set; }

        public ShieldEnhance() { }

        public ShieldEnhance(int shieldEnhanceAmount)
        {
            this.shieldEnhanceAmount = shieldEnhanceAmount;
        }

        /// <summary>
        /// Just strings
        /// </summary>
        public override void OnStart(Player target)
        {
            LogFile.Log.LogEntry("ShieldEnhance start");
            Game.MessageQueue.AddMessage("Shield enhance enabled");
        }

        /// <summary>
        /// Just strings
        /// </summary>
        public override void OnEnd(Player target)
        {
            LogFile.Log.LogEntry("ShieldEnhance ended");
            Game.MessageQueue.AddMessage("Shield enhance stopped.");
        }
    }
}
