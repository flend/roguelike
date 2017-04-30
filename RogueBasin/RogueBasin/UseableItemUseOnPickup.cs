namespace RogueBasin
{
    abstract public class UseableItemUseOnPickup : Item, IUseableItem
    {
        /// <summary>
        /// Applies the effect of the object
        /// </summary>
        /// <param name="user"></param>
        /// <returns>True if the object could be used</returns>
        public abstract bool Use(Creature user);

        /// <summary>
        /// If the object has been used up by a Use()
        /// </summary>
        public abstract bool UsedUp
        {
            get;
            set;
        }

        public override bool OnPickup(Creature pickupCreature) {
            return Use(pickupCreature);
        }

        public override bool DestroyedOnPickup()
        {
            return true;
        }
    }
}
