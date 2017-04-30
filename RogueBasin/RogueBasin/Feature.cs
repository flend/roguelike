namespace RogueBasin
{
    /// <summary>
    /// Non-pickupable objects in the dungeon
    /// </summary>
    public abstract class Feature : MapObject
    {

        public Feature()
        {
            IsBlocking = false;
            BlocksLight = false;
        }

        public bool IsBlocking { get; set; }
        public bool BlocksLight { get; set; }

        public virtual string Description
        {
            get
            {
                return "Feature";
            }
        }

        public virtual string QuestId
        {
            get
            {
                return "";
            }
        }
    }
}
