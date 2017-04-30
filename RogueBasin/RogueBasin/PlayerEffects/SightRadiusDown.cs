namespace RogueBasin.PlayerEffects
{
    public class SightRadiusDown : PlayerEffectSimpleDuration
    {
        int duration  { get; set; }

        int sightModifier  { get; set; }

        //int effectiveSightDownAmount  { get; set; }
        //public bool sightZeroCase { get; set; }

        public SightRadiusDown() { }

        public SightRadiusDown(int duration, int sightDownAmount)
        {
            //this.sightZeroCase = false;
            this.duration = duration;
            this.sightModifier = -1 * sightDownAmount;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart(Player target)
        {
            Player player = target as Player;

            LogFile.Log.LogEntry("SightDown start");

            //Bit of a hack, but for 0 sight dungeons, restrict to 1 square. This is what the blinding effects normally do
            /*
            if (player.SightRadius == 0)
            {
                sightZeroCase = true;
                player.SightRadius = 1;
            }
            else
            {
                if (sightModifier > player.SightRadius)
                {
                    effectiveSightDownAmount = player.SightRadius;
                }
                else
                {
                    effectiveSightDownAmount = sightModifier;
                }
            }
             * */
            //player.SightRadius -= effectiveSightDownAmount;
            Game.MessageQueue.AddMessage("The shadows come closer.");
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd(Player target)
        {
            Player player = target as Player;

            LogFile.Log.LogEntry("SightDown ended");

            /*
            if (sightZeroCase)
            {
                player.SightRadius = 0;
            }
            else
            {
                player.SightRadius += effectiveSightDownAmount;
            }*/
            Game.MessageQueue.AddMessage("Everything is clear again.");
        }

        public override int GetDuration()
        {
            return duration;
        }

        public override int SightModifier()
        {
            return sightModifier;
        }
    }
}
