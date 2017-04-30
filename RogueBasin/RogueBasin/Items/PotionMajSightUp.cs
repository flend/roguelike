namespace RogueBasin.Items
{
    public class PotionMajSightUp : Item, IUseableItem
    {
        bool usedUp;

        public PotionMajSightUp()
        {
            usedUp = false;
        }

        public bool Use(Creature user)
        {
            //Currently healing is implemented as a player effect so we need to check the user is a player
            Player player = user as Player;

            //Not a player
            if (player == null)
            {
                return false;
            }
            //Add a message
            Game.MessageQueue.AddMessage("You drink the potion.");

            int duration = 50 * Creature.turnTicks + Game.Random.Next(80 * Creature.turnTicks);

            player.AddEffect(new PlayerEffects.SightRadiusUp(duration, 2));



            //This uses up the potion
            usedUp = true;

            return true;
        }

        public override int GetWeight()
        {
            return 10;
        }

        public override string SingleItemDescription
        {
            get { return "p4"; }
        }

        public override string GroupItemDescription
        {
            get { return "potions"; }
        }

        public bool UsedUp
        {
            set { usedUp = value; }
            get { return usedUp; }
        }

        protected override char GetRepresentation()
        {
            return '!';
        }

        public override bool UseHiddenName { get { return true; } }

        public override string HiddenSuffix
        {
            get
            {
                return "vial";
            }
        }
    }
}
