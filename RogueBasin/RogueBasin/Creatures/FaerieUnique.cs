using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Medium threat. Fast but weak missile.
    /// </summary>
    public class FaerieUnique : MonsterSpecialAI
    {
        const int classDeltaHitpoints = 8;
        const int classMinHitpoints = 16;

        public string UniqueName { get; set; }

        public FaerieUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));

            Unique = true;
            UniqueName = "Aerie the Faerie";
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Faerie();
        }

        public override int BaseSpeed()
        {
            return 200;
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
            return 2;
        }

        /// <summary>
        /// Creature damage modifier.  Set by type of creature.
        /// </summary>
        public override int DamageModifier()
        {
            return 1;
        }

        public override int HitModifier()
        {
            return 6;
        }

        protected override double GetMissileRange()
        {
            return 5.5;
        }

        protected override string GetWeaponName()
        {
            return "sprinkles caustic dust";
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return UniqueName; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription  { get { return "faeries"; } }

        protected override char GetRepresentation()
        {
            return 'f';
        }

        protected override int RelaxDirectionAt()
        {
            return 5;
        }

        protected override int GetChanceToRecover()
        {
            return 10;
        }

        protected override int GetChanceToFlee()
        {
            return 75;
        }

        protected override int GetMaxHPWillFlee()
        {
            return Hitpoints;
        }

        protected override int GetTotalFleeLoops()
        {
            return 20;
        }
        public override int CreatureCost()
        {
            return 40;
        }

        public override int CreatureLevel()
        {
            return 3;
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.Cyan;
        }

        public override int GetMagicXP()
        {
            return 30;
        }

        public override int GetCombatXP()
        {
            return 35;
        }

        public override int GetMagicRes()
        {
            return 50;
        }

        public override int GetCharmRes()
        {
            return 50;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }

        protected override string EffectAttackString()
        {
            return "blink";
        }

        protected override bool DoPlayerResistance()
        {
            Player player = Game.Dungeon.Player;

            //Chance to resist the blinding attack
            int highestSkill = player.AttackStat;
            if (player.CharmStat > highestSkill)
                highestSkill = player.CharmStat;
            if (player.MagicStat > highestSkill)
                highestSkill = player.MagicStat;


            highestSkill = highestSkill / 2;
            if (highestSkill > 75)
                highestSkill = 75;

            int roll = Game.Random.Next(100);

            LogFile.Log.LogEntryDebug("Player resistance: " + roll + " below " + highestSkill, LogDebugLevel.Medium);

            if (roll < highestSkill)
                return true;
            return false;
        }

        protected override Spell GetSpecialAISpell()
        {
            return new Spells.Blink();
        }

        protected override SpecialAIType GetSpecialAIType()
        {
            return SpecialAIType.PlayerCaster;
        }

        protected override int GetUseSpecialChance()
        {
            return 80;
        }
        

    }
}