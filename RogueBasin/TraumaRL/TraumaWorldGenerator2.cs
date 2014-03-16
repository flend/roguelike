using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraumaRL
{
    public partial class TraumaWorldGenerator
    {
        public class MonsterSet
        {
            public List<Tuple<int, Monster>> monsterSet;
            public int difficulty;
            public double scaling;
        
            public MonsterSet(int difficulty, double scaling) {
                this.difficulty = difficulty;
                this.scaling = scaling;
                monsterSet = new List<Tuple<int,Monster>>();
            }

            public MonsterSet(int difficulty)
            {
                this.difficulty = difficulty;
                this.scaling = 1.0;
                monsterSet = new List<Tuple<int, Monster>>();
            }
            public void AddMonsterType(int weighting, Monster monsterType) {
                monsterSet.Add(new Tuple<int, Monster>(weighting, monsterType));
            }
        }

        List<MonsterSet> monsterSets;

        private void SetupMonsterWeightings()
        {
            monsterSets = new List<MonsterSet>();

            var zeroDifficultySet = new MonsterSet(0, 0.6);

            zeroDifficultySet.AddMonsterType(5, new RogueBasin.Creatures.Swarmer());
            zeroDifficultySet.AddMonsterType(20, new RogueBasin.Creatures.MaintBot());
            zeroDifficultySet.AddMonsterType(5, new RogueBasin.Creatures.RotatingTurret());

            monsterSets.Add(zeroDifficultySet);

            var oneDifficultySet = new MonsterSet(1);

            oneDifficultySet.AddMonsterType(20, new RogueBasin.Creatures.Swarmer());
            oneDifficultySet.AddMonsterType(5, new RogueBasin.Creatures.MaintBot());
            oneDifficultySet.AddMonsterType(5, new RogueBasin.Creatures.ExplosiveBarrel());

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

            twoDifficultySet3.AddMonsterType(20, new RogueBasin.Creatures.Swarmer());
            twoDifficultySet3.AddMonsterType(5, new RogueBasin.Creatures.MaintBot());
            twoDifficultySet3.AddMonsterType(5, new RogueBasin.Creatures.ExplosiveBarrel());

            monsterSets.Add(twoDifficultySet3);

            var twoDiffSet2 = new MonsterSet(2);

            twoDiffSet2.AddMonsterType(20, new RogueBasin.Creatures.RotatingTurret());
            twoDiffSet2.AddMonsterType(20, new RogueBasin.Creatures.PatrolBotRanged());
            twoDiffSet2.AddMonsterType(5, new RogueBasin.Creatures.RollingBomb());

            monsterSets.Add(twoDiffSet2);

            var threeDiffSet3 = new MonsterSet(3, 1.5);

            threeDiffSet3.AddMonsterType(20, new RogueBasin.Creatures.UberSwarmer());
            threeDiffSet3.AddMonsterType(10, new RogueBasin.Creatures.AlertBot());
            threeDiffSet3.AddMonsterType(20, new RogueBasin.Creatures.ExplosiveBarrel());

            monsterSets.Add(threeDiffSet3);

            var fourDifficultySet = new MonsterSet(4);

            fourDifficultySet.AddMonsterType(50, new RogueBasin.Creatures.WarriorCyborgRanged());
            fourDifficultySet.AddMonsterType(50, new RogueBasin.Creatures.WarriorCyborgMelee());
            fourDifficultySet.AddMonsterType(30, new RogueBasin.Creatures.ExplosiveBarrel());

            monsterSets.Add(fourDifficultySet);

            var fiveDifficultySet = new MonsterSet(5);

            fiveDifficultySet.AddMonsterType(50, new RogueBasin.Creatures.AssaultCyborgRanged());
            fiveDifficultySet.AddMonsterType(50, new RogueBasin.Creatures.AssaultCyborgMelee());
            fiveDifficultySet.AddMonsterType(50, new RogueBasin.Creatures.ExplosiveBarrel());

            var fiveDifficultySet2 = new MonsterSet(5);

            fiveDifficultySet2.AddMonsterType(50, new RogueBasin.Creatures.HeavyBotRanged());
            fiveDifficultySet2.AddMonsterType(50, new RogueBasin.Creatures.HeavyTurret());

            monsterSets.Add(fiveDifficultySet2);

            var fiveDifficultySet3 = new MonsterSet(5);

            fiveDifficultySet3.AddMonsterType(50, new RogueBasin.Creatures.AssaultCyborgMelee());
            fiveDifficultySet3.AddMonsterType(50, new RogueBasin.Creatures.HeavyTurret());
            fiveDifficultySet3.AddMonsterType(10, new RogueBasin.Creatures.AlertBot());

            monsterSets.Add(fiveDifficultySet3);


            /*
            var monsterTypesToPlace = new List<Tuple<int, Monster>> {
               //new Tuple<int, Monster>(1, new RogueBasin.Creatures.AlertBot()),
               new Tuple<int, Monster>(1, new RogueBasin.Creatures.Swarmer()),
               new Tuple<int, Monster>(1, new RogueBasin.Creatures.MaintBot()),
               new Tuple<int, Monster>(1, new RogueBasin.Creatures.RotatingTurret()),
               new Tuple<int, Monster>(50, new RogueBasin.Creatures.WarriorCyborgRanged()),
               new Tuple<int, Monster>(50, new RogueBasin.Creatures.WarriorCyborgMelee()),
               new Tuple<int, Monster>(1, new RogueBasin.Creatures.ExplosiveBarrel()),
               new Tuple<int, Monster>(1, new RogueBasin.Creatures.RollingBomb())
            };*/
        }

        private void CreateMonstersForLevels(MapInfo mapInfo)
        {
            SetupMonsterWeightings();
            
            var monsterSetsUsed = new List<MonsterSet>();

            foreach (var level in gameLevels)
            {
                var roomVertices = mapInfo.FilterOutCorridors(mapInfo.GetRoomIndicesForLevel(level));
                var floorAreaForLevel = roomVertices.Sum(v => mapInfo.GetAllPointsInRoomOfTerrain(v, RoomTemplateTerrain.Floor).Count() + mapInfo.GetAllPointsInRoomOfTerrain(v, RoomTemplateTerrain.Wall).Count());
                LogFile.Log.LogEntryDebug("Floor area for level: " + level + ": " + floorAreaForLevel, LogDebugLevel.Medium);

                //0.05 is a bit high
                double areaScaling = 0.03;
                var monstersForLevel = (int)Math.Floor(floorAreaForLevel * areaScaling);

                var monsterSetsForLevel = monsterSets.Where(s => s.difficulty == levelDifficulty[level]);

                if (!monsterSetsForLevel.Any())
                {
                    monsterSetsForLevel = monsterSets.Where(s => s.difficulty <= levelDifficulty[level]);
                }

                var newSets = monsterSetsForLevel.Except(monsterSetsUsed);
                var setsToPick = newSets;
                if (!newSets.Any())
                    setsToPick = monsterSetsForLevel;

                var setToUse = setsToPick.RandomElement();

                LogFile.Log.LogEntryDebug("Use set of difficulty " + setToUse.difficulty + " for level " + Game.Dungeon.DungeonInfo.LevelNaming[level], LogDebugLevel.Medium);

                monsterSetsUsed.Add(setToUse);

                AddMonstersToLevelGaussianDistribution(mapInfo, level, setToUse.monsterSet, monstersForLevel);

                //Not working for the time being - maybe check tomorrow morning 
                /*
                //var heurestics = new MapHeuristics(mapInfo.Model.GraphNoCycles, 0);

                for (int i = 0; i < 10; i++)
                {
                    AddMonsterLinearPatrol(mapInfo, new RogueBasin.Creatures.PatrolBot(), terminalBranchNodes, level);
                }*/
            }

        }

        private void AddMonstersToLevelGaussianDistribution(MapInfo mapInfo, int levelNo, List<Tuple<int, Monster>> monsterTypesForLevel, int totalMonsters)
        {
            var monstersToPlace = CreateGaussianDistributionOfMonsterTypes(monsterTypesForLevel, totalMonsters);
            AddMonstersToRoomsOnLevelGaussianDistribution(mapInfo, levelNo, monstersToPlace);
        }

        private IEnumerable<Monster> CreateGaussianDistributionOfMonsterTypes(List<Tuple<int, Monster>> typesToPlace, int totalMonsters)
        {
            int weightAverage = 10;
            int weightStdDev = 30;

            var monstersAndWeights = typesToPlace.Select(f => new Tuple<int, Monster>((int)Math.Abs(Gaussian.BoxMuller(weightAverage, weightStdDev)) * f.Item1, f.Item2));

            var monsterTypesDistributionExpanded = Enumerable.Range(0, totalMonsters).Select(i => ChooseItemFromWeights<Monster>(monstersAndWeights));

            return monsterTypesDistributionExpanded.Select(m => m.NewCreatureOfThisType());
        }

        private void AddMonstersToRoomsOnLevelGaussianDistribution(MapInfo mapInfo, int level, IEnumerable<Monster> monster)
        {
            //Get the number of rooms
            var allRoomsAndCorridors = mapInfo.GetRoomIndicesForLevel(level).Except(allReplaceableVaults).Except(new List<int>{mapInfo.StartRoom});
            var rooms = mapInfo.FilterOutCorridors(allRoomsAndCorridors).ToList();

            var monstersToPlaceRandomized = monster.Shuffle().ToList();

            int noMonsters = monstersToPlaceRandomized.Count;
            int noRooms = rooms.Count();

            LogFile.Log.LogEntryDebug("No rooms: " + noRooms + " Total monsters to place (level: " + level + "): " + noMonsters, LogDebugLevel.Medium);

            //Distribution amongst rooms, mostly evenly

            double[] roomMonsterRatio = new double[noRooms];

            for (int i = 0; i < noRooms; i++)
            {
                roomMonsterRatio[i] = Math.Max(0, Gaussian.BoxMuller(5, 3));
            }

            double totalMonsterRatio = 0.0;

            for (int i = 0; i < noRooms; i++)
            {
                totalMonsterRatio += roomMonsterRatio[i];
            }

            double ratioToTotalMonsterBudget = noMonsters / totalMonsterRatio;

            int[] monstersPerRoom = new int[noRooms];
            double remainder = 0.0;

            for (int i = 0; i < noRooms; i++)
            {
                double monsterBudget = roomMonsterRatio[i] * ratioToTotalMonsterBudget + remainder;

                double actualMonstersToPlace = Math.Floor(monsterBudget);

                double levelBudgetSpent = actualMonstersToPlace;

                double levelBudgetLeftOver = monsterBudget - levelBudgetSpent;

                monstersPerRoom[i] = (int)actualMonstersToPlace;
                remainder = levelBudgetLeftOver;

                //Any left over monster ratio gets added to the next level up
            }

            //Calculate actual number of monster levels placed

            int totalMonsters = 0;
            for (int i = 0; i < noRooms; i++)
            {
                totalMonsters += monstersPerRoom[i];
            }

            LogFile.Log.LogEntryDebug("Total monsters actually placed (level: " + level + "): " + noMonsters, LogDebugLevel.Medium);

            //Place monsters in rooms

            Dungeon dungeon = Game.Dungeon;

            int monsterPos = 0;

            for (int r = 0; r < noRooms; r++)
            {
                int monstersToPlaceInRoom = monstersPerRoom[r];

                var candidatePointsInRoom = mapInfo.GetAllPointsInRoomOfTerrain(rooms[r], RoomTemplateTerrain.Floor).Shuffle();

                for (int m = 0; m < monstersToPlaceInRoom; m++)
                {
                    if (monsterPos >= monstersToPlaceRandomized.Count)
                    {
                        LogFile.Log.LogEntryDebug("Trying to place too many monsters", LogDebugLevel.High);
                        monsterPos++;
                        continue;
                    }

                    Monster mon = monstersToPlaceRandomized[monsterPos];
                    GiveMonsterStandardItems(mon);

                    foreach (var p in candidatePointsInRoom)
                    {

                        bool placedSuccessfully = Game.Dungeon.AddMonster(mon, level, p);
                        if (placedSuccessfully)
                        {
                            monsterPos++;
                            break;
                        }
                    }
                }
            }
        }

        private int AddMonstersToRoom(MapInfo mapInfo, int level, int roomId, List<Monster> monsters)
        {
            var candidatePointsInRoom = mapInfo.GetAllPointsInRoomOfTerrain(roomId, RoomTemplateTerrain.Floor).Shuffle();

            int monstersPlaced = 0;

            Monster mon = monsters[monstersPlaced];

            foreach (Point p in candidatePointsInRoom)
            {
                bool placedSuccessfully = Game.Dungeon.AddMonster(mon, level, p);

                if (placedSuccessfully)
                {
                    
                    GiveMonsterStandardItems(mon);
                    monstersPlaced++;

                    if (monstersPlaced == monsters.Count())
                        break;

                    mon = monsters[monstersPlaced];
                }
            }
            return monstersPlaced;
        }

        private bool AddMonsterLinearPatrol(MapInfo mapInfo, MonsterFightAndRunAI monster, Dictionary<int, List<int>> terminalBranchNodes, int level)
        {
            var roomsOnLevel = mapInfo.FilterOutCorridors(mapInfo.GetRoomIndicesForLevel(level).Except(allReplaceableVaults));

            var roomsWithNeigbours = terminalBranchNodes.Where(tb => tb.Key > 1).SelectMany(tb => tb.Value).Intersect(roomsOnLevel);

            var sourceRooms = roomsWithNeigbours.Shuffle();

            //May be expensive
            Point startPoint = null;
            List<Point> waypoints = new List<Point>();

            foreach(var room in sourceRooms) {
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

        private bool Chance(int outOf)
        {
            if (Game.Random.Next(outOf) == 0)
                return true;
            return false;
        }

        private void GiveMonsterStandardItems(Monster mon)
        {
            mon.PickUpItem(new RogueBasin.Items.ShieldPack());

            if(Chance(2))
                mon.PickUpItem(new RogueBasin.Items.AmmoPack());

            if (Chance(20))
                mon.PickUpItem(new RogueBasin.Items.FragGrenade());
            if (Chance(20))
                mon.PickUpItem(new RogueBasin.Items.StunGrenade());
            if (Chance(20))
                mon.PickUpItem(new RogueBasin.Items.SoundGrenade());

            if (Chance(50))
                mon.PickUpItem(new RogueBasin.Items.NanoRepair());
        }

        Dictionary<int, int> levelDifficulty;

        private void CalculateLevelDifficulty()
        {
            var levelsToHandleSeparately = new List<int> { medicalLevel, arcologyLevel, computerCoreLevel, bridgeLevel };

            levelDifficulty = new Dictionary<int, int>(levelDepths);
            levelDifficulty[reactorLevel] = 4;
            levelDifficulty[arcologyLevel] = 4;
            levelDifficulty[computerCoreLevel] = 5;
            levelDifficulty[bridgeLevel] = 5;

        }

        Dictionary<int, List<Item>> itemsInArmory;

        enum Ware
        {
            Boost,
            Shield,
            Aim,
            Stealth
        }

        private void PlaceLootInArmory(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            itemsInArmory = new Dictionary<int, List<Item>>();

            

            foreach(var l in gameLevels) {
                itemsInArmory[l] = new List<Item>();
            }

            var level1Ware = new List<Item>();

            var lootLevels = new Dictionary<int, List<Item>>();

            lootLevels[0] = new List<Item> { new RogueBasin.Items.Pistol(), new RogueBasin.Items.Vibroblade() };
            lootLevels[0].AddRange(level1Ware);

            lootLevels[1] = new List<Item> { new RogueBasin.Items.Shotgun(), new RogueBasin.Items.Laser()  };

            lootLevels[2] = new List<Item>();
            // {   };
            lootLevels[3] = new List<Item> { new RogueBasin.Items.HeavyPistol() };
            //new RogueBasin.Items.BoostWare(2),  new RogueBasin.Items.StealthWare()

            lootLevels[4] = new List<Item> { new RogueBasin.Items.AssaultRifle(), new RogueBasin.Items.HeavyLaser(),  new RogueBasin.Items.HeavyShotgun(), };
            //new RogueBasin.Items.BoostWare(3), new RogueBasin.Items.AimWare(3), new RogueBasin.Items.ShieldWare(3)

            var itemsPlaced = new List<Item>();

            var wareInGame = new List<Ware> { Ware.Aim, Ware.Shield, Ware.Boost, Ware.Stealth }.RandomElements(3);

            foreach (var ware in Enum.GetValues(typeof(Ware)).Cast<Ware>())
            {
                if(!wareInGame.Contains(ware))
                    continue;

                if (ware == Ware.Aim)
                {
                    level1Ware.Add(new RogueBasin.Items.AimWare(1));
                    lootLevels[2].AddRange(new List<Item> { new RogueBasin.Items.AimWare(2) });
                    lootLevels[4].AddRange(new List<Item> { new RogueBasin.Items.AimWare(3) });
                }
                if (ware == Ware.Shield)
                {
                    level1Ware.Add(new RogueBasin.Items.ShieldWare(1));
                    lootLevels[2].AddRange(new List<Item> { new RogueBasin.Items.ShieldWare(2) });
                    lootLevels[4].AddRange(new List<Item> { new RogueBasin.Items.ShieldWare(3) });
                }
                if (ware == Ware.Boost)
                {
                    level1Ware.Add(new RogueBasin.Items.BoostWare(1));
                    lootLevels[3].AddRange(new List<Item> { new RogueBasin.Items.BoostWare(2) });
                    lootLevels[4].AddRange(new List<Item> { new RogueBasin.Items.BoostWare(3) });
                }
                if (ware == Ware.Stealth)
                {
                    lootLevels[3].AddRange(new List<Item> { new RogueBasin.Items.StealthWare() });
                }
            }

            //Give 1 ware
            var itemsGivenToPlayer = PlayerInitialItems(level1Ware);

            itemsPlaced.AddRange(itemsGivenToPlayer);

            lootLevels[0].AddRange(level1Ware.Except(itemsGivenToPlayer));

            //Guarantee on medical, at least 1 ware and a pistol or vibroblade
            var randomWare = level1Ware.Except(itemsPlaced).RandomElement();
            PlaceItems(mapInfo, new List<Item> { randomWare }, new List<int> { goodyRooms[medicalLevel] }, false, true, true);
            itemsPlaced.Add(randomWare);
            itemsInArmory[0].Add(randomWare);

            PlaceItems(mapInfo, new List<Item> { lootLevels[0][0] }, new List<int> { goodyRooms[medicalLevel] }, false, true, true);
            itemsPlaced.Add(lootLevels[0][0]);
            itemsInArmory[0].Add(lootLevels[0][0]);
            PlaceItems(mapInfo, new List<Item> { lootLevels[0][1] }, new List<int> { goodyRooms[medicalLevel] }, false, true, true);
            itemsPlaced.Add(lootLevels[0][1]);
            itemsInArmory[0].Add(lootLevels[0][0]);

            var levelsToHandleSeparately = new List<int> { medicalLevel };

            var totalLoot = lootLevels.SelectMany(kv => kv.Value).Except(itemsPlaced).Count();
            var totalRooms = goodyRooms.Select(kv => kv.Key).Except(levelsToHandleSeparately).Count();

            double lootPerRoom = totalLoot / (double)totalRooms;
            int lootPerRoomInt = (int)Math.Floor(lootPerRoom);

            int lootPlaced = 0;
            int roomsDone = 0;

            foreach (var kv in goodyRooms.OrderBy(k => k.Key))
            {
                var level = kv.Key;
                var room = kv.Value;

                if (levelsToHandleSeparately.Contains(level))
                    continue;

                var possibleLoot = lootLevels.Where(l => l.Key <= levelDifficulty[level]).SelectMany(l => l.Value).Except(itemsPlaced);

                var lootInRoom = 0;
                while (lootInRoom < lootPerRoomInt)
                {
                    if (!possibleLoot.Any())
                        break;

                    var lootToPlace = possibleLoot.RandomElement();

                    PlaceItems(mapInfo, new List<Item> { lootToPlace }, new List<int> { room }, false, true, true);
                    LogFile.Log.LogEntryDebug("Placing item: " + lootToPlace.SingleItemDescription + " on level " + Game.Dungeon.DungeonInfo.LevelNaming[level], LogDebugLevel.Medium);

                    itemsPlaced.Add(lootToPlace);
                    itemsInArmory[level].Add(lootToPlace);
                    lootInRoom++;
                    lootPlaced++;
                }

                roomsDone++;

                //If we are below our quota
                var behindLoot = (int)Math.Floor(roomsDone * lootPerRoom - lootPlaced);

                var behindLootPlaced = 0;
                while (behindLootPlaced < behindLoot)
                {
                    if (!possibleLoot.Any())
                        break;

                    var lootToPlace = possibleLoot.RandomElement();

                    PlaceItems(mapInfo, new List<Item> { lootToPlace }, new List<int> { room }, false, true, true);
                    LogFile.Log.LogEntryDebug("Placing item (catchup): " + lootToPlace.SingleItemDescription + " on level " + Game.Dungeon.DungeonInfo.LevelNaming[level], LogDebugLevel.Medium);

                    itemsPlaced.Add(lootToPlace);
                    itemsInArmory[level].Add(lootToPlace);
                    lootPlaced++;
                    behindLootPlaced++;
                }
            }


            //If we have loot remaining
            if (lootPlaced < totalLoot)
            {
                var possibleLoot = lootLevels.SelectMany(l => l.Value).Except(itemsPlaced);

                //Place at random
                foreach (var i in possibleLoot)
                {
                    var randomRoom = goodyRooms.RandomElement();
                    PlaceItems(mapInfo, new List<Item> { i }, new List<int> { randomRoom.Value }, false, true, true);
                    itemsPlaced.Add(i);
                    itemsInArmory[randomRoom.Key].Add(i);
                    lootPlaced++;
                    LogFile.Log.LogEntryDebug("Placing item (final): " + i.SingleItemDescription + " on level " + Game.Dungeon.DungeonInfo.LevelNaming[randomRoom.Key], LogDebugLevel.Medium);
                }
            }

            LogFile.Log.LogEntryDebug("Total items placed  " + itemsPlaced.Count() + " of " + lootLevels.SelectMany(kv => kv.Value).Count(), LogDebugLevel.Medium);

            //Add extra standard loot
            foreach (var kv in goodyRooms.OrderBy(k => k.Key))
            {
                var level = kv.Key;
                var room = kv.Value;

                var randomMedKits = ProduceMultipleItems<RogueBasin.Items.NanoRepair>(levelDifficulty[level] / 2 + Game.Random.Next(2));
                PlaceItems(mapInfo, randomMedKits, new List<int> { room }, false, true, true);

                var totalGrenades = Game.Random.Next(2 * levelDifficulty[level], 3 * levelDifficulty[level]);

                var totalExposiveGrenades = totalGrenades / 2;
                var totalStunGrenades = Game.Random.Next(totalGrenades - totalExposiveGrenades);
                var totalSoundGrenades = totalGrenades - totalExposiveGrenades - totalStunGrenades;

                var fragGrenades = ProduceMultipleItems<RogueBasin.Items.FragGrenade>(1 + Game.Random.Next(levelDifficulty[level]));
                var stunGrenades = ProduceMultipleItems<RogueBasin.Items.StunGrenade>(1 + Game.Random.Next(levelDifficulty[level]));
                var soundGrenades = ProduceMultipleItems<RogueBasin.Items.SoundGrenade>(1 + Game.Random.Next(levelDifficulty[level]));

                PlaceItems(mapInfo, fragGrenades, new List<int> { room }, false, true, true);
                PlaceItems(mapInfo, stunGrenades, new List<int> { room }, false, true, true);
                PlaceItems(mapInfo, soundGrenades, new List<int> { room }, false, true, true);
            }
            
        }

        private List<Item> ProduceMultipleItems<T>(int count) where T : Item, new() {

            List<Item> toReturn = new List<Item>();
            for(int i=0;i<count;i++) {
                toReturn.Add(new T());
            }

            return toReturn;
        }

        private IEnumerable<Item> PlayerInitialItems(List<Item> level1Ware)
        {
            var itemsGiven = new List<Item>();

            var player = Game.Dungeon.Player;
            player.GiveItemNotFromDungeon(new RogueBasin.Items.Fists());

            var level1WareToGive = level1Ware.RandomElement();

            itemsGiven.Add(level1WareToGive);

            player.GiveItemNotFromDungeon(level1WareToGive);

            return itemsGiven;
        }
    }


}
