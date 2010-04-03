using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Medium threat. Fast but weak missile.
    /// </summary>
    public class DragonUnique : MonsterSpecialAI
    {
        const int classDeltaHitpoints = 20;
        const int classMinHitpoints = 30;

        public string UniqueName;

        public DragonUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
            Speed = 100;
            Unique = true;
            UniqueName = "Fafir the Fiery";
        }

        public override Monster NewCreatureOfThisType()
        {
            return new DragonUnique();
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
            return 8;
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
            return 4.5;
        }

        protected override string GetWeaponName()
        {
            return "breathes fire";
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return UniqueName; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return UniqueName; } }

        protected override char GetRepresentation()
        {
            return 'D';
        }

        protected override int RelaxDirectionAt()
        {
            return 100;
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

        protected override int GetTotalFleeLoops()
        {
            return 100;
        }
        public override int CreatureCost()
        {
            return 40;
        }

        public override int CreatureLevel()
        {
            return 3;
        }

        public override Color CreatureColor()
        {
            return ColorPresets.Red;
        }

        public override int GetMagicXP()
        {
            return 230;
        }

        public override int GetCombatXP()
        {
            return 235;
        }

        public override int GetMagicRes()
        {
            return 20;
        }

        public override int GetCharmRes()
        {
            return 50;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }

        int nextMove = -1;

        protected override string EffectAttackString()
        {
            //Set nextMove

            nextMove = Game.Random.Next(4);

            switch (nextMove)
            {
                case 0:
                    return "blink";
                    
                case 1:
                    return "slow";
                   
                case 2:
                    return "blind";
                   
                case 3:
                    return "ignores";
                   
            }

            return "";
        }

        protected override bool CreatureWillBackAway()
        {
            return false;
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
            return SpecialAIType.Dragon;
        }

        protected override int GetUseSpecialChance()
        {
            return 80;
        }
        

    }
}