using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin.PlayerEffects
{
    class Aim : PlayerEffectNoDuration
    {
        public int duration { get; set; }


        public Aim()
        {

        }

        public override void OnStart(Player player)
        {
            LogFile.Log.LogEntryDebug("Aim effect started", LogDebugLevel.Medium);
            Game.MessageQueue.AddMessage("Deadeye ... active!");
        }

        public override void OnEnd(Player player)
        {
            LogFile.Log.LogEntryDebug("Aim effect ended", LogDebugLevel.Medium);
            /*
            if (!sightZeroCase)
            {
                player.SightRadius -= sightUpAmount;
            }*/
            Game.MessageQueue.AddMessage("Nah, no time to aim.");
        }

        public override int SightModifier()
        {
            return 0;
        }

        public override int SpeedModifier()
        {
            return 0;
        }

        public override string GetName()
        {
            return "Dodge";
        }

        internal override System.Drawing.Color GetColor()
        {
            return System.Drawing.Color.BlanchedAlmond;
        }
    }
}
