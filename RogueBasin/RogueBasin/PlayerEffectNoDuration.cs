namespace RogueBasin
{
    /// <summary>
    /// An effect that needs to be explicitally added and removed
    /// </summary>
    public abstract class PlayerEffectNoDuration : PlayerEffect
    {
        public PlayerEffectNoDuration()
        {

        }

        /// <summary>
        /// No duration effects normally have an empty OnEnd
        /// </summary>
        /// <param name="target"></param>
        public override void OnEnd(Player target)
        {

        }

        /// <summary>
        /// Never ends
        /// </summary>
        /// <returns></returns>
        public override bool HasEnded()
        {
            return false;
        }

        /// <summary>
        /// Do nothing here
        /// </summary>
        public override void IncrementTime(Player target)
        {

        }
    }
}
