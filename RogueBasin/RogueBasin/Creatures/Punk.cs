namespace RogueBasin.Creatures
{

    public class Punk : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 4;
        const int classMinHitpoints = 1;

        public Punk(int level) : base (level)
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            NormalSightRadius = 5;
        }

        protected override int ClassMaxHitpoints()
        {
            return 20;
        }

        public override int DamageBase()
        {
            return 10;
        }


        public override CreatureFOV.CreatureFOVType FOVType()
        {
            return CreatureFOV.CreatureFOVType.Base;
        }

        public override Pathing.PathingType PathingType()
        {
            return Pathing.PathingType.Normal;
        }

        protected override bool WillInvestigateSounds()
        {
            return true;
        }

        protected override bool WillPursue()
        {
            return true;
        }

        public override bool CanOpenDoors()
        {
            return true;
        }

        public override string SingleDescription { get { return "punk"; } }

        public override string GroupDescription { get { return "punks"; } }

        protected override char GetRepresentation()
        {
            return 'p';
        }

        protected override string GetGameSprite()
        {
            return "punk";
        }

        protected override string GetUISprite()
        {
            return "punk";
        }

        protected override string GetGameOverlaySprite()
        {
            return "knife";
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

        public override int GetCharmRes()
        {
            return 5;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }

        public override int CreatureCost()
        {
            return 10;
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Punk(CreatureLevel());
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.WhiteSmoke;
        }

        public override int GetCombatXP()
        {
            return 10;
        }

    }
}
