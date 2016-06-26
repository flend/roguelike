using GraphMap;
using libtcodWrapper;
using RogueBasin;
using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TraumaRL
{
    public partial class TraumaWorldGenerator
    {
        /// <summary>
        /// Mapping from template terrain to real terrain on the map
        /// </summary>
        Dictionary<RoomTemplateTerrain, MapTerrain> terrainMapping;

        Dictionary<MapTerrain, List<MapTerrain>> brickTerrainMapping;

        ConnectivityMap levelLinks;

        IEnumerable<int> allReplaceableVaults;

        //For development, skip making most of the levels
        bool quickLevelGen = false;

        List<int> gameLevels;
        static Dictionary<int, string> levelNaming;

        LogGenerator logGen = new LogGenerator();

        MapState mapState;

        static List<Tuple<System.Drawing.Color, string>> availableColors;
        static Dictionary<int, List<DecorationFeatureDetails.DecorationFeatures>> featuresByLevel;
        List<Tuple<System.Drawing.Color, string>> usedColors = new List<Tuple<System.Drawing.Color, string>>();

        public const int medicalLevel = 0;
        public const int lowerAtriumLevel = 1;
        public const int scienceLevel = 2;
        public const int storageLevel = 3;
        public const int flightDeck = 4;
        public const int reactorLevel = 5;
        public const int arcologyLevel = 6;
        public const int commercialLevel = 7;
        public const int computerCoreLevel = 8;
        public const int bridgeLevel = 9;

        Connection escapePodsConnection;

        public TraumaWorldGenerator()
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

        private static void SetupColors()
        {
            availableColors = new List<Tuple<System.Drawing.Color, string>> {
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Red, "red"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Coral, "coral"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Blue, "blue"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Orange, "orange"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Yellow, "yellow"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Khaki, "khaki"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Chartreuse, "chartreuse"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.HotPink, "hot pink"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Cyan, "cyan"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Lime, "lime"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Navy, "navy"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Tan, "tan"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Fuchsia, "fuchsia"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.GhostWhite, "ghost"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Teal, "teal"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Plum, "plum"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Plum, "wheat")
            };
        }

        static TraumaWorldGenerator() { 
            
            BuildLevelNaming();
            SetupColors();
            SetupFeatures();
        }

        private static void SetupFeatures()
        {

            featuresByLevel = new Dictionary<int, List<DecorationFeatureDetails.DecorationFeatures>>();

            featuresByLevel[medicalLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
            {
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                DecorationFeatureDetails.DecorationFeatures.Bone,
                DecorationFeatureDetails.DecorationFeatures.Skeleton,
                DecorationFeatureDetails.DecorationFeatures.Instrument1,
                DecorationFeatureDetails.DecorationFeatures.Instrument2,
                DecorationFeatureDetails.DecorationFeatures.Instrument3,
DecorationFeatureDetails.DecorationFeatures.MedicalAutomat,
DecorationFeatureDetails.DecorationFeatures.CoffeePC,
                DecorationFeatureDetails.DecorationFeatures.DesktopPC,
                DecorationFeatureDetails.DecorationFeatures.Chair1,
DecorationFeatureDetails.DecorationFeatures.Chair2,
                DecorationFeatureDetails.DecorationFeatures.Stool,
DecorationFeatureDetails.DecorationFeatures.Plant1,
DecorationFeatureDetails.DecorationFeatures.Plant2,
DecorationFeatureDetails.DecorationFeatures.Plant3,
DecorationFeatureDetails.DecorationFeatures.CleaningDevice,
DecorationFeatureDetails.DecorationFeatures.WheelChair,
                DecorationFeatureDetails.DecorationFeatures.Bin
            };

            featuresByLevel[lowerAtriumLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
            {
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                DecorationFeatureDetails.DecorationFeatures.Bone,
                DecorationFeatureDetails.DecorationFeatures.Skeleton,
                DecorationFeatureDetails.DecorationFeatures.Instrument1,
                DecorationFeatureDetails.DecorationFeatures.Instrument2,
                DecorationFeatureDetails.DecorationFeatures.Instrument3,
                DecorationFeatureDetails.DecorationFeatures.Plant1,
DecorationFeatureDetails.DecorationFeatures.Plant2,
DecorationFeatureDetails.DecorationFeatures.Plant3,
DecorationFeatureDetails.DecorationFeatures.Chair1,
DecorationFeatureDetails.DecorationFeatures.Chair1,
DecorationFeatureDetails.DecorationFeatures.Safe1,
DecorationFeatureDetails.DecorationFeatures.Safe2,
                DecorationFeatureDetails.DecorationFeatures.Statue1,
DecorationFeatureDetails.DecorationFeatures.Statue2,
DecorationFeatureDetails.DecorationFeatures.Statue3,
DecorationFeatureDetails.DecorationFeatures.Statue4,
DecorationFeatureDetails.DecorationFeatures.AutomatMachine,
DecorationFeatureDetails.DecorationFeatures.Bin
            };

            featuresByLevel[scienceLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
            {
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                DecorationFeatureDetails.DecorationFeatures.Bone,
                DecorationFeatureDetails.DecorationFeatures.Skeleton,
DecorationFeatureDetails.DecorationFeatures.CleaningDevice,
                DecorationFeatureDetails.DecorationFeatures.Instrument1,
                DecorationFeatureDetails.DecorationFeatures.Instrument2,
                DecorationFeatureDetails.DecorationFeatures.Instrument3,
                DecorationFeatureDetails.DecorationFeatures.MedicalAutomat,
DecorationFeatureDetails.DecorationFeatures.CoffeePC,
                DecorationFeatureDetails.DecorationFeatures.DesktopPC,
                DecorationFeatureDetails.DecorationFeatures.Chair1,
DecorationFeatureDetails.DecorationFeatures.Chair2,
                DecorationFeatureDetails.DecorationFeatures.Stool,
DecorationFeatureDetails.DecorationFeatures.Plant1,
DecorationFeatureDetails.DecorationFeatures.Plant2,
DecorationFeatureDetails.DecorationFeatures.Plant3,
DecorationFeatureDetails.DecorationFeatures.WheelChair,
                DecorationFeatureDetails.DecorationFeatures.Screen1,
                DecorationFeatureDetails.DecorationFeatures.Screen2,
                DecorationFeatureDetails.DecorationFeatures.Screen8,
                DecorationFeatureDetails.DecorationFeatures.Screen9
            };

            featuresByLevel[storageLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
            {
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                DecorationFeatureDetails.DecorationFeatures.Bone,
                DecorationFeatureDetails.DecorationFeatures.Skeleton,
                DecorationFeatureDetails.DecorationFeatures.Instrument1,
                DecorationFeatureDetails.DecorationFeatures.Instrument2,
DecorationFeatureDetails.DecorationFeatures.Crate,
                DecorationFeatureDetails.DecorationFeatures.Safe1,
DecorationFeatureDetails.DecorationFeatures.Safe2,
DecorationFeatureDetails.DecorationFeatures.Machine,
DecorationFeatureDetails.DecorationFeatures.Machine2,
                DecorationFeatureDetails.DecorationFeatures.MachinePart1,
DecorationFeatureDetails.DecorationFeatures.MachinePart2,
DecorationFeatureDetails.DecorationFeatures.MachinePart3,
                DecorationFeatureDetails.DecorationFeatures.Screen1,
                DecorationFeatureDetails.DecorationFeatures.Screen2,
                DecorationFeatureDetails.DecorationFeatures.Screen3,
                DecorationFeatureDetails.DecorationFeatures.Screen4
            };

            featuresByLevel[flightDeck] = new List<DecorationFeatureDetails.DecorationFeatures>
            {
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                DecorationFeatureDetails.DecorationFeatures.Bone,
                DecorationFeatureDetails.DecorationFeatures.Skeleton,
                DecorationFeatureDetails.DecorationFeatures.Instrument1,
                DecorationFeatureDetails.DecorationFeatures.Instrument2,
DecorationFeatureDetails.DecorationFeatures.Crate,
DecorationFeatureDetails.DecorationFeatures.Machine,
DecorationFeatureDetails.DecorationFeatures.Machine2,
DecorationFeatureDetails.DecorationFeatures.Computer1,
DecorationFeatureDetails.DecorationFeatures.Computer2,
DecorationFeatureDetails.DecorationFeatures.Computer3,
                DecorationFeatureDetails.DecorationFeatures.MachinePart1,
DecorationFeatureDetails.DecorationFeatures.MachinePart2,
DecorationFeatureDetails.DecorationFeatures.MachinePart3,
                DecorationFeatureDetails.DecorationFeatures.Screen1,
                DecorationFeatureDetails.DecorationFeatures.Screen2,
                DecorationFeatureDetails.DecorationFeatures.Screen3,
                DecorationFeatureDetails.DecorationFeatures.Pillar1,
DecorationFeatureDetails.DecorationFeatures.Pillar2,
                DecorationFeatureDetails.DecorationFeatures.Pillar3,
                DecorationFeatureDetails.DecorationFeatures.Screen8
            };

            featuresByLevel[reactorLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
            {
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                DecorationFeatureDetails.DecorationFeatures.Bone,
                DecorationFeatureDetails.DecorationFeatures.Skeleton,
                DecorationFeatureDetails.DecorationFeatures.Instrument1,
                DecorationFeatureDetails.DecorationFeatures.Instrument2,
DecorationFeatureDetails.DecorationFeatures.EggChair,
DecorationFeatureDetails.DecorationFeatures.Machine,
DecorationFeatureDetails.DecorationFeatures.Machine2,
                DecorationFeatureDetails.DecorationFeatures.MachinePart1,
DecorationFeatureDetails.DecorationFeatures.MachinePart2,
DecorationFeatureDetails.DecorationFeatures.MachinePart3,
DecorationFeatureDetails.DecorationFeatures.Computer1,
DecorationFeatureDetails.DecorationFeatures.Computer2,
DecorationFeatureDetails.DecorationFeatures.Computer3,
                DecorationFeatureDetails.DecorationFeatures.Screen1,
                DecorationFeatureDetails.DecorationFeatures.Screen2,
                DecorationFeatureDetails.DecorationFeatures.Screen3,
                DecorationFeatureDetails.DecorationFeatures.Screen4,
                DecorationFeatureDetails.DecorationFeatures.Screen6,
                DecorationFeatureDetails.DecorationFeatures.Screen7,
                DecorationFeatureDetails.DecorationFeatures.Screen8
            };

            featuresByLevel[arcologyLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
            {
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                DecorationFeatureDetails.DecorationFeatures.Bone,
                DecorationFeatureDetails.DecorationFeatures.Skeleton,
                DecorationFeatureDetails.DecorationFeatures.Instrument1,
                DecorationFeatureDetails.DecorationFeatures.Instrument2,
                DecorationFeatureDetails.DecorationFeatures.Egg1,
DecorationFeatureDetails.DecorationFeatures.Egg2,
DecorationFeatureDetails.DecorationFeatures.Egg3,
DecorationFeatureDetails.DecorationFeatures.Spike,
DecorationFeatureDetails.DecorationFeatures.CorpseinGoo,
DecorationFeatureDetails.DecorationFeatures.Machine,
DecorationFeatureDetails.DecorationFeatures.Machine2,
                DecorationFeatureDetails.DecorationFeatures.MachinePart1,
DecorationFeatureDetails.DecorationFeatures.MachinePart2,
DecorationFeatureDetails.DecorationFeatures.MachinePart3
              
            };

            featuresByLevel[commercialLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
            {
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                DecorationFeatureDetails.DecorationFeatures.Bone,
                DecorationFeatureDetails.DecorationFeatures.Skeleton,
                DecorationFeatureDetails.DecorationFeatures.Instrument1,
                DecorationFeatureDetails.DecorationFeatures.Instrument2,
DecorationFeatureDetails.DecorationFeatures.Crate,
                DecorationFeatureDetails.DecorationFeatures.Safe1,
DecorationFeatureDetails.DecorationFeatures.Safe2,
                DecorationFeatureDetails.DecorationFeatures.MachinePart1,
DecorationFeatureDetails.DecorationFeatures.MachinePart2,
DecorationFeatureDetails.DecorationFeatures.MachinePart3,
DecorationFeatureDetails.DecorationFeatures.ShopAutomat1,
DecorationFeatureDetails.DecorationFeatures.ShopAutomat2,
DecorationFeatureDetails.DecorationFeatures.Statue1,
DecorationFeatureDetails.DecorationFeatures.Statue2,
DecorationFeatureDetails.DecorationFeatures.Statue3,
DecorationFeatureDetails.DecorationFeatures.Statue4,
DecorationFeatureDetails.DecorationFeatures.AutomatMachine,
DecorationFeatureDetails.DecorationFeatures.Plant1,
DecorationFeatureDetails.DecorationFeatures.Plant2,
DecorationFeatureDetails.DecorationFeatures.Plant3,
DecorationFeatureDetails.DecorationFeatures.Pillar1,
DecorationFeatureDetails.DecorationFeatures.Pillar2,
                DecorationFeatureDetails.DecorationFeatures.Pillar3,
DecorationFeatureDetails.DecorationFeatures.CleaningDevice,
DecorationFeatureDetails.DecorationFeatures.Bin
              
            };

            featuresByLevel[computerCoreLevel] = featuresByLevel[reactorLevel];

            featuresByLevel[bridgeLevel] = featuresByLevel[flightDeck];

        }
        
        
        private static void BuildLevelNaming()
        {
            levelNaming = new Dictionary<int, string>();
            levelNaming[medicalLevel] = "Medical";
            levelNaming[lowerAtriumLevel] = "Lower Atrium";
            levelNaming[scienceLevel] = "Science";
            levelNaming[storageLevel] = "Storage";
            levelNaming[flightDeck] = "Flight deck";
            levelNaming[reactorLevel] = "Reactor";
            levelNaming[arcologyLevel] = "Arcology";
            levelNaming[commercialLevel] = "Commercial";
            levelNaming[bridgeLevel] = "Bridge";
            levelNaming[computerCoreLevel] = "Computer Core";
        }



        public ConnectivityMap LevelLinks { get { return levelLinks; } }

        public static Dictionary<int, string> LevelNaming { get { return levelNaming; } }

        MapModel levelMap;
        Dictionary<int, int> levelDepths;

        /// <summary>
        /// Build a level->level map showing how the levels are connected
        /// </summary>
        private ConnectivityMap GenerateLevelLinks()
        {
            var levelLinks = new ConnectivityMap();

            //Player starts in medical which links to the lower atrium
            levelLinks.AddRoomConnection(new Connection(medicalLevel, lowerAtriumLevel));

            if (!quickLevelGen)
            {
                var standardLowerLevels = new List<int> { scienceLevel, storageLevel, flightDeck, arcologyLevel, commercialLevel };

                //3 of these branch from the lower atrium
                var directLinksFromLowerAtrium = standardLowerLevels.RandomElements(3);

                foreach (var level in directLinksFromLowerAtrium)
                    levelLinks.AddRoomConnection(lowerAtriumLevel, level);

                //The remainder branch from other levels (except the arcology)
                var leafLevels = directLinksFromLowerAtrium.Select(x => x);
                leafLevels = leafLevels.Except(new List<int> { arcologyLevel });

                var allLowerLevelsToPlace = standardLowerLevels.Except(directLinksFromLowerAtrium).Union(new List<int> { reactorLevel });
                foreach (var level in allLowerLevelsToPlace)
                {
                    levelLinks.AddRoomConnection(leafLevels.RandomElement(), level);
                }

                //Bridge and computer core are also leaves
                var allLaterLevels = standardLowerLevels.Except(directLinksFromLowerAtrium);
                var finalLevelsToPlace = new List<int> { computerCoreLevel, bridgeLevel };
                foreach (var level in finalLevelsToPlace)
                {
                    levelLinks.AddRoomConnection(allLaterLevels.RandomElement(), level);
                }
            }
            gameLevels = levelLinks.GetAllConnections().SelectMany(c => new List<int> { c.Source, c.Target }).Distinct().OrderBy(c => c).ToList();

            //Calculate some data about the levels
            levelMap = new MapModel(levelLinks, medicalLevel);
            levelDepths = levelMap.GetDistanceOfVerticesFromParticularVertexInFullMap(medicalLevel, gameLevels);
            foreach (var kv in levelDepths)
            {
                LogFile.Log.LogEntryDebug("Level " + kv.Key + " depth " + kv.Value, LogDebugLevel.Medium);
            }

            return levelLinks;
        }

        public MapState MapState { get { return mapState; } }

        
        /** Build a map using templated rooms */
        public MapInfo GenerateTraumaLevels(bool retry)
        {
            //We catch exceptions on generation and keep looping

            //Reset shared state
            usedColors = new List<Tuple<System.Drawing.Color, string>>();

            //Generate the overall level structure
            levelLinks = GenerateLevelLinks();

            //Build each level individually
            TraumaLevelBuilder levelBuilder = new TraumaLevelBuilder(gameLevels, levelLinks, quickLevelGen);
            levelBuilder.GenerateLevels();
            
            Dictionary<int, LevelInfo> levelInfo = levelBuilder.LevelInfo;
            allReplaceableVaults = levelBuilder.AllReplaceableVaults;

            //Feels like there will be a more dynamic way of getting this state in future
            escapePodsConnection = levelBuilder.EscapePodsConnection;

            //Create the state object which will hold the map state in the generation phase

            var startVertex = 0;
            var startLevel = 0;
            mapState = new MapState();
            mapState.UpdateWithNewLevelMaps(levelLinks, levelInfo, startLevel);
            mapState.InitialiseDoorAndClueManager(startVertex);

            MapInfo mapInfo = mapState.MapInfo;
            
            //Add maps to the dungeon (must be ordered)
            AddLevelMapsToDungeon(levelInfo);

            //Set player's start location (must be done before adding items)
            SetPlayerStartLocation(mapInfo);

            //Add elevator features to link the maps
            if (!quickLevelGen)
                AddElevatorFeatures(mapInfo, levelInfo);
            
            //Attach debugger at this point
            MessageBox.Show("Attach debugger now for any generation post slow pathing setup");

            //Generate quests at mapmodel level
            GenerateQuests(mapInfo, levelInfo);

            //Place loot
            CalculateLevelDifficulty();

            if (!quickLevelGen)
                PlaceLootInArmory(mapInfo, levelInfo);

            if (!quickLevelGen)
                AddGoodyQuestLogClues(mapInfo, levelInfo);

            //Set maps in engine (needs to be done before placing items and monsters)
            SetupMapsInEngine();

            //Add non-interactable features
            AddDecorationFeatures(mapInfo, levelInfo);
            
            //Quests is being refactored to store information in MapInfo, rather than in the Dungeon
            //Need to add here the code which transfers the completed MapInfo creatures, features, items and locks into the Dungeon
            AddMapObjectsToDungeon(mapInfo);
            
            //Add monsters
            Game.Dungeon.MonsterPlacement.CreateMonstersForLevels(mapInfo, gameLevels, levelDifficulty);

            //Add debug stuff in the first room
            AddDebugItems(mapInfo);

            //Check we are solvable
            AssertMapIsSolveable(mapInfo, mapState.DoorAndClueManager);

            if (retry)
            {
                throw new ApplicationException("It happened!");
            }

            return mapInfo;
        }

        /// <summary>
        /// RoomPlacements currently contain absolute co-ordinates. I would prefer them to have relative coordinates, and those to get
        /// mapped to absolute coordinates here
        /// </summary>
        /// <param name="mapInfo"></param>
        private void AddMapObjectsToDungeon(MapInfo mapInfo)
        {
            var rooms = mapInfo.Populator.AllRoomsInfo();

            foreach (RoomInfo roomInfo in rooms)
            {
                foreach (MonsterRoomPlacement monsterPlacement in roomInfo.Monsters)
                {
                    bool monsterResult = Game.Dungeon.AddMonster(monsterPlacement.monster, monsterPlacement.location);

                    if (!monsterResult) {
                        LogFile.Log.LogEntryDebug("Cannot add monster to dungeon: " + monsterPlacement.monster.SingleDescription + " at: " + monsterPlacement.location, LogDebugLevel.Medium);
                    }
                }

                foreach (ItemRoomPlacement itemPlacement in roomInfo.Items)
                {
                    bool monsterResult = Game.Dungeon.AddItem(itemPlacement.item, itemPlacement.location);

                    if (!monsterResult)
                    {
                        LogFile.Log.LogEntryDebug("Cannot add item to dungeon: " + itemPlacement.item.SingleItemDescription + " at: " + itemPlacement.location, LogDebugLevel.Medium);
                    }
                }

                foreach (FeatureRoomPlacement featurePlacement in roomInfo.Features)
                {
                    if (featurePlacement.feature.IsBlocking)
                    {
                        bool featureResult = Game.Dungeon.AddFeatureBlocking(featurePlacement.feature, featurePlacement.location.Level, featurePlacement.location.MapCoord, featurePlacement.feature.BlocksLight);

                        if (!featureResult)
                        {
                            LogFile.Log.LogEntryDebug("Cannot add blocking feature to dungeon: " + featurePlacement.feature.Description + " at: " + featurePlacement.location, LogDebugLevel.Medium);
                        }
                    }
                    else
                    {
                        bool featureResult = Game.Dungeon.AddFeature(featurePlacement.feature, featurePlacement.location.Level, featurePlacement.location.MapCoord);

                        if (!featureResult)
                        {
                            LogFile.Log.LogEntryDebug("Cannot add feature to dungeon: " + featurePlacement.feature.Description + " at: " + featurePlacement.location, LogDebugLevel.Medium);
                        }
                    }
                }
            }

            foreach (var doorInfo in mapInfo.Populator.DoorInfo)
            {
                var door = doorInfo.Value;

                foreach(var doorLock in door.Locks) {
                    Game.Dungeon.AddLock(doorLock);
                }
            }
        }

        private void AssertMapIsSolveable(MapInfo mapInfo, DoorAndClueManager doorAndClueManager)
        {
            var graphSolver = new GraphSolver(mapInfo.Model, doorAndClueManager);
            if (!graphSolver.MapCanBeSolved())
            {
                LogFile.Log.LogEntryDebug("MAP CAN'T BE SOLVED!", LogDebugLevel.High);
                throw new ApplicationException("It's all over - map can't be solved.");
            }
            else
            {
                LogFile.Log.LogEntryDebug("Phew - map can be solved", LogDebugLevel.High);
            }

            if (!quickLevelGen && !RoutabilityUtilities.CheckItemRouteability())
            {
                throw new ApplicationException("Item is not connected to elevator, aborting.");
            }

            if (!quickLevelGen && !RoutabilityUtilities.CheckFeatureRouteability())
            {
                //throw new ApplicationException("Feature is not connected to elevator, aborting.");
            }
        }

        private static void SetPlayerStartLocation(MapInfo mapInfo)
        {
            var firstRoom = mapInfo.Room(0);
            Game.Dungeon.Levels[0].PCStartLocation = new RogueBasin.Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);
        }


        private void AddLevelMapsToDungeon(Dictionary<int, LevelInfo> levelInfo)
        {
            foreach (var kv in levelInfo.OrderBy(kv => kv.Key))
            {
                var thisLevelInfo = kv.Value;

                Map masterMap = thisLevelInfo.LevelBuilder.MergeTemplatesIntoMap(terrainMapping);

                Dictionary<MapTerrain, List<MapTerrain>> terrainSubstitution = brickTerrainMapping;
                if (thisLevelInfo.TerrainMapping != null)
                    terrainSubstitution = thisLevelInfo.TerrainMapping;

                Map randomizedMapL1 = MapTerrainRandomizer.RandomizeTerrainInMap(masterMap, terrainSubstitution);
                Game.Dungeon.AddMap(randomizedMapL1);
            }
        }


        private void AddDebugItems(MapInfo mapInfo)
        {
            
        }

        private void SetupMapsInEngine()
        {
            //Comment for faster UI check
            Game.Dungeon.RefreshAllLevelPathingAndFOV();

            foreach (var level in gameLevels)
            {
                Game.Dungeon.Levels[level].LightLevel = 0;
            }
        }

        private void AddDecorationFeatures(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            foreach (var kv in levelInfo)
            {
                var thisLevel = kv.Key;

                var roomsInThisLevel = mapInfo.GetRoomIndicesForLevel(thisLevel);
                roomsInThisLevel = mapInfo.FilterOutCorridors(roomsInThisLevel);

                double chanceToSkip = 0.5;
                double avConcentration = 0.1;
                double stdConcentration = 0.02;

                double featureAv = 10;
                double featureStd = 100;

                if (!featuresByLevel.ContainsKey(thisLevel))
                    continue;

                foreach (var room in roomsInThisLevel)
                {
                    //if (Gaussian.BoxMuller(0, 1) < chanceToSkip)
                    //  continue;

                    //Bias rooms towards one or two types
                    var featuresAndWeights = featuresByLevel[thisLevel].Select(f => new Tuple<int, DecorationFeatureDetails.Decoration>((int)Math.Abs(Gaussian.BoxMuller(featureAv, featureStd)), DecorationFeatureDetails.decorationFeatures[f]));

                    var thisRoom = mapInfo.Room(room);
                    var thisRoomArea = thisRoom.Room.Width * thisRoom.Room.Height;

                    var numberOfFeatures = (int)Math.Abs(Gaussian.BoxMuller(thisRoomArea * avConcentration, thisRoomArea * stdConcentration));

                    AddStandardDecorativeFeaturesToRoomUsingGrid(mapInfo, room, numberOfFeatures, featuresAndWeights);
                }
            }
        }

        private void GenerateQuests(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            var mapHeuristics = new MapHeuristics(mapInfo.Model.GraphNoCycles, mapInfo.StartRoom);
            var roomConnectivityMap = mapHeuristics.GetTerminalBranchConnections();

            if (!quickLevelGen)
            {
                BuildMainQuest(mapInfo, levelInfo, roomConnectivityMap);
            }
            BuildMedicalLevelQuests(mapInfo, levelInfo, roomConnectivityMap);

            if (!quickLevelGen)
            {
                BuildAtriumLevelQuests(mapInfo, levelInfo, roomConnectivityMap);

                BuildRandomElevatorQuests(mapInfo, levelInfo, roomConnectivityMap);

                BuildGoodyQuests(mapInfo, levelInfo, roomConnectivityMap);
            }
        }

        private void BuildRandomElevatorQuests(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            var noLevelsToBlock = 1 + Game.Random.Next(1);

            var candidateLevels = gameLevels.Except(new List<int> { lowerAtriumLevel, medicalLevel }).Where(l => levelInfo[l].ConnectionsToOtherLevels.Count() > 1);
            LogFile.Log.LogEntryDebug("Candidates for elevator quests: " + candidateLevels, LogDebugLevel.Medium);
            var chosenLevels = candidateLevels.RandomElements(noLevelsToBlock);

            foreach (var level in chosenLevels)
            {
                try
                {
                    BlockElevatorPaths(mapInfo, levelInfo, roomConnectivityMap, level, 1, Game.Random.Next(2) > 0);
                }
                catch (Exception ex)
                {
                    LogFile.Log.LogEntryDebug("Random Elevator Exception (level " + level + "): " + ex, LogDebugLevel.High);
                }
            }
        }

        private void BuildAtriumLevelQuests(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            try
            {
                BlockElevatorPaths(mapInfo, levelInfo, roomConnectivityMap, lowerAtriumLevel, 1, false);
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntryDebug("Atrium Elevator Exception: " + ex, LogDebugLevel.High);
            }
        }

        private bool BlockElevatorPaths(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, Dictionary<int, List<Connection>> roomConnectivityMap,
            int levelForBlocks, int maxDoorsToMake, bool clueOnElevatorLevel)
        {
            var connectionsFromThisLevel = levelInfo[levelForBlocks].ConnectionsToOtherLevels;

            var pairs = Utility.GetPermutations<int>(connectionsFromThisLevel.Keys, 2);

            if (pairs.Count() == 0)
            {
                LogFile.Log.LogEntryDebug("Can't find pair of elevators to connection", LogDebugLevel.High);
                return false;
            }

            var pairsLeft = pairs.Select(s => s);

            int doorsMade = 0;
            while (doorsMade < maxDoorsToMake && pairsLeft.Count() > 0)
            {
                var pairToTry = pairsLeft.RandomElement();

                var sourceElevatorConnection = levelInfo[levelForBlocks].ConnectionsToOtherLevels[pairToTry.ElementAt(0)];
                var targetElevatorConnection = levelInfo[levelForBlocks].ConnectionsToOtherLevels[pairToTry.ElementAt(1)];

                var startDoor = sourceElevatorConnection.Source;
                var endDoor = targetElevatorConnection.Source;

                var colorToUse = GetUnusedColor();

                var doorName = colorToUse.Item2 + " key card";
                var doorId = levelNaming[levelForBlocks] + "-" + doorName + Game.Random.Next();
                var doorColor = colorToUse.Item1;

                LogFile.Log.LogEntryDebug("Blocking elevators " + pairToTry.ElementAt(0) + " to " + pairToTry.ElementAt(1) + " with " + doorId, LogDebugLevel.High);

                BlockPathBetweenRoomsWithSimpleDoor(mapInfo, levelInfo, roomConnectivityMap,
                    doorId, doorName, doorColor, 1, startDoor, endDoor,
                    0.5, clueOnElevatorLevel, CluePath.NotOnCriticalPath, true,
                    true, CluePath.OnCriticalPath, true);

                doorsMade++;
                pairsLeft = pairsLeft.Except(Enumerable.Repeat(pairToTry, 1));
            }

            return true;
        }

        private Tuple<System.Drawing.Color, string> GetUnusedColor()
        {
            var unusedColor = availableColors.Except(usedColors);
            var colorToReturn = availableColors.RandomElement();

            if (unusedColor.Count() > 0)
                colorToReturn = unusedColor.RandomElement();

            usedColors.Add(colorToReturn);

            return colorToReturn;
        }

        Dictionary<int, int> goodyRooms;
        Dictionary<int, string> goodyRoomKeyNames;

        private void BuildGoodyQuests(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            //Ensure that we have a goody room on every level that will support it
            var replaceableVaultsForLevels = levelInfo.ToDictionary(kv => kv.Key, kv => kv.Value.ReplaceableVaultConnections.Except(kv.Value.ReplaceableVaultConnectionsUsed));
            goodyRooms = new Dictionary<int,int>();
            goodyRoomKeyNames = new Dictionary<int, string>();

            var manager = mapState.DoorAndClueManager;

            foreach (var kv in replaceableVaultsForLevels)
            {
                if (kv.Value.Count() == 0)
                {
                    LogFile.Log.LogEntryDebug("No vaults left for armory on level " + kv.Key, LogDebugLevel.High);
                    continue;
                }

                var thisLevel = kv.Key;
                var thisConnection = kv.Value.RandomElement();
                var thisRoom = thisConnection.Target;

                LogFile.Log.LogEntryDebug("Placing goody room at: level: " + thisLevel + " room: " + thisRoom, LogDebugLevel.Medium);

                //Place door
                var doorReadableId = levelNaming[thisLevel] + " armory";
                var doorId = doorReadableId;
                
                var unusedColor = GetUnusedColor();
                var clueName = unusedColor.Item2 + " key card";

                PlaceLockedDoorOnMap(mapInfo, doorId, clueName, 1, unusedColor.Item1, thisConnection);

                goodyRooms[thisLevel] = thisRoom;

                //Clue
                var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(doorId);

                //Assume a critical path from the lower level elevator
                var lowerLevelFloor = levelInfo[thisLevel].ConnectionsToOtherLevels.Min(level => level.Key);
                var elevatorFromLowerLevel = levelInfo[thisLevel].ConnectionsToOtherLevels[lowerLevelFloor].Target;
                var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(elevatorFromLowerLevel, thisRoom);

                var filteredRooms = FilterClueRooms(mapInfo, allowedRoomsForClues, criticalPath, true, CluePath.NotOnCriticalPath, true);
                var roomsToPlaceMonsters = new List<int>();

                var roomsForMonsters = GetRandomRoomsForClues(mapInfo, 1, filteredRooms);
                var clues = manager.AddCluesToExistingDoor(doorId, roomsForMonsters);

                
                goodyRoomKeyNames[thisLevel] = clueName;
                var cluesAndColors = clues.Select(c => new Tuple<Clue, System.Drawing.Color, string>(c, unusedColor.Item1, clueName));
                PlaceSimpleClueItems(mapInfo, cluesAndColors, true, false);

                //Vault is used
                levelInfo[thisLevel].ReplaceableVaultConnectionsUsed.Add(thisConnection);
            }
        
        }

        private void AddGoodyQuestLogClues(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            //Ensure that we have a goody room on every level that will support it
            var manager = mapState.DoorAndClueManager;

            foreach (var kv in goodyRooms)
            {
                var thisLevel = kv.Key;
                var thisRoom = kv.Value;
                
                var doorId = levelNaming[thisLevel] + " armory";

                //Clue
                var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(doorId);

                //Assume a critical path from the lower level elevator
                var lowerLevelFloor = levelInfo[thisLevel].ConnectionsToOtherLevels.Min(level => level.Key);
                var elevatorFromLowerLevel = levelInfo[thisLevel].ConnectionsToOtherLevels[lowerLevelFloor].Target;
                var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(elevatorFromLowerLevel, thisRoom);

                //Logs - try placing them on the critical path from the start of the game!

                var criticalPathFromStart = mapInfo.Model.GetPathBetweenVerticesInReducedMap(0, thisRoom);
                var preferredRoomsForLogsNonCritical = FilterClueRooms(mapInfo, allowedRoomsForClues, criticalPath, false, CluePath.OnCriticalPath, true);

                var roomsForLogsNonCritical = GetRandomRoomsForClues(mapInfo, 1, preferredRoomsForLogsNonCritical);

                var logClues = manager.AddCluesToExistingDoor(doorId, roomsForLogsNonCritical);
                var clueName = goodyRoomKeyNames[thisLevel];
                var log1 = new Tuple<LogEntry, Clue>(logGen.GenerateGoodyRoomLogEntry(clueName, thisLevel, itemsInArmory[thisLevel]), logClues[0]);
                PlaceLogClues(mapInfo, new List<Tuple<LogEntry, Clue>> { log1 }, true, true);
            }

        }
        
        private void BuildMedicalLevelQuests(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            //Lock the door to the elevator and require a certain number of monsters to be killed
            var elevatorConnection = levelInfo[medicalLevel].ConnectionsToOtherLevels.First().Value;

            var doorId = "medical-security";
            int objectsToPlace = 15;
            int objectsToDestroy = 10;

            //Place door
            PlaceMovieDoorOnMap(mapInfo, doorId, doorId, objectsToDestroy, System.Drawing.Color.Red, "t_medicalsecurityunlocked", "t_medicalsecuritylocked", elevatorConnection);

            //This will be restricted to the medical level since we cut off the door
            var manager = mapState.DoorAndClueManager;

            var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            allowedRoomsForClues = mapInfo.FilterOutCorridors(allowedRoomsForClues);

            var roomsForMonsters = GetRandomRoomsForClues(mapInfo, objectsToPlace, allowedRoomsForClues);
            var clues = manager.AddCluesToExistingDoor(doorId, roomsForMonsters);

            PlaceCreatureClues<RogueBasin.Creatures.Camera>(mapInfo, clues, true, false);

            //Place log entries explaining the puzzle
            //These will not be turned into in-engine clue items, so they can't be used to open the door
            //They are added though, to ensure that they are readable before the door is opened

            var roomsForLogs = GetRandomRoomsForClues(mapInfo, 2, allowedRoomsForClues);
            var logClues = manager.AddCluesToExistingDoor(doorId, roomsForLogs);

            var log1 = new Tuple<LogEntry, Clue>(logGen.GenerateElevatorLogEntry(medicalLevel, lowerAtriumLevel), logClues[0]);
            var log2 = new Tuple<LogEntry, Clue>(logGen.GenerateArbitaryLogEntry("qe_medicalsecurity"), logClues[1]);
            PlaceLogClues(mapInfo, new List<Tuple<LogEntry, Clue>> { log1, log2 }, true, true);
        }

        private void BlockPathBetweenRoomsWithSimpleDoor(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, Dictionary<int, List<Connection>> roomConnectivityMap,
            string doorId, string doorName, System.Drawing.Color colorToUse, int cluesForDoor, int sourceRoom, int endRoom, 
            double distanceFromSourceRatio, bool enforceClueOnDestLevel, CluePath clueNotOnCriticalPath, bool clueNotInCorridors,
            bool hasLogClue, CluePath logOnCriticalPath, bool logNotInCorridors)
        {
            var manager = mapState.DoorAndClueManager;

            var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(sourceRoom, endRoom);
            var criticalConnectionForDoor = criticalPath.ElementAt((int)Math.Min(criticalPath.Count() * distanceFromSourceRatio, criticalPath.Count() - 1));

            criticalConnectionForDoor = MapAnalysisUtilities.FindFreeConnectionOnPath(manager, criticalPath, criticalConnectionForDoor);

            //Place door

            PlaceLockedDoorOnMap(mapInfo, doorId, doorName, cluesForDoor, colorToUse, criticalConnectionForDoor);

            //Place clues

            var allRoomsForClue = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            var preferredRooms = FilterClueRooms(mapInfo, allRoomsForClue, criticalPath, enforceClueOnDestLevel, clueNotOnCriticalPath, clueNotInCorridors);

            var roomsForClues = GetRandomRoomsForClues(mapInfo, cluesForDoor, preferredRooms);
            var clues = manager.AddCluesToExistingDoor(doorId, roomsForClues);

            var cluesAndColors = clues.Select(c => new Tuple<Clue, System.Drawing.Color, string>(c, colorToUse, doorName));

            var clueLocations = PlaceSimpleClueItems(mapInfo, cluesAndColors, clueNotInCorridors, false);

            //Place log entries explaining the puzzle

            if (hasLogClue)
            {
                //Put major clue on the critical path

                var preferredRoomsForLogs = FilterClueRooms(mapInfo, allRoomsForClue, criticalPath, false, logOnCriticalPath, logNotInCorridors);
                var roomsForLogs = GetRandomRoomsForClues(mapInfo, 1, preferredRoomsForLogs);
                var logClues = manager.AddCluesToExistingDoor(doorId, roomsForLogs);

                //Put minor clue somewhere else
                var preferredRoomsForLogsNonCritical = FilterClueRooms(mapInfo, allRoomsForClue, criticalPath, false, CluePath.Any, logNotInCorridors);

                var roomsForLogsNonCritical = GetRandomRoomsForClues(mapInfo, 1, preferredRoomsForLogsNonCritical);
                var logCluesNonCritical = manager.AddCluesToExistingDoor(doorId, roomsForLogsNonCritical);

                var coupledLogs = logGen.GenerateCoupledDoorLogEntry(doorName, mapInfo.GetLevelForRoomIndex(criticalConnectionForDoor.Source),
                    clueLocations.First().level);
                var log1 = new Tuple<LogEntry, Clue>(coupledLogs[0], logClues[0]);
                var log2 = new Tuple<LogEntry, Clue>(coupledLogs[1], logCluesNonCritical[0]);
                PlaceLogClues(mapInfo, new List<Tuple<LogEntry, Clue>> { log1, log2 }, true, true);
            }
        }

        private Door PlaceLockedDoorOnMap(MapInfo mapInfo, string doorId, string doorName, int numberOfCluesForDoor, System.Drawing.Color colorToUse, Connection criticalConnectionForDoor)
        {
            var door = PlaceLockedDoorInManager(mapInfo, doorId, numberOfCluesForDoor, criticalConnectionForDoor);

            var lockedDoor = new RogueBasin.Locks.SimpleLockedDoor(door, doorName, colorToUse);
            
            PlaceLockedDoorOnMap(mapInfo, lockedDoor, door);

            return door;
        }

        private Door PlaceMovieDoorOnMap(MapInfo mapInfo, string doorId, string doorName, int numberOfCluesForDoor, System.Drawing.Color colorToUse, string openMovie, string cantOpenMovie, Connection criticalConnectionForDoor)
        {
            var door = PlaceLockedDoorInManager(mapInfo, doorId, numberOfCluesForDoor, criticalConnectionForDoor);

            var lockedDoor = new RogueBasin.Locks.SimpleLockedDoorWithMovie(door, openMovie, cantOpenMovie, doorName, colorToUse);

            PlaceLockedDoorOnMap(mapInfo, lockedDoor, door);

            return door;
        }

        private Door PlaceLockedDoorInManager(MapInfo mapInfo, string doorId, int numberOfCluesForDoor, Connection criticalConnectionForDoor)
        {
            var manager = mapState.DoorAndClueManager;

            manager.PlaceDoor(new DoorRequirements(criticalConnectionForDoor, doorId, numberOfCluesForDoor));
            var door = manager.GetDoorById(doorId);
            return door;
        }

        private void PlaceLockedDoorOnMap(MapInfo mapInfo, Lock lockedDoor, Door door)
        {
            var doorInfo = mapInfo.GetDoorForConnection(door.DoorConnectionFullMap);
            lockedDoor.LocationLevel = doorInfo.LevelNo;
            lockedDoor.LocationMap = doorInfo.MapLocation;

            mapInfo.Populator.GetDoorInfo(door.Id).AddLock(lockedDoor);
        }


        private enum CluePath
        {
            OnCriticalPath, NotOnCriticalPath, Any
        }

        private IEnumerable<int> FilterClueRooms(MapInfo mapInfo, IEnumerable<int> allCandidateRooms, IEnumerable<Connection> criticalPath, bool enforceClueOnDestLevel, CluePath clueCriticalPath, bool clueNotInCorridors)
        {
            var candidateRooms = allCandidateRooms.Except(allReplaceableVaults);
            if (enforceClueOnDestLevel)
                candidateRooms = candidateRooms.Intersect(mapInfo.GetRoomIndicesForLevel(mapInfo.GetLevelForRoomIndex(criticalPath.Last().Target)));

            var preferredRooms = candidateRooms;
            if (clueCriticalPath == CluePath.NotOnCriticalPath)
            {
                preferredRooms = candidateRooms.Except(criticalPath.SelectMany(c => new List<int> { c.Source, c.Target }));
                if (preferredRooms.Count() == 0)
                    preferredRooms = candidateRooms;
            }
            else if (clueCriticalPath == CluePath.OnCriticalPath)
            {
                preferredRooms = candidateRooms.Intersect(criticalPath.SelectMany(c => new List<int> { c.Source, c.Target }));
                if (preferredRooms.Count() == 0)
                    preferredRooms = candidateRooms;
            }

            var preferredRoomsIncludingType = preferredRooms;
            if (clueNotInCorridors)
            {
                preferredRoomsIncludingType = mapInfo.FilterOutCorridors(preferredRooms);
                if (preferredRoomsIncludingType.Count() == 0)
                    preferredRoomsIncludingType = preferredRooms;
            }

            return preferredRoomsIncludingType;
        }

        private List<int> GetRandomRoomsForClues(MapInfo info, int cluesToPlace, IEnumerable<int> allowedRoomsForClues)
        {
            if (allowedRoomsForClues.Count() == 0)
                throw new ApplicationException("Not enough rooms to place clues");

            //To get an even distribution we need to take into account how many nodes are in each group node
            var expandedAllowedRoomForClues = info.RepeatRoomNodesByNumberOfRoomsInCollapsedCycles(allowedRoomsForClues.Except(allReplaceableVaults));

            if (expandedAllowedRoomForClues.Count() == 0)
                throw new ApplicationException("No allowed rooms for clues.");

            var roomsToPlaceClues = new List<int>();

            //Can reuse rooms if number of rooms < cluesToPlace
            while (roomsToPlaceClues.Count() < cluesToPlace)
            {
                roomsToPlaceClues.AddRange(expandedAllowedRoomForClues.Shuffle());
            }

            return roomsToPlaceClues.GetRange(0, cluesToPlace);
        }

        int selfDestructRoom;

        private void BuildMainQuest(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            //MAIN QUEST
            var manager = mapState.DoorAndClueManager;

            //Escape pod end game
            EscapePod(mapInfo);
            
            //Self destruct
            //Requires priming the reactor
            SelfDestruct(mapInfo, levelInfo, manager);

            //Computer core to destroy
            ComputerCore(mapInfo, levelInfo, manager);

            //Bridge lock
            //Requires captain's id
            BridgeLock(mapInfo, levelInfo);

            //Computer core lock
            //Requires computer tech's id
            ComputerCoreId(mapInfo, levelInfo);

            //Archology lock
            //Requires bioprotect wetware
            ArcologyLock(mapInfo, levelInfo);

            //Antanae
            //Requires servo motor
            AntennaeQuest(mapInfo, levelInfo, manager);
        }

        private void EscapePod(MapInfo mapInfo)
        {
            var escapePodRoom = escapePodsConnection.Target;
            PlaceFeatureInRoom(mapInfo, new RogueBasin.Features.EscapePod(), new List<int> () { escapePodRoom });

            LogFile.Log.LogEntryDebug("Adding features to escape pod room", LogDebugLevel.Medium);
            var escapePodDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.MedicalAutomat]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Pillar1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Pillar2])
            };
            AddStandardDecorativeFeaturesToRoom(mapInfo, escapePodRoom, 20, escapePodDecorations, false);

            //Escape pod door
            //Requires enabling self-destruct

            var colorForEscapePods = GetUnusedColor();
            var escapedoorName = "escape";
            var escapedoorId = escapedoorName;
            var escapedoorColor = colorForEscapePods.Item1;

            PlaceMovieDoorOnMap(mapInfo, escapedoorId, escapedoorName, 1, escapedoorColor, "escapepodunlocked", "escapepodlocked", escapePodsConnection);
        }

        int computerCoresToDestroy = 15;

        private void SelfDestruct(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, DoorAndClueManager manager)
        {
            int selfDestructLevel = bridgeLevel;
            var replaceableVaultsInBridge = levelInfo[selfDestructLevel].ReplaceableVaultConnections.Except(levelInfo[selfDestructLevel].ReplaceableVaultConnectionsUsed);
            var bridgeRoomsInDistanceOrderFromStart = mapInfo.RoomsInDescendingDistanceFromSource(levelInfo[bridgeLevel].ConnectionsToOtherLevels.First().Value.Target, replaceableVaultsInBridge.Select(c => c.Target));
            selfDestructRoom = bridgeRoomsInDistanceOrderFromStart.ElementAt(0);
            var selfDestructConnection = replaceableVaultsInBridge.Where(c => c.Target == selfDestructRoom).First();

            manager.PlaceObjective(new ObjectiveRequirements(selfDestructRoom, "self-destruct", 1, new List<string> { "escape" }));
            var selfDestructObjective = manager.GetObjectiveById("self-destruct");
            //PlaceObjective(mapInfo, selfDestructObjective, null, true, true);
            var bridgeLocation = PlaceObjective(mapInfo, selfDestructObjective, new RogueBasin.Features.SelfDestructObjective(selfDestructObjective, mapState.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(selfDestructObjective)), true, true);
            
            UseVault(levelInfo, selfDestructConnection);

            var bridgeDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer3]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Screen1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.HighTechBench])
            };
            AddStandardDecorativeFeaturesToRoom(mapInfo, selfDestructRoom, 20, bridgeDecorations, false);

            LogFile.Log.LogEntryDebug("Placing self-destruct on level " + selfDestructLevel + " in room " + selfDestructRoom + " off connection " + selfDestructConnection, LogDebugLevel.Medium);

            //Self destruct objective in reactor
            //Requires destruction of computer core
            var unusedVaultsInReactorLevel = GetAllAvailableVaults(levelInfo).Where(c => mapInfo.GetLevelForRoomIndex(c.Target) == reactorLevel);
            var reactorSelfDestructVaultConnection = unusedVaultsInReactorLevel.First();
            var reactorSelfDestructVault = reactorSelfDestructVaultConnection.Target;
            UseVault(levelInfo, reactorSelfDestructVaultConnection);

            manager.PlaceObjective(new ObjectiveRequirements(reactorSelfDestructVault, "prime-self-destruct", computerCoresToDestroy, new List<string> { "self-destruct" }));
            var selfDestructPrimeObjective = manager.GetObjectiveById("prime-self-destruct");
            //PlaceObjective(mapInfo, selfDestructPrimeObjective, null, true, true);
            var reactorLocation = PlaceObjective(mapInfo, selfDestructPrimeObjective, new RogueBasin.Features.SelfDestructPrimeObjective(selfDestructPrimeObjective, mapState.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(selfDestructPrimeObjective)), true, true);

            var reactorDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument3])
            };
            AddStandardDecorativeFeaturesToRoom(mapInfo, reactorSelfDestructVault, 100, reactorDecorations, false);

        }

        private void ComputerCore(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, DoorAndClueManager manager)
        {

            var primeSelfDestructId = "prime-self-destruct";
            var coresToPlace = 20;

            var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForObjective(primeSelfDestructId);
            allowedRoomsForClues = mapInfo.FilterOutCorridors(allowedRoomsForClues);
            var roomsOnComputerCoreLevel = allowedRoomsForClues.Intersect(mapInfo.GetRoomIndicesForLevel(computerCoreLevel));

            var roomsToPlaceMonsters = new List<int>();

            var roomsForMonsters = GetRandomRoomsForClues(mapInfo, coresToPlace, roomsOnComputerCoreLevel);
            var clues = manager.AddCluesToExistingObjective(primeSelfDestructId, roomsForMonsters);

            PlaceCreatureClues<RogueBasin.Creatures.ComputerNode>(mapInfo, clues, true, false);

            //Place log entries explaining the puzzle
            //These will not be turned into in-engine clue items, so they can't be used to open the door
            //They are added though, to ensure that they are readable before the door is opened

            //CC is a dead end
            var sourceElevatorConnection = levelInfo[computerCoreLevel].ConnectionsToOtherLevels.First();
            var connectingLevel = sourceElevatorConnection.Key;
            var elevatorToCC = levelInfo[connectingLevel].ConnectionsToOtherLevels[computerCoreLevel];

            var allowedRoomsForLogs = manager.GetValidRoomsToPlaceClueForObjective(primeSelfDestructId);

            var preferredLevelsForLogs = new List<int>{ arcologyLevel, commercialLevel };
            var preferredRooms = preferredLevelsForLogs.SelectMany(l => mapInfo.GetRoomIndicesForLevel(l));

            var preferredRoomsForLogs = allowedRoomsForLogs.Intersect(preferredRooms);

            var criticalPathLog = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, elevatorToCC.Source);

            var preferredRoomsForLogsCritical = FilterClueRooms(mapInfo, preferredRoomsForLogs, criticalPathLog, false, CluePath.OnCriticalPath, true);
            var preferredRoomsForLogsNonCritical = FilterClueRooms(mapInfo, preferredRoomsForLogs, criticalPathLog, false, CluePath.Any, true);

            var roomsForLogsCritical = GetRandomRoomsForClues(mapInfo, 2, preferredRoomsForLogsCritical);
            var roomsForLogsNonCritical = GetRandomRoomsForClues(mapInfo, 2, preferredRoomsForLogsNonCritical);

            var logCluesCritical = manager.AddCluesToExistingObjective(primeSelfDestructId, roomsForLogsCritical);
            var logCluesNonCritical = manager.AddCluesToExistingObjective(primeSelfDestructId, roomsForLogsNonCritical);

            var log2 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_computer1", connectingLevel, computerCoreLevel), logCluesCritical[0]);
            var log3 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_computer2", connectingLevel, computerCoreLevel), logCluesCritical[1]);

            var log1 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_computer3", connectingLevel, computerCoreLevel), logCluesNonCritical[0]);
            var log4 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_computer4", connectingLevel, computerCoreLevel), logCluesNonCritical[1]);

            PlaceLogClues(mapInfo, new List<Tuple<LogEntry, Clue>> { log1, log2, log3, log4 }, true, true);

            /*
            var unusedVaultsInComputerLevel = GetAllAvailableVaults(levelInfo).Where(c => mapInfo.GetLevelForRoomIndex(c.Target) == computerCoreLevel);

            var unusedVaultsInComputerLevelOrderFromStart = RoomsInDescendingDistanceFromSource(mapInfo, mapInfo.StartRoom, unusedVaultsInComputerLevel.Select(c => c.Target));
            var computerClueRoom = unusedVaultsInComputerLevelOrderFromStart.ElementAt(0);
            var computerCoreVaultConnection = GetAllVaults(levelInfo).Where(c => c.Target == computerClueRoom).First();

            var computerCoreVault = computerCoreVaultConnection.Target;
            var computerVaultClue = manager.AddCluesToExistingObjective("prime-self-destruct", new List<int> { computerCoreVault });

            PlaceCreatureClues<RogueBasin.Creatures.Camera>(mapInfo, computerVaultClue, true, true);

            UseVault(levelInfo, computerCoreVaultConnection);
             * */
        }

        private void UseVault(Dictionary<int, LevelInfo> levelInfo, Connection vaultConnection)
        {
            var levelForVault = gameLevels.Where(l => levelInfo[l].ReplaceableVaultConnections.Contains(vaultConnection)).First();

            levelInfo[levelForVault].ReplaceableVaultConnectionsUsed.Add(vaultConnection);
        }

        private void BridgeLock(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            var colorForCaptainId = GetUnusedColor();

            //bridge is a dead end
            var sourceElevatorConnection = levelInfo[bridgeLevel].ConnectionsToOtherLevels.First();
            var connectingLevel = sourceElevatorConnection.Key;
            var elevatorToBridge = levelInfo[connectingLevel].ConnectionsToOtherLevels[bridgeLevel];

            var doorName = "captain's id bridge";
            var doorId = doorName;
            var doorColor = colorForCaptainId.Item1;

            PlaceMovieDoorOnMap(mapInfo, doorId, doorName, 1, doorColor, "bridgelocked", "bridgeunlocked", elevatorToBridge);

            //Captain's id
            var captainIdIdealLevel = levelDepths.Where(kv => kv.Value >= 1).Select(kv => kv.Key).Except(new List<int> { lowerAtriumLevel, medicalLevel, storageLevel, scienceLevel });
            var captainsIdRoom = PlaceClueForDoorInVault(mapInfo, levelInfo, doorId, doorColor, doorName, captainIdIdealLevel);

            //Add monsters - nice to put ID on captain but not for now
            var captainsIdLevel = mapInfo.GetLevelForRoomIndex(captainsIdRoom);
            var monstersToPlace = new List<Monster> { new RogueBasin.Creatures.HeavyTurret(), new RogueBasin.Creatures.HeavyTurret(), new RogueBasin.Creatures.AssaultCyborgRanged(), new RogueBasin.Creatures.Captain() };
            Game.Dungeon.MonsterPlacement.AddMonstersToRoom(mapInfo, captainsIdLevel, captainsIdRoom, monstersToPlace);

            var decorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Skeleton]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Plant2]),
                new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Plant3])};
            AddStandardDecorativeFeaturesToRoom(mapInfo, captainsIdRoom, 10, decorations, false);

            //Logs

            var manager = mapState.DoorAndClueManager;

            var allowedRoomsForLogs = manager.GetValidRoomsToPlaceClueForDoor(doorId);

            var criticalPathLog = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, elevatorToBridge.Source);

            var preferredRoomsForLogsCritical = FilterClueRooms(mapInfo, allowedRoomsForLogs, criticalPathLog, false, CluePath.OnCriticalPath, true);
            var preferredRoomsForLogsNonCritical = FilterClueRooms(mapInfo, allowedRoomsForLogs, criticalPathLog, false, CluePath.Any, true);

            var roomsForLogsCritical = GetRandomRoomsForClues(mapInfo, 2, preferredRoomsForLogsCritical);
            var roomsForLogsNonCritical = GetRandomRoomsForClues(mapInfo, 2, preferredRoomsForLogsNonCritical);

            var logCluesCritical = manager.AddCluesToExistingDoor(doorId, roomsForLogsCritical);
            var logCluesNonCritical = manager.AddCluesToExistingDoor(doorId, roomsForLogsNonCritical);

            var log2 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_captain1", connectingLevel, captainsIdLevel), logCluesCritical[0]);
            var log3 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_captain2", connectingLevel, captainsIdLevel), logCluesCritical[1]);

            var log1 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_captain3", connectingLevel, captainsIdLevel), logCluesNonCritical[0]);
            var log4 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_captain4", connectingLevel, captainsIdLevel), logCluesNonCritical[1]);

            PlaceLogClues(mapInfo, new List<Tuple<LogEntry, Clue>> { log1, log2, log3, log4 }, true, true);
        }

        private void ComputerCoreId(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            var colorForComputerTechsId = GetUnusedColor();

            //computer core is a dead end
            var computerCoreSourceElevatorConnection = levelInfo[computerCoreLevel].ConnectionsToOtherLevels.First();
            var levelToComputerCore = computerCoreSourceElevatorConnection.Key;
            var elevatorToComputerCore = levelInfo[levelToComputerCore].ConnectionsToOtherLevels[computerCoreLevel];

            var computerDoorName = "tech's id computer core";
            var computerDoorId = computerDoorName;
            var computerDoorColor = colorForComputerTechsId.Item1;

            PlaceMovieDoorOnMap(mapInfo, computerDoorId, computerDoorName, 1, computerDoorColor, "computercoreunlocked", "computercorelocked", elevatorToComputerCore);

            //Tech's id (this should always work)
            var techIdIdealLevel = new List<int> { arcologyLevel };
            var techIdRoom = PlaceClueForDoorInVault(mapInfo, levelInfo, computerDoorId, computerDoorColor, computerDoorName, techIdIdealLevel);
            var techIdLevel = mapInfo.GetLevelForRoomIndex(techIdRoom);

            //A slaughter
            var bioDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Egg1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.HumanCorpse]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.CorpseinGoo]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.HumanCorpse2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Skeleton])
            };
            AddStandardDecorativeFeaturesToRoom(mapInfo, techIdRoom, 20, bioDecorations, false);
        }

        private void ArcologyLock(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            var colorForArcologyLock = GetUnusedColor();

            // arcology is not a dead end, but only the cc and bridge can follow it
            var arcologyLockSourceElevatorConnections = levelInfo[arcologyLevel].ConnectionsToOtherLevels.Where(c => c.Key != computerCoreLevel && c.Key != bridgeLevel);
            if (arcologyLockSourceElevatorConnections.Count() != 1)
                throw new ApplicationException("arcology connectivity is wrong");

            var arcologyLockSourceElevatorConnection = arcologyLockSourceElevatorConnections.First();
            var levelToArcology = arcologyLockSourceElevatorConnection.Key;
            var elevatorToArcology = levelInfo[levelToArcology].ConnectionsToOtherLevels[arcologyLevel];

            var arcologyDoorName = "bioware - arcology door lock";
            var arcologyDoorId = arcologyDoorName;
            var arcologyDoorColor = colorForArcologyLock.Item1;

            //Place the arcology door
            var manager = mapState.DoorAndClueManager;

            manager.PlaceDoor(new DoorRequirements(elevatorToArcology, arcologyDoorId, 1));
            var door = manager.GetDoorById(arcologyDoorId);
            
            var arcologyDoor = new RogueBasin.Locks.SimpleOptionalLockedDoorWithMovie(door, "arcologyunlocked", "arcologylocked", "Override the security and go in anyway?", arcologyDoorName, arcologyDoorColor);

            PlaceLockedDoorOnMap(mapInfo, arcologyDoor, door);

            //Bioware
            var biowareIdIdealLevel = new List<int> { storageLevel, scienceLevel, flightDeck };
            //PlaceClueForDoorInVault(mapInfo, levelInfo, arcologyDoorId, arcologyDoorColor, arcologyDoorName, biowareIdIdealLevel);
            var biowareRoom = PlaceClueItemForDoorInVault(mapInfo, levelInfo, arcologyDoorId, new RogueBasin.Items.BioWare(), arcologyDoorName, biowareIdIdealLevel);
            var biowareLevel = mapInfo.GetLevelForRoomIndex(biowareRoom);
            var bioDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Egg1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Egg2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.CorpseinGoo]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.EggChair])
            };
            AddStandardDecorativeFeaturesToRoom(mapInfo, biowareRoom, 10, bioDecorations, false);

            //Logs

            var allowedRoomsForLogs = manager.GetValidRoomsToPlaceClueForDoor(arcologyDoorName);

            var criticalPathLog = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, elevatorToArcology.Source);

            var preferredRoomsForLogsCritical = FilterClueRooms(mapInfo, allowedRoomsForLogs, criticalPathLog, false, CluePath.OnCriticalPath, true);
            var preferredRoomsForLogsNonCritical = FilterClueRooms(mapInfo, allowedRoomsForLogs, criticalPathLog, false, CluePath.Any, true);

            var roomsForLogsCritical = GetRandomRoomsForClues(mapInfo, 2, preferredRoomsForLogsCritical);
            var roomsForLogsNonCritical = GetRandomRoomsForClues(mapInfo, 2, preferredRoomsForLogsNonCritical);

            var logCluesCritical = manager.AddCluesToExistingDoor(arcologyDoorId, roomsForLogsCritical);
            var logCluesNonCritical = manager.AddCluesToExistingDoor(arcologyDoorId, roomsForLogsNonCritical);

            var log2 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_arcology3", levelToArcology, biowareLevel), logCluesCritical[0]);
            var log3 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_arcology4", levelToArcology, biowareLevel), logCluesCritical[1]);

            var log1 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_arcology1", levelToArcology, biowareLevel), logCluesNonCritical[0]);
            var log4 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_arcology2", levelToArcology, biowareLevel), logCluesNonCritical[1]);

            PlaceLogClues(mapInfo, new List<Tuple<LogEntry, Clue>> { log1, log2, log3, log4 }, true, true);

            //Wrap the arcology door in another door that depends on the antennae
            //Get critical path to archology door

            var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, arcologyLockSourceElevatorConnection.Value.Source);

            //Don't use 2 sincee that's between levels
            var lastCorridorToArcology = criticalPath.ElementAt(criticalPath.Count() - 4);

            var colorForArcologyAntLock = GetUnusedColor();
            
            arcologyAntDoorId = "antennae - arcology door lock";
            var arcologyAntDoorColor = colorForArcologyAntLock.Item1;

            manager.PlaceDoor(new DoorRequirements(lastCorridorToArcology, arcologyAntDoorId, 1));
            var door2 = manager.GetDoorById(arcologyAntDoorId);

            var arcologyAntDoor = new RogueBasin.Locks.SimpleLockedDoorWithMovie(door2, "arcologyantunlocked", "arcologyantlocked", arcologyAntDoorId, arcologyAntDoorColor);

            PlaceLockedDoorOnMap(mapInfo, arcologyAntDoor, door2);
        }

        string arcologyAntDoorId;

        private void AntennaeQuest(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, DoorAndClueManager manager)
        {
            var levelsForAntennae = new List<int> { scienceLevel, storageLevel };
            var unusedVaultsInAntennaeLevel = GetAllAvailableVaults(levelInfo).Where(c => levelsForAntennae.Contains(mapInfo.GetLevelForRoomIndex(c.Target)));

            var unusedVaultsInnAntennaeLevelOrderFromStart = mapInfo.RoomsInDescendingDistanceFromSource(mapInfo.StartRoom, unusedVaultsInAntennaeLevel.Select(c => c.Target));
            var antennaeRoom = unusedVaultsInnAntennaeLevelOrderFromStart.ElementAt(0);
            var antennaeVaultConnection = GetAllVaults(levelInfo).Where(c => c.Target == antennaeRoom).First();

            var antennaeVault = antennaeVaultConnection.Target;
            var antennaeObjName = "antennae";
            manager.PlaceObjective(new ObjectiveRequirements(antennaeVault, antennaeObjName, 1, new List<string> { arcologyAntDoorId }));
            var antennaeObj = manager.GetObjectiveById(antennaeObjName);
            PlaceObjective(mapInfo, antennaeObj, new RogueBasin.Features.AntennaeObjective(antennaeObj, mapState.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(antennaeObj)), true, true);

            UseVault(levelInfo, antennaeVaultConnection);

            //Extra stuff for antenna room

            var antennaeLevel = mapInfo.GetLevelForRoomIndex(antennaeVault);

            var monstersToPlace = new List<Monster> { new RogueBasin.Creatures.RotatingTurret(), new RogueBasin.Creatures.HeavyTurret(), new RogueBasin.Creatures.PatrolBotRanged(), new RogueBasin.Creatures.PatrolBotRanged()};
            Game.Dungeon.MonsterPlacement.AddMonstersToRoom(mapInfo, antennaeLevel, antennaeVault, monstersToPlace);

            var decorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Antennae]) };
            AddStandardDecorativeFeaturesToRoom(mapInfo, antennaeVault, 10, decorations, false);

            //Servo motor

            var servoRoom = PlaceMovieClueForObjectiveInVault(mapInfo, levelInfo, antennaeObjName, (char)312, "interface_demod", "Interface Demodulator", new List<int> { scienceLevel, storageLevel });
            var servoLevel = mapInfo.GetLevelForRoomIndex(servoRoom);

            var servoDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Machine]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Machine2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.MachinePart2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.MachinePart3])
            };
            AddStandardDecorativeFeaturesToRoom(mapInfo, servoRoom, 10, servoDecorations, false);

            //Logs

            var allowedRoomsForLogs = manager.GetValidRoomsToPlaceClueForObjective(antennaeObjName);

            var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, antennaeVault);

            var preferredRoomsForLogsCritical = FilterClueRooms(mapInfo, allowedRoomsForLogs, criticalPath, false, CluePath.OnCriticalPath, true);
            var preferredRoomsForLogsNonCritical = FilterClueRooms(mapInfo, allowedRoomsForLogs, criticalPath, false, CluePath.Any, true);

            var roomsForLogsCritical = GetRandomRoomsForClues(mapInfo, 2, preferredRoomsForLogsCritical);
            var roomsForLogsNonCritical = GetRandomRoomsForClues(mapInfo, 2, preferredRoomsForLogsNonCritical);

            var logCluesCritical = manager.AddCluesToExistingObjective(antennaeObjName, roomsForLogsCritical);
            var logCluesNonCritical = manager.AddCluesToExistingObjective(antennaeObjName, roomsForLogsNonCritical);

            var log2 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_antennae2", antennaeLevel, servoLevel), logCluesCritical[0]);
            var log3 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_antennae3", antennaeLevel, servoLevel), logCluesCritical[1]);

            var log1 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_antennae1", antennaeLevel, servoLevel), logCluesNonCritical[0]);
            var log4 = new Tuple<LogEntry, Clue>(logGen.GenerateGeneralQuestLogEntry("qe_antennae4", antennaeLevel, servoLevel), logCluesNonCritical[1]);

            PlaceLogClues(mapInfo, new List<Tuple<LogEntry, Clue>> { log1, log2, log3, log4 }, true, true);
            
        }

        private int PlaceClueForDoorInVault(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, string doorId, System.Drawing.Color clueColour, string clueName, IEnumerable<int> idealLevelsForClue)
        {
            var manager = mapState.DoorAndClueManager;

            var possibleRoomsForCaptainsId = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            var possibleVaultsForCaptainsId = possibleRoomsForCaptainsId.Intersect(GetAllAvailableVaults(levelInfo).Select(c => c.Target));

            var roomsOnRequestedLevels = mapInfo.FilterRoomsByLevel(possibleVaultsForCaptainsId, idealLevelsForClue);

            if (!roomsOnRequestedLevels.Any())
                roomsOnRequestedLevels = possibleVaultsForCaptainsId;

           // var captainIdRoomsInDistanceOrderFromStart = RoomsInDescendingDistanceFromSource(mapInfo, mapInfo.StartRoom, roomsOnRequestedLevels);
           // var captainIdRoom = captainIdRoomsInDistanceOrderFromStart.ElementAt(0);
            //above is not performing, since it always sticks everything in level 8 as far away from everything as it can
            var captainIdRoom = roomsOnRequestedLevels.RandomElement();

            var captainsIdConnection = GetAllVaults(levelInfo).Where(c => c.Target == captainIdRoom).First();
            var captainsIdLevel = mapInfo.GetLevelForRoomIndex(captainIdRoom);

            UseVault(levelInfo, captainsIdConnection);

            var captainIdClue = mapState.DoorAndClueManager.AddCluesToExistingDoor(doorId, new List<int> { captainIdRoom }).First();
            PlaceSimpleClueItem(mapInfo, new Tuple<Clue, System.Drawing.Color, string>(captainIdClue, clueColour, clueName), true, true);

            LogFile.Log.LogEntryDebug("Placing " + clueName +" on level " + captainsIdLevel + " in vault " + captainIdRoom, LogDebugLevel.Medium);

            return captainIdRoom;
        }

        private int PlaceClueItemForDoorInVault(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, string doorId, Item itemToPlace, string clueName, IEnumerable<int> idealLevelsForClue)
        {
            var manager = mapState.DoorAndClueManager;

            var possibleRoomsForCaptainsId = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            var possibleVaultsForCaptainsId = possibleRoomsForCaptainsId.Intersect(GetAllAvailableVaults(levelInfo).Select(c => c.Target));

            var roomsOnRequestedLevels = mapInfo.FilterRoomsByLevel(possibleVaultsForCaptainsId, idealLevelsForClue);

            if (!roomsOnRequestedLevels.Any())
                roomsOnRequestedLevels = possibleVaultsForCaptainsId;

            // var captainIdRoomsInDistanceOrderFromStart = RoomsInDescendingDistanceFromSource(mapInfo, mapInfo.StartRoom, roomsOnRequestedLevels);
            // var captainIdRoom = captainIdRoomsInDistanceOrderFromStart.ElementAt(0);
            //above is not performing, since it always sticks everything in level 8 as far away from everything as it can
            var captainIdRoom = roomsOnRequestedLevels.RandomElement();

            var captainsIdConnection = GetAllVaults(levelInfo).Where(c => c.Target == captainIdRoom).First();
            var captainsIdLevel = mapInfo.GetLevelForRoomIndex(captainIdRoom);

            UseVault(levelInfo, captainsIdConnection);

            var captainIdClue = mapState.DoorAndClueManager.AddCluesToExistingDoor(doorId, new List<int> { captainIdRoom }).First();

            PlaceItems(mapInfo, new List<Item> { itemToPlace }, new List<int> {captainIdRoom}, true);

            LogFile.Log.LogEntryDebug("Placing " + clueName + " on level " + captainsIdLevel + " in vault " + captainIdRoom, LogDebugLevel.Medium);

            return captainIdRoom;
        }

        private int PlaceClueForObjectiveInVault(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, string doorId, System.Drawing.Color clueColour, string clueName, IEnumerable<int> idealLevelsForClue)
        {
            var manager = mapState.DoorAndClueManager;

            var possibleRoomsForCaptainsId = manager.GetValidRoomsToPlaceClueForObjective(doorId);
            var possibleVaultsForCaptainsId = possibleRoomsForCaptainsId.Intersect(GetAllAvailableVaults(levelInfo).Select(c => c.Target));

            var roomsOnRequestedLevels = mapInfo.FilterRoomsByLevel(possibleVaultsForCaptainsId, idealLevelsForClue);

            if (!roomsOnRequestedLevels.Any())
                roomsOnRequestedLevels = possibleVaultsForCaptainsId;

            // var captainIdRoomsInDistanceOrderFromStart = RoomsInDescendingDistanceFromSource(mapInfo, mapInfo.StartRoom, roomsOnRequestedLevels);
            // var captainIdRoom = captainIdRoomsInDistanceOrderFromStart.ElementAt(0);
            //above is not performing, since it always sticks everything in level 8 as far away from everything as it can
            var captainIdRoom = roomsOnRequestedLevels.RandomElement();

            var captainsIdConnection = GetAllVaults(levelInfo).Where(c => c.Target == captainIdRoom).First();
            var captainsIdLevel = mapInfo.GetLevelForRoomIndex(captainIdRoom);

            UseVault(levelInfo, captainsIdConnection);

            var captainIdClue = mapState.DoorAndClueManager.AddCluesToExistingObjective(doorId, new List<int> { captainIdRoom }).First();
            PlaceSimpleClueItem(mapInfo, new Tuple<Clue, System.Drawing.Color, string>(captainIdClue, clueColour, clueName), true, true);

            LogFile.Log.LogEntryDebug("Placing " + clueName + " on level " + captainsIdLevel + " in vault " + captainIdRoom, LogDebugLevel.Medium);

            return captainIdRoom;
        }

        private int PlaceMovieClueForObjectiveInVault(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, string objectiveId, char representation, string pickupMovie, string description, IEnumerable<int> idealLevelsForClue)
        {
            var manager = mapState.DoorAndClueManager;

            var possibleRoomsForCaptainsId = manager.GetValidRoomsToPlaceClueForObjective(objectiveId);
            var possibleVaultsForCaptainsId = possibleRoomsForCaptainsId.Intersect(GetAllAvailableVaults(levelInfo).Select(c => c.Target));

            var roomsOnRequestedLevels = mapInfo.FilterRoomsByLevel(possibleVaultsForCaptainsId, idealLevelsForClue);

            if (!roomsOnRequestedLevels.Any())
                roomsOnRequestedLevels = possibleVaultsForCaptainsId;

            // var captainIdRoomsInDistanceOrderFromStart = RoomsInDescendingDistanceFromSource(mapInfo, mapInfo.StartRoom, roomsOnRequestedLevels);
            // var captainIdRoom = captainIdRoomsInDistanceOrderFromStart.ElementAt(0);            //above is not performing, since it always sticks everything in level 8 as far away from everything as it can
            var captainIdRoom = roomsOnRequestedLevels.RandomElement();

            var captainsIdConnection = GetAllVaults(levelInfo).Where(c => c.Target == captainIdRoom).First();
            var captainsIdLevel = mapInfo.GetLevelForRoomIndex(captainIdRoom);

            UseVault(levelInfo, captainsIdConnection);

            var captainIdClue = mapState.DoorAndClueManager.AddCluesToExistingObjective(objectiveId, new List<int> { captainIdRoom }).First();
            Item clueItemToPlace = new RogueBasin.Items.MovieClue(captainIdClue, representation, pickupMovie, description);
            PlaceClueItems(mapInfo, new List<Tuple<Clue,Item>>{new Tuple<Clue, Item>(captainIdClue, clueItemToPlace)}, false, true, true);

            LogFile.Log.LogEntryDebug("Placing " + clueItemToPlace.SingleItemDescription + " on level " + captainsIdLevel + " in vault " + captainIdRoom, LogDebugLevel.Medium);

            return captainIdRoom;
        }


        private void AddElevatorFeatures(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            var elevatorLocations = new Dictionary<Tuple<int, int>, Tuple<int, RogueBasin.Point>>();

            foreach (var kv in levelInfo)
            {
                var thisLevelNo = kv.Key;
                var thisLevelInfo = kv.Value;

                foreach (var connectionToOtherLevel in thisLevelInfo.ConnectionsToOtherLevels)
                {
                    var elevatorLoc = mapInfo.GetUnoccupiedPointsInRoom(connectionToOtherLevel.Value.Target).Shuffle().First();
                    elevatorLocations[new Tuple<int, int>(thisLevelNo, connectionToOtherLevel.Key)] =
                        new Tuple<int, RogueBasin.Point>(connectionToOtherLevel.Value.Target, elevatorLoc);
                }
            }

            foreach (var kv in elevatorLocations)
            {
                var sourceLevel = kv.Key.Item1;
                var targetLevel = kv.Key.Item2;

                var sourceToTargetElevator = kv.Value;
                var targetToSourceElevator = elevatorLocations[new Tuple<int, int>(targetLevel, sourceLevel)];

                var sourceToTargetElevatorRoomId = kv.Value.Item1;
                var sourceToTargetElevatorPoint = kv.Value.Item2;

                var elevatorFeature = new RogueBasin.Features.Elevator(targetLevel, targetToSourceElevator.Item2);

                mapInfo.Populator.AddFeatureToRoom(mapInfo, sourceToTargetElevatorRoomId, sourceToTargetElevatorPoint, elevatorFeature);
                
                LogFile.Log.LogEntryDebug("Adding elevator connection " + sourceLevel + ":" + targetLevel + " via points" +
                    sourceToTargetElevator + "->" + targetToSourceElevator, LogDebugLevel.Medium);
            }
        }

        private void PlaceCreatureClues<T>(MapInfo mapInfo, List<Clue> monsterCluesToPlace, bool autoPickup, bool includeVaults) where T : Monster, new()
        {
            foreach (var clue in monsterCluesToPlace)
            {

                var pointsForClues = GetAllWalkablePointsInRoomsToPlaceClueBoundariesOnly(mapInfo, clue, true, includeVaults);

                if (!pointsForClues.Any())
                    pointsForClues = GetAllWalkableRoomPointsToPlaceClue(mapInfo, clue, true, includeVaults);

                var newMonster = new T();
                Item clueItem;
                if (autoPickup)
                    clueItem = new RogueBasin.Items.ClueAutoPickup(clue);
                else
                    clueItem = new RogueBasin.Items.Clue(clue);

                newMonster.PickUpItem(clueItem);

                if (!pointsForClues.Any())
                {
                    throw new ApplicationException("Nowhere to place clue monster " + newMonster.SingleDescription);
                }

                var pointToPlaceClue = pointsForClues.Shuffle().First();

                mapInfo.Populator.AddMonsterToRoom(newMonster, pointToPlaceClue.roomId, pointToPlaceClue.ToLocation());
            }
        }

        private void PlaceLogClues(MapInfo mapInfo, List<Tuple<LogEntry, Clue>> logCluesToPlace, bool boundariesPreferred, bool cluesNotInCorridors)
        {
            var clueItems = logCluesToPlace.Select(t => new Tuple<Clue, Item>(t.Item2, new RogueBasin.Items.Log(t.Item1)));
            PlaceClueItems(mapInfo, clueItems, boundariesPreferred, cluesNotInCorridors, false);
        }
        
        private IEnumerable<RoomPoint> PlaceClueItems(MapInfo mapInfo, IEnumerable<Tuple<Clue, Item>> clueItems, bool boundariesPreferred, bool avoidCorridors, bool includeVaults)
        {
            List<RoomPoint> pointsPlaced = new List<RoomPoint>();

            foreach (var clueItem in clueItems)
            {
                var clue = clueItem.Item1;
                var itemToPlace = clueItem.Item2;

                IEnumerable<RoomPoint> pointsForClue;
                if (boundariesPreferred)
                {
                    pointsForClue = GetAllWalkablePointsInRoomsToPlaceClueBoundariesOnly(mapInfo, clue, avoidCorridors, includeVaults);

                    if (!pointsForClue.Any())
                        pointsForClue = GetAllWalkableRoomPointsToPlaceClue(mapInfo, clue, avoidCorridors, includeVaults);
                }
                else
                    pointsForClue = GetAllWalkableRoomPointsToPlaceClue(mapInfo, clue, avoidCorridors, includeVaults);

                if (!pointsForClue.Any())
                {
                    throw new ApplicationException("Nowhere to place clue item: " + itemToPlace.SingleItemDescription);
                }

                var pointToPlaceClue = pointsForClue.Shuffle().First();
                mapInfo.Populator.AddItemToRoom(itemToPlace, pointToPlaceClue.roomId, pointToPlaceClue.ToLocation());
                pointsPlaced.Add(pointToPlaceClue);
            }

            return pointsPlaced;
        }

        private void PlaceItems(MapInfo mapInfo, IEnumerable<Item> items, IEnumerable<int> rooms, bool boundariesPreferred)
        {
            IEnumerable<RoomPoint> pointsToPlace;
            if (boundariesPreferred)
            {
                pointsToPlace = GetAllWalkablePointsInRoomsBoundariesOnly(mapInfo, rooms);

                if (!pointsToPlace.Any())
                    pointsToPlace = GetAllWalkableRoomPoints(mapInfo, rooms);
            }
            else
                pointsToPlace = GetAllWalkableRoomPoints(mapInfo, rooms);

            if (!pointsToPlace.Any())
            {
                throw new ApplicationException("Nowhere to place item");
            }

            var pointsForItems = pointsToPlace.RepeatToLength(items.Count());
            var pointsAndItems = pointsForItems.Zip(items, (p, i) => new Tuple<Item, RoomPoint>(i, p));

            foreach (var pi in pointsAndItems)
            {
                mapInfo.Populator.AddItemToRoom(pi.Item1, pi.Item2.roomId, pi.Item2.ToLocation());
            }
        }

        private class RoomPoint
        {
            public readonly int level;
            public readonly RogueBasin.Point mapLocation;
            public readonly int roomId;

            public RoomPoint(int level, int roomId, RogueBasin.Point mapLocation)
            {
                this.level = level;
                this.roomId = roomId;
                this.mapLocation = mapLocation;
            }

            public Location ToLocation() {
                return new Location(level, mapLocation);
            }
        }

        private IEnumerable<RoomPoint> GetAllWalkableRoomPointsToPlaceClue(MapInfo mapInfo, Clue clue, bool filterCorridors, bool includeVaults)
        {
            var candidateRooms = GetPossibleRoomsForClues(mapInfo, clue, filterCorridors, includeVaults);

            return GetAllWalkableRoomPoints(mapInfo, candidateRooms);
        }
        
        private IEnumerable<RoomPoint> GetAllWalkableRoomPoints(MapInfo mapInfo, IEnumerable<int> rooms)
        {
            var allWalkablePoints = new List<RoomPoint>();

            //Hmm, could be quite expensive
            foreach (var room in rooms)
            {
                var level = mapInfo.GetLevelForRoomIndex(room);
                var allPossiblePoints = mapInfo.GetAllPointsInRoomOfTerrain(room, RoomTemplateTerrain.Floor);
                var allUnoccupiedPoints = allPossiblePoints.Except(mapInfo.GetOccupiedPointsInRoom(room));
                var allUnoccupiedRoomPoints = allUnoccupiedPoints.Select(p => new RoomPoint(level, room, p));
                
                allWalkablePoints.AddRange(allUnoccupiedRoomPoints);
            }

            return allWalkablePoints.Shuffle();
        }

        private IEnumerable<RoomPoint> GetAllWalkablePointsInRoomsToPlaceClueBoundariesOnly(MapInfo mapInfo, Clue clue, bool filterCorridors, bool includeVaults)
        {
            var candidateRooms = GetPossibleRoomsForClues(mapInfo, clue, filterCorridors, includeVaults);

            return GetAllWalkablePointsInRoomsBoundariesOnly(mapInfo, candidateRooms);
        }

        private IEnumerable<RoomPoint> GetAllWalkablePointsInRoomsBoundariesOnly(MapInfo mapInfo, IEnumerable<int> rooms) {

            var allWalkablePoints = new List<RoomPoint>();

            //Hmm, could be quite expensive
            foreach (var room in rooms)
            {
                var level = mapInfo.GetLevelForRoomIndex(room);
                var allPossiblePoints = mapInfo.GetBoundaryFloorPointsInRoom(room);
                var allUnoccupiedPoints = allPossiblePoints.Except(mapInfo.GetOccupiedPointsInRoom(room));
                var allUnoccupiedRoomPoints = allUnoccupiedPoints.Select(p => new RoomPoint(level, room, p));
                
                allWalkablePoints.AddRange(allUnoccupiedRoomPoints);
            }

            return allWalkablePoints.Shuffle();
        }

        private IEnumerable<int> GetPossibleRoomsForClues(MapInfo mapInfo, Clue clue, bool filterCorridors, bool includeVaults)
        {
            var possibleRooms = clue.PossibleClueRoomsInFullMap;
            IEnumerable<int> initialRooms = possibleRooms;
            if (!includeVaults)
                initialRooms = possibleRooms.Except(allReplaceableVaults);
            var candidateRooms = initialRooms;
            if (filterCorridors)
                candidateRooms = mapInfo.FilterOutCorridors(initialRooms);
            if (candidateRooms.Count() == 0)
                candidateRooms = initialRooms;
            return candidateRooms;
        }

        private IEnumerable<int> GetPossibleRoomsForObjective(MapInfo mapInfo, Objective clue, bool filterCorridors, bool includeVaults)
        {
            var possibleRooms = clue.PossibleClueRoomsInFullMap;
            IEnumerable<int> initialRooms = possibleRooms;
            if (!includeVaults)
                initialRooms = possibleRooms.Except(allReplaceableVaults);
            var candidateRooms = initialRooms;
            if (filterCorridors)
                candidateRooms = mapInfo.FilterOutCorridors(initialRooms);
            if (candidateRooms.Count() == 0)
                candidateRooms = initialRooms;
            return candidateRooms;
        }

        private Tuple<int, IEnumerable<RogueBasin.Point>> GetAllWalkablePointsToPlaceClueBoundariesOnly(MapInfo mapInfo, Clue clue, bool filterCorridors, bool includeVaults)
        {
            var candidateRooms = GetPossibleRoomsForClues(mapInfo, clue, filterCorridors, includeVaults);

            //Must be on the same level
            var levelForRandomRoom = mapInfo.GetLevelForRoomIndex(candidateRooms.First());

            var allWalkablePoints = new List<RogueBasin.Point>();

            //Hmm, could be quite expensive
            foreach (var room in candidateRooms)
            {
                var allPossiblePoints = mapInfo.GetBoundaryFloorPointsInRoom(room);
                var allUnoccupiedPoints = allPossiblePoints.Except(mapInfo.GetOccupiedPointsInRoom(room));
                allWalkablePoints.AddRange(Game.Dungeon.GetWalkablePointsFromSet(levelForRandomRoom, allPossiblePoints));
            }

            return new Tuple<int, IEnumerable<RogueBasin.Point>>(levelForRandomRoom, allWalkablePoints.Shuffle());
        }

        private Tuple<int, IEnumerable<RogueBasin.Point>> GetAllWalkablePointsToPlaceObjective(MapInfo mapInfo, Objective clue, bool filterCorridors, bool includeVaults)
        {
            IEnumerable<int> possibleRooms = clue.PossibleClueRoomsInFullMap;

            if(!includeVaults)
                possibleRooms = possibleRooms.Except(allReplaceableVaults);
            var candidateRooms = possibleRooms;

            if (filterCorridors)
                candidateRooms = mapInfo.FilterOutCorridors(possibleRooms);
            if (candidateRooms.Count() == 0)
                candidateRooms = possibleRooms;

             var levelForRandomRoom = mapInfo.GetLevelForRoomIndex(candidateRooms.First());

             var allWalkablePoints = new List<RogueBasin.Point>();

            //Hmm, could be quite expensive
            foreach (var room in candidateRooms)
            {
                var allPossiblePoints = mapInfo.GetAllPointsInRoomOfTerrain(room, RoomTemplateTerrain.Floor);
                allWalkablePoints.AddRange(Game.Dungeon.GetWalkablePointsFromSet(levelForRandomRoom, allPossiblePoints));
            }

            return new Tuple<int, IEnumerable<RogueBasin.Point>>(levelForRandomRoom, allWalkablePoints);
        }

        private IEnumerable<RoomPoint> PlaceSimpleClueItem(MapInfo mapInfo, Tuple<Clue, System.Drawing.Color, string> clues, bool avoidCorridors, bool includeVaults)
        {
            return PlaceSimpleClueItems(mapInfo, new List<Tuple<Clue, System.Drawing.Color, string>> { clues }, avoidCorridors, includeVaults);
        }

        private IEnumerable<RoomPoint> PlaceSimpleClueItems(MapInfo mapInfo, IEnumerable<Tuple<Clue, System.Drawing.Color, string>> clues, bool avoidCorridors, bool includeVaults)
        {
            var simpleClueItems = clues.Select(c => new Tuple<Clue, Item>(c.Item1, new RogueBasin.Items.Clue(c.Item1, c.Item2, c.Item3)));
            return PlaceClueItems(mapInfo, simpleClueItems, false, avoidCorridors, includeVaults);
        }

        private RoomPoint PlaceObjective(MapInfo mapInfo, Objective obj, Feature objectiveFeature, bool avoidCorridors, bool includeVaults)
        {
            var candidateRooms = GetPossibleRoomsForObjective(mapInfo, obj, avoidCorridors, includeVaults);
            return PlaceFeatureInRoom(mapInfo, objectiveFeature, candidateRooms);
        }

        /// <summary>
        /// This is a bit fragile for blocking features, since it only tries one square
        /// </summary>
        private RoomPoint PlaceFeatureInRoom(MapInfo mapInfo, Feature objectiveFeature, IEnumerable<int> candidateRooms)
        {
            var roomPoints = GetAllWalkableRoomPoints(mapInfo, candidateRooms);

            if (!roomPoints.Any())
            {
                throw new ApplicationException("Unable to place feature " + objectiveFeature.Description);
            }

            var roomPoint = roomPoints.First();

            bool success = mapInfo.Populator.AddFeatureToRoom(mapInfo, roomPoint.roomId, roomPoint.mapLocation, objectiveFeature);

            if (!success)
            {
                throw new ApplicationException("Unable to place feature " + objectiveFeature.Description + " in room " + roomPoint.roomId);
            }

            return roomPoint;
        }

        

        private IEnumerable<Connection> GetAllUsedVaults(Dictionary<int, LevelInfo> levelInfo)
        {
            return gameLevels.SelectMany(l => levelInfo[l].ReplaceableVaultConnectionsUsed);
        }

        private IEnumerable<Connection> GetAllAvailableVaults(Dictionary<int, LevelInfo> levelInfo)
        {
            return GetAllVaults(levelInfo).Except(GetAllUsedVaults(levelInfo));
        }

        private IEnumerable<Connection> GetAllVaults(Dictionary<int, LevelInfo> levelInfo)
        {
            return gameLevels.SelectMany(l => levelInfo[l].ReplaceableVaultConnections);
        }



        private void AddStandardDecorativeFeaturesToRoom(MapInfo mapInfo, int roomId, int featuresToPlace, IEnumerable<Tuple<int, DecorationFeatureDetails.Decoration>> decorationDetails, bool useBoundary)
        {
            var floorPoints = new List<RogueBasin.Point>();
            var thisRoom = mapInfo.Room(roomId);

            if (!useBoundary)
                floorPoints = RoomTemplateUtilities.GetPointsInRoomWithTerrain(thisRoom.Room, RoomTemplateTerrain.Floor);
            else
                floorPoints = RoomTemplateUtilities.GetBoundaryFloorPointsInRoom(thisRoom.Room);

            var floorPointsToUse = floorPoints.Shuffle().Take(featuresToPlace);

            AddStandardDecorativeFeaturesToRoom(mapInfo, roomId, floorPointsToUse, decorationDetails);
        }

        private void AddStandardDecorativeFeaturesToRoomUsingGrid(MapInfo mapInfo, int roomId, int featuresToPlace, IEnumerable<Tuple<int, DecorationFeatureDetails.Decoration>> decorationDetails)
        {
            var thisRoom = mapInfo.Room(roomId);
            var floorPoints = RoomTemplateUtilities.GetGridFromRoom(thisRoom.Room, 2, 1, 0.5);
            var floorPointsToUse = floorPoints.Shuffle().Take(featuresToPlace);
            AddStandardDecorativeFeaturesToRoom(mapInfo, roomId, floorPointsToUse, decorationDetails);
        }

        private void AddStandardDecorativeFeaturesToRoom(MapInfo mapInfo, int roomId, IEnumerable<RogueBasin.Point> points, IEnumerable<Tuple<int, DecorationFeatureDetails.Decoration>> decorationDetails)
        {
            if (points.Count() == 0)
                return;

            var featuresObjectsDetails = points.Select(p => new Tuple<RogueBasin.Point, DecorationFeatureDetails.Decoration>
                (p + mapInfo.Room(roomId).Location, Utility.ChooseItemFromWeights<DecorationFeatureDetails.Decoration>(decorationDetails)));
            var featureObjectsToPlace = featuresObjectsDetails.Select(dt => new Tuple<RogueBasin.Point, Feature>
                (dt.Item1, new RogueBasin.Features.StandardDecorativeFeature(dt.Item2.representation, dt.Item2.colour, dt.Item2.isBlocking)));

            var featuresPlaced = mapInfo.Populator.AddFeaturesToRoom(mapInfo, roomId, featureObjectsToPlace);
            LogFile.Log.LogEntryDebug("Placed " + featuresPlaced + " standard decorative features in room " + roomId, LogDebugLevel.Medium);
        }
        


    }
}
