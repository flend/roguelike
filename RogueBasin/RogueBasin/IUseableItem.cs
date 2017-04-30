namespace RogueBasin
{
    interface IUseableItem
    {
        /// <summary>
        /// Applies the effect of the object
        /// </summary>
        /// <param name="user"></param>
        /// <returns>True if the object could be used</returns>
        bool Use(Creature user);

        /// <summary>
        /// If the object has been used up by a Use()
        /// </summary>
        bool UsedUp
        {
            get;
            set;
        }
    }
}
