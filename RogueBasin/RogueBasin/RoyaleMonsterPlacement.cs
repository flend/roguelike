using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RogueBasin
{
    class RoyaleMonsterPlacement
    {
        List<Func<int, double, IEnumerable<Monster>>> monsterGenerators = new List<Func<int, double, IEnumerable<Monster>>>();
        List<Func<IEnumerable<Item>>> itemGenerators = new List<Func<IEnumerable<Item>>>();

        public RoyaleMonsterPlacement()
        {
            SetupMonsterGenerators();
            SetupItemGenerators();
        }

        private void SetupMonsterGenerators()
        {
            monsterGenerators.Add(SwarmerSet);
            monsterGenerators.Add(GrenadierSet);
            monsterGenerators.Add(ThugSet);
        }

        private void SetupItemGenerators()
        {
            itemGenerators.Add(ShotgunItem);
            itemGenerators.Add(AxeItem);
            itemGenerators.Add(LaserItem);
            itemGenerators.Add(FragGrenade);

        }

        public IEnumerable<Item> ShotgunItem()
        {
            return new List<Item> { new Items.Shotgun() };
        }

        public IEnumerable<Item> AxeItem()
        {
            return new List<Item> { new Items.Axe() };
        }

        public IEnumerable<Item> LaserItem()
        {
            return new List<Item> { new Items.Laser() };
        }

        public IEnumerable<Item> FragGrenade()
        {
            return new List<Item> { new Items.FragGrenade() };
        }

        public IEnumerable<Item> StunGrenade()
        {
            return new List<Item> { new Items.StunGrenade() };
        }

        public IEnumerable<Item> SoundGrenade()
        {
            return new List<Item> { new Items.SoundGrenade() };
        }

        public int LevelWithVariance(int level, double levelVariance)
        {
            return (int)Math.Round(level + (level * Gaussian.BoxMuller(0, levelVariance)));
        }

        public IEnumerable<Monster> MonsterType(int constant, int variable, double variance, int level, double levelVariance, Monster monster)
        {
            var swarmerNo = (int)Math.Round(constant + (variable * Gaussian.BoxMuller(0, variance)));
            var levels = Enumerable.Range(0, swarmerNo).Select(i => LevelWithVariance(level, levelVariance));
            return levels.Select(l => monster.NewCreatureOfThisType());
        }

        public IEnumerable<Monster> SwarmerSet(int level, double levelVariance)
        {
            return MonsterType(5, 3, 0.2, level, levelVariance, new RogueBasin.Creatures.Swarmer(1));
        }

        public IEnumerable<Monster> GrenadierSet(int level, double levelVariance)
        {
            var punks = MonsterType(3, 3, 0.2, level, levelVariance, new RogueBasin.Creatures.Punk(1));
            var grenadiers = MonsterType(1, 1, 0.2, level, levelVariance, new RogueBasin.Creatures.Grenadier(1));

            return punks.Concat(grenadiers);
        }

        public IEnumerable<Monster> ThugSet(int level, double levelVariance)
        {
            var thugs = MonsterType(1, 1, 0.2, level, levelVariance, new RogueBasin.Creatures.Thug(1));
            var psychos = MonsterType(1, 1, 0.2, level, levelVariance, new RogueBasin.Creatures.Psycho(1));

            return thugs.Concat(psychos);
        }


        public void CreateMonstersForLevels(MapInfo mapInfo, IEnumerable<Tuple<int, int>> levelsToProcess)
        {
            foreach (var level in levelsToProcess)
            {
                var levelNo = level.Item1;
                var baseXPLevel = level.Item2;
                var levelVariance = 0.2;

                var levelMonsterXP = 0;
                var totalMonsterXP = 300;

                List<IEnumerable<Monster>> monsterSets = new List<IEnumerable<Monster>>();

                while(levelMonsterXP < totalMonsterXP) {
                    var generatorToUse = monsterGenerators.RandomElement();

                    var monsterSetGenerated = generatorToUse(baseXPLevel, levelVariance);

                    levelMonsterXP += monsterSetGenerated.Select(m => m.GetCombatXP()).Sum();

                    monsterSets.Add(monsterSetGenerated);
                }

                LogFile.Log.LogEntryDebug("Placing total monster XP (base)" + levelMonsterXP, LogDebugLevel.Medium);

                var monsterSetPositions = PlaceMonsterSets(mapInfo, levelNo, monsterSets);

                PlaceItemSets(mapInfo, levelNo, monsterSetPositions);
            }
        }


        private void PlaceItemSets(MapInfo mapInfo, int levelNo, List<Point> monsterSetPositions)
        {
            //Place a couple of random items near each group
            MessageBox.Show("Stop");
            foreach(Point origin in monsterSetPositions) {
                var candidateSquares = Game.Dungeon.GetWalkableSquaresFreeOfCreaturesWithinRange(levelNo, origin, 3, 7);
                var squaresToPlace = candidateSquares.Shuffle();

                int numberOfItems = 1;
                for (int i = 0; i < numberOfItems; i++)
                {
                    var generatorToUse = itemGenerators.RandomElement();
                    var itemToPlace = generatorToUse();

                    int itemsPlaced = 0;

                    foreach (Point p in squaresToPlace)
                    {
                        bool placedSuccessfully = Game.Dungeon.AddItem(itemToPlace.ElementAt(itemsPlaced), levelNo, p);

                        if (placedSuccessfully)
                        {
                            LogFile.Log.LogEntryDebug("Placing item " + itemToPlace.ElementAt(itemsPlaced) + " at " + p + " close to set at " + origin, LogDebugLevel.Medium);

                            itemsPlaced++;

                            if (itemsPlaced == itemToPlace.Count())
                                break;
                        }
                    }
                }
            }
        }

        private List<Point> PlaceMonsterSets(MapInfo mapInfo, int levelNo, IEnumerable<IEnumerable<Monster>> monsterSetsGenerated)
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

            for (int r = 0; r < noRooms; r++)
            {
                int monsterSetsToPlaceInRoom = monstersPerRoom[r];

                for (int setNo = 0; setNo < monstersPerRoom[r]; setNo++)
                {

                    var candidatePointsInRoom = roomsAndPointsInRooms.ElementAt(r).Item2.Shuffle();
                    Point origin = candidatePointsInRoom.First();
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
