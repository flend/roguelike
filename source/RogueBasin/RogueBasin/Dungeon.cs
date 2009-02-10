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

        /// <summary>
        /// List of global events
        /// </summary>
        List<DungeonEffect> effects;

        public Dungeon()
        {
            levels = new List<Map>();
            monsters = new List<Monster>();
            items = new List<Item>();
            features = new List<Feature>();
            levelTCODMaps = new List<TCODFov>();
            effects = new List<DungeonEffect>();

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
                    LogFile.Log.LogEntryDebug("AddMonster failure: Square not enterable", LogDebugLevel.Low);
                    return false;
                }

                //Check square has nothing else on it
                SquareContents contents = MapSquareContents(level, location);

                if (contents.monster != null)
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Monster at this square", LogDebugLevel.Low);
                    return false;
                }

                if (contents.player != null)
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Player at this square", LogDebugLevel.Low);
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
                Map featureLevel = levels[level];

                //Check square is accessable
                if (!MapSquareCanBeEntered(level, location))
                {
                    return false;
                }

                //Check another feature isn't there
                foreach (Feature otherFeature in features)
                {
                    if (otherFeature.LocationLevel == feature.LocationLevel &&
                        otherFeature.LocationMap == feature.LocationMap)
                    {
                        return false;
                    }
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

            //Check creature that be blocking
            foreach (Monster creature in monsters)
            {
                if (creature.LocationLevel == level &&
                    creature.LocationMap.x == location.x && creature.LocationMap.y == location.y)
                {
                    contents.monster = creature;
                    break;
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
                LogFile.Log.LogEntryDebug("MapSquareCanBeEntered failure: Wall", LogDebugLevel.Low);
                return false;
            }

            //Void (outside of map)
            if (levels[level].mapSquares[location.x, location.y].Terrain == MapTerrain.Void)
            {
                LogFile.Log.LogEntryDebug("MapSquareCanBeEntered failure: Void", LogDebugLevel.Low);
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

        /// <summary>
        /// Get the number of levels
        /// </summary>
        public int NoLevels
        {
            get
            {
                return levels.Count;
            }
        }

        public List<TCODFov> FOVs
        {
            get
            {
                return levelTCODMaps;
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
            //Don't do this anymore
            /*foreach (Monster monster in monsters)
            {
                levels[monster.LocationLevel].mapSquares[monster.LocationMap.x, monster.LocationMap.y].Walkable = false;
            }*/
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
        /// </summary>
        /// <param name="creature"></param>
        public TCODFov CalculateCreatureFOV(Creature creature)
        {
            Map currentMap = levels[creature.LocationLevel];
            TCODFov tcodFOV = levelTCODMaps[creature.LocationLevel];

            //Update FOV
            tcodFOV.CalculateFOV(creature.LocationMap.x, creature.LocationMap.y, creature.SightRadius);

            return tcodFOV;

        }

        /// <summary>
        /// Displays the creature FOV on the map. Note that this clobbers the FOV map
        /// </summary>
        /// <param name="creature"></param>
        public void ShowCreatureFOVOnMap(Creature creature) {

            //Only do this if the creature is on a visible level
            if(creature.LocationLevel != Player.LocationLevel)
                return;

            Map currentMap = levels[creature.LocationLevel];
            TCODFov tcodFOV = levelTCODMaps[creature.LocationLevel];
           
            //Calculate FOV
            tcodFOV.CalculateFOV(creature.LocationMap.x, creature.LocationMap.y, creature.SightRadius);

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
                    bool inFOV = tcodFOV.CheckTileFOV(i, j);
                    if(inFOV)
                        thisSquare.InMonsterFOV = true;
                }
            }
        }

        /// <summary>
        /// Recalculate the players FOV. Subsequent accesses to the TCODMap of the player's level will have his FOV
        /// Note that the maps may get hijacked by other creatures
        /// </summary>
        internal void CalculatePlayerFOV()
        {
            //Get TCOD to calculate the player's FOV
            Map currentMap = levels[Player.LocationLevel];

            TCODFov tcodFOV = levelTCODMaps[Player.LocationLevel];
            
            tcodFOV.CalculateFOV(Player.LocationMap.x, Player.LocationMap.y, Player.SightRadius);

            //Set the FOV flags on the map
            //Process the whole level, which effectively resets out-of-FOV areas

            for (int i = 0; i < currentMap.width; i++)
            {
                for (int j = 0; j < currentMap.height; j++)
                {
                    MapSquare thisSquare = currentMap.mapSquares[i, j];
                    thisSquare.InPlayerFOV = tcodFOV.CheckTileFOV(i, j);
                    //Set 'has ever been seen flag' if appropriate
                    if (thisSquare.InPlayerFOV == true)
                    {
                        thisSquare.SeenByPlayer = true;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the direction to go in (+-xy) for the next step towards the target
        /// If there's no route at all, return -1, -1. Right now we throw an exception for this, since it shouldn't happen in a connected dungeon
        /// If there's a route but its blocked by a creature return the originCreature's coords
        /// </summary>
        /// <param name="originCreature"></param>
        /// <param name="destCreature"></param>
        /// <returns></returns>
        internal Point GetPathTo(Creature originCreature, Creature destCreature)
        {
            //If on different levels it's an error
            if (originCreature.LocationLevel != destCreature.LocationLevel)
            {
                string msg = originCreature.Representation + " not on the same level as " + destCreature.Representation;
                LogFile.Log.LogEntry(msg);
                throw new ApplicationException(msg);
            }


            //Destination square needs to be walkable for the path finding algorithm. However it isn't walkable at the moment since there is the target creature on it
            //Temporarily make it walkable, keeping transparency the same
            //levelTCODMaps[destCreature.LocationLevel].SetCell(destCreature.LocationMap.x, destCreature.LocationMap.y,
              //  !levels[destCreature.LocationLevel].mapSquares[destCreature.LocationMap.x, destCreature.LocationMap.y].BlocksLight, true);

            

            //Try to walk the path
            //If we fail, check if this square occupied by a creature
            //If so, make that square temporarily unwalkable and try to re-route

            List<Point> blockedSquares = new List<Point>();
            bool goodPath = false;
            bool pathBlockedByCreature = false;
            Point nextStep = new Point(-1, -1);

            do
            {
                //Generate path object
                TCODPathFinding path = new TCODPathFinding(levelTCODMaps[originCreature.LocationLevel], 1.0);
                path.ComputePath(originCreature.LocationMap.x, originCreature.LocationMap.y, destCreature.LocationMap.x, destCreature.LocationMap.y);

                //Find the first step. We need to load x and y with the origin of the path
                int x, y;
                int xOrigin, yOrigin;
               
                path.GetPathOrigin(out x, out y);
                xOrigin = x; yOrigin = y;

                path.WalkPath(ref x, ref y, false);

                //If the x and y of the next step it means the path is blocked

                if (x == xOrigin && y == yOrigin)
                {
                    //If there was no blocking creature then there is no possible route (hopefully impossible in a fully connected dungeon)
                    if (!pathBlockedByCreature)
                    {
                        throw new ApplicationException("Path blocked in connected dungeon!");
                        
                        /*
                        nextStep = new Point(x, y);
                        bool trans;
                        bool walkable;
                        levelTCODMaps[0].GetCell(originCreature.LocationMap.x, originCreature.LocationMap.y, out trans, out walkable);
                        levelTCODMaps[0].GetCell(destCreature.LocationMap.x, destCreature.LocationMap.y, out trans, out walkable);
                        */

                        //Uncomment this if you want to return -1, -1
                        
                        //nextStep = new Point(-1, -1);
                        //goodPath = true;
                        //continue;
                    }
                    else
                    {
                        //Blocking creature but no path
                        nextStep = new Point(x, y);
                        goodPath = true;
                        continue;
                    }
                }


                //Check if that square is occupied
                Creature blockingCreature = null;

                foreach (Monster creature in monsters)
                {
                    if (creature.LocationLevel != originCreature.LocationLevel)
                        continue;

                    //Is it the source creature itself?
                    if (creature == originCreature)
                        continue;

                    //Is it the target creature?
                    if (creature == destCreature)
                        continue;

                    //Another creature is blocking
                    if (creature.LocationMap.x == x && creature.LocationMap.y == y)
                    {
                        blockingCreature = creature;
                    }
                }
                //Do the same for the player (if the creature is chasing another creature around the player)

                if (destCreature != Player)
                {
                    if (Player.LocationLevel == originCreature.LocationLevel &&
                        Player.LocationMap.x == x && Player.LocationMap.y == y)
                    {
                        blockingCreature = Player;
                    }
                }

                //If no blocking creature, the path is good
                if (blockingCreature == null)
                {
                    goodPath = true;
                    nextStep = new Point(x, y);
                    path.Dispose();
                }
                else
                {
                    //Otherwise, there's a blocking creature. Make his square unwalkable temporarily and try to reroute
                    pathBlockedByCreature = true;
                    
                    int blockingLevel = blockingCreature.LocationLevel;
                    int blockingX = blockingCreature.LocationMap.x;
                    int blockingY = blockingCreature.LocationMap.y;
                    
                    levelTCODMaps[blockingLevel].SetCell(blockingX, blockingY, !levels[blockingLevel].mapSquares[blockingX, blockingY].BlocksLight, false);

                    //Add this square to a list of squares to put back
                    blockedSquares.Add(new Point(blockingX, blockingY));

                    //Dispose the old path
                    path.Dispose();

                    //We will try again
                }
            } while (!goodPath);

            //Put back any squares we made unwalkable
            foreach (Point sq in blockedSquares)
            {
                levelTCODMaps[originCreature.LocationLevel].SetCell(sq.x, sq.y, !levels[originCreature.LocationLevel].mapSquares[sq.x, sq.y].BlocksLight, true);
            }

            //path.WalkPath(ref x, ref y, false);

            //path.GetPointOnPath(0, out x, out y); //crashes for some reason

            //Dispose of path (bit wasteful seeming!)
            //path.Dispose();

            //Set the destination square as unwalkable again
            //levelTCODMaps[destCreature.LocationLevel].SetCell(destCreature.LocationMap.x, destCreature.LocationMap.y,
              //  !levels[destCreature.LocationLevel].mapSquares[destCreature.LocationMap.x, destCreature.LocationMap.y].BlocksLight, false);

            //Point nextStep = new Point(x, y);

            return nextStep;
        }

        public long WorldClock
        {
            get
            {
                return worldClock;
            }
        }

        /// <summary>
        /// Increment time on all dungeon (global) events. Events that expire will run their onExit() routines and then delete themselves from the list
        /// </summary>
        internal void IncrementEventTime()
        {
            //Increment time on events and remove finished ones
            List<DungeonEffect> finishedEffects = new List<DungeonEffect>();

            foreach (DungeonEffect effect in effects)
            {
                effect.IncrementTime();

                if (effect.HasEnded())
                {
                    finishedEffects.Add(effect);
                }
            }

            //Remove finished effects
            foreach (DungeonEffect effect in finishedEffects)
            {
                effects.Remove(effect);
            }
        }

        /// <summary>
        /// Player requests at interaction with feature
        /// </summary>
        internal void PCInteractWithFeature()
        {
            //Is there a feature here? Supports > 1 feature / square only by triggering all of them
            //If there is, pass interaction to feature-specific code
            foreach (Feature feature in features)
            {
                if (feature.LocationLevel == player.LocationLevel &&
                    feature.LocationMap == player.LocationMap)
                {
                    feature.PlayerInteraction(player);
                }

            }
        }

        /// <summary>
        /// PC down a down staircase. Is placed at the up staircase on the lower level
        /// Could be placed in StaircaseDown, if we don't mind making Features publicly accessible
        /// </summary>
        /// <returns></returns>
        internal bool PCDownStaircase()
        {
            //If we are trying to go deeper than the dungeon exists
            if (player.LocationLevel + 1 == Game.Dungeon.NoLevels)
            {
                LogFile.Log.LogEntry("Tried to go down stairs to level " + (player.LocationLevel + 1).ToString() + " which doesn't exist");
                Game.MessageQueue.AddMessage("Bizarrely, the stairs don't work.");
                return false;
            }

            //Otherwise move down

            //Increment player level
            player.LocationLevel++;

            Feature foundStaircase = null;
            //Set player's location to up staircase on lower level
            foreach (Feature feature in Features)
            {
                if (feature.LocationLevel == player.LocationLevel
                    && feature as Features.StaircaseUp != null)
                {
                    player.LocationMap = feature.LocationMap;
                    foundStaircase = feature as Features.StaircaseUp;
                    break;
                }
            }

            if (foundStaircase == null)
            {
                LogFile.Log.LogEntry("Couldn't find up staircase on level " + (player.LocationLevel).ToString());
                return true;
            }

            //Check there is no monster on the stairs
            //If there is, kill it (for now)
            foreach (Monster monster in monsters)
            {
                if (monster.InSameSpace(foundStaircase))
                {
                    KillMonster(monster);
                }
            }

            return true;
        }

        /// <summary>
        /// PC down an up staircase. Is placed at the down staircase on the higher level
        /// </summary>
        /// <returns></returns>
        internal bool PCUpStaircase()
        {
            //If we are trying to up from the highest level
            if (player.LocationLevel - 1 < 0)
            {
                LogFile.Log.LogEntry("Player tried to escape");
                Game.MessageQueue.AddMessage("You can't escape that easily!");
                return false;
            }

            //Otherwise move up

            //Increment player level
            player.LocationLevel--;

            Feature foundStaircase = null;

            //Set player's location to up staircase on lower level
            foreach (Feature feature in Features)
            {
                if (feature.LocationLevel == player.LocationLevel
                    && feature as Features.StaircaseDown != null)
                {
                    player.LocationMap = feature.LocationMap;
                    foundStaircase = feature as Features.StaircaseDown;
                    break;
                }
            }

            if (foundStaircase == null)
            {
                LogFile.Log.LogEntry("Couldn't find down staircase on level " + (player.LocationLevel).ToString());
                return true;
            }

            //Check there is no monster on the stairs
            //If there is, kill it (for now)
            foreach (Monster monster in monsters)
            {
                if (monster.InSameSpace(foundStaircase))
                {
                    KillMonster(monster);
                }
            }

            return true;
        }
    }
}
