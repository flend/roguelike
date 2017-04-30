﻿namespace RogueBasin.Creatures
{
    /// <summary>
    /// Slower. Quite clever missile troop
    /// </summary>
    public class Nymph : MonsterSpecialAI
    {
        const int classDeltaHitpoints = 5;
        const int classMinHitpoints = 10;

        public Nymph()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
        }

        public override Monster NewCreatureOfThisType()
        {
            return new Nymph();
        }

        public override int BaseSpeed()
        {
            return 80;
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
            return 4;
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
            return 3;
        }

        protected override int GetUseSpecialChance()
        {
            return 95;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "nymph"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "nymphs"; } }

        protected override char GetRepresentation()
        {
            return 'N';
        }

        protected override SpecialAIType GetSpecialAIType()
        {
            return SpecialAIType.PlayerEffecter;
        }

        protected override int RelaxDirectionAt()
        {
            return 25;
        }

        protected override int GetTotalFleeLoops()
        {
            return 25;
        }

        public override double GetMissileRange()
        {
            return 4;
        }

        protected override string GetWeaponName()
        {
            return "throws a stick";
        }

        public override int CreatureCost()
        {
            return 50;
        }

        public override int CreatureLevel()
        {
            return 4;
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.CornflowerBlue;
        }

        public override int GetMagicXP()
        {
            return 60;
        }

        public override int GetCombatXP()
        {
            return 30;
        }

        public override int GetMagicRes()
        {
            return 70;
        }

        public override int GetCharmRes()
        {
            return 50;
        }

        public override bool CanBeCharmed()
        {
            return true;
        }

        protected override string EffectAttackString()
        {
            return "slow";
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
            int duration = 2 * Creature.turnTicks + Game.Random.Next(5 * Creature.turnTicks);

            PlayerEffects.SpeedDown speedDownEff = new RogueBasin.PlayerEffects.SpeedDown(duration, 30);

            return speedDownEff;
        }
    }
}
