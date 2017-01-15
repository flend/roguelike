using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public class LevelIdData
    {
        public readonly string name;
        public readonly LevelType type;
        public readonly int id;

        public LevelIdData(string name, LevelType type, int id)
        {
            this.name = name;
            this.type = type;
            this.id = id;
        }
    }

    /// <summary>
    /// Assigns continguous ids to new levels
    /// Stores difficulty relationship between levels
    /// </summary>
    public class LevelRegister
    {
        private int nextLevel = 0;

        private Dictionary<int, RequiredLevelInfo> levelInfo = new Dictionary<int, RequiredLevelInfo>();
        private Dictionary<RequiredLevelInfo, int> levelInfoReverse = new Dictionary<RequiredLevelInfo, int>();
        private DirectedGraphWrapper graphWrapper = new DirectedGraphWrapper();


        public LevelRegister()
        {

        }

        public LevelIdData GetIdForLevelType(RequiredLevelInfo newLevelInfo)
        {
            if (levelInfoReverse.ContainsKey(newLevelInfo))
            {
                var id = levelInfoReverse[newLevelInfo];
                return new LevelIdData(LevelNaming.LevelNames[newLevelInfo.LevelType], newLevelInfo.LevelType, id);
            }
                        
            var newIdLevel = nextLevel;
            levelInfo[nextLevel] = newLevelInfo;
            levelInfoReverse[newLevelInfo] = nextLevel;

            nextLevel++;

            return new LevelIdData(LevelNaming.LevelNames[newLevelInfo.LevelType], newLevelInfo.LevelType, newIdLevel);
        }

        public void RegisterAscendingDifficultyRelationship(int easierLevel, int harderLevel)
        {
            if(!levelInfo.Keys.Contains(easierLevel)) {
                var errorMsg = "Error: easier level: " + easierLevel + " not in registered levels";
                LogFile.Log.LogEntry(errorMsg);
                throw new ApplicationException(errorMsg);
            }

            if(!levelInfo.Keys.Contains(harderLevel)) {
                var errorMsg = "Error: harder level: " + harderLevel + " not in registered levels";
                LogFile.Log.LogEntry(errorMsg);
                throw new ApplicationException(errorMsg);
            }
            graphWrapper.AddSourceDestEdge(easierLevel, harderLevel);
        }


        public Dictionary<int, RequiredLevelInfo> LevelInfo {
            get {
                return levelInfo;
            }
        }

        public DirectedGraphWrapper DifficultyGraph
        {
            get
            {
                return graphWrapper;
            }
        }
    }
}
