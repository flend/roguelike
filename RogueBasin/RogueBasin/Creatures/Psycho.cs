using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{

    public class Psycho : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 4;
        const int classMinHitpoints = 1;

        public Psycho(int level)
            : base(level)
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            NormalSightRadius = 4;
            IgnoreDangerousTerrain = true;
        }

        protected override int ClassMaxHitpoints()
        {
            return 60;
        }

        public override int DamageBase()
        {
            return 30;
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
            return 'k';
        }

        protected override string GetGameSprite()
        {
            return "psycho";
        }

        protected override string GetUISprite()
        {
            return "psycho";
        }

        protected override string GetGameOverlaySprite()
        {
            return "chainsaw";
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
            return new Psycho(CreatureLevel());
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
