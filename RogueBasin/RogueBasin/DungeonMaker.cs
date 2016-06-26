using GraphMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RogueBasin
{
    public enum GameDifficulty
    {
        Easy, Medium, Hard
    }


    public class DungeonMaker
    {
        public GameDifficulty difficulty { get; set; }

        int maxLoopCount = 500;

        int ruinedExtraCorridorDefinite = 5;
        int ruinedExtraCorridorRandom = 10;

        int hallsExtraCorridorDefinite = 0;
        int hallsExtraCorridorRandom = 8;

        //Non-Public as can't be serialized
        SerializableDictionary<int, MapGenerator> levelGen = new SerializableDictionary<int, MapGenerator>();
        List<int> randomCreatureSeeds = new List<int>();
        List<int> randomItemSeeds = new List<int>();
        List<int> randomObjectiveSeeds = new List<int>();


        public DungeonMaker()
        {
        }

        public DungeonMaker(GameDifficulty diff) {
            difficulty = diff;

        }

        /// <summary>
        /// Sets up player, dungeons, creatures, uniques
        /// </summary>
        /// <returns></returns>
        public Dungeon SpawnNewDungeon()
        {
            Game.Dungeon = new Dungeon(); //not classy but I have to do it here since some other classes (e.g. map gen) call it
            Game.Dungeon.DungeonMaker = this;
            Game.Dungeon.Difficulty = difficulty;

            SetupPlayer();
            
            //Player start location must be set in here
            //SetupMapsFlatline();

            
            //bool decision = Screen.Instance.YesNoQuestion("Locks?");
            /*
            bool decision = true;

            if (decision)
            {

                //bool decision2 = Screen.Instance.YesNoQuestion("Multiple?");
                bool decision2 = true;

                if (decision2)
                    SetupMapsGraphingDemo(true, 4);
                else
                    SetupMapsGraphingDemo(true, 1);
            }
            else
            {
                //bool decision2 = false;
                bool decision2 = Screen.Instance.YesNoQuestion("Cycles?");

                if (decision2)
                    SetupMapsGraphingDemoCycles();
                else
                    SetupMapsGraphingDemo(false, 0);

            }
  */


            SetupMapsFlatline();
            
            return Game.Dungeon;
        }


        private void SetupPlayer()
        {
            Player player = Game.Dungeon.Player;
            player.LocationLevel = 0;
            //player.Representation = '@';

            //player.CalculateCombatStats();
            //5player.Hitpoints = player.MaxHitpoints;
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

        /// <summary>
        /// Select a monster based on the probabilities in the list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Place a monster on level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="monster"></param>
        /// <returns></returns>
        private Point PlaceMonster(int level, Monster monster)
        {
            Point location = new Point();
            int loopCount = 0;
            do
            {
                location = Game.Dungeon.RandomWalkablePointInLevel(level);
                loopCount++;
            } while (!Game.Dungeon.AddMonster(monster, level, location) && loopCount < 1000);

            if (loopCount < 1000)
            {
                CheckSpecialMonsterGroups(monster, level);
            }
            else
            {
                LogFile.Log.LogEntryDebug("Error: Couldn't place monster", LogDebugLevel.High);
            }

            return location;
        }


        /// <summary>
        /// Spawn a dungeon for the first time - creatures and items.
        /// Also used by the respawner for a second visit.
        /// </summary>
        /// <param name="dungeonID"></param>
        private void SpawnDungeon(int dungeonID) {

            int budgetScale = 1;

            switch(dungeonID) {

                case 0:
                    SpawnCaveCreatures(budgetScale);
                    SpawnCaveUniqueFollowers();
                    SpawnCaveItems(budgetScale);
                    break;
                case 1:
                    SpawnWaterCaveCreatures(budgetScale);
                    SpawnWaterCaveUniqueFollowers();
                    SpawnWaterCaveItems(budgetScale);
                    break;
                case 2:
                    SpawnForestCreatures(budgetScale);
                    SpawnForestUniqueFollowers();
                    SpawnForestItems(budgetScale);
                    break;
                case 3:
                    SpawnOrcHutCreatures(budgetScale);
                    SpawnOrcUniqueFollowers();
                    SpawnOrcItems(budgetScale);
                    break;
                case 4:
                    SpawnCryptCreatures(budgetScale);
                    SpawnCryptUniqueFollowers();
                    break;
                case 5:
                    SpawnDemonCreatures(budgetScale);
                    SpawnDemonUniqueFollowers();
                    SpawnDemonItems(budgetScale);
                    break;
                case 6:
                    SpawnPrinceCreatures(budgetScale);
                    SpawnPrinceItems(budgetScale);
                    break;
            }
        }

        /// <summary>
        /// Respawn a dungeon for a second visit. Keep uniques. Kill other monsters and items.
        /// Respawn items, monsters and unique followers
        /// </summary>
        /// <param name="dungeonID"></param>
        public void ReSpawnDungeon(int missionLevel, bool useOldSeed)
        {
            List<Monster> monsters = Game.Dungeon.Monsters;

            foreach (Monster m in monsters)
            {
                if (m.LocationLevel == missionLevel && m.LocationLevel == missionLevel)
                {
                    Game.Dungeon.KillMonster(m, true);
                }
            }

            //Remove all items
            List<Item> items = Game.Dungeon.Items;

            foreach (Item i in items)
            {
                if (i.LocationLevel == missionLevel && i.LocationLevel == missionLevel)
                {
                    i.InInventory = true;
                }
            }

            List<int> levelToRespawn = new List<int>();
            levelToRespawn.Add(missionLevel);

            //If we've not got level gen, we need to rebuild
            if (levelGen.Count == 0)
            {
                //Load / save cycle we have to completely start again

                //How evil is this???
                Game.Dungeon.Levels.Clear();
                Game.Dungeon.Monsters.Clear();
                Game.Dungeon.Items.Clear();
                Game.Dungeon.Triggers.Clear();
                //Game.Dungeon.Features.Clear(); //find another way!
                SetupMapsFlatline();

                //Otherwise respawn
                Game.Dungeon.ReplaceMap(missionLevel, levelGen[missionLevel].GetOriginalMap());

                //Recalculate walkable
                CalculateWalkableAndTCOD();
            }
            else
            {
                //Otherwise respawn
                Game.Dungeon.ReplaceMap(missionLevel, levelGen[missionLevel].GetOriginalMap());

                //Recalculate walkable
                CalculateWalkableAndTCOD();

                //Respawn the creatures, items and unique followers
                SpawnCreaturesFlatline(levelToRespawn, levelGen, useOldSeed);
                SpawnItemsFlatline(levelToRespawn, levelGen, useOldSeed);
                SpawnObjectivesFlatline(levelToRespawn, levelGen, useOldSeed);
            }
        }

        //Spawning shared variables
        int looseGroupDist;
        int lonerChance;
        int maxGroupSize;
        int minGroupSize;

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
            caveList.Add(new MonsterCommon(new Creatures.Pixie(), 10));

            looseGroupDist = 8;
            lonerChance = 50;
            maxGroupSize = 6;
            minGroupSize = 3;

            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int levelNo = dungeonStartLevel; levelNo <= dungeonEndLevel; levelNo++)
            {
                GroupAndDisperseMonsters(levelNo, monsterBudgets[levelNo - dungeonStartLevel], caveList);
            }

            SetLightLevelUniversal(dungeonStartLevel, dungeonEndLevel, 5);

        }
        private void SpawnCaveCreatures(int budgetScale)
        {
            //Dungeon 0: CAVE

            //Monster budget per level for the 4 levels
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

            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int levelNo = dungeonStartLevel; levelNo <= dungeonEndLevel; levelNo++)
            {
                GroupAndDisperseMonsters(levelNo, monsterBudgets[levelNo - dungeonStartLevel], caveList);
            }

            SetLightLevelUniversal(dungeonStartLevel, dungeonEndLevel, 8);

        }

        private void SpawnCaveItems(int budgetScale)
        {
            int dungeonID = 0;

            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

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

            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

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

        private void SpawnPrinceItems(int budgetScale)
        {
            int dungeonID = 6;

            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            int totalPotions = 1 + Game.Random.Next(3);
            /*
            //bonus potions for the dragon level
            for (int j = 0; j < totalPotions; j++)
            {
                int randomChance = Game.Random.Next(100);

                Item potion;

                if (randomChance < 50)
                    potion = new Items.Potion();
                else
                    potion = new Items.PotionMPRestore();

                PlaceItemOnLevel(potion, dungeonEndLevel, 0);
            }*/


            for (int i = dungeonStartLevel; i <= dungeonEndLevel; i++)
            {

                //Spawn bonus potions

                totalPotions = 2 + Game.Random.Next(4);

                for (int j = 0; j < totalPotions; j++)
                {
                    int randomChance = Game.Random.Next(100);

                    Item potion;

                    if (randomChance < 25)
                        potion = new Items.PotionSpeedUp();
                    else if(randomChance < 50)
                        potion = new Items.PotionSightUp();
                    else if(randomChance < 75)
                        potion = new Items.PotionToHitUp();
                    else
                        potion = new Items.PotionDamUp();

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

        private void SpawnForestItems(int budgetScale)
        {
            int dungeonID = 2;

            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

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

                totalPotions = 1 + Game.Random.Next(3);

                //Some more magic mushrooms
                /*
                for (int j = 0; j < totalPotions; j++)
                {
                    int randomChance = Game.Random.Next(100);

                    Item potion;

                    potion = new Items.PotionMPRestore();

                    PlaceItemOnLevel(potion, i, 50);
                }*/
            }

        }

        private void SpawnOrcItems(int budgetScale)
        {
            int dungeonID = 3;

            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

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

                totalPotions = Game.Random.Next(3);

                for (int j = 0; j < totalPotions; j++)
                {
                    int randomChance = Game.Random.Next(2);

                    Item potion;

                        potion = new Items.PotionMPRestore();

                    PlaceItemOnLevel(potion, i, 50);
                }
            }

        }

        private void SpawnDemonItems(int budgetScale)
        {
            int dungeonID = 5;

            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int i = dungeonStartLevel; i <= dungeonEndLevel; i++)
            {
                int totalPotions = Game.Random.Next(3);

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

            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

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

            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

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

            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

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

            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

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

            monsterBudgets.Add(budgetScale * 200);
            monsterBudgets.Add(budgetScale * 250);
            monsterBudgets.Add(budgetScale * 300);
            monsterBudgets.Add(budgetScale * 320);

            //build commonness list for caves

            //Gives about 50 monsters
            List<MonsterCommon> caveList = new List<MonsterCommon>();
            caveList.Add(new MonsterCommon(new Creatures.Imp(), 10));
            caveList.Add(new MonsterCommon(new Creatures.Demon(), 2));
            caveList.Add(new MonsterCommon(new Creatures.Skeleton(), 20));
            caveList.Add(new MonsterCommon(new Creatures.SkeletalArcher(), 20));
            caveList.Add(new MonsterCommon(new Creatures.Pixie(), 10));
            caveList.Add(new MonsterCommon(new Creatures.Ferret(), 30));
            caveList.Add(new MonsterCommon(new Creatures.Faerie(), 10));
            caveList.Add(new MonsterCommon(new Creatures.Ogre(), 20));
            caveList.Add(new MonsterCommon(new Creatures.Ghoul(), 10));
            caveList.Add(new MonsterCommon(new Creatures.Rat(), 20));
            caveList.Add(new MonsterCommon(new Creatures.OrcShaman(), 5));
            caveList.Add(new MonsterCommon(new Creatures.GoblinWitchdoctor(), 5));

            looseGroupDist = 6;
            lonerChance = 30;
            maxGroupSize = 6;
            minGroupSize = 3;

            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int levelNo = dungeonStartLevel; levelNo <= dungeonEndLevel; levelNo++)
            {
                GroupAndDisperseMonsters(levelNo, monsterBudgets[levelNo - dungeonStartLevel], caveList);
            }

            SetLightLevelUniversal(dungeonStartLevel, dungeonEndLevel, 0);
        }


        /// <summary>
        /// Add a monster with a random patrol. Needs the mapgenerator of the level in question
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="mapGen"></param>
        private void AddMonsterLinearPatrol(MonsterFightAndRunAI monster, int level, MapGenerator mapGen)
        {

            Dungeon dungeon = Game.Dungeon;

            Point startLocation;

            int loops = 0;
            int maxLoops = 50;

            bool success = false;

            do
            {
                CreaturePatrol patrol = mapGen.CreatureStartPosAndWaypointsSisterRooms(monster.GetPatrolRotationClockwise(), 3);
                monster.Waypoints = patrol.Waypoints;
                startLocation = patrol.StartPos;

                loops++;

                success = Game.Dungeon.AddMonster(monster, level, startLocation);

                //Linear patrols start from the centre of rooms so monsters often overlap in small number of room levels
                //Try with a random point
                if(!success)
                    success = Game.Dungeon.AddMonster(monster, level, patrol.StartRoom.RandomPointInRoom());

            } while (!success && loops < maxLoops);

            if (loops == maxLoops)
            {
                LogFile.Log.LogEntryDebug("Failed to place patrolling monster: " + monster.Representation, LogDebugLevel.High);
            }
        }

        /// <summary>
        /// Add a monster with a random patrol. Needs the mapgenerator of the level in question
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="mapGen"></param>
        private bool AddMonsterFarFromLocation(Monster monster, Point location, int level, MapGenerator mapGen)
        {

            //Offset location

            int maxLoops = 50;
            int loops = 0;

            Point toPlaceLoc = new Point(location);

            int distance = 40;

            do
            {
                toPlaceLoc = new Point(location.x + (int)Gaussian.BoxMuller(distance, 5), location.y + (int)Gaussian.BoxMuller(distance, 5));

                loops++;
                distance--;

            } while (!Game.Dungeon.AddMonster(monster, level, toPlaceLoc) && loops < maxLoops);

            if (loops == maxLoops)
            {
                LogFile.Log.LogEntryDebug("Failed to place: " + monster.Representation + " far from to: " + location + " reverting to random placement", LogDebugLevel.Medium);

                loops = 0;

                do
                {
                    toPlaceLoc = Game.Dungeon.RandomWalkablePointInLevel(level);
                    loops++;

                } while (!Game.Dungeon.AddMonster(monster, level, toPlaceLoc) && loops < maxLoops);

                LogFile.Log.LogEntryDebug("Failed to place: " + monster.Representation + " giving up", LogDebugLevel.High);
                return false;
            }

            LogFile.Log.LogEntryDebug("Item " + monster.Representation + " placed at: " + location.ToString(), LogDebugLevel.High);

            return true;
        }

        /// <summary>
        /// Add a monster with a random patrol. Needs the mapgenerator of the level in question
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="mapGen"></param>
        private void AddMonsterSquarePatrol(MonsterFightAndRunAI monster, int level, MapGenerator mapGen)
        {

            Dungeon dungeon = Game.Dungeon;

            Point startLocation;

            int loops = 0;
            int maxLoops = 50;

            do
            {
                CreaturePatrol patrol = mapGen.CreatureStartPosAndWaypoints(monster.GetPatrolRotationClockwise());
                monster.Waypoints = patrol.Waypoints;
                startLocation = patrol.StartPos;

                loops++;
            } while (!Game.Dungeon.AddMonster(monster, level, startLocation) && loops < maxLoops);

            if (loops == maxLoops)
            {
                LogFile.Log.LogEntryDebug("Failed to place patrolling monster: " + monster.Representation, LogDebugLevel.High);
            }
        }

        private void AddMonstersEqualDistribution(List<Monster> monster, int level, MapGenerator mapGen)
        {
            //Get the number of rooms
            List<RoomCoords> rooms = mapGen.GetAllRooms();

            int noMonsters = monster.Count;
            int noRooms = rooms.Count;

            LogFile.Log.LogEntryDebug("No rooms: " + noRooms + " Total monsters to place (level: " + level + "): " + noMonsters, LogDebugLevel.Medium);

            //Distribution amongst rooms, mostly evenly

            double[] roomMonsterRatio = new double[noRooms];

            for (int i = 0; i < noRooms; i++)
            {
                roomMonsterRatio[i] = Math.Max(0, Gaussian.BoxMuller(5, 1));
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
            int maxLoops = 10;

            for (int r = 0; r < noRooms; r++)
            {

                int monstersToPlaceInRoom = monstersPerRoom[r];

                int loops = 0;
                for (int m = 0; m < monstersToPlaceInRoom; m++)
                {
                    if(monsterPos >= monster.Count) {
                        LogFile.Log.LogEntryDebug("Trying to place too many monsters", LogDebugLevel.High);
                        monsterPos++;
                        continue;
                    }

                    Monster mon = monster[monsterPos];

                    Point location;
                    do
                    {
                        location = rooms[r].RandomPointInRoom();
                        loops++;
                    } while (!Game.Dungeon.AddMonster(mon, level, location) && loops < maxLoops);

                    monsterPos++;

                    if (loops == maxLoops)
                    {
                        LogFile.Log.LogEntryDebug("Failed to place: " + mon.Representation, LogDebugLevel.High);
                    }
                }
            }
            
        }

        /// <summary>
        /// Tries to add an item close to a location, puts it anywhere if that's not possible
        /// </summary>
        /// <param name="item"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        private bool AddItemCloseToLocation(Item item, int level, Point location)
        {

            //Offset location

            int maxLoops = 50;
            int loops = 0;

            Point toPlaceLoc = new Point(location);

            do
            {
                toPlaceLoc = new Point(location.x + (int)Gaussian.BoxMuller(2, 2), location.y + (int)Gaussian.BoxMuller(2, 2));
                loops++;

            } while (!Game.Dungeon.AddItem(item, level, toPlaceLoc) && loops < maxLoops);

            if (loops == maxLoops)
            {
                LogFile.Log.LogEntryDebug("Failed to place: " + item.Representation + " close to: " + location + " reverting to random placement", LogDebugLevel.Medium);

                loops = 0;

                do
                {
                    toPlaceLoc = Game.Dungeon.RandomWalkablePointInLevel(level);
                    loops++;

                } while (!Game.Dungeon.AddItem(item, level, toPlaceLoc) && loops < maxLoops);

                LogFile.Log.LogEntryDebug("Failed to place: " + item.Representation + " giving up", LogDebugLevel.High);
                return false;
            }

            LogFile.Log.LogEntryDebug("Item " + item.Representation + " placed at: " + location.ToString(), LogDebugLevel.High);

            return true;

        }


        /// <summary>
        /// Tries to add an item close to a location, puts it anywhere if that's not possible
        /// </summary>
        /// <param name="item"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        private bool AddItemAtLocation(Item item, int level, Point location)
        {

          Point toPlaceLoc = new Point(location);

          if(!Game.Dungeon.AddItem(item, level, toPlaceLoc)) {

              LogFile.Log.LogEntryDebug("Failed to place: " + item.Representation + " giving up", LogDebugLevel.High);
              return false;
          }

            return true;
        }

        //Copy of monster one
        private void AddItemsEqualDistribution(List<Item> monster, int level, MapGenerator mapGen)
        {
            //Get the number of rooms
            List<RoomCoords> rooms = mapGen.GetAllRooms();

            int noMonsters = monster.Count;
            int noRooms = rooms.Count;

            LogFile.Log.LogEntryDebug("No rooms: " + noRooms + " Total items to place (level: " + level + "): " + noMonsters, LogDebugLevel.Medium);

            //Distribution amongst rooms, mostly evenly
            //(better to use a norm dist here)

            double[] roomMonsterRatio = new double[noRooms];

            for (int i = 0; i < noRooms; i++)
            {
                roomMonsterRatio[i] = Math.Max(0, Gaussian.BoxMuller(5, 1));
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

                //In monster levels. A level 3 monster will cost 3 of these.
                double monsterBudget = roomMonsterRatio[i] * ratioToTotalMonsterBudget + remainder;

                double actualMonstersToPlace = Math.Floor(monsterBudget);
                //This is a historic bug - should be actualMonstersToPlace not monstersOfThisLevel. Still, the game is now balanced to this so I can't change it!
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

            LogFile.Log.LogEntryDebug("Total items actually placed (level: " + level + "): " + totalMonsters, LogDebugLevel.Medium);

            if (totalMonsters < noMonsters)
            {
                //find room with no items
                int i = 0;
                for (; i < noRooms; i++)
                {
                    if (monstersPerRoom[i] == 0)
                        break;
                }

                if(i == noRooms)
                    monstersPerRoom[0] += noMonsters - totalMonsters;
                else
                    monstersPerRoom[i] += noMonsters - totalMonsters;
                LogFile.Log.LogEntryDebug("Compensating (level: " + level + "): " + (noMonsters - totalMonsters) + " extra items", LogDebugLevel.Medium);
            }

            //Place monsters in rooms

            Dungeon dungeon = Game.Dungeon;

            int monsterPos = 0;
            int maxLoops = 10;

            for (int r = 0; r < noRooms; r++)
            {

                int monstersToPlaceInRoom = monstersPerRoom[r];

                int loops = 0;
                for (int m = 0; m < monstersToPlaceInRoom; m++)
                {
                    if (monsterPos >= monster.Count)
                    {
                        LogFile.Log.LogEntryDebug("Trying to place too many items", LogDebugLevel.High);
                        monsterPos++;
                        continue;
                    }

                    Item mon = monster[monsterPos];

                    Point location;
                    do
                    {
                        location = rooms[r].RandomPointInRoom();
                        LogFile.Log.LogEntryDebug("Item " + mon.Representation + " at: " + location.ToString(), LogDebugLevel.Medium);

                        loops++;
                    } while (!Game.Dungeon.AddItem(mon, level, location) && loops < maxLoops);

                    monsterPos++;

                    if (loops == maxLoops)
                    {
                        LogFile.Log.LogEntryDebug("Failed to place: " + mon.Representation, LogDebugLevel.High);
                    }
                }
            }

        }

        /// <summary>
        /// Add a monster at a random walkable point in the dungeon
        /// Queries the dungeon for walkable places
        /// </summary>
        /// <param name="monster"></param>
        private void AddMonsterRandomWalkablePoint(Monster monster, int level) {

            Dungeon dungeon = Game.Dungeon;

            Point location;

            int loops = 0;
            int maxLoops = 50;

            do {
                location = Game.Dungeon.RandomWalkablePointInLevel(level);
                loops++;
            } while (!Game.Dungeon.AddMonster(monster, level, location) && loops < maxLoops);

            if(loops == maxLoops) {
                LogFile.Log.LogEntryDebug("Failed to place: " + monster.Representation, LogDebugLevel.High);
            }
        }


         private void SpawnCreaturesAndItems() {

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
            
            monstersToAdd.Add(new Creatures.Goblin());
            monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin()); monstersToAdd.Add(new Creatures.Goblin());


            monstersToAdd.Add(new Creatures.Mushroom());
            monstersToAdd.Add(new Creatures.Mushroom());
            monstersToAdd.Add(new Creatures.Mushroom());
            monstersToAdd.Add(new Creatures.Mushroom());
            monstersToAdd.Add(new Creatures.Mushroom());
            monstersToAdd.Add(new Creatures.Mushroom());
            monstersToAdd.Add(new Creatures.Mushroom());
            monstersToAdd.Add(new Creatures.Mushroom());
            monstersToAdd.Add(new Creatures.Mushroom());
            monstersToAdd.Add(new Creatures.Mushroom());
            monstersToAdd.Add(new Creatures.Mushroom());
            monstersToAdd.Add(new Creatures.Mushroom());

            monstersToAdd.Add(new Creatures.Statue());
            monstersToAdd.Add(new Creatures.Statue()); monstersToAdd.Add(new Creatures.Statue()); monstersToAdd.Add(new Creatures.Statue()); monstersToAdd.Add(new Creatures.Statue()); monstersToAdd.Add(new Creatures.Statue()); monstersToAdd.Add(new Creatures.Statue()); monstersToAdd.Add(new Creatures.Statue()); monstersToAdd.Add(new Creatures.Statue());

            foreach (Monster monster in monstersToAdd)
            {
                Point location = new Point();
                do
                {
                    location = Game.Dungeon.RandomWalkablePointInLevel(2);
                } while (!Game.Dungeon.AddMonster(monster, 2, location));

            }
         }

         /// <summary>
         /// Sets the light level for all levels in a dungeon
         /// Updates all monsters on the level
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
                     monster.CalculateSightRadius();
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

             int innerLoopCount = 0;
             int outerLoopCount = 0;
             do
             {
                 location = Game.Dungeon.RandomWalkablePointInLevel(levelNo);

                 innerLoopCount++;

                 if (Utility.GetDistanceBetween(centerLoc, location) > looseGroupDist && innerLoopCount < 50)
                     continue;

                 outerLoopCount++;

             } while (!Game.Dungeon.AddMonster(monsterToPlace, levelNo, location) && outerLoopCount < 50);

             if(outerLoopCount != 50)
                CheckSpecialMonsterGroups(monsterToPlace, levelNo);

             return location;
         }

        /// <summary>
        /// Add cohort monsters close to a unique. Spawns new copies of the cohort monster type passed in (does not use the passed object)
        /// </summary>
        /// <param name="monsterType"></param>
        /// <param name="noMonsters"></param>
        /// <param name="masterMonser"></param>
        /// <param name="minDistance"></param>
        /// <param name="levelNo"></param>
        /// <returns></returns>
         private bool AddMonstersCloseToMaster(Monster monsterType, int noMonsters, Creature masterMonser, int minDistance, int levelNo) 
         {
             Point location;

             int outerLoopCount = 0;

             for (int i = 0; i < noMonsters; i++)
             {
                 do
                 {
                     int loopCount = 0;
                     do
                     {
                         location = Game.Dungeon.RandomWalkablePointInLevel(i);
                         loopCount++;
                     } while (Utility.GetDistanceBetween(masterMonser, location) > minDistance && loopCount < maxLoopCount);
                     outerLoopCount++;
                 } while (!Game.Dungeon.AddMonster(NewMonsterOfType(monsterType), levelNo, location) && outerLoopCount < 50);
             }

             //Failed to add monster
             if (outerLoopCount == 50)
             {
                 LogFile.Log.LogEntryDebug("Failed to place a monster near master", LogDebugLevel.Medium);
                 return false;
             }
             return true;
         }

        /// <summary>
        /// Returns a new monster of this type
        /// </summary>
        /// <param name="monsterType"></param>
        /// <returns></returns>
         private Monster NewMonsterOfType(Monster monsterType)
         {
             if (monsterType is Creatures.Bat)
                 return new Creatures.Bat();

             if (monsterType is Creatures.BlackUnicorn)
                 return new Creatures.BlackUnicorn();

             if (monsterType is Creatures.Bugbear)
                 return new Creatures.Bugbear();

             if (monsterType is Creatures.Demon)
                 return new Creatures.Demon();

             if (monsterType is Creatures.Drainer)
                 return new Creatures.Drainer();

             if (monsterType is Creatures.Faerie)
                 return new Creatures.Faerie();

             if (monsterType is Creatures.Ferret)
                 return new Creatures.Ferret();

             if (monsterType is Creatures.Ghoul)
                 return new Creatures.Ghoul();

             if (monsterType is Creatures.Goblin)
                 return new Creatures.Goblin();

             if (monsterType is Creatures.GoblinWitchdoctor)
                return new Creatures.GoblinWitchdoctor();

             if (monsterType is Creatures.Imp)
                 return new Creatures.Imp();

             if (monsterType is Creatures.Maleficarum)
                 return new Creatures.Maleficarum();

             if (monsterType is Creatures.Meddler)
                 return new Creatures.Meddler();

             if (monsterType is Creatures.Necromancer)
                 return new Creatures.Necromancer();

             if (monsterType is Creatures.Nymph)
                 return new Creatures.Nymph();

             if (monsterType is Creatures.Ogre)
                 return new Creatures.Ogre();

             if (monsterType is Creatures.Orc)
                 return new Creatures.Orc();

             if (monsterType is Creatures.OrcShaman)
                 return new Creatures.OrcShaman();

             if (monsterType is Creatures.Overlord)
                 return new Creatures.Overlord();

             if (monsterType is Creatures.Peon)
                 return new Creatures.Peon();

             if (monsterType is Creatures.Pixie)
                 return new Creatures.Pixie();

             if (monsterType is Creatures.Rat)
                 return new Creatures.Rat();

             if (monsterType is Creatures.SkeletalArcher)
                 return new Creatures.SkeletalArcher();

             if (monsterType is Creatures.Skeleton)
                 return new Creatures.Skeleton();

             if (monsterType is Creatures.Spider)
                 return new Creatures.Spider();

             if (monsterType is Creatures.Uruk)
                 return new Creatures.Uruk();

             if (monsterType is Creatures.Whipper)
                 return new Creatures.Whipper();

             if (monsterType is Creatures.Zombie)
                 return new Creatures.Zombie();

            LogFile.Log.LogEntryDebug("Failed to add a creature of type: " + monsterType.SingleDescription, LogDebugLevel.High);
            return null;
         }

        /// <summary>
        /// If we have generated a group-bearing monster, add its group
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="levelToPlace"></param>
        private void CheckSpecialMonsterGroups(Monster monster, int levelToPlace)
        {
            int minDistance = 5;

            //Certain monsters spawn in with groups of their friends
            if (monster is Creatures.GoblinWitchdoctor)
            {
                //Spawn in with a random number of ferrets & goblins
                int noFerrets = 2 + Game.Random.Next(4);
                int noGoblins = 1 + Game.Random.Next(3);

                AddMonstersCloseToMaster(new Creatures.Ferret(), noFerrets, monster, minDistance, levelToPlace);
                AddMonstersCloseToMaster(new Creatures.Goblin(), noGoblins, monster, minDistance, levelToPlace);
            }
            else if (monster is Creatures.OrcShaman)
            {
                //Spawn in with a random number of orcs & spiders
                int noOrcs = 2 + Game.Random.Next(3);
                int noSpiders = 1 + Game.Random.Next(2);

                AddMonstersCloseToMaster(new Creatures.Orc(), noOrcs, monster, minDistance, levelToPlace);
                AddMonstersCloseToMaster(new Creatures.Spider(), noSpiders, monster, minDistance, levelToPlace);
            }

            else if (monster is Creatures.Necromancer)
            {
                //Spawn in with a random number of skels & zombs
                int noSkel = 1 + Game.Random.Next(3);
                int noZomb = 1 + Game.Random.Next(2);

                AddMonstersCloseToMaster(new Creatures.Skeleton(), noSkel, monster, minDistance, levelToPlace);
                AddMonstersCloseToMaster(new Creatures.Zombie(), noZomb, monster, minDistance, levelToPlace);
            }

            else if (monster is Creatures.Meddler)
            {
                //Spawn in with a random number of demons & peons
                int noDem = 1 + Game.Random.Next(2);
                int noPeon = 1 + Game.Random.Next(3);

                AddMonstersCloseToMaster(new Creatures.Demon(), noDem, monster, minDistance, levelToPlace);
                AddMonstersCloseToMaster(new Creatures.Peon(), noPeon, monster, minDistance, levelToPlace);
            }

            else if (monster is Creatures.Maleficarum)
            {
                //Spawn in with a random number of demons & whippers
                int noDem = 1 + Game.Random.Next(2);
                int noWhippers = 1 + Game.Random.Next(2);

                AddMonstersCloseToMaster(new Creatures.Demon(), noDem, monster, minDistance, levelToPlace);
                AddMonstersCloseToMaster(new Creatures.Whipper(), noWhippers, monster, minDistance, levelToPlace);
            }
            else if (monster is Creatures.Overlord)
            {
                //Spawn in with a random number of demons, imps and drainers
                int noDem = 1 + Game.Random.Next(3);
                int noImp = 1;
                int noDrainer = 1;

                AddMonstersCloseToMaster(new Creatures.Demon(), noDem, monster, minDistance, levelToPlace);
                AddMonstersCloseToMaster(new Creatures.Imp(), noImp, monster, minDistance, levelToPlace);
                AddMonstersCloseToMaster(new Creatures.Drainer(), noDrainer, monster, minDistance, levelToPlace);
            }
        }

        private void SpawnUniques()
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
            LogFile.Log.LogEntry("Adding cave uniques...");

            //Level 1: Unique ferret

            Creatures.FerretUnique ferret = new RogueBasin.Creatures.FerretUnique();
            int uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(0) + 1;
            PlaceMonster(uniqueLevel, ferret);

            //ferret.PickUpItem(new RogueBasin.Items.Dagger());

            //Level 2: Unique rat

            Creatures.RatUnique rat = new RogueBasin.Creatures.RatUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(0) + 2;
            PlaceMonster(uniqueLevel, rat);
            
            //rat.PickUpItem(new RogueBasin.Items.Lantern());
 
            //Level 3: Unique goblin witchdoctor

            Creatures.GoblinWitchdoctorUnique gobbo = new RogueBasin.Creatures.GoblinWitchdoctorUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(0) + 3;
            PlaceMonster(uniqueLevel, gobbo);

            //gobbo.PickUpItem(new RogueBasin.Items.Glove());

            //Followers get spawned in SpawnCaveUniqueFollowers() called by SpawnDungeon()
        }

        /// <summary>
        /// Spawn in followers. This is called whenever the dungeon is regened. Don't spawn followers if the uniques are dead.
        /// </summary>
        private void SpawnCaveUniqueFollowers()
        {
            int minDistance = 4;

            Monster ferretUnique = Game.Dungeon.FindMonsterByType(typeof(Creatures.FerretUnique));
            Monster ratUnique = Game.Dungeon.FindMonsterByType(typeof(Creatures.RatUnique));
            Monster goblinUnique = Game.Dungeon.FindMonsterByType(typeof(Creatures.GoblinWitchdoctorUnique));

            //Level 1: Unique ferret

            //Level 2: Unique rat

            if (ratUnique != null)
            {
                //Cohort
                int noRats = 3 + Game.Random.Next(8);

                AddMonstersCloseToMaster(new Creatures.Rat(), noRats, ratUnique, minDistance, ratUnique.LocationLevel);
            }

            //Level 3: Unique goblin witchdoctor

            if (goblinUnique != null)
            {
                //Spawn in with a random number of ferrets & goblins
                int noFerrets = 4 + Game.Random.Next(2);
                int noGoblins = 3 + Game.Random.Next(2);

                AddMonstersCloseToMaster(new Creatures.Ferret(), noFerrets, goblinUnique, minDistance, goblinUnique.LocationLevel);
                AddMonstersCloseToMaster(new Creatures.Goblin(), noGoblins, goblinUnique, minDistance, goblinUnique.LocationLevel);
            }
        }

        private void SpawnWaterCaveUniques()
        {
            LogFile.Log.LogEntry("Adding water cave uniques...");

            //Level 3 : Unique spider
            Creatures.SpiderUnique spider = new RogueBasin.Creatures.SpiderUnique();
            int uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(1) + 2;
            Point uniqLoc = PlaceMonster(uniqueLevel, spider);

           // Items.ShortSword sword = new RogueBasin.Items.ShortSword();
           // spider.PickUpItem(sword);
            
            //Level 4 : Unique ogre

            Creatures.OgreUnique ogre = new RogueBasin.Creatures.OgreUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(1) + 3;
            uniqLoc = PlaceMonster(uniqueLevel, ogre);

            //Items.PrettyDress dress = new RogueBasin.Items.PrettyDress();
            //ogre.PickUpItem(dress);           
        }

        private void SpawnWaterCaveUniqueFollowers()
        {
            int minDistance = 4;

            Monster spiderUnique = Game.Dungeon.FindMonsterByType(typeof(Creatures.SpiderUnique));
            Monster ogreUnique = Game.Dungeon.FindMonsterByType(typeof(Creatures.OgreUnique));

            //Level 3 : Unique spider

            //Spawn in with a random number of spiders & goblins
            int noSpiders = 3 + Game.Random.Next(2);
            int noGobbos = 2 + Game.Random.Next(2);

            AddMonstersCloseToMaster(new Creatures.Spider(), noSpiders, spiderUnique, minDistance, spiderUnique.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Goblin(), noGobbos, spiderUnique, minDistance, spiderUnique.LocationLevel);

            //Level 4 : Unique ogre

            //Spawn in with a random number of ferrets & goblins
            int noPixie = 2 + Game.Random.Next(2);
            int noBugbear = 2 + Game.Random.Next(2);

            AddMonstersCloseToMaster(new Creatures.Pixie(), noPixie, ogreUnique, minDistance, ogreUnique.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Bugbear(), noBugbear, ogreUnique, minDistance, ogreUnique.LocationLevel);
        }

        private void SpawnForestUniques()
        {
            LogFile.Log.LogEntry("Adding forest uniques...");

            //Level 2 : Unique faerie

            Creatures.FaerieUnique faerie = new RogueBasin.Creatures.FaerieUnique();
            int uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(2) + 1;
            Point uniqLoc = PlaceMonster(uniqueLevel, faerie);

            //Items.SparklingEarrings earring = new RogueBasin.Items.SparklingEarrings();
           // faerie.PickUpItem(earring);

            //Level 3 : Unique black unicorn

            Creatures.BlackUnicornUnique unicorn = new RogueBasin.Creatures.BlackUnicornUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(2) + 2;
            uniqLoc = PlaceMonster(uniqueLevel, unicorn);

          //  Items.LeatherArmour leather = new RogueBasin.Items.LeatherArmour();
           // unicorn.PickUpItem(leather);

            //Level 4 : Unique pixie

            Creatures.PixieUnique pixie = new RogueBasin.Creatures.PixieUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(2) + 3;
            uniqLoc = PlaceMonster(uniqueLevel, pixie);

            //Items.StaffPower staff = new RogueBasin.Items.StaffPower();
           // pixie.PickUpItem(staff);  
        }

        private void SpawnForestUniqueFollowers()
        {
            int minDistance = 4;

            Monster faerie = Game.Dungeon.FindMonsterByType(typeof(Creatures.FaerieUnique));
            Monster unicorn = Game.Dungeon.FindMonsterByType(typeof(Creatures.BlackUnicornUnique));
            Monster pixie = Game.Dungeon.FindMonsterByType(typeof(Creatures.PixieUnique));

            //Level 2 : Unique faerie

            int noNymph = 2 + Game.Random.Next(2);
            int noPixie = 2 + Game.Random.Next(2);

            AddMonstersCloseToMaster(new Creatures.Pixie(), noPixie, faerie, minDistance, faerie.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Nymph(), noNymph, faerie, minDistance, faerie.LocationLevel);

            //Level 3 : Unique black unicorn

            //Spawn in with a random number of unicorns and nymphs
            int noUni = 2 + Game.Random.Next(2);
            noNymph = 1 + Game.Random.Next(2);

            AddMonstersCloseToMaster(new Creatures.BlackUnicorn(), noUni, unicorn, minDistance, unicorn.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Nymph(), noNymph, unicorn, minDistance, unicorn.LocationLevel);

            //Level 4 : Unique pixie

            //Spawn in with a random number of ferrets & goblins
            noPixie = 2 + Game.Random.Next(2);
            int noFairie = 2 + Game.Random.Next(2);
            noNymph = 2 + Game.Random.Next(2);

            AddMonstersCloseToMaster(new Creatures.Pixie(), noPixie, pixie, minDistance, pixie.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Faerie(), noFairie, pixie, minDistance, pixie.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Nymph(), noNymph, pixie, minDistance, pixie.LocationLevel);
        }

        private void SpawnDemonUniques()
        {

            //Level 3 : Unique Maleficarum

            LogFile.Log.LogEntry("Adding demon uniques...");

            Creatures.MaleficarumUnique mal = new RogueBasin.Creatures.MaleficarumUnique();
            int uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(5) + 2;
            Point uniqLoc = PlaceMonster(uniqueLevel, mal);

            //Items.RestoreOrb orb = new RogueBasin.Items.RestoreOrb();
           // mal.PickUpItem(orb);

            //Level 4 : Unique Overlord

            Creatures.OverlordUnique overlord = new RogueBasin.Creatures.OverlordUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(5) + 3;
            uniqLoc = PlaceMonster(uniqueLevel, overlord);

           // Items.GodSword sword = new RogueBasin.Items.GodSword();
          //  overlord.PickUpItem(sword);

            
        }

        private void SpawnDemonUniqueFollowers()
        {
            int minDistance = 5;

            Monster mal = Game.Dungeon.FindMonsterByType(typeof(Creatures.MaleficarumUnique));
            Monster overlord = Game.Dungeon.FindMonsterByType(typeof(Creatures.OverlordUnique));

            //Level 3 : Unique Maleficarum

            //Spawn in with a random number of followers
            int noDran = 2 + Game.Random.Next(2);
            int noImp = 2 + Game.Random.Next(2);
            int noDemon = 3 + Game.Random.Next(4);

            AddMonstersCloseToMaster(new Creatures.Drainer(), noDran, mal, minDistance, mal.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Imp(), noImp, mal, minDistance, mal.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Demon(), noDemon, mal, minDistance, mal.LocationLevel);

            //Level 4 : Unique Overlord
            
            //Spawn in with a random number of followers
            noDran = 2 + Game.Random.Next(2);
            noImp = 2 + Game.Random.Next(2);
            noDemon = 3 + Game.Random.Next(4);
            int noMed = 2;

            AddMonstersCloseToMaster(new Creatures.Drainer(), noDran, overlord, minDistance, overlord.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Imp(), noImp, overlord, minDistance, overlord.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Demon(), noDemon, overlord, minDistance, overlord.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Meddler(), noMed, overlord, minDistance, overlord.LocationLevel);
        }

        private void SpawnCryptUniques()
        {
            //Level 4 : Unique Necromancer

            LogFile.Log.LogEntry("Adding crypt uniques...");

            Creatures.NecromancerUnique necro = new RogueBasin.Creatures.NecromancerUnique();
            int uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(4) + 3;
            Point uniqLoc = PlaceMonster(uniqueLevel, necro);

          //  Items.MetalArmour armour = new RogueBasin.Items.MetalArmour();
         //   necro.PickUpItem(armour);

            // Level 3 : Unique Skeleton
            
            Creatures.SkeletonUnique skel = new RogueBasin.Creatures.SkeletonUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(4) + 2;
            uniqLoc = PlaceMonster(uniqueLevel, skel);

        //    Items.ExtendOrb orb = new RogueBasin.Items.ExtendOrb();
        //    skel.PickUpItem(orb);

            // Level 2 : Unique Ghoul

            Creatures.GhoulUnique ghoul = new RogueBasin.Creatures.GhoulUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(4) + 1;
            uniqLoc = PlaceMonster(uniqueLevel, ghoul);

           // Items.LongSword sword = new RogueBasin.Items.LongSword();
          //  ghoul.PickUpItem(sword);
        }

        private void SpawnCryptUniqueFollowers()
        {
            int minDistance = 4;

            Monster necro = Game.Dungeon.FindMonsterByType(typeof(Creatures.NecromancerUnique));
            Monster skel = Game.Dungeon.FindMonsterByType(typeof(Creatures.SkeletonUnique));
            Monster ghoul = Game.Dungeon.FindMonsterByType(typeof(Creatures.GhoulUnique));

            //Level 4 : Unique Necromancer

            //Spawn in with a random number of skels & zombs
            int noSkelArch = 2 + Game.Random.Next(3);
            int noSkel = 2 + Game.Random.Next(2);
            int noZomb = 2 + Game.Random.Next(2);

            AddMonstersCloseToMaster(new Creatures.SkeletalArcher(), noSkelArch, necro, minDistance, necro.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Skeleton(), noSkel, necro, minDistance, necro.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Zombie(), noZomb, necro, minDistance, necro.LocationLevel);

            // Level 3 : Unique Skeleton

            //Spawn in with a random number of skel archers
            noSkelArch = 3 + Game.Random.Next(4);

            AddMonstersCloseToMaster(new Creatures.SkeletalArcher(), noSkelArch, skel, minDistance, skel.LocationLevel);

            // Level 2 : Unique Ghoul

            //Spawn in with a random number of skel archers
            noSkelArch = 4 + Game.Random.Next(5);

            AddMonstersCloseToMaster(new Creatures.SkeletalArcher(), noSkelArch, ghoul, minDistance, ghoul.LocationLevel);
        }

        private void SpawnOrcUniques()
        {
            LogFile.Log.LogEntry("Adding orc uniques...");

            //Level 3 : Unique Uruk

            Creatures.UrukUnique uruk = new RogueBasin.Creatures.UrukUnique();
            int uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(3) + 2;
            Point uniqLoc = PlaceMonster(uniqueLevel, uruk);

            //Add his items
            Items.HealingPotion dag = new RogueBasin.Items.HealingPotion();
          uruk.PickUpItem(dag);

            //Level 4 : Unique Orc Shaman

            Creatures.OrcShamanUnique shaman = new RogueBasin.Creatures.OrcShamanUnique();
            uniqueLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(3) + 3;
            uniqLoc = PlaceMonster(uniqueLevel, shaman);

            //Add his items

         //   Items.KnockoutDress dress = new RogueBasin.Items.KnockoutDress();
         //   shaman.PickUpItem(dress);            
        }

        private void SpawnOrcUniqueFollowers()
        {
            int minDistance = 6;

            Monster uruk = Game.Dungeon.FindMonsterByType(typeof(Creatures.UrukUnique));
            Monster shaman = Game.Dungeon.FindMonsterByType(typeof(Creatures.OrcShamanUnique));

            //Level 3 : Unique Uruk

            //Spawn in with a random number of rats
            int noBugbear = 3 + Game.Random.Next(2);
            int noUruk = 2 + Game.Random.Next(3);
            int noOrcShaman = 1 + Game.Random.Next(1);

            AddMonstersCloseToMaster(new Creatures.Bugbear(), noBugbear, uruk, minDistance, uruk.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Uruk(), noUruk, uruk, minDistance, uruk.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.OrcShaman(), noOrcShaman, uruk, minDistance, uruk.LocationLevel);

            //Level 4 : Unique Orc Shaman

            //Spawn in with a random number of ferrets & goblins
            int noOrc = 6 + Game.Random.Next(6);
            noBugbear = 3 + Game.Random.Next(2);
            noOrcShaman = 2 + Game.Random.Next(2);

            AddMonstersCloseToMaster(new Creatures.Orc(), noOrc, shaman, minDistance, shaman.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.Bugbear(), noBugbear, shaman, minDistance, shaman.LocationLevel);
            AddMonstersCloseToMaster(new Creatures.OrcShaman(), noOrcShaman, shaman, minDistance, shaman.LocationLevel);
        }

        private void SpawnItems()
        {
            //Debug items

            
            LogFile.Log.LogEntry("Generating items...");


            
             
            //Spawn all the collect items

            
            
            Game.Dungeon.AddItem(new Items.HealingPotion(), 0, new Point(Game.Dungeon.Player.LocationMap.x, Game.Dungeon.Player.LocationMap.y + 2));
  

            //Spawn all the potions / berries
             
            Game.Dungeon.AddItem(new Items.Potion(), 0, new Point(Game.Dungeon.Player.LocationMap.x - 2, Game.Dungeon.Player.LocationMap.y + 3));
            Game.Dungeon.AddItem(new Items.PotionDamUp(), 0, new Point(Game.Dungeon.Player.LocationMap.x - 1, Game.Dungeon.Player.LocationMap.y + 3));
            Game.Dungeon.AddItem(new Items.PotionToHitUp(), 0, new Point(Game.Dungeon.Player.LocationMap.x, Game.Dungeon.Player.LocationMap.y + 3));
            Game.Dungeon.AddItem(new Items.PotionSightUp(), 0, new Point(Game.Dungeon.Player.LocationMap.x, Game.Dungeon.Player.LocationMap.y -1));
            Game.Dungeon.AddItem(new Items.PotionSpeedUp(), 0, new Point(Game.Dungeon.Player.LocationMap.x+1, Game.Dungeon.Player.LocationMap.y -1));
            Game.Dungeon.AddItem(new Items.PotionMPRestore(), 0, new Point(Game.Dungeon.Player.LocationMap.x + 2, Game.Dungeon.Player.LocationMap.y - 1));
            
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
                Monster monster = Game.Dungeon.RandomMonsterOnLevel(level);

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
                    location = Game.Dungeon.RandomWalkablePointInLevel(level);

                    //May want to specify a minimum distance from staircases??? TODO
                } while (!Game.Dungeon.AddItem(item, level, location));
            }
        }


        /// <summary>
        /// Adds levels and interconnecting staircases
        /// </summary>
        private void SetupMapsFlatline()
        {
            Dungeon dungeon = Game.Dungeon;

            //Levels

            //Set up the maps here. Light levels are set up in SpawnXXXXCreatures methods. These set the dungeons light and the creature sight. Perhaps set light here - TODO

            //Set up the levels. Needs to be done here so the wilderness is initialized properly.

            //Game.Dungeon.DungeonInfo.SetupDungeonStartAndEndDebug();

            //Make the generators

            MapGeneratorCave caveGen = new MapGeneratorCave();

            //Set width height of all maps to 60 / 25
            caveGen.Width = 60;
            caveGen.Height = 25;

            //These need to start from 0 now and be continuous

            List<int> dungeonLevelsToTest = RebuildAllMaps();


            //Place the player, so monster placing can be checked against it
            //Game.Dungeon.Player.LocationLevel = 0; //on reload, don't reset this
            Game.Dungeon.Player.LocationMap = Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation;

            //Place monsters in levels
            SpawnCreaturesFlatline(dungeonLevelsToTest, levelGen, false);

            SpawnItemsFlatline(dungeonLevelsToTest, levelGen, false);

            SpawnObjectivesFlatline(dungeonLevelsToTest, levelGen, false);
        }
        
        private List<int> RebuildAllMaps()
        {
            List<int> dungeonLevelsToTest = new List<int>();

            int totalLevels = 5; //15
            for (int i = 0; i < totalLevels; i++)
            {
                dungeonLevelsToTest.Add(i);
            }

            bool experimentalTemplateGen = false;

            if(!experimentalTemplateGen)
                SpawnMapFlatline(dungeonLevelsToTest, false);
            else
            {
                //Setup a single test level

                //MapGeneratorTemplated templateGen = new MapGeneratorTemplated();
                //Map templateMap = templateGen.GenerateMap();
                //int levelNo = Game.Dungeon.AddMap(templateMap);

                CalculateWalkableAndTCOD();
            }

            //Write maps to disk
            MapExport exporter = new MapExport();

            int index = 0;
            foreach (Map map in Game.Dungeon.Levels)
            {
                exporter.ExportMapToTextFile(map, "map-" + index + ".txt");
                index++;
            }

            return dungeonLevelsToTest;
        }


        private void SpawnMapFlatline(List<int> dungeonLevelsToSpawn, bool useOldSeed) {

            Dungeon dungeon = Game.Dungeon;

            //Fixed and random levels
            foreach (int level in dungeonLevelsToSpawn)
            {

                switch (level)
                {

                    case 0:
                        {
                            //Make level 0 rather small

                            MapGeneratorBSP hallsGen = new MapGeneratorBSP();

                            hallsGen.Width = 40;
                            hallsGen.Height = 80;

                            Map hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
                            int levelNo = Game.Dungeon.AddMap(hallMap);

                            //Store the hallGen so we can use it for monsters
                            levelGen.Add(level, hallsGen);

                            //Add standard dock triggers
                            AddStandardEntryExitTriggers(dungeon, hallsGen, levelNo);

                            //Add first level entry trigger
                            Game.Dungeon.AddTrigger(levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation, new Triggers.FirstLevelEntry());
                            
                            //Place dock bay feature at PC startloc
                            Game.Dungeon.AddFeature(new Features.DockBay(), levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation);

                        }
                        break;

                    case 1:
                        {
                            //Make level 1 rather small

                            MapGeneratorBSP hallsGen = new MapGeneratorBSP();

                            hallsGen.Width = 40;
                            hallsGen.Height = 25;

                            Map hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
                            int levelNo = Game.Dungeon.AddMap(hallMap);

                            //Store the hallGen so we can use it for monsters
                            levelGen.Add(level, hallsGen);

                            //Add standard dock triggers
                            AddStandardEntryExitTriggers(dungeon, hallsGen, levelNo);

                            //Add level entry trigger
                            Game.Dungeon.AddTrigger(levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation, new Triggers.Mission1Entry());

                            //Place dock bay feature at PC startloc
                            Game.Dungeon.AddFeature(new Features.DockBay(), levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation);

                        }
                        break;

                    case 2:
                        {
                            //Make level 2 rather small

                            MapGeneratorBSP hallsGen = new MapGeneratorBSP();

                            hallsGen.Width = 40;
                            hallsGen.Height = 25;

                            Map hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
                            int levelNo = Game.Dungeon.AddMap(hallMap);

                            //Store the hallGen so we can use it for monsters
                            levelGen.Add(level, hallsGen);

                            //Add standard dock triggers
                            AddStandardEntryExitTriggers(dungeon, hallsGen, levelNo);

                            //Add level entry trigger
                            Game.Dungeon.AddTrigger(levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation, new Triggers.Mission2Entry());

                            //Place dock bay feature at PC startloc
                            Game.Dungeon.AddFeature(new Features.DockBay(), levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation);

                        }
                        break;

                    case 3:
                        {
                            //Make level 3 rather small

                            MapGeneratorBSP hallsGen = new MapGeneratorBSP();

                            hallsGen.Width = 40;
                            hallsGen.Height = 25;

                            Map hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
                            int levelNo = Game.Dungeon.AddMap(hallMap);

                            //Store the hallGen so we can use it for monsters
                            levelGen.Add(level, hallsGen);

                            //Add standard dock triggers
                            AddStandardEntryExitTriggers(dungeon, hallsGen, levelNo);

                            //Add level entry trigger
                            Game.Dungeon.AddTrigger(levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation, new Triggers.Mission3Entry());

                            //Place dock bay feature at PC startloc
                            Game.Dungeon.AddFeature(new Features.DockBay(), levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation);

                        }
                        break;

                    case 4:
                        {
                            //Make level 4 rather small
                            MapGeneratorBSP hallsGen = new MapGeneratorBSP();

                            hallsGen.Width = 40;
                            hallsGen.Height = 25;

                            Map hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
                            int levelNo = Game.Dungeon.AddMap(hallMap);

                            //Store the hallGen so we can use it for monsters
                            levelGen.Add(level, hallsGen);

                            //Add standard dock triggers
                            AddStandardEntryExitTriggers(dungeon, hallsGen, levelNo);

                            //Add level entry trigger
                            Game.Dungeon.AddTrigger(levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation, new Triggers.Mission4Entry());

                            //Place dock bay feature at PC startloc
                            Game.Dungeon.AddFeature(new Features.DockBay(), levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation);

                        }
                        break;
                    case 5:
                        {
                            //Make level 5 rather small

                            MapGeneratorBSP hallsGen = new MapGeneratorBSP();

                            hallsGen.Width = 40;
                            hallsGen.Height = 25;

                            Map hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
                            int levelNo = Game.Dungeon.AddMap(hallMap);

                            //Store the hallGen so we can use it for monsters
                            levelGen.Add(level, hallsGen);

                            //Add standard dock triggers
                            AddStandardEntryExitTriggers(dungeon, hallsGen, levelNo);

                            //Add level entry trigger
                            Game.Dungeon.AddTrigger(levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation, new Triggers.Mission5Entry());

                            //Place dock bay feature at PC startloc
                            Game.Dungeon.AddFeature(new Features.DockBay(), levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation);

                        }
                        break;

                    case 6:
                        {
                            //Make level 6 test level

                            MapGeneratorBSP hallsGen = new MapGeneratorBSP();

                            hallsGen.Width = 40;
                            hallsGen.Height = 25;

                            Map hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
                            int levelNo = Game.Dungeon.AddMap(hallMap);

                            //Store the hallGen so we can use it for monsters
                            levelGen.Add(level, hallsGen);

                            //Add standard dock triggers
                            AddStandardEntryExitTriggers(dungeon, hallsGen, levelNo);

                            //Add level entry trigger
                            Game.Dungeon.AddTrigger(levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation, new Triggers.Mission6Entry());

                            //Place dock bay feature at PC startloc
                            Game.Dungeon.AddFeature(new Features.DockBay(), levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation);

                        }
                        break;

                    case 11:
                        SpawnRandomMap(dungeon, level);
                        //Add level entry trigger
                        Game.Dungeon.AddTrigger(level, Game.Dungeon.Levels[level].PCStartLocation, new Triggers.Mission11Entry());

                        break;

                    case 14:
                        SpawnRandomMap(dungeon, level);
                        //Add level entry trigger
                        Game.Dungeon.AddTrigger(level, Game.Dungeon.Levels[level].PCStartLocation, new Triggers.Mission14Entry());
                        break;

                    default:
                        //Use random dungeon generator
                        {
                            SpawnRandomMap(dungeon, level);
                        }

                        break;
                }

            }


            //Build TCOD maps
            //Necessary so connectivity checks on items and monsters can work
            //Only place where this happens now
            CalculateWalkableAndTCOD();
}

        private void SpawnRandomMap(Dungeon dungeon, int level)
        {
            MapGeneratorBSP hallsGen = new MapGeneratorBSP();

            //Clip to 60
            hallsGen.Width = (int)Math.Min(40 + Game.Random.Next(25), 60);
            hallsGen.Height = 25;

            Map hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
            int levelNo = Game.Dungeon.AddMap(hallMap);

            //Store the hallGen
            //Will get sorted in level order
            levelGen.Add(level, hallsGen);

            //Add standard dock triggers (allows map abortion & completion)
            AddStandardEntryExitTriggers(dungeon, hallsGen, levelNo);

            //Place dock bay feature at PC startloc
            Game.Dungeon.AddFeature(new Features.DockBay(), levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation);

        }

        private void SaveRandomCreatureSeed()
        {
            int seedToUse = Game.Random.Next();
            Game.Random = new Random(seedToUse);
            randomCreatureSeeds.Add(seedToUse);
        }

        private void SaveRandomItemSeed()
        {
            int seedToUse = Game.Random.Next();
            Game.Random = new Random(seedToUse);
            randomItemSeeds.Add(seedToUse);
        }

        private void SaveRandomObjectiveSeed()
        {
            int seedToUse = Game.Random.Next();
            Game.Random = new Random(seedToUse);
            randomObjectiveSeeds.Add(seedToUse);
        }

        private void RestoreRandomCreatureSeed(int level)
        {
            Game.Random = new Random(randomCreatureSeeds[level]);
        }

        private void RestoreRandomItemSeed(int level)
        {
            Game.Random = new Random(randomItemSeeds[level]);
        }

        private void RestoreRandomObjectiveSeed(int level)
        {
            Game.Random = new Random(randomObjectiveSeeds[level]);
        }

        private static void AddStandardEntryExitTriggers(Dungeon dungeon, MapGeneratorBSP hallsGen, int levelNo)
        {
            List<Point> surroundingDock = hallsGen.GetEntryDoor();
            foreach(Point p in surroundingDock)
                Game.Dungeon.AddTrigger(levelNo, p, new Triggers.DockDoor());

            //Add exit trigger
            Game.Dungeon.AddTrigger(levelNo, Game.Dungeon.Levels[levelNo].PCStartLocation, new Triggers.LeaveByDock());
        }


        private void SpawnCreaturesFlatline(List<int> dungeonLevelsToTest, Dictionary<int, MapGenerator> mapGenerators, bool useOldSeed)
        {

            LogFile.Log.LogEntry("Generating creatures...");

            Dungeon dungeon = Game.Dungeon;            

            foreach (int level in dungeonLevelsToTest)
            {

                switch (level)
                {

                    case 0:

                        SpawnCreaturesLevel0(level, mapGenerators[level] as MapGeneratorBSP);
                        break;

                    case 1:

                        SpawnCreaturesLevel1(level, mapGenerators[level] as MapGeneratorBSP);
                        break;

                    case 2:

                        SpawnCreaturesLevel2(level, mapGenerators[level] as MapGeneratorBSP);
                        break;

                    case 3:

                        SpawnCreaturesLevel3(level, mapGenerators[level] as MapGeneratorBSP);
                        break;
                    
                    case 4:

                        SpawnCreaturesLevel4(level, mapGenerators[level] as MapGeneratorBSP);
                        break;

                    case 5:

                        SpawnCreaturesLevel5(level, mapGenerators[level] as MapGeneratorBSP);
                        break;

                   // case 6:

                   //     SpawnCreaturesLevel6(level, mapGenerators[level] as MapGeneratorBSP);
                  //      break;

                    default:
                        {

                            //Use budget-based spawning
                            SpawnCreaturesRandomly(level, mapGenerators[level] as MapGeneratorBSP);
                        }
                        break;
                }

            }
        }

        private void SpawnObjectivesFlatline(List<int> dungeonLevelsToTest, Dictionary<int, MapGenerator> mapGenerators, bool useOldSeed)
        {

            LogFile.Log.LogEntry("Generating objectives...");

            Dungeon dungeon = Game.Dungeon;
            
       

            foreach (int level in dungeonLevelsToTest)
            {

                MapGenerator mapGen = mapGenerators[level];

                int noOfNodes = 3 + Game.Random.Next(3);

                if (level == 14)
                {
                    //Last level, lots of nodes
                    noOfNodes = 8;
                }

                List<Monster> nodes = new List<Monster>();

                //Add a few computer nodes
                for (int i = 0; i < noOfNodes; i++)
                {
                    nodes.Add(new Creatures.ComputerNode());
                }

                AddMonstersEqualDistribution(nodes, level, mapGen);

            }
        }


        private void SpawnCreaturesLevel0(int level, MapGeneratorBSP mapGen)
        {

            //Level 0 just Area Patrol Bots

            for (int i = 0; i < 6; i++)
            {
                Creatures.PatrolBotArea patrolBot = new Creatures.PatrolBotArea();
                AddMonsterSquarePatrol(patrolBot, level, mapGen);
            }

            //This sets light level in the creatures
            SetLightLevelUniversal(level, level, 5);
        }

        private void SpawnCreaturesLevel1(int level, MapGeneratorBSP mapGen)
        {

            //Level 1 just Linear Patrol Bots

            for (int i = 0; i < 6; i++)
            {
                Creatures.PatrolBot patrolBot = new Creatures.PatrolBot();
                AddMonsterLinearPatrol(patrolBot, level, mapGen);
            }

            //This sets light level in the creatures
            SetLightLevelUniversal(level, level, 5);
        }

        private void SpawnCreaturesLevel2(int level, MapGeneratorBSP mapGen)
        {

            //Level 2 just Swarmers (but lots of them)
            List<Monster> monstersToPlace = new List<Monster>();

            for (int i = 0; i < 15; i++)
            {
                Creatures.Swarmer patrolBot = new Creatures.Swarmer(1);
                monstersToPlace.Add(patrolBot);
            }

            AddMonstersEqualDistribution(monstersToPlace, level, mapGen);

            //This sets light level in the creatures
            SetLightLevelUniversal(level, level, 5);
        }

        private void SpawnCreaturesLevel3(int level, MapGeneratorBSP mapGen)
        {

            //Level 2 just Swarmers (but lots of them)
            List<Monster> monstersToPlace = new List<Monster>();

            for (int i = 0; i < 4; i++)
            {
                Creatures.RotatingTurret patrolBot = new Creatures.RotatingTurret();
                monstersToPlace.Add(patrolBot);
            }

           for (int i = 0; i < 4; i++)
            {
                Creatures.PatrolBot patrolBot = new Creatures.PatrolBot();
                AddMonsterLinearPatrol(patrolBot, level, mapGen);
            }

            AddMonstersEqualDistribution(monstersToPlace, level, mapGen);

            //This sets light level in the creatures
            SetLightLevelUniversal(level, level, 5);
        }

        private void SpawnCreaturesLevel4(int level, MapGeneratorBSP mapGen)
        {

            //Level 2 just Swarmers (but lots of them)
            List<Monster> monstersToPlace = new List<Monster>();

            for (int i = 0; i < 15; i++)
            {
                Creatures.Swarmer patrolBot = new Creatures.Swarmer(1);
                monstersToPlace.Add(patrolBot);
            }

            AddMonstersEqualDistribution(monstersToPlace, level, mapGen);

            //This sets light level in the creatures
            SetLightLevelUniversal(level, level, 5);
        }

        private void SpawnCreaturesLevel5(int level, MapGeneratorBSP mapGen)
        {

            //Level 2 just Swarmers (but lots of them)
            List<Monster> monstersToPlace = new List<Monster>();

            for (int i = 0; i < 5; i++)
            {
                Creatures.Swarmer patrolBot = new Creatures.Swarmer(1);
                monstersToPlace.Add(patrolBot);
            }

            for (int i = 0; i < 3; i++)
            {
                Creatures.PatrolBot patrolBot = new Creatures.PatrolBot();
                AddMonsterLinearPatrol(patrolBot, level, mapGen);


            }
            for (int i = 0; i < 3; i++)
            {
                Creatures.RollingBomb patrolBot = new Creatures.RollingBomb();
                monstersToPlace.Add(patrolBot);
            }

            AddMonstersEqualDistribution(monstersToPlace, level, mapGen);

            //This sets light level in the creatures
            SetLightLevelUniversal(level, level, 5);
        }
        
        private void SpawnCreaturesLevel6(int level, MapGeneratorBSP mapGen)
        {

            //Level 2 just Swarmers (but lots of them)
            List<Monster> monstersToPlace = new List<Monster>();

            for (int i = 0; i < 5; i++)
            {
                Creatures.CombatBot patrolBot = new Creatures.CombatBot();
                monstersToPlace.Add(patrolBot);
            }

            for (int i = 0; i < 6; i++)
            {
                Creatures.PatrolBotArea patrolBot = new Creatures.PatrolBotArea();
                AddMonsterSquarePatrol(patrolBot, level, mapGen);
            }

            monstersToPlace.Add(new Creatures.AlertBot());
 
           // AddMonsterLinearPatrol(new Creatures.Juggernaut(), level, mapGen);

            AddMonstersEqualDistribution(monstersToPlace, level, mapGen);

            //This sets light level in the creatures
            SetLightLevelUniversal(level, level, 5);
        }


        private void SpawnItemsFlatline(List<int> dungeonLevelsToTest, Dictionary<int, MapGenerator> mapGenerators, bool useOldSeed)
        {

            

            foreach (int level in dungeonLevelsToTest)
            {

                switch (level)
                {

                    case 0:

                        SpawnItemsLevel0(level, mapGenerators[level] as MapGeneratorBSP);
                        break;
                    case 1:

                        SpawnItemsLevel1(level, mapGenerators[level] as MapGeneratorBSP);
                        break;
                    case 2:
                        SpawnItemsLevel2(level, mapGenerators[level] as MapGeneratorBSP);
                        break;
                    case 3:

                        SpawnItemsLevel3(level, mapGenerators[level] as MapGeneratorBSP);
                        break;
                    case 4:

                        SpawnItemsLevel4(level, mapGenerators[level] as MapGeneratorBSP);
                        break;
                    case 5:
                        SpawnItemsLevel5(level, mapGenerators[level] as MapGeneratorBSP);
                        break;
                    //case 6:

                        //SpawnItemsLevel6(level, mapGenerators[level] as MapGeneratorBSP);
                       // break;
                    default:

                        SpawnItemsRandomly(level, mapGenerators[level] as MapGeneratorBSP);
                        break;



                }
            }
        }

        private void SpawnCreaturesRandomly(int levelIndex, MapGeneratorBSP mapGen)
        {
            //Monster catalogue

            //Type of monster : typical group size

            //Melee monsters

            List<KeyValuePair<Monster, int>> meleeMonsters = new List<KeyValuePair<Monster, int>>();
            meleeMonsters.Add(new KeyValuePair<Monster, int>(new Creatures.Swarmer(1), 6));
            meleeMonsters.Add(new KeyValuePair<Monster, int>(new Creatures.Juggernaut(), 1));

            //Patrolling catalogue

            List<KeyValuePair<Monster, int>> patrolMonsters = new List<KeyValuePair<Monster, int>>();
            patrolMonsters.Add(new KeyValuePair<Monster, int>(new Creatures.PatrolBot(), 3));
            patrolMonsters.Add(new KeyValuePair<Monster, int>(new Creatures.PatrolBotArea(), 3));

            //Rotating catalogue

            List<KeyValuePair<Monster, int>> staticMonsters = new List<KeyValuePair<Monster, int>>();
            staticMonsters.Add(new KeyValuePair<Monster, int>(new Creatures.RotatingTurret(), 2));
            staticMonsters.Add(new KeyValuePair<Monster, int>(new Creatures.CombatBot(), 3));

            //Special monsters
            List<KeyValuePair<Monster, int>> specialMonsters = new List<KeyValuePair<Monster, int>>();
            specialMonsters.Add(new KeyValuePair<Monster, int>(new Creatures.AlertBot(), 2));
            specialMonsters.Add(new KeyValuePair<Monster, int>(new Creatures.RollingBomb(), 2));

            List<List<KeyValuePair<Monster, int>>> monsterCatalogue = new List<List<KeyValuePair<Monster, int>>>();
            monsterCatalogue.Add(meleeMonsters);
            monsterCatalogue.Add(patrolMonsters);
            monsterCatalogue.Add(staticMonsters);
            //Grenades go in last to suck up any left-over budget
            monsterCatalogue.Add(specialMonsters);

            //Calculate the budget we have to spend

            //An 10 cost item (pistol) is good for 30 - 40 points of monster

            //Item budget: 50 + (levelIndex - 5) * 10;

            int creatureBudget = (50 + (levelIndex - 5) * 10) * 3;

            //Divide between different types

            List<double> budgetRatios = new List<double>();

            //Initialize

            budgetRatios.Add(Game.Random.NextDouble());
            budgetRatios.Add(Game.Random.NextDouble());
            budgetRatios.Add(Game.Random.NextDouble());
            budgetRatios.Add(Game.Random.NextDouble());
            
            //Try to spend 2/3 of our budget on one type
            //(+3 was too much at present)

            //exclude special
            budgetRatios[Game.Random.Next(budgetRatios.Count - 1)] += 1.5;

            //Calculate budget split
            double totalRatio = 0.0;
            foreach (double d in budgetRatios)
            {
                totalRatio += d;
            }

            double ratioToTotalBudget = creatureBudget / totalRatio;

            double remainder = 0.0;

            //To hold all the items to place
            List<Monster> monstersToPlaceEvenly = new List<Monster>();
            List<MonsterFightAndRunAI> monstersToPlaceLinearPatrol = new List<MonsterFightAndRunAI>();
            List<MonsterFightAndRunAI> monstersToPlaceSquarePatrol = new List<MonsterFightAndRunAI>();

            //Spend budget on each catalogue, depending upon ratio
            for (int i = 0; i < monsterCatalogue.Count; i++)
            {
                double categoryBudget = budgetRatios[i] * ratioToTotalBudget + remainder;
                remainder = (double)SpendMonsterBudget((int)categoryBudget, monsterCatalogue[i], monstersToPlaceEvenly, monstersToPlaceLinearPatrol, monstersToPlaceSquarePatrol);
            }

            LogFile.Log.LogEntryDebug("SpawnCreaturesRandomly: level: " + levelIndex + " total budget: " + creatureBudget + " remainder: " + remainder, LogDebugLevel.High);

            //Actually add the monsters
            List<RoomCoords> allRooms = mapGen.GetAllRooms();

            AddMonstersEqualDistribution(monstersToPlaceEvenly, levelIndex, mapGen);
            foreach (MonsterFightAndRunAI m in monstersToPlaceLinearPatrol)
                AddMonsterLinearPatrol(m, levelIndex, mapGen);

            foreach (MonsterFightAndRunAI m in monstersToPlaceSquarePatrol)
                AddMonsterSquarePatrol(m, levelIndex, mapGen);

            //This sets light level in the creatures
            SetLightLevelUniversal(levelIndex, levelIndex, 5);
        }

        private double SpendMonsterBudget(int budget, List<KeyValuePair<Monster, int>> monsterCatelogue, List<Monster> monstersToPlace, List<MonsterFightAndRunAI> linearPatrol, List<MonsterFightAndRunAI> squarePatrol)
        {
            int minCostItem = monsterCatelogue[0].Key.CreatureCost();
            int remainingBudget = budget;

            //Since we can buy with 1 quantity, we should clean up the budget easily
            while (remainingBudget >= minCostItem)
            {
                KeyValuePair<Monster, int> monsterToBuy = monsterCatelogue[Game.Random.Next(monsterCatelogue.Count)];
                Monster thisMonster = monsterToBuy.Key;

                //Try to buy full quantity
                int monsterQuantity = (int)Math.Floor(monsterToBuy.Value * 0.5 + Game.Random.NextDouble());

                if (monsterQuantity * thisMonster.CreatureCost() <= remainingBudget)
                {
                    //Buy them
                    for (int i = 0; i < monsterQuantity; i++)
                        PlaceMonsterInAppropriateClass(thisMonster.NewCreatureOfThisType(), monstersToPlace, linearPatrol, squarePatrol);

                    remainingBudget -= monsterQuantity * thisMonster.CreatureCost();
                }
                else
                {
                    int canBuyQuantity = remainingBudget / thisMonster.CreatureCost();

                    if (canBuyQuantity == 0)
                        continue;

                    //Otherwise buy this many
                    for (int i = 0; i < canBuyQuantity; i++)
                        PlaceMonsterInAppropriateClass(thisMonster.NewCreatureOfThisType(), monstersToPlace, linearPatrol, squarePatrol);

                    remainingBudget -= canBuyQuantity * thisMonster.CreatureCost();

                }
            }

            return remainingBudget;


        }

        /// <summary>
        /// Place the monster, based on type in an appropriate bucket for adding
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="monstersToPlace"></param>
        /// <param name="linearPatrol"></param>
        /// <param name="squarePatrol"></param>
        private void PlaceMonsterInAppropriateClass(Monster monster, List<Monster> monstersToPlace, List<MonsterFightAndRunAI> linearPatrol, List<MonsterFightAndRunAI> squarePatrol)
        {
            if (!(monster is MonsterFightAndRunAI))
            {
                monstersToPlace.Add(monster);
                return;
            }

            MonsterFightAndRunAI monsterAI = monster as MonsterFightAndRunAI;

            if (monsterAI.GetPatrolType() == PatrolType.Waypoints && monsterAI.HasSquarePatrol()) {
                squarePatrol.Add(monsterAI);
                return;
            }

            if (monsterAI.GetPatrolType() == PatrolType.Waypoints)
            {
                linearPatrol.Add(monsterAI);
                return;
            }

            monstersToPlace.Add(monster);
            return;
        }


        private void SpawnItemsRandomly(int levelIndex, MapGeneratorBSP mapGen)
        {
            //Item catalogue

            List<Item> meleeRelatedItems = new List<Item>();
            meleeRelatedItems.Add(new Items.Vibroblade());
            meleeRelatedItems.Add(new Items.Pistol());
            meleeRelatedItems.Sort((x,y) => x.ItemCost().CompareTo(y.ItemCost()));
            
            List<Item> rangedRelatedItems = new List<Item>();
            rangedRelatedItems.Add(new Items.Shotgun());
            rangedRelatedItems.Add(new Items.Laser());
            rangedRelatedItems.Add(new Items.Pistol());
            rangedRelatedItems.Sort((x,y) => x.ItemCost().CompareTo(y.ItemCost()));

            List<Item> grenadeRelatedItems = new List<Item>();
            grenadeRelatedItems.Add(new Items.SoundGrenade());
            grenadeRelatedItems.Add(new Items.FragGrenade());
            grenadeRelatedItems.Add(new Items.StunGrenade());
            grenadeRelatedItems.Sort((x,y) => x.ItemCost().CompareTo(y.ItemCost()));

            List<Item> healingRelatedItems = new List<Item>();
            healingRelatedItems.Add(new Items.NanoRepair());
            healingRelatedItems.Add(new Items.TacticalOverlay());
            healingRelatedItems.Add(new Items.StealthCloak());
            healingRelatedItems.Sort((x,y) => x.ItemCost().CompareTo(y.ItemCost()));
            
            List<List<Item>> itemCatalogue = new List<List<Item>>();
            itemCatalogue.Add(meleeRelatedItems);
            itemCatalogue.Add(rangedRelatedItems);
            itemCatalogue.Add(healingRelatedItems);
            //Grenades go in last to suck up any left-over budget
            itemCatalogue.Add(grenadeRelatedItems);

            //Calculate the budget we have to spend
            int itemBudget = 60 + (levelIndex - 5) * 10;

            //Divide between different types

            List<double> budgetRatios = new List<double>();

            double meleeRelatedBudgetRatio = Game.Random.NextDouble();
            budgetRatios.Add(meleeRelatedBudgetRatio);

            double firingRelatedBudgetRatio = 1 - meleeRelatedBudgetRatio;
            budgetRatios.Add(firingRelatedBudgetRatio);

            double grenadeRelatedBudgeRadio = Game.Random.NextDouble();
            budgetRatios.Add(grenadeRelatedBudgeRadio);

            double healingRelatedBudgeRadio = Game.Random.NextDouble();
            budgetRatios.Add(healingRelatedBudgeRadio);

            //Calculate budget split
            double totalRatio = 0.0;
            foreach(double d in budgetRatios) {
                totalRatio += d;
            }

            double ratioToTotalBudget = itemBudget / totalRatio;

            double remainder = 0.0;

            //To hold all the items to place
            List<Item> itemsToPlace = new List<Item>();

            //Spend budget on each catalogue, depending upon ratio
            for (int i = 0; i < itemCatalogue.Count; i++)
            {
                double categoryBudget = budgetRatios[i] * ratioToTotalBudget + remainder;
                remainder = (double)SpendItemBudget((int)categoryBudget, itemCatalogue[i], itemsToPlace);
            }

            LogFile.Log.LogEntryDebug("SpawnItemsRandomly: level: " + levelIndex + " total budget: " + itemBudget + " remainder: " + remainder, LogDebugLevel.High);

            //Add any standard items
                        
            //1 healing kit
            itemsToPlace.Add(new Items.NanoRepair());

            //Actually add the items
            List<RoomCoords> allRooms = mapGen.GetAllRooms();

            AddItemsEqualDistribution(itemsToPlace, levelIndex, mapGen);

            //Pistol at start location
            AddItemAtLocation(new Items.Pistol(), levelIndex, mapGen.GetPlayerStartLocation());

        }


        /// <summary>
        /// Spent the budget randomly (but to completion) on the items in the cateogue. Returns remaining budget
        /// </summary>
        /// <param name="p"></param>
        /// <param name="list"></param>
        /// <param name="itemsToPlace"></param>
        /// <returns></returns>
        private int SpendItemBudget(int budget, List<Item> itemCatalogue, List<Item> itemsToPlace)
        {
            int minCostItem = itemCatalogue[0].ItemCost();
            int remainingBudget = budget;

            while (remainingBudget >= minCostItem)
            {
                Item itemToBuy = itemCatalogue[Game.Random.Next(itemCatalogue.Count)];

                if (itemToBuy.ItemCost() > remainingBudget)
                    continue;

                //Otherwise buy it
                itemsToPlace.Add(itemToBuy.CloneItem());
                remainingBudget -= itemToBuy.ItemCost();
            }

            return remainingBudget;


        }
        
        
        void SpawnItemsLevel0(int levelIndex, MapGeneratorBSP mapGen) {

            List<RoomCoords> allRooms = mapGen.GetAllRooms();

            //Spawn some items
            List<Item> itemsToPlace = new List<Item>();
            itemsToPlace.Add(new Items.Vibroblade());

            AddItemsEqualDistribution(itemsToPlace, levelIndex, mapGen);
        }

        private void SpawnItemsLevel1(int levelIndex, MapGeneratorBSP mapGen)
        {

            List<RoomCoords> allRooms = mapGen.GetAllRooms();

            //Spawn some items
            List<Item> itemsToPlace = new List<Item>();
            itemsToPlace.Add(new Items.Pistol());

            AddItemsEqualDistribution(itemsToPlace, levelIndex, mapGen);

        }

        private void SpawnItemsLevel2(int levelIndex, MapGeneratorBSP mapGen)
        {

            List<RoomCoords> allRooms = mapGen.GetAllRooms();

            //Spawn some items

            List<Item> itemsToPlace = new List<Item>();

            //Tempt the player with the shotgun
            //AddItemCloseToLocation(new Items.Shotgun(), 0, mapGen.GetPlayerStartLocation());
            AddItemAtLocation(new Items.Shotgun(), levelIndex, mapGen.GetPlayerStartLocation());

            //Vibroblade is a better choice
            itemsToPlace.Add(new Items.Vibroblade());
            //Another shotty
            itemsToPlace.Add(new Items.Shotgun());

            AddItemsEqualDistribution(itemsToPlace, levelIndex, mapGen);
        }

        private void SpawnItemsLevel3(int levelIndex, MapGeneratorBSP mapGen)
        {

            List<RoomCoords> allRooms = mapGen.GetAllRooms();

            //Pistol at start location
            AddItemAtLocation(new Items.Pistol(), levelIndex, mapGen.GetPlayerStartLocation());

            //Spawn some items

            List<Item> itemsToPlace = new List<Item>();

            itemsToPlace.Add(new Items.Pistol());
            itemsToPlace.Add(new Items.Pistol());

            AddItemsEqualDistribution(itemsToPlace, levelIndex, mapGen);
        }

        private void SpawnItemsLevel4(int levelIndex, MapGeneratorBSP mapGen)
        {

            List<RoomCoords> allRooms = mapGen.GetAllRooms();

            //Pistol at start location
            AddItemAtLocation(new Items.Pistol(), levelIndex, mapGen.GetPlayerStartLocation());

            //Spawn some items

            List<Item> itemsToPlace = new List<Item>();

            itemsToPlace.Add(new Items.SoundGrenade());
            itemsToPlace.Add(new Items.SoundGrenade());
            itemsToPlace.Add(new Items.SoundGrenade());
            itemsToPlace.Add(new Items.SoundGrenade());
            itemsToPlace.Add(new Items.FragGrenade());
            itemsToPlace.Add(new Items.FragGrenade());

            itemsToPlace.Add(new Items.NanoRepair());

            AddItemsEqualDistribution(itemsToPlace, levelIndex, mapGen);
        }

        private void SpawnItemsLevel5(int levelIndex, MapGeneratorBSP mapGen)
        {

            List<RoomCoords> allRooms = mapGen.GetAllRooms();

            //Pistol at start location
            AddItemAtLocation(new Items.Pistol(), levelIndex, mapGen.GetPlayerStartLocation());

            //Spawn some items

            List<Item> itemsToPlace = new List<Item>();

            itemsToPlace.Add(new Items.Pistol());
            itemsToPlace.Add(new Items.Pistol());

            itemsToPlace.Add(new Items.TacticalOverlay());

            AddItemsEqualDistribution(itemsToPlace, levelIndex, mapGen);
        }

        private void SpawnItemsLevel6(int levelIndex, MapGeneratorBSP mapGen)
        {

            List<RoomCoords> allRooms = mapGen.GetAllRooms();

            //Pistol at start location
            AddItemAtLocation(new Items.Laser(), levelIndex, mapGen.GetPlayerStartLocation());
            AddItemAtLocation(new Items.StealthCloak(), levelIndex, mapGen.GetPlayerStartLocation());

            //Spawn some items

            List<Item> itemsToPlace = new List<Item>();

            itemsToPlace.Add(new Items.TacticalOverlay());
            itemsToPlace.Add(new Items.StealthCloak());


            AddItemsEqualDistribution(itemsToPlace, levelIndex, mapGen);
        }
        
        /*
        /// <summary>
        /// Adds levels and interconnecting staircases
        /// </summary>
        private void SetupMaps()
        {
            Dungeon dungeon = Game.Dungeon;

            //Levels

            //Set up the maps here. Light levels are set up in SpawnXXXXCreatures methods. These set the dungeons light and the creature sight. Perhaps set light here - TODO

            //Set up the levels. Needs to be done here so the wilderness is initialized properly.

            //Game.Dungeon.DungeonInfo.SetupDungeonStartAndEnd();

            //Make the generators

            MapGeneratorCave caveGen = new MapGeneratorCave();
            MapGeneratorBSPCave ruinedGen = new MapGeneratorBSPCave();
            MapGeneratorBSP hallsGen = new MapGeneratorBSP();

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
            Game.Dungeon.Player.LocationLevel = 0;
            Game.Dungeon.Player.LocationMap = asciiTown.GetPCStartLocation();
            Game.Dungeon.AddTrigger(0, asciiTown.GetPCStartLocation(), new Triggers.SchoolEntryTrigger());

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

            caveGen.DoFillInPass = true;
            caveGen.FillInChance = 15;
            caveGen.FillInTerrain = MapTerrain.Rubble;

            //level 2
            //top level has special up staircase leading to wilderness

            caveGen.GenerateMap();

            int levelNo = Game.Dungeon.AddMap(caveGen.Map);
            caveGen.AddDownStaircaseOnly(levelNo);
            caveGen.AddExitStaircaseOnly(levelNo);

            //Add a trigger here
            Game.Dungeon.AddTrigger(levelNo, caveGen.GetPCStartLocation(), new Triggers.DungeonEntranceTrigger());
            
            //level 3-4

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                caveGen.GenerateMap();

                //AddStaircases needs to know the level number
                levelNo = Game.Dungeon.AddMap(caveGen.Map);
                caveGen.AddStaircases(levelNo);
            }

            //level 5

            //Lowest level doens't have a downstaircase
            caveGen.GenerateMap();

            levelNo = Game.Dungeon.AddMap(caveGen.Map);
            caveGen.AddUpStaircaseOnly(levelNo);

            //DUNGEON 2 - levels 6-9

            //Generate and add cave levels
            //Cave with water

            //level 6
            //top level has special up staircase leading to wilderness

            caveGen.DoFillInPass = false;

            caveGen.GenerateMap();

            levelNo = Game.Dungeon.AddMap(caveGen.Map);
            caveGen.AddWaterToCave(15, 4);
            caveGen.AddDownStaircaseOnly(levelNo);
            caveGen.AddExitStaircaseOnly(levelNo);
           
            //Add a trigger here
            Game.Dungeon.AddTrigger(levelNo, caveGen.GetPCStartLocation(), new Triggers.DungeonEntranceTrigger());

            //level 7-8

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                caveGen.GenerateMap();
                caveGen.AddWaterToCave(15, 4);

                //AddStaircases needs to know the level number
                levelNo = Game.Dungeon.AddMap(caveGen.Map);
                caveGen.AddStaircases(levelNo);
            }

            //level 9

            //Lowest level doens't have a downstaircase
            caveGen.GenerateMap();
            caveGen.AddWaterToCave(15, 4);

            levelNo = Game.Dungeon.AddMap(caveGen.Map);
            caveGen.AddUpStaircaseOnly(levelNo);

            //DUNGEON 3 - levels 10-13

            //Forested cave

            //level 10
            //top level has special up staircase leading to wilderness

            caveGen.ResetClosedSquareTerrainType();
            caveGen.ResetOpenSquareTerrainType();
            caveGen.SetClosedSquareTerrainType(MapTerrain.Forest);
            caveGen.SetOpenSquareTerrainType(MapTerrain.Grass);

            caveGen.GenerateMap();

            levelNo = Game.Dungeon.AddMap(caveGen.Map);
            caveGen.AddDownStaircaseOnly(levelNo);
            caveGen.AddExitStaircaseOnly(levelNo);
            
            //Add a trigger here
            Game.Dungeon.AddTrigger(levelNo, caveGen.GetPCStartLocation(), new Triggers.DungeonEntranceTrigger());

            //level 11-12

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                caveGen.GenerateMap();
                caveGen.AddWaterToCave(15, 4);

                //AddStaircases needs to know the level number
                levelNo = Game.Dungeon.AddMap(caveGen.Map);
                caveGen.AddStaircases(levelNo);
            }

            //level 13

            //Lowest level doens't have a downstaircase
            caveGen.GenerateMap();

            levelNo = Game.Dungeon.AddMap(caveGen.Map);
            caveGen.AddUpStaircaseOnly(levelNo);

            //DUNGEON 4 - levels 14-17

            //Old town

            //level 14
            //top level has special up staircase leading to wilderness
            
            Map hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
            levelNo = Game.Dungeon.AddMap(hallMap);

            hallsGen.AddDownStaircaseOnly(levelNo);
            hallsGen.AddExitStaircaseOnly(levelNo);

            //Add a trigger here
            Game.Dungeon.AddTrigger(levelNo, hallsGen.GetUpStaircaseLocation(), new Triggers.DungeonEntranceTrigger());

            //level 15-16

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));
                
                //AddStaircases needs to know the level number
                levelNo = Game.Dungeon.AddMap(hallMap);
                hallsGen.AddStaircases(levelNo);
            }

            //level 17

            //Lowest level doens't have a downstaircase
            hallMap = hallsGen.GenerateMap(hallsExtraCorridorDefinite + Game.Random.Next(hallsExtraCorridorRandom));

            levelNo = Game.Dungeon.AddMap(hallMap);
            hallsGen.AddUpStaircaseOnly(levelNo);

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
            levelNo = Game.Dungeon.AddMap(ruinedLevel);
            ruinedGen.AddDownStaircaseOnly(levelNo);
            ruinedGen.AddExitStaircaseOnly(levelNo);

            //Add a trigger here
            Game.Dungeon.AddTrigger(levelNo, ruinedGen.GetUpStaircaseLocation(), new Triggers.DungeonEntranceTrigger());

            //level 19-20

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                ruinedLevel = ruinedGen.GenerateMap(ruinedExtraCorridorDefinite + Game.Random.Next(ruinedExtraCorridorRandom));
                levelNo = Game.Dungeon.AddMap(ruinedLevel);
                ruinedGen.AddStaircases(levelNo);

            }

            //level 21

            //Lowest level doens't have a downstaircase
            ruinedLevel = ruinedGen.GenerateMap(ruinedExtraCorridorDefinite + Game.Random.Next(ruinedExtraCorridorRandom));
            levelNo = Game.Dungeon.AddMap(ruinedLevel);

            ruinedGen.AddUpStaircaseOnly(levelNo);

            //DUNGEON 6 - levels 22-24

            //Ancient passage

            ruinedGen.ClearRubbleType();
            ruinedGen.AddRubbleType(MapTerrain.Rubble);

            ruinedGen.ClearWallType();
            ruinedGen.AddWallType(MapTerrain.Volcano);
            ruinedGen.AddWallType(MapTerrain.HellWall);
            ruinedGen.RubbleChance = 5;

            ruinedLevel = ruinedGen.GenerateMap(ruinedExtraCorridorDefinite + Game.Random.Next(ruinedExtraCorridorRandom));
            levelNo = Game.Dungeon.AddMap(ruinedLevel);
            ruinedGen.AddDownStaircaseOnly(levelNo);
            ruinedGen.AddExitStaircaseOnly(levelNo);

            //Add a trigger here
            Game.Dungeon.AddTrigger(levelNo, ruinedGen.GetUpStaircaseLocation(), new Triggers.DungeonEntranceTrigger());

            //level 23-24

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                ruinedLevel = ruinedGen.GenerateMap(ruinedExtraCorridorDefinite + Game.Random.Next(ruinedExtraCorridorRandom));
                levelNo = Game.Dungeon.AddMap(ruinedLevel);
                ruinedGen.AddStaircases(levelNo);
            }

            //level 25

            //Lowest level doens't have a downstaircase
            ruinedLevel = ruinedGen.GenerateMap(ruinedExtraCorridorDefinite + Game.Random.Next(ruinedExtraCorridorRandom));
            levelNo = Game.Dungeon.AddMap(ruinedLevel);

            ruinedGen.AddUpStaircaseOnly(levelNo);

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

            levelNo = Game.Dungeon.AddMap(caveGen.Map);
            caveGen.AddDownStaircaseOnly(levelNo);
            caveGen.AddExitStaircaseOnly(levelNo);

            //level 26-27

            for (int i = 0; i < middleLevelsInDungeon; i++)
            {
                caveGen.GenerateMap();
                //caveGen.AddWaterToCave(15, 4);

                //AddStaircases needs to know the level number
                levelNo = Game.Dungeon.AddMap(caveGen.Map);
                caveGen.AddStaircases(levelNo);
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

            //Build TCOD maps
            //Necessary so connectivity checks on items and monsters can work
            //Only place where this happens now
            CalculateWalkableAndTCOD();

        }*/

        private void CalculateWalkableAndTCOD() {

            //TCOD routine uses Walkable flag set above
            Game.Dungeon.RefreshAllLevelPathingAndFOV();
        }
    }
}
