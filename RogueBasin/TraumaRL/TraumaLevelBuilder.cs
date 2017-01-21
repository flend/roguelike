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
        private Dictionary<int, LevelBuilder> levelBuilders = new Dictionary<int, LevelBuilder>();

        private readonly LevelRegister levelRegister;
        private bool quickLevelGen;
        private readonly ConnectivityMap levelLinks;
        private readonly int startLevel;
        private int startVertex;

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

        public TraumaLevelBuilder(LevelRegister levelRegister, ConnectivityMap levelLinks, int startLevel, bool quickLevelGen)
        {
            this.levelRegister = levelRegister;
            this.quickLevelGen = quickLevelGen;
            this.levelLinks = levelLinks;
            this.startLevel = startLevel;

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

        public int StartVertex
        {
            get
            {
                return startVertex;
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
            var allRegisteredLevelData = levelRegister.GetAllRegisteredLevelIdData();

            levelInfo = new Dictionary<int, LevelInfo>();
            var levelBuilderUtils = new LevelBuilderUtils();

            foreach (var levelData in allRegisteredLevelData)
            {
                var levelId = levelData.id;
                var levelName = levelData.name;
                var levelReadableName = levelData.readableName;
                var levelType = levelData.type;

                switch (levelType)
                {
                    case LevelType.ArcologyLevel:
                        levelBuilders[levelId] = new Levels.ArcologyLevelBuilder(levelBuilderUtils, levelLinks, levelId * 100, bioTerrainMapping, levelName, levelReadableName);
                        break;

                    case LevelType.BridgeLevel:
                        levelBuilders[levelId] = new Levels.BridgeLevelBuilder(levelBuilderUtils, levelLinks, levelId * 100, lineTerrainMapping, levelName, levelReadableName);
                        break;

                    case LevelType.CommercialLevel:
                        levelBuilders[levelId] = new Levels.CommercialLevelBuilder(levelBuilderUtils, levelLinks, levelId * 100, dipTerrainMapping, levelName, levelReadableName);
                        break;

                    case LevelType.ComputerCoreLevel:
                        levelBuilders[levelId] = new Levels.ComputerCoreLevelBuilder(levelBuilderUtils, levelLinks, levelId * 100, panelTerrainMapping, levelName, levelReadableName);
                        break;

                    case LevelType.FlightDeck:
                        levelBuilders[levelId] = new Levels.FlightDeckLevelBuilder(levelBuilderUtils, levelLinks, levelId * 100, cutTerrainMapping, levelName, levelReadableName);
                        break;

                    case LevelType.LowerAtriumLevel:
                        levelBuilders[levelId] = new Levels.StandardLevelBuilder(levelBuilderUtils, levelLinks, levelId * 100, irisTerrainMapping, LevelType.LowerAtriumLevel, levelName, levelReadableName);
                        break;

                    case LevelType.MedicalLevel:
                        levelBuilders[levelId] = new Levels.MedicalLevelBuilder(levelBuilderUtils, levelLinks, levelId * 100, irisTerrainMapping, levelName, levelReadableName);
                        break;

                    case LevelType.ReactorLevel:
                        levelBuilders[levelId] = new Levels.ReactorLevelBuilder(levelBuilderUtils, levelLinks, levelId * 100, securityTerrainMapping, levelName, levelReadableName);
                        break;

                    case LevelType.ScienceLevel:
                        levelBuilders[levelId] = new Levels.ScienceLevelBuilder(levelBuilderUtils, levelLinks, levelId * 100, lineTerrainMapping, levelName, levelReadableName);
                        break;

                    case LevelType.StorageLevel:
                        levelBuilders[levelId] = new Levels.StorageLevelBuilder(levelBuilderUtils, levelLinks, levelId * 100, irisTerrainMapping, levelName, levelReadableName);
                        break;

                }

                levelInfo[levelId] = levelBuilders[levelId].GenerateLevel(levelId);

                //This should be moved into the quest now we have dynamic map generation
                if (levelType == LevelType.FlightDeck)
                {
                    escapePodsConnection = ((Levels.FlightDeckLevelBuilder)levelBuilders[levelId]).GetEscapePodsConnection();
                }
            }

            startVertex = levelInfo[startLevel].StartVertex;
            
            //Maintain a list of the replaceable vaults. We don't want to put stuff in these as they may disappear
            allReplaceableVaults = levelInfo.SelectMany(kv => kv.Value.ReplaceableVaultConnections.Select(v => v.Target)).ToList();
        }

        public void CompleteLevels()
        {
            foreach(var levelBuilder in levelBuilders) {
                levelBuilder.Value.CompleteLevel();
            }
        }
    }
}
