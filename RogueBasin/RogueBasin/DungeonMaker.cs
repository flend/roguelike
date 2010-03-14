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
                case "bugbear":
                    return new Creatures.Bugbear();
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
                case "ghoul":
                    return new Creatures.Zombie();
                case "skeletal archer":
                    return new Creatures.SkeletalArcher();
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
        public Dungeon dungeon = null;

        public GameDifficulty difficulty { get; set; }

        int maxLoopCount = 500;

       // int caveDungeonNoLevels = 4;

        //int noCaveLevels = 10;
       // int noCaveWaterLevels = 5;

        //int noRuinedLevels = 3;
        int ruinedExtraCorridorDefinite = 5;
        int ruinedExtraCorridorRandom = 10;

        //int noHallLevels = 2;
        int hallsExtraCorridorDefinite = 0;
        int hallsExtraCorridorRandom = 8;

        //int potionOnMonsterChance = 60;

        public DungeonMaker(GameDifficulty diff) {
            difficulty = diff;
        }

        /// <summary>
        /// Sets up player, dungeons, creatures, uniques
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

            SpawnUniquesAndItems();

            SpawnItems();

            
            //SpawnUniques();

            switch (difficulty)
            {
                case GameDifficulty.Easy:
                    Game.Dungeon.Player.TimeToRescueFriend = 800000;
                    break;
                case GameDifficulty.Medium:
                    Game.Dungeon.Player.TimeToRescueFriend = 700000;
                    break;
                case GameDifficulty.Hard:
                    Game.Dungeon.Player.TimeToRescueFriend = 600000;
                    break;
            }
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
                    lich.MaxSummons = 3;
                }
                else if (Game.Dungeon.Difficulty == GameDifficulty.Medium)
                {
                    lich.MaxSummons = 6;
                }
                else
                {
                    lich.MaxSummons = 8;
                }


                if (Game.Dungeon.Difficulty == GameDifficulty.Easy)
                {
                    lich.MaxHitpoints = 40;
                    lich.Hitpoints = 40;
                }
                else if (Game.Dungeon.Difficulty == GameDifficulty.Medium)
                {
                    lich.MaxHitpoints = 60;
                    lich.Hitpoints = 60;
                }
                else
                {
                    lich.MaxHitpoints = 75;
                    lich.Hitpoints = 75;
                }
            }


        }

        private void SetupPlayer()
        {
            Player player = dungeon.Player;

            player.Representation = '@';

            player.CalculateCombatStats();
            player.Hitpoints = player.MaxHitpoints;

            //player.Hitpoints = 100;
            //player.MaxHitpoints = 100;

        }

        struct MonsterCommon
        {
            public MonsterCommon(Monster mon, int common)
            {
                monster = mon;
                commonness = common;
            }

            public int commonness;
            public Monster monster;
        }

        private Monster GetMonsterFromCommonList(List<MonsterCommon> list)
        {
            //Find total prob
            int totalProb = 0;
            
            foreach (MonsterCommon com in list)
            {
                totalProb += com.commonness;
            }

            int toChoose = Game.Random.Next(totalProb);

            int thisProb = 0;

            foreach (MonsterCommon com in list)
            {
                if (thisProb + com.commonness >= toChoose)
                {
                    return com.monster.NewCreatureOfThisType();
                }
                thisProb += com.commonness;
            }

            LogFile.Log.LogEntryDebug("GetMonsterFromCommonness: Should never get here", LogDebugLevel.High);
            return null;
        }

        private Point PlaceMonster(int level, Monster monster)
        {
            Point location = new Point();
            do
            {
                location = dungeon.RandomWalkablePointInLevel(level);
            } while (!dungeon.AddMonster(monster, level, location));

            CheckSpecialMonsterGroups(monster, level);

            return location;
        }



        //Spawning shared variables
        int looseGroupDist;
        int lonerChance;
        int maxGroupSize;
        int minGroupSize;

        private void SpawnDungeon(int dungeonID) {

            int budgetScale = 1;

            switch(dungeonID) {

                case 0:
                    SpawnCaveCreatures(budgetScale);
                    SpawnCaveItems(budgetScale);
                    break;
                case 1:
                    SpawnWaterCaveCreatures(budgetScale);
                    SpawnWaterCaveItems(budgetScale);
                    break;
                case 2:
                    SpawnForestCreatures(budgetScale);
                    SpawnForestItems(budgetScale);
                    break;
                case 3:
                    SpawnOrcHutCreatures(budgetScale);
                    SpawnOrcItems(budgetScale);
                    break;
                case 4:
                    SpawnCryptCreatures(budgetScale);
                    break;
                case 5:
                    SpawnDemonCreatures(budgetScale);
                    break;
                case 6:
                    SpawnPrinceCreatures(budgetScale);
                    break;
            }
        }

        public void ReSpawnDungeon(int dungeonID)
        {
            //Kill all the creatures currently in there, except for the uniques
            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            List<Monster> monsters = Game.Dungeon.Monsters;

            foreach (Monster m in monsters)
            {
                if (m.LocationLevel >= dungeonStartLevel && m.LocationLevel <= dungeonEndLevel)
                {
                    if(!m.Unique)
                        Game.Dungeon.KillMonster(m, true);
                }
            }

            //Respawn the creatures

            int budgetScale = 1;

            switch (dungeonID)
            {

                case 0:
                    SpawnCaveCreatures(budgetScale);
                    SpawnCaveItems(budgetScale);
                    break;
                case 1:
                    SpawnWaterCaveCreatures(budgetScale);
                    SpawnWaterCaveItems(budgetScale);
                    break;
                case 2:
                    SpawnForestCreatures(budgetScale);
                    SpawnForestItems(budgetScale);
                    break;
                case 3:
                    SpawnOrcHutCreatures(budgetScale);
                    SpawnOrcItems(budgetScale);
                    break;
                case 4:
                    SpawnCryptCreatures(budgetScale);
                    break;
                case 5:
                    SpawnDemonCreatures(budgetScale);
                    break;
                case 6:
                    SpawnPrinceCreatures(budgetScale);
                    break;
            }
        }

        private void SpawnWaterCaveCreatures(int budgetScale)
        {
            //Dungeon 1: CAVE WATER

            int dungeonID = 1;

            List<int> monsterBudgets = new List<int>();

            monsterBudgets.Add(budgetScale * 250);
            monsterBudgets.Add(budgetScale * 300);
            monsterBudgets.Add(budgetScale * 300);
            monsterBudgets.Add(budgetScale * 400);

            //build commonness list for caves

            //Gives about 50 monsters
           List<MonsterCommon> caveList = new List<MonsterCommon>();
            caveList.Add(new MonsterCommon(new Creatures.Ferret(), 50));
            caveList.Add(new MonsterCommon(new Creatures.Rat(), 50));
            caveList.Add(new MonsterCommon(new Creatures.Bat(), 50));
            caveList.Add(new MonsterCommon(new Creatures.Spider(), 30));
            caveList.Add(new MonsterCommon(new Creatures.Ogre(), 20));
            caveList.Add(new MonsterCommon(new Creatures.Bugbear(), 40));
            caveList.Add(new MonsterCommon(new Creatures.Pixie(), 20));

            looseGroupDist = 8;
            lonerChance = 50;
            maxGroupSize = 6;
            minGroupSize = 3;

            int dungeonStartLevel = dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int levelNo = dungeonStartLevel; levelNo <= dungeonEndLevel; levelNo++)
            {
                GroupAndDisperseMonsters(levelNo, monsterBudgets[levelNo - dungeonStartLevel], caveList);
            }

            SetLightLevelUniversal(dungeonStartLevel, dungeonEndLevel, 5);

        }
        private void SpawnCaveCreatures(int budgetScale)
        {
            //Dungeon 0: CAVE

            List<int> monsterBudgets = new List<int>();

            int dungeonID = 0;

            monsterBudgets.Add(budgetScale * 100);
            monsterBudgets.Add(budgetScale * 150);
            monsterBudgets.Add(budgetScale * 250);
            monsterBudgets.Add(budgetScale * 300);

            //build commonness list for caves

            //Gives about 50 monsters
            List<MonsterCommon> caveList = new List<MonsterCommon>();
            caveList.Add(new MonsterCommon(new Creatures.Ferret(), 50));
            caveList.Add(new MonsterCommon(new Creatures.Rat(), 50));
            caveList.Add(new MonsterCommon(new Creatures.Spider(), 30));
            caveList.Add(new MonsterCommon(new Creatures.Goblin(), 30));
            caveList.Add(new MonsterCommon(new Creatures.GoblinWitchdoctor(), 10));

            looseGroupDist = 10;
            lonerChance = 50;
            maxGroupSize = 6;
            minGroupSize = 3;

            int dungeonStartLevel = dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int levelNo = dungeonStartLevel; levelNo <= dungeonEndLevel; levelNo++)
            {
                GroupAndDisperseMonsters(levelNo, monsterBudgets[levelNo - dungeonStartLevel], caveList);
            }

            SetLightLevelUniversal(dungeonStartLevel, dungeonEndLevel, 8);

        }

        private void SpawnCaveItems(int budgetScale)
        {
            int dungeonID = 0;

            int dungeonStartLevel = dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int i = dungeonStartLevel; i <= dungeonEndLevel; i++)
            {

                //Spawn bonus potions

                int totalPotions = 1 + Game.Random.Next(3);

                for (int j = 0; j < totalPotions; j++)
                {
                    int randomChance = Game.Random.Next(100);

                    Item potion;

                    if (randomChance < 50)
                        potion = new Items.PotionDamUp();
                    else 
                        potion = new Items.PotionToHitUp();
                 
                    PlaceItemOnLevel(potion, i, 50);
                }

                //Spawn restore / heal potions

                totalPotions = 1 + Game.Random.Next(3);

                for (int j = 0; j < totalPotions; j++)
                {
                    int randomChance = Game.Random.Next(100);

                    Item potion;

                    if (randomChance < 50)
                        potion = new Items.Potion();
                    else
                        potion = new Items.PotionMPRestore();

                    PlaceItemOnLevel(potion, i, 50);
                }
            }

        }

        private void SpawnWaterCaveItems(int budgetScale)
        {
            int dungeonID = 1;

            int dungeonStartLevel = dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int i = dungeonStartLevel; i <= dungeonEndLevel; i++)
            {

                //Spawn bonus potions

                int totalPotions = 1 + Game.Random.Next(3);

                for (int j = 0; j < totalPotions; j++)
                {
                    int randomChance = Game.Random.Next(100);

                    Item potion;

                    if (randomChance < 50)
                        potion = new Items.PotionSpeedUp();
                    else
                        potion = new Items.PotionSightUp();

                    PlaceItemOnLevel(potion, i, 50);
                }

                //Spawn restore / heal potions

                totalPotions = Game.Random.Next(3);

                for (int j = 0; j < totalPotions; j++)
                {
                    int randomChance = Game.Random.Next(100);

                    Item potion;

                    if (randomChance < 50)
                        potion = new Items.Potion();
                    else
                        potion = new Items.PotionMPRestore();

                    PlaceItemOnLevel(potion, i, 50);
                }
            }

        }

        private void SpawnForestItems(int budgetScale)
        {
            int dungeonID = 2;

            int dungeonStartLevel = dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int i = dungeonStartLevel; i <= dungeonEndLevel; i++)
            {

                //Spawn bonus potions

                int totalPotions = Game.Random.Next(2);

                for (int j = 0; j < totalPotions; j++)
                {
                    int randomChance = Game.Random.Next(100);

                    Item potion;

                    if (randomChance < 50)
                        potion = new Items.PotionSpeedUp();
                    else
                        potion = new Items.PotionSightUp();

                    PlaceItemOnLevel(potion, i, 50);
                }

                //Spawn restore / heal potions

                totalPotions = 1 + Game.Random.Next(3);

                for (int j = 0; j < totalPotions; j++)
                {
                    int randomChance = Game.Random.Next(100);

                    Item potion;

                    if (randomChance < 35)
                        potion = new Items.Potion();
                    else
                        potion = new Items.PotionMPRestore();

                    PlaceItemOnLevel(potion, i, 50);
                }
            }

        }

        private void SpawnOrcItems(int budgetScale)
        {
            int dungeonID = 3;

            int dungeonStartLevel = dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int i = dungeonStartLevel; i <= dungeonEndLevel; i++)
            {
                int totalPotions = Game.Random.Next(3);

                for (int j = 0; j < totalPotions; j++)
                {
                    int randomChance = Game.Random.Next(100);

                    Item potion;

                    if (randomChance < 10)
                        potion = new Items.Potion();
                    else if (randomChance < 20)
                        potion = new Items.PotionMPRestore();
                    else if (randomChance < 60)
                        potion = new Items.PotionDamUp();
                    else
                        potion = new Items.PotionToHitUp();

                    PlaceItemOnLevel(potion, i, 50);
                }
            }

        }


        private void SpawnForestCreatures(int budgetScale)
        {
            //Dungeon 2: FOREST

            int dungeonID = 2;

            List<int> monsterBudgets = new List<int>();

            monsterBudgets.Add(budgetScale * 250);
            monsterBudgets.Add(budgetScale * 300);
            monsterBudgets.Add(budgetScale * 350);
            monsterBudgets.Add(budgetScale * 400);

            //build commonness list for caves

            //Gives about 50 monsters
            List<MonsterCommon> caveList = new List<MonsterCommon>();
            caveList.Add(new MonsterCommon(new Creatures.Pixie(), 15));
            caveList.Add(new MonsterCommon(new Creatures.Ferret(), 60));
            caveList.Add(new MonsterCommon(new Creatures.Bat(), 50));
            caveList.Add(new MonsterCommon(new Creatures.BlackUnicorn(), 30));
            caveList.Add(new MonsterCommon(new Creatures.Faerie(), 15));
            caveList.Add(new MonsterCommon(new Creatures.Nymph(), 15));
            caveList.Add(new MonsterCommon(new Creatures.Peon(), 10));

            looseGroupDist = 6;
            lonerChance = 30;
            maxGroupSize = 6;
            minGroupSize = 3;

            int dungeonStartLevel = dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int levelNo = dungeonStartLevel; levelNo <= dungeonEndLevel; levelNo++)
            {
                GroupAndDisperseMonsters(levelNo, monsterBudgets[levelNo - dungeonStartLevel], caveList);
            }

            SetLightLevelUniversal(dungeonStartLevel, dungeonEndLevel, 10);
        }

        private void SpawnCryptCreatures(int budgetScale)
        {
            //Dungeon 4: CRYPT

            int dungeonID = 4;

            List<int> monsterBudgets = new List<int>();

            monsterBudgets.Add(budgetScale * 300);
            monsterBudgets.Add(budgetScale * 350);
            monsterBudgets.Add(budgetScale * 450);
            monsterBudgets.Add(budgetScale * 500);

            //build commonness list for caves

            //Gives about 50 monsters
            List<MonsterCommon> caveList = new List<MonsterCommon>();
            caveList.Add(new MonsterCommon(new Creatures.Ghoul(), 30));
            caveList.Add(new MonsterCommon(new Creatures.Skeleton(), 50));
            caveList.Add(new MonsterCommon(new Creatures.SkeletalArcher(), 30));
            caveList.Add(new MonsterCommon(new Creatures.Zombie(), 50));
            caveList.Add(new MonsterCommon(new Creatures.Necromancer(), 20));
            caveList.Add(new MonsterCommon(new Creatures.Peon(), 10));

            looseGroupDist = 6;
            lonerChance = 30;
            maxGroupSize = 6;
            minGroupSize = 3;

            int dungeonStartLevel = dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int levelNo = dungeonStartLevel; levelNo <= dungeonEndLevel; levelNo++)
            {
                GroupAndDisperseMonsters(levelNo, monsterBudgets[levelNo - dungeonStartLevel], caveList);
            }

            SetLightLevelUniversal(dungeonStartLevel, dungeonEndLevel, 15);

        }

        private void SpawnOrcHutCreatures(int budgetScale)
        {
            //Dungeon 3: ORC HUT

            int dungeonID = 3;

            List<int> monsterBudgets = new List<int>();

            monsterBudgets.Add(budgetScale * 250);
            monsterBudgets.Add(budgetScale * 300);
            monsterBudgets.Add(budgetScale * 350);
            monsterBudgets.Add(budgetScale * 400);

            //build commonness list for caves

            //Gives about 50 monsters
            List<MonsterCommon> caveList = new List<MonsterCommon>();
            caveList.Add(new MonsterCommon(new Creatures.Orc(), 50));
            caveList.Add(new MonsterCommon(new Creatures.OrcShaman(), 20));
            caveList.Add(new MonsterCommon(new Creatures.GoblinWitchdoctor(), 20));
            caveList.Add(new MonsterCommon(new Creatures.Ogre(), 30));
            caveList.Add(new MonsterCommon(new Creatures.Bugbear(), 35));
            caveList.Add(new MonsterCommon(new Creatures.Ferret(), 30));
            caveList.Add(new MonsterCommon(new Creatures.Uruk(), 30));
            caveList.Add(new MonsterCommon(new Creatures.Spider(), 20));

            looseGroupDist = 6;
            lonerChance = 30;
            maxGroupSize = 6;
            minGroupSize = 3;

            int dungeonStartLevel = dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int levelNo = dungeonStartLevel; levelNo <= dungeonEndLevel; levelNo++)
            {
                GroupAndDisperseMonsters(levelNo, monsterBudgets[levelNo - dungeonStartLevel], caveList);
            }

            SetLightLevelUniversal(dungeonStartLevel, dungeonEndLevel, 0);
        }

        void SpawnDemonCreatures(int budgetScale)
        {
            //Dungeon 5: DEMON LAYER

            int dungeonID = 5;

            List<int> monsterBudgets = new List<int>();

            monsterBudgets.Add(budgetScale * 450);
            monsterBudgets.Add(budgetScale * 550);
            monsterBudgets.Add(budgetScale * 650);
            monsterBudgets.Add(budgetScale * 750);

            //build commonness list for caves

            //Gives about 50 monsters
            List<MonsterCommon> caveList = new List<MonsterCommon>();
            caveList.Add(new MonsterCommon(new Creatures.Imp(), 20));
            caveList.Add(new MonsterCommon(new Creatures.Demon(), 60));
            caveList.Add(new MonsterCommon(new Creatures.Maleficarum(), 20));
            caveList.Add(new MonsterCommon(new Creatures.Whipper(), 40));
            caveList.Add(new MonsterCommon(new Creatures.Meddler(), 20));
            caveList.Add(new MonsterCommon(new Creatures.Drainer(), 30));
            caveList.Add(new MonsterCommon(new Creatures.Peon(), 60));
            caveList.Add(new MonsterCommon(new Creatures.Overlord(), 10));


            looseGroupDist = 6;
            lonerChance = 30;
            maxGroupSize = 6;
            minGroupSize = 3;

            int dungeonStartLevel = dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int levelNo = dungeonStartLevel; levelNo <= dungeonEndLevel; levelNo++)
            {
                GroupAndDisperseMonsters(levelNo, monsterBudgets[levelNo - dungeonStartLevel], caveList);
            }

            SetLightLevelUniversal(dungeonStartLevel, dungeonEndLevel, 10);
        }

        private void SpawnPrinceCreatures(int budgetScale)
        {
            //Dungeon 6: PRINCE

            int dungeonID = 6;

            List<int> monsterBudgets = new List<int>();

            monsterBudgets.Add(budgetScale * 300);
            monsterBudgets.Add(budgetScale * 300);
            monsterBudgets.Add(budgetScale * 350);
            monsterBudgets.Add(budgetScale * 400);

            //build commonness list for caves

            //Gives about 50 monsters
            List<MonsterCommon> caveList = new List<MonsterCommon>();
            caveList.Add(new MonsterCommon(new Creatures.Imp(), 10));
            caveList.Add(new MonsterCommon(new Creatures.Demon(), 10));
            caveList.Add(new MonsterCommon(new Creatures.Skeleton(), 20));
            caveList.Add(new MonsterCommon(new Creatures.SkeletalArcher(), 20));
            caveList.Add(new MonsterCommon(new Creatures.Necromancer(), 10));
            caveList.Add(new MonsterCommon(new Creatures.Pixie(), 10));
            caveList.Add(new MonsterCommon(new Creatures.Ferret(), 30));
            caveList.Add(new MonsterCommon(new Creatures.Faerie(), 10));
            caveList.Add(new MonsterCommon(new Creatures.Ogre(), 20));
            caveList.Add(new MonsterCommon(new Creatures.Ghoul(), 10));
            caveList.Add(new MonsterCommon(new Creatures.Rat(), 10));
            caveList.Add(new MonsterCommon(new Creatures.OrcShaman(), 10));

            looseGroupDist = 6;
            lonerChance = 30;
            maxGroupSize = 6;
            minGroupSize = 3;

            int dungeonStartLevel = dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int levelNo = dungeonStartLevel; levelNo <= dungeonEndLevel; levelNo++)
            {
                GroupAndDisperseMonsters(levelNo, monsterBudgets[levelNo - dungeonStartLevel], caveList);
            }

            SetLightLevelUniversal(dungeonStartLevel, dungeonEndLevel, 0);
        }

         private void SpawnInitialCreatures() {
             LogFile.Log.LogEntry("Generating creatures...");

             Dungeon dungeon = Game.Dungeon;

            //Add monsters to levels

             //Quick up down compensate globally

             for (int i = 0; i < 7; i++)
             {
                 SpawnDungeon(i);
             }

             

            //Debug monsters

                          
            List<Monster> monstersToAdd = new List<Monster>();
            
           // monstersToAdd.Add(new Creatures.DragonUnique());

            foreach (Monster monster in monstersToAdd)
            {
                Point location = new Point();
                do
                {
                    location = dungeon.RandomWalkablePointInLevel(2);
                } while (!dungeon.AddMonster(monster, 2, location));

            }

             

            //The levels divide into 3 groups: cave, cave/halls and halls
            //The difficulty in each set is roughly the same

            //1-3: with water
            //1-5: Cave
            //5-10: Ruined Halls
            //11-15: Halls

            //Could take into account depth and difficulty level
             /*
            List<int> levelMonsterAmounts = new List<int>();

            int Dungeon1StartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(0);
            int Dungeon1EndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(0);


            for (int i = Dungeon1StartLevel; i <= Dungeon1EndLevel; i++)
            {
                int num = 200 + 80 * (i - Dungeon1StartLevel);
                levelMonsterAmounts.Add(num);
            }
             */
            //levelMonsterAmounts[0] = 500;
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
             /*
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
                new RogueBasin.Creatures.Zombie(),
                new RogueBasin.Creatures.Bugbear(),
                new RogueBasin.Creatures.Ghoul(),
                new RogueBasin.Creatures.SkeletalArcher()
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

            if (difficulty == GameDifficulty.Easy)
            {
                for (int i = 0; i < levelMonsterAmounts.Count; i++)
                {
                    levelMonsterAmounts[i] = (int)(levelMonsterAmounts[i] * 0.5);
                }
            }

            if (difficulty == GameDifficulty.Medium)
            {
                for (int i = 0; i < levelMonsterAmounts.Count; i++)
                {
                    levelMonsterAmounts[i] = (int)(levelMonsterAmounts[i] * 0.7);
                }
            }

            if (difficulty == GameDifficulty.Hard)
            {
                for (int i = 0; i < levelMonsterAmounts.Count; i++)
                {
                    levelMonsterAmounts[i] = (int)(levelMonsterAmounts[i] * 1);
                }
            }

            //Dungeon 1 - caves
            //levels 2 - 5

            for (int i = Dungeon1StartLevel; i <= Dungeon1EndLevel; i++)
            {
                int randomNum;

                //Switch on dungeon level
                switch (i)
                {
                    //Early caves
                    case 2:
                    case 3:
                    case 4:
                    case 5:

                        int costSpent = 0;
                        int indexIntoMonsterAmounts = i -Dungeon1StartLevel;

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

                        } while (costSpent < levelMonsterAmounts[indexIntoMonsterAmounts]);

                        break;
                }
            }

            List<Monster> monstersToAdd = new List<Monster>();*/
            /*
            monstersToAdd.Add(new Creatures.Bat());
            monstersToAdd.Add(new Creatures.BlackUnicorn());
            monstersToAdd.Add(new Creatures.Demon());
            monstersToAdd.Add(new Creatures.Ghoul());
            monstersToAdd.Add(new Creatures.Maleficarum());
            monstersToAdd.Add(new Creatures.Ogre());
            monstersToAdd.Add(new Creatures.Overlord());
            monstersToAdd.Add(new Creatures.Peon());
            monstersToAdd.Add(new Creatures.Pixie());
            monstersToAdd.Add(new Creatures.Uruk());
            monstersToAdd.Add(new Creatures.Whipper());
            */

            /*
            monstersToAdd.Add(new Creatures.Imp());
            monstersToAdd.Add(new Creatures.Imp());
            monstersToAdd.Add(new Creatures.Imp());
            monstersToAdd.Add(new Creatures.Drainer());
            monstersToAdd.Add(new Creatures.Drainer());
            monstersToAdd.Add(new Creatures.Drainer());
            monstersToAdd.Add(new Creatures.Nymph());
            monstersToAdd.Add(new Creatures.Nymph());
            monstersToAdd.Add(new Creatures.Nymph());*/
            
             /*
            List<Monster> monstersToAdd = new List<Monster>();
            monstersToAdd.Add(new Creatures.Faerie());
            monstersToAdd.Add(new Creatures.Faerie());
            monstersToAdd.Add(new Creatures.Faerie());
            monstersToAdd.Add(new Creatures.Faerie());

            foreach (Monster monster in monstersToAdd)
            {
                Point location = new Point();
                do
                {
                    location = dungeon.RandomWalkablePointInLevel(2);
                } while (!dungeon.AddMonster(monster, 2, location));
            }*/


             /*
             //Don't auto spawn for the last 2 levels
             for (int i = 0; i < dungeon.NoLevels - 2; i++)
             {
                 int randomNum;

                 //Switch on dungeon level
                 switch (i)
                 {
                         //Early caves
                     case 0:

                         //0 is the town

                         int costSpent = 0;

                       
                         int costSpent = 0;

                         do
                         {
                             randomNum = Game.Random.Next(100);

                             Monster monsterToAdd = null;

                             if (randomNum < 50)
                             {
                                 monsterToAdd = new Creatures.Orc();
                                 //monsterToAdd = levelList[0].GetRandomMonster();
                             }
                             else
                             {
                                 monsterToAdd = new Creatures.Bugbear();
                                 //monsterToAdd = levelList[1].GetRandomMonster();
                                 //monsterToAdd = levelList[0].GetRandomMonster();
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
             */
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
         /// Set the light level for all creatures between the levels specified (inclusive).
         /// All sets it on the dungeon levels themselves
         /// </summary>
         /// <param name="dungeonStartLevel"></param>
         /// <param name="dungeonEndLevel"></param>
         /// <param name="p"></param>
         private void SetLightLevelUniversal(int dungeonStartLevel, int dungeonEndLevel, int sightRadius)
         {
             List<Monster> monsters = Game.Dungeon.Monsters;

             for (int i = dungeonStartLevel; i <= dungeonEndLevel; i++)
             {
                 Game.Dungeon.Levels[i].LightLevel = sightRadius;
             }

             foreach (Monster monster in monsters)
             {
                 if (monster.LocationLevel >= dungeonStartLevel && monster.LocationLevel <= dungeonEndLevel)
                 {
                     //Get the monster's sight radius. 5 is normal, so use this as a ratio
                     double sightUpRatio = monster.NormalSightRadius / 5.0;

                     monster.SightRadius = (int)Math.Floor(sightRadius * sightUpRatio);
                 }
             }
         }

         private void GroupAndDisperseMonsters(int levelNo, int monsterBudget, List<MonsterCommon> caveList)
         {
             int budgetSpent = 0;

             while (budgetSpent < monsterBudget)
             {
                 Monster monsterToPlace = null;

                 //Monster by itself
                 if (Game.Random.Next(100) < lonerChance)
                 {
                     monsterToPlace = GetMonsterFromCommonList(caveList);
                     PlaceMonster(levelNo, monsterToPlace);
                     budgetSpent += monsterToPlace.CreatureCost();
                 }
                 else
                 {
                     //Loose group
                     int monsInGroup = minGroupSize + Game.Random.Next(maxGroupSize - minGroupSize);

                     //First monster is centre

                     monsterToPlace = GetMonsterFromCommonList(caveList);
                     Point centerLoc = PlaceMonster(levelNo, monsterToPlace);

                     //Other monsters surround

                     for (int i = 0; i < monsInGroup - 1; i++)
                     {
                         monsterToPlace = GetMonsterFromCommonList(caveList);
                         PlaceMonsterCloseToLocation(levelNo, monsterToPlace, centerLoc, looseGroupDist);
                         //If this takes us a bit over budget, don't worry
                         budgetSpent += monsterToPlace.CreatureCost();
                     }
                 }
             }
         }

         private Point PlaceMonsterCloseToLocation(int levelNo, Monster monsterToPlace, Point centerLoc, int looseGroupDist)
         {
             Point location = new Point();
             do
             {
                 location = dungeon.RandomWalkablePointInLevel(levelNo);

                 if (Game.Dungeon.GetDistanceBetween(centerLoc, location) > looseGroupDist)
                     continue;

             } while (!dungeon.AddMonster(monsterToPlace, levelNo, location));

             CheckSpecialMonsterGroups(monsterToPlace, levelNo);

             return location;
         }

        /// <summary>
        /// Slightly unsafe due to infinite loop but not a big deal if it fails
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="levelToPlace"></param>
        private void CheckSpecialMonsterGroups(Monster monster, int levelToPlace)
        {
            int minDistance = 4;
            int loopCount = 0;

            Point location = new Point();

            //Certain monsters spawn in with groups of their friends
            if (monster is Creatures.GoblinWitchdoctor)
            {
                //Spawn in with a random number of ferrets & goblins
                int noFerrets = 2 + Game.Random.Next(4);
                int noGoblins = 1 + Game.Random.Next(3);

                for (int i = 0; i < noFerrets; i++)
                {
                    do {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < maxLoopCount);
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
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < maxLoopCount);
                    } while (!dungeon.AddMonster(new Creatures.Goblin(), levelToPlace, location));
                }
            }
            else if (monster is Creatures.OrcShaman)
            {
                //Spawn in with a random number of orcs & spiders
                int noOrcs = 2 + Game.Random.Next(3);
                int noSpiders = 1 + Game.Random.Next(2);

                for (int i = 0; i < noOrcs; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < maxLoopCount);
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
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < maxLoopCount);
                    } while (!dungeon.AddMonster(new Creatures.Spider(), levelToPlace, location));
                }
            }

            else if (monster is Creatures.Necromancer)
            {
                //Spawn in with a random number of skels & zombs
                int noSkel = 1 + Game.Random.Next(3);
                int noZomb = 1 + Game.Random.Next(2);

                for (int i = 0; i < noSkel; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < maxLoopCount);
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
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < maxLoopCount);
                    } while (!dungeon.AddMonster(new Creatures.Zombie(), levelToPlace, location));
                }
            }
            else if (monster is Creatures.Meddler)
            {
                //Spawn in with a random number of demons & peons
                int noDem = 1 + Game.Random.Next(2);
                int noPeon = 1 + Game.Random.Next(3);

                for (int i = 0; i < noDem; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < maxLoopCount);
                    } while (!dungeon.AddMonster(new Creatures.Demon(), levelToPlace, location));
                }

                for (int i = 0; i < noPeon; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < maxLoopCount);
                    } while (!dungeon.AddMonster(new Creatures.Peon(), levelToPlace, location));
                }
            }

            else if (monster is Creatures.Maleficarum)
            {
                //Spawn in with a random number of demons & whippers
                int noDem = 1 + Game.Random.Next(2);
                int noPeon = 1 + Game.Random.Next(2);

                for (int i = 0; i < noDem; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < maxLoopCount);
                    } while (!dungeon.AddMonster(new Creatures.Demon(), levelToPlace, location));
                }

                for (int i = 0; i < noPeon; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < maxLoopCount);
                    } while (!dungeon.AddMonster(new Creatures.Whipper(), levelToPlace, location));
                }
            }
            else if (monster is Creatures.Overlord)
            {
                //Spawn in with a random number of demons & whippers
                int noDem = 1 + Game.Random.Next(3);
                int noImp = 1;
                int noDrainer = 1;

                for (int i = 0; i < noImp; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < maxLoopCount);
                    } while (!dungeon.AddMonster(new Creatures.Imp(), levelToPlace, location));
                }


                for (int i = 0; i < noDem; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < maxLoopCount);
                    } while (!dungeon.AddMonster(new Creatures.Demon(), levelToPlace, location));
                }

                for (int i = 0; i < noDrainer; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(monster, location) > minDistance && loopCount < maxLoopCount);
                    } while (!dungeon.AddMonster(new Creatures.Drainer(), levelToPlace, location));
                }
            }

        }

        private void SpawnUniquesAndItems()
        {
            //Add a pregenerated list of uniques 
            SpawnCaveUniques();
            SpawnWaterCaveUniques();
            SpawnForestUniques();
            SpawnCryptUniques();
            SpawnOrcUniques();
            SpawnDemonUniques();
            
        }

        private void SpawnCaveUniques()
        {
            int minDistance = 4;
            int loopCount = 0;

            Point location = new Point();

            //Unique rat

            LogFile.Log.LogEntryDebug("Adding cave uniques...", LogDebugLevel.Medium);

            Creatures.RatUnique rat = new RogueBasin.Creatures.RatUnique();
            int uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(0) + 2;
            Point uniqLoc = PlaceMonster(uniqueLevel, rat);

            //Spawn in with a random number of rats
            int noRats = 3 + Game.Random.Next(8);
            
            for (int i = 0; i < noRats; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Rat(), uniqueLevel, location));
            }

            //Add his items

            Items.Dagger dag = new RogueBasin.Items.Dagger();

            rat.PickUpItem(dag);

            //Unique rat

            Creatures.GoblinWitchdoctorUnique gobbo = new RogueBasin.Creatures.GoblinWitchdoctorUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(0) + 3;
            uniqLoc = PlaceMonster(uniqueLevel, gobbo);

            //Spawn in with a random number of ferrets & goblins
            int noFerrets = 4 + Game.Random.Next(2);
            int noGoblins = 3 + Game.Random.Next(2);

            for (int i = 0; i < noFerrets; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Ferret(), uniqueLevel, location));
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
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Goblin(), uniqueLevel, location));
            }

            //Add his items

            Items.Glove map = new RogueBasin.Items.Glove();

            gobbo.PickUpItem(map);

        }

        private void SpawnWaterCaveUniques()
        {
            int minDistance = 4;
            int loopCount = 0;

            Point location = new Point();

            //Unique spider

            LogFile.Log.LogEntryDebug("Adding water cave uniques...", LogDebugLevel.Medium);

            Creatures.SpiderUnique rat = new RogueBasin.Creatures.SpiderUnique();
            int uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(1) + 2;
            Point uniqLoc = PlaceMonster(uniqueLevel, rat);

            //Spawn in with a random number of rats
            int noSpiders = 3 + Game.Random.Next(2);
            int noGobbos = 2 + Game.Random.Next(2);

            for (int i = 0; i < noSpiders; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Spider(), uniqueLevel, location));
            }

            for (int i = 0; i < noGobbos; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.GoblinWitchdoctor(), uniqueLevel, location));
            }

            //Add his items

            Items.ShortSword dag = new RogueBasin.Items.ShortSword();

            rat.PickUpItem(dag);

            //Unique rat

            Creatures.OgreUnique gobbo = new RogueBasin.Creatures.OgreUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(1) + 3;
            uniqLoc = PlaceMonster(uniqueLevel, gobbo);

            //Spawn in with a random number of ferrets & goblins
            int noPixie = 2 + Game.Random.Next(2);
            int noBugbear = 2 + Game.Random.Next(2);

            for (int i = 0; i < noPixie; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Pixie(), uniqueLevel, location));
            }

            for (int i = 0; i < noBugbear; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Bugbear(), uniqueLevel, location));
            }

            //Add his items

            Items.LeatherArmour map = new RogueBasin.Items.LeatherArmour();

            gobbo.PickUpItem(map);

        }

        private void SpawnForestUniques()
        {
            int minDistance = 4;
            int loopCount = 0;

            Point location = new Point();

            //Unique spider

            LogFile.Log.LogEntryDebug("Adding forest uniques...", LogDebugLevel.Medium);

            Creatures.BlackUnicornUnique rat = new RogueBasin.Creatures.BlackUnicornUnique();
            int uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(2) + 2;
            Point uniqLoc = PlaceMonster(uniqueLevel, rat);

            //Spawn in with a random number of rats
            int noUni = 2 + Game.Random.Next(2);
            int noNymph = 1 + Game.Random.Next(2);

            for (int i = 0; i < noUni; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.BlackUnicorn(), uniqueLevel, location));
            }

            for (int i = 0; i < noNymph; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Nymph(), uniqueLevel, location));
            }

            //Add his items

            Items.PrettyDress dag = new RogueBasin.Items.PrettyDress();

            rat.PickUpItem(dag);

            //Unique rat

            Creatures.PixieUnique gobbo = new RogueBasin.Creatures.PixieUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(2) + 3;
            uniqLoc = PlaceMonster(uniqueLevel, gobbo);

            //Spawn in with a random number of ferrets & goblins
            int noPixie = 2 + Game.Random.Next(2);
            int noFairie = 2 + Game.Random.Next(2);
            noNymph = 2 + Game.Random.Next(2);

            for (int i = 0; i < noPixie; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Pixie(), uniqueLevel, location));
            }

            for (int i = 0; i < noFairie; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Faerie(), uniqueLevel, location));
            }

            for (int i = 0; i < noNymph; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Nymph(), uniqueLevel, location));
            }

            //Add his items

            Items.Lantern map = new RogueBasin.Items.Lantern();

            gobbo.PickUpItem(map);

        }

        private void SpawnDemonUniques()
        {
            int minDistance = 5;
            int loopCount = 0;

            Point location = new Point();

            //Unique spider

            LogFile.Log.LogEntryDebug("Adding demon uniques...", LogDebugLevel.Medium);

            Creatures.MaleficarumUnique rat = new RogueBasin.Creatures.MaleficarumUnique();
            int uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(5) + 2;
            Point uniqLoc = PlaceMonster(uniqueLevel, rat);

            //Spawn in with a random number of rats
            int noDran = 2 + Game.Random.Next(2);
            int noImp = 2 + Game.Random.Next(2);
            int noDemon =  3 + Game.Random.Next(4);

            for (int i = 0; i < noDran; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Drainer(), uniqueLevel, location));
            }

            for (int i = 0; i < noImp; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Imp(), uniqueLevel, location));
            }

            for (int i = 0; i < noDemon; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Demon(), uniqueLevel, location));
            }

            //Add his items

            Items.RestoreOrb dag = new RogueBasin.Items.RestoreOrb();

            rat.PickUpItem(dag);

            //Unique rat

            Creatures.OverlordUnique gobbo = new RogueBasin.Creatures.OverlordUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(5) + 3;
            uniqLoc = PlaceMonster(uniqueLevel, gobbo);

            //Spawn in with a random number of ferrets & goblins
            noDran = 2 + Game.Random.Next(2);
            noImp = 2 + Game.Random.Next(2);
             noDemon = 3 + Game.Random.Next(4);
            int noMed = 2;

            for (int i = 0; i < noDran; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Drainer(), uniqueLevel, location));
            }

            for (int i = 0; i < noImp; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Imp(), uniqueLevel, location));
            }

            for (int i = 0; i < noDemon; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Demon(), uniqueLevel, location));
            }

            for (int i = 0; i < noMed; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Meddler(), uniqueLevel, location));
            }

            //Add his items

            Items.GodSword map = new RogueBasin.Items.GodSword();

            gobbo.PickUpItem(map);

        }


        private void SpawnCryptUniques()
        {
            int minDistance = 4;
            int loopCount = 0;

            Point location = new Point();

            //Unique spider

            LogFile.Log.LogEntryDebug("Adding crypt uniques...", LogDebugLevel.Medium);

            Creatures.NecromancerUnique rat = new RogueBasin.Creatures.NecromancerUnique();
            int uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(4) + 2;
            Point uniqLoc = PlaceMonster(uniqueLevel, rat);

            //Spawn in with a random number of skels & zombs
                int noSkelArch = 2 + Game.Random.Next(3);
                int noSkel = 2 + Game.Random.Next(2);
                int noZomb = 2 + Game.Random.Next(2);

                for (int i = 0; i < noSkel; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                    } while (!dungeon.AddMonster(new Creatures.Skeleton(), uniqueLevel, location));
                }

            for (int i = 0; i < noSkelArch; i++)
                {
                    do
                    {
                        loopCount = 0;
                        do
                        {
                            location = dungeon.RandomWalkablePointInLevel(i);
                            loopCount++;
                        } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                    } while (!dungeon.AddMonster(new Creatures.SkeletalArcher(), uniqueLevel, location));
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
                        } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                    } while (!dungeon.AddMonster(new Creatures.Zombie(), uniqueLevel, location));
                }
        


            //Add his items

            Items.MetalArmour dag = new RogueBasin.Items.MetalArmour();

            rat.PickUpItem(dag);

            //Unique rat

            Creatures.GhoulUnique gobbo = new RogueBasin.Creatures.GhoulUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(4) + 3;
            uniqLoc = PlaceMonster(uniqueLevel, gobbo);

            //Spawn in with a random number of ferrets & goblins

            noSkelArch = 4 + Game.Random.Next(5);
            
            for (int i = 0; i < noSkelArch; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.SkeletalArcher(), uniqueLevel, location));
            }

            //Add his items

            Items.StaffPower map = new RogueBasin.Items.StaffPower();

            gobbo.PickUpItem(map);

        }


        private void SpawnOrcUniques()
        {
            int minDistance = 4;
            int loopCount = 0;

            Point location = new Point();

            //Unique spider

            LogFile.Log.LogEntryDebug("Adding forest uniques...", LogDebugLevel.Medium);

            Creatures.UrukUnique rat = new RogueBasin.Creatures.UrukUnique();
            int uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(3) + 2;
            Point uniqLoc = PlaceMonster(uniqueLevel, rat);

            //Spawn in with a random number of rats
            int noBugbear = 3 + Game.Random.Next(2);
            int noUruk = 2 + Game.Random.Next(3);
            int noOrcShaman = 1 + Game.Random.Next(1);

            for (int i = 0; i < noBugbear; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Bugbear(), uniqueLevel, location));
            }

            for (int i = 0; i < noUruk; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Uruk(), uniqueLevel, location));
            }
            for (int i = 0; i < noOrcShaman; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(rat, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.OrcShaman(), uniqueLevel, location));
            }

            //Add his items

            Items.HealingPotion dag = new RogueBasin.Items.HealingPotion();

            rat.PickUpItem(dag);

            //Unique rat

            Creatures.OrcShamanUnique gobbo = new RogueBasin.Creatures.OrcShamanUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(3) + 3;
            uniqLoc = PlaceMonster(uniqueLevel, gobbo);

            //Spawn in with a random number of ferrets & goblins
            int noOrc = 6 + Game.Random.Next(6);
            noBugbear = 3 + Game.Random.Next(2);
            noOrcShaman = 2 + Game.Random.Next(2);

            for (int i = 0; i < noBugbear; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Bugbear(), uniqueLevel, location));
            }

            for (int i = 0; i < noOrc; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.Orc(), uniqueLevel, location));
            }

            for (int i = 0; i < noOrcShaman; i++)
            {
                do
                {
                    loopCount = 0;
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                        loopCount++;
                    } while (Game.Dungeon.GetDistanceBetween(gobbo, location) > minDistance && loopCount < maxLoopCount);
                } while (!dungeon.AddMonster(new Creatures.OrcShaman(), uniqueLevel, location));
            }


            //Add his items

            Items.LongSword map = new RogueBasin.Items.LongSword();

            gobbo.PickUpItem(map);

        }

        private void SpawnItems()
        {
            LogFile.Log.LogEntry("Generating items...");

            //Spawn a test item at level 0

            dungeon.AddItemNoChecks(new Items.Glove(), 0, dungeon.Player.LocationMap);
            dungeon.AddItemNoChecks(new Items.LeatherArmour(), 0, new Point(dungeon.Player.LocationMap.x + 1, dungeon.Player.LocationMap.y));
            dungeon.AddItemNoChecks(new Items.MetalArmour(), 0, new Point(dungeon.Player.LocationMap.x + 2, dungeon.Player.LocationMap.y));
            dungeon.AddItemNoChecks(new Items.GodSword(), 0, new Point(dungeon.Player.LocationMap.x, dungeon.Player.LocationMap.y + 1));
            dungeon.AddItemNoChecks(new Items.ShortSword(), 0, new Point(dungeon.Player.LocationMap.x + 1, dungeon.Player.LocationMap.y + 1));
            dungeon.AddItemNoChecks(new Items.LongSword(), 0, new Point(dungeon.Player.LocationMap.x + 2, dungeon.Player.LocationMap.y + 1));
            dungeon.AddItemNoChecks(new Items.PrettyDress(), 0, new Point(dungeon.Player.LocationMap.x - 1, dungeon.Player.LocationMap.y + 1));
            dungeon.AddItemNoChecks(new Items.MapGem(), 0, new Point(dungeon.Player.LocationMap.x - 2, dungeon.Player.LocationMap.y + 1));
            dungeon.AddItemNoChecks(new Items.Lantern(), 0, new Point(dungeon.Player.LocationMap.x - 3, dungeon.Player.LocationMap.y + 1));
            dungeon.AddItemNoChecks(new Items.HealingPotion(), 0, new Point(dungeon.Player.LocationMap.x, dungeon.Player.LocationMap.y + 2));
            dungeon.AddItemNoChecks(new Items.StaffPower(), 0, new Point(dungeon.Player.LocationMap.x - 1, dungeon.Player.LocationMap.y + 2));
            dungeon.AddItemNoChecks(new Items.RestoreOrb(), 0, new Point(dungeon.Player.LocationMap.x -2, dungeon.Player.LocationMap.y + 2));
            dungeon.AddItemNoChecks(new Items.Potion(), 0, new Point(dungeon.Player.LocationMap.x - 2, dungeon.Player.LocationMap.y + 3));
            dungeon.AddItemNoChecks(new Items.PotionDamUp(), 0, new Point(dungeon.Player.LocationMap.x - 1, dungeon.Player.LocationMap.y + 3));
            dungeon.AddItemNoChecks(new Items.PotionToHitUp(), 0, new Point(dungeon.Player.LocationMap.x, dungeon.Player.LocationMap.y + 3));
            dungeon.AddItemNoChecks(new Items.PotionSightUp(), 0, new Point(dungeon.Player.LocationMap.x, dungeon.Player.LocationMap.y -1));
            dungeon.AddItemNoChecks(new Items.PotionSpeedUp(), 0, new Point(dungeon.Player.LocationMap.x+1, dungeon.Player.LocationMap.y -1));
            dungeon.AddItemNoChecks(new Items.PotionMPRestore(), 0, new Point(dungeon.Player.LocationMap.x + 2, dungeon.Player.LocationMap.y - 1));
            
            //Add some berries
            
            //Cave levels
            /*
            for (int i = 0; i < 1; i++)
            {
                int totalPotions = 3 + Game.Random.Next(5);

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

                    PlaceItemOnLevel(potion, 2 + i, 0);
                }
            }*/

//            Point location = new Point(0, 0);
            
//            //Plot items

//            //These are max 1 per level
//            //Not all of them necessarily appear in all games
//            //They may be on the ground or carried by a creature

//            //Guarantee the glove (vamparic regeneration) on level 1 or 2
            
//            //dungeon.AddItem(new Items.Glove(), 0, dungeon.Player.LocationMap);
//            //The rest of the plot items are split between the remaining cave and ruined levels

//            List<Item> plotItems = new List<Item> { 
//                //special mode items (9)
//                new Items.Badge(), new Items.Boots(), new Items.Bracer(), new Items.GlassGem(),
//            new Items.LeadRing(), new Items.Lockpicks(), new Items.Sash() };

//            List<Item> plotLevelItems = new List<Item> {
//            //levelling items 
//            new Items.Backpack(), new Items.Book(), new Items.Medal(), new Items.Stone(), new Items.Flint() };
//            //glove is separate

//            Game.Dungeon.Player.TotalPlotItems = 15;
//            Game.Dungeon.Player.PlotItemsFound = 0;

//            /*
//            int level = 0;
//            List<int> levelsWithPlotItems = new List<int> { gloveLevel };

//            foreach (Item plotItem in plotItems)
//            {
//                do
//                {
//                    location = dungeon.RandomWalkablePointInLevel(level);

//                    //May want to specify a minimum distance from staircases??? TODO
//                } while (!dungeon.AddItem(plotItem, 0, location));
//            }
//            */

//            /*
//            //debug
//            //Give them all to me!

//            foreach (Item item in plotItems)
//            {
//                dungeon.Player.PickUpItem(item);
//            }
//            */
//            //Stick them all on the first level


//            int totalNormalLevels = noCaveLevels + noHallLevels + noRuinedLevels;

//            int level = 0;
//            int loopCount = 0;

//            int gloveLevel = 0;

//            do
//            {
//                location = dungeon.RandomWalkablePointInLevel(gloveLevel);
//            } while (!dungeon.AddItem(new Items.Glove(), gloveLevel, location));
            
//            //Include glove on level 1
//            List<int> levelsWithPlotItems = new List<int> { gloveLevel };

//            List<Item> itemsPlaced = new List<Item>();

//            //Guarantee Greaves or band on level 1
//            //Add the one we don't place to the array of spare items
//            Item firstItem;

//            if (Game.Random.Next(2) < 1)
//            {
//                firstItem = new Items.Greaves();
//                plotItems.Add(new Items.Band());
//            }
//            else
//            {
//                firstItem = new Items.Band();
//                plotItems.Add(new Items.Greaves());
//            }
//            do
//            {
//                location = dungeon.RandomWalkablePointInLevel(gloveLevel);
//            } while (!dungeon.AddItem(firstItem, gloveLevel, location));

//            itemsPlaced.Add(firstItem);

//            //Levelling items are distributed through the first 12 levels

//            for (int i = 0; i < plotLevelItems.Count; i++)
//            {

//                Item plotItem;
//                do
//                {
//                    plotItem = plotLevelItems[Game.Random.Next(plotLevelItems.Count)];
//                } while (itemsPlaced.Contains(plotItem));
                
//                loopCount = 0;

//                do
//                {
//                    level = 2 * i + 1;

//                    /*if (Game.Random.Next(10) < 2)
//                    {
//                        level--;
//                    }
//                    else*/
                    
//                    if (Game.Random.Next(10) < 5)
//                    {
//                        level++;
//                    }

//                    level = Game.Random.Next(noCaveLevels + noRuinedLevels);
//                    loopCount++;

//                } while (levelsWithPlotItems.Contains(level) && loopCount < 100);



//                //Put on the floor
//                do
//                {
//                    location = dungeon.RandomWalkablePointInLevel(level);

//                    //May want to specify a minimum distance from staircases??? TODO
//                } while (!dungeon.AddItem(plotItem, level, location));

//                levelsWithPlotItems.Add(level);
//                itemsPlaced.Add(plotItem);
//            }

//            //Distribute the move items randomly on other levels

//            //Guarantee a few special move items early on

//            for (int i = 0; i < plotItems.Count; i++)
//            {
//                //Find random level w/o plotItem
//                loopCount = 0;
//                Item plotItem;
               
//                do
//                {
//                    plotItem = plotItems[Game.Random.Next(plotItems.Count)];
//                } while (itemsPlaced.Contains(plotItem));

//                //guarantee some items early on
//              //  if (i < 2)
//               // {
//              //      level = Game.Random.Next(2);
//              //  }
//             //   else
////{
//                    do
//                    {
//                        level = Game.Random.Next(totalNormalLevels - 2);
//                        loopCount++;
//                    } while (levelsWithPlotItems.Contains(level) && loopCount < 100);
//               // }
//                levelsWithPlotItems.Add(level);
//                itemsPlaced.Add(plotItem);

//                //On the floor
//                //Find position in level and place item
//                do
//                {
//                    location = dungeon.RandomWalkablePointInLevel(level);

//                    //May want to specify a minimum distance from staircases??? TODO
//                } while (!dungeon.AddItem(plotItem, level, location));


//                /*
//                //50% chance they will be generated on a monster
//                bool putOnMonster = false;

//                if(Game.Random.Next(100) < plotItemOnMonsterChance)
//                    putOnMonster = true;

//                if (putOnMonster)
//                {
//                    //On a monster

//                    //Find a random monster on this level
//                    Monster monster = dungeon.RandomMonsterOnLevel(level);

//                    //If no monster, it'll go on the floor
//                    if (monster == null)
//                    {
//                        putOnMonster = false;
//                    }

//                    //Give it to him!
//                    monster.PickUpItem(plotItem);
//                }

//                if(!putOnMonster)
//                {
//                }
//                 * */
//            }
            
//            //Potions

//            //Cave levels

//            for (int i = 0; i < 1; i++)
//            {
//                int totalPotions = 3 + Game.Random.Next(5);

//                for (int j = 0; j < totalPotions; j++)
//                {
//                    int randomChance = Game.Random.Next(100);

//                    Item potion;

//                    if (randomChance < 45)
//                        potion = new Items.Potion();
//                    else if (randomChance < 65)
//                        potion = new Items.PotionToHitUp();
//                    else if (randomChance < 70)
//                        potion = new Items.PotionDamUp();
//                    else if (randomChance < 85)
//                        potion = new Items.PotionSpeedUp();
//                    else if (randomChance < 90)
//                        potion = new Items.PotionSightUp();

//                    else if (randomChance < 93)
//                        potion = new Items.PotionMajHealing();
//                    else if (randomChance < 94)
//                        potion = new Items.PotionMajDamUp();
//                    else if (randomChance < 95)
//                        potion = new Items.PotionMajSpeedUp();
//                    else if (randomChance < 96)
//                        potion = new Items.PotionMajSightUp();
//                    else if (randomChance < 97)
//                        potion = new Items.PotionSuperHealing();
//                    else if (randomChance < 98)
//                        potion = new Items.PotionSuperDamUp();
//                    else if (randomChance < 99)
//                        potion = new Items.PotionSuperToHitUp();
//                    else
//                        potion = new Items.PotionSuperSpeedUp();

//                    PlaceItemOnLevel(potion, i, potionOnMonsterChance);
//                }
//            }

//            for (int i = 1; i < noCaveLevels; i++)
//            {
//                int totalPotions = 3 + Game.Random.Next(5);

//                for (int j = 0; j < totalPotions; j++)
//                {
//                    int randomChance = Game.Random.Next(100);

//                    Item potion;

//                    if (randomChance < 45)
//                        potion = new Items.Potion();
//                    else if (randomChance < 65)
//                        potion = new Items.PotionToHitUp();
//                    else if (randomChance < 70)
//                        potion = new Items.PotionDamUp();
//                    else if (randomChance < 85)
//                        potion = new Items.PotionSpeedUp();
//                    else if (randomChance < 90)
//                        potion = new Items.PotionSightUp();

//                    else if (randomChance < 93)
//                        potion = new Items.PotionMajHealing();
//                    else if (randomChance < 94)
//                        potion = new Items.PotionMajDamUp();
//                    else if (randomChance < 95)
//                        potion = new Items.PotionMajSpeedUp();
//                    else if (randomChance < 96)
//                        potion = new Items.PotionMajSightUp();
//                    else if (randomChance < 97)
//                        potion = new Items.PotionSuperHealing();
//                    else if (randomChance < 98)
//                        potion = new Items.PotionSuperDamUp();
//                    else if (randomChance < 99)
//                        potion = new Items.PotionSuperToHitUp();
//                    else
//                        potion = new Items.PotionSuperSpeedUp();

//                    PlaceItemOnLevel(potion, i, potionOnMonsterChance);
//                }
//            }

//            for (int i = noRuinedLevels; i < noRuinedLevels + noCaveLevels - 2; i++)
//            {
//                int totalPotions = 3 + Game.Random.Next(5);

//                for (int j = 0; j < totalPotions; j++)
//                {
//                    int randomChance = Game.Random.Next(100);

//                    Item potion;

//                    if (randomChance < 5)
//                        potion = new Items.Potion();
//                    else if (randomChance < 10)
//                        potion = new Items.PotionToHitUp();
//                    else if (randomChance < 15)
//                        potion = new Items.PotionDamUp();
//                    else if (randomChance < 20)
//                        potion = new Items.PotionSpeedUp();
//                    else if (randomChance < 25)
//                        potion = new Items.PotionSightUp();

//                    else if (randomChance < 55)
//                        potion = new Items.PotionMajHealing();
//                    else if (randomChance < 65)
//                        potion = new Items.PotionMajDamUp();
//                    else if (randomChance < 75)
//                        potion = new Items.PotionMajSpeedUp();
//                    else if (randomChance < 85)
//                        potion = new Items.PotionMajSightUp();
//                    else if (randomChance < 93)
//                        potion = new Items.PotionSuperHealing();
//                    else if (randomChance < 95)
//                        potion = new Items.PotionSuperDamUp();
//                    else if (randomChance < 98)
//                        potion = new Items.PotionSuperToHitUp();
//                    else
//                        potion = new Items.PotionSuperSpeedUp();

//                    PlaceItemOnLevel(potion, i, potionOnMonsterChance);
//                }
//            }

//            for (int i = noRuinedLevels + noCaveLevels - 2; i < noRuinedLevels + noCaveLevels + noHallLevels; i++)
//            {
//                int totalPotions = 1 + Game.Random.Next(5);

//                for (int j = 0; j < totalPotions; j++)
//                {
//                    int randomChance = Game.Random.Next(100);

//                    Item potion;

//                    if (randomChance < 5)
//                        potion = new Items.Potion();
//                    else if (randomChance < 7)
//                        potion = new Items.PotionToHitUp();
//                    else if (randomChance < 9)
//                        potion = new Items.PotionDamUp();
//                    else if (randomChance < 11)
//                        potion = new Items.PotionSpeedUp();
//                    else if (randomChance < 14)
//                        potion = new Items.PotionSightUp();

//                    else if (randomChance < 35)
//                        potion = new Items.PotionMajHealing();
//                    else if (randomChance < 40)
//                        potion = new Items.PotionMajDamUp();
//                    else if (randomChance < 45)
//                        potion = new Items.PotionMajSpeedUp();
//                    else if (randomChance < 50)
//                        potion = new Items.PotionMajSightUp();
//                    else if (randomChance < 70)
//                        potion = new Items.PotionSuperHealing();
//                    else if (randomChance < 80)
//                        potion = new Items.PotionSuperDamUp();
//                    else if (randomChance < 90)
//                        potion = new Items.PotionSuperToHitUp();
//                    else
//                        potion = new Items.PotionSuperSpeedUp();

//                    PlaceItemOnLevel(potion, i, potionOnMonsterChance);
//                }
//            }

//            /*
//            //Add a few healing potions

//            for (int i = 0; i < 10; i++)
//            {
//                do
//                {
//                    location = dungeon.RandomWalkablePointInLevel(level);

//                    //May want to specify a minimum distance from staircases??? TODO
//                } while (!dungeon.AddItem(new Items.Potion(), 0, location));

                
//            }*/
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
            Dungeon dungeon = Game.Dungeon;

            //Levels

            //1: town
            //2: wilderness

            //Dungeon 1: levels 3-5: Caves
            //
            //Dungeon 2: levels 6-9: Ruined Halls
            //Dungeon 3: level 10-13: Halls

            //Set up the levels. Needs to be done here so the wilderness is initialized properly.
            int Dungeon1StartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(0);
            int Dungeon1EndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(0);

            int Dungeon2StartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(1);
            int Dungeon2EndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(1);

            int Dungeon3StartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(2);
            int Dungeon3EndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(2);

            int Dungeon4StartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(3);
            int Dungeon4EndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(3);

            int Dungeon5StartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(4);
            int Dungeon5EndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(4);

            int Dungeon6StartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(5);
            int Dungeon6EndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(5);

            int Dungeon7StartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(6);
            int Dungeon7EndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(6);


            //Make the generators

            MapGeneratorCave caveGen = new MapGeneratorCave();
            
            MapGeneratorBSPCave ruinedGen = new MapGeneratorBSPCave();
            MapGeneratorBSP hallsGen = new MapGeneratorBSP();

            //caveGen.ResetClosedSquareTerrainType();
            //caveGen.ResetOpenSquareTerrainType();
            //caveGen.SetClosedSquareTerrainType(MapTerrain.Mountains);
            //caveGen.SetClosedSquareTerrainType(MapTerrain.Volcano);
            //caveGen.SetOpenSquareTerrainType(MapTerrain.Empty);


            //Set width height of all maps to 80 / 25
            caveGen.Width = 80;
            caveGen.Height = 25;

            ruinedGen.Width = 80;
            ruinedGen.Height = 25;

            hallsGen.Width = 80;
            hallsGen.Height = 25;

            //level 0 - town

            TownGeneratorFromASCIIFile asciiTown = new TownGeneratorFromASCIIFile();

            try
            {
                asciiTown.LoadASCIIFile("town.txt");
                asciiTown.AddMapToDungeon();
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Failed to load town level!: " + ex.Message);
                throw new ApplicationException("Failed to load town level! Is the game installed correctly?");
            }

            //PC starts at start location
            dungeon.Player.LocationLevel = 0;
            dungeon.Player.LocationMap = asciiTown.GetPCStartLocation();
            dungeon.AddTrigger(0, asciiTown.GetPCStartLocation(), new Triggers.SchoolEntryTrigger());

            //level 1 - wilderness

            MapGeneratorFromASCIIFile asciiGen = new MapGeneratorFromASCIIFile();

            try
            {
                asciiGen.LoadASCIIFile("wilderness.txt");
                asciiGen.AddMapToDungeon();
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Failed to load wilderness level!: " + ex.Message);
                throw new ApplicationException("Failed to wilderness town level! Is the game installed correctly?");
            }

            int middleLevelsInDungeon = 2;

            //DUNGEON 1 - levels 2-5

            //Generate and add cave levels
            //Just a cave

            //level 2
            //top level has special up staircase leading to wilderness

            caveGen.DoFillInPass = true;
            caveGen.FillInChance = 15;
            caveGen.FillInTerrain = MapTerrain.Rubble;

            caveGen.GenerateMap();

            int levelNo = dungeon.AddMap(caveGen.Map);
            caveGen.AddDownStaircaseOnly(levelNo);
            caveGen.AddExitStaircaseOnly(levelNo);
            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);

            //Add a trigger here
            //dungeon.AddTrigger(levelNo, caveGen.GetPCStartLocation(), new Triggers.DungeonEntranceTrigger());
            
            //level 3-4

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                caveGen.GenerateMap();

                //AddStaircases needs to know the level number
                levelNo = dungeon.AddMap(caveGen.Map);
                caveGen.AddStaircases(levelNo);

                //Set light
                //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);
            }

            //level 5

            //Lowest level doens't have a downstaircase
            caveGen.GenerateMap();
            caveGen.AddWaterToCave(15, 4);

            levelNo = dungeon.AddMap(caveGen.Map);
            caveGen.AddUpStaircaseOnly(levelNo);

            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);


            //DUNGEON 2 - levels 6-9

            //Generate and add cave levels
            //Cave with water

            //level 6
            //top level has special up staircase leading to wilderness

            caveGen.DoFillInPass = false;

            caveGen.GenerateMap();

            levelNo = dungeon.AddMap(caveGen.Map);
            caveGen.AddWaterToCave(15, 4);
            caveGen.AddDownStaircaseOnly(levelNo);
            caveGen.AddExitStaircaseOnly(levelNo);
            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);

            //Add a trigger here
            //dungeon.AddTrigger(levelNo, caveGen.GetPCStartLocation(), new Triggers.DungeonEntranceTrigger());

            //level 7-8

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                caveGen.GenerateMap();
                caveGen.AddWaterToCave(15, 4);

                //AddStaircases needs to know the level number
                levelNo = dungeon.AddMap(caveGen.Map);
                caveGen.AddStaircases(levelNo);

                //Set light
                //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);
            }

            //level 9

            //Lowest level doens't have a downstaircase
            caveGen.GenerateMap();
            caveGen.AddWaterToCave(15, 4);

            levelNo = dungeon.AddMap(caveGen.Map);
            caveGen.AddUpStaircaseOnly(levelNo);

            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);


            //DUNGEON 3 - levels 10-13

            //Forested cave

            //level 10
            //top level has special up staircase leading to wilderness

            caveGen.ResetClosedSquareTerrainType();
            caveGen.ResetOpenSquareTerrainType();
            caveGen.SetClosedSquareTerrainType(MapTerrain.Forest);
            caveGen.SetOpenSquareTerrainType(MapTerrain.Grass);

            caveGen.GenerateMap();

            levelNo = dungeon.AddMap(caveGen.Map);
            caveGen.AddDownStaircaseOnly(levelNo);
            caveGen.AddExitStaircaseOnly(levelNo);
            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);

            //Add a trigger here
            //dungeon.AddTrigger(levelNo, caveGen.GetPCStartLocation(), new Triggers.DungeonEntranceTrigger());

            //level 11-12

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                caveGen.GenerateMap();
                caveGen.AddWaterToCave(15, 4);

                //AddStaircases needs to know the level number
                levelNo = dungeon.AddMap(caveGen.Map);
                caveGen.AddStaircases(levelNo);

                //Set light
                //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);
            }

            //level 13

            //Lowest level doens't have a downstaircase
            caveGen.GenerateMap();

            levelNo = dungeon.AddMap(caveGen.Map);
            caveGen.AddUpStaircaseOnly(levelNo);

            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);


            //DUNGEON 4 - levels 14-17

            //Old town

            //level 14
            //top level has special up staircase leading to wilderness
            
            Map hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
            levelNo = dungeon.AddMap(hallMap);

            hallsGen.AddDownStaircaseOnly(levelNo);
            hallsGen.AddExitStaircaseOnly(levelNo);

            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);

            //Add a trigger here
            //dungeon.AddTrigger(levelNo, hallsGen.GetUpStaircaseLocation(), new Triggers.DungeonEntranceTrigger());

            //level 15-16

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
                
                //AddStaircases needs to know the level number
                levelNo = dungeon.AddMap(hallMap);
                hallsGen.AddStaircases(levelNo);

                //Set light
               // Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);
            }

            //level 17

            //Lowest level doens't have a downstaircase
            hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));

            levelNo = dungeon.AddMap(hallMap);
            caveGen.AddUpStaircaseOnly(levelNo);

            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);



            //DUNGEON 5 - levels 18-21

            //Graveyard

            //Generate and add cave levels

            //level 18
            //top level has special up staircase leading to wilderness

           ruinedGen.AddWallType(MapTerrain.SkeletonWall);
           ruinedGen.AddWallType(MapTerrain.SkeletonWallWhite);
           ruinedGen.RubbleChance = 5;
           ruinedGen.AddRubbleType(MapTerrain.Gravestone);

            Map ruinedLevel = ruinedGen.GenerateMap(ruinedExtraCorridorDefinite + Game.Random.Next(ruinedExtraCorridorRandom));
            levelNo = dungeon.AddMap(ruinedLevel);
            ruinedGen.AddDownStaircaseOnly(levelNo);
            ruinedGen.AddExitStaircaseOnly(levelNo);

            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);

            //Add a trigger here
            //dungeon.AddTrigger(levelNo, ruinedGen.GetUpStaircaseLocation(), new Triggers.DungeonEntranceTrigger());

            //level 19-20

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                ruinedLevel = ruinedGen.GenerateMap(ruinedExtraCorridorDefinite + Game.Random.Next(ruinedExtraCorridorRandom));
                levelNo = dungeon.AddMap(ruinedLevel);
                ruinedGen.AddStaircases(levelNo);

                //Set light
                //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);
            }

            //level 21

            //Lowest level doens't have a downstaircase
            ruinedLevel = ruinedGen.GenerateMap(ruinedExtraCorridorDefinite + Game.Random.Next(ruinedExtraCorridorRandom));
            levelNo = dungeon.AddMap(ruinedLevel);

            ruinedGen.AddUpStaircaseOnly(levelNo);

            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);


            //DUNGEON 6 - levels 22-24

            //Ancient passage

            ruinedGen.ClearRubbleType();
            ruinedGen.AddRubbleType(MapTerrain.Rubble);

            ruinedGen.ClearWallType();
            ruinedGen.AddWallType(MapTerrain.Wall);
            ruinedGen.AddWallType(MapTerrain.Wall);
            ruinedGen.RubbleChance = 5;


            //Generate and add cave levels

            //level 22
            //top level has special up staircase leading to wilderness

            ruinedLevel = ruinedGen.GenerateMap(ruinedExtraCorridorDefinite + Game.Random.Next(ruinedExtraCorridorRandom));
            levelNo = dungeon.AddMap(ruinedLevel);
            ruinedGen.AddDownStaircaseOnly(levelNo);
            ruinedGen.AddExitStaircaseOnly(levelNo);

            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);

            //Add a trigger here
            //dungeon.AddTrigger(levelNo, ruinedGen.GetUpStaircaseLocation(), new Triggers.DungeonEntranceTrigger());

            //level 23-24

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                ruinedLevel = ruinedGen.GenerateMap(ruinedExtraCorridorDefinite + Game.Random.Next(ruinedExtraCorridorRandom));
                levelNo = dungeon.AddMap(ruinedLevel);
                ruinedGen.AddStaircases(levelNo);

                //Set light
                //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);
            }

            //level 25

            //Lowest level doens't have a downstaircase
            ruinedLevel = ruinedGen.GenerateMap(ruinedExtraCorridorDefinite + Game.Random.Next(ruinedExtraCorridorRandom));
            levelNo = dungeon.AddMap(ruinedLevel);

            ruinedGen.AddUpStaircaseOnly(levelNo);

            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);


            //DUNGEON 7 - levels 25-28

            //Volcano
            //level 25
            //top level has special up staircase leading to wilderness

            caveGen.ResetClosedSquareTerrainType();
            caveGen.ResetOpenSquareTerrainType();
            caveGen.SetClosedSquareTerrainType(MapTerrain.Mountains);
            caveGen.SetClosedSquareTerrainType(MapTerrain.Volcano);
            caveGen.SetOpenSquareTerrainType(MapTerrain.Empty);

            caveGen.GenerateMap();

            levelNo = dungeon.AddMap(caveGen.Map);
            caveGen.AddDownStaircaseOnly(levelNo);
            caveGen.AddExitStaircaseOnly(levelNo);
            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);

            //Add a trigger here
            //dungeon.AddTrigger(levelNo, caveGen.GetPCStartLocation(), new Triggers.DungeonEntranceTrigger());

            //level 26-27

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                caveGen.GenerateMap();
                //caveGen.AddWaterToCave(15, 4);

                //AddStaircases needs to know the level number
                levelNo = dungeon.AddMap(caveGen.Map);
                caveGen.AddStaircases(levelNo);

                //Set light
                Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);
            }

            //level 28

            //Lowest level doens't have a downstaircase
            LastMapGeneratorFromASCIIFile lastGen = new LastMapGeneratorFromASCIIFile();

            try
            {
                lastGen.LoadASCIIFile("dragonlevel.txt");
                lastGen.AddMapToDungeon();
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Failed to load last level!: " + ex.Message);
                throw new ApplicationException("Failed to load last level! Is the game installed correctly?");
            }

            //Set light
            //Game.Dungeon.Levels[levelNo].LightLevel = GetLightLevel(levelNo);



            /*
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
            */


            //Build TCOD maps
            //Necessary so connectivity checks on items and monsters can work
            //Only place where this happens now
            CalculateWalkableAndTCOD();

        }

        private void CalculateWalkableAndTCOD() {

            dungeon.RecalculateWalkable();

            //TCOD routine uses Walkable flag set above
            dungeon.RefreshTCODMaps();
        }

        private double GetLightLevel(int levelNo)
        {
            int lightDelta = 5 + Game.Random.Next(15);
            lightDelta -= levelNo;

            double lightLevel = lightDelta / 10.0;

            if (lightLevel < 0.8)
                lightLevel = 0.8;

            if (lightLevel > 2.0)
            {
                lightLevel = 2.0;
            }
            return lightLevel;
        }
    }
}
