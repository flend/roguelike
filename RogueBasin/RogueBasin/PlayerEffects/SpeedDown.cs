namespace RogueBasin.PlayerEffects
{
    /// <summary>
    /// Speed up the player for a duration
    /// </summary>
    public class SpeedDown : PlayerEffectSimpleDuration
    {
        public int duration  { get; set; }

        public int speedDownAmount { get; set; }

        public SpeedDown() { }

        public SpeedDown(int duration, int speedDownAmount) {

            this.duration = duration;
            this.speedDownAmount = speedDownAmount;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart(Player target)
        {
            LogFile.Log.LogEntry("Speed Down started");
            Game.MessageQueue.AddMessage("You slow up!");

            //player.Speed -= speedUpAmount;
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd(Player target)
        {
            LogFile.Log.LogEntry("Speed Down ended");
            Game.MessageQueue.AddMessage("You speed up!");

            //player.Speed += speedUpAmount;
        }

        public override int SpeedModifier()
        {
            return -speedDownAmount;
        }

        public override int GetDuration()
        {
            return duration;
        }

    }
}
