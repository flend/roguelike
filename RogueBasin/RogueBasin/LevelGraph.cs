using GraphMap;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Search;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    /// <summary>
    /// Represents the level tree in the game, keeps data about levels and exposes useful utility methods
    /// </summary>
    public class LevelGraph
    {
        private readonly ConnectivityMap levelLinks;
        private readonly int startLevel;
        private readonly Dictionary<int, LevelInfo> levelInfo;

        private List<int> gameLevels;

        private ImmutableDictionary<string, int> levelIds;
        private ImmutableDictionary<int, string> levelNames;
        private ImmutableDictionary<int, string> levelReadableNames;

        private ImmutableDictionary<int, int> levelDifficulty;
        private Dictionary<int, int> levelDepths;

        public LevelGraph(Dictionary<int, LevelInfo> levelInfo, ConnectivityMap levelLinks, int startLevel)
        {
            this.levelLinks = levelLinks;
            this.startLevel = startLevel;
            this.levelInfo = levelInfo;

            CalculateDerivedMetrics();
        }

        private void CalculateDerivedMetrics()
        {
            gameLevels = levelLinks.GetAllConnections().SelectMany(c => new List<int> { c.Source, c.Target }).Distinct().OrderBy(c => c).ToList();

            var levelMap = new MapModel(levelLinks, startLevel);
            levelDepths = levelMap.GetDistanceOfVerticesFromParticularVertexInFullMap(startLevel, gameLevels);
            foreach (var kv in levelDepths)
            {
                LogFile.Log.LogEntryDebug("Level " + kv.Key + " depth " + kv.Value, LogDebugLevel.Medium);
            }

            //Level name and id lookup
            levelNames = levelInfo.ToImmutableDictionary(i => i.Value.LevelNo, i => i.Value.LevelName);
            levelReadableNames = levelInfo.ToImmutableDictionary(i => i.Value.LevelNo, i => i.Value.LevelReadableName);
            levelIds = levelInfo.ToImmutableDictionary(i => i.Value.LevelName, i => i.Value.LevelNo);
            //TODO: fix
            levelDifficulty = levelInfo.ToImmutableDictionary(i => i.Value.LevelNo, i => 0);
        }
                
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
        
        public IEnumerable<Connection> GetPathBetweenLevels(int startLevel, int endLevel)
        {
            var tryGetPath = levelLinks.RoomConnectionGraph.ShortestPathsDijkstra(x => 1, startLevel);

            IEnumerable<TaggedEdge<int, string>> path;
            if (tryGetPath(endLevel, out path))
            {
                return path.Select(e => new Connection(e.Source, e.Target));
            }
            else
            {
                return new List<Connection>();
            }
        }

        public ConnectivityMap LevelLinks
        {
            get
            {
                return levelLinks;
            }
        }


        public ImmutableDictionary<int, int> LevelDifficulty { get { return levelDifficulty; } }

        public List<int> GameLevels { get { return gameLevels; } }

        public ImmutableDictionary<string, int> LevelIds { get { return levelIds; } }
        public ImmutableDictionary<int, string> LevelNames { get { return levelNames; } }
        public ImmutableDictionary<int, string> LevelReadableNames { get { return levelNames; } }

        public Dictionary<int, int> LevelDepths { get { return levelDepths; } }


        public int StartLevel { get { return startLevel; } }

        public Dictionary<int, LevelInfo> LevelInfo { get { return levelInfo; } }
    }
}
