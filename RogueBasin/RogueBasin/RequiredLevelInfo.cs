using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public class RequiredLevelInfo
    {
        public LevelId LevelId { get; set; }
        public IEnumerable<LevelId> HarderLevels { get; set; }
    }
}
