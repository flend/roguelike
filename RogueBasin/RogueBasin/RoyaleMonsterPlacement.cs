﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RogueBasin
{
    class RoyaleMonsterPlacement
    {
        List<Func<int, double, IEnumerable<Monster>>> monsterGenerators = new List<Func<int, double, IEnumerable<Monster>>>();

        public RoyaleMonsterPlacement()
        {
            SetupMonsterGenerators();
        }

        private void SetupMonsterGenerators()
        {
            monsterGenerators.Add(SwarmerSet);
        }

        public int LevelWithVariance(int level, double levelVariance)
        {
            return (int)Math.Round(level + (level * Gaussian.BoxMuller(0, levelVariance)));
        }

        public IEnumerable<Monster> SwarmerSet(int level, double levelVariance)
        {
            var swarmerNo = (int)Math.Round(5 + (3 * Gaussian.BoxMuller(0, 0.2)));
            var levels = Enumerable.Range(0, swarmerNo).Select(i => LevelWithVariance(level, levelVariance));
            return levels.Select(l => new RogueBasin.Creatures.Swarmer(l));
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

                PlaceMonsterSets(mapInfo, levelNo, monsterSets);
            }
        }

        private void PlaceMonsterSets(MapInfo mapInfo, int levelNo, IEnumerable<IEnumerable<Monster>> monsterSetsGenerated)
        {
            //Get points in rooms
            var allRoomsAndCorridors = mapInfo.GetRoomIndicesForLevel(levelNo);//.Except(new List<int> { mapInfo.StartRoom });
            var rooms = mapInfo.FilterOutCorridors(allRoomsAndCorridors).ToList();
            var candidatePointsInRooms = rooms.Select(room => mapInfo.GetAllPointsInRoomOfTerrain(room, RoomTemplateTerrain.Floor));
            var roomsAndPointsInRooms = rooms.Zip(candidatePointsInRooms, Tuple.Create);

            //Distribution of sets amongst rooms, mostly evenly, scaled by room size
            MessageBox.Show("stop");
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

                for (int setNo = 0; setNo < monstersPerRoom[r]; setNo++) {

                    var candidatePointsInRoom = roomsAndPointsInRooms.ElementAt(r).Item2.Shuffle();
                    Point origin = candidatePointsInRoom.First();
                    var candidatePointsFromFirstPoint = candidatePointsInRoom.OrderBy(pt => Utility.GetDistanceBetween(origin, pt));

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
        }
    }
}
