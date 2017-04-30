namespace RogueBasin.Creatures
{
    /// <summary>
    /// Low threat, fights to the death. Good eyesight
    /// </summary>
    public class Rat : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 6;
        const int classMinHitpoints = 2;

        public Rat()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            SightRadius = 6;
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Rat();
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
            return 5;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 2;
        }

        /// <summary>
        /// Creature damage modifier.  Set by type of creature.
        /// </summary>
        public override double DamageModifier()
        {
            return 0;
        }

        public override int HitModifier()
        {
            return 0;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "rat"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "rats"; } }

        protected override char GetRepresentation()
        {
            return 'r';
        }

        protected override int GetChanceToRecover()
        {
            return 0;
        }

        protected override int GetChanceToFlee()
        {
            return 0;
        }

        protected override int GetMaxHPWillFlee()
        {
            return 0;
        }
        public override int CreatureCost()
        {
            return 15;
        }

        public override int CreatureLevel()
        {
            return 1;
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.Coral;
        }

        public override int GetCombatXP()
        {
            return 15;
        }

        public override int GetMagicXP()
        {
            return 15;
        }

        public override int GetMagicRes()
        {
            return 0;
        }

        public override int GetCharmRes()
        {
            return 10;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }
    }
}
