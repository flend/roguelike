using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    public class DungeonMaker
    {
        Dungeon dungeon = null;

        int noCaveLevels = 5;
        int noCaveWaterLevels = 3;

        int noRuinedLevels = 5;
        int ruinedExtraCorridorDefinite = 5;
        int ruinedExtraCorridorRandom = 10;

        int noHallLevels = 5;
        int hallsExtraCorridorDefinite = 0;
        int hallsExtraCorridorRandom = 8;

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

            SpawnItems();

            SpawnInitialCreatures();

            return dungeon;
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

            //Could take into account depth and difficulty level

            //For now just spawn some random creatures in each level

            for (int i = 0; i < dungeon.NoLevels; i++)
            {
                int noCreatures = 5 + Game.Random.Next(10);

                for (int j = 0; j < noCreatures; j++)
                {
                    Monster monster = new Creatures.Rat();
                    Point location = new Point(0, 0);

                    //Find an acceptable location (walkable and with no other creatures in it)
                    //Note that there is no guarantee of connectivity on cave squares
                    do
                    {
                        location = dungeon.RandomWalkablePointInLevel(i);
                    } while (!dungeon.AddMonster(monster, i, location));
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
            
            do
            {
                location = dungeon.RandomWalkablePointInLevel(gloveLevel);
            } while (!dungeon.AddItem(new Items.Glove(), 0, location));

            //The rest of the plot items are split between the remaining cave and ruined levels

            List<Item> plotItems = new List<Item> { new Items.Badge(), new Items.Band(), new Items.Book(), new Items.Boots(), new Items.Bracelet(), new Items.Bracer(), new Items.GlassGem(),
            new Items.Greaves(), new Items.LeadRing() };

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

                //Find position in level and place item
                do
                {
                    location = dungeon.RandomWalkablePointInLevel(level);

                    //May want to specify a minimum distance from staircases??? TODO
                } while (!dungeon.AddItem(plotItem, level, location));
            }

            //Potions

            //Stick 5 potions on level 1 for testing
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

            //1-5: Cave
                //1-3: with water
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
