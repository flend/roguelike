using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public class LevelAndDifficulty
    {
        public readonly int level;
        public readonly int difficulty;

        public LevelAndDifficulty(int level, int difficulty)
        {
            this.level = level;
            this.difficulty = difficulty;
        }

        public static bool operator ==(LevelAndDifficulty i, LevelAndDifficulty j)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(i, j))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)i == null) || ((object)j == null))
            {
                return false;
            }

            // Return true if the fields match:
            if (i.level == j.level && i.difficulty == j.difficulty)
                return true;
            return false;
        }

        public static bool operator !=(LevelAndDifficulty i, LevelAndDifficulty j)
        {
            return !(i == j);
        }

        public override bool Equals(object obj)
        {
            //Value-wise comparison ensured by the cast
            return this == (LevelAndDifficulty)obj;
        }

        public override int GetHashCode()
        {
            return level + 17 * difficulty;
        }
    }
}
