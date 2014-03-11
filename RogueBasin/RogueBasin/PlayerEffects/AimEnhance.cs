using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    public class AimEnhance : PlayerEffectNoDuration
    {
        public int aimEnhanceAmount { get; set; }

        public AimEnhance() { }

        public AimEnhance(int shieldEnhanceAmount)
        {
            this.aimEnhanceAmount = shieldEnhanceAmount;
        }

        /// <summary>
        /// Just strings
        /// </summary>
        public override void OnStart(Player target)
        {
            LogFile.Log.LogEntry("AimEnhance start");
            Game.MessageQueue.AddMessage("Aim enhance enabled");
        }

        /// <summary>
        /// Just strings
        /// </summary>
        public override void OnEnd(Player target)
        {
            LogFile.Log.LogEntry("AimEnhance ended");
            Game.MessageQueue.AddMessage("Aim enhance stopped.");
        }
    }
}