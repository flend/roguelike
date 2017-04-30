using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumaRL
{
    /// <summary>
    /// Chooses and builds the quests for this instance of the game
    /// </summary>
    class TraumaQuestManager
    {
        private bool quickLevelGen;
        private QuestMapBuilder builder;
        private LogGenerator logGen;

        private Quests.EscapePodQuest escapePodQuest;
        private Quests.CaptainIdQuest captainsIdQuest;
        private Quests.ArcologyQuest arcologyQuest;
        private Quest medicalQuest;
        private Quests.LowerAtriumQuest lowerAtriumQuest;

        private LevelIdData startLevel;

        public TraumaQuestManager(QuestMapBuilder builder, LogGenerator logGen, bool quickLevelGen)
        {
            this.builder = builder;
            this.logGen = logGen;
            this.quickLevelGen = quickLevelGen;

            escapePodQuest = new Quests.EscapePodQuest(builder, logGen);
            captainsIdQuest = new Quests.CaptainIdQuest(builder, logGen);
            arcologyQuest = new Quests.ArcologyQuest(builder, logGen);
            medicalQuest = new Quests.MedicalTurretTrapQuest(builder, logGen);
            lowerAtriumQuest = new Quests.LowerAtriumQuest(builder, logGen);
        }

        public void RegisterQuests(LevelRegister register)
        {
            escapePodQuest.RegisterLevels(register);
            captainsIdQuest.RegisterLevels(register);
            arcologyQuest.RegisterLevels(register);
            medicalQuest.RegisterLevels(register);
            lowerAtriumQuest.RegisterLevels(register);

            startLevel = register.GetIdForLevelType(new RequiredLevelInfo(LevelType.MedicalLevel));
        }

        public void GenerateQuests(MapState mapState)
        {
            var mapInfo = mapState.MapInfo;
            var levelInfo = mapState.LevelGraph.LevelInfo;

            var mapHeuristics = new MapHeuristics(mapInfo.Model.GraphNoCycles, mapInfo.StartRoom);
            var roomConnectivityMap = mapHeuristics.GetTerminalBranchConnections();

            if (!quickLevelGen)
            {
                BuildMainQuest(mapState, builder);
            }
            BuildMedicalLevelQuests(mapState, builder);

            if (!quickLevelGen)
            {
                BuildAtriumLevelQuests(mapState, builder, roomConnectivityMap);

                BuildRandomElevatorQuests(mapState, builder, roomConnectivityMap);

                BuildGoodyQuests(mapState, builder, roomConnectivityMap);
            }
        }

        private void BuildRandomElevatorQuests(MapState mapState, QuestMapBuilder builder, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            var noLevelsToBlock = 1 + Game.Random.Next(1);

            var lowerAtriumLevel = mapState.LevelGraph.LevelIds["lowerAtrium"];
            var medicalLevel = mapState.LevelGraph.LevelIds["medical"];

            var candidateLevels = mapState.LevelGraph.GameLevels.Except(new List<int> { lowerAtriumLevel, medicalLevel }).Where(l => mapState.LevelGraph.LevelInfo[l].ConnectionsToOtherLevels.Count() > 1);
            LogFile.Log.LogEntryDebug("Candidates for elevator quests: " + candidateLevels, LogDebugLevel.Medium);
            var chosenLevels = candidateLevels.RandomElements(noLevelsToBlock);

            foreach (var level in chosenLevels)
            {
                try
                {
                    var blockElevatorQuest = new Quests.BlockElevatorQuest(builder, logGen, level, roomConnectivityMap);
                    blockElevatorQuest.ClueOnElevatorLevel = Game.Random.Next(2) > 0;
                    blockElevatorQuest.SetupQuest(mapState);
                }
                catch (Exception ex)
                {
                    LogFile.Log.LogEntryDebug("Random Elevator Exception (level " + level + "): " + ex, LogDebugLevel.High);
                }
            }
        }

        private void BuildAtriumLevelQuests(MapState mapState, QuestMapBuilder builder, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            try
            {
                var lowerAtriumLevel = mapState.LevelGraph.LevelIds["lowerAtrium"];
                var blockElevatorQuest = new Quests.BlockElevatorQuest(builder, logGen, lowerAtriumLevel, roomConnectivityMap);
                blockElevatorQuest.SetupQuest(mapState);
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntryDebug("Atrium Elevator Exception: " + ex, LogDebugLevel.High);
            }
        }


        private void BuildGoodyQuests(MapState mapState, QuestMapBuilder builder, Dictionary<int, List<Connection>> roomConnectivityMap)
        {
            var armoryQuest = new Quests.ArmoryQuest(builder, logGen);
            armoryQuest.SetupQuest(mapState);
        }

        private void BuildMedicalLevelQuests(MapState mapState, QuestMapBuilder builder)
        {
            //var cameraQuest = new Quests.MedicalCameraQuest(mapState, builder, logGen);
            medicalQuest.SetupQuest(mapState);
        }

        private void BuildMainQuest(MapState mapState, QuestMapBuilder questMapBuilder)
        {
            escapePodQuest.SetupQuest(mapState);

            captainsIdQuest.SetupQuest(mapState);

            arcologyQuest.SetupQuest(mapState);
        }

        public LevelIdData StartLevel
        {
            get
            {
                return startLevel;
            }
        }
    }
}
