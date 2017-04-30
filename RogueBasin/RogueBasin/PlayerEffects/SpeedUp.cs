namespace RogueBasin.PlayerEffects
{
    /// <summary>
    /// Speed up the player for a duration
    /// </summary>
    public class SpeedUp : PlayerEffectSimpleDuration
    {
        public int duration { get; set; }

        public int speedUpAmount  { get; set; }

        public SpeedUp() { }

        public SpeedUp(int duration, int speedUpAmount) {

            this.duration = duration;
            this.speedUpAmount = speedUpAmount;
        }

        /// <summary>
        /// Increase the player's speed
        /// </summary>
        public override void OnStart(Player target)
        {
            LogFile.Log.LogEntry("Speed Up started");
            Game.MessageQueue.AddMessage("You speed up!");

            //player.Speed += speedUpAmount;
        }

        /// <summary>
        /// Decrease the player's speed again
        /// </summary>
        public override void OnEnd(Player target)
        {
            LogFile.Log.LogEntry("Speed Up ended");
            Game.MessageQueue.AddMessage("You slow down!");

            //player.Speed -= speedUpAmount;
        }

        public override int SpeedModifier()
        {
            return speedUpAmount;
        }

        public override int GetDuration()
        {
            return duration;
        }

    }
}
