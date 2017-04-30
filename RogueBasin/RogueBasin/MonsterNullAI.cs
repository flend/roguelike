namespace RogueBasin
{
    /// <summary>
    /// Monster doesn't do anything
    /// </summary>
    public abstract class MonsterNullAI : Monster
    {
        public MonsterNullAI()
        {
        }

        public override void ProcessTurn()
        {
        }

        protected override string HitsPlayerCombatString()
        {
            return "";
        }

        protected override string MissesPlayerCombatString()
        {
            return "";
        }

        protected override string HitsMonsterCombatString(Monster target)
        {
            return "";
        }

        protected override string MissesMonsterCombatString(Monster target)
        {
            return "";
        }

        public override CombatResults AttackMonster(Monster monster, bool ranged)
        {
            return CombatResults.NeitherDied;
        }

        public override CombatResults AttackPlayer(Player player, bool ranged)
        {
            return CombatResults.NeitherDied;
        }

        public override void NotifyAttackByCreature(Creature creature)
        {
            
        }

        public override void NotifyHitByCreature(Creature creature, int damage)
        {
            
        }

        public override void NotifyMonsterDeath()
        {
           
        }

        public override bool OnPatrol()
        {
            return true;
            //always
        }

        public override bool ShowHeading()
        {
            return false;
        }
    }
}
