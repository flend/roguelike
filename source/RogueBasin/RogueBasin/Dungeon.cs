using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;


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
        List<TCODFov> levelTCODMaps;
        List<Monster> monsters;
        List<Item> items;
        List<Feature> features;

        Player player;

        long worldClock = 0;

        public Dungeon()
        {
            levels = new List<Map>();
            monsters = new List<Monster>();
            items = new List<Item>();
            features = new List<Feature>();
            levelTCODMaps = new List<TCODFov>();

            player = new Player();
        }

        public void AddMap(Map mapToAdd)
        {
            levels.Add(mapToAdd);

            //Add TCOD version
            levelTCODMaps.Add(new TCODFov(mapToAdd.width, mapToAdd.height));
        }

        public bool AddMonster(Monster creature, int level, Point location)
        {
            //Try to add a creature at the requested location
            //This may fail due to something else being there or being non-walkable
            try
            {
                Map creatureLevel = levels[level];
                
                //Check square is accessable
                if (!MapSquareCanBeEntered(level, location))
                {
                    return false;
                }

                //Check square has nothing else on it
                SquareContents contents = MapSquareContents(level, location);

                if (contents.monster != null)
                    return false;

                if (contents.player != null)
                    return false;

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
        /// Add an item to the dungeon. May fail if location is invalid or unwalkable
        /// </summary>
        /// <param name="item"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool AddItem(Item item, int level, Point location)
        {
            //Try to add a item at the requested location
            //This may fail due to the square being inaccessable
            try
            {
                Map creatureLevel = levels[level];

                //Check square is accessable
                if (!MapSquareCanBeEntered(level, location))
                {
                    return false;
                }

                //Otherwise OK
                item.LocationLevel = level;
                item.LocationMap = location;

                items.Add(item);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddItem: ") + ex.Message);
                return false;
            }

        }
        /// <summary>
        /// Add feature to the dungeon
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool AddFeature(Feature feature, int level, Point location)
        {
            //Try to add a creature at the requested location
            //This may fail due to something else being there or being non-walkable
            try
            {
                Map creatureLevel = levels[level];

                //Check square is accessable
                if (!MapSquareCanBeEntered(level, location))
                {
                    return false;
                }

                //Otherwise OK
                feature.LocationLevel = level;
                feature.LocationMap = location;

                features.Add(feature);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddItem: ") + ex.Message);
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

            //Void (outside of map)
            if (levels[level].mapSquares[location.x, location.y].Terrain == MapTerrain.Void)
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

        /// <summary>
        /// Get the list of maps
        /// </summary>
        public List<Map> Levels
        {
            get
            {
                return levels;
            }
        }

        public List<TCODFov> LevelFOVs
        {
            get
            {
                return LevelFOVs;
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

        /// <summary>
        /// List of all the items in the game
        /// </summary>
        public List<Item> Items
        {
            get
            {
                return items;
            }
        }

        /// <summary>
        /// List of all the features in the game
        /// </summary>
        public List<Feature> Features
        {
            get
            {
                return features;
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

        /// <summary>
        /// Kill a monster. This monster won't get any further turns.
        /// </summary>
        /// <param name="monster"></param>
        public void KillMonster(Monster monster)
        {
            //We can't take the monster out of the collection directly since we might still be iterating through them
            //Instead set a flag on the monster and remove it after all turns are complete
            monster.Alive = false;
        }

        /// <summary>
        /// Remove all dead creatures from the list so they are not processed again
        /// </summary>
        public void RemoveDeadMonsters()
        {
            List<Monster> deadMonsters = new List<Monster>();

            foreach (Monster monster in monsters)
            {
                if (monster.Alive == false)
                {
                    deadMonsters.Add(monster);
                }
            }

            foreach (Monster monster in deadMonsters)
            {
                monsters.Remove(monster);
            }
        }

        /// <summary>
        /// Check and set the walkable parameter on each map square
        /// At the moment done for all levels
        /// </summary>
        internal void RecalculateWalkable()
        {
            //Terrain

            for (int i = 0; i < levels.Count; i++)
            {
                {
                    Map level = levels[i];

                    for (int j = 0; j < level.width; j++)
                    {
                        for (int k = 0; k < level.height; k++)
                        {

                            //Terrain

                            bool walkable = true;

                            //Walls
                            if (level.mapSquares[j, k].Terrain == MapTerrain.Wall)
                            {
                                walkable = false;
                            }

                            //Void
                            if (level.mapSquares[j, k].Terrain == MapTerrain.Void)
                            {
                                walkable = false;
                            }

                            level.mapSquares[j, k].Walkable = walkable;
                        }
                    }
                }
            }

            //Creatures
            
            //Set each monster's square to non-walkable
            foreach (Monster monster in monsters)
            {
                levels[monster.LocationLevel].mapSquares[monster.LocationMap.x, monster.LocationMap.y].Walkable = false;
            }
        }

        /// <summary>
        /// Find best path between 2 points. No reason really to restrict this to one level only but that would require extending TCOD
        /// </summary>
        /// <param name="level"></param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public bool CalculatePath(int level, Point startPoint, Point endPoint)
        {
            return true;
        }

        /// <summary>
        /// Refresh the TCOD maps used for FOV and pathfinding
        /// Unoptimised at present
        /// </summary>
        internal void RefreshTCODMaps()
        {
            //Set the properties on the TCODMaps from our Maps
            for(int i=0;i<levels.Count;i++) {
                Map level = levels[i];
                TCODFov tcodLevel = levelTCODMaps[i];

                for (int j = 0; j < level.width; j++)
                {
                    for (int k = 0; k < level.height; k++)
                    {
                        tcodLevel.SetCell(j, k, !level.mapSquares[j, k].BlocksLight, level.mapSquares[j, k].Walkable);
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetCreatureFOVOnMap()
        {
            Map level = levels[Player.LocationLevel];

            foreach (MapSquare sq in level.mapSquares)
            {
                sq.InMonsterFOV = false;
            }
        }

        /// <summary>
        /// Calculates the FOV for a creature
        /// Maybe for debug purposes only.
        /// </summary>
        /// <param name="creature"></param>
        public TCODFov CalculateCreatureFOV(Creature creature)
        {
            Map currentMap = levels[creature.LocationLevel];
            TCODFov tcodFOV = levelTCODMaps[creature.LocationLevel];

            //Update FOV
            tcodFOV.CalculateFOV(creature.LocationMap.x, creature.LocationMap.y, creature.SightRadius);

            //Copy this information to the map object for use in drawing

            //Only do this if the creature is on a visible level
            if(creature.LocationLevel != Player.LocationLevel)
                return tcodFOV;

            //Only check sightRadius around the creature

            int xl = creature.LocationMap.x - creature.SightRadius;
            int xr = creature.LocationMap.x + creature.SightRadius;

            int yt = creature.LocationMap.y - creature.SightRadius;
            int yb = creature.LocationMap.y + creature.SightRadius;

            //If sight is infinite, check all the map
            if (creature.SightRadius == 0)
            {
                xl = 0;
                xr = currentMap.width;
                yt = 0;
                yb = currentMap.height;
            }

            if (xl < 0)
                xl = 0;
            if (xr >= currentMap.width)
                xr = currentMap.width - 1;
            if (yt < 0)
                yt = 0;
            if (yb >= currentMap.height)
                yb = currentMap.height - 1;

            for (int i = xl; i <= xr; i++)
            {
                for (int j = yt; j <= yb; j++)
                {
                    MapSquare thisSquare = currentMap.mapSquares[i, j];
                    bool yesOrNo = tcodFOV.CheckTileFOV(i, j);
                    thisSquare.InMonsterFOV = tcodFOV.CheckTileFOV(i, j);
                }
            }

            return tcodFOV;
        }

        /// <summary>
        /// Recalculate the players FOV. Subsequent accesses to the TCODMap of the player's level will have his FOV
        /// Note that the maps may get hijacked by other creatures
        /// </summary>
        internal void CalculatePlayerFOV()
        {
            //Get TCOD to calculate the player's FOV

            levelTCODMaps[Player.LocationLevel].CalculateFOV(Player.LocationMap.x, Player.LocationMap.y, Player.SightRadius);

            //TCODFov tcodFOV = new TCODFov(levels[Player.LocationLevel].width, levels[Player.LocationLevel].height);
            //tcodFOV.ClearMap();

            //tcodFOV.CalculateFOV(Player.LocationMap.x, Player.LocationMap.y, 5);

            //bool yesorno = tcodFOV.CheckTileFOV(1, 1);

            //Copy this information to the map object for use in drawing
            Map level = levels[Player.LocationLevel];
            TCODFov tcodFOV = levelTCODMaps[Player.LocationLevel];

            for (int i = 0; i < level.width; i++)
            {
                for (int j = 0; j < level.height; j++)
                {
                    MapSquare thisSquare = level.mapSquares[i, j];
                    bool yesOrNo = tcodFOV.CheckTileFOV(i, j);
                    thisSquare.InPlayerFOV = tcodFOV.CheckTileFOV(i, j);
                    //Set 'has ever been seen flag' if appropriate
                    if (thisSquare.InPlayerFOV == true)
                    {
                        thisSquare.SeenByPlayer = true;
                    }
                }
            }
        }
    }
}
