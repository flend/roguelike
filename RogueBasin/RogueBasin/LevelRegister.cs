using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    /// <summary>
    /// Assigns continguous ids to new levels
    /// Stores difficulty relationship between levels
    /// </summary>
    public class LevelRegister
    {
        private int nextLevel = 0;

        private Dictionary<int, RequiredLevelInfo> levelInfo = new Dictionary<int, RequiredLevelInfo>();
        private AdjacencyGraph<int, TaggedEdge<int, string>> difficultyGraph = new AdjacencyGraph<int,TaggedEdge<int, string>>();
        
        public LevelRegister()
        {

        }

        public int RegisterNewLevel(RequiredLevelInfo newLevelInfo)
        {
            var newIdLevel = nextLevel;
            levelInfo[nextLevel] = newLevelInfo;

            nextLevel++;

            return newIdLevel;
        }

        public void RegisterDifficultyRelationship(int easierLevel, int harderLevel)
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

            TaggedEdge<int, string> possibleEdge = null;

            difficultyGraph.TryGetEdge(easierLevel, harderLevel, out possibleEdge);

            if (possibleEdge == null)
                difficultyGraph.AddVerticesAndEdge(new TaggedEdge<int, string>(easierLevel, harderLevel, ""));
        }

        public AdjacencyGraph<int, TaggedEdge<int, string>> DifficultyGraph
        {
            get
            {
                return difficultyGraph;
            }
        }

        public Dictionary<int, RequiredLevelInfo> LevelInfo {
            get {
                return levelInfo;
            }
        }
    }
}
