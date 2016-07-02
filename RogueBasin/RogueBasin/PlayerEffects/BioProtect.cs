using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.PlayerEffects
{
    public class BioProtect : PlayerEffectNoDuration
    {

        //public bool sightZeroCase  { get; set; }

        public BioProtect()
        { 
            //this.sightZeroCase = false;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart(Player player)
        {
            LogFile.Log.LogEntry("Bioprotect start");

            //Sight radius is already maximum so don't do anything
            /*if (player.SightRadius == 0)
            {
                sightZeroCase = true;
            }
            else
            {
                player.SightRadius += sightUpAmount;
            }*/
            Game.MessageQueue.AddMessage("You are surrounded by a cloud of nanobodies.");
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd(Player player)
        {
            LogFile.Log.LogEntry("Bioprotect end");
            /*
            if (!sightZeroCase)
            {
                player.SightRadius -= sightUpAmount;
            }*/
            Game.MessageQueue.AddMessage("The nanobodies dissipate.");
        }


        public override int SightModifier()
        {
            return 0;
        }

        public override string GetName()
        {
            return "Bio";
        }

        internal override System.Drawing.Color GetColor()
        {
            return System.Drawing.Color.BlanchedAlmond;
        }
    }
}
