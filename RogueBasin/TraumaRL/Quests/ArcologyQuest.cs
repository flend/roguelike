using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL.Quests
{
    class ArcologyQuest : Quest
    {
        
        string arcologyAntDoorId;

        public ArcologyQuest(QuestMapBuilder builder, LogGenerator logGen)
            : base(builder, logGen)
        {
            //Order of the main quest (in future, this will be generic)

            //ESCAPE POD QUEST
            //Escape pod (flight deck) [requires active self-destruct]
            //Activate self-destruct (bridge) [requires enable self-destruct]
            //Enable self-destruct (reactor) [requires computer cores destroyed]
            //Destroy computer cores (computer-core) [no pre-requisite]
            
            //Bridge lock (any level place captain's cabin) [no pre-requisite]

            //Computer core lock (arcology) [no pre-requisite]
            //Arcology lock (any level - place bioware) [no pre-requisite]
            //Arcology lock (any level) [antennae disabled]
            //Antennae (science / storage) [no pre-requisite]

            //To chain quests
            //Figure out the list of quests, from last to first
            //From first to last, pass on the result-clues to the next quest
            //(e.g. door key, 20 monsters killed clues) + possibly some meta-data to inform the logs
            //Then create quests from last to first, referring to the inherited clues

            //[or never write quests with pre-requisites! lock doors to levels with a complete quest as a single quest object]
        }

        public override void SetupQuest(MapState mapState)
        {
            //Computer core lock
            //Requires computer tech's id
            ComputerCoreId(mapState);

            //Arcology lock
            //Requires bioprotect wetware
            ArcologyLock(mapState);

            //Antanae
            //Requires servo motor
            AntennaeQuest(mapState);

        }

        private void ComputerCoreId(MapState mapState)
        {
            var levelInfo = mapState.LevelGraph.LevelInfo;
            var mapInfo = mapState.MapInfo;

            var colorForComputerTechsId = Builder.GetUnusedColor();
            var computerCoreLevel = mapState.LevelGraph.LevelIds["computerCore"];

            //computer core is a dead end
            var computerCoreSourceElevatorConnection = levelInfo[computerCoreLevel].ConnectionsToOtherLevels.First();
            var levelToComputerCore = computerCoreSourceElevatorConnection.Key;
            var elevatorToComputerCore = levelInfo[levelToComputerCore].ConnectionsToOtherLevels[computerCoreLevel];

            var computerDoorName = "tech's id computer core";
            var computerDoorId = computerDoorName;
            var computerDoorColor = colorForComputerTechsId.Item1;

            Builder.PlaceMovieDoorOnMap(mapState, computerDoorId, computerDoorName, 1, computerDoorColor, "computercoreunlocked", "computercorelocked", elevatorToComputerCore);

            //Tech's id (this should always work)
            var arcologyLevel = mapState.LevelGraph.LevelIds["arcology"];
            var techIdIdealLevel = new List<int> { arcologyLevel };
            var techIdRoom = Builder.PlaceClueForDoorInVault(mapState, levelInfo, computerDoorId, computerDoorColor, computerDoorName, techIdIdealLevel);
            var techIdLevel = mapInfo.GetLevelForRoomIndex(techIdRoom);

            //A slaughter
            var bioDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Egg1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.HumanCorpse]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.CorpseinGoo]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.HumanCorpse2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Skeleton])
            };
            Builder.AddStandardDecorativeFeaturesToRoom(mapState, techIdRoom, 20, bioDecorations, false);
        }

        private void ArcologyLock(MapState mapState)
        {
            var levelInfo = mapState.LevelGraph.LevelInfo;
            var mapInfo = mapState.MapInfo;

            var arcologyLevel = mapState.LevelGraph.LevelIds["arcology"];
            var computerCoreLevel = mapState.LevelGraph.LevelIds["computerCore"];
            var bridgeLevel = mapState.LevelGraph.LevelIds["bridge"];
            var colorForArcologyLock = Builder.GetUnusedColor();

            //Find door blocking arcology from start6
            var routeToArcology = mapState.LevelGraph.GetPathBetweenLevels(mapState.StartLevel, arcologyLevel);

            var levelBeforeArcology = routeToArcology.Last().Source;

            var levelToArcology = routeToArcology.Last().Source;
            var elevatorToArcology = levelInfo[levelToArcology].ConnectionsToOtherLevels[arcologyLevel];

            var arcologyDoorName = "bioware - arcology door lock";
            var arcologyDoorId = arcologyDoorName;
            var arcologyDoorColor = colorForArcologyLock.Item1;

            //Place the arcology door
            var manager = mapState.DoorAndClueManager;

            manager.PlaceDoor(new DoorRequirements(elevatorToArcology, arcologyDoorId, 1));
            var door = manager.GetDoorById(arcologyDoorId);

            var arcologyDoor = new RogueBasin.Locks.SimpleOptionalLockedDoorWithMovie(door, "arcologyunlocked", "arcologylocked", "Override the security and go in anyway?", arcologyDoorName, arcologyDoorColor);

            Builder.PlaceLockedDoorOnMap(mapState, arcologyDoor, door);

            //Bioware
            var storageLevel = mapState.LevelGraph.LevelIds["storage"];
            var scienceLevel = mapState.LevelGraph.LevelIds["science"];

            var biowareIdIdealLevel = new List<int> { storageLevel, scienceLevel };
            //PlaceClueForDoorInVault(mapInfo, levelInfo, arcologyDoorId, arcologyDoorColor, arcologyDoorName, biowareIdIdealLevel);
            var biowareRoom = Builder.PlaceClueItemForDoorInVault(mapState, levelInfo, arcologyDoorId, new RogueBasin.Items.BioWare(), arcologyDoorName, biowareIdIdealLevel);
            var biowareLevel = mapInfo.GetLevelForRoomIndex(biowareRoom);
            var bioDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Egg1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Egg2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.CorpseinGoo]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.EggChair])
            };
            Builder.AddStandardDecorativeFeaturesToRoom(mapState, biowareRoom, 10, bioDecorations, false);

            //Logs

            var allowedRoomsForLogs = manager.GetValidRoomsToPlaceClueForDoor(arcologyDoorName);

            var criticalPathLog = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, elevatorToArcology.Source);

            var preferredRoomsForLogsCritical = Builder.FilterRoomsByPath(mapState, allowedRoomsForLogs, criticalPathLog, false, QuestMapBuilder.CluePath.OnCriticalPath, true);
            var preferredRoomsForLogsNonCritical = Builder.FilterRoomsByPath(mapState, allowedRoomsForLogs, criticalPathLog, false, QuestMapBuilder.CluePath.Any, true);

            var roomsForLogsCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, 2, preferredRoomsForLogsCritical);
            var roomsForLogsNonCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, 2, preferredRoomsForLogsNonCritical);

            var logCluesCritical = manager.AddCluesToExistingDoor(arcologyDoorId, roomsForLogsCritical);
            var logCluesNonCritical = manager.AddCluesToExistingDoor(arcologyDoorId, roomsForLogsNonCritical);

            var log2 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_arcology3", levelToArcology, biowareLevel), logCluesCritical[0]);
            var log3 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_arcology4", levelToArcology, biowareLevel), logCluesCritical[1]);

            var log1 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_arcology1", levelToArcology, biowareLevel), logCluesNonCritical[0]);
            var log4 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_arcology2", levelToArcology, biowareLevel), logCluesNonCritical[1]);

            Builder.PlaceLogClues(mapState, new List<Tuple<LogEntry, Clue>> { log1, log2, log3, log4 }, true, true);

            //Wrap the arcology door in another door that depends on the antennae
            //Get critical path to archology door

            var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, elevatorToArcology.Source);

            var lastCorridorToArcology = criticalPath.ElementAt(criticalPath.Count() - 1);

            var colorForArcologyAntLock = Builder.GetUnusedColor();

            arcologyAntDoorId = "antennae - arcology door lock";
            var arcologyAntDoorColor = colorForArcologyAntLock.Item1;

            manager.PlaceDoor(new DoorRequirements(lastCorridorToArcology, arcologyAntDoorId, 1));
            var door2 = manager.GetDoorById(arcologyAntDoorId);

            var arcologyAntDoor = new RogueBasin.Locks.SimpleLockedDoorWithMovie(door2, "arcologyantunlocked", "arcologyantlocked", arcologyAntDoorId, arcologyAntDoorColor);

            Builder.PlaceLockedDoorOnMap(mapState, arcologyAntDoor, door2);
        }

        private void AntennaeQuest(MapState mapState)
        {
            var scienceLevel = mapState.LevelGraph.LevelIds["science"];
            var storageLevel = mapState.LevelGraph.LevelIds["storage"];

            var mapInfo = mapState.MapInfo;
            var manager = mapState.DoorAndClueManager;

            var levelsForAntennae = new List<int> { scienceLevel, storageLevel };
            var unusedVaultsInAntennaeLevel = Builder.GetAllAvailableVaults(mapState).Where(c => levelsForAntennae.Contains(mapInfo.GetLevelForRoomIndex(c.Target)));

            var unusedVaultsInnAntennaeLevelOrderFromStart = mapInfo.RoomsInDescendingDistanceFromSource(mapInfo.StartRoom, unusedVaultsInAntennaeLevel.Select(c => c.Target));
            var antennaeRoom = unusedVaultsInnAntennaeLevelOrderFromStart.ElementAt(0);
            var antennaeVaultConnection = Builder.GetAllVaults(mapState).Where(c => c.Target == antennaeRoom).First();

            var antennaeVault = antennaeVaultConnection.Target;
            var antennaeObjName = "antennae";
            manager.PlaceObjective(new ObjectiveRequirements(antennaeVault, antennaeObjName, 1, new List<string> { arcologyAntDoorId }));
            var antennaeObj = manager.GetObjectiveById(antennaeObjName);
            Builder.PlaceObjective(mapState, antennaeObj, new RogueBasin.Features.AntennaeObjective(antennaeObj, mapState.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(antennaeObj)), true, true, true);

            Builder.UseVault(mapState, antennaeVaultConnection);

            //Extra stuff for antenna room

            var antennaeLevel = mapInfo.GetLevelForRoomIndex(antennaeVault);

            var monstersToPlace = new List<Monster> { new RogueBasin.Creatures.RotatingTurret(), new RogueBasin.Creatures.HeavyTurret(), new RogueBasin.Creatures.PatrolBotRanged(), new RogueBasin.Creatures.PatrolBotRanged() };
            
            Builder.PlaceCreaturesInRoom(mapState, antennaeLevel, antennaeVault, monstersToPlace, false);

            var decorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Antennae]) };
            Builder.AddStandardDecorativeFeaturesToRoom(mapState, antennaeVault, 10, decorations, false);

            //Servo motor

            var servoRoom = Builder.PlaceMovieClueForObjectiveInVault(mapState, antennaeObjName, (char)312, "interface_demod", "Interface Demodulator", new List<int> { scienceLevel, storageLevel });
            var servoLevel = mapInfo.GetLevelForRoomIndex(servoRoom);

            var servoDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Machine]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Machine2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.MachinePart2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.MachinePart3])
            };
            Builder.AddStandardDecorativeFeaturesToRoom(mapState, servoRoom, 10, servoDecorations, false);

            //Logs

            var allowedRoomsForLogs = manager.GetValidRoomsToPlaceClueForObjective(antennaeObjName);

            var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, antennaeVault);

            var preferredRoomsForLogsCritical = Builder.FilterRoomsByPath(mapState, allowedRoomsForLogs, criticalPath, false, QuestMapBuilder.CluePath.OnCriticalPath, true);
            var preferredRoomsForLogsNonCritical = Builder.FilterRoomsByPath(mapState, allowedRoomsForLogs, criticalPath, false, QuestMapBuilder.CluePath.Any, true);

            var roomsForLogsCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, 2, preferredRoomsForLogsCritical);
            var roomsForLogsNonCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, 2, preferredRoomsForLogsNonCritical);

            var logCluesCritical = manager.AddCluesToExistingObjective(antennaeObjName, roomsForLogsCritical);
            var logCluesNonCritical = manager.AddCluesToExistingObjective(antennaeObjName, roomsForLogsNonCritical);

            var log2 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_antennae2", antennaeLevel, servoLevel), logCluesCritical[0]);
            var log3 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_antennae3", antennaeLevel, servoLevel), logCluesCritical[1]);

            var log1 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_antennae1", antennaeLevel, servoLevel), logCluesNonCritical[0]);
            var log4 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(mapState, "qe_antennae4", antennaeLevel, servoLevel), logCluesNonCritical[1]);

            Builder.PlaceLogClues(mapState, new List<Tuple<LogEntry, Clue>> { log1, log2, log3, log4 }, true, true);

        }
        public override void RegisterLevels(LevelRegister register)
        {
            throw new NotImplementedException();
        }
    }
}
