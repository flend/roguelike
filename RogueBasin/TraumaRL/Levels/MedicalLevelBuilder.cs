using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL.Levels
{
    class MedicalLevelBuilder : LevelBuilder
    {
        private readonly LevelBuilderUtils utils;
        private readonly Dictionary<MapTerrain, List<MapTerrain>> terrainMapping;
        private readonly ConnectivityMap levelLinks;
        private readonly int startVertexIndex;
        private readonly string levelName;
        private readonly string levelReadableName;

        public MedicalLevelBuilder(LevelBuilderUtils utils, ConnectivityMap levelLinks, int startVertexIndex, Dictionary<MapTerrain, List<MapTerrain>> terrainMapping,
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
            var medicalInfo = new LevelInfo(levelNo, levelName, levelReadableName);

            //Load standard room types

            RoomTemplate deadEnd = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.chamber3x3_1door.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate medicalBay = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.medical_bay1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate corridor1 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            RoomTemplate replacementVault = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.replacevault1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate placeHolderVault = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.placeholdervault1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            var mapBuilder = new TemplatedMapBuilder(100, 100);
            medicalInfo.LevelBuilder = mapBuilder;
            var templateGenerator = new TemplatedMapGenerator(mapBuilder, startVertexIndex);
            medicalInfo.LevelGenerator = templateGenerator;

            utils.PlaceOriginRoom(templateGenerator, medicalBay);

            int numberOfRandomRooms = 10;

            utils.BuildTXShapedRoomsBig(templateGenerator, numberOfRandomRooms);

            //Add connections to other levels

            var connections = utils.AddConnectionsToOtherLevels(levelLinks, levelNo, medicalInfo, corridor1, replacementVault, templateGenerator);
            foreach (var connection in connections)
            {
                medicalInfo.ConnectionsToOtherLevels[connection.Item1] = connection.Item2;
            }

            //Add a small number of place holder holder rooms for vaults
            int maxPlaceHolders = 3;

            utils.AddStandardPlaceholderVaults(medicalInfo, templateGenerator, maxPlaceHolders);

            //Add extra corridors
            utils.AddCorridorsBetweenOpenDoors(templateGenerator, 5, new List<RoomTemplate> { corridor1 });

            //Wall type
            medicalInfo.TerrainMapping = terrainMapping;

            return medicalInfo;
        }
    }
}
