namespace RogueBasin.Features
{
    public class EscapePod : UseableFeature
    {
        int destLevel;
        Point destLocation;

        public EscapePod()
        {

        }

        /// <summary>
        /// Escape pod end game
        /// </summary>
        public override bool PlayerInteraction(Player player)
        {
            Dungeon dungeon = Game.Dungeon;

            Game.MessageQueue.AddMessage("You take the escape pod. YOU WIN!");

            //dungeon.EndOfGame(true, false);

            return true;
        }

        protected override char GetRepresentation()
        {
            return (char)538;
        }

        public override string Description
        {
            get
            {
                return "Escape Pod Launch";
            }
        }

        public override string QuestId
        {
            get
            {
                return "escape-pod-launch";
            }
        }
    }
}
