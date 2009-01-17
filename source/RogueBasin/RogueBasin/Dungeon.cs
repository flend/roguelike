using System;
using System.Collections.Generic;
using System.Text;


namespace RogueBasin
{
    /// <summary>
    /// The contents of a map square: Creatures & Items
    /// </summary>
    public class SquareContents
    {
        /// <summary>
        /// Reference to monster in the square
        /// </summary>
        public Monster monster = null;

        /// <summary>
        /// Reference to player in the square
        /// </summary>
        public Player player = null;

        /// <summary>
        /// Set if no monster or player
        /// </summary>
        public bool empty = false;

        public SquareContents()
        {

        }
    }

    /// <summary>
    /// Keeps or links to all the state in the game
    /// </summary>
    public class Dungeon
    {
        List<Map> levels;
        List<Monster> monsters;
        List<Item> items;

        Player player;

        long worldClock = 0;

        public Dungeon()
        {
            levels = new List<Map>();
            monsters = new List<Monster>();
            items = new List<Item>();

            player = new Player();
        }

        public void AddMap(Map mapToAdd)
        {
            levels.Add(mapToAdd);
        }

        public bool AddMonster(Monster creature, int level, Point location)
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

                monsters.Add(creature);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddCreature: ") + ex.Message);
                return false;
            }

        }

        /// <summary>
        /// Does the square contain a player or creature?
        /// </summary>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public SquareContents MapSquareContents(int level, Point location)
        {
            SquareContents contents = new SquareContents();

            //Check creatures that be blocking
            foreach (Monster creature in monsters)
            {
                if (creature.LocationLevel == level &&
                    creature.LocationMap.x == location.x && creature.LocationMap.y == location.y)
                {
                    contents.monster = creature;
                }
            }

            //Check for PC blocking
            if (player.LocationMap.x == location.x && player.LocationMap.y == location.y)
            {
                contents.player = player;
            }

            if (contents.monster == null && contents.player == null)
                contents.empty = true;

            return contents;
        }

        /// <summary>
        /// Is the requested square moveable into? Only deals with terrain, not creatures or items
        /// </summary>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool MapSquareCanBeEntered(int level, Point location)
        {
            //Off the map
            if (location.x < 0 || location.x >= levels[level].width)
            {
                return false;
            }

            if (location.y < 0 || location.y >= levels[level].height)
            {
                return false;
            }

            //A wall
            if (levels[level].mapSquares[location.x, location.y].Terrain == MapTerrain.Wall)
            {
                return false;
            }

            //Not Walkable
            if (!levels[level].mapSquares[location.x, location.y].Walkable)
            {
                return false;
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
        public List<Monster> Monsters
        {
            get
            {
                return monsters;
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

            //If this is not a valid square, return false
           
            if (!MapSquareCanBeEntered(player.LocationLevel, newPCLocation))
            {
                return false;
            }

            //Check for monsters in the square
            SquareContents contents = MapSquareContents(player.LocationLevel, newPCLocation);
            bool okToMoveIntoSquare = false;

            //If it's empty, it's OK
            if (contents.empty)
            {
                okToMoveIntoSquare = true;
            }
            
            //Monster - attack it
            if (contents.monster != null)
            {
                CombatResults results = player.AttackMonster(contents.monster);
                if (results == CombatResults.DefenderDied)
                {
                    okToMoveIntoSquare = true;
                }
            }
            
            if(okToMoveIntoSquare)
                player.LocationMap = newPCLocation;
            return true;
        }

        //Remove the monster from the list of creatures. It won't get processed or drawn next turn
        internal void KillMonster(Monster monster)
        {
            monsters.Remove(monster);
        }
    }
}
