namespace RogueBasin.PlayerEffects
{
    class Dodge : PlayerEffectNoDuration
    {
        public int duration { get; set; }


        public Dodge()
        {

        }

        public override void OnStart(Player player)
        {
            LogFile.Log.LogEntryDebug("Dodge effect started", LogDebugLevel.Medium);
            Game.MessageQueue.AddMessage("Everything seems ... dodgy");
        }

        public override void OnEnd(Player player)
        {
            LogFile.Log.LogEntryDebug("Dodge effect ended", LogDebugLevel.Medium);
            /*
            if (!sightZeroCase)
            {
                player.SightRadius -= sightUpAmount;
            }*/
            Game.MessageQueue.AddMessage("The world slows back down.");
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
