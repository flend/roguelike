using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    public class MapState
    {
        MapInfo mapInfo;
        DoorAndClueManager doorAndClueManager;
        MapPopulator populator;
        Dictionary<int, LevelInfo> levelInfo;
        List<int> gameLevels;
        ImmutableDictionary<string, int> levelIds;
        ImmutableDictionary<int, string> levelNames;
        ImmutableDictionary<int, string> levelReadableNames;
        
        Dictionary<int, int> levelDifficulty = new Dictionary<int,int>();
        IEnumerable<int> allReplaceableVaults;
        Dictionary<int, int> levelDepths;

        /// <summary>
        /// A way of communicating between the level generation and quest generation.
        /// Not sure we will keep this in a more dynamic setup
        /// </summary>
        Dictionary<string, Connection> connectionStore = new Dictionary<string,Connection>();

        public MapState()
        {
            populator = new MapPopulator();
        }

        public void UpdateWithNewLevelMaps(ConnectivityMap levelLinks, Dictionary<int, LevelInfo> levelInfo, int startLevel)
        {
            this.levelInfo = levelInfo;
            
            //Build the room graph containing all levels

            //Build and add the start level

            var mapInfoBuilder = new MapInfoBuilder();
            var startRoom = 0;
            var startLevelInfo = levelInfo[startLevel];
            mapInfoBuilder.AddConstructedLevel(startLevel, startLevelInfo.LevelGenerator.ConnectivityMap, startLevelInfo.LevelGenerator.GetRoomTemplatesInWorldCoords(),
                startLevelInfo.LevelGenerator.GetDoorsInMapCoords(), startRoom);

            //Build and add each connected level
            //Needs to be done in DFS fashion so we don't add the same level twice

            var levelsAdded = new HashSet<int> { startLevel };

            MapModel levelModel = new MapModel(levelLinks, startLevel);
            var vertexDFSOrder = levelModel.GraphNoCycles.mapMST.verticesInDFSOrder;

            foreach (var level in vertexDFSOrder)
            {
                var thisLevel = level;
                var thisLevelInfo = levelInfo[level];

                //Since links to other levels are bidirectional, ensure we only add each level once
                foreach (var connectionToOtherLevel in thisLevelInfo.ConnectionsToOtherLevels)
                {
                    var otherLevel = connectionToOtherLevel.Key;
                    var otherLevelInfo = levelInfo[otherLevel];

                    var thisLevelElevator = connectionToOtherLevel.Value.Target;
                    var otherLevelElevator = otherLevelInfo.ConnectionsToOtherLevels[thisLevel].Target;

                    var levelConnection = new Connection(thisLevelElevator, otherLevelElevator);

                    if (!levelsAdded.Contains(otherLevel))
                    {
                        mapInfoBuilder.AddConstructedLevel(otherLevel, otherLevelInfo.LevelGenerator.ConnectivityMap, otherLevelInfo.LevelGenerator.GetRoomTemplatesInWorldCoords(),
                        otherLevelInfo.LevelGenerator.GetDoorsInMapCoords(), levelConnection);

                        LogFile.Log.LogEntryDebug("Adding level connection " + thisLevelInfo.LevelNo + ":" + connectionToOtherLevel.Key + " via nodes" +
                            thisLevelElevator + "->" + otherLevelElevator, LogDebugLevel.Medium);

                        levelsAdded.Add(otherLevel);
                    }
                }
            }
            mapInfo = new MapInfo(mapInfoBuilder, populator);

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
        }

        public void InitialiseDoorAndClueManager(int startVertex)
        {
            //This must be done after UpdateMapInfoWithNewLevelMaps() called [the first time]
            doorAndClueManager = new DoorAndClueManager(MapInfo.Model.GraphNoCycles, MapInfo.Model.GraphNoCycles.roomMappingFullToNoCycleMap[startVertex]);
        }

        public DoorAndClueManager DoorAndClueManager { get { return doorAndClueManager; } }

        public MapInfo MapInfo
        {
            get
            {
                return mapInfo;
            }
        }

        public Dictionary<int, LevelInfo> LevelInfo { get { return levelInfo;  } }

        public Dictionary<int, int> LevelDifficulty { get { return levelDifficulty; } }

        public Dictionary<string, Connection> ConnectionStore { get { return connectionStore; } }

        public List<int> GameLevels { get { return gameLevels;  } }

        public ImmutableDictionary<string, int> LevelIds { get { return levelIds; } }
        public ImmutableDictionary<int, string> LevelNames { get { return levelNames; } }
        public ImmutableDictionary<int, string> LevelReadableNames { get { return levelNames; } }
        
        public IEnumerable<int> AllReplaceableVaults { get { return allReplaceableVaults; } set { allReplaceableVaults = value; } }

        public Dictionary<int, int> LevelDepths { get { return levelDepths; } }
    }
}
