using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    /// <summary>
    /// Level request from a quest.
    /// At the moment we assume there is only 1 type of each level in the game
    /// </summary>
    public class RequiredLevelInfo
    {
        public RequiredLevelInfo(LevelType type)
        {
            this.LevelType = type;
        }

        public LevelType LevelType { get; set; }

        public static bool operator ==(RequiredLevelInfo i, RequiredLevelInfo j)
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
            if (i.LevelType == j.LevelType)
                return true;
            return false;
        }

        public static bool operator !=(RequiredLevelInfo i, RequiredLevelInfo j)
        {
            return !(i == j);
        }

        public override bool Equals(object obj)
        {
            //Value-wise comparison ensured by the cast
            return this == (RequiredLevelInfo)obj;
        }

        public override int GetHashCode()
        {
            return LevelType.GetHashCode();
        }
    }
}
