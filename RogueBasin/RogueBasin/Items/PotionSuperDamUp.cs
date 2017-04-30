namespace RogueBasin.Items
{
    public class PotionSuperDamUp : Item, IUseableItem
    {
        bool usedUp;

        public PotionSuperDamUp()
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

            //Apply the healing effect to the player
            //Duration note 100 is normally 1 turn for a non-sped up player

            int duration = 5 * Creature.turnTicks + Game.Random.Next(10 * Creature.turnTicks);
            int toHitUp = 3 + Game.Random.Next(5);

            player.AddEffect(new PlayerEffects.DamageUp(duration, toHitUp));


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
            get { return "p9"; }
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
