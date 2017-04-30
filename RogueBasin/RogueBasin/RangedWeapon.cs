namespace RogueBasin
{
    public abstract class RangedWeapon : Item
    {

        /// <summary>
        /// Public for serialization
        /// </summary>
        public int Ammo { get; set; }

        public RangedWeapon()
        {
            Ammo = MaxAmmo();
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        public override int GetWeight()
        {
            return 50;
        }

        public override int ItemCost()
        {
            return 10;
        }

        public virtual double FireSoundMagnitude()
        {
            return 0.4;
        }

        public abstract int MaxAmmo();

        public int RemainingAmmo() {
            return Ammo;

        }
    }
}
