﻿namespace RogueBasin
{
    public abstract class Quest
    {
        private QuestMapBuilder builder;
        private LogGenerator logGen;

        public Quest(QuestMapBuilder builder, LogGenerator logGen)
        {
            this.builder = builder;
            this.logGen = logGen;
        }

        public abstract void RegisterLevels(LevelRegister register);
        public abstract void SetupQuest(MapState mapState);

        protected QuestMapBuilder Builder { get { return builder; } }
        protected LogGenerator LogGen { get { return logGen; } }
    }
}
