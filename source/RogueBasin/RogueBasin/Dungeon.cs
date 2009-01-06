using System;
using System.Collections.Generic;
using System.Text;


namespace RogueBasin
{
    //This class keeps track of all the spaces in the dungeon and where everything is
    public class Dungeon
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

            if (newPCLocation.x < 0 || newPCLocation.x >= levels[PCLevel].width)
            {
                return false;
            }

            if (newPCLocation.y < 0 || newPCLocation.y >= levels[PCLevel].height)
            {
                return false;
            }

            //OK to move into this space
            if (levels[PCLevel].mapSquares[newPCLocation.x, newPCLocation.y].Walkable == true)
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
