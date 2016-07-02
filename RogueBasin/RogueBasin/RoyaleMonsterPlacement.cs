﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RogueBasin
{

    enum ItemType
    {
        Ranged, RangedExtras, Melee, Utility, NerdExtras, MeleeExtras
    }

    class RoyaleMonsterPlacement
    {
        //level => list of monster set generators with weights
        Dictionary<int, IEnumerable<Tuple<int, Func<int, double, IEnumerable<Monster>>>>> monsterGenerators = new Dictionary<int, IEnumerable<Tuple<int, Func<int, double, IEnumerable<Monster>>>>>();
        //item => list of item set generators with weights
        Dictionary<ItemType, IEnumerable<Tuple<int, Func<IEnumerable<Item>>>>> itemGenerators = new Dictionary<ItemType, IEnumerable<Tuple<int, Func<IEnumerable<Item>>>>>();
        Dictionary<PlayerClass, IEnumerable<Tuple<int, ItemType>>> classItemCategories = new Dictionary<PlayerClass, IEnumerable<Tuple<int, ItemType>>>();

        public RoyaleMonsterPlacement()
        {
            SetupMonsterGenerators();
            SetupItemGenerators();
            SetupItemCategories();
        }

        private void SetupItemCategories()
        {
            classItemCategories[PlayerClass.Athlete] = new List<Tuple<int, ItemType>> {
                new Tuple<int, ItemType>(100, ItemType.Ranged),
                new Tuple<int, ItemType>(100, ItemType.Utility)
            };

            classItemCategories[PlayerClass.Gunner] = new List<Tuple<int, ItemType>> {
                new Tuple<int, ItemType>(100, ItemType.Ranged),
                new Tuple<int, ItemType>(30, ItemType.Melee),
                new Tuple<int, ItemType>(100, ItemType.Utility)
            };

            classItemCategories[PlayerClass.Sneaker] = new List<Tuple<int, ItemType>> {
                new Tuple<int, ItemType>(100, ItemType.Ranged),
                new Tuple<int, ItemType>(50, ItemType.Melee),
                new Tuple<int, ItemType>(200, ItemType.Utility)
            };
        }

        private void SetupMonsterGenerators()
        {
            monsterGenerators[0] = new List<Tuple<int, Func<int, double, IEnumerable<Monster>>>> {
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, PunkSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 50, HunterSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 50, SwarmerSet )
            };

            monsterGenerators[1] = new List<Tuple<int, Func<int, double, IEnumerable<Monster>>>> {
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, SwarmerSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, HunterSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, PunkSet )
            };

            monsterGenerators[2] = new List<Tuple<int, Func<int, double, IEnumerable<Monster>>>> {
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, SwarmerSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 200, ThugSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, HunterSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, PunkSet )
            };

            monsterGenerators[3] = new List<Tuple<int, Func<int, double, IEnumerable<Monster>>>> {
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, SwarmerSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, HunterSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 200, GrenadierSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, ThugSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, PsychoSet )
            };

            monsterGenerators[4] = new List<Tuple<int, Func<int, double, IEnumerable<Monster>>>> {
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, SwarmerSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, HunterSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, GrenadierSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, ThugSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 200, JunkborgSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, PsychoSet )

            };

            monsterGenerators[5] = new List<Tuple<int, Func<int, double, IEnumerable<Monster>>>> {
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, SwarmerSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, HunterSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, GrenadierSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, ThugSet ),
                new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, JunkborgSet ),
               new Tuple<int, Func<int, double, IEnumerable<Monster>>> ( 100, PsychoSet )

            };

        }

        private void SetupItemGenerators()
        {
            itemGenerators[ItemType.Ranged] = new List<Tuple<int, Func<IEnumerable<Item>>>> {
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, ShotgunItem ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, LaserItem ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, AssaultRifle ),
            };

            itemGenerators[ItemType.RangedExtras] = new List<Tuple<int, Func<IEnumerable<Item>>>> {
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, ShotgunItem ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, LaserItem ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, AssaultRifle ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 50, RocketLauncher )

            };

            itemGenerators[ItemType.Melee] = new List<Tuple<int, Func<IEnumerable<Item>>>> {
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, AxeItem ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, PoleItem )
            };

            itemGenerators[ItemType.MeleeExtras] = new List<Tuple<int, Func<IEnumerable<Item>>>> {
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, AxeItem ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, PoleItem )
            };

            itemGenerators[ItemType.Utility] = new List<Tuple<int, Func<IEnumerable<Item>>>> {
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, FragGrenade ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, StunGrenade ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 30, SoundGrenade ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 30, Mine ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 30, AcidGrenade )
            };

            itemGenerators[ItemType.NerdExtras] = new List<Tuple<int, Func<IEnumerable<Item>>>> {
                new Tuple<int, Func<IEnumerable<Item>>> ( 40, FragGrenade ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 40, StunGrenade ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, AcidGrenade ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, SoundGrenade ),
                new Tuple<int, Func<IEnumerable<Item>>> ( 100, Mine )
            };
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


        public IEnumerable<Item> ShotgunItem()
        {
            return new List<Item> { new Items.Shotgun() };
        }

        public IEnumerable<Item> AxeItem()
        {
            return new List<Item> { new Items.Axe() };
        }

        public IEnumerable<Item> PoleItem()
        {
            return new List<Item> { new Items.Pole() };
        }

        public IEnumerable<Item> LaserItem()
        {
            return new List<Item> { new Items.Laser() };
        }

        public IEnumerable<Item> FragGrenade()
        {
            return new List<Item> { new Items.FragGrenade() };
        }

        public IEnumerable<Item> AssaultRifle()
        {
            return new List<Item> { new Items.AssaultRifle() };
        }

        public IEnumerable<Item> RocketLauncher()
        {
            return new List<Item> { new Items.RocketLauncher() };
        }

        public IEnumerable<Item> StunGrenade()
        {
            return new List<Item> { new Items.StunGrenade() };
        }

        public IEnumerable<Item> AcidGrenade()
        {
            return new List<Item> { new Items.StunGrenade() };
        }

        public IEnumerable<Item> SoundGrenade()
        {
            return new List<Item> { new Items.SoundGrenade() };
        }

        public IEnumerable<Item> Mine()
        {
            return new List<Item> { new Items.Mine() };
        }

        public Monster MonsterSwarmer(int level) 
        {
            return new Creatures.Swarmer(level);
        }

        public Monster MonsterExplosiveBarrel(int level)
        {
            return new Creatures.ExplosiveBarrel(level);
        }

        public Monster MonsterPunk(int level)
        {
            return new Creatures.Punk(level);
        }

        public Monster MonsterGrenadier(int level)
        {
            return new Creatures.Grenadier(level);
        }

        public Monster MonsterThug(int level)
        {
            return new Creatures.Thug(level);
        }

        public Monster MonsterPsycho(int level)
        {
            return new Creatures.Psycho(level);
        }

        public Monster MonsterHunter(int level)
        {
            return new Creatures.Hunter(level);
        }

        public Monster MonsterArenaMaster(int level)
        {
            return new Creatures.ArenaMaster(level);
        }

        public Monster MonsterJunkborg(int level)
        {
            return new Creatures.Junkborg(level);
        }



        public int LevelWithVariance(int level, double levelVariance)
        {
            return (int)Math.Round(level + (level * Gaussian.BoxMuller(0, levelVariance)));
        }

        public IEnumerable<Monster> MonsterType(int constant, int variable, double variance, int level, double levelVariance, Func<int, Monster> monsterGenerator)
        {
            var swarmerNo = (int)Math.Max(1, Math.Round(constant + (variable * Gaussian.BoxMuller(0, variance))));
            var levels = Enumerable.Range(0, swarmerNo).Select(i => LevelWithVariance(level, levelVariance));
            return levels.Select(l => monsterGenerator(l));
        }

        public IEnumerable<Monster> SwarmerSet(int level, double levelVariance)
        {
            return MonsterType(6, 3, 0.2, level, levelVariance, MonsterSwarmer);
        }

        public IEnumerable<Monster> ExplosiveBarrelSet(int level, double levelVariance)
        {
            return MonsterType(1, 0, 0.2, level, levelVariance, MonsterExplosiveBarrel);
        }

        public IEnumerable<Monster> PunkSet(int level, double levelVariance)
        {
            return MonsterType(3, 1, 0.2, level, levelVariance, MonsterPunk);
        }

        public IEnumerable<Monster> JunkborgSet(int level, double levelVariance)
        {
            var punks = MonsterType(3, 2, 0.2, level, levelVariance, MonsterPunk);
            var junkborg = MonsterType(1, 0, 0, level, levelVariance, MonsterJunkborg);
            return punks.Concat(junkborg);
        }

        public IEnumerable<Monster> GrenadierSet(int level, double levelVariance)
        {
            var punks = MonsterType(3, 3, 0.2, level, levelVariance, MonsterPunk);
            var grenadiers = MonsterType(1, 1, 0.2, level, levelVariance, MonsterGrenadier);

            return punks.Concat(grenadiers);
        }

        public IEnumerable<Monster> ThugSet(int level, double levelVariance)
        {
            var thugs = MonsterType(1, 1, 0.2, level, levelVariance, MonsterThug);
            var psychos = MonsterType(1, 1, 0.2, level, levelVariance, MonsterPsycho);

            return thugs.Concat(psychos);
        }

        public IEnumerable<Monster> PsychoSet(int level, double levelVariance)
        {
            var psychos = MonsterType(2, 2, 0.2, level, levelVariance, MonsterPsycho);

            return psychos;
        }

        public IEnumerable<Monster> HunterSet(int level, double levelVariance)
        {
            var hunters = MonsterType(1, 2, 0.2, level, levelVariance, MonsterHunter);

            return hunters;
        }

        public IEnumerable<Monster> ArenaMasterSet(int level, double levelVariance)
        {
            var master = MonsterType(1, 0, 0.2, level, levelVariance, MonsterArenaMaster);
            var punks = MonsterType(3, 2, 0.2, level, levelVariance, MonsterPunk);

            return master.Concat(punks);
        }


        public void CreateMonstersForLevels(MapInfo mapInfo, IEnumerable<Tuple<int, int>> levelsToProcess, IEnumerable<Point> entryPoints)
        {
            var allLevelData = levelsToProcess.Zip(entryPoints, Tuple.Create);

            foreach (var level in allLevelData)
            {
                var levelNo = level.Item1.Item1;
                var baseXPLevel = level.Item1.Item2;

                var arenaNo = (int)Math.Floor(levelNo / 3.0);

                var entryPoint = level.Item2;

                var levelVariance = 0.2;

                var levelMonsterXP = 0;
                var totalMonsterXP = 300;

                List<IEnumerable<Monster>> monsterSets = new List<IEnumerable<Monster>>();

                while(levelMonsterXP < totalMonsterXP) {

                    var generatorToUse = ChooseItemFromWeights(monsterGenerators[arenaNo]);
                    var monsterSetGenerated = generatorToUse(baseXPLevel, levelVariance);

                    levelMonsterXP += monsterSetGenerated.Select(m => m.GetCombatXP()).Sum();

                    monsterSets.Add(monsterSetGenerated);
                }

                //Random chance for some barrels

                var random = Game.Random.Next(4);

                if (random < 1)
                {
                    var numBarrels = 3 + Game.Random.Next(6);

                    for (int i = 0; i < numBarrels;i++ )
                        monsterSets.Add(ExplosiveBarrelSet(baseXPLevel, levelVariance));
                }

                LogFile.Log.LogEntryDebug("Placing total monster XP (base)" + levelMonsterXP, LogDebugLevel.Medium);
                //End boss
                if (Game.Dungeon.ArenaLevelNumber() == 4)
                {
                    monsterSets.Add(ArenaMasterSet(baseXPLevel, levelVariance));
                }

                var monsterSetPositions = PlaceMonsterSets(mapInfo, levelNo, monsterSets, entryPoint);
                var items = GetRandomsItemForPlayer(8);
                PlaceItemSets(mapInfo, levelNo, monsterSetPositions, items);
            }

            
        }

        private IEnumerable<IEnumerable<Item>> GetRandomsItemForPlayer(int numberOfItems)
        {
            var player = Game.Dungeon.Player;
            IEnumerable<IEnumerable<Item>> items = new List<List<Item>>();

            
            var itemTypesToGenerate = Enumerable.Repeat(0, numberOfItems).Select(x => ChooseItemFromWeights(classItemCategories[player.PlayerClass]));
            var weightedGenerators = itemTypesToGenerate.Select(itemType => itemGenerators[itemType]);
            var generatorsToUse = weightedGenerators.Select(weightedGen => ChooseItemFromWeights(weightedGen));
            items = generatorsToUse.Select(gen => gen());
            
            //items = AddExtraItemsToSets(numberOfItems, items, ChooseItemFromWeights(classItemCategories[player.PlayerClass]));

            //Add extra utilities for the nerd
            if (player.PlayerClass == PlayerClass.Sneaker)
            {
                var extraItemNumber = numberOfItems / 2;
                items = AddExtraItemsToSets(extraItemNumber, items, ItemType.NerdExtras);               

                LogFile.Log.LogEntryDebug("Adding extra utility items: " + extraItemNumber, LogDebugLevel.Medium);
            }

            if (player.PlayerClass == PlayerClass.Athlete)
            {
                var extraItemNumber = numberOfItems / 3;
                items = AddExtraItemsToSets(extraItemNumber, items, ItemType.MeleeExtras);

                LogFile.Log.LogEntryDebug("Adding extra melee items: " + extraItemNumber, LogDebugLevel.Medium);
            }

            if (player.PlayerClass == PlayerClass.Gunner)
            {
                var extraItemNumber = numberOfItems / 2;
                items = AddExtraItemsToSets(extraItemNumber, items, ItemType.RangedExtras);

                LogFile.Log.LogEntryDebug("Adding extra ranged items: " + extraItemNumber, LogDebugLevel.Medium);
            }

            return items;
        }

        private IEnumerable<IEnumerable<Item>> AddExtraItemsToSets(int noExtraItems, IEnumerable<IEnumerable<Item>> originalItems, ItemType extraItemTypes)
        {
            if (noExtraItems > originalItems.Count())
            {
                LogFile.Log.LogEntryDebug("Too many extra items, will not be added", LogDebugLevel.High);
            }

            var itemTypesToGenerate = Enumerable.Repeat(0, noExtraItems).Select(x => extraItemTypes);
            var weightedGenerators = itemTypesToGenerate.Select(itemType => itemGenerators[itemType]);
            var generatorsToUse = weightedGenerators.Select(weightedGen => ChooseItemFromWeights(weightedGen));
            var extraItems = generatorsToUse.Select(gen => gen());

            //Add to existing sets, preserving items that are already there
            var extraItemsAdded = originalItems.Zip(extraItems, (a, b) => a.Concat(b));
            var allItems = extraItemsAdded.Concat(originalItems.Skip(extraItems.Count()));
            return allItems;
        }


        private void PlaceItemSets(MapInfo mapInfo, int levelNo, List<Point> monsterSetPositions, IEnumerable<IEnumerable<Item>> itemSetsToPlace)
        {

            //var itemsAndPlaces = monsterSetPositions.Zip(itemSetsToPlace, (f, s) => new Tuple<Point, IEnumerable<Item>>(f, s));

            //Place a couple of random items near each group
            int counter = 0;
            foreach (var itemsToPlace in itemSetsToPlace)
            {
                //var origin = pointAndItem.Item1;
                //var itemsToPlace = pointAndItem.Item2;

                if (counter >= monsterSetPositions.Count())
                    counter = 0;

                Point origin = monsterSetPositions.ElementAt(counter);

                var candidateSquares = Game.Dungeon.GetWalkableSquaresFreeOfCreaturesWithinRange(levelNo, origin, 3, 7);
                var squaresToPlace = candidateSquares.Shuffle();

                int itemsPlaced = 0;

                foreach (Point p in squaresToPlace)
                {
                    bool placedSuccessfully = Game.Dungeon.AddItem(itemsToPlace.ElementAt(itemsPlaced), levelNo, p);

                    if (placedSuccessfully)
                    {
                        LogFile.Log.LogEntryDebug("Placing item " + itemsToPlace.ElementAt(itemsPlaced) + " at " + p + " close to set at " + origin, LogDebugLevel.Medium);

                        itemsPlaced++;

                        if (itemsPlaced == itemsToPlace.Count())
                            break;
                    }
                }
                counter++;
            }
        }

        private List<Point> PlaceMonsterSets(MapInfo mapInfo, int levelNo, IEnumerable<IEnumerable<Monster>> monsterSetsGenerated, Point playerStart)
        {
            var monsterSetPositions = new List<Point>();

            //Get points in rooms
            var allRoomsAndCorridors = mapInfo.GetRoomIndicesForLevel(levelNo);//.Except(new List<int> { mapInfo.StartRoom });
            
            var rooms = mapInfo.FilterOutCorridors(allRoomsAndCorridors).ToList();
            
            var candidatePointsInRooms = rooms.Select(room => mapInfo.GetAllPointsInRoomOfTerrain(room, RoomTemplateTerrain.Floor));
            
            var roomsAndPointsInRooms = rooms.Zip(candidatePointsInRooms, Tuple.Create);

            //Distribution of sets amongst rooms, mostly evenly, scaled by room size
            //(variance disabled for now)
            var roomMonsterRatio = roomsAndPointsInRooms.Select(rp => Math.Max(0, Gaussian.BoxMuller(1, 0)) * rp.Item2.Count());
            double totalMonsterRatio = roomMonsterRatio.Sum();

            var noMonsterSets = monsterSetsGenerated.Count();

            double ratioToTotalMonsterBudget = noMonsterSets / totalMonsterRatio;
            int noRooms = rooms.Count();

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
            }

            if (remainder > 0.5)
            {
                monstersPerRoom[0]++;
            }

            //Calculate actual number of monster sets placed

            int totalMonsters = monstersPerRoom.Sum();
            LogFile.Log.LogEntryDebug("Total monsters actually placed (level: " + levelNo + "): " + monstersPerRoom.Sum(), LogDebugLevel.Medium);

            //Place monsters in rooms

            Dungeon dungeon = Game.Dungeon;
            var totalSetNo = 0;

            var pointsCloseToPlayer = Game.Dungeon.GetValidMapSquaresWithinRange(levelNo, playerStart, 6);
                    
            for (int r = 0; r < noRooms; r++)
            {
                int monsterSetsToPlaceInRoom = monstersPerRoom[r];

                var longShortRatio = 4 / 3.0;
                var longDivision = Math.Sqrt(longShortRatio * monsterSetsToPlaceInRoom);
                var shortDivison = longDivision / longShortRatio;

                var regularGridOfCentres = RoyaleDungeonLevelMaker.DivideRoomIntoCentres(mapInfo.Room(rooms.ElementAt(r)).Room, (int)Math.Ceiling(longDivision), (int)Math.Ceiling(shortDivison), 0.3, new Point(2,2));
                var regularGridOfCentresMapCoordsUnsafe = regularGridOfCentres.Select(p => mapInfo.Room(rooms.ElementAt(r)).Location + p);
                var regularGridOfCentresMapCoords = regularGridOfCentresMapCoordsUnsafe.Except(pointsCloseToPlayer).Shuffle();


                for (int setNo = 0; setNo < monstersPerRoom[r]; setNo++)
                {
                    var candidatePointsInRoom = roomsAndPointsInRooms.ElementAt(r).Item2;
                    var safeCandidatePointsInRoom = candidatePointsInRoom.Except(pointsCloseToPlayer);

                    Point origin = safeCandidatePointsInRoom.First();

                    if (setNo < regularGridOfCentresMapCoords.Count())
                    {
                        origin = regularGridOfCentresMapCoords.ElementAt(setNo);
                    }

                    var candidatePointsFromFirstPoint = candidatePointsInRoom.OrderBy(pt => Utility.GetDistanceBetween(origin, pt));

                    List<Point> candidatePointsSpaced = new List<Point>();

                    var monsterSet = monsterSetsGenerated.ElementAt(totalSetNo);
                    totalSetNo++;
                    var monsterNo = 0;

                    LogFile.Log.LogEntryDebug("Placing monster set " + totalSetNo + " in room " + r + " on level " + levelNo, LogDebugLevel.High);

                    //Place monsters around this point
                    foreach (var p in candidatePointsFromFirstPoint)
                    {
                        //Finished
                        if (monsterNo == monsterSet.Count())
                            break;

                        var thisMonster = monsterSet.ElementAt(monsterNo);

                        bool placedSuccessfully = Game.Dungeon.AddMonster(thisMonster, levelNo, p);
                        if (placedSuccessfully)
                        {
                            if (monsterNo == 0)
                            {
                                //Make a note of the location of the first monster in each set
                                monsterSetPositions.Add(p);
                            }
                            monsterNo++;

                        }
                    }

                    if (monsterNo != monsterSet.Count())
                    {
                        LogFile.Log.LogEntryDebug("Unable to place " + (monsterSet.Count() - monsterNo) + " monsters from set " + totalSetNo, LogDebugLevel.High);
                    }
                }
            }

            if (totalSetNo != monsterSetsGenerated.Count())
            {

                LogFile.Log.LogEntryDebug("Unable to place " + (monsterSetsGenerated.Count() - totalSetNo) + " sets on level " + levelNo, LogDebugLevel.High);
            }

            return monsterSetPositions;
        }
    }
}
