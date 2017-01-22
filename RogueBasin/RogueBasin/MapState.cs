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

        LevelGraph levelGraph;
        
        IEnumerable<int> allReplaceableVaults;

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
            InitialiseWithLevelMaps(levelGraph.LevelLinks, levelGraph.LevelInfo, StartLevel);
            doorAndClueManager = new DoorAndClueManager(this.doorAndClueManager, MapInfo.Model.GraphNoCycles, MapInfo.Model.GraphNoCycles.roomMappingFullToNoCycleMap[StartVertex]);
        }

        /// <summary>
        /// Call initially to initialise the MapState ready for use.
        /// Builds the full connected map representation
        /// </summary>
        /// <param name="levelLinks"></param>
        /// <param name="levelInfo"></param>
        /// <param name="startLevel"></param>
        /// <param name="startVertex"></param>
        public void BuildLevelMaps(ConnectivityMap levelLinks, Dictionary<int, LevelInfo> levelInfo, int startLevel)
        {
            InitialiseWithLevelMaps(levelLinks, levelInfo, startLevel);
            doorAndClueManager = new DoorAndClueManager(MapInfo.Model.GraphNoCycles, MapInfo.Model.GraphNoCycles.roomMappingFullToNoCycleMap[StartVertex]);
        }

        /// <summary>
        /// Process the level maps and links to build mapInfo and related state
        /// </summary>
        private void InitialiseWithLevelMaps(ConnectivityMap levelLinks, Dictionary<int, LevelInfo> levelInfo, int startLevel)
        {
            this.levelGraph = new LevelGraph(levelInfo, levelLinks, startLevel);
                       
            //Build the room graph containing all levels

            //Build and add the start level

            var mapInfoBuilder = new MapInfoBuilder();
            var startLevelInfo = levelInfo[startLevel];
            var startVertex = levelInfo[startLevel].StartVertex;
            mapInfoBuilder.AddConstructedLevel(startLevel, startLevelInfo.LevelGenerator.ConnectivityMap, startLevelInfo.LevelGenerator.GetRoomTemplatesInWorldCoords(),
                startLevelInfo.LevelGenerator.GetDoorsInMapCoords(), startVertex);

            //Build and add each connected level
            //Needs to be done in DFS fashion so we don't add the same level twice

            var levelsAdded = new HashSet<int> { startLevel };

            //This is a slight abuse of MapModel which has more functionality than needed
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

            CopyLevelMapRoomFeaturesIfNotAlreadyPresent(levelInfo);

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
                        if (!populator.RoomInfo(roomTemplate.RoomIndex).IsFeatureAt(featureLoc.Key))
                        {
                            populator.AddFeatureToRoom(mapInfo, roomTemplate.RoomIndex, featureLoc.Key, featureLoc.Value.CreateFeature());
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
       
        public IEnumerable<int> AllReplaceableVaults { get { return allReplaceableVaults; } set { allReplaceableVaults = value; } }

        public LevelGraph LevelGraph { get { return levelGraph; } }

        public int StartLevel { get { return levelGraph.StartLevel; } }
        public int StartVertex { get { return mapInfo.StartRoom; } }
    }
}
