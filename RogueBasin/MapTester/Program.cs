using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueBasin;

namespace MapTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var tester = new MapTester();
            
            //Choose manual test
            tester.TemplatedMapTest();
        }
    }

    class MapTester {

        RogueBase rb;

        public void TemplatedMapTest()
        {
            StandardGameSetup();

            //Setup a single test level

            MapGeneratorTemplated templateGen = new MapGeneratorTemplated();
            Map templateMap = templateGen.GenerateMap();
            int levelNo = Game.Dungeon.AddMap(templateMap);

            Game.Dungeon.Player.LocationMap = Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation;

            Game.Dungeon.RecalculateWalkable();
            Game.Dungeon.RefreshAllLevelPathing();

            RunGame();
        }

        private void StandardGameSetup()
        {
            rb = new RogueBase();
            rb.SetupSystem();

            Game.Dungeon = new Dungeon();

            Game.Dungeon.Player.LocationLevel = 0;
        }

        private void RunGame()
        {
            rb.MainLoop(false);
        }
    }
}
