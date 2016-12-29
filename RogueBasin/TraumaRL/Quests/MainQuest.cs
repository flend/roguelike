using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraumaRL.Quests
{
    class MainQuest : Quest
    {

        string arcologyAntDoorId;

        public MainQuest(MapState mapState, QuestMapBuilder builder, LogGenerator logGen) : base(mapState, builder, logGen)
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

        public override void SetupQuest()
        {
            //Bridge lock
            //Requires captain's id
            BridgeLock();

            //Computer core lock
            //Requires computer tech's id
            ComputerCoreId();

            //Archology lock
            //Requires bioprotect wetware
            ArcologyLock();

            //Antanae
            //Requires servo motor
            AntennaeQuest();

        }


        private void BridgeLock()
        {
            var bridgeLevel = MapState.LevelIds["bridge"];
            var lowerAtriumLevel = MapState.LevelIds["lowerAtrium"];
            var medicalLevel = MapState.LevelIds["medical"];
            var storageLevel = MapState.LevelIds["storage"];
            var scienceLevel = MapState.LevelIds["science"];

            var levelInfo = MapState.LevelInfo;
            var mapInfo = MapState.MapInfo;

            //bridge is a dead end
            var sourceElevatorConnection = levelInfo[bridgeLevel].ConnectionsToOtherLevels.First();
            var connectingLevel = sourceElevatorConnection.Key;
            var elevatorToBridge = levelInfo[connectingLevel].ConnectionsToOtherLevels[bridgeLevel];

            var doorName = "captain's id bridge";
            var doorId = doorName;
            var colorForCaptainId = Builder.GetUnusedColor();
            var doorColor = colorForCaptainId.Item1;

            Builder.PlaceMovieDoorOnMap(MapState, doorId, doorName, 1, doorColor, "bridgelocked", "bridgeunlocked", elevatorToBridge);

            //Captain's id
            var captainIdIdealLevel = MapState.LevelDepths.Where(kv => kv.Value >= 1).Select(kv => kv.Key).Except(new List<int> { lowerAtriumLevel, medicalLevel, storageLevel, scienceLevel });
            var captainsIdRoom = Builder.PlaceClueForDoorInVault(MapState, levelInfo, doorId, doorColor, doorName, captainIdIdealLevel);

            //Add monsters - nice to put ID on captain but not for now
            var captainsIdLevel = mapInfo.GetLevelForRoomIndex(captainsIdRoom);
            var monstersToPlace = new List<Monster> { new RogueBasin.Creatures.HeavyTurret(), new RogueBasin.Creatures.HeavyTurret(), new RogueBasin.Creatures.AssaultCyborgRanged(), new RogueBasin.Creatures.Captain() };
            Builder.PlaceCreaturesInRoom(MapState, captainsIdLevel, captainsIdRoom, monstersToPlace, false);

            var decorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Skeleton]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Plant2]),
                new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Plant3])};
            Builder.AddStandardDecorativeFeaturesToRoom(MapState, captainsIdRoom, 10, decorations, false);

            //Logs

            var manager = MapState.DoorAndClueManager;

            var allowedRoomsForLogs = manager.GetValidRoomsToPlaceClueForDoor(doorId);

            var criticalPathLog = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, elevatorToBridge.Source);

            var preferredRoomsForLogsCritical = Builder.FilterRoomsByPath(MapState, allowedRoomsForLogs, criticalPathLog, false, QuestMapBuilder.CluePath.OnCriticalPath, true);
            var preferredRoomsForLogsNonCritical = Builder.FilterRoomsByPath(MapState, allowedRoomsForLogs, criticalPathLog, false, QuestMapBuilder.CluePath.Any, true);

            var roomsForLogsCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(MapState, 2, preferredRoomsForLogsCritical);
            var roomsForLogsNonCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(MapState, 2, preferredRoomsForLogsNonCritical);

            var logCluesCritical = manager.AddCluesToExistingDoor(doorId, roomsForLogsCritical);
            var logCluesNonCritical = manager.AddCluesToExistingDoor(doorId, roomsForLogsNonCritical);

            var log2 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_captain1", connectingLevel, captainsIdLevel), logCluesCritical[0]);
            var log3 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_captain2", connectingLevel, captainsIdLevel), logCluesCritical[1]);

            var log1 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_captain3", connectingLevel, captainsIdLevel), logCluesNonCritical[0]);
            var log4 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_captain4", connectingLevel, captainsIdLevel), logCluesNonCritical[1]);

            Builder.PlaceLogClues(MapState, new List<Tuple<LogEntry, Clue>> { log1, log2, log3, log4 }, true, true);
        }

        private void ComputerCoreId()
        {
            var levelInfo = MapState.LevelInfo;
            var mapInfo = MapState.MapInfo;

            var colorForComputerTechsId = Builder.GetUnusedColor();
            var computerCoreLevel = MapState.LevelIds["computerCore"];

            //computer core is a dead end
            var computerCoreSourceElevatorConnection = levelInfo[computerCoreLevel].ConnectionsToOtherLevels.First();
            var levelToComputerCore = computerCoreSourceElevatorConnection.Key;
            var elevatorToComputerCore = levelInfo[levelToComputerCore].ConnectionsToOtherLevels[computerCoreLevel];

            var computerDoorName = "tech's id computer core";
            var computerDoorId = computerDoorName;
            var computerDoorColor = colorForComputerTechsId.Item1;

            Builder.PlaceMovieDoorOnMap(MapState, computerDoorId, computerDoorName, 1, computerDoorColor, "computercoreunlocked", "computercorelocked", elevatorToComputerCore);

            //Tech's id (this should always work)
            var arcologyLevel = MapState.LevelIds["arcology"];
            var techIdIdealLevel = new List<int> { arcologyLevel };
            var techIdRoom = Builder.PlaceClueForDoorInVault(MapState, levelInfo, computerDoorId, computerDoorColor, computerDoorName, techIdIdealLevel);
            var techIdLevel = mapInfo.GetLevelForRoomIndex(techIdRoom);

            //A slaughter
            var bioDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Egg1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.HumanCorpse]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.CorpseinGoo]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.HumanCorpse2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Skeleton])
            };
            Builder.AddStandardDecorativeFeaturesToRoom(MapState, techIdRoom, 20, bioDecorations, false);
        }

        private void ArcologyLock()
        {
            var levelInfo = MapState.LevelInfo;
            var mapInfo = MapState.MapInfo;

            var arcologyLevel = MapState.LevelIds["arcology"];
            var computerCoreLevel = MapState.LevelIds["computerCore"];
            var bridgeLevel = MapState.LevelIds["bridge"];
            var colorForArcologyLock = Builder.GetUnusedColor();

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
            var manager = MapState.DoorAndClueManager;

            manager.PlaceDoor(new DoorRequirements(elevatorToArcology, arcologyDoorId, 1));
            var door = manager.GetDoorById(arcologyDoorId);

            var arcologyDoor = new RogueBasin.Locks.SimpleOptionalLockedDoorWithMovie(door, "arcologyunlocked", "arcologylocked", "Override the security and go in anyway?", arcologyDoorName, arcologyDoorColor);

            Builder.PlaceLockedDoorOnMap(MapState, arcologyDoor, door);

            //Bioware
            var flightDeck = MapState.LevelIds["flightDeck"];
            var storageLevel = MapState.LevelIds["storage"];
            var scienceLevel = MapState.LevelIds["science"];

            var biowareIdIdealLevel = new List<int> { storageLevel, scienceLevel, flightDeck };
            //PlaceClueForDoorInVault(mapInfo, levelInfo, arcologyDoorId, arcologyDoorColor, arcologyDoorName, biowareIdIdealLevel);
            var biowareRoom = Builder.PlaceClueItemForDoorInVault(MapState, levelInfo, arcologyDoorId, new RogueBasin.Items.BioWare(), arcologyDoorName, biowareIdIdealLevel);
            var biowareLevel = mapInfo.GetLevelForRoomIndex(biowareRoom);
            var bioDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Egg1]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Egg2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.CorpseinGoo]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.EggChair])
            };
            Builder.AddStandardDecorativeFeaturesToRoom(MapState, biowareRoom, 10, bioDecorations, false);

            //Logs

            var allowedRoomsForLogs = manager.GetValidRoomsToPlaceClueForDoor(arcologyDoorName);

            var criticalPathLog = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, elevatorToArcology.Source);

            var preferredRoomsForLogsCritical = Builder.FilterRoomsByPath(MapState, allowedRoomsForLogs, criticalPathLog, false, QuestMapBuilder.CluePath.OnCriticalPath, true);
            var preferredRoomsForLogsNonCritical = Builder.FilterRoomsByPath(MapState, allowedRoomsForLogs, criticalPathLog, false, QuestMapBuilder.CluePath.Any, true);

            var roomsForLogsCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(MapState, 2, preferredRoomsForLogsCritical);
            var roomsForLogsNonCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(MapState, 2, preferredRoomsForLogsNonCritical);

            var logCluesCritical = manager.AddCluesToExistingDoor(arcologyDoorId, roomsForLogsCritical);
            var logCluesNonCritical = manager.AddCluesToExistingDoor(arcologyDoorId, roomsForLogsNonCritical);

            var log2 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_arcology3", levelToArcology, biowareLevel), logCluesCritical[0]);
            var log3 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_arcology4", levelToArcology, biowareLevel), logCluesCritical[1]);

            var log1 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_arcology1", levelToArcology, biowareLevel), logCluesNonCritical[0]);
            var log4 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_arcology2", levelToArcology, biowareLevel), logCluesNonCritical[1]);

            Builder.PlaceLogClues(MapState, new List<Tuple<LogEntry, Clue>> { log1, log2, log3, log4 }, true, true);

            //Wrap the arcology door in another door that depends on the antennae
            //Get critical path to archology door

            var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, arcologyLockSourceElevatorConnection.Value.Source);

            //Don't use 2 sincee that's between levels
            var lastCorridorToArcology = criticalPath.ElementAt(criticalPath.Count() - 4);

            var colorForArcologyAntLock = Builder.GetUnusedColor();

            arcologyAntDoorId = "antennae - arcology door lock";
            var arcologyAntDoorColor = colorForArcologyAntLock.Item1;

            manager.PlaceDoor(new DoorRequirements(lastCorridorToArcology, arcologyAntDoorId, 1));
            var door2 = manager.GetDoorById(arcologyAntDoorId);

            var arcologyAntDoor = new RogueBasin.Locks.SimpleLockedDoorWithMovie(door2, "arcologyantunlocked", "arcologyantlocked", arcologyAntDoorId, arcologyAntDoorColor);

            Builder.PlaceLockedDoorOnMap(MapState, arcologyAntDoor, door2);
        }

        private void AntennaeQuest()
        {
            var scienceLevel = MapState.LevelIds["science"];
            var storageLevel = MapState.LevelIds["storage"];

            var mapInfo = MapState.MapInfo;
            var manager = MapState.DoorAndClueManager;

            var levelsForAntennae = new List<int> { scienceLevel, storageLevel };
            var unusedVaultsInAntennaeLevel = Builder.GetAllAvailableVaults(MapState).Where(c => levelsForAntennae.Contains(mapInfo.GetLevelForRoomIndex(c.Target)));

            var unusedVaultsInnAntennaeLevelOrderFromStart = mapInfo.RoomsInDescendingDistanceFromSource(mapInfo.StartRoom, unusedVaultsInAntennaeLevel.Select(c => c.Target));
            var antennaeRoom = unusedVaultsInnAntennaeLevelOrderFromStart.ElementAt(0);
            var antennaeVaultConnection = Builder.GetAllVaults(MapState).Where(c => c.Target == antennaeRoom).First();

            var antennaeVault = antennaeVaultConnection.Target;
            var antennaeObjName = "antennae";
            manager.PlaceObjective(new ObjectiveRequirements(antennaeVault, antennaeObjName, 1, new List<string> { arcologyAntDoorId }));
            var antennaeObj = manager.GetObjectiveById(antennaeObjName);
            Builder.PlaceObjective(MapState, antennaeObj, new RogueBasin.Features.AntennaeObjective(antennaeObj, MapState.DoorAndClueManager.GetClueObjectsLiberatedByAnObjective(antennaeObj)), true, true, true);

            Builder.UseVault(MapState, antennaeVaultConnection);

            //Extra stuff for antenna room

            var antennaeLevel = mapInfo.GetLevelForRoomIndex(antennaeVault);

            var monstersToPlace = new List<Monster> { new RogueBasin.Creatures.RotatingTurret(), new RogueBasin.Creatures.HeavyTurret(), new RogueBasin.Creatures.PatrolBotRanged(), new RogueBasin.Creatures.PatrolBotRanged() };
            
            Builder.PlaceCreaturesInRoom(MapState, antennaeLevel, antennaeVault, monstersToPlace, false);

            var decorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Antennae]) };
            Builder.AddStandardDecorativeFeaturesToRoom(MapState, antennaeVault, 10, decorations, false);

            //Servo motor

            var servoRoom = Builder.PlaceMovieClueForObjectiveInVault(MapState, antennaeObjName, (char)312, "interface_demod", "Interface Demodulator", new List<int> { scienceLevel, storageLevel });
            var servoLevel = mapInfo.GetLevelForRoomIndex(servoRoom);

            var servoDecorations = new List<Tuple<int, DecorationFeatureDetails.Decoration>> { new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Machine]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Machine2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.MachinePart2]),
            new Tuple<int, DecorationFeatureDetails.Decoration>(1, DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.MachinePart3])
            };
            Builder.AddStandardDecorativeFeaturesToRoom(MapState, servoRoom, 10, servoDecorations, false);

            //Logs

            var allowedRoomsForLogs = manager.GetValidRoomsToPlaceClueForObjective(antennaeObjName);

            var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(mapInfo.StartRoom, antennaeVault);

            var preferredRoomsForLogsCritical = Builder.FilterRoomsByPath(MapState, allowedRoomsForLogs, criticalPath, false, QuestMapBuilder.CluePath.OnCriticalPath, true);
            var preferredRoomsForLogsNonCritical = Builder.FilterRoomsByPath(MapState, allowedRoomsForLogs, criticalPath, false, QuestMapBuilder.CluePath.Any, true);

            var roomsForLogsCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(MapState, 2, preferredRoomsForLogsCritical);
            var roomsForLogsNonCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(MapState, 2, preferredRoomsForLogsNonCritical);

            var logCluesCritical = manager.AddCluesToExistingObjective(antennaeObjName, roomsForLogsCritical);
            var logCluesNonCritical = manager.AddCluesToExistingObjective(antennaeObjName, roomsForLogsNonCritical);

            var log2 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_antennae2", antennaeLevel, servoLevel), logCluesCritical[0]);
            var log3 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_antennae3", antennaeLevel, servoLevel), logCluesCritical[1]);

            var log1 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_antennae1", antennaeLevel, servoLevel), logCluesNonCritical[0]);
            var log4 = new Tuple<LogEntry, Clue>(LogGen.GenerateGeneralQuestLogEntry(MapState, "qe_antennae4", antennaeLevel, servoLevel), logCluesNonCritical[1]);

            Builder.PlaceLogClues(MapState, new List<Tuple<LogEntry, Clue>> { log1, log2, log3, log4 }, true, true);

        }
    }
}
