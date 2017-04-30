namespace RogueBasin.PlayerEffects
{
    public class SightRadiusUp : PlayerEffectSimpleDuration
    {
        public int duration { get; set; }

        public int sightUpAmount { get; set; }
        //public bool sightZeroCase  { get; set; }

        public SightRadiusUp() { }

        public SightRadiusUp(int duration, int sightUpAmount)
        {
            //this.sightZeroCase = false;
            this.duration = duration;
            this.sightUpAmount = sightUpAmount;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart(Player player)
        {
            LogFile.Log.LogEntry("SightUp start");

            //Sight radius is already maximum so don't do anything
            /*if (player.SightRadius == 0)
            {
                sightZeroCase = true;
            }
            else
            {
                player.SightRadius += sightUpAmount;
            }*/
            Game.MessageQueue.AddMessage("The darkness falls away in a glance.");
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
            Game.MessageQueue.AddMessage("The shadows grow longer.");
        }

        public override int GetDuration()
        {
            return duration;
        }

        public override int SightModifier()
        {
            return sightUpAmount;
        }
    }
}
