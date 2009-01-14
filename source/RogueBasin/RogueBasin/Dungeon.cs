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

        Player player;

        long worldClock = 0;

        public Dungeon()
        {
            levels = new List<Map>();
            creatures = new List<Creature>();

            player = new Player();
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
                
                //Check nothing else is there
                if (!MapSquareCanBeEntered(level, location))
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

        private bool MapSquareCanBeEntered(int level, Point location)
        {
            //Not a wall
            if (levels[level].mapSquares[location.x, location.y].Terrain == MapTerrain.Wall)
            {
                return false;
            }

            //Walkable?
            if (!levels[level].mapSquares[location.x, location.y].Walkable)
            {
                return false;
            }

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

        /// <summary>
        /// Increments the world clock. May in future check events
        /// </summary>
        public void IncrementWorldClock()
        {
            worldClock++;
        }

        public int CurrentLevel
        {
            set
            {
                player.LocationLevel = value;
            }
        }

        //Get current map the PC is on
        public Map PCMap
        {
            get
            {
                return levels[player.LocationLevel];
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

        public Player Player {
            get
            {
                return player;
            }
        }

        internal bool PCMove(int x, int y)
        {
            Point newPCLocation = new Point(Player.LocationMap.x + x, Player.LocationMap.y + y);

            if (newPCLocation.x < 0 || newPCLocation.x >= levels[player.LocationLevel].width)
            {
                return false;
            }

            if (newPCLocation.y < 0 || newPCLocation.y >= levels[player.LocationLevel].height)
            {
                return false;
           }

            //OK to move into this space
            if (MapSquareCanBeEntered(player.LocationLevel, newPCLocation))
            {
                player.LocationMap = newPCLocation;
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
