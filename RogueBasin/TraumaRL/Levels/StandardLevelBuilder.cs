using GraphMap;
using RogueBasin;
using System.Collections.Generic;

namespace TraumaRL.Levels
{
    class StandardLevelBuilder : LevelBuilder
    {
        private readonly LevelBuilderUtils utils;
        private readonly Dictionary<MapTerrain, List<MapTerrain>> terrainMapping;
        private readonly ConnectivityMap levelLinks;
        private readonly int startVertexIndex;
        private readonly string levelName;
        private readonly string levelReadableName;
        private readonly LevelType levelType;

        private LevelInfo levelInfo;

        public StandardLevelBuilder(LevelBuilderUtils utils, ConnectivityMap levelLinks, int startVertexIndex, Dictionary<MapTerrain, List<MapTerrain>> terrainMapping,
            LevelType levelType, string levelName, string levelReadableName)
        {
            this.utils = utils;
            this.terrainMapping = terrainMapping;
            this.levelLinks = levelLinks;
            this.startVertexIndex = startVertexIndex;
            this.levelName = levelName;
            this.levelReadableName = levelReadableName;
            this.levelType = levelType;
        }

        public override LevelInfo GenerateLevel(int levelNo)
        {
            levelInfo = new LevelInfo(levelNo, levelType, startVertexIndex, levelName, levelReadableName);

            //Load standard room types

            RoomTemplate room1 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate corridor1 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            RoomTemplate replacementVault = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate placeHolderVault = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            levelInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            levelInfo.LevelGenerator = templateGenerator;

            utils.PlaceOriginRoom(templateGenerator, room1);
            utils.PlaceRandomConnectedRooms(templateGenerator, 4, room1, corridor1, 5, 10);

            //Add connections to other levels

            var connections = utils.AddConnectionsToOtherLevels(levelLinks, levelNo, levelInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                levelInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            levelInfo.ReplaceableVaultConnections.AddRange(
                utils.AddReplaceableVaults(templateGenerator, corridor1, placeHolderVault, maxPlaceHolders));

            //Tidy terrain
            templateGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);

            return levelInfo;
        }

        public override LevelInfo CompleteLevel()
        {
            utils.ReplaceAllsDoorsWithFloor(levelInfo);
            return levelInfo;
        }
    }
}
