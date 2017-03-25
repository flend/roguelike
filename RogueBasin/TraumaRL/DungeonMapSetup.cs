using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL
{
    class DungeonMapSetup
    {

        public DungeonMapSetup()
        {
            BuildTerrainMapping();
            
        }

        private void BuildTerrainMapping()
        {
            terrainMapping = new Dictionary<RoomTemplateTerrain, MapTerrain>();
            terrainMapping[RoomTemplateTerrain.Wall] = MapTerrain.Wall;
            terrainMapping[RoomTemplateTerrain.Floor] = MapTerrain.Empty;
            terrainMapping[RoomTemplateTerrain.Transparent] = MapTerrain.Void;
            terrainMapping[RoomTemplateTerrain.WallWithPossibleDoor] = MapTerrain.ClosedDoor;

            brickTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {

                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.BrickWall1, MapTerrain.BrickWall1, MapTerrain.BrickWall1, MapTerrain.BrickWall2, MapTerrain.BrickWall3, MapTerrain.BrickWall4, MapTerrain.BrickWall5 } }};
        }        
    
        /// <summary>
        /// Mapping from template terrain to real terrain on the map
        /// </summary>
        Dictionary<RoomTemplateTerrain, MapTerrain> terrainMapping;

        Dictionary<MapTerrain, List<MapTerrain>> brickTerrainMapping;

        public void AddLevelMapsToDungeon(Dictionary<int, LevelInfo> levelInfo)
        {
            foreach (var kv in levelInfo.OrderBy(kv => kv.Key))
            {
                var thisLevelInfo = kv.Value;

                Map masterMap = thisLevelInfo.LevelBuilder.MergeTemplatesIntoMap(terrainMapping);

                Dictionary<MapTerrain, List<MapTerrain>> terrainSubstitution = brickTerrainMapping;
                if (thisLevelInfo.TerrainMapping != null)
                    terrainSubstitution = thisLevelInfo.TerrainMapping;

                Map randomizedMap = MapTerrainRandomizer.RandomizeTerrainInMap(masterMap, terrainSubstitution);
                Game.Dungeon.AddMap(randomizedMap);
            }
        }

        public void SetupMapsInEngine(MapState mapState)
        {
            //Comment for faster UI check
            Game.Dungeon.RefreshAllLevelPathingAndFOV();

            foreach (var level in mapState.LevelGraph.GameLevels)
            {
                Game.Dungeon.Levels[level].LightLevel = 0;
            }
        }


        public void AddMapObjectsToDungeon(MapInfo mapInfo)
        {
            var rooms = mapInfo.Populator.AllRoomsInfo();

            foreach (RoomInfo roomInfo in rooms)
            {
                var roomPositioned = mapInfo.Room(roomInfo.Id);
                var roomLocation = roomPositioned.Location;
                var roomLevel = mapInfo.GetLevelForRoomIndex(roomInfo.Id);

                foreach (TriggerRoomPlacement triggerPlacement in roomInfo.Triggers)
                {
                    bool monsterResult = Game.Dungeon.AddTrigger(triggerPlacement.trigger, new Location(roomLevel, roomLocation + triggerPlacement.location));

                    if (!monsterResult)
                    {
                        LogFile.Log.LogEntryDebug("Cannot add trigger to dungeon at: " + triggerPlacement.location, LogDebugLevel.Medium);
                    }
                }

                foreach (MonsterRoomPlacement monsterPlacement in roomInfo.Monsters)
                {
                    var absoluteLocation = roomLocation + monsterPlacement.location;
                    bool monsterResult = Game.Dungeon.AddMonster(monsterPlacement.monster, new Location(roomLevel, absoluteLocation));

                    if (!monsterResult)
                    {
                        LogFile.Log.LogEntryDebug("Cannot add monster to dungeon: " + monsterPlacement.monster.SingleDescription + " at: level: " + roomLevel + " point: " + absoluteLocation + " roomIndex: " + roomInfo.Id, LogDebugLevel.Medium);
                    }
                }

                foreach (ItemRoomPlacement itemPlacement in roomInfo.Items)
                {
                    bool monsterResult = Game.Dungeon.AddItem(itemPlacement.item, new Location(roomLevel, roomLocation + itemPlacement.location));

                    if (!monsterResult)
                    {
                        LogFile.Log.LogEntryDebug("Cannot add item to dungeon: " + itemPlacement.item.SingleItemDescription + " at: " + itemPlacement.location, LogDebugLevel.Medium);
                    }
                }

                foreach (FeatureRoomPlacement featurePlacement in roomInfo.Features)
                {
                    if (featurePlacement.feature.IsBlocking)
                    {
                        bool featureResult = Game.Dungeon.AddFeatureBlocking(featurePlacement.feature, roomLevel, roomLocation + featurePlacement.location, featurePlacement.feature.BlocksLight);

                        if (!featureResult)
                        {
                            LogFile.Log.LogEntryDebug("Cannot add blocking feature to dungeon: " + featurePlacement.feature.Description + " at: " + featurePlacement.location, LogDebugLevel.Medium);
                        }
                        else
                        {
                            LogFile.Log.LogEntryDebug("Adding blocking feature to dungeon, room : " + roomInfo.Id + " feature: " + featurePlacement.feature.Description + " at: " + featurePlacement.location, LogDebugLevel.Medium);
                        }
                    }
                    else
                    {
                        bool featureResult = Game.Dungeon.AddFeature(featurePlacement.feature, roomLevel, roomLocation + featurePlacement.location);

                        if (!featureResult)
                        {
                            LogFile.Log.LogEntryDebug("Cannot add feature to dungeon: " + featurePlacement.feature.Description + " at: " + featurePlacement.location, LogDebugLevel.Medium);
                        }
                        else
                        {
                            LogFile.Log.LogEntryDebug("Adding non-blocking feature to dungeon, room : " + roomInfo.Id + " feature: " + featurePlacement.feature.Description + " at: " + featurePlacement.location, LogDebugLevel.Medium);
                        }
                    }
                }

                foreach (LockRoomPlacement lockPlacement in roomInfo.Locks)
                {
                    var thisLock = lockPlacement.thisLock;
                    thisLock.LocationLevel = roomLevel;
                    thisLock.LocationMap = roomLocation + lockPlacement.location;
                    bool lockResult = Game.Dungeon.AddLock(lockPlacement.thisLock);

                    if (!lockResult)
                    {
                        LogFile.Log.LogEntryDebug("Cannot add lock to dungeon at: " + lockPlacement.location, LogDebugLevel.Medium);
                    }
                }
            }
        }

        public void SetPlayerStartLocation(MapState mapState)
        {
            var mapInfo = mapState.MapInfo;
            var firstRoom = mapInfo.Room(mapState.StartVertex);
            Game.Dungeon.Levels[mapState.StartLevel].PCStartLocation = new RogueBasin.Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);

            Game.Dungeon.Player.LocationLevel = mapState.StartLevel;
            Game.Dungeon.Player.LocationMap = Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation;
        }

        public void AddMapStatePropertiesToDungeon(MapState mapState)
        {
            Game.Dungeon.MapState = mapState;
        }
    }
}
