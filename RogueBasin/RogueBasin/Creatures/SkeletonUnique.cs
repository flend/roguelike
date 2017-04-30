namespace RogueBasin.Creatures
{
    /// <summary>
    /// Medium threat, faster than normal
    /// </summary>
    public class SkeletonUnique : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 20;
        const int classMinHitpoints = 25;

        public string UniqueName { get; set; }

        public SkeletonUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));

            Unique = true;
            UniqueName = "Daphill the Dry";
        }

        public override Monster NewCreatureOfThisType()
        {
            return new SkeletonUnique();
        }
        
        public override int BaseSpeed()
        {
            return 110;
        }
        
        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        protected override int ClassMaxHitpoints()
        {
            return classMinHitpoints + Game.Random.Next(classDeltaHitpoints) + 1;
        }

        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return 14;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 8;
        }

        /// <summary>
        /// Creature damage modifier.  Set by type of creature.
        /// </summary>
        public override double DamageModifier()
        {
            return 1;
        }

        public override int HitModifier()
        {
            return 5;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return UniqueName; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "skeleton"; } }

        protected override char GetRepresentation()
        {
            return 'S';
        }

        protected override int GetChanceToRecover()
        {
            return 20;
        }

        protected override int GetChanceToFlee()
        {
            return 50;
        }

        protected override int GetMaxHPWillFlee()
        {
            return MaxHitpoints / 2;
        }
        public override int CreatureCost()
        {
            return 35;
        }

        public override int CreatureLevel()
        {
            return 4;
        }

        public override int GetMagicXP()
        {
            return 50;
        }

        public override int GetCombatXP()
        {
            return 50;
        }

        public override int GetMagicRes()
        {
            return 50;
        }

        public override int GetCharmRes()
        {
            return 0;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }
    }
}
