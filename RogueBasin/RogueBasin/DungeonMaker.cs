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

        public GameDifficulty difficulty { get; set; }

        int noCaveLevels = 5;
        int noCaveWaterLevels = 3;

        int noRuinedLevels = 5;
        int ruinedExtraCorridorDefinite = 5;
        int ruinedExtraCorridorRandom = 10;

        int noHallLevels = 5;
        int hallsExtraCorridorDefinite = 0;
        int hallsExtraCorridorRandom = 8;

        int plotItemOnMonsterChance = 50;

        public DungeonMaker(GameDifficulty diff) {
            difficulty = diff;
        }

        /// <summary>
        /// Big function that spawns and fills a new dungeon
        /// </summary>
        /// <returns></returns>
        public Dungeon SpawnNewDungeon()
        {
            dungeon = new Dungeon();
            Game.Dungeon = dungeon; //not classy but I have to do it here since some other classes (e.g. map gen) call it
            Game.Dungeon.Difficulty = difficulty;

            SetupPlayer();

            SetupMaps();

            SpawnInitialCreatures();

            SpawnItems();

            
            SpawnUniques();

            Game.Dungeon.TimeToRescueFriend = 1000000;


            return dungeon;
        }

        private void SpawnUniques()
        {
            int outsideLevel = dungeon.NoLevels - 1;
            int battleLevel = dungeon.NoLevels - 2;

            //Find lich
            Creatures.Lich lich = null;

            foreach (Monster m in dungeon.Monsters)
            {
                if(m is Creatures.Lich)
                lich = m as Creatures.Lich;
            }

            if (lich != null)
            {
                //Set some difficulty specific params

                if (Game.Dungeon.Difficulty == GameDifficulty.Easy)
                {
                    lich.MaxSummons = 8;
                }
                else if (Game.Dungeon.Difficulty == GameDifficulty.Medium)
                {
                    lich.MaxSummons = 12;
                }
                else
                {
                    lich.MaxSummons = 20;
                }


                if (Game.Dungeon.Difficulty == GameDifficulty.Easy)
                {
                    lich.MaxHitpoints = 50;
                    lich.Hitpoints = 50;
                }
                else if (Game.Dungeon.Difficulty == GameDifficulty.Medium)
                {
                    lich.MaxHitpoints = 75;
                    lich.Hitpoints = 75;
                }
                else
                {
                    lich.MaxHitpoints = 100;
                    lich.Hitpoints = 100;
                }
            }
        }

        private void SetupPlayer()
        {
            Player player = dungeon.Player;

            player.Representation = '@';

            //player.Hitpoints = 100;
            //player.MaxHitpoints = 100;

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

            List<int> levelMonsterAmounts = new List<int>();
            
            for(int i=0;i<15;i++) {
                int num = 100 + 40 * i;
                levelMonsterAmounts.Add(num);
            }
            
            /*{

                150, //1
                120, //2
                140, //3
                160, //4
                200, //5
                220, //6
                240, //7
                260, //8
                300, //9
                340, //10
                380, //11
                420, //12
                460, //13
                500, //14
                550 //15
            };*/

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

                        int costSpent = 0;

                        do
                        {
                            randomNum = Game.Random.Next(100);

                            Monster monsterToAdd = null;

                            if (randomNum < 95)
                            {
                                monsterToAdd = levelList[0].GetRandomMonster();
                            }
                            else
                            {
                                monsterToAdd = levelList[1].GetRandomMonster();
                            }

                            Point location;

                            do
                            {
                                location = dungeon.RandomWalkablePointInLevel(i);
                            } while (!dungeon.AddMonster(monsterToAdd, i, location));

                            CheckSpecialMonsterGroups(monsterToAdd, i);

                            costSpent += monsterToAdd.CreatureCost();

                        } while (costSpent < levelMonsterAmounts[i]);

                        break;

                    case 1:
                    case 2:

                        costSpent = 0;

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

                            if (randomNum < 25)
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

                            if (randomNum < 10)
                            {
                                monsterToAdd = levelList[0].GetRandomMonster();
                            }
                            else if (randomNum < 30)
                            {
                                monsterToAdd = levelList[1].GetRandomMonster();
                            }
                            else if (randomNum < 60)
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

                            if (randomNum < 15)
                            {
                                monsterToAdd = levelList[0].GetRandomMonster();
                            }
                            else if (randomNum < 25)
                            {
                                monsterToAdd = levelList[1].GetRandomMonster();
                            }
                            else if (randomNum < 35)
                                monsterToAdd = levelList[2].GetRandomMonster();
                            else if(randomNum < 80)
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

                            if (randomNum < 10)
                            {
                                monsterToAdd = levelList[0].GetRandomMonster();
                            }
                            else if (randomNum < 20)
                            {
                                monsterToAdd = levelList[1].GetRandomMonster();
                            }
                            else if (randomNum < 30)
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
                int noGoblins = 0 + Game.Random.Next(2);

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
                int noOrcs = 1 + Game.Random.Next(2);
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
                int noSkel = 1 + Game.Random.Next(2);
                int noZomb = 0 + Game.Random.Next(2);

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
            
            //dungeon.AddItem(new Items.Glove(), 0, dungeon.Player.LocationMap);
            //The rest of the plot items are split between the remaining cave and ruined levels

            List<Item> plotItems = new List<Item> { 
                //special mode items (9)
                new Items.Badge(), new Items.Band(), new Items.Boots(), new Items.Bracer(), new Items.GlassGem(),
            new Items.Greaves(), new Items.LeadRing(), new Items.Lockpicks(), new Items.Sash() };

            List<Item> plotLevelItems = new List<Item> {
            //levelling items 
            new Items.Backpack(), new Items.Book(), new Items.Medal(), new Items.Stone(), new Items.Flint() };
            //glove is separate

            Game.Dungeon.TotalPlotItems = 15;

            /*
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
            */

            /*
            //debug
            //Give them all to me!

            foreach (Item item in plotItems)
            {
                dungeon.Player.PickUpItem(item);
            }
            */
            //Stick them all on the first level


            int totalNormalLevels = noCaveLevels + noHallLevels + noRuinedLevels;

            int level = 0;
            int loopCount = 0;

            int gloveLevel = 0;

            do
            {
                location = dungeon.RandomWalkablePointInLevel(gloveLevel);
            } while (!dungeon.AddItem(new Items.Glove(), gloveLevel, location));
            
            //Include glove on level 1
            List<int> levelsWithPlotItems = new List<int> { gloveLevel };

            List<Item> itemsPlaced = new List<Item>();

            //Levelling items are distributed through the first 12 levels

            for (int i = 0; i < plotLevelItems.Count; i++)
            {

                Item plotItem;
                do
                {
                    plotItem = plotLevelItems[Game.Random.Next(plotLevelItems.Count)];
                } while (itemsPlaced.Contains(plotItem));
                
                loopCount = 0;

                do
                {
                    level = 2 * i + 1;

                    if (Game.Random.Next(10) < 2)
                    {
                        level--;
                    }
                    else if (Game.Random.Next(10) < 4)
                    {
                        level++;
                    }

                    level = Game.Random.Next(noCaveLevels + noRuinedLevels);
                    loopCount++;

                } while (levelsWithPlotItems.Contains(level) && loopCount < 100);



                //Put on the floor
                do
                {
                    location = dungeon.RandomWalkablePointInLevel(level);

                    //May want to specify a minimum distance from staircases??? TODO
                } while (!dungeon.AddItem(plotItem, level, location));

                levelsWithPlotItems.Add(level);
                itemsPlaced.Add(plotItem);
            }

            //Distribute the move items randomly on other levels

            for (int i = 0; i < plotItems.Count; i++)
            {
                //Find random level w/o plotItem
                loopCount = 0;
                Item plotItem;
               
                do
                {
                    plotItem = plotItems[Game.Random.Next(plotItems.Count)];
                } while (itemsPlaced.Contains(plotItem));

                do
                {
                    level = Game.Random.Next(totalNormalLevels - 2);
                    loopCount++;
                } while (levelsWithPlotItems.Contains(level) && loopCount < 100);

                levelsWithPlotItems.Add(level);
                itemsPlaced.Add(plotItem);

                //On the floor
                //Find position in level and place item
                do
                {
                    location = dungeon.RandomWalkablePointInLevel(level);

                    //May want to specify a minimum distance from staircases??? TODO
                } while (!dungeon.AddItem(plotItem, level, location));


                /*
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
                }
                 * */
            }
            
            //Potions

            //Cave levels

            for (int i = 0; i < noCaveLevels; i++)
            {
                int totalPotions = 5 + Game.Random.Next(5);

                for (int j = 0; j < totalPotions; j++)
                {
                    int randomChance = Game.Random.Next(100);

                    Item potion;

                    if (randomChance < 45)
                        potion = new Items.Potion();
                    else if (randomChance < 65)
                        potion = new Items.PotionToHitUp();
                    else if (randomChance < 70)
                        potion = new Items.PotionDamUp();
                    else if (randomChance < 85)
                        potion = new Items.PotionSpeedUp();
                    else if (randomChance < 90)
                        potion = new Items.PotionSightUp();

                    else if (randomChance < 93)
                        potion = new Items.PotionMajHealing();
                    else if (randomChance < 94)
                        potion = new Items.PotionMajDamUp();
                    else if (randomChance < 95)
                        potion = new Items.PotionMajSpeedUp();
                    else if (randomChance < 96)
                        potion = new Items.PotionMajSightUp();
                    else if (randomChance < 97)
                        potion = new Items.PotionSuperHealing();
                    else if (randomChance < 98)
                        potion = new Items.PotionSuperDamUp();
                    else if (randomChance < 99)
                        potion = new Items.PotionSuperToHitUp();
                    else
                        potion = new Items.PotionSuperSpeedUp();

                    PlaceItemOnLevel(potion, i, 50);
                }
            }

            for (int i = noRuinedLevels; i < noRuinedLevels + noCaveLevels; i++)
            {
                int totalPotions = 5 + Game.Random.Next(5);

                for (int j = 0; j < totalPotions; j++)
                {
                    int randomChance = Game.Random.Next(100);

                    Item potion;

                    if (randomChance < 5)
                        potion = new Items.Potion();
                    else if (randomChance < 10)
                        potion = new Items.PotionToHitUp();
                    else if (randomChance < 15)
                        potion = new Items.PotionDamUp();
                    else if (randomChance < 20)
                        potion = new Items.PotionSpeedUp();
                    else if (randomChance < 25)
                        potion = new Items.PotionSightUp();

                    else if (randomChance < 55)
                        potion = new Items.PotionMajHealing();
                    else if (randomChance < 65)
                        potion = new Items.PotionMajDamUp();
                    else if (randomChance < 75)
                        potion = new Items.PotionMajSpeedUp();
                    else if (randomChance < 85)
                        potion = new Items.PotionMajSightUp();
                    else if (randomChance < 93)
                        potion = new Items.PotionSuperHealing();
                    else if (randomChance < 95)
                        potion = new Items.PotionSuperDamUp();
                    else if (randomChance < 98)
                        potion = new Items.PotionSuperToHitUp();
                    else
                        potion = new Items.PotionSuperSpeedUp();

                    PlaceItemOnLevel(potion, i, 50);
                }
            }

            for (int i = noRuinedLevels + noCaveLevels; i < noRuinedLevels + noCaveLevels + noHallLevels; i++)
            {
                int totalPotions = 5 + Game.Random.Next(5);

                for (int j = 0; j < totalPotions; j++)
                {
                    int randomChance = Game.Random.Next(100);

                    Item potion;

                    if (randomChance < 5)
                        potion = new Items.Potion();
                    else if (randomChance < 7)
                        potion = new Items.PotionToHitUp();
                    else if (randomChance < 9)
                        potion = new Items.PotionDamUp();
                    else if (randomChance < 11)
                        potion = new Items.PotionSpeedUp();
                    else if (randomChance < 14)
                        potion = new Items.PotionSightUp();

                    else if (randomChance < 35)
                        potion = new Items.PotionMajHealing();
                    else if (randomChance < 40)
                        potion = new Items.PotionMajDamUp();
                    else if (randomChance < 45)
                        potion = new Items.PotionMajSpeedUp();
                    else if (randomChance < 50)
                        potion = new Items.PotionMajSightUp();
                    else if (randomChance < 70)
                        potion = new Items.PotionSuperHealing();
                    else if (randomChance < 80)
                        potion = new Items.PotionSuperDamUp();
                    else if (randomChance < 90)
                        potion = new Items.PotionSuperToHitUp();
                    else
                        potion = new Items.PotionSuperSpeedUp();

                    PlaceItemOnLevel(potion, i, 50);
                }
            }

            /*
            //Add a few healing potions

            for (int i = 0; i < 10; i++)
            {
                do
                {
                    location = dungeon.RandomWalkablePointInLevel(level);

                    //May want to specify a minimum distance from staircases??? TODO
                } while (!dungeon.AddItem(new Items.Potion(), 0, location));

                
            }*/
        }

        private void PlaceItemOnLevel(Item item, int level, int onMonsterChance)
        {
            //50% chance they will be generated on a monster
            bool putOnMonster = false;

            if (Game.Random.Next(100) < onMonsterChance)
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
                monster.PickUpItem(item);
            }

            if (!putOnMonster)
            {
                //On the floor
                //Find position in level and place item

                Point location = new Point(0, 0);
                do
                {
                    location = dungeon.RandomWalkablePointInLevel(level);

                    //May want to specify a minimum distance from staircases??? TODO
                } while (!dungeon.AddItem(item, level, location));
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

                //Set light
                Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);
            }

            //Ruined halls levels
            for (int i = 0; i < noRuinedLevels; i++)
            {
                Map ruinedLevel = ruinedGen.GenerateMap(ruinedExtraCorridorDefinite + Game.Random.Next(ruinedExtraCorridorRandom));
                levelNo = dungeon.AddMap(ruinedLevel);
                ruinedGen.AddStaircases(levelNo);

                //Set light
                Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);
            }

            //Halls
            for (int i = 0; i < noHallLevels; i++)
            {
                Map hallsLevel = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
                levelNo = dungeon.AddMap(hallsLevel);
                hallsGen.AddStaircases(levelNo);

                //Set light
                Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);
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

        private double GetLightLevel(int levelNo)
        {
            int lightDelta = 5 + Game.Random.Next(15);
            lightDelta -= levelNo;

            double lightLevel = lightDelta / 10.0;

            if (lightLevel < 0.4)
                lightLevel = 0.4;

            if (lightLevel > 2.0)
            {
                lightLevel = 2.0;
            }
            return lightLevel;
        }
    }
}
