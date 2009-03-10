using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using System.Xml.Serialization;
using System.IO;
using System.Xml;


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

        List<SpecialMove> specialMoves;

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
            specialMoves = new List<SpecialMove>();

            SetupSpecialMoves();

            player = new Player();
        }

        /// <summary>
        /// Add to the special moves list
        /// </summary>
        private void SetupSpecialMoves()
        {
            //Add here
            specialMoves.Add(new SpecialMoves.WallVault());
            specialMoves.Add(new SpecialMoves.StunBox());
            specialMoves.Add(new SpecialMoves.WallPush());

            foreach (SpecialMove move in specialMoves)
            {
                move.Known = true;
            }
        }

        /// <summary>
        /// Save the game to disk. Throws exceptions
        /// </summary>
        /// <param name="saveGameName"></param>
        public void SaveGame(string saveGameName)
        {
            FileStream stream = null;

            try
            {
                //Copy across the data we need to save from dungeon

                SaveGameInfo saveGameInfo = new SaveGameInfo();

                saveGameInfo.effects = this.effects;
                saveGameInfo.features = this.features;
                saveGameInfo.items = this.items;
                //saveGameInfo.levels = this.levels;
                //saveGameInfo.levelTCODMaps = this.levelTCODMaps; //If this doens't work, we could easily recalculate them
                saveGameInfo.monsters = this.monsters;
                saveGameInfo.player = this.player;
                saveGameInfo.specialMoves = this.specialMoves;
                saveGameInfo.worldClock = this.worldClock;

                //Make maps into serializablemaps and store
                List<SerializableMap> serializedLevels = new List<SerializableMap>();
                foreach (Map level in levels)
                {
                    serializedLevels.Add(new SerializableMap(level));
                }

                saveGameInfo.levels = serializedLevels;

                //Construct save game filename
                string filename = saveGameName + ".sav";

                XmlSerializer serializer = new XmlSerializer(typeof(SaveGameInfo));
                stream = File.Open(filename, FileMode.Create);

                XmlTextWriter writer = new XmlTextWriter(stream, System.Text.Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, saveGameInfo);

                Game.MessageQueue.AddMessage("Game saved successfully");
                LogFile.Log.LogEntry("Game saved successfully: " + filename);
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Save game failed. Name: " + saveGameName + " Reason: " + ex.Message);
                throw new ApplicationException("Save game failed. Name: " + saveGameName + " Reason: " + ex.Message);
            }
            finally
            {
                stream.Close();
            }

        }

        /// <summary>
        /// Add map and return its level index
        /// </summary>
        /// <param name="mapToAdd"></param>
        /// <returns></returns>
        public int AddMap(Map mapToAdd)
        {
            levels.Add(mapToAdd);

            //Add TCOD version
            levelTCODMaps.Add(new TCODFov(mapToAdd.width, mapToAdd.height));

            return levels.Count - 1;
        }

        /// <summary>
        /// Player learns a random move. Play movie.
        /// </summary>
        public void PlayerLearnsRandomMove()
        {
            //OK, this needs to be fixed so you don't keep learning the same moves, but I'm leaving it like this for now for debug

            int moveToLearn = Game.Random.Next(specialMoves.Count);

            specialMoves[moveToLearn].Known = true;

            //Play movie
            Screen.Instance.PlayMovie(specialMoves[moveToLearn].MovieRoot());
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
                    LogFile.Log.LogEntry("AddFeature: map square can't be entered");
                    return false;
                }

                //Check another feature isn't there
                foreach (Feature otherFeature in features)
                {
                    if (otherFeature.LocationLevel == feature.LocationLevel &&
                        otherFeature.LocationMap == feature.LocationMap)
                    {
                        LogFile.Log.LogEntry("AddFeature: other feature already there");
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

        public MapTerrain GetTerrainAtPoint(int level, Point location)
        {
            //Not a level
            if (level < 0 || level > levels.Count)
            {
                string error = "Level " + level + "does not exist";
                LogFile.Log.LogEntry(error);
                throw new ApplicationException(error);
            }

            //Off the map
            if (location.x < 0 || location.x >= levels[level].width ||
                location.y < 0 || location.y >= levels[level].height)
            {
                string error = "Location " + location.x + ":" + location.y + " does not exist on level " + level;
                LogFile.Log.LogEntry(error);
                throw new ApplicationException(error);
            }

            //Otherwise return terrain
            return levels[level].mapSquares[location.x, location.y].Terrain;
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

            //Not walkable
            if (!levels[level].mapSquares[location.x, location.y].Walkable)
            {
                LogFile.Log.LogEntryDebug("MapSquareCanBeEntered failure: Not Walkable", LogDebugLevel.Low);
                return false;
            }

            //A wall - should be caught above
            if (levels[level].mapSquares[location.x, location.y].Terrain == MapTerrain.Wall)
            {
                LogFile.Log.LogEntryDebug("MapSquareCanBeEntered failure: Wall", LogDebugLevel.Low);
                return false;
            }

            //Void (outside of map) - should be caught above
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

        /// <summary>
        /// Move PC to an absolute square. Doesn't do any checking at the mo, should return false if there's a problem
        /// </summary>
        /// <param name="level"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool MovePCAbsolute(int level, int x, int y)
        {
            player.LocationLevel = level;
            player.LocationMap.x = x;
            player.LocationMap.y = y;

            return true;
        }

        /// <summary>
        /// Move a creature to a location
        /// </summary>
        /// <param name="monsterToMove"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        internal bool MoveMonsterAbsolute(Monster monsterToMove, int level, Point location)
        {
            monsterToMove.LocationLevel = level;
            monsterToMove.LocationMap = location;

            //Do anything needed with the AI, not needed right now

            return true;
        }

        /// <summary>
        /// Move PC to another square on the same level. Doesn't do any checking at the mo
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool MovePCAbsoluteSameLevel(int x, int y) {

            player.LocationMap.x = x;
            player.LocationMap.y = y;

            return true;
        }
        /// <summary>
        /// Move PC to another square on the same level. Doesn't do any checking at the mo
        /// </summary>
        internal bool MovePCAbsoluteSameLevel(Point location)
        {
            player.LocationMap.x = location.x;
            player.LocationMap.y = location.y;

            return true;
        }

        /// <summary>
        /// Process a relative PC move, from a keypress
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool PCMove(int x, int y)
        {
            Point newPCLocation = new Point(Player.LocationMap.x + x, Player.LocationMap.y + y);

            //Moves off the map don't work

            if (newPCLocation.x < 0 || newPCLocation.x >= levels[player.LocationLevel].width)
            {
                return false;
            }

            if (newPCLocation.y < 0 || newPCLocation.y >= levels[player.LocationLevel].height)
            {
                return false;
            }
            
            //Check special moves. These take precidence over normal moves. Only if no special move is ready do we do normal resolution here

            foreach (SpecialMove move in specialMoves)
            {
                if (move.Known)
                {
                    move.CheckAction(true, newPCLocation);
                }
            }
            
            //Are any moves ready, if so carry the first one out. All other are deleted (otherwise move interactions have to be worried about)

            SpecialMove moveToDo = null;

            foreach(SpecialMove move in specialMoves) {
                if (move.Known && move.MoveComplete())
                {
                    moveToDo = move;
                    break;
                }
            }

            //Carry out move, if one is ready
            if (moveToDo != null)
            {
                moveToDo.DoMove(newPCLocation);

                //Clear all moves
                foreach (SpecialMove move in specialMoves)
                {
                    move.ClearMove();
                }
                return true;
            }
            
            //No special move this go, do normal moving


            //Moving into void not allowed (but should never happen)
            if (!MapSquareCanBeEntered(player.LocationLevel, newPCLocation))
            {
                //This now costs time since it could be part of a special move
                return true;
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

            if (okToMoveIntoSquare)
            {
                MovePCAbsoluteSameLevel(newPCLocation.x, newPCLocation.y);
            }
             
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
            //Can use RemoveAll now
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

                            //Closed door
                            if (level.mapSquares[j, k].Terrain == MapTerrain.ClosedDoor)
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
            for (int i = 0; i < levels.Count; i++)
            {
                RefreshTCODMap(i);
            }
        }

        /// <summary>
        /// Refresh the TCOD maps used for FOV and pathfinding
        /// Unoptimised at present
        /// </summary>
        internal void RefreshTCODMap(int levelToRefresh)
        {
            //Fail if we have been asked for an invalid level
            if (levelToRefresh < 0 || levelToRefresh > levels.Count)
            {
                LogFile.Log.LogEntry("RefreshTCODMap: Level " + levelToRefresh + " does not exist");
                return;
            }

            Map level = levels[levelToRefresh];
            TCODFov tcodLevel = levelTCODMaps[levelToRefresh];

            for (int j = 0; j < level.width; j++)
            {
                for (int k = 0; k < level.height; k++)
                {
                    tcodLevel.SetCell(j, k, !level.mapSquares[j, k].BlocksLight, level.mapSquares[j, k].Walkable);
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
        /// Return a (the first) feature at this location or null
        /// </summary>
        /// <param name="locationLevel"></param>
        /// <param name="locationMap"></param>
        /// <returns></returns>
        internal Feature FeatureAtSpace(int locationLevel, Point locationMap)
        {
            foreach (Feature feature in features)
            {
                if(feature.IsLocatedAt(locationLevel, locationMap)) {
                    return feature;
                }
            }

            return null;
        }

        /// <summary>
        /// Return an item if there is one at the requested square, or return null if not
        /// </summary>
        /// <param name="locationLevel"></param>
        /// <param name="locationMap"></param>
        /// <returns></returns>
        internal Item ItemAtSpace(int locationLevel, Point locationMap)
        {
            foreach (Item item in items)
            {
                if (item.IsLocatedAt(locationLevel, locationMap) &&
                    !item.InInventory)
                {
                    return item;
                }
            }

            return null;
        }

        internal void RemoveItem(Item itemToUse)
        {
            items.Remove(itemToUse);
        }

        /// <summary>
        /// Open the door at the requested location. Returns true if the door was successfully opened
        /// </summary>
        /// <param name="p"></param>
        /// <param name="doorLocation"></param>
        /// <returns></returns>
        internal bool OpenDoor(int level, Point doorLocation)
        {
            try
            {
                //Check there is a door here                
                MapTerrain doorTerrain = GetTerrainAtPoint(player.LocationLevel, doorLocation);

                if (doorTerrain != MapTerrain.ClosedDoor)
                {
                    return false;
                }

                //Open the door
                levels[level].mapSquares[doorLocation.x, doorLocation.y].Terrain = MapTerrain.OpenDoor;
                levels[level].mapSquares[doorLocation.x, doorLocation.y].SetOpen();

                //This is very inefficient since it resets the whole level. Could just do the door
                //RefreshTCODMap(level);

                //More efficient version
                levelTCODMaps[level].SetCell(doorLocation.x, doorLocation.y, !levels[level].mapSquares[doorLocation.x, doorLocation.y].BlocksLight, levels[level].mapSquares[doorLocation.x, doorLocation.y].Walkable);


                return true;
            }
            catch (ApplicationException)
            {
                //Not a valid location - should not occur
                LogFile.Log.LogEntry("Non-valid location for door requested");
                return false;
            }
        }

        /// <summary>
        /// Equivalent of PCMove for an action that doesn't have a move.
        /// Tell the special moves that this was a non-move action
        /// Theoretically I should also check to see if any of them fire, but I can't imagine why
        /// </summary>
        internal void PCActionNoMove()
        {
            //Check special moves.

            foreach (SpecialMove move in specialMoves)
            {
                if(move.Known)
                    move.CheckAction(false, new Point(0, 0));
            }

            //Are any moves ready, if so carry the first one out. All other are deleted (otherwise move interactions have to be worried about)

            SpecialMove moveToDo = null;

            foreach (SpecialMove move in specialMoves)
            {
                if (move.Known && move.MoveComplete())
                {
                    moveToDo = move;
                    break;
                }
            }

            //Carry out move, if one is ready
            if (moveToDo != null)
            {
                moveToDo.DoMove(new Point(0,0));

                //Clear all moves
                foreach (SpecialMove move in specialMoves)
                {
                    move.ClearMove();
                }
            }
        }
    }
}
