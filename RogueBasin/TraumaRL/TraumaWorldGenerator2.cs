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
        private void CreateMonstersForLevels(MapInfo mapInfo)
        {
            var monsterTypesToPlace = new List<Tuple<int, Monster>> {
               //new Tuple<int, Monster>(1, new RogueBasin.Creatures.AlertBot()),
               new Tuple<int, Monster>(1, new RogueBasin.Creatures.Swarmer()),
               new Tuple<int, Monster>(1, new RogueBasin.Creatures.MaintBot()),
               new Tuple<int, Monster>(1, new RogueBasin.Creatures.RotatingTurret()),
               new Tuple<int, Monster>(50, new RogueBasin.Creatures.WarriorCyborgRanged()),
               new Tuple<int, Monster>(50, new RogueBasin.Creatures.WarriorCyborgMelee()),
               new Tuple<int, Monster>(1, new RogueBasin.Creatures.ExplosiveBarrel()),
               new Tuple<int, Monster>(1, new RogueBasin.Creatures.RollingBomb())
            };

            foreach (var level in gameLevels)
            {
                var roomVertices = mapInfo.FilterOutCorridors(mapInfo.GetRoomIndicesForLevel(level));
                var floorAreaForLevel = roomVertices.Sum(v => mapInfo.GetAllPointsInRoomOfTerrain(v, RoomTemplateTerrain.Floor).Count());
                LogFile.Log.LogEntryDebug("Floor area for level: " + level + ": " + floorAreaForLevel, LogDebugLevel.Medium);

                double areaScaling = 0.05;
                var monstersForLevel = (int)Math.Floor(floorAreaForLevel * areaScaling);

                //todo - number of monsters on level size?
                AddMonstersToLevelGaussianDistribution(mapInfo, level, monsterTypesToPlace, monstersForLevel);

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
            var allRoomsAndCorridors = mapInfo.GetRoomIndicesForLevel(level).Except(allReplaceableVaults);
            var rooms = mapInfo.FilterOutCorridors(allRoomsAndCorridors).ToList();

            var monstersToPlaceRandomized = monster.Shuffle().ToList();

            int noMonsters = monstersToPlaceRandomized.Count;
            int noRooms = rooms.Count();

            LogFile.Log.LogEntryDebug("No rooms: " + noRooms + " Total monsters to place (level: " + level + "): " + noMonsters, LogDebugLevel.Medium);

            //Distribution amongst rooms, mostly evenly

            double[] roomMonsterRatio = new double[noRooms];

            for (int i = 0; i < noRooms; i++)
            {
                roomMonsterRatio[i] = 1;//Math.Max(0, Gaussian.BoxMuller(5, 0));
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
        
        private void GiveMonsterStandardItems(Monster mon)
        {
            mon.PickUpItem(new RogueBasin.Items.ShieldPack());
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

        private void PlaceLootInArmory(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            itemsInArmory = new Dictionary<int, List<Item>>();
            
            foreach(var l in gameLevels) {
                itemsInArmory[l] = new List<Item>();
            }

            var level1Ware = new List<Item> { new RogueBasin.Items.BoostWare(1), new RogueBasin.Items.AimWare(1), new RogueBasin.Items.ShieldWare(1) };

            var lootLevels = new Dictionary<int, List<Item>>();

            lootLevels[0] = new List<Item> { new RogueBasin.Items.Pistol(), new RogueBasin.Items.Vibroblade() };
            lootLevels[0].AddRange(level1Ware);

            lootLevels[1] = new List<Item> { new RogueBasin.Items.Shotgun(), new RogueBasin.Items.Laser()  };

            lootLevels[2] = new List<Item> {  new RogueBasin.Items.AimWare(2), new RogueBasin.Items.ShieldWare(2) };

            lootLevels[3] = new List<Item> { new RogueBasin.Items.HeavyPistol(), new RogueBasin.Items.BoostWare(2), new RogueBasin.Items.StealthWare() };

            lootLevels[4] = new List<Item> { new RogueBasin.Items.AssaultRifle(), new RogueBasin.Items.HeavyLaser(),  new RogueBasin.Items.HeavyShotgun(), new RogueBasin.Items.BoostWare(3), 
                new RogueBasin.Items.AimWare(3), new RogueBasin.Items.ShieldWare(3), };

            var itemsPlaced = new List<Item>();

            itemsPlaced.AddRange(PlayerInitialItems(level1Ware));

            //Guarantee on medical, at least 1 ware and a pistol or vibroblade
            var randomWare = level1Ware.Except(itemsPlaced).RandomElement();
            PlaceItems(mapInfo, new List<Item> { randomWare }, new List<int> { goodyRooms[medicalLevel] }, false, true);
            itemsPlaced.Add(randomWare);
            itemsInArmory[0].Add(randomWare);

            PlaceItems(mapInfo, new List<Item> { lootLevels[0][0] }, new List<int> { goodyRooms[medicalLevel] }, false, true);
            itemsPlaced.Add(lootLevels[0][0]);
            itemsInArmory[0].Add(lootLevels[0][0]);
            PlaceItems(mapInfo, new List<Item> { lootLevels[0][1] }, new List<int> { goodyRooms[medicalLevel] }, false, true);
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
                    
                    PlaceItems(mapInfo, new List<Item>{lootToPlace}, new List<int> {room}, false, true);
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

                    PlaceItems(mapInfo, new List<Item> { lootToPlace }, new List<int> { room }, false, true);
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
                    PlaceItems(mapInfo, new List<Item> { i }, new List<int> { randomRoom.Value }, false, true);
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
                PlaceItems(mapInfo, randomMedKits, new List<int> { room }, false, true);

                var totalGrenades = Game.Random.Next(2 * levelDifficulty[level], 3 * levelDifficulty[level]);

                var totalExposiveGrenades = totalGrenades / 2;
                var totalStunGrenades = Game.Random.Next(totalGrenades - totalExposiveGrenades);
                var totalSoundGrenades = totalGrenades - totalExposiveGrenades - totalStunGrenades;

                var fragGrenades = ProduceMultipleItems<RogueBasin.Items.FragGrenade>(1 + Game.Random.Next(levelDifficulty[level]));
                var stunGrenades = ProduceMultipleItems<RogueBasin.Items.StunGrenade>(1 + Game.Random.Next(levelDifficulty[level]));
                var soundGrenades = ProduceMultipleItems<RogueBasin.Items.SoundGrenade>(1 + Game.Random.Next(levelDifficulty[level]));

                PlaceItems(mapInfo, fragGrenades, new List<int> { room }, false, true);
                PlaceItems(mapInfo, stunGrenades, new List<int> { room }, false, true);
                PlaceItems(mapInfo, soundGrenades, new List<int> { room }, false, true);
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
