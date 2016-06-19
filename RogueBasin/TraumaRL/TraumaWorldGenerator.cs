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

        List<int> allReplaceableVaults;

        //For development, skip making most of the levels
        bool quickLevelGen = false;

        ConnectivityMap levelLinks;
        List<int> gameLevels;
        static Dictionary<int, string> levelNaming;

        HashSet<Clue> placedClues;
        HashSet<Objective> placedObjectives;
        HashSet<Door> placedDoors;

        LogGenerator logGen = new LogGenerator();

        static List<Tuple<System.Drawing.Color, string>> availableColors;
        static Dictionary<int, List<DecorationFeatureDetails.DecorationFeatures>> featuresByLevel;
        List<Tuple<System.Drawing.Color, string>> usedColors = new List<Tuple<System.Drawing.Color, string>>();

        public TraumaWorldGenerator()
        {
            BuildTerrainMapping();
            
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

        //Quest important rooms / vaults
        Connection escapePodsConnection;
        int escapePodsLevel;

        //Wall mappings
        Dictionary<MapTerrain, List<MapTerrain>> brickTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> panelTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> securityTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> irisTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> bioTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> lineTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> dipTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> cutTerrainMapping;

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

        private void BuildTerrainMapping()
        {
            terrainMapping = new Dictionary<RoomTemplateTerrain, MapTerrain>();
            terrainMapping[RoomTemplateTerrain.Wall] = MapTerrain.Wall;
            terrainMapping[RoomTemplateTerrain.Floor] = MapTerrain.Empty;
            terrainMapping[RoomTemplateTerrain.Transparent] = MapTerrain.Void;
            terrainMapping[RoomTemplateTerrain.WallWithPossibleDoor] = MapTerrain.ClosedDoor;

            brickTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {

                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.BrickWall1, MapTerrain.BrickWall1, MapTerrain.BrickWall1, MapTerrain.BrickWall2, MapTerrain.BrickWall3, MapTerrain.BrickWall4, MapTerrain.BrickWall5 } }};

            panelTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.PanelWall1, MapTerrain.PanelWall1, MapTerrain.PanelWall1, MapTerrain.PanelWall2, MapTerrain.PanelWall3, MapTerrain.PanelWall4, MapTerrain.PanelWall5 } }};

            bioTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.BioWall1, MapTerrain.BioWall1, MapTerrain.BioWall1, MapTerrain.BioWall2, MapTerrain.BioWall3, MapTerrain.BioWall4, MapTerrain.BioWall5 } }};

            securityTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.SecurityWall1, MapTerrain.SecurityWall1, MapTerrain.SecurityWall1, MapTerrain.SecurityWall2, MapTerrain.SecurityWall3, MapTerrain.SecurityWall4, MapTerrain.SecurityWall5 } }};

            lineTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.LineWall1, MapTerrain.LineWall1, MapTerrain.LineWall1, MapTerrain.LineWall2, MapTerrain.LineWall3, MapTerrain.LineWall4, MapTerrain.LineWall5 } }};

            irisTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.IrisWall1, MapTerrain.IrisWall1, MapTerrain.IrisWall1, MapTerrain.IrisWall2, MapTerrain.IrisWall3, MapTerrain.IrisWall4, MapTerrain.IrisWall5 } }};

            dipTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.DipWall1, MapTerrain.DipWall1, MapTerrain.DipWall1, MapTerrain.DipWall2, MapTerrain.DipWall3, MapTerrain.DipWall4, MapTerrain.DipWall5 } }};

            cutTerrainMapping = new Dictionary<MapTerrain, List<MapTerrain>> {
                { MapTerrain.Wall, new List<MapTerrain> { MapTerrain.CutWall1, MapTerrain.CutWall1, MapTerrain.CutWall1, MapTerrain.CutWall2, MapTerrain.CutWall3, MapTerrain.CutWall4, MapTerrain.CutWall5 } }};
        }


        private DoorInfo RandomDoor(TemplatedMapGenerator generator)
        {
            return generator.PotentialDoors[Game.Random.Next(generator.PotentialDoors.Count())];
        }

        public ConnectivityMap LevelLinks { get { return levelLinks; } }

        public static Dictionary<int, string> LevelNaming { get { return levelNaming; } }

        MapModel levelMap;
        Dictionary<int, int> levelDepths;

        /// <summary>
        /// Build a level->level map showing how the levels are connected
        /// </summary>
        private void GenerateLevelLinks()
        {
            levelLinks = new ConnectivityMap();

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
        }

        public class LevelInfo
        {
            public LevelInfo(int levelNo)
            {
                LevelNo = levelNo;

                ConnectionsToOtherLevels = new Dictionary<int, Connection>();
                ReplaceableVaultConnections = new List<Connection>();
                ReplaceableVaultConnectionsUsed = new List<Connection>();
            }

            public int LevelNo { get; private set; }

            public Dictionary<int, Connection> ConnectionsToOtherLevels { get; set; }

            public TemplatedMapGenerator LevelGenerator { get; set; }
            public TemplatedMapBuilder LevelBuilder { get; set; }

            //Replaceable vault at target
            public List<Connection> ReplaceableVaultConnections { get; set; }
            public List<Connection> ReplaceableVaultConnectionsUsed { get; set; }

            public Dictionary<MapTerrain, List<MapTerrain>> TerrainMapping { get; set; }
        }


        /** Build a map using templated rooms */
        public MapInfo GenerateTraumaLevels(bool retry)
        {
            //We catch exceptions on generation and keep looping
            MapInfo mapInfo;

            //Reset shared state
            placedClues = new HashSet<Clue>();
            placedDoors = new HashSet<Door>();
            placedObjectives = new HashSet<Objective>();
            usedColors = new List<Tuple<System.Drawing.Color, string>>();

            //Generate the overall level structure
            GenerateLevelLinks();

            //Build each level individually

            Dictionary<int, LevelInfo> levelInfo = new Dictionary<int, LevelInfo>();

            var medicalInfo = GenerateMedicalLevel(medicalLevel);
            levelInfo[medicalLevel] = medicalInfo;
            if (!quickLevelGen)
            {
                var scienceInfo = GenerateScienceLevel(scienceLevel, scienceLevel * 100);
                levelInfo[scienceLevel] = scienceInfo;

                var bridgeInfo = GenerateBridgeLevel(bridgeLevel, bridgeLevel * 100);
                levelInfo[bridgeLevel] = bridgeInfo;

                var storageInfo = GenerateStorageLevel(storageLevel, storageLevel * 100);
                levelInfo[storageLevel] = storageInfo;

                var flightInfo = GenerateFlightDeckLevel(flightDeck, flightDeck * 100);
                levelInfo[flightDeck] = flightInfo;

                var reactorInfo = GenerateReactorLevel(reactorLevel, reactorLevel * 100);
                levelInfo[reactorLevel] = reactorInfo;

                var computerInfo = GenerateComputerCoreLevel(computerCoreLevel, computerCoreLevel * 100);
                levelInfo[computerCoreLevel] = computerInfo;

                var archologyInfo = GenerateArcologyLevel(arcologyLevel, arcologyLevel * 100);
                levelInfo[arcologyLevel] = archologyInfo;

                var commercialInfo = GenerateCommercialLevel(commercialLevel, commercialLevel * 100);
                levelInfo[commercialLevel] = commercialInfo;
            }
            //Make other levels generically

            IEnumerable<int> standardGameLevels;

            if (quickLevelGen)
            {
                standardGameLevels = gameLevels.Except(new List<int> { medicalLevel });
            }
            else {
                standardGameLevels = gameLevels.Except(new List<int> { medicalLevel, storageLevel, reactorLevel, flightDeck, arcologyLevel, scienceLevel, computerCoreLevel, bridgeLevel, commercialLevel });
            }

            foreach (var level in standardGameLevels)
            {
                var thisLevelInfo = GenerateStandardLevel(level, level * 100);
                levelInfo[level] = thisLevelInfo;
            }

            //Build the room graph containing all levels

            //Build and add the start level

            var mapInfoBuilder = new MapInfoBuilder();
            var startRoom = 0;
            var startLevelInfo = levelInfo[medicalLevel];
            mapInfoBuilder.AddConstructedLevel(medicalLevel, startLevelInfo.LevelGenerator.ConnectivityMap, startLevelInfo.LevelGenerator.GetRoomTemplatesInWorldCoords(),
                startLevelInfo.LevelGenerator.GetDoorsInMapCoords(), startRoom);

            //Build and add each connected level
            //Needs to be done in DFS fashion so we don't add the same level twice

            var levelsAdded = new HashSet<int> { medicalLevel };

            MapModel levelModel = new MapModel(levelLinks, medicalLevel);
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

            mapInfo = new MapInfo(mapInfoBuilder);

            //Add maps to the dungeon (must be ordered)
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

            //Set player's start location (must be done before adding items)

            var firstRoom = mapInfo.GetRoom(0);
            Game.Dungeon.Levels[0].PCStartLocation = new RogueBasin.Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);

            //Maintain a list of the replaceable vaults. We don't want to put stuff in these as they may disappear
            allReplaceableVaults = levelInfo.SelectMany(kv => kv.Value.ReplaceableVaultConnections.Select(v => v.Target)).ToList();

            //Set maps in engine (needs to be done before placing items and monsters)
            SetupMapsInEngine();

            //Add elevator features to link the maps
            if (!quickLevelGen)
                AddElevatorFeatures(mapInfo, levelInfo);
            
            //Attach debugger at this point
            //MessageBox.Show("Attach debugger now for any generation post slow pathing setup");

            //Generate quests at mapmodel level
            GenerateQuests(mapInfo, levelInfo);

            //Place loot
            CalculateLevelDifficulty();

            if (!quickLevelGen)
                PlaceLootInArmory(mapInfo, levelInfo);

            if (!quickLevelGen)
                AddGoodyQuestLogClues(mapInfo, levelInfo);

            //Add non-interactable features
            AddDecorationFeatures(mapInfo, levelInfo);

            //Add monsters
            Game.Dungeon.MonsterPlacement.CreateMonstersForLevels(mapInfo, gameLevels, levelDifficulty);

            //Add debug stuff in the first room
            AddDebugItems(mapInfo);

            //Check we are solvable
            var graphSolver = new GraphSolver(mapInfo.Model);
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
                throw new ApplicationException("Feature is not connected to elevator, aborting.");
            }

            if (retry)
            {
                throw new ApplicationException("It happened!");
            }

            return mapInfo;
        }

        private void AddDebugItems(MapInfo mapInfo)
        {
            var startRoom = mapInfo.StartRoom;

            var allWalkablePointsAndLevel = GetAllWalkablePointsInRooms(mapInfo, Enumerable.Repeat(startRoom, 1), true, true);
            var allWalkablePoints = allWalkablePointsAndLevel.Item2;
            var level = allWalkablePointsAndLevel.Item1;

            

            var logItem = new RogueBasin.Items.Log(logGen.GenerateArbitaryLogEntry("qe_medicalsecurity"));


            foreach (RogueBasin.Point p in allWalkablePoints)
            {
                var placedItem = Game.Dungeon.AddItem(logItem, level, p);

                if (placedItem)
                    break;
            }
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

                    var thisRoom = mapInfo.GetRoom(room);
                    var thisRoomArea = thisRoom.Room.Width * thisRoom.Room.Height;

                    var numberOfFeatures = (int)Math.Abs(Gaussian.BoxMuller(thisRoomArea * avConcentration, thisRoomArea * stdConcentration));
                    //LogFile.Log.LogEntryDebug("bm " + numberOfFeatures, LogDebugLevel.Low);

                    AddStandardDecorativeFeaturesToRoomUsingGrid(thisLevel, thisRoom, numberOfFeatures, featuresAndWeights);
                }
            }
        }

        /*private IEnumerable<Connection> GetCriticalRouteBetweenElevators(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, int startLevel, int endLevel)
        {
            var 

            return mapInfo.Model.GetPathBetweenVerticesInReducedMap(bridgeTransitConnection.Target, bridgeMainBridgeConnection.Target);
        }*/

        private IEnumerable<int> RoomsInConnectionSet(IEnumerable<int> testRooms, IEnumerable<Connection> connectionSet)
        {
            return connectionSet.Where(c => testRooms.Contains(c.Source) && testRooms.Contains(c.Target)).SelectMany(c => new List<int>{c.Source, c.Target}).Distinct();
        }

        private IEnumerable<Connection> ConnectionsWithinRoomSet(IEnumerable<int> testRooms, IEnumerable<Connection> connectionSet)
        {
            return connectionSet.Where(c => testRooms.Contains(c.Source) && testRooms.Contains(c.Target));
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
                var doorId = Game.Dungeon.DungeonInfo.LevelNaming[levelForBlocks] + "-" + doorName + Game.Random.Next();
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

            var manager = mapInfo.Model.DoorAndClueManager;

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
                var doorReadableId = Game.Dungeon.DungeonInfo.LevelNaming[thisLevel] + " armory";
                var doorId = doorReadableId;
                manager.PlaceDoor(new DoorRequirements(thisConnection, doorId, 1));
                var door = manager.GetDoorById(doorId);

                var unusedColor = GetUnusedColor();
                var clueName = unusedColor.Item2 + " key card";


                var lockedDoor = new RogueBasin.Locks.SimpleLockedDoor(door, clueName, unusedColor.Item1);
                var doorInfo = mapInfo.GetDoorForConnection(door.DoorConnectionFullMap);
                lockedDoor.LocationLevel = doorInfo.LevelNo;
                lockedDoor.LocationMap = doorInfo.MapLocation;

                Game.Dungeon.AddLock(lockedDoor);

                placedDoors.Add(door);

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
                var clueLocations = PlaceClueItem(mapInfo, cluesAndColors, true, false);

                //Vault is used
                levelInfo[thisLevel].ReplaceableVaultConnectionsUsed.Add(thisConnection);
            }
        
        }

        private void AddGoodyQuestLogClues(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            //Ensure that we have a goody room on every level that will support it
            var manager = mapInfo.Model.DoorAndClueManager;

            foreach (var kv in goodyRooms)
            {
                var thisLevel = kv.Key;
                var thisRoom = kv.Value;
                
                var doorId = Game.Dungeon.DungeonInfo.LevelNaming[thisLevel] + " armory";

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

            var manager = mapInfo.Model.DoorAndClueManager;

            var doorId = "medical-security";
            int objectsToPlace = 15;
            int objectsToDestroy = 10;

            //Place door
            manager.PlaceDoor(new DoorRequirements(elevatorConnection, doorId, objectsToDestroy));
            var door = manager.GetDoorById(doorId);

            var lockedDoor = new RogueBasin.Locks.SimpleLockedDoorWithMovie(door, "t_medicalsecurityunlocked", "t_medicalsecuritylocked", doorId, System.Drawing.Color.Red);
            var doorInfo = mapInfo.GetDoorForConnection(door.DoorConnectionFullMap);
            lockedDoor.LocationLevel = doorInfo.LevelNo;
            lockedDoor.LocationMap = doorInfo.MapLocation;

            Game.Dungeon.AddLock(lockedDoor);

            placedDoors.Add(door);

            //Place monsters (not in corridors)
            
            //This will be restricted to the medical level since we cut off the door
            var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            allowedRoomsForClues = mapInfo.FilterOutCorridors(allowedRoomsForClues);
            var roomsToPlaceMonsters = new List<int>();

            var roomsForMonsters = GetRandomRoomsForClues(mapInfo, objectsToPlace, allowedRoomsForClues);
            var clues = manager.AddCluesToExistingDoor(doorId, roomsForMonsters);

            PlaceCreatureClues<RogueBasin.Creatures.Camera>(mapInfo, clues, true, false);

            //Place log entries explaining the puzzle
            //These will not be turned into in-engine clue items, so they can't be used to open the door
            //They are added though, to ensure that they are readable before the door is opened

            var roomsForLogs = GetRandomRoomsForClues(mapInfo, 2, allowedRoomsForClues);
            var logClues = manager.AddCluesToExistingDoor(doorId, roomsForLogs);

            //try
            //{
                var log1 = new Tuple<LogEntry, Clue>(logGen.GenerateElevatorLogEntry(medicalLevel, lowerAtriumLevel), logClues[0]);
                var log2 = new Tuple<LogEntry, Clue>(logGen.GenerateArbitaryLogEntry("qe_medicalsecurity"), logClues[1]);
                PlaceLogClues(mapInfo, new List<Tuple<LogEntry, Clue>> { log1, log2 }, true, true);
            //}
           // catch (Exception)
            //{
                //Ignore log problems
            //}
        }

        /*
        private void BlockPathBetweenElevatorsWithSimpleDoor(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, Dictionary<int, List<Connection>> roomConnectivityMap, 
            string doorId, int cluesForDoor, int sourceRoom, int endRoom, 
            double distanceFromSourceRatio, bool enforceClueOnDestLevel, CluePath clueNotOnCriticalPath, bool clueNotInCorridors,
            bool hasLogClue, CluePath logOnCriticalPath, bool logNotInCorridors)
        {
            BlockPathBetweenRoomsWithSimpleDoor(mapInfo, levelInfo, roomConnectivityMap,
            doorId, cluesForDoor, sourceRoom, endRoom,
            distanceFromSourceRatio, enforceClueOnDestLevel, clueNotOnCriticalPath, clueNotInCorridors,
            hasLogClue, logOnCriticalPath, logNotInCorridors);
        }*/

        private void BlockPathBetweenRoomsWithSimpleDoor(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, Dictionary<int, List<Connection>> roomConnectivityMap,
            string doorId, string doorName, System.Drawing.Color colorToUse, int cluesForDoor, int sourceRoom, int endRoom, 
            double distanceFromSourceRatio, bool enforceClueOnDestLevel, CluePath clueNotOnCriticalPath, bool clueNotInCorridors,
            bool hasLogClue, CluePath logOnCriticalPath, bool logNotInCorridors)
        {
            var manager = mapInfo.Model.DoorAndClueManager;

            var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(sourceRoom, endRoom);
            var criticalConnectionForDoor = criticalPath.ElementAt((int)Math.Min(criticalPath.Count() * distanceFromSourceRatio, criticalPath.Count() - 1));

            criticalConnectionForDoor = CheckAndReplaceConnectionIfOccupied(manager, criticalPath, criticalConnectionForDoor);

            //Place door

            PlaceDoorOnMap(mapInfo, doorId, doorName, cluesForDoor, colorToUse, criticalConnectionForDoor);

            //Place clues

            var allRoomsForClue = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            var preferredRooms = FilterClueRooms(mapInfo, allRoomsForClue, criticalPath, enforceClueOnDestLevel, clueNotOnCriticalPath, clueNotInCorridors);

            var roomsForClues = GetRandomRoomsForClues(mapInfo, cluesForDoor, preferredRooms);
            var clues = manager.AddCluesToExistingDoor(doorId, roomsForClues);

            var cluesAndColors = clues.Select(c => new Tuple<Clue, System.Drawing.Color, string>(c, colorToUse, doorName));

            var clueLocations = PlaceClueItem(mapInfo, cluesAndColors, clueNotInCorridors, false);

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

                //try
                //{
                var coupledLogs = logGen.GenerateCoupledDoorLogEntry(doorName, mapInfo.GetLevelForRoomIndex(criticalConnectionForDoor.Source),
                    clueLocations.First().Item1);
                var log1 = new Tuple<LogEntry, Clue>(coupledLogs[0], logClues[0]);
                var log2 = new Tuple<LogEntry, Clue>(coupledLogs[1], logCluesNonCritical[0]);
                PlaceLogClues(mapInfo, new List<Tuple<LogEntry, Clue>> { log1, log2 }, true, true);
                //}
                // catch (Exception)
                //{
                //Ignore log problems
                //}
            }
        }

        private Door PlaceDoorOnMap(MapInfo mapInfo, string doorId, string doorName, int numberOfCluesForDoor, System.Drawing.Color colorToUse, Connection criticalConnectionForDoor)
        {
            var manager = mapInfo.Model.DoorAndClueManager;

            manager.PlaceDoor(new DoorRequirements(criticalConnectionForDoor, doorId, numberOfCluesForDoor));
            var door = manager.GetDoorById(doorId);

            var lockedDoor = new RogueBasin.Locks.SimpleLockedDoor(door, doorName, colorToUse);
            var doorInfo = mapInfo.GetDoorForConnection(door.DoorConnectionFullMap);
            lockedDoor.LocationLevel = doorInfo.LevelNo;
            lockedDoor.LocationMap = doorInfo.MapLocation;

            Game.Dungeon.AddLock(lockedDoor);

            placedDoors.Add(door);

            return door;
        }

        private Door PlaceMovieDoorOnMap(MapInfo mapInfo, string doorId, string doorName, int numberOfCluesForDoor, System.Drawing.Color colorToUse, string openMovie, string cantOpenMovie, Connection criticalConnectionForDoor)
        {
            var manager = mapInfo.Model.DoorAndClueManager;

            manager.PlaceDoor(new DoorRequirements(criticalConnectionForDoor, doorId, numberOfCluesForDoor));
            var door = manager.GetDoorById(doorId);

            var lockedDoor = new RogueBasin.Locks.SimpleLockedDoorWithMovie(door, openMovie, cantOpenMovie, doorName, colorToUse);
            var doorInfo = mapInfo.GetDoorForConnection(door.DoorConnectionFullMap);
            lockedDoor.LocationLevel = doorInfo.LevelNo;
            lockedDoor.LocationMap = doorInfo.MapLocation;

            Game.Dungeon.AddLock(lockedDoor);

            placedDoors.Add(door);

            return door;
        }

        private void PlaceDoorOnMap(MapInfo mapInfo, Lock lockedDoor, Door door)
        {
            var doorInfo = mapInfo.GetDoorForConnection(door.DoorConnectionFullMap);
            lockedDoor.LocationLevel = doorInfo.LevelNo;
            lockedDoor.LocationMap = doorInfo.MapLocation;

            Game.Dungeon.AddLock(lockedDoor);

            placedDoors.Add(door);
        }

        private Connection CheckAndReplaceConnectionIfOccupied(DoorAndClueManager manager, IEnumerable<Connection> criticalPath, Connection connectionCandidate)
        {
            if (manager.GetDoorsForEdge(connectionCandidate).Count() > 0)
            {
                //Try another edge
                var possibleEdges = criticalPath.Shuffle();
                Connection foundEdge = null;
                foreach (var edge in possibleEdges)
                {
                    if (manager.GetDoorsForEdge(connectionCandidate).Count() == 0)
                    {
                        foundEdge = edge;
                        break;
                    }
                }

                if (foundEdge == null)
                {
                    throw new ApplicationException("No free doors to place lock.");
                }

                return foundEdge;
            }

            return connectionCandidate;
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

        private List<int> GetRandomRoomsForClues(MapInfo info, int objectsToPlace, IEnumerable<int> allowedRoomsForClues)
        {
            if (allowedRoomsForClues.Count() == 0)
                throw new ApplicationException("Not enough rooms to place clues");

            //To get an even distribution we need to take into account how many nodes are in each group node
            var expandedAllowedRoomForClues = allowedRoomsForClues.Except(allReplaceableVaults).SelectMany(r => Enumerable.Repeat(r, info.Model.GraphNoCycles.roomMappingNoCycleToFullMap[r].Count()));

            if (expandedAllowedRoomForClues.Count() == 0)
                throw new ApplicationException("No allowed rooms for clues.");

            var roomsToPlaceMonsters = new List<int>();

            while (roomsToPlaceMonsters.Count() < objectsToPlace)
            {
                var shuffledRooms = expandedAllowedRoomForClues.Shuffle();
                foreach (var room in shuffledRooms)
                {
                    roomsToPlaceMonsters.Add(room);
                    if (roomsToPlaceMonsters.Count() == objectsToPlace)
                        break;
                }
            }

            return roomsToPlaceMonsters;
        }

        int selfDestructRoom;

        private void BuildMainQuest(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            var deadEndRooms = roomConnectivityMap[0];
            
            //MAIN QUEST


            //escapePodsConnection = levelInfo[flightDeck].ReplaceableVaultConnections.Except(levelInfo[flightDeck].ReplaceableVaultConnectionsUsed).RandomElement();
            //escapePodsLevel = flightDeck;
            //levelInfo[flightDeck].ReplaceableVaultConnectionsUsed.Add(escapePodsConnection);

            //TODO: replace vault in map

            var manager = mapInfo.Model.DoorAndClueManager;

            //Escape pod end game
            EscapePod(mapInfo);
            
            //Self destruct
            //Requires priming the reactor

            SelfDestruct(mapInfo, levelInfo, manager);

            //Computer core to destroy
            ComputerCore(mapInfo, levelInfo, manager);


            /*
             * //standard clue placing
            var reactorColor = GetUnusedColor();
            var allowedRoomsForClue = manager.GetValidRoomsToPlaceClueForObjective("self-destruct");
            var reactorClue = manager.AddCluesToExistingObjective("self-destruct", new List<int> { allowedRoomsForClue.RandomElement() }).First();
            PlaceClueItem(mapInfo, new Tuple<Clue, Color, string>(reactorClue, reactorColor.Item1, "self-destruct-" + reactorColor.Item2), false, false);
            */
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



            //Fueling system
            //int fuelingLevel = lowerAtriumLevel;
            //var fuelingLevelIndices = mapInfo.GetRoomIndicesForLevel(fuelingLevel);
            //var randomRoomForFueling = fuelingLevelIndices.Except(allReplaceableVaults).RandomElement();

            //mapInfo.Model.DoorAndClueManager.AddCluesToExistingDoor("escape", new List<int> { randomRoomForFueling });
        }

        private void EscapePod(MapInfo mapInfo)
        {
            var escapePodRoom = escapePodsConnection.Target;
            PlaceFeatureInRoom(mapInfo, new RogueBasin.Features.EscapePod(), new List<int> { escapePodRoom }, true, false);

            LogFile.Log.LogEntryDebug("Adding features to escape pod room", LogDebugLevel.Medium);
            var escapePodDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.MedicalAutomat]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Pillar1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Pillar2])
            };
            AddStandardDecorativeFeaturesToRoom(mapInfo.GetLevelForRoomIndex(escapePodRoom), mapInfo.GetRoom(escapePodRoom), 20, escapePodDecorations, false);

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
            var bridgeLocation = PlaceObjective(mapInfo, selfDestructObjective, new RogueBasin.Features.SelfDestructObjective(selfDestructObjective, mapInfo.Model.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(selfDestructObjective)), true, true);
            
            UseVault(levelInfo, selfDestructConnection);

            var bridgeDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer3]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Screen1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.HighTechBench])
            };
            AddStandardDecorativeFeaturesToRoom(mapInfo.GetLevelForRoomIndex(selfDestructRoom), mapInfo.GetRoom(selfDestructRoom), 20, bridgeDecorations, false);

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
            var reactorLocation = PlaceObjective(mapInfo, selfDestructPrimeObjective, new RogueBasin.Features.SelfDestructPrimeObjective(selfDestructPrimeObjective, mapInfo.Model.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(selfDestructPrimeObjective)), true, true);

            var reactorDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Instrument3])
            };
            AddStandardDecorativeFeaturesToRoom(mapInfo.GetLevelForRoomIndex(reactorSelfDestructVault), mapInfo.GetRoom(reactorSelfDestructVault), 20, reactorDecorations, false);

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
            AddStandardDecorativeFeaturesToRoom(captainsIdLevel, mapInfo.GetRoom(captainsIdRoom), 10, decorations, false);

            //Logs

            var manager = mapInfo.Model.DoorAndClueManager;

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
            AddStandardDecorativeFeaturesToRoom(techIdLevel, mapInfo.GetRoom(techIdRoom), 20, bioDecorations, false);
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
            var manager = mapInfo.Model.DoorAndClueManager;

            manager.PlaceDoor(new DoorRequirements(elevatorToArcology, arcologyDoorId, 1));
            var door = manager.GetDoorById(arcologyDoorId);
            
            var arcologyDoor = new RogueBasin.Locks.SimpleOptionalLockedDoorWithMovie(door, "arcologyunlocked", "arcologylocked", "Override the security and go in anyway?", arcologyDoorName, arcologyDoorColor);

            PlaceDoorOnMap(mapInfo, arcologyDoor, door);

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
            AddStandardDecorativeFeaturesToRoom(biowareLevel, mapInfo.GetRoom(biowareRoom), 10, bioDecorations, false);

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

            PlaceDoorOnMap(mapInfo, arcologyAntDoor, door2);
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
            PlaceObjective(mapInfo, antennaeObj, new RogueBasin.Features.AntennaeObjective(antennaeObj, mapInfo.Model.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(antennaeObj)), true, true);

            UseVault(levelInfo, antennaeVaultConnection);

            //Extra stuff for antenna room

            var antennaeLevel = mapInfo.GetLevelForRoomIndex(antennaeVault);

            var monstersToPlace = new List<Monster> { new RogueBasin.Creatures.RotatingTurret(), new RogueBasin.Creatures.HeavyTurret(), new RogueBasin.Creatures.PatrolBotRanged(), new RogueBasin.Creatures.PatrolBotRanged()};
            Game.Dungeon.MonsterPlacement.AddMonstersToRoom(mapInfo, antennaeLevel, antennaeVault, monstersToPlace);

            var decorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Antennae]) };
            AddStandardDecorativeFeaturesToRoom(antennaeLevel, mapInfo.GetRoom(antennaeVault), 10, decorations, false);

            //Servo motor

            var servoRoom = PlaceMovieClueForObjectiveInVault(mapInfo, levelInfo, antennaeObjName, (char)312, "interface_demod", "Interface Demodulator", new List<int> { scienceLevel, storageLevel });
            var servoLevel = mapInfo.GetLevelForRoomIndex(servoRoom);

            var servoDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Machine]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Machine2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.MachinePart2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.MachinePart3])
            };
            AddStandardDecorativeFeaturesToRoom(servoLevel, mapInfo.GetRoom(servoRoom), 10, servoDecorations, false);

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
            var manager = mapInfo.Model.DoorAndClueManager;

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

            var captainIdClue = mapInfo.Model.DoorAndClueManager.AddCluesToExistingDoor(doorId, new List<int> { captainIdRoom }).First();
            PlaceClueItem(mapInfo, new Tuple<Clue, System.Drawing.Color, string>(captainIdClue, clueColour, clueName), true, true);

            LogFile.Log.LogEntryDebug("Placing " + clueName +" on level " + captainsIdLevel + " in vault " + captainIdRoom, LogDebugLevel.Medium);

            return captainIdRoom;
        }

        private int PlaceClueItemForDoorInVault(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, string doorId, Item itemToPlace, string clueName, IEnumerable<int> idealLevelsForClue)
        {
            var manager = mapInfo.Model.DoorAndClueManager;

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

            var captainIdClue = mapInfo.Model.DoorAndClueManager.AddCluesToExistingDoor(doorId, new List<int> { captainIdRoom }).First();

            PlaceItems(mapInfo, new List<Item> { itemToPlace }, new List<int> {captainIdRoom}, true, true, true);

            LogFile.Log.LogEntryDebug("Placing " + clueName + " on level " + captainsIdLevel + " in vault " + captainIdRoom, LogDebugLevel.Medium);

            return captainIdRoom;
        }

        private int PlaceClueForObjectiveInVault(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, string doorId, System.Drawing.Color clueColour, string clueName, IEnumerable<int> idealLevelsForClue)
        {
            var manager = mapInfo.Model.DoorAndClueManager;

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

            var captainIdClue = mapInfo.Model.DoorAndClueManager.AddCluesToExistingObjective(doorId, new List<int> { captainIdRoom }).First();
            PlaceClueItem(mapInfo, new Tuple<Clue, System.Drawing.Color, string>(captainIdClue, clueColour, clueName), true, true);

            LogFile.Log.LogEntryDebug("Placing " + clueName + " on level " + captainsIdLevel + " in vault " + captainIdRoom, LogDebugLevel.Medium);

            return captainIdRoom;
        }

        private int PlaceMovieClueForObjectiveInVault(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo, string objectiveId, char representation, string pickupMovie, string description, IEnumerable<int> idealLevelsForClue)
        {
            var manager = mapInfo.Model.DoorAndClueManager;

            var possibleRoomsForCaptainsId = manager.GetValidRoomsToPlaceClueForObjective(objectiveId);
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

            var captainIdClue = mapInfo.Model.DoorAndClueManager.AddCluesToExistingObjective(objectiveId, new List<int> { captainIdRoom }).First();
            Item clueItemToPlace = new RogueBasin.Items.MovieClue(captainIdClue, representation, pickupMovie, description);
            PlaceClueItem(mapInfo, new List<Tuple<Clue,Item>>{new Tuple<Clue, Item>(captainIdClue, clueItemToPlace)}, true, true);

            LogFile.Log.LogEntryDebug("Placing " + clueItemToPlace.SingleItemDescription + " on level " + captainsIdLevel + " in vault " + captainIdRoom, LogDebugLevel.Medium);

            return captainIdRoom;
        }


        private static void AddElevatorFeatures(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            var elevatorLocations = new Dictionary<Tuple<int, int>, RogueBasin.Point>();

            foreach (var kv in levelInfo)
            {
                var thisLevelNo = kv.Key;
                var thisLevelInfo = kv.Value;

                foreach (var connectionToOtherLevel in thisLevelInfo.ConnectionsToOtherLevels)
                {
                    var elevatorLoc = mapInfo.GetRandomPointInRoomOfTerrain(connectionToOtherLevel.Value.Target, RoomTemplateTerrain.Floor);
                    elevatorLocations[new Tuple<int, int>(thisLevelNo, connectionToOtherLevel.Key)] = elevatorLoc;
                }
            }

            foreach (var kv in elevatorLocations)
            {
                var sourceLevel = kv.Key.Item1;
                var targetLevel = kv.Key.Item2;

                var sourceToTargetElevator = kv.Value;
                var targetToSourceElevator = elevatorLocations[new Tuple<int, int>(targetLevel, sourceLevel)];

                Game.Dungeon.AddFeature(new RogueBasin.Features.Elevator(targetLevel, targetToSourceElevator), sourceLevel, sourceToTargetElevator);

                LogFile.Log.LogEntryDebug("Adding elevator connection " + sourceLevel + ":" + targetLevel + " via points" +
                    sourceToTargetElevator + "->" + targetToSourceElevator, LogDebugLevel.Medium);
            }
        }

        private void PlaceCreatureClues<T>(MapInfo mapInfo, List<Clue> monsterCluesToPlace, bool autoPickup, bool includeVaults) where T : Monster, new()
        {
            foreach (var clue in monsterCluesToPlace)
            {
                if (placedClues.Contains(clue))
                    continue;

                var roomsForClue = GetAllWalkablePointsToPlaceClueBoundariesOnly(mapInfo, clue, true, includeVaults);

                if (!roomsForClue.Item2.Any())
                    roomsForClue = GetAllWalkablePointsToPlaceClue(mapInfo, clue, true, includeVaults);

                var levelForClue = roomsForClue.Item1;
                var allWalkablePoints = roomsForClue.Item2;

                bool placedItem = false;

                var newMonster = new T();
                Item clueItem;
                if (autoPickup)
                    clueItem = new RogueBasin.Items.ClueAutoPickup(clue);
                else
                    clueItem = new RogueBasin.Items.Clue(clue);

                newMonster.PickUpItem(clueItem);

                foreach (RogueBasin.Point p in allWalkablePoints)
                {
                    placedItem = Game.Dungeon.AddMonster(newMonster, levelForClue, p);

                    if (placedItem)
                        break;
                }

                if (!placedItem)
                    throw new ApplicationException("Nowhere to place monster");

                placedClues.Add(clue);
            }
        }

        private void PlaceLogClues(MapInfo mapInfo, List<Tuple<LogEntry, Clue>> logCluesToPlace, bool boundariesPreferred, bool cluesNotInCorridors)
        {
            foreach (var t in logCluesToPlace)
            {
                var clue = t.Item2;
                var logEntry = t.Item1;

                if (placedClues.Contains(clue))
                    continue;

                Tuple<int, IEnumerable<RogueBasin.Point>> roomsForClue;
                if (boundariesPreferred)
                {
                    roomsForClue = GetAllWalkablePointsToPlaceClueBoundariesOnly(mapInfo, clue, cluesNotInCorridors, false);

                    if (!roomsForClue.Item2.Any())
                        roomsForClue = GetAllWalkablePointsToPlaceClue(mapInfo, clue, cluesNotInCorridors, false);
                }
                else
                    roomsForClue = GetAllWalkablePointsToPlaceClue(mapInfo, clue, cluesNotInCorridors,false);

                var levelForClue = roomsForClue.Item1;
                var allWalkablePoints = roomsForClue.Item2;

                bool placedItem = false;

                var logItem = new RogueBasin.Items.Log(logEntry);

                foreach (RogueBasin.Point p in allWalkablePoints)
                {
                    placedItem = Game.Dungeon.AddItem(logItem, levelForClue, p);

                    if (placedItem)
                        break;
                }

                if (!placedItem)
                    throw new ApplicationException("Nowhere to place item");

                placedClues.Add(clue);
            }
        }

        private void PlaceItems(MapInfo mapInfo, IEnumerable<Item> items, IEnumerable<int> rooms, bool boundariesPreferred, bool notInCorridors, bool includeVaults)
        {
            foreach (var item in items)
            {

                Tuple<int, IEnumerable<RogueBasin.Point>> roomsForClue;
                if (boundariesPreferred)
                {
                    roomsForClue = GetAllWalkablePointsInRoomsBoundariesOnly(mapInfo, rooms, notInCorridors, includeVaults);

                    if (!roomsForClue.Item2.Any())
                        roomsForClue = GetAllWalkablePointsInRooms(mapInfo, rooms, notInCorridors, includeVaults);
                }
                else
                    roomsForClue = GetAllWalkablePointsInRooms(mapInfo, rooms, notInCorridors, includeVaults);

                var levelForClue = roomsForClue.Item1;
                var allWalkablePoints = roomsForClue.Item2;

                bool placedItem = false;

                foreach (RogueBasin.Point p in allWalkablePoints)
                {
                    placedItem = Game.Dungeon.AddItem(item, levelForClue, p);

                    if (placedItem)
                        break;
                }

                if (!placedItem)
                    throw new ApplicationException("Nowhere to place item");
            }
        }


        private Tuple<int, IEnumerable<RogueBasin.Point>> GetAllWalkablePointsToPlaceClue(MapInfo mapInfo, Clue clue, bool filterCorridors, bool includeVaults)
        {
            var possibleRooms = clue.PossibleClueRoomsInFullMap;

            IEnumerable<int> possibleRoomMinusVaults = possibleRooms;
            if(!includeVaults)
                possibleRoomMinusVaults = possibleRooms.Except(allReplaceableVaults);

            IEnumerable<int> candidateRooms = possibleRoomMinusVaults;
            if (filterCorridors)
                candidateRooms = mapInfo.FilterOutCorridors(possibleRoomMinusVaults);
            if (candidateRooms.Count() == 0)
                candidateRooms = possibleRoomMinusVaults;

            //Must be on the same level
            var levelForRandomRoom = mapInfo.GetLevelForRoomIndex(candidateRooms.First());

            var allWalkablePoints = new List<RogueBasin.Point>();

            //Hmm, could be quite expensive
            foreach (var room in candidateRooms)
            {
                var allPossiblePoints = mapInfo.GetAllPointsInRoomOfTerrain(room, RoomTemplateTerrain.Floor);
                allWalkablePoints.AddRange(Game.Dungeon.GetWalkablePointsFromSet(levelForRandomRoom, allPossiblePoints));
            }

            return new Tuple<int, IEnumerable<RogueBasin.Point>>(levelForRandomRoom, allWalkablePoints.Shuffle());
        }

        private IEnumerable<int> GetAllRoomsToPlaceClue(MapInfo mapInfo, Clue clue, bool filterCorridors, bool includeVaults)
        {
            var possibleRooms = clue.PossibleClueRoomsInFullMap;

            IEnumerable<int> possibleRoomMinusVaults = possibleRooms;
            if (!includeVaults)
                possibleRoomMinusVaults = possibleRooms.Except(allReplaceableVaults);

            IEnumerable<int> candidateRooms = possibleRoomMinusVaults;
            if (filterCorridors)
                candidateRooms = mapInfo.FilterOutCorridors(possibleRoomMinusVaults);
            if (candidateRooms.Count() == 0)
                candidateRooms = possibleRoomMinusVaults;

            return candidateRooms;
        }

        private Tuple<int, IEnumerable<RogueBasin.Point>> GetAllWalkablePointsInRooms(MapInfo mapInfo, IEnumerable<int> rooms, bool filterCorridors, bool includeVaults)
        {
            var possibleRooms = rooms;

            IEnumerable<int> possibleRoomMinusVaults = possibleRooms;
            if (!includeVaults)
                possibleRoomMinusVaults = possibleRooms.Except(allReplaceableVaults);

            IEnumerable<int> candidateRooms = possibleRoomMinusVaults;
            if (filterCorridors)
                candidateRooms = mapInfo.FilterOutCorridors(possibleRoomMinusVaults);
            if (candidateRooms.Count() == 0)
                candidateRooms = possibleRoomMinusVaults;

            //Must be on the same level
            var levelForRandomRoom = mapInfo.GetLevelForRoomIndex(candidateRooms.First());

            var allWalkablePoints = new List<RogueBasin.Point>();

            //Hmm, could be quite expensive
            foreach (var room in candidateRooms)
            {
                var allPossiblePoints = mapInfo.GetAllPointsInRoomOfTerrain(room, RoomTemplateTerrain.Floor);
                allWalkablePoints.AddRange(Game.Dungeon.GetWalkablePointsFromSet(levelForRandomRoom, allPossiblePoints));
            }

            return new Tuple<int, IEnumerable<RogueBasin.Point>>(levelForRandomRoom, allWalkablePoints.Shuffle());
        }

        private Tuple<int, IEnumerable<RogueBasin.Point>> GetAllWalkablePointsInRoomsBoundariesOnly(MapInfo mapInfo, IEnumerable<int> rooms, bool filterCorridors, bool includeVaults)
        {
            var possibleRooms = rooms;
            if(!includeVaults)
                possibleRooms = possibleRooms.Except(allReplaceableVaults);
            var candidateRooms = possibleRooms;
            if (filterCorridors)
                candidateRooms = mapInfo.FilterOutCorridors(candidateRooms);
            if (candidateRooms.Count() == 0)
                candidateRooms = possibleRooms;

            //Must be on the same level
            var levelForRandomRoom = mapInfo.GetLevelForRoomIndex(candidateRooms.First());

            var allWalkablePoints = new List<RogueBasin.Point>();

            //Hmm, could be quite expensive
            foreach (var room in candidateRooms)
            {
                var allPossiblePoints = mapInfo.GetBoundaryPointsInRoomOfTerrain(room);
                allWalkablePoints.AddRange(Game.Dungeon.GetWalkablePointsFromSet(levelForRandomRoom, allPossiblePoints));
            }

            return new Tuple<int, IEnumerable<RogueBasin.Point>>(levelForRandomRoom, allWalkablePoints.Shuffle());
        }


        private Tuple<int, IEnumerable<RogueBasin.Point>> GetAllWalkablePointsToPlaceClueBoundariesOnly(MapInfo mapInfo, Clue clue, bool filterCorridors, bool includeVaults)
        {
            var possibleRooms = clue.PossibleClueRoomsInFullMap;
            IEnumerable<int> initialRooms = possibleRooms;
            if(!includeVaults)
                initialRooms = possibleRooms.Except(allReplaceableVaults);
            var candidateRooms = initialRooms;
            if (filterCorridors)
                candidateRooms = mapInfo.FilterOutCorridors(initialRooms);
            if (candidateRooms.Count() == 0)
                candidateRooms = initialRooms;

            //Must be on the same level
            var levelForRandomRoom = mapInfo.GetLevelForRoomIndex(candidateRooms.First());

            var allWalkablePoints = new List<RogueBasin.Point>();

            //Hmm, could be quite expensive
            foreach (var room in candidateRooms)
            {
                var allPossiblePoints = mapInfo.GetBoundaryPointsInRoomOfTerrain(room);
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

        /// <summary>
        /// Add any remaining clues, locks and objectives as simple types (to ensure we don't miss anything)
        /// </summary>
        /// <param name="mapInfo"></param>
        private void AddSimpleCluesAndLocks(MapInfo mapInfo)
        {
            //Add clues

            //Find a random room corresponding to a vertex with a clue and place a clue there
            foreach (var cluesAtVertex in mapInfo.Model.DoorAndClueManager.ClueMap)
            {
                foreach (var clue in cluesAtVertex.Value)
                {
                    if (placedClues.Contains(clue))
                        continue;

                    bool avoidCorridors = false;
                    PlaceClueItem(mapInfo, new Tuple<Clue, System.Drawing.Color, string>(clue, System.Drawing.Color.Magenta, ""), avoidCorridors, false);

                    placedClues.Add(clue);
                }

            }

            //Add locks to dungeon as simple doors

            foreach (var door in mapInfo.Model.DoorAndClueManager.DoorMap.Values)
            {
                if (placedDoors.Contains(door))
                    continue;

                var lockedDoor = new RogueBasin.Locks.SimpleLockedDoor(door);
                var doorInfo = mapInfo.GetDoorForConnection(door.DoorConnectionFullMap);
                lockedDoor.LocationLevel = doorInfo.LevelNo;
                lockedDoor.LocationMap = doorInfo.MapLocation;

                LogFile.Log.LogEntryDebug("Lock door level " + lockedDoor.LocationLevel + " loc: " + doorInfo.MapLocation, LogDebugLevel.High);

                Game.Dungeon.AddLock(lockedDoor);

                placedDoors.Add(door);
            }

            //Add objectives to dungeon as simple objectives

            foreach (var objAtVertex in mapInfo.Model.DoorAndClueManager.ObjectiveRoomMap)
            {
                foreach (var obj in objAtVertex.Value)
                {

                    if (placedObjectives.Contains(obj))
                        continue;

                    var possibleRooms = obj.PossibleClueRoomsInFullMap;
                    var randomRoom = possibleRooms.RandomElement();
                    var levelForRandomRoom = mapInfo.GetLevelForRoomIndex(randomRoom);

                    var allPossiblePoints = mapInfo.GetAllPointsInRoomOfTerrain(randomRoom, RoomTemplateTerrain.Floor);
                    var allWalkablePoints = Game.Dungeon.GetWalkablePointsFromSet(levelForRandomRoom, allPossiblePoints);

                    bool placedItem = false;
                    foreach (RogueBasin.Point p in allWalkablePoints)
                    {
                        var objectiveFeature = new RogueBasin.Features.SimpleObjective(obj, mapInfo.Model.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(obj));
                        placedItem = Game.Dungeon.AddFeature(objectiveFeature, levelForRandomRoom, p);

                        if (placedItem)
                            break;
                    }

                    if (!placedItem)
                    {
                        var str = "Can't place objective " + obj.Id;
                        LogFile.Log.LogEntryDebug(str, LogDebugLevel.High);
                        throw new ApplicationException(str);
                    }

                    placedObjectives.Add(obj);
                }
            }
        }

        private List<Tuple<int, RogueBasin.Point>> PlaceClueItem(MapInfo mapInfo, Tuple<Clue, System.Drawing.Color, string> clues, bool avoidCorridors, bool includeVaults)
        {
            return PlaceClueItem(mapInfo, new List<Tuple<Clue, System.Drawing.Color, string>> { clues }, avoidCorridors, includeVaults);
        }

        private List<Tuple<int, RogueBasin.Point>> PlaceClueItem(MapInfo mapInfo, IEnumerable<Tuple<Clue, System.Drawing.Color, string>> clues, bool avoidCorridors, bool includeVaults)
        {
            var toRet = new List<Tuple<int, RogueBasin.Point>>();

            foreach (var tp in clues)
            {
                var clue = tp.Item1;

                var roomsForClue = GetAllWalkablePointsToPlaceClue(mapInfo, clue, avoidCorridors, includeVaults);
                var levelForRandomRoom = roomsForClue.Item1;
                var allWalkablePoints = roomsForClue.Item2;

                bool placedItem = false;
                RogueBasin.Point pointToPlace = null;
                foreach (RogueBasin.Point p in allWalkablePoints)
                {
                    placedItem = Game.Dungeon.AddItem(new RogueBasin.Items.Clue(clue, tp.Item2, tp.Item3), levelForRandomRoom, p);
                    pointToPlace = p;

                    if (placedItem)
                        break;
                }

                placedClues.Add(clue);

                if (!placedItem)
                {
                    var str = "Can't place clue " + clue.OpenLockIndex;
                    LogFile.Log.LogEntryDebug(str, LogDebugLevel.High);
                    throw new ApplicationException(str);
                }

                toRet.Add(new Tuple<int, RogueBasin.Point>(levelForRandomRoom, pointToPlace));
            }

            return toRet;
        }

        private List<Tuple<int, RogueBasin.Point>> PlaceClueItem(MapInfo mapInfo, List<Tuple<Clue, Item>> clueItem, bool avoidCorridors, bool includeVaults)
        {
            var toRet = new List<Tuple<int, RogueBasin.Point>>();

            foreach (var tp in clueItem)
            {
                var clue = tp.Item1;
                var item = tp.Item2;

                var roomsForClue = GetAllWalkablePointsToPlaceClue(mapInfo, clue, avoidCorridors, includeVaults);
                var levelForRandomRoom = roomsForClue.Item1;
                var allWalkablePoints = roomsForClue.Item2;

                bool placedItem = false;
                RogueBasin.Point pointToPlace = null;
                foreach (RogueBasin.Point p in allWalkablePoints)
                {
                    placedItem = Game.Dungeon.AddItem(item, levelForRandomRoom, p);
                    pointToPlace = p;

                    if (placedItem)
                        break;
                }

                placedClues.Add(clue);

                if (!placedItem)
                {
                    var str = "Can't place clue " + clue.OpenLockIndex;
                    LogFile.Log.LogEntryDebug(str, LogDebugLevel.High);
                    throw new ApplicationException(str);
                }

                toRet.Add(new Tuple<int, RogueBasin.Point>(levelForRandomRoom, pointToPlace));
            }

            return toRet;
        }

        private Tuple<int, RogueBasin.Point> PlaceObjective(MapInfo mapInfo, Objective obj, Feature objectiveFeature, bool avoidCorridors, bool includeVaults)
        {
            var toRet = new List<Tuple<int, RogueBasin.Point>>();

            var roomsForClue = GetAllWalkablePointsToPlaceObjective(mapInfo, obj, avoidCorridors, includeVaults);
            var levelForRandomRoom = roomsForClue.Item1;
            var allWalkablePoints = roomsForClue.Item2;

            bool placedItem = false;
            RogueBasin.Point pointToPlace = null;
            foreach (RogueBasin.Point p in allWalkablePoints)
            {
                if (objectiveFeature == null)
                    objectiveFeature = new RogueBasin.Features.SimpleObjective(obj, mapInfo.Model.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(obj));
                placedItem = Game.Dungeon.AddFeature(objectiveFeature, levelForRandomRoom, p);

                pointToPlace = p;

                if (placedItem)
                    break;
            }

            placedObjectives.Add(obj);

            if (!placedItem)
            {
                var str = "Can't place obj " + obj.Id;
                LogFile.Log.LogEntryDebug(str, LogDebugLevel.High);
                throw new ApplicationException(str);
            }

            return new Tuple<int, RogueBasin.Point>(levelForRandomRoom, pointToPlace);
        }

        private Tuple<int, RogueBasin.Point> PlaceFeatureInRoom(MapInfo mapInfo, Feature objectiveFeature, List<int> candidateRooms, bool avoidCorridors, bool includeVaults)
        {
            var toRet = new List<Tuple<int, RogueBasin.Point>>();

            var roomsForClue = GetAllWalkablePointsInRooms(mapInfo, candidateRooms, avoidCorridors, includeVaults);
            var levelForRandomRoom = roomsForClue.Item1;
            var allWalkablePoints = roomsForClue.Item2;

            bool placedItem = false;
            RogueBasin.Point pointToPlace = null;
            foreach (RogueBasin.Point p in allWalkablePoints)
            {
                placedItem = Game.Dungeon.AddFeature(objectiveFeature, levelForRandomRoom, p);

                pointToPlace = p;

                if (placedItem)
                    break;
            }

            if (!placedItem)
            {
                var str = "Can't place obj ";
                LogFile.Log.LogEntryDebug(str, LogDebugLevel.High);
                throw new ApplicationException(str);
            }

            return new Tuple<int, RogueBasin.Point>(levelForRandomRoom, pointToPlace);
        }

        private LevelInfo GenerateMedicalLevel(int levelNo)
        {
            var medicalInfo = new LevelInfo(levelNo);
            
            //Load standard room types

            RoomTemplate deadEnd = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber3x3_1door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate medicalBay = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.medical_bay1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, medicalBay);

            int numberOfRandomRooms = 10;

            BuildTXShapedRoomsBig(templateGenerator, numberOfRandomRooms);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            AddStandardPlaceholderVaults(medicalInfo, templateGenerator, maxPlaceHolders);

            //Add extra corridors
            AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            //Wall type
            medicalInfo.TerrainMapping = irisTerrainMapping;

            return medicalInfo;
        }

        private LevelInfo GenerateScienceLevel(int levelNo, int startVertexIndex)
        {
            var levelInfo = new LevelInfo(levelNo);

            //Load standard room types
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            levelInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            levelInfo.LevelGenerator = templateGenerator;

            //Load sample templates
            RoomTemplate branchRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.branchroom.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate branchRoom2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.branchroom2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber1Doors = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber7x3_1door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber2Doors = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber7x3_2door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber1Doors2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber6x4_1door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber2Doors2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber6x4_2door.room", StandardTemplateMapping.terrainMapping);

            //Build a network of branched corridors

            //Place branch rooms to form the initial structure, joined on long axis
            PlaceOriginRoom(templateGenerator, branchRoom);

            PlaceRandomConnectedRooms(templateGenerator, 3, branchRoom, corridor1, 0, 0, () => Game.Random.Next(1) > 0 ? 3 : 4);
            PlaceRandomConnectedRooms(templateGenerator, 3, branchRoom2, corridor1, 0, 0, () => Game.Random.Next(1) > 0 ? 2 : 3);

            //Add some 2-door rooms
            var twoDoorDistribution = new List<Tuple<int, RoomTemplate>> {
                new Tuple<int, RoomTemplate>(2, chamber1Doors),
                new Tuple<int, RoomTemplate>(2, chamber1Doors2)
            };

            PlaceRandomConnectedRooms(templateGenerator, 10, twoDoorDistribution, corridor1, 0, 0);

            //Add some 1-door deadends

            var oneDoorDistribution = new List<Tuple<int, RoomTemplate>> {
                new Tuple<int, RoomTemplate>(2, chamber2Doors),
                new Tuple<int, RoomTemplate>(2, chamber2Doors2)
            };
            PlaceRandomConnectedRooms(templateGenerator, 10, oneDoorDistribution, corridor1, 0, 0);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, levelInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                levelInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            AddStandardPlaceholderVaults(levelInfo, templateGenerator, maxPlaceHolders);

            //Add extra corridors
            //AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            //Wall type
            levelInfo.TerrainMapping = lineTerrainMapping;

            return levelInfo;
        }

            private LevelInfo GenerateBridgeLevel(int levelNo, int startVertexIndex)
        {
            var levelInfo = new LevelInfo(levelNo);

            //Load standard room types
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            levelInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            levelInfo.LevelGenerator = templateGenerator;

            //Load sample templates
            RoomTemplate branchRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.branchroom.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate branchRoom2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.branchroom2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber1Doors = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber7x3_1door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber2Doors = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber7x3_2door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber1Doors2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber6x4_1door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate chamber2Doors2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.chamber6x4_2door.room", StandardTemplateMapping.terrainMapping);

            //Build a network of branched corridors

            //Place branch rooms to form the initial structure, joined on long axis
            PlaceOriginRoom(templateGenerator, branchRoom);

            PlaceRandomConnectedRooms(templateGenerator, 3, branchRoom, corridor1, 0, 0, () => Game.Random.Next(1) > 0 ? 3 : 4);
            PlaceRandomConnectedRooms(templateGenerator, 3, branchRoom2, corridor1, 0, 0, () => Game.Random.Next(1) > 0 ? 2 : 3);

            //Add some 2-door rooms
            var twoDoorDistribution = new List<Tuple<int, RoomTemplate>> {
                new Tuple<int, RoomTemplate>(2, chamber1Doors),
                new Tuple<int, RoomTemplate>(2, chamber1Doors2)
            };

            PlaceRandomConnectedRooms(templateGenerator, 10, twoDoorDistribution, corridor1, 0, 0);

            //Add some 1-door deadends

            var oneDoorDistribution = new List<Tuple<int, RoomTemplate>> {
                new Tuple<int, RoomTemplate>(2, chamber2Doors),
                new Tuple<int, RoomTemplate>(2, chamber2Doors2)
            };
            PlaceRandomConnectedRooms(templateGenerator, 10, oneDoorDistribution, corridor1, 0, 0);
            
            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, levelInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                levelInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            AddStandardPlaceholderVaults(levelInfo, templateGenerator, maxPlaceHolders);

            //Add extra corridors
            //AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            //Wall type
            levelInfo.TerrainMapping = lineTerrainMapping;

            return levelInfo;
        }

        private LevelInfo GenerateStorageLevel(int levelNo, int startVertexIndex)
        {
            var medicalInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate originRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, originRoom);

            int numberOfRandomRooms = 12;

            GenerateLargeRooms(templateGenerator, numberOfRandomRooms);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            AddStandardPlaceholderVaults(medicalInfo, templateGenerator, maxPlaceHolders);

            //Add extra corridors
            AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            return medicalInfo;
        }

        private LevelInfo GenerateFlightDeckLevel(int levelNo, int startVertexIndex)
        {
            var medicalInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate originRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate escapePodVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.escape_pod1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, originRoom);

            int numberOfRandomRooms = 12;

            GenerateLargeRooms(templateGenerator, numberOfRandomRooms);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add the escape pods
            escapePodsConnection = AddRoomToRandomOpenDoor(templateGenerator, escapePodVault, corridor1, 2);
            escapePodsLevel = levelNo;

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            AddStandardPlaceholderVaults(medicalInfo, templateGenerator, maxPlaceHolders);

            //Add extra corridors
            AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            medicalInfo.TerrainMapping = cutTerrainMapping;

            return medicalInfo;
        }

        private void AddStandardPlaceholderVaults(LevelInfo medicalInfo, TemplatedMapGenerator templateGenerator, int maxPlaceHolders)
        {
            RoomTemplate armory1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.armory1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate armory2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.armory2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate armory3 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.armory3.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            medicalInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(templateGenerator, corridor1, new List<RoomTemplate> { armory1, armory2, armory3 }, maxPlaceHolders));
        }

        Connection connectionFromReactorOriginRoom;

        private LevelInfo GenerateReactorLevel(int levelNo, int startVertexIndex)
        {
            var medicalInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate originRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.reactor1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate deadEnd = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way_1door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate largeRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way3.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, originRoom);

            connectionFromReactorOriginRoom = AddRoomToRandomOpenDoor(templateGenerator, largeRoom, corridor1, 2);

            int numberOfRandomRooms = 20;

            GenerateClosePackedSquareRooms(templateGenerator, numberOfRandomRooms);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            medicalInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(templateGenerator, corridor1, placeHolderVault, maxPlaceHolders));

            //If we have any more doors, add a couple of dead ends
            PlaceRandomConnectedRooms(templateGenerator, 3, deadEnd, corridor1, 0, 0);

            //Add extra corridors
            AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            //Wall type
            medicalInfo.TerrainMapping = securityTerrainMapping;

            return medicalInfo;
        }

        private LevelInfo GenerateComputerCoreLevel(int levelNo, int startVertexIndex)
        {
            var medicalInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate originRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.reactor1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate deadEnd = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way_1door.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate largeRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way3.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate largeRoom2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_2way3.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, originRoom);

            connectionFromReactorOriginRoom = AddRoomToRandomOpenDoor(templateGenerator, largeRoom, corridor1, 2);

            int numberOfRandomRooms = 20;

            GenerateClosePackedSquareRooms2(templateGenerator, numberOfRandomRooms);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            medicalInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(templateGenerator, corridor1, placeHolderVault, maxPlaceHolders));

            //If we have any more doors, add a couple of dead ends
            PlaceRandomConnectedRooms(templateGenerator, 3, deadEnd, corridor1, 0, 0);

            //Add extra corridors
            AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            //Wall type
            medicalInfo.TerrainMapping = panelTerrainMapping;

            return medicalInfo;
        }

        private LevelInfo GenerateArcologyLevel(int levelNo, int startVertexIndex)
        {
            var medicalInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate originRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_special1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate arcologyBig = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_vault_big1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate arcologySmall = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_vault_small1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate arcologyTiny = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_vault_tiny1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate arcologyOval = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_vault_oval1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_vault_small_deadend1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, originRoom);

            int numberOfRandomRooms = 12;

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>> { 
                new Tuple<int, RoomTemplate>(100, arcologyBig),
                new Tuple<int, RoomTemplate>(100, arcologySmall),
                new Tuple<int, RoomTemplate>(50, arcologyOval)};

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 2, 4);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            medicalInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(templateGenerator, corridor1, placeHolderVault, maxPlaceHolders));

            //If we have any more doors, add a couple of dead ends
            PlaceRandomConnectedRooms(templateGenerator, 3, arcologyTiny, corridor1, 0, 0);

            //Add extra corridors
            AddCorridorsBetweenOpenDoors(templateGenerator, 1, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);
            //Remove doors
            templateGenerator.ReplaceConnectedDoorsWithTerrain(RoomTemplateTerrain.Floor);

            //Wall type
            medicalInfo.TerrainMapping = bioTerrainMapping;

            return medicalInfo;


        }

        private LevelInfo GenerateCommercialLevel(int levelNo, int startVertexIndex)
        {
            var medicalInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate originRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.central_pillars1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate lshapeRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.lshape3.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate lshapeAsymmetric = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.lshape_asymmetric3.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate tshape = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.tshape2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate xshape = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.xshape2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate arcologyOval = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_vault_oval1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate arcologySmall = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.arcology_vault_small1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate armoryVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.armory1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, originRoom);

            int numberOfRandomRooms = 16;

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>> { 
                new Tuple<int, RoomTemplate>(250, originRoom),
                new Tuple<int, RoomTemplate>(100, tshape),
                new Tuple<int, RoomTemplate>(100, xshape),
                new Tuple<int, RoomTemplate>(100, arcologySmall),
                new Tuple<int, RoomTemplate>(50, arcologyOval)};

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 2, 4);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            AddStandardPlaceholderVaults(medicalInfo, templateGenerator, maxPlaceHolders);

            //If we have any more doors, add a couple of dead ends
            PlaceRandomConnectedRooms(templateGenerator, 3, armoryVault, corridor1, 0, 0);

            //Add extra corridors
            AddCorridorsBetweenOpenDoors(templateGenerator, 1, new List<RoomTemplate> { corridor1 });

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);
            //Remove doors
            templateGenerator.ReplaceConnectedDoorsWithTerrain(RoomTemplateTerrain.Floor);

            //Wall type
            medicalInfo.TerrainMapping = dipTerrainMapping;

            return medicalInfo;
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

        private void UseVault(Dictionary<int, LevelInfo> levelInfo, Connection vaultConnection) {
            var levelForVault = gameLevels.Where(l => levelInfo[l].ReplaceableVaultConnections.Contains(vaultConnection)).First();

            levelInfo[levelForVault].ReplaceableVaultConnectionsUsed.Add(vaultConnection);
        }

        private void GenerateLargeRooms(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate largeRoom1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.largeconnectingvault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate largeRoom2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.largeconnectingvault2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);
            
            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>>();

            for (int i = 0; i < 10; i++)
            {
                allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(2, RoomTemplateUtilities.BuildRandomRectangularRoom(6, 14, 6, 14, 4, 4)));
            }

            allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom1));
            allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom2));

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 0, 0);
        }

        private void GenerateClosePackedSquareRooms(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate largeRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way3.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate smallRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate tinyRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>>();

            //allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom1));
            //allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom2));

            int numberOfLargeRooms = (int)Math.Ceiling(numberOfRandomRooms / 2.0);
            int numberOfMediumRooms = (int)Math.Ceiling(numberOfRandomRooms / 6.0);
            int numberOfSmallRooms = numberOfRandomRooms - numberOfLargeRooms - numberOfMediumRooms;

            PlaceRandomConnectedRooms(templateGenerator, numberOfLargeRooms, largeRoom, corridor1, 0, 0);
            PlaceRandomConnectedRooms(templateGenerator, numberOfMediumRooms, smallRoom, corridor1, 0, 0);
            PlaceRandomConnectedRooms(templateGenerator, numberOfSmallRooms, tinyRoom, corridor1, 0, 0);
        }

        private void GenerateClosePackedSquareRooms2(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate largeRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way3.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate largeRoom2 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_2way3.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate smallRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate tinyRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.square_4way.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>>();

            //allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom1));
            //allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom2));

            int numberOfLargeRooms = (int)Math.Ceiling(numberOfRandomRooms / 2.0);
            int numberOfMediumRooms = (int)Math.Ceiling(numberOfRandomRooms / 6.0);
            int numberOfSmallRooms = numberOfRandomRooms - numberOfLargeRooms - numberOfMediumRooms;

            PlaceRandomConnectedRooms(templateGenerator, numberOfLargeRooms, largeRoom, corridor1, 0, 0);
            PlaceRandomConnectedRooms(templateGenerator, numberOfLargeRooms, largeRoom2, corridor1, 0, 0);
            PlaceRandomConnectedRooms(templateGenerator, numberOfMediumRooms, smallRoom, corridor1, 0, 0);
            PlaceRandomConnectedRooms(templateGenerator, numberOfSmallRooms, tinyRoom, corridor1, 0, 0);
        }

        private void BuildTXShapedRooms(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate lshapeRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.lshape2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate lshapeAsymmetric = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.lshape_asymmetric2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate tshape = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.tshape1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate xshape = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.xshape1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>> { 
                new Tuple<int, RoomTemplate>(100, lshapeRoom),
                new Tuple<int, RoomTemplate>(100, lshapeAsymmetric),
                new Tuple<int, RoomTemplate>(100, tshape),
                new Tuple<int, RoomTemplate>(100, xshape) };

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 4, 6);
        }

        private void BuildTXShapedRoomsBig(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate lshapeRoom = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.lshape3.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate lshapeAsymmetric = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.lshape_asymmetric3.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate tshape = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.tshape2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate xshape = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.xshape2.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>> { 
                new Tuple<int, RoomTemplate>(100, lshapeRoom),
                new Tuple<int, RoomTemplate>(100, lshapeAsymmetric),
                new Tuple<int, RoomTemplate>(100, tshape),
                new Tuple<int, RoomTemplate>(100, xshape) };

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 4, 6);
        }

        private LevelInfo GenerateStandardLevel(int levelNo, int startVertexIndex)
        {
            var medicalInfo = new LevelInfo(levelNo);

            //Load standard room types

            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            RoomTemplate replacementVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate placeHolderVault = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping);

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = templateGenerator;

            PlaceOriginRoom(templateGenerator, room1);
            PlaceRandomConnectedRooms(templateGenerator, 4, room1, corridor1, 5, 10);

            //Add connections to other levels

            var connections = AddConnectionsToOtherLevels(levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            medicalInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(templateGenerator, corridor1, placeHolderVault, maxPlaceHolders));

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            return medicalInfo;
        }

        private List<Tuple<int, Connection>> AddConnectionsToOtherLevels(int levelNo, LevelInfo medicalInfo, RoomTemplate corridor1, RoomTemplate elevatorVault, TemplatedMapGenerator templateGenerator)
        {
            var otherLevelConnections = LevelLinks.GetAllConnections().Where(c => c.IncludesVertex(levelNo)).Select(c => c.Source == levelNo ? c.Target : c.Source);
            var connectionsToReturn = new List<Tuple<int, Connection>>();

            foreach (var otherLevel in otherLevelConnections)
            {
                var connectingRoom = AddRoomToRandomOpenDoor(templateGenerator, elevatorVault, corridor1, 3);
                connectionsToReturn.Add(new Tuple<int, Connection>(otherLevel, connectingRoom));
            }

            return connectionsToReturn;
        }

        private List<Connection> AddReplaceableVaults(TemplatedMapGenerator templateGenerator, RoomTemplate corridor1, RoomTemplate placeHolderVault, int maxPlaceHolders)
        {
            return AddReplaceableVaults(templateGenerator, corridor1, new List<RoomTemplate> {placeHolderVault}, maxPlaceHolders);
        }

        private List<Connection> AddReplaceableVaults(TemplatedMapGenerator templateGenerator, RoomTemplate corridor1, List<RoomTemplate> placeHolderVault, int maxPlaceHolders)
        {
            var vaultsToReturn = new List<Connection>();
            int cargoTotalPlaceHolders = 0;
            do
            {
                var placeHolderRoom = AddRoomToRandomOpenDoor(templateGenerator, placeHolderVault.RandomElement(), corridor1, 3);
                if (placeHolderRoom != null)
                {
                    vaultsToReturn.Add(placeHolderRoom);
                    cargoTotalPlaceHolders++;
                }
                else
                    break;
            } while (cargoTotalPlaceHolders < maxPlaceHolders);
            return vaultsToReturn;
        }

        private LevelInfo GenerateLowerAtriumLevel(int levelNo)
        {
            var medicalInfo = new LevelInfo(levelNo);

            return medicalInfo;
        }

        private static void AddFeaturesToRoom<T>(int level, TemplatePositioned positionedRoom, int featuresToPlace) where T: Feature, new()
        {
            var bridgeRouter = new RoomFilling(positionedRoom.Room);

            var floorPoints = RoomTemplateUtilities.GetPointsInRoomWithTerrain(positionedRoom.Room, RoomTemplateTerrain.Floor);

            if (floorPoints.Count() == 0)
                return;

            for (int i = 0; i < featuresToPlace; i++)
            {
                var randomPoint = floorPoints.RandomElement();
                floorPoints.Remove(randomPoint);

                if (bridgeRouter.SetSquareUnWalkableIfMaintainsConnectivity(randomPoint))
                {
                    var featureLocationInMapCoords = positionedRoom.Location + randomPoint;
                    Game.Dungeon.AddFeatureBlocking(new T(), level, featureLocationInMapCoords, false);

                    LogFile.Log.LogEntryDebug("Placing feature in room " + positionedRoom.RoomIndex + " at location " + featureLocationInMapCoords, LogDebugLevel.Medium);
                }

                if (floorPoints.Count() == 0)
                    break;
            }
        }

        private void AddStandardDecorativeFeaturesToRoom(int level, TemplatePositioned positionedRoom, int featuresToPlace, IEnumerable<Tuple<int, DecorationFeatureDetails.Decoration>> decorationDetails, bool useBoundary)
        {
            //This is probably rather slow
            var roomFiller = new RoomFilling(positionedRoom.Room);

            AddExistingBlockingFeaturesToRoomFiller(level, positionedRoom, roomFiller);

            var floorPoints = new List<RogueBasin.Point>();
            if(!useBoundary)
                floorPoints = RoomTemplateUtilities.GetPointsInRoomWithTerrain(positionedRoom.Room, RoomTemplateTerrain.Floor);
            else
                floorPoints = RoomTemplateUtilities.GetBoundaryFloorPointsInRoom(positionedRoom.Room);

            if (floorPoints.Count() == 0)
                return;

            for (int i = 0; i < featuresToPlace; i++)
            {
                var randomPoint = floorPoints.RandomElement();
                floorPoints.Remove(randomPoint);

                var featureToPlace = ChooseItemFromWeights<DecorationFeatureDetails.Decoration>(decorationDetails);
                var featureLocationInMapCoords = positionedRoom.Location + randomPoint;

                if (!featureToPlace.isBlocking)
                {
                    Game.Dungeon.AddFeature(new RogueBasin.Features.StandardDecorativeFeature(featureToPlace.representation, featureToPlace.colour), level, featureLocationInMapCoords);

                    LogFile.Log.LogEntryDebug("Placing feature in room " + positionedRoom.RoomIndex + " at location " + featureLocationInMapCoords, LogDebugLevel.Medium);
                }
                else if (roomFiller.SetSquareUnWalkableIfMaintainsConnectivity(randomPoint))
                {
                    Game.Dungeon.AddFeatureBlocking(new RogueBasin.Features.StandardDecorativeFeature(featureToPlace.representation, featureToPlace.colour), level, featureLocationInMapCoords, false);

                    LogFile.Log.LogEntryDebug("Placing feature in room " + positionedRoom.RoomIndex + " size: w:" + positionedRoom.Room.Width + " h:" + positionedRoom.Room.Height + " at location " + featureLocationInMapCoords + " (rel " + randomPoint + ")", LogDebugLevel.Medium);
                }

                if (floorPoints.Count() == 0)
                    break;
            }
        }

        private void AddStandardDecorativeFeaturesToRoomUsingGrid(int level, TemplatePositioned positionedRoom, int featuresToPlace, IEnumerable<Tuple<int, DecorationFeatureDetails.Decoration>> decorationDetails)
        {
            var roomFiller = new RoomFilling(positionedRoom.Room);

            //Need to account for all current blocking features in room
            AddExistingBlockingFeaturesToRoomFiller(level, positionedRoom, roomFiller);

            var floorPoints = RoomTemplateUtilities.GetGridFromRoom(positionedRoom.Room, 2, 1, 0.5);

            if (floorPoints.Count() == 0)
                return;

            for (int i = 0; i < featuresToPlace; i++)
            {
                var randomPoint = floorPoints.RandomElement();
                floorPoints.Remove(randomPoint);

                var featureToPlace = ChooseItemFromWeights<DecorationFeatureDetails.Decoration>(decorationDetails);
                var featureLocationInMapCoords = positionedRoom.Location + randomPoint;

                if (!featureToPlace.isBlocking)
                {
                    Game.Dungeon.AddFeature(new RogueBasin.Features.StandardDecorativeFeature(featureToPlace.representation, featureToPlace.colour), level, featureLocationInMapCoords);

                    LogFile.Log.LogEntryDebug("Placing feature in room " + positionedRoom.RoomIndex + " at location " + featureLocationInMapCoords, LogDebugLevel.Medium);
                }
                else if (roomFiller.SetSquareUnWalkableIfMaintainsConnectivity(randomPoint))
                {
                    Game.Dungeon.AddFeatureBlocking(new RogueBasin.Features.StandardDecorativeFeature(featureToPlace.representation, featureToPlace.colour), level, featureLocationInMapCoords, false);

                    LogFile.Log.LogEntryDebug("Placing blocking feature in room " + positionedRoom.RoomIndex + " at location " + featureLocationInMapCoords, LogDebugLevel.Medium);
                }

                if (floorPoints.Count() == 0)
                    break;
            }
        }

        private static void AddExistingBlockingFeaturesToRoomFiller(int level, TemplatePositioned positionedRoom, RoomFilling bridgeRouter)
        {
            var floorPointsInRoom = RoomTemplateUtilities.GetPointsInRoomWithTerrain(positionedRoom.Room, RoomTemplateTerrain.Floor).Select(p => p + positionedRoom.Location);
            foreach (var roomPoint in floorPointsInRoom)
            {
                if (Game.Dungeon.BlockingFeatureAtLocation(level, roomPoint))
                {
                    var stillWalkable = bridgeRouter.SetSquareUnWalkableIfMaintainsConnectivity(roomPoint - positionedRoom.Location);

                    if (!stillWalkable)
                    {
                        LogFile.Log.LogEntryDebug("Room " + positionedRoom.RoomIndex + " appears unconnected.", LogDebugLevel.High);
                    }
                }
            }
        }

        T RandomItem<T>(IEnumerable<T> items)
        {
            var totalItems = items.Count();
            if (totalItems == 0)
                throw new ApplicationException("Empty list for randomization");

            return items.ElementAt(Game.Random.Next(totalItems));
        }

        Connection AddRoomToRandomOpenDoor(TemplatedMapGenerator gen, RoomTemplate templateToPlace, RoomTemplate corridorTemplate, int distanceFromDoor)
        {
            var doorsToTry = gen.PotentialDoors.Shuffle();
            
            foreach(var door in doorsToTry) {
                try {
                    return gen.PlaceRoomTemplateAlignedWithExistingDoor(templateToPlace, corridorTemplate, RandomDoor(gen), 0, distanceFromDoor);
                }
                catch (ApplicationException)
                {
                    //No good, continue
                }
            }

            throw new ApplicationException("No applicable doors left");
        }


        private void AddCorridorsBetweenOpenDoors(TemplatedMapGenerator templatedGenerator, int totalExtraConnections, List<RoomTemplate> corridorsToUse)
        {
            var extraConnections = 0;

            var allDoors = templatedGenerator.PotentialDoors;

            //Find all possible doors matches that aren't in the same room
            var allBendDoorPossibilities = from d1 in allDoors
                                           from d2 in allDoors
                                           where RoomTemplateUtilities.CanBeConnectedWithBendCorridor(d1.MapCoords, d1.DoorLocation, d2.MapCoords, d2.DoorLocation)
                                                 && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                           select new { origin = d1, target = d2 };

            var allLDoorPossibilities = from d1 in allDoors
                                        from d2 in allDoors
                                        where RoomTemplateUtilities.CanBeConnectedWithLShapedCorridor(d1.MapCoords, d1.DoorLocation, d2.MapCoords, d2.DoorLocation)
                                              && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                        select new { origin = d1, target = d2 };

            var allStraightDoorPossibilities = from d1 in allDoors
                                        from d2 in allDoors
                                        where RoomTemplateUtilities.CanBeConnectedWithStraightCorridor(d1.MapCoords, d1.DoorLocation, d2.MapCoords, d2.DoorLocation)
                                              && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                        select new { origin = d1, target = d2 };

            var allOverlappingDoorPossibilities = from d1 in allDoors
                                                  from d2 in allDoors
                                                  where d1.MapCoords == d2.MapCoords
                                                        && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                                  select new { origin = d1, target = d2 };


            //Materialize for speed

            var allMatchingDoorPossibilities = allBendDoorPossibilities.Union(allLDoorPossibilities).Union(allOverlappingDoorPossibilities).Union(allStraightDoorPossibilities).ToList();
            //var allMatchingDoorPossibilities = allLDoorPossibilities;
            //var allMatchingDoorPossibilities = allBendDoorPossibilities;

            var shuffleMatchingDoors = allMatchingDoorPossibilities.Shuffle(Game.Random);

            for (int i = 0; i < allMatchingDoorPossibilities.Count; i++)
            {
                //Try a random combination to see if it works
                var doorsToTry = shuffleMatchingDoors.ElementAt(i);

                LogFile.Log.LogEntryDebug("Trying door " + doorsToTry.origin.MapCoords + " to " + doorsToTry.target.MapCoords, LogDebugLevel.Medium);

                bool success = templatedGenerator.JoinDoorsWithCorridor(doorsToTry.origin, doorsToTry.target, corridorsToUse.RandomElement());
                if (success)
                    extraConnections++;

                if (extraConnections > totalExtraConnections)
                    break;
            }

            //Previous code (was super-slow!)
            //while (allMatchingDoorPossibilities.Any() && extraConnections < totalExtraConnections)
            //In any case, remove this attempt
            //var doorsToTry = allMatchingDoorPossibilities.ElementAt(Game.Random.Next(allMatchingDoorPossibilities.Count()));
            //allMatchingDoorPossibilities = allMatchingDoorPossibilities.Except(Enumerable.Repeat(doorsToTry, 1)); //order n - making it slow?

        }

        private int PlaceOriginRoom(TemplatedMapGenerator templatedGenerator, RoomTemplate roomToPlace)
        {
            return templatedGenerator.PlaceRoomTemplateAtPosition(roomToPlace, new RogueBasin.Point(0, 0));
        }

        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, RoomTemplate roomToPlace, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength)
        {
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, roomToPlace, corridorToPlace, minCorridorLength, maxCorridorLength, null);
        }
        
        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, List<RoomTemplate> roomToPlaces, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength)
        {
            var tuples = roomToPlaces.Select(r => new Tuple<int, RoomTemplate>(1, r));
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, tuples, corridorToPlace, minCorridorLength, maxCorridorLength, null);
        }

        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, List<Tuple<int,RoomTemplate>> roomToPlaceWithWeights, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength)
        {
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, roomToPlaceWithWeights, corridorToPlace, minCorridorLength, maxCorridorLength, null);
        }


        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, RoomTemplate roomToPlace, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength, Func<int> doorPicker)
        {
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, new List<Tuple<int,RoomTemplate>> { new Tuple<int, RoomTemplate>(1, roomToPlace) }, corridorToPlace, minCorridorLength, maxCorridorLength, doorPicker);
        }

        private T ChooseItemFromWeights<T>(IEnumerable<Tuple<int, T>> itemsWithWeights)
        {
            var totalWeight = itemsWithWeights.Select(t => t.Item1).Sum();
            var randomNumber = Game.Random.Next(totalWeight);

            int weightSoFar = 0;
            T roomToPlace = itemsWithWeights.First().Item2;
            foreach (var t in itemsWithWeights)
            {
                weightSoFar += t.Item1;
                if (weightSoFar > randomNumber)
                {
                    roomToPlace = t.Item2;
                    break;
                }
            }

            return roomToPlace;
        }

        /// <summary>
        /// Failure mode is placing fewer rooms than requested
        /// </summary>
        private int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, IEnumerable<Tuple<int, RoomTemplate>> roomsToPlaceWithWeights, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength, Func<int> doorPicker)
        {
            int roomsPlaced = 0;
            int attempts = 0;

            //This uses random distances and their might be collisions so we should avoid infinite loops
            int maxAttempts = roomsToPlace * 5;

            //Terminate when all rooms placed or no more potential door sites
            do
            {
                //Find a random potential door and try to grow a random room off this
                
                //Find room using weights
                var roomToPlace = ChooseItemFromWeights<RoomTemplate>(roomsToPlaceWithWeights);

                //Use a random door, or the function passed in
                int randomNewDoorIndex;
                if (doorPicker == null)
                {
                    //Random door
                    randomNewDoorIndex = Game.Random.Next(roomToPlace.PotentialDoors.Count);
                }
                else
                    randomNewDoorIndex = doorPicker();

                int corridorLength = Game.Random.Next(minCorridorLength, maxCorridorLength);

                try
                {
                    templatedGenerator.PlaceRoomTemplateAlignedWithExistingDoor(roomToPlace, corridorToPlace, RandomDoor(templatedGenerator), randomNewDoorIndex,
                    corridorLength);

                    roomsPlaced++;
                }
                catch (ApplicationException) { }

                attempts++;

            } while (roomsPlaced < roomsToPlace && attempts < maxAttempts && templatedGenerator.HaveRemainingPotentialDoors());
            return roomsPlaced;
        }

        /** Build a map using templated rooms */
        public MapInfo GenerateTestGraphicsDungeon()
        {

            //Load standard room types
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.largetestvault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            //Build level 1

            var l1mapBuilder = new TemplatedMapBuilder(100, 100);
            var l1templateGenerator = new TemplatedMapGenerator(l1mapBuilder);

            PlaceOriginRoom(l1templateGenerator, room1);
            PlaceRandomConnectedRooms(l1templateGenerator, 1, room1, corridor1, 0, 0, () => 0);

            //Build the graph containing all the levels

            //Build and add the l1 map

            var mapInfoBuilder = new MapInfoBuilder();
            var startRoom = 0;
            mapInfoBuilder.AddConstructedLevel(0, l1templateGenerator.ConnectivityMap, l1templateGenerator.GetRoomTemplatesInWorldCoords(), l1templateGenerator.GetDoorsInMapCoords(), startRoom);

            MapInfo mapInfo = new MapInfo(mapInfoBuilder);

            //Add maps to the dungeon

            Map masterMap = l1mapBuilder.MergeTemplatesIntoMap(terrainMapping);
            Game.Dungeon.AddMap(masterMap);

            //Recalculate walkable to allow placing objects
            Game.Dungeon.RefreshAllLevelPathingAndFOV();

            //Set player's start location (must be done before adding items)

            //Set PC start location

            var firstRoom = mapInfo.GetRoom(0);
            masterMap.PCStartLocation = new RogueBasin.Point(firstRoom.X + firstRoom.Room.Width / 2, firstRoom.Y + firstRoom.Room.Height / 2);

            //Add items
            var dungeon = Game.Dungeon;

            dungeon.AddItem(new RogueBasin.Items.Pistol(), 0, new RogueBasin.Point(1, 1));
            dungeon.AddItem(new RogueBasin.Items.Shotgun(), 0, new RogueBasin.Point(2, 1));
            dungeon.AddItem(new RogueBasin.Items.Laser(), 0, new RogueBasin.Point(3, 1));
            dungeon.AddItem(new RogueBasin.Items.Vibroblade(), 0, new RogueBasin.Point(4, 1));

            //Set map for visualisation
            return mapInfo;
        }

        public static KeyValuePair<int, int> MaxEntry(Dictionary<int, int> dict)
        {
            return dict.Aggregate((a, b) => a.Value > b.Value ? a : b);
        }
    }
}
