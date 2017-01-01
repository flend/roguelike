﻿using QuickGraph;
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
        private DirectedGraphWrapper graphWrapper = new DirectedGraphWrapper();
        
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
