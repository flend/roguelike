namespace RogueBasin
{
    /// <summary>
    /// The kind of feature everyone HAS to interact with
    /// </summary>
    public abstract class ActiveFeature : Feature
    {
        /// <summary>
        /// Process a player interacting with this object
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool PlayerInteraction(Player player);

        public abstract bool MonsterInteraction(Monster monster);

    }
}