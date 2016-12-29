using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL
{

    public class TraumaLevelBuilder
    {
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

        private IEnumerable<int> gameLevels;
        private bool quickLevelGen;
        private ConnectivityMap levelLinks;

        List<int> allReplaceableVaults;
        Dictionary<int, LevelInfo> levelInfo;

        //Quest important rooms / vaults
        Connection escapePodsConnection;

        //Wall mappings
        Dictionary<MapTerrain, List<MapTerrain>> brickTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> panelTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> securityTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> irisTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> bioTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> lineTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> dipTerrainMapping;
        Dictionary<MapTerrain, List<MapTerrain>> cutTerrainMapping;

        public TraumaLevelBuilder(IEnumerable<int> gameLevels, ConnectivityMap levelLinks, bool quickLevelGen)
        {
            this.gameLevels = gameLevels;
            this.quickLevelGen = quickLevelGen;
            this.levelLinks = levelLinks;

            BuildTerrainMapping();
        }

        public IEnumerable<int> AllReplaceableVaults
        {
            get
            {
                return allReplaceableVaults;
            }
        }

        public Dictionary<int, LevelInfo> LevelInfo
        {
            get
            {
                return levelInfo;
            }
        }

        public Connection EscapePodsConnection
        {
            get
            {
                return escapePodsConnection;
            }
        }


        private void BuildTerrainMapping()
        {
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

        public void GenerateLevels()
        {
            levelInfo = new Dictionary<int, LevelInfo>();

            var levelBuilderUtils = new LevelBuilderUtils();

            var medicalLevelBuilder = new Levels.MedicalLevelBuilder(levelBuilderUtils, levelLinks, 0, irisTerrainMapping, "medical", "Medical");
            var medicalInfo = medicalLevelBuilder.GenerateLevel(medicalLevel);
            levelInfo[medicalLevel] = medicalInfo;

            if (!quickLevelGen)
            {
                var lowerAtriumBuilder = new Levels.StandardLevelBuilder(levelBuilderUtils, levelLinks, lowerAtriumLevel * 100, irisTerrainMapping, "lowerAtrium", "Lower Atrium");
                var lowerAtriumInfo = lowerAtriumBuilder.GenerateLevel(lowerAtriumLevel);
                levelInfo[lowerAtriumLevel] = lowerAtriumInfo;

                var scienceLevelBuilder = new Levels.ScienceLevelBuilder(levelBuilderUtils, levelLinks, scienceLevel * 100, lineTerrainMapping, "science", "Science");
                var scienceInfo = scienceLevelBuilder.GenerateLevel(scienceLevel);
                levelInfo[scienceLevel] = scienceInfo;

                var bridgeLevelBuilder = new Levels.BridgeLevelBuilder(levelBuilderUtils, levelLinks, bridgeLevel * 100, lineTerrainMapping, "bridge", "Bridge");
                var bridgeInfo = bridgeLevelBuilder.GenerateLevel(bridgeLevel);
                levelInfo[bridgeLevel] = bridgeInfo;

                var storageLevelBuilder = new Levels.StorageLevelBuilder(levelBuilderUtils, levelLinks, storageLevel * 100, irisTerrainMapping, "storage", "Storage");
                var storageInfo = storageLevelBuilder.GenerateLevel(storageLevel);
                levelInfo[storageLevel] = storageInfo;

                var flightDeckBuilder = new Levels.FlightDeckLevelBuilder(levelBuilderUtils, levelLinks, flightDeck * 100, cutTerrainMapping, "flightDeck", "Flight Deck");
                var flightInfo = flightDeckBuilder.GenerateLevel(flightDeck);
                levelInfo[flightDeck] = flightInfo;
                escapePodsConnection = flightDeckBuilder.GetEscapePodsConnection();

                var reactorLevelBuilder = new Levels.ReactorLevelBuilder(levelBuilderUtils, levelLinks, reactorLevel * 100, securityTerrainMapping, "reactor", "Reactor");
                var reactorInfo = reactorLevelBuilder.GenerateLevel(reactorLevel);
                levelInfo[reactorLevel] = reactorInfo;

                var computerCoreLevelBuilder = new Levels.ComputerCoreLevelBuilder(levelBuilderUtils, levelLinks, computerCoreLevel * 100, panelTerrainMapping, "computerCore", "Computer Core");
                var computerInfo = computerCoreLevelBuilder.GenerateLevel(computerCoreLevel);
                levelInfo[computerCoreLevel] = computerInfo;

                var arcologyLevelBuilder = new Levels.ArcologyLevelBuilder(levelBuilderUtils, levelLinks, arcologyLevel * 100, bioTerrainMapping, "arcology", "Arcology");
                var arcologyInfo = arcologyLevelBuilder.GenerateLevel(arcologyLevel);
                levelInfo[arcologyLevel] = arcologyInfo;

                var commercialLevelBuilder = new Levels.CommercialLevelBuilder(levelBuilderUtils, levelLinks, commercialLevel * 100, dipTerrainMapping, "commercial", "Commercial");
                var commercialInfo = commercialLevelBuilder.GenerateLevel(commercialLevel);
                levelInfo[commercialLevel] = commercialInfo;
            }
            //Make other levels generically

            IEnumerable<int> standardGameLevels = GetStandardGameLevelNos();

            foreach (var level in standardGameLevels)
            {
                var standardLevelBuilder = new Levels.StandardLevelBuilder(levelBuilderUtils, levelLinks, level * 100, irisTerrainMapping, "standard" + level, "Standard " + level);
                var thisLevelInfo = standardLevelBuilder.GenerateLevel(level);
                levelInfo[level] = thisLevelInfo;
            }

            //Maintain a list of the replaceable vaults. We don't want to put stuff in these as they may disappear
            allReplaceableVaults = levelInfo.SelectMany(kv => kv.Value.ReplaceableVaultConnections.Select(v => v.Target)).ToList();
        }

        public void CompleteLevels()
        {
            //Refactor - these should live in the levelbuilder classes

            ReplaceUnconnectedDoorsWithWalls(medicalLevel);

            if (!quickLevelGen)
            {
                ReplaceUnconnectedDoorsWithWalls(lowerAtriumLevel);

                ReplaceUnconnectedDoorsWithWalls(scienceLevel);

                ReplaceUnconnectedDoorsWithWalls(bridgeLevel);

                ReplaceUnconnectedDoorsWithWalls(storageLevel);

                ReplaceUnconnectedDoorsWithWalls(flightDeck);

                ReplaceUnconnectedDoorsWithWalls(reactorLevel);

                ReplaceUnconnectedDoorsWithWalls(computerCoreLevel);

                ReplaceAllsDoorsWithFloor(arcologyLevel);

                ReplaceAllsDoorsWithFloor(commercialLevel);
            }

            IEnumerable<int> standardGameLevels = GetStandardGameLevelNos();

            foreach (var level in standardGameLevels)
            {
                ReplaceUnconnectedDoorsWithWalls(level);
            }
        }

        private IEnumerable<int> GetStandardGameLevelNos()
        {
            IEnumerable<int> standardGameLevels;

            if (quickLevelGen)
            {
                standardGameLevels = gameLevels.Except(new List<int> { medicalLevel });
            }
            else
            {
                standardGameLevels = gameLevels.Except(new List<int> { medicalLevel, storageLevel, reactorLevel, flightDeck, arcologyLevel, scienceLevel, computerCoreLevel, bridgeLevel, commercialLevel, lowerAtriumLevel });
            }
            return standardGameLevels;
        }



        private void ReplaceAllsDoorsWithFloor(int levelNo)
        {
            levelInfo[levelNo].LevelGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);
            //Remove doors
            levelInfo[levelNo].LevelGenerator.ReplaceConnectedDoorsWithTerrain(RoomTemplateTerrain.Floor);
        }

        private void ReplaceUnconnectedDoorsWithWalls(int levelNo)
        {
            levelInfo[levelNo].LevelGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);
        }


    }
}
