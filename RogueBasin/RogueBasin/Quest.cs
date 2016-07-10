using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public abstract class Quest
    {
        private QuestMapBuilder builder;
        private MapState mapState;
        private LogGenerator logGen;

        public Quest(MapState mapState, QuestMapBuilder builder, LogGenerator logGen)
        {
            this.mapState = mapState;
            this.builder = builder;
            this.logGen = logGen;
        }

        public abstract void SetupQuest();

        protected QuestMapBuilder Builder { get { return builder; } }
        protected MapState MapState { get { return mapState; } }
        protected LogGenerator LogGen { get { return logGen; } }
    }
}
