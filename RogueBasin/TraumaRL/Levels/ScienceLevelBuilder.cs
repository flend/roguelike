using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL.Levels
{
    class ScienceLevelBuilder : LevelBuilder
    {
        private readonly LevelBuilderUtils utils;
        private readonly Dictionary<MapTerrain, List<MapTerrain>> terrainMapping;
        private readonly ConnectivityMap levelLinks;
        private readonly int startVertexIndex;
        private readonly string levelName;
        private readonly string levelReadableName;

        public ScienceLevelBuilder(LevelBuilderUtils utils, ConnectivityMap levelLinks, int startVertexIndex, Dictionary<MapTerrain, List<MapTerrain>> terrainMapping,
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
            var levelInfo = new LevelInfo(levelNo, levelName, levelReadableName);

            //Load standard room types
            RoomTemplate corridor1 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            RoomTemplate replacementVault = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate placeHolderVault = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            levelInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            levelInfo.LevelGenerator = templateGenerator;

            //Load sample templates
            RoomTemplate branchRoom = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.branchroom.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate branchRoom2 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.branchroom2.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate chamber1Doors = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.chamber7x3_1door.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate chamber2Doors = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.chamber7x3_2door.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate chamber1Doors2 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.chamber6x4_1door.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate chamber2Doors2 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.chamber6x4_2door.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            //Build a network of branched corridors

            //Place branch rooms to form the initial structure, joined on long axis
            utils.PlaceOriginRoom(templateGenerator, branchRoom);

            utils.PlaceRandomConnectedRooms(templateGenerator, 3, branchRoom, corridor1, 0, 0, () => Game.Random.Next(1) > 0 ? 3 : 4);
            utils.PlaceRandomConnectedRooms(templateGenerator, 3, branchRoom2, corridor1, 0, 0, () => Game.Random.Next(1) > 0 ? 2 : 3);

            //Add some 2-door rooms
            var twoDoorDistribution = new List<Tuple<int, RoomTemplate>> {
                new Tuple<int, RoomTemplate>(2, chamber1Doors),
                new Tuple<int, RoomTemplate>(2, chamber1Doors2)
            };

            utils.PlaceRandomConnectedRooms(templateGenerator, 10, twoDoorDistribution, corridor1, 0, 0);

            //Add some 1-door deadends

            var oneDoorDistribution = new List<Tuple<int, RoomTemplate>> {
                new Tuple<int, RoomTemplate>(2, chamber2Doors),
                new Tuple<int, RoomTemplate>(2, chamber2Doors2)
            };
            utils.PlaceRandomConnectedRooms(templateGenerator, 10, oneDoorDistribution, corridor1, 0, 0);

            //Add connections to other levels

            var connections = utils.AddConnectionsToOtherLevels(levelLinks, levelNo, levelInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                levelInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            utils.AddStandardPlaceholderVaults(levelInfo, templateGenerator, maxPlaceHolders);

            //Add extra corridors
            //AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Wall type
            levelInfo.TerrainMapping = terrainMapping;

            return levelInfo;
        }
    }
}
