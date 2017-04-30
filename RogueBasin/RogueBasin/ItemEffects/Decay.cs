namespace RogueBasin.ItemEffects
{
    class Decay : ItemEffectSimpleDuration
    {
        int duration;

        public Decay(int duration)
        {
            this.duration = duration;
        }

        public override void OnStart(Item target)
        {
            LogFile.Log.LogEntryDebug("Starting decay effect, duration " + duration + " on item " + target, LogDebugLevel.Medium);
        }

        public override void OnEnd(Item target)
        {
            LogFile.Log.LogEntryDebug("Ending decay effect on item " + target, LogDebugLevel.Medium);

            //Remove this object from interaction
            Game.Dungeon.HideItem(target);
        }

        protected override int GetDuration()
        {
            return duration;
        }
    }
}
