using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    public class MonsterPlacement
    {
        public class MonsterSet
        {
            public List<Tuple<int, Monster>> monsterSet;
            public int difficulty;
            public double scaling;

            public MonsterSet(int difficulty, double scaling)
            {
                this.difficulty = difficulty;
                this.scaling = scaling;
                monsterSet = new List<Tuple<int, Monster>>();
            }

            public MonsterSet(int difficulty)
            {
                this.difficulty = difficulty;
                this.scaling = 1.0;
                monsterSet = new List<Tuple<int, Monster>>();
            }
            public void AddMonsterType(int weighting, Monster monsterType)
            {
                monsterSet.Add(new Tuple<int, Monster>(weighting, monsterType));
            }
        }

        private class MonsterInRoom
        {
            public readonly Monster monster;
            public readonly int roomId;
            public readonly Point point;

            public MonsterInRoom(Monster monster, int roomId, Point point)
            {
                this.monster = monster;
                this.roomId = roomId;
                this.point = point;
            }
        }


        private List<MonsterSet> monsterSets;

        public MonsterPlacement() {

        }

        private void SetupMonsterWeightings()
        {
            monsterSets = new List<MonsterSet>();

            var zeroDifficultySet = new MonsterSet(0, 0.6);

            zeroDifficultySet.AddMonsterType(5, new RogueBasin.Creatures.Swarmer(1));
            zeroDifficultySet.AddMonsterType(20, new RogueBasin.Creatures.MaintBot());
            zeroDifficultySet.AddMonsterType(5, new RogueBasin.Creatures.RotatingTurret());

            monsterSets.Add(zeroDifficultySet);

            var oneDifficultySet = new MonsterSet(1);

            oneDifficultySet.AddMonsterType(20, new RogueBasin.Creatures.Swarmer(1));
            oneDifficultySet.AddMonsterType(5, new RogueBasin.Creatures.MaintBot());
            oneDifficultySet.AddMonsterType(5, new RogueBasin.Creatures.ExplosiveBarrel(1));

            monsterSets.Add(oneDifficultySet);

            var oneDifficultySet2 = new MonsterSet(1);

            oneDifficultySet2.AddMonsterType(20, new RogueBasin.Creatures.ServoCyborgMelee());
            oneDifficultySet2.AddMonsterType(20, new RogueBasin.Creatures.ServoCyborgRanged());
            oneDifficultySet2.AddMonsterType(5, new RogueBasin.Creatures.MaintBot());

            monsterSets.Add(oneDifficultySet2);

            var twoDiffSet1 = new MonsterSet(2);

            twoDiffSet1.AddMonsterType(20, new RogueBasin.Creatures.ServoCyborgMelee());
            twoDiffSet1.AddMonsterType(20, new RogueBasin.Creatures.ServoCyborgRanged());
            twoDiffSet1.AddMonsterType(10, new RogueBasin.Creatures.AlertBot());

            monsterSets.Add(twoDiffSet1);

            var twoDifficultySet3 = new MonsterSet(2, 2.0);

            twoDifficultySet3.AddMonsterType(20, new RogueBasin.Creatures.Swarmer(1));
            twoDifficultySet3.AddMonsterType(5, new RogueBasin.Creatures.MaintBot());
            twoDifficultySet3.AddMonsterType(5, new RogueBasin.Creatures.ExplosiveBarrel(1));

            monsterSets.Add(twoDifficultySet3);

            var twoDiffSet2 = new MonsterSet(2);

            twoDiffSet2.AddMonsterType(20, new RogueBasin.Creatures.RotatingTurret());
            twoDiffSet2.AddMonsterType(20, new RogueBasin.Creatures.PatrolBotRanged());
            twoDiffSet2.AddMonsterType(5, new RogueBasin.Creatures.RollingBomb());

            monsterSets.Add(twoDiffSet2);

            var threeDiffSet3 = new MonsterSet(3, 1.5);

            threeDiffSet3.AddMonsterType(20, new RogueBasin.Creatures.UberSwarmer());
            threeDiffSet3.AddMonsterType(10, new RogueBasin.Creatures.AlertBot());
            threeDiffSet3.AddMonsterType(20, new RogueBasin.Creatures.ExplosiveBarrel(1));

            monsterSets.Add(threeDiffSet3);

            var fourDifficultySet = new MonsterSet(4);

            fourDifficultySet.AddMonsterType(50, new RogueBasin.Creatures.WarriorCyborgRanged());
            fourDifficultySet.AddMonsterType(50, new RogueBasin.Creatures.WarriorCyborgMelee());
            fourDifficultySet.AddMonsterType(30, new RogueBasin.Creatures.ExplosiveBarrel(1));

            monsterSets.Add(fourDifficultySet);

            var fiveDifficultySet = new MonsterSet(5);

            fiveDifficultySet.AddMonsterType(50, new RogueBasin.Creatures.AssaultCyborgRanged());
            fiveDifficultySet.AddMonsterType(50, new RogueBasin.Creatures.AssaultCyborgMelee());
            fiveDifficultySet.AddMonsterType(50, new RogueBasin.Creatures.ExplosiveBarrel(1));

            var fiveDifficultySet2 = new MonsterSet(5);

            fiveDifficultySet2.AddMonsterType(50, new RogueBasin.Creatures.HeavyBotRanged());
            fiveDifficultySet2.AddMonsterType(50, new RogueBasin.Creatures.HeavyTurret());

            monsterSets.Add(fiveDifficultySet2);

            var fiveDifficultySet3 = new MonsterSet(5);

            fiveDifficultySet3.AddMonsterType(50, new RogueBasin.Creatures.AssaultCyborgMelee());
            fiveDifficultySet3.AddMonsterType(50, new RogueBasin.Creatures.HeavyTurret());
            fiveDifficultySet3.AddMonsterType(10, new RogueBasin.Creatures.AlertBot());

            monsterSets.Add(fiveDifficultySet3);
        }


        public void CreateMonstersForLevels(MapState mapState, GameDifficulty difficulty, IEnumerable<int> levelsToProcess, ImmutableDictionary<int, int> levelDifficulty)
        {
            var monstersToPlace = CreateMonstersToPlaceInLevels(mapState, difficulty, levelsToProcess, levelDifficulty);

            foreach (var monsterPlacement in monstersToPlace)
            {
                mapState.MapInfo.Populator.AddMonsterToRoom(monsterPlacement.monster, monsterPlacement.roomId, monsterPlacement.point);
            }
        }

        public void CreateMonstersForLevelsAndPopulateInDungeon(MapState mapState, GameDifficulty difficulty, int level, int levelDifficulty)
        {
            var levels = new List<int> { level };
            var levelDifficulties = new Dictionary<int, int> { { level, levelDifficulty } }.ToImmutableDictionary();
            var monstersToPlace = CreateMonstersToPlaceInLevels(mapState, difficulty, new List<int> { level }, levelDifficulties);

            foreach (var monsterPlacement in monstersToPlace)
            {
                var absoluteLocation = mapState.MapInfo.RelativeRoomPointToLocation(monsterPlacement.roomId, monsterPlacement.point);
                var success = Game.Dungeon.AddMonster(monsterPlacement.monster, absoluteLocation);

                if (!success)
                {
                    LogFile.Log.LogEntryDebug("Failed to place monster " + monsterPlacement.monster.SingleDescription + " at " + absoluteLocation, LogDebugLevel.High);
                }
            }
        }


        private IEnumerable<MonsterInRoom> CreateMonstersToPlaceInLevels(MapState mapState, GameDifficulty difficulty, IEnumerable<int> levelsToProcess, ImmutableDictionary<int, int> levelDifficulty)
        {
            var mapInfo = mapState.MapInfo;

            SetupMonsterWeightings();

            var monsterSetsUsed = new List<MonsterSet>();
            IEnumerable<MonsterInRoom> allMonstersToPlace = new List<MonsterInRoom>();

            foreach (var level in levelsToProcess)
            {
                var roomVertices = mapInfo.FilterOutCorridors(mapInfo.GetRoomIndicesForLevel(level));
                var floorAreaForLevel = roomVertices.Sum(v => mapInfo.GetAllPointsInRoomOfTerrain(v, RoomTemplateTerrain.Floor).Count() + mapInfo.GetAllPointsInRoomOfTerrain(v, RoomTemplateTerrain.Wall).Count());
                LogFile.Log.LogEntryDebug("Floor area for level: " + level + ": " + floorAreaForLevel, LogDebugLevel.Medium);

                //0.05 is a bit high
                double areaScaling = 0.03;
                var monstersForLevel = (int)Math.Floor(floorAreaForLevel * areaScaling);
                var monsterScaledDifficulty = levelDifficulty[level] / 2;

                var monsterSetsForLevel = monsterSets.Where(s => s.difficulty == monsterScaledDifficulty);

                if (!monsterSetsForLevel.Any())
                {
                    monsterSetsForLevel = monsterSets.Where(s => s.difficulty <= monsterScaledDifficulty);
                }

                var newSets = monsterSetsForLevel.Except(monsterSetsUsed);
                var setsToPick = newSets;
                if (!newSets.Any())
                    setsToPick = monsterSetsForLevel;

                var setToUse = setsToPick.RandomElement();

                monstersForLevel = (int)Math.Ceiling(monstersForLevel * setToUse.scaling);

                if (difficulty == GameDifficulty.Easy)
                    monstersForLevel = (int)Math.Ceiling(monstersForLevel * 0.5);
                else if (difficulty == GameDifficulty.Medium)
                    monstersForLevel = (int)Math.Ceiling(monstersForLevel * 1.0);
                else if (difficulty == GameDifficulty.Hard)
                    monstersForLevel = (int)Math.Ceiling(monstersForLevel * 1.2);

                LogFile.Log.LogEntryDebug("Use set of difficulty " + setToUse.difficulty + " for level " + mapState.LevelGraph.LevelNames[level], LogDebugLevel.Medium);

                monsterSetsUsed.Add(setToUse);

                var monstersToPlace = AddMonstersToLevelGaussianDistribution(mapInfo, difficulty, level, setToUse.monsterSet, monstersForLevel);

                IEnumerable<MonsterInRoom> barrelsToPlace = new List<MonsterInRoom>();
                var monsterSetContainsBarrels = setToUse.monsterSet.Where(t => t.Item2.GetType() == typeof(RogueBasin.Creatures.ExplosiveBarrel));
                
                if (monsterSetContainsBarrels.Any())
                {
                    barrelsToPlace = AddMonstersToLevelGaussianDistribution(mapInfo, difficulty, level, new List<Tuple<int, Monster>> { new Tuple<int, Monster>(1, new RogueBasin.Creatures.ExplosiveBarrel(1)) }, monstersForLevel / 3);
                }

                var monstersToPlaceOnLevel = monstersToPlace.Concat(barrelsToPlace);
                allMonstersToPlace = allMonstersToPlace.Concat(monstersToPlaceOnLevel);
            }

            return allMonstersToPlace;
        }

        private IEnumerable<Monster> CreateGaussianDistributionOfMonsterTypes(List<Tuple<int, Monster>> typesToPlace, int totalMonsters)
        {
            int weightAverage = 10;
            int weightStdDev = 30;

            var monstersAndWeights = typesToPlace.Select(f => new Tuple<int, Monster>((int)Math.Abs(Gaussian.BoxMuller(weightAverage, weightStdDev)) * f.Item1, f.Item2));

            var monsterTypesDistributionExpanded = Enumerable.Range(0, totalMonsters).Select(i => ChooseItemFromWeights<Monster>(monstersAndWeights));

            return monsterTypesDistributionExpanded.Select(m => m.NewCreatureOfThisType());
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

        private IEnumerable<MonsterInRoom> AddMonstersToLevelGaussianDistribution(MapInfo mapInfo, GameDifficulty difficulty, int levelNo, List<Tuple<int, Monster>> monsterTypesForLevel, int totalMonsters)
        {
            var monstersToPlace = CreateGaussianDistributionOfMonsterTypes(monsterTypesForLevel, totalMonsters);
            return AddMonstersToRoomsOnLevel(mapInfo, difficulty, levelNo, monstersToPlace);
        }

        private IEnumerable<MonsterInRoom> AddMonstersToRoomsOnLevel(MapInfo mapInfo, GameDifficulty difficulty, int level, IEnumerable<Monster> monster)
        {
            //Get the number of rooms
            var allRoomsAndCorridors = mapInfo.GetRoomIndicesForLevel(level).Except(new List<int> { mapInfo.StartRoom });
            var rooms = mapInfo.FilterOutCorridors(allRoomsAndCorridors).ToList();
            var candidatePointsInRooms = rooms.Select(room => RoomTemplateUtilities.GetPointsInRoomWithTerrain(mapInfo.Room(room).Room, RoomTemplateTerrain.Floor));
            var roomsAndPointsInRooms = rooms.Zip(candidatePointsInRooms, Tuple.Create);

            var monstersToPlaceRandomized = monster.Shuffle().ToList();

            int noMonsters = monstersToPlaceRandomized.Count;
            int noRooms = rooms.Count();

            LogFile.Log.LogEntryDebug("No rooms: " + noRooms + " Total monsters to place (level: " + level + "): " + noMonsters, LogDebugLevel.Medium);

            //Distribution amongst rooms, mostly evenly, scaled by room size

            var roomMonsterRatio = roomsAndPointsInRooms.Select( rp => Math.Max(0, Gaussian.BoxMuller(5, 3)) * rp.Item2.Count());

            double totalMonsterRatio = roomMonsterRatio.Sum();

            double ratioToTotalMonsterBudget = noMonsters / totalMonsterRatio;

            int[] monstersPerRoom = new int[noRooms];
            double remainder = 0.0;

            for (int i = 0; i < noRooms; i++)
            {
                double monsterBudget = roomMonsterRatio.ElementAt(i) * ratioToTotalMonsterBudget + remainder;

                double actualMonstersToPlace = Math.Floor(monsterBudget);

                double levelBudgetSpent = actualMonstersToPlace;
                double levelBudgetLeftOver = monsterBudget - levelBudgetSpent;

                monstersPerRoom[i] = (int)actualMonstersToPlace;
                remainder = levelBudgetLeftOver;

                //Any left over monster ratio gets added to the next level up
            }

            //Calculate actual number of monster levels placed

            int totalMonsters = monstersPerRoom.Sum();
            LogFile.Log.LogEntryDebug("Total monsters actually placed (level: " + level + "): " + noMonsters, LogDebugLevel.Medium);

            //Place monsters in rooms
              
            int monsterPos = 0;
            var monstersInRooms = new List<MonsterInRoom>();

            for (int r = 0; r < noRooms; r++)
            {
                int monstersToPlaceInRoom = monstersPerRoom[r];

                var roomId = roomsAndPointsInRooms.ElementAt(r).Item1;
                var candidatePointsInRoom = roomsAndPointsInRooms.ElementAt(r).Item2.Shuffle();
                int monstersPlacedInRoom = 0;

                foreach (var p in candidatePointsInRoom)
                {
                    if (monsterPos >= monstersToPlaceRandomized.Count)
                    {
                        LogFile.Log.LogEntryDebug("Trying to place too many monsters", LogDebugLevel.High);
                        monsterPos++;
                        break;
                    }

                    Monster mon = monstersToPlaceRandomized[monsterPos];
                    GiveMonsterStandardItems(mon, difficulty);

                    monstersInRooms.Add(new MonsterInRoom(mon, roomId, p));
                    monsterPos++;
                    monstersPlacedInRoom++;
                
                    if (monstersPlacedInRoom >= monstersToPlaceInRoom)
                        break;
                }
            }

            return monstersInRooms;
        }
        
        /* unused
        private bool AddMonsterLinearPatrol(MapInfo mapInfo, MonsterFightAndRunAI monster, Dictionary<int, List<int>> terminalBranchNodes, int level)
        {
            var roomsOnLevel = mapInfo.FilterOutCorridors(mapInfo.GetRoomIndicesForLevel(level));

            var roomsWithNeigbours = terminalBranchNodes.Where(tb => tb.Key > 1).SelectMany(tb => tb.Value).Intersect(roomsOnLevel);

            var sourceRooms = roomsWithNeigbours.Shuffle();

            //May be expensive
            Point startPoint = null;
            List<Point> waypoints = new List<Point>();

            foreach (var room in sourceRooms)
            {
                var distanceToOtherRooms = mapInfo.Model.GetDistanceOfVerticesFromParticularVertexInFullMap(room, roomsWithNeigbours);

                var sisterRooms = distanceToOtherRooms.Where(kv => kv.Value == 2).Select(kv => kv.Key);

                if (!sisterRooms.Any())
                    continue;

                var candidatePointsInRoom = mapInfo.GetAllPointsInRoomOfTerrain(room, RoomTemplateTerrain.Floor).Shuffle();

                bool success = false;
                foreach (var p in candidatePointsInRoom)
                {
                    success = Game.Dungeon.AddMonster(monster, level, p);

                    if (success)
                    {
                        startPoint = p;
                        break;
                    }
                }

                if (!success)
                {
                    //Failure
                    continue;
                }

                foreach (var sisterRoom in sisterRooms)
                {
                    var candidatePointInSisterRoom = mapInfo.GetAllPointsInRoomOfTerrain(sisterRoom, RoomTemplateTerrain.Floor).RandomElement();
                    waypoints.Add(candidatePointInSisterRoom);
                }

                monster.Waypoints = waypoints;
                break;
            }

            if (startPoint == null)
                return false;

            return true;
        }
        */

        private bool Chance(double outOf)
        {
            if (Game.Random.Next((int)Math.Floor(outOf)) < 100)
                return true;
            return false;
        }

        private void GiveMonsterStandardItems(Monster mon, GameDifficulty difficulty)
        {

            double ammoChance = 100;
            double shieldChance = 100;
            double nadeChance = 2000;
            double repairChance = 20000;

            switch (difficulty)
            {
                case GameDifficulty.Easy:
                    ammoChance = 50;
                    shieldChance = 50;
                    nadeChance = 4000;
                    repairChance = 20000;
                    break;
                case GameDifficulty.Medium:
                    ammoChance = 200;
                    shieldChance = 100;
                    nadeChance = 6000;
                    repairChance = 20000;
                    break;
                case GameDifficulty.Hard:
                    ammoChance = 400;
                    shieldChance = 200;
                    nadeChance = 6000;
                    repairChance = 20000;
                    break;
            }

            //Alter by monster type
            if (mon.DropChance() > 0)
            {
                ammoChance = ammoChance * 20 / (double)mon.DropChance();
                shieldChance = shieldChance * 20 / (double)mon.DropChance();

                if (Chance(ammoChance))
                    mon.PickUpItem(new RogueBasin.Items.AmmoPack());

                if (Chance(shieldChance))
                    mon.PickUpItem(new RogueBasin.Items.ShieldPack());

                if (Chance(nadeChance))
                    mon.PickUpItem(new RogueBasin.Items.FragGrenade());
                if (Chance(nadeChance))
                    mon.PickUpItem(new RogueBasin.Items.StunGrenade());
                if (Chance(nadeChance))
                    mon.PickUpItem(new RogueBasin.Items.SoundGrenade());

                if (Chance(repairChance))
                    mon.PickUpItem(new RogueBasin.Items.NanoRepair());
            }
            

            
        }
    }
}
