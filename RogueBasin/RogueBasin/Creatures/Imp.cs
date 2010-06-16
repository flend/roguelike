using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Blinder. Quite clever missile troop
    /// </summary>
    public class Imp : MonsterSpecialAI
    {
        const int classDeltaHitpoints = 15;
        const int classMinHitpoints = 5;

        public Imp()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Imp();
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
            return 6;
        }

        /// <summary>
        /// Creature damage modifier.  Set by type of creature.
        /// </summary>
        public override int DamageModifier()
        {
            return 0;
        }

        public override int HitModifier()
        {
            return 4;
        }

        protected override int GetUseSpecialChance()
        {
            return 20;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "imp"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "imps"; } }

        protected override char GetRepresentation()
        {
            return 'I';
        }

        protected override SpecialAIType GetSpecialAIType()
        {
            return SpecialAIType.PlayerEffecter;
        }

        protected override int RelaxDirectionAt()
        {
            return 20;
        }

        protected override int GetTotalFleeLoops()
        {
            return 20;
        }

        protected override double GetMissileRange()
        {
            return 4;
        }

        protected override string GetWeaponName()
        {
            return "fires a sphere of fire";
        }

        public override int CreatureCost()
        {
            return 50;
        }

        public override int CreatureLevel()
        {
            return 4;
        }

        public override Color CreatureColor()
        {
            return ColorPresets.OrangeRed;
        }

        public override int GetMagicXP()
        {
            return 70;
        }

        public override int GetCombatXP()
        {
            return 70;
        }

        public override int GetMagicRes()
        {
            return 40;
        }

        public override int GetCharmRes()
        {
            return 60;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }

        protected override string EffectAttackString()
        {
            return "blind";
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

        protected override PlayerEffect GetSpecialAIEffect()
        {
            int duration = 250 + Game.Random.Next(500);
            int playerSight = Game.Dungeon.Player.SightRadius;
            int sightDown = playerSight - 1;

            PlayerEffects.SightRadiusDown sightDownEff = new RogueBasin.PlayerEffects.SightRadiusDown(duration, sightDown);

            return sightDownEff;
        }
    }
}
