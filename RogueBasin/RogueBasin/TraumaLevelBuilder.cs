using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    public class LevelInfo
    {
        private string levelName;
        private string levelReadableName;

        public LevelInfo(int levelNo, string levelName, string levelReadableName)
        {
            LevelNo = levelNo;
            LevelName = levelName;
            LevelReadableName = levelReadableName;

            ConnectionsToOtherLevels = new Dictionary<int, Connection>();
            ReplaceableVaultConnections = new List<Connection>();
            ReplaceableVaultConnectionsUsed = new List<Connection>();
        }

        public int LevelNo { get; private set; }
        public string LevelName { get; private set; }
        public string LevelReadableName { get; private set; }
        
        public Dictionary<int, Connection> ConnectionsToOtherLevels { get; set; }

        public TemplatedMapGenerator LevelGenerator { get; set; }
        public TemplatedMapBuilder LevelBuilder { get; set; }

        //Replaceable vault at target
        public List<Connection> ReplaceableVaultConnections { get; set; }
        public List<Connection> ReplaceableVaultConnectionsUsed { get; set; }

        public Dictionary<MapTerrain, List<MapTerrain>> TerrainMapping { get; set; }
    }
}
