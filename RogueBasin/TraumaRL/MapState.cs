using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraumaRL
{
    public class MapState
    {
        MapInfo mapInfo;
        DoorAndClueManager doorAndClueManager;
        MapPopulator populator;

        public MapState()
        {
            populator = new MapPopulator();
        }

        public void UpdateMapInfoWithNewLevelMaps(ConnectivityMap levelLinks, Dictionary<int, LevelInfo> levelInfo, int startLevel)
        {
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
        }

        public void InitialiseDoorAndClueManager(int startVertex)
        {
            //This must be done after BuildConnectedMapModel() called
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


    }
}
