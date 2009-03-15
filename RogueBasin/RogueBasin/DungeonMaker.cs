using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    class LevelOfMonsters
    {
        public List<Monster> monsterList;

        public LevelOfMonsters()
        {
            monsterList = new List<Monster>();
        }

        public Monster GetRandomMonster()
        {
            Monster randomMonster = monsterList[Game.Random.Next(monsterList.Count)];

            string randomMonsterName = randomMonster.SingleDescription;

            switch (randomMonsterName)
            {
                case "ferret":
                    return new Creatures.Ferret();
                case "goblin":
                    return new Creatures.Goblin();
                case "goblin witch":
                    return new Creatures.GoblinWitchdoctor();
                case "necromancer":
                    return new Creatures.Necromancer();
                case "orc":
                    return new Creatures.Orc();
                case "orc shaman":
                    return new Creatures.OrcShaman();
                case "rat":
                    return new Creatures.Rat();
                case "skeleton":
                    return new Creatures.Skeleton();
                case "spider":
                    return new Creatures.Spider();
                case "zombie":
                    return new Creatures.Zombie();
            }

            LogFile.Log.LogEntryDebug("Can't find monster type: " + randomMonster, LogDebugLevel.High);
            return null;
        }

        internal void Add(Monster monster)
        {
            monsterList.Add(monster);
        }
    }

    public enum GameDifficulty
    {
        Easy, Medium, Hard
    }

    public class DungeonMaker
    {
        Dungeon dungeon = null;

        GameDifficulty difficulty = GameDifficulty.Easy;

        int noCaveLevels = 5;
        int noCaveWaterLevels = 3;

        int noRuinedLevels = 5;
        int ruinedExtraCorridorDefinite = 5;
        int ruinedExtraCorridorRandom = 10;

        int noHallLevels = 5;
        int hallsExtraCorridorDefinite = 0;
        int hallsExtraCorridorRandom = 8;

        int plotItemOnMonsterChance = 50;

        public DungeonMaker() {}

        /// <summary>
        /// Big function that spawns and fills a new dungeon
        /// </summary>
        /// <returns></returns>
        public Dungeon SpawnNewDungeon()
        {
            dungeon = new Dungeon();
            Game.Dungeon = dungeon; //not classy but I have to do it here since some other classes (e.g. map gen) call it

            SetupPlayer();

            SetupMaps();

            SpawnInitialCreatures();

            SpawnItems();

            Game.Dungeon.TotalPlotItems = 5;

            SpawnUniques();

            Game.Dungeon.TimeToRescueFriend = 1000000;


            return dungeon;
        }

        private void SpawnUniques()
        {
            int outsideLevel = dungeon.NoLevels - 1;
            int battleLevel = dungeon.NoLevels - 2;


        }

        private void SetupPlayer()
        {
            Player player = dungeon.Player;

            player.Representation = '@';

            player.Hitpoints = 100;
            player.MaxHitpoints = 100;

        }

        private void SpawnInitialCreatures()
        {
            LogFile.Log.LogEntry("Generating creatures...");

            //Add monsters to levels

            //The levels divide into 3 groups: cave, cave/halls and halls
            //The difficulty in each set is roughly the same

            //1-3: with water
            //1-5: Cave
            //5-10: Ruined Halls
            //11-15: Halls

            //Could take into account depth and difficulty level

            List<int> levelMonsterAmounts = new List<int>() {

                200, //1
                220, //2
                240, //3
                260, //4
                300, //5
                320, //6
                340, //7
                360, //8
                400, //9
                440, //10
                480, //11
                520, //12
                560, //13
                600, //14
                750 //15
            };

            //Monster Types
            Creatures.Ferret ferret = new RogueBasin.Creatures.Ferret();
            Creatures.Goblin goblin = new RogueBasin.Creatures.Goblin();
            Creatures.GoblinWitchdoctor goblinWitch = new RogueBasin.Creatures.GoblinWitchdoctor();
            Creatures.Necromancer necromancer = new RogueBasin.Creatures.Necromancer();
            Creatures.Orc orc = new RogueBasin.Creatures.Orc();
            Creatures.OrcShaman orcShaman = new RogueBasin.Creatures.OrcShaman();
            Creatures.Rat rat = new RogueBasin.Creatures.Rat();
            Creatures.Skeleton skeleton = new RogueBasin.Creatures.Skeleton();
            Creatures.Spider spider = new RogueBasin.Creatures.Spider();
            Creatures.Zombie zombie = new RogueBasin.Creatures.Zombie();

            List<Monster> allMonsters = new List<Monster>() {
                new RogueBasin.Creatures.Ferret(),
                new RogueBasin.Creatures.Goblin(),
                new RogueBasin.Creatures.GoblinWitchdoctor(),
                new RogueBasin.Creatures.Necromancer(),
                new RogueBasin.Creatures.Orc(),
                new RogueBasin.Creatures.OrcShaman(),
                new RogueBasin.Creatures.Rat(),
                new RogueBasin.Creatures.Skeleton(),
                new RogueBasin.Creatures.Spider(),
                new RogueBasin.Creatures.Zombie()
            };

            List<LevelOfMonsters> levelList = new List<LevelOfMonsters>();
            for (int i = 0; i < 5; i++)
            {
                levelList.Add(new LevelOfMonsters());
            }

            //Arrange the monsters into levels of difficulty
            foreach (Monster monster in allMonsters)
            {
                levelList[monster.CreatureLevel() - 1].Add(monster);
            }

            if (difficulty == GameDifficulty.Medium)
            {
                for (int i = 0; i < levelMonsterAmounts.Count; i++)
                {
                    levelMonsterAmounts[i] = (int)(levelMonsterAmounts[i] * 1.5);
                }
            }

            if (difficulty == GameDifficulty.Hard)
            {
                for (int i = 0; i < levelMonsterAmounts.Count; i++)
                {
                    levelMonsterAmounts[i] *= (int)(levelMonsterAmounts[i] * 2.5);
                }
            }

            //Don't auto spawn for the last 2 levels
            for (int i = 0; i < dungeon.NoLevels - 2; i++)
            {
                int randomNum;

                //Switch on dungeon level
                switch (i)
                {
                        //Early caves
                    case 0:
                    case 1:
                    case 2:

                        int costSpent = 0;

                        do
                        {
                            randomNum = Game.Random.Next(100);

                            Monster monsterToAdd = null;

                            if (randomNum < 75)
                            {
                                monsterToAdd = levelList[0].GetRandomMonster();
                            }
                            else if (randomNum < 98)
                            {
                                monsterToAdd = levelList[1].GetRandomMonster();
                            }
                            else
                                monsterToAdd = levelList[2].GetRandomMonster();

                            Point location;

                            do
                            {
                                location = dungeon.RandomWalkablePointInLevel(i);
                            } while (!dungeon.AddMonster(monsterToAdd, i, location));

                            CheckSpecialMonsterGroups(monsterToAdd, i);

                            costSpent += monsterToAdd.CreatureCost();

                        } while (costSpent < levelMonsterAmounts[i]);

                        break;
                    //Late caves
                    case 3:
                    case 4:
                    case 5:

                        costSpent = 0;

                        do
                        {
                            randomNum = Game.Random.Next(100);

                            Monster monsterToAdd = null;

                            if (randomNum < 50)
                            {
                                monsterToAdd = levelList[0].GetRandomMonster();
                            }
                            else if (randomNum < 75)
                            {
                                monsterToAdd = levelList[1].GetRandomMonster();
                            }
                            else if(randomNum < 98)
                                monsterToAdd = levelList[2].GetRandomMonster();
                            else
                                monsterToAdd = levelList[3].GetRandomMonster();

                            Point location;

                            do
                            {
                                location = dungeon.RandomWalkablePointInLevel(i);
                            } while (!dungeon.AddMonster(monsterToAdd, i, location));

                            CheckSpecialMonsterGroups(monsterToAdd, i);

                            costSpent += monsterToAdd.CreatureCost();

                        } while (costSpent < levelMonsterAmounts[i]);

                        break;
                    //Early cave/hall
                    case 6:
                    case 7:
                    case 8:

                        costSpent = 0;

                        do
                        {
                            randomNum = Game.Random.Next(100);

                            Monster monsterToAdd = null;

                            if (randomNum < 15)
                            {
                                monsterToAdd = levelList[0].GetRandomMonster();
                            }
                            else if (randomNum < 40)
                            {
                                monsterToAdd = levelList[1].GetRandomMonster();
                            }
                            else if (randomNum < 90)
                                monsterToAdd = levelList[2].GetRandomMonster();
                            else
                                monsterToAdd = levelList[3].GetRandomMonster();

                            Point location;

                            do
                            {
                                location = dungeon.RandomWalkablePointInLevel(i);
                            } while (!dungeon.AddMonster(monsterToAdd, i, location));

                            CheckSpecialMonsterGroups(monsterToAdd, i);

                            costSpent += monsterToAdd.CreatureCost();

                        } while (costSpent < levelMonsterAmounts[i]);

                        break;

                    //Late cave/hall
                    case 9:
                    case 10:

                        costSpent = 0;

                        do
                        {
                            randomNum = Game.Random.Next(100);

                            Monster monsterToAdd = null;

                            if (randomNum < 5)
                            {
                                monsterToAdd = levelList[0].GetRandomMonster();
                            }
                            else if (randomNum < 20)
                            {
                                monsterToAdd = levelList[1].GetRandomMonster();
                            }
                            else if (randomNum < 50)
                                monsterToAdd = levelList[2].GetRandomMonster();
                            else if(randomNum < 90)
                                monsterToAdd = levelList[3].GetRandomMonster();
                            else
                                monsterToAdd = levelList[4].GetRandomMonster();

                            Point location;

                            do
                            {
                                location = dungeon.RandomWalkablePointInLevel(i);
                            } while (!dungeon.AddMonster(monsterToAdd, i, location));

                            CheckSpecialMonsterGroups(monsterToAdd, i);

                            costSpent += monsterToAdd.CreatureCost();

                        } while (costSpent < levelMonsterAmounts[i]);
                        break;
                        //Early halls
                    case 11:
                    case 12:
                        case 13:

                        costSpent = 0;

                        do
                        {
                            randomNum = Game.Random.Next(100);

                            Monster monsterToAdd = null;

                            if (randomNum < 5)
                            {
                                monsterToAdd = levelList[0].GetRandomMonster();
                            }
                            else if (randomNum < 10)
                            {
                                monsterToAdd = levelList[1].GetRandomMonster();
                            }
                            else if (randomNum < 20)
                                monsterToAdd = levelList[2].GetRandomMonster();
                            else if(randomNum < 70)
                                monsterToAdd = levelList[3].GetRandomMonster();
                            else
                                monsterToAdd = levelList[4].GetRandomMonster();

                            Point location;

                            do
                            {
                                location = dungeon.RandomWalkablePointInLevel(i);
                            } while (!dungeon.AddMonster(monsterToAdd, i, location));

                            CheckSpecialMonsterGroups(monsterToAdd, i);

                            costSpent += monsterToAdd.CreatureCost();

                        } while (costSpent < levelMonsterAmounts[i]);
                        break;
                             //Late halls
                    case 14:
                    case 15:

                        costSpent = 0;

                        do
                        {
                            randomNum = Game.Random.Next(100);

                            Monster monsterToAdd = null;

                            if (randomNum < 2)
                            {
                                monsterToAdd = levelList[0].GetRandomMonster();
                            }
                            else if (randomNum < 5)
                            {
                                monsterToAdd = levelList[1].GetRandomMonster();
                            }
                            else if (randomNum < 10)
                                monsterToAdd = levelList[2].GetRandomMonster();
                            else if(randomNum < 40)
                                monsterToAdd = levelList[3].GetRandomMonster();
                            else
                                monsterToAdd = levelList[4].GetRandomMonster();

                            Point location;

                            do
                            {
                                location = dungeon.RandomWalkablePointInLevel(i);
                            } while (!dungeon.AddMonster(monsterToAdd, i, location));

                            CheckSpecialMonsterGroups(monsterToAdd, i);

                            costSpent += monsterToAdd.CreatureCost();

                        } while (costSpent < levelMonsterAmounts[i]);
                        break;
                }

            }

            /*
                            int noCreatures = 25 + Game.Random.Next(5);

                            for (int j = 0; j < noCreatures; j++)
                            {
                                Monster monster;
                                if (Game.Random.Next(8) < 6)
                                    monster = new Creatures.Rat();
                                else
                                    monster = new Creatures.GoblinWitchdoctor();

                                Point location = new Point(0, 0);

                                //Find an acceptable location (walkable and with no other creatures in it)
                                //Note that there is no guarantee of connectivity on cave squares
                                do
                                {
                                    location = dungeon.RandomWalkablePointInLevel(i);
                                } while (!dungeon.AddMonster(monster, i, location));
                            }
                        }*/

        }

        /// <summary>
        /// Slightly unsafe due to infinite loop but not a big deal if it fails
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="levelToPlace"></param>
        private void CheckSpecialMonsterGroups(Monster monster, int levelToPlace)
        {
            int minDistance = 8;
            int loopCount = 0;

            Point location = new Point();

            //Certain monsters spawn in with groups of their friends
            if (monster is Creatures.GoblinWitchdoctor)
            {
                //Spawn in with a random number of ferrets & goblins
                int noFerrets = 1 + Game.Random.Next(4);
                int noGoblins = 1 + Game.Random.Next(2);

                for (int i = 0; i < noFerrets; i++)
                {
                    do {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < 50);
                    } while (!dungeon.AddMonster(new Creatures.Ferret(), levelToPlace, location));
                }

                for (int i = 0; i < noGoblins; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < 50);
                    } while (!dungeon.AddMonster(new Creatures.Goblin(), levelToPlace, location));
                }
            }
            else if (monster is Creatures.OrcShaman)
            {
                //Spawn in with a random number of orcs & spiders
                int noOrcs = 1 + Game.Random.Next(4);
                int noSpiders = 0 + Game.Random.Next(2);

                for (int i = 0; i < noOrcs; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < 50);
                    } while (!dungeon.AddMonster(new Creatures.Orc(), levelToPlace, location));
                }

                for (int i = 0; i < noSpiders; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < 50);
                    } while (!dungeon.AddMonster(new Creatures.Spider(), levelToPlace, location));
                }
            }

            else if (monster is Creatures.Necromancer)
            {
                //Spawn in with a random number of skels & zombs
                int noSkel = 1 + Game.Random.Next(4);
                int noZomb = 1 + Game.Random.Next(3);

                for (int i = 0; i < noSkel; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < 50);
                    } while (!dungeon.AddMonster(new Creatures.Skeleton(), levelToPlace, location));
                }

                for (int i = 0; i < noZomb; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < 50);
                    } while (!dungeon.AddMonster(new Creatures.Zombie(), levelToPlace, location));
                }
            }
        }
        private void SpawnItems()
        {
            LogFile.Log.LogEntry("Generating items...");

            Point location = new Point(0, 0);
            
            //Plot items

            //These are max 1 per level
            //Not all of them necessarily appear in all games
            //They may be on the ground or carried by a creature

            //Guarantee the glove (vamparic regeneration) on level 1 or 2
            
            int gloveLevel = Game.Random.Next(2);
            /*
            do
            {
                location = dungeon.RandomWalkablePointInLevel(gloveLevel);
            } while (!dungeon.AddItem(new Items.Glove(), 0, location));
            */
            dungeon.AddItem(new Items.Glove(), 0, dungeon.Player.LocationMap);
            //The rest of the plot items are split between the remaining cave and ruined levels

            List<Item> plotItems = new List<Item> { new Items.Badge(), new Items.Band(), new Items.Book(), new Items.Boots(), new Items.Bracer(), new Items.GlassGem(),
            new Items.Greaves(), new Items.LeadRing(), new Items.Lockpicks(), new Items.Sash(), new Items.Backpack() };
            
            int level = 0;
            List<int> levelsWithPlotItems = new List<int> { gloveLevel };

            foreach (Item plotItem in plotItems)
            {
                do
                {
                    location = dungeon.RandomWalkablePointInLevel(level);

                    //May want to specify a minimum distance from staircases??? TODO
                } while (!dungeon.AddItem(plotItem, 0, location));
            }


            /*
            //debug
            //Give them all to me!

            foreach (Item item in plotItems)
            {
                dungeon.Player.PickUpItem(item);
            }
            */
            //Stick them all on the first level


            /*
            int level = 0;
            List<int> levelsWithPlotItems = new List<int> { gloveLevel };

            foreach (Item plotItem in plotItems)
            {
                //Find random level w/o plotItem
                do
                {
                    level = Game.Random.Next(noCaveLevels + noRuinedLevels);
                } while (levelsWithPlotItems.Contains(level));

                levelsWithPlotItems.Add(level);

                //50% chance they will be generated on a monster
                bool putOnMonster = false;

                if(Game.Random.Next(100) < plotItemOnMonsterChance)
                    putOnMonster = true;

                if (putOnMonster)
                {
                    //On a monster

                    //Find a random monster on this level
                    Monster monster = dungeon.RandomMonsterOnLevel(level);

                    //If no monster, it'll go on the floor
                    if (monster == null)
                    {
                        putOnMonster = false;
                    }

                    //Give it to him!
                    monster.PickUpItem(plotItem);
                }

                if(!putOnMonster)
                {
                    //On the floor
                    //Find position in level and place item
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(level);

                        //May want to specify a minimum distance from staircases??? TODO
                    } while (!dungeon.AddItem(plotItem, level, location));
                }
            }*/
            
            //Potions

            //Stick 5 potions on level 1 for testing
            level = 0;
            for (int i = 0; i < 10; i++)
            {
                do
                {
                    location = dungeon.RandomWalkablePointInLevel(level);

                    //May want to specify a minimum distance from staircases??? TODO
                } while (!dungeon.AddItem(new Items.Potion(), 0, location));
            }
        }

        /// <summary>
        /// Adds levels and interconnecting staircases
        /// </summary>
        private void SetupMaps()
        {
            //Levels

            //1-3: with water
            //1-5: Cave
            //5-10: Ruined Halls
            //11-15: Halls
            //16: Final encounter (ASCIIPaint)
            //17: Outside battleground (ASCIIPaint)

            

            //Make the generators

            MapGeneratorCave caveGen = new MapGeneratorCave();
            MapGeneratorBSPCave ruinedGen = new MapGeneratorBSPCave();
            MapGeneratorBSP hallsGen = new MapGeneratorBSP();
            MapGeneratorFromASCIIFile asciiGen = new MapGeneratorFromASCIIFile();

            //Set width height of all maps to 80 / 25
            caveGen.Width = 80;
            caveGen.Height = 25;

            ruinedGen.Width = 80;
            ruinedGen.Height = 25;

            hallsGen.Width = 80;
            hallsGen.Height = 25;

            //Generate and add cave levels

            //First level is a bit different
            //Has a PC start location and only a downstaircase
            
            caveGen.GenerateMap(true);
            caveGen.AddWaterToCave(15, 5);
            int levelNo = dungeon.AddMap(caveGen.Map);
            caveGen.AddDownStaircaseOnly(levelNo);

            //PC starts at what would be the up staircase
            dungeon.Player.LocationMap = caveGen.GetPCStartLocation();

            //Add a trigger here
            dungeon.AddTrigger(0, caveGen.GetPCStartLocation(), new Triggers.DungeonEntranceTrigger());
            
            //Rest of the cave levels
            for (int i = 0; i < noCaveLevels - 1; i++)
            {
                caveGen.GenerateMap(false);

                if (i < noCaveWaterLevels)
                    caveGen.AddWaterToCave(15, 4);  

                //AddStaircases needs to know the level number
                levelNo = dungeon.AddMap(caveGen.Map);
                caveGen.AddStaircases(levelNo);
            }

            //Ruined halls levels
            for (int i = 0; i < noRuinedLevels; i++)
            {
                Map ruinedLevel = ruinedGen.GenerateMap(ruinedExtraCorridorDefinite + Game.Random.Next(ruinedExtraCorridorRandom));
                levelNo = dungeon.AddMap(ruinedLevel);
                ruinedGen.AddStaircases(levelNo);
            }

            //Halls
            for (int i = 0; i < noHallLevels; i++)
            {
                Map hallsLevel = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
                levelNo = dungeon.AddMap(hallsLevel);
                hallsGen.AddStaircases(levelNo);
            }
            
            //Final battle level

            try
            {
                asciiGen.LoadASCIIFile("battle.txt");
                asciiGen.AddMapToDungeon();
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Failed to load battle level!: " + ex.Message);
                throw new ApplicationException("Failed to load battle level! Is the game installed correctly?");
            }

            //Outdoors level

            try
            {
                asciiGen.LoadASCIIFile("last.txt");
                asciiGen.AddMapToDungeon();
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Failed to load last level!: " + ex.Message);
                throw new ApplicationException("Failed to load last level! Is the game installed correctly?");
            }

            //Build TCOD maps
            //Necessary so connectivity checks on items and monsters can work
            dungeon.RecalculateWalkable();
            dungeon.RefreshTCODMaps();

        }
    }
}
