namespace RogueBasin.MonsterEffects
{
    public class SlowDown : MonsterEffectSimpleDuration
    {
        public int duration { get; set; }

        public int speedEffect  { get; set; }

        public SlowDown() { }

        public SlowDown(int duration, int speedEffect)
        {
            this.duration = duration;
            this.speedEffect = speedEffect;
        }

        /// <summary>
        /// Combat power so recalculate stats
        /// </summary>
        public override void OnStart(Monster target)
        {
            LogFile.Log.LogEntry("SlowDown started");
            Game.MessageQueue.AddMessage("The " + target.SingleDescription + " slows!");
        }

        /// <summary>
        /// Combat power so recalculate stats
        /// </summary>
        public override void OnEnd(Monster target)
        {
            LogFile.Log.LogEntry("SlowDown ended");
            Game.MessageQueue.AddMessage("The " + target.SingleDescription + " recovers.");
        }

        protected override int GetDuration()
        {
            return duration;
        }

        public override int SpeedModifier()
        {
            return -speedEffect;
        }
    }
}
