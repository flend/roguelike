using System;
using System.Collections.Generic;
using System.Text;


namespace RogueBasin
{
    //This class keeps track of all the state in the game
    public class Dungeon
    {
        List<Map> levels;
        List<Creature> creatures;

        Point pcLocation;

        int pcLevel = 0;

        public Dungeon()
        {
            levels = new List<Map>();
            creatures = new List<Creature>();
        }

        public void AddMap(Map mapToAdd)
        {
            levels.Add(mapToAdd);
        }

        public bool AddCreature(Creature creature, int level, Point location)
        {
            //Try to add a creature at the requested location
            //This may fail due to something else being there or being non-walkable
            try
            {
                Map creatureLevel = levels[level];
                
                //Check walkable
                if (!creatureLevel.mapSquares[location.x, location.y].Walkable)
                {
                    return false;
                }

                //Check nothing else is there
                if (!MapSquareIsEmpty(level, location))
                {
                    return false;
                }

                //Otherwise OK
                creature.LocationLevel = level;
                creature.LocationMap = location;

                creatures.Add(creature);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddCreature: ") + ex.Message);
                return false;
            }

        }

        private bool MapSquareIsEmpty(int level, Point location)
        {
            //Check the terrain
            if (levels[level].mapSquares[location.x, location.y].Terrain == MapTerrain.Wall)
            {
                return false;
            }

            //Walkable?

            //Check creatures that be blocking
            foreach (Creature creature in creatures)
            {
                if (creature.LocationMap.x == location.x && creature.LocationMap.y == location.y)
                {
                    return false;
                }
            }

            //Otherwise OK
            return true;
        }

        public int CurrentLevel
        {
            set
            {
                pcLevel = value;
            }
        }

        //Get current map the PC is on
        public Map PCMap
        {
            get
            {
                return levels[pcLevel];
            }
        }

        public int PCLevel
        {
            get
            {
                return pcLevel;
            }
        }

        //Get the list of creatures
        public List<Creature> Creatures
        {
            get
            {
                return creatures;
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
