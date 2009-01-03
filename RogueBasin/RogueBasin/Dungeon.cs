using System;
using System.Collections.Generic;
using System.Text;


namespace RogueBasin
{
    //This class keeps track of all the spaces in the dungeon and where everything is
    class Dungeon
    {
        List<Map> levels;

        Point pcLocation;

        int PCLevel = 0;

        public Dungeon()
        {
            levels = new List<Map>();
        }

        public void AddMap(Map mapToAdd)
        {
            levels.Add(mapToAdd);
        }

        public int CurrentLevel
        {
            set
            {
                PCLevel = value;
            }
        }

        //Get current map the PC is on
        public Map PCMap
        {
            get
            {
                return levels[PCLevel];
            }
        }

        public Point PCLocation
        {
            get
            {
                return pcLocation;
            }
            set
            {
                pcLocation = value;
            }
        }


        internal bool PCMove(int x, int y)
        {
            Point newPCLocation = new Point(pcLocation.x + x, pcLocation.y + y);
            
            //OK to move into this space
            if (levels[PCLevel].mapSquares[newPCLocation.x, newPCLocation.y] == Map.MapTerrain.Empty)
            {
                PCLocation = newPCLocation;
                return true;
            }
            else
            {
                //Don't move PC and return false;
                return false;
            }
        }
    }
}
