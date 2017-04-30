namespace RogueBasin.Creatures
{

    public class Thug : MonsterThrowAndRunAI
    {

        public Thug(int level)
            : base(level)
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            NormalSightRadius = 5;

        }

        protected override int ClassMaxHitpoints()
        {
            return 45;
        }

        public override int DamageBase()
        {
            return 30;
        }

        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Base;
        }

        public override PatrolType GetPatrolType()
        {
            return PatrolType.Rotate;
        }

        protected override bool WillInvestigateSounds()
        {
            return true;
        }

        internal override int ReloadTurnsRequired()
        {
            return 2;
        }

        protected override bool WillPursue()
        {
            return true;
        }

        public override double GetMissileRange()
        {
            return 5.0;
        }

        protected override int GetChanceToBackAway()
        {
            return 0;
        }

        protected override string GetWeaponName()
        {
            return "fires his shotgun";
        }

        public override bool CanOpenDoors()
        {
            return true;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "Thug"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "Thugs"; } }

        protected override char GetRepresentation()
        {
            return 'f';
        }

        internal override char GetCorpseRepresentation()
        {
            return (char)501;
        }

        internal override System.Drawing.Color GetCorpseRepresentationColour()
        {
            return System.Drawing.Color.DarkRed;
        }

        protected override string GetGameSprite()
        {
            return "thug";
        }

        protected override string GetUISprite()
        {
            return "thug";
        }

        protected override int GetChanceToRecover()
        {
            return 10;
        }

        protected override int GetChanceToFlee()
        {
            return 0;
        }

        protected override int GetMaxHPWillFlee()
        {
            return Hitpoints;
        }

        public override int CreatureCost()
        {
            return 200;
        }

        public override int CreatureLevel()
        {
            return 1;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Thug(Level);
        }

        protected override CreatureAttackType GetCreatureAttackType()
        {
            return CreatureAttackType.Shotgun;
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.Chartreuse;
        }

        public override int GetMagicXP()
        {
            return 10;
        }

        public override int GetMagicRes()
        {
            return 0;
        }

        public override int GetCharmRes()
        {
            return 5;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }



        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return 5;
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

        public override int DropChance()
        {
            return 30;
        }

        public override int GetCombatXP()
        {
            return 20;
        }

        protected override string GetGameOverlaySprite()
        {
            return "shotgun";
        }
    }
}

