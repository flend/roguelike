using GraphMap;
using System.Collections.Generic;

namespace RogueBasin
{
    public class LevelInfo
    {
        private string levelName;
        private string levelReadableName;
        private LevelType levelType;
        private int startVertex;

        public LevelInfo(int levelNo, LevelType levelType, int startVertex, string levelName, string levelReadableName)
        {
            LevelNo = levelNo;
            LevelName = levelName;
            LevelReadableName = levelReadableName;
            LevelType = levelType;
            StartVertex = startVertex;

            ConnectionsToOtherLevels = new Dictionary<int, Connection>();
            ReplaceableVaultConnections = new List<Connection>();
            ReplaceableVaultConnectionsUsed = new List<Connection>();
        }

        public int LevelNo { get; private set; }
        public string LevelName { get; private set; }
        public string LevelReadableName { get; private set; }
        public LevelType LevelType { get; private set; }
        public int StartVertex { get; private set; }
        
        public Dictionary<int, Connection> ConnectionsToOtherLevels { get; set; }

        public TemplatedMapGenerator LevelGenerator { get; set; }
        public TemplatedMapBuilder LevelBuilder { get; set; }

        //Replaceable vault at target
        public List<Connection> ReplaceableVaultConnections { get; set; }
        public List<Connection> ReplaceableVaultConnectionsUsed { get; set; }

        public Dictionary<MapTerrain, List<MapTerrain>> TerrainMapping { get; set; }
    }
}
