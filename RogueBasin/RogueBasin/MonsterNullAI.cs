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

        public override CombatResults AttackMonster(Monster monster)
        {
            return CombatResults.NeitherDied;
        }

        public override CombatResults AttackPlayer(Player player)
        {
            return CombatResults.NeitherDied;
        }
    }
}
