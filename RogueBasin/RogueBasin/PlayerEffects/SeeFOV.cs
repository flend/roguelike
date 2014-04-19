using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.PlayerEffects
{
    public class SeeFOV : PlayerEffectSimpleDuration
    {
        public int duration { get; set; }

        //public bool sightZeroCase  { get; set; }

        public SeeFOV() { 
            //this.sightZeroCase = false;
            this.duration = 1000;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart(Player player)
        {
            LogFile.Log.LogEntry("Player sees FOV start");

            //Sight radius is already maximum so don't do anything
            /*if (player.SightRadius == 0)
            {
                sightZeroCase = true;
            }
            else
            {
                player.SightRadius += sightUpAmount;
            }*/
            Game.MessageQueue.AddMessage("FOVs overlay on your tactical display.");
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd(Player player)
        {
            LogFile.Log.LogEntry("SightUp ended");
            /*
            if (!sightZeroCase)
            {
                player.SightRadius -= sightUpAmount;
            }*/
            Game.MessageQueue.AddMessage("The tactical display ends.");
        }

        public override int GetDuration()
        {
            return duration;
        }

        public override int SightModifier()
        {
            return 0;
        }

        public override string GetName()
        {
            return "Tac. Overlay";
        }

        internal override libtcodWrapper.Color GetColor()
        {
            return ColorPresets.SeaGreen;
        }
    }
}
