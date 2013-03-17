using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.PlayerEffects
{
    public class StealthField : PlayerEffectSimpleDuration
    {
        public int duration { get; set; }

        //public bool sightZeroCase  { get; set; }

        public StealthField()
        { 
            //this.sightZeroCase = false;
            this.duration = 200;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart(Player player)
        {
            LogFile.Log.LogEntry("Stealth start");

            //Sight radius is already maximum so don't do anything
            /*if (player.SightRadius == 0)
            {
                sightZeroCase = true;
            }
            else
            {
                player.SightRadius += sightUpAmount;
            }*/
            Game.MessageQueue.AddMessage("You fade out from your surroundings.");
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd(Player player)
        {
            LogFile.Log.LogEntry("Stealth end");
            /*
            if (!sightZeroCase)
            {
                player.SightRadius -= sightUpAmount;
            }*/
            Game.MessageQueue.AddMessage("The stealth emitter is discharged.");
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
            return "Stealth";
        }

        internal override libtcodWrapper.Color GetColor()
        {
            return ColorPresets.BlanchedAlmond;
        }
    }
}