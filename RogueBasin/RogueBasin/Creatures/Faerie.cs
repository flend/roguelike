using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Medium threat. Fast but weak missile.
    /// </summary>
    public class Faerie : MonsterSpecialAI
    {
        const int classDeltaHitpoints = 6;
        const int classMinHitpoints = 6;

        public Faerie()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Faerie();
        }
        
        public override int BaseSpeed()
        {
            return 150;
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
            return 12;
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
            return 4;
        }

        public override double GetMissileRange()
        {
            return 4.5;
        }

        protected override string GetWeaponName()
        {
            return "sprinkles caustic dust";
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "faerie"; } }

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
            return 15;
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

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.Cyan;
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


            //highestSkill = highestSkill / 2;
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
            return 40;
        }
        

    }
}
