using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public class RequiredLevelInfo
    {
        public RequiredLevelInfo(LevelType type)
        {
            this.LevelType = type;
        }

        public LevelType LevelType { get; set; }
    }
}
