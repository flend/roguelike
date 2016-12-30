using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL.Levels
{
    class CommercialLevelBuilder : LevelBuilder
    {
        private readonly LevelBuilderUtils utils;
        private readonly Dictionary<MapTerrain, List<MapTerrain>> terrainMapping;
        private readonly ConnectivityMap levelLinks;
        private readonly int startVertexIndex;
        private readonly string levelName;
        private readonly string levelReadableName;

        private LevelInfo levelInfo;

        public CommercialLevelBuilder(LevelBuilderUtils utils, ConnectivityMap levelLinks, int startVertexIndex, Dictionary<MapTerrain, List<MapTerrain>> terrainMapping,
            string levelName, string levelReadableName)
        {
            this.utils = utils;
            this.terrainMapping = terrainMapping;
            this.levelLinks = levelLinks;
            this.startVertexIndex = startVertexIndex;
            this.levelName = levelName;
            this.levelReadableName = levelReadableName;
        }

        public override LevelInfo GenerateLevel(int levelNo)
        {
            levelInfo = new LevelInfo(levelNo, levelName, levelReadableName);

            //Load standard room types

            RoomTemplate originRoom = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.central_pillars1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate lshapeRoom = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.lshape3.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate lshapeAsymmetric = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.lshape_asymmetric3.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate tshape = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.tshape2.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate xshape = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.xshape2.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate corridor1 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            RoomTemplate arcologyOval = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.arcology_vault_oval1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate arcologySmall = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.arcology_vault_small1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            RoomTemplate armoryVault = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.armory1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            RoomTemplate replacementVault = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate placeHolderVault = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            levelInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            levelInfo.LevelGenerator = templateGenerator;

            utils.PlaceOriginRoom(templateGenerator, originRoom);

            int numberOfRandomRooms = 16;

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>> { 
                new Tuple<int, RoomTemplate>(250, originRoom),
                new Tuple<int, RoomTemplate>(100, tshape),
                new Tuple<int, RoomTemplate>(100, xshape),
                new Tuple<int, RoomTemplate>(100, arcologySmall),
                new Tuple<int, RoomTemplate>(50, arcologyOval)};

            utils.PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 2, 4);

            //Add connections to other levels

            var connections = utils.AddConnectionsToOtherLevels(levelLinks, levelNo, levelInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                levelInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            utils.AddStandardPlaceholderVaults(levelInfo, templateGenerator, maxPlaceHolders);

            //If we have any more doors, add a couple of dead ends
            utils.PlaceRandomConnectedRooms(templateGenerator, 3, armoryVault, corridor1, 0, 0);

            //Add extra corridors
            utils.AddCorridorsBetweenOpenDoors(templateGenerator, 1, new List<RoomTemplate> { corridor1 });

            //Wall type
            levelInfo.TerrainMapping = terrainMapping;

            return levelInfo;
        }

        public override LevelInfo CompleteLevel()
        {
            utils.ReplaceUnconnectedDoorsWithWalls(levelInfo);
            return levelInfo;
        }
    }
}
