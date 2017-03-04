using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public abstract class LevelBuilder
    {
        public abstract LevelInfo GenerateLevel(int levelNo);
        public abstract LevelInfo CompleteLevel();
    }
}
