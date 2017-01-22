using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL.Quests
{
    class LowerAtriumQuest : Quest
    {
        public LowerAtriumQuest(QuestMapBuilder builder, LogGenerator logGen)
            : base(builder, logGen)
        {
        }

        public override void SetupQuest(MapState mapState)
        {
            //No action, this quest only exists to ensure the lowerAtrium is easier than other levels in the game
        }

        public override void RegisterLevels(LevelRegister register)
        {
            var lowerAtriumLevelData = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.LowerAtriumLevel));
            var storageLevelData = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.StorageLevel));
            var scienceLevelData = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.ScienceLevel));

            register.RegisterAscendingDifficultyRelationship(lowerAtriumLevelData.id, storageLevelData.id);
            register.RegisterAscendingDifficultyRelationship(lowerAtriumLevelData.id, scienceLevelData.id);
        }
    
    }
}
