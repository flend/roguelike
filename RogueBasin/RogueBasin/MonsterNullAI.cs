using System;
using System.Collections.Generic;
using System.Text;

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

        public override CombatResults AttackMonster(Monster monster)
        {
            return CombatResults.NeitherDied;
        }

        public override CombatResults AttackPlayer(Player player)
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
    }
}
