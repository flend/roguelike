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
        ConnectivityMap levelLinks;
        ImmutableDictionary<string, int> levelIds;
        ImmutableDictionary<int, string> levelNames;
        ImmutableDictionary<int, string> levelReadableNames;
        int startLevel;
        int startVertex;
                
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

        /// <summary>
        /// Call if the contents of levelLinks or levelInfo has changed (e.g. to add a new room).
        /// Rebuilds mapInfo and related state
        /// </summary>
        public void RefreshLevelMaps()
        {
            InitialiseWithLevelMaps(levelLinks, levelInfo, startLevel, startVertex);
            doorAndClueManager = new DoorAndClueManager(this.doorAndClueManager, MapInfo.Model.GraphNoCycles, MapInfo.Model.GraphNoCycles.roomMappingFullToNoCycleMap[startVertex]);
        }

        /// <summary>
        /// Call initially to initialise the MapState ready for use.
        /// Builds the full connected map representation
        /// </summary>
        /// <param name="levelLinks"></param>
        /// <param name="levelInfo"></param>
        /// <param name="startLevel"></param>
        /// <param name="startVertex"></param>
        public void BuildLevelMaps(ConnectivityMap levelLinks, Dictionary<int, LevelInfo> levelInfo, int startLevel, int startVertex)
        {
            InitialiseWithLevelMaps(levelLinks, levelInfo, startLevel, startVertex);
            doorAndClueManager = new DoorAndClueManager(MapInfo.Model.GraphNoCycles, MapInfo.Model.GraphNoCycles.roomMappingFullToNoCycleMap[startVertex]);
        }

        /// <summary>
        /// Process the level maps and links to build mapInfo and related state
        /// </summary>
        private void InitialiseWithLevelMaps(ConnectivityMap levelLinks, Dictionary<int, LevelInfo> levelInfo, int startLevel, int startVertex)
        {
            this.levelInfo = levelInfo;
            this.levelLinks = levelLinks;
            this.startLevel = startLevel;
            this.startVertex = startVertex;
            
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

            CopyLevelMapRoomFeaturesIfNotAlreadyPresent(levelInfo);

            //Level name and id lookup
            levelNames = levelInfo.ToImmutableDictionary(i => i.Value.LevelNo, i => i.Value.LevelName);
            levelReadableNames = levelInfo.ToImmutableDictionary(i => i.Value.LevelNo, i => i.Value.LevelReadableName);
            levelIds = levelInfo.ToImmutableDictionary(i => i.Value.LevelName, i => i.Value.LevelNo);
        }

        /// <summary>
        /// Copy any features from the templates into the mapPopulator.
        /// Since this may be run many times, we avoid copying features that are already there.
        /// </summary>
        /// <param name="levelInfo"></param>
        private void CopyLevelMapRoomFeaturesIfNotAlreadyPresent(Dictionary<int, LevelInfo> levelInfo)
        {
            foreach (var levelPair in levelInfo) {
                var thisLevelId = levelPair.Key;
                var thisLevel = levelPair.Value;
                var thisLevelGen = thisLevel.LevelGenerator;

                var thisLevelRoomTemplates = thisLevelGen.GetRoomTemplatesInWorldCoords();

                foreach (var roomTemplate in thisLevelRoomTemplates)
                {
                    foreach (var featureLoc in roomTemplate.Room.Features)
                    {
                        var featureWorldPos = featureLoc.Key + new Point(roomTemplate.X, roomTemplate.Y);
                        if (!populator.RoomInfo(roomTemplate.RoomIndex).IsFeatureAt(new Location(thisLevelId, featureWorldPos)))
                        {
                            populator.AddFeatureToRoom(mapInfo, roomTemplate.RoomIndex, featureWorldPos, featureLoc.Value.CreateFeature());
                        }
                    }
                }
                
            }
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

        public int StartLevel { get { return startLevel; } }
        public int StartVertex { get { return startVertex; } }
    }
}
