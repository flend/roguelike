using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace RogueBasin
{
    /// <summary>
    /// Store the mapping between a hidden name and the actual name of a potion. Much nicer OO ways to do it but I don't have time!
    /// </summary>
    public class HiddenNameInfo
    {
        public string ActualName { get; set; } //SingleItemDescription
        public string HiddenName { get; set; } //random each time
        public string UserName { get; set; } //name the user has given it

        public HiddenNameInfo() { } //for serialization

        public HiddenNameInfo(string actual, string hidden, string user) { ActualName = actual; HiddenName = hidden; UserName = user; }
    }

    public class KillCount
    {
        public Monster type;
        public int count = 0;
    }

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

        public IEnumerable<Feature> features = Enumerable.Empty<Feature>();

        public IEnumerable<Item> items = Enumerable.Empty<Item>();

        /// <summary>
        /// Set if no monster or player
        /// </summary>
        public bool empty = false;

        public bool offMap = false;

        public SquareContents()
        {

        }
    }

    /// <summary>
    /// Keeps or links to all the state in the game
    /// </summary>
    public class Dungeon
    {
        Player player;

        List<Map> levels;

        Pathing pathFinding;
        Algorithms.IFieldOfView fov;

        List<Monster> monsters;
        List<Item> items;
        Dictionary<Location, List<Feature>> features;
        Dictionary<Location, List<Lock>> locks;
        public Dictionary<Location, List<DungeonSquareTrigger>> Triggers { get; set; }

        List<SpecialMove> specialMoves;
        List<Spell> spells;

        public List<HiddenNameInfo> HiddenNameInfo { get; set; } //for serialization

        public bool SaveScumming { get; set; }
        public GameDifficulty Difficulty { get; set; }
        public bool PlayerImmortal { get; set; }
        public bool PlayerCheating { get; set; }

        private List<Monster> summonedMonsters; //no need to serialize

        public bool Profiling { get; set; }

        long worldClock = 0;
        
        /// <summary>
        /// Monster have a unique ID. This stores the next free ID. The player is 0.
        /// </summary>
        public int nextUniqueID = 1;

        /// <summary>
        /// Sounds have a unique ID. This stores the next free ID.
        /// </summary>
        public int nextUniqueSoundID = 0;

        /// <summary>
        /// Set to false to end the game
        /// </summary>
        public bool RunMainLoop { get; set; }

        /// <summary>
        /// Give the player a bonus turn on next loop
        /// </summary>
        bool playerBonusTurn;

        /// <summary>
        /// Set after bonus turn complete
        /// </summary>
        public bool PlayerHadBonusTurn { get; set; }

        /// <summary>
        /// List of global events, indexed by the time they occur
        /// </summary>
        List<SoundEffect> effects;

        System.Drawing.Color defaultPCColor = System.Drawing.Color.White;

        Dictionary<int, bool> DoorStatus = new Dictionary<int, bool>();
        private Combat combat;
        private Movement movement;
        private WeaponUtility weaponUtility;

        public bool AllLocksOpen { get; set; }

        public MapState MapState { get; set; }

        public MonsterPlacement MonsterPlacement { get; private set; }
        
        public String PlayerDeathString { get; set; }
        public bool PlayerDeathOccured { get; set; }

        public bool FunMode { get; set; }

        public Dictionary<int, string> levelNaming;

        public Dungeon()
        {
            SetupDungeon();
        }

        private void SetupDungeon()
        {
            levels = new List<Map>();
            monsters = new List<Monster>();
            items = new List<Item>();
            features = new Dictionary<Location, List<Feature>>();
            locks = new Dictionary<Location, List<Lock>>();

            var rogueSharpPathAndFovWrapper = new LibRogueSharp.RogueSharpPathAndFoVWrapper();
            pathFinding = new Pathing(this, rogueSharpPathAndFovWrapper);
            fov = rogueSharpPathAndFovWrapper;

            ///DungeonEffects are indexed by the time that they occur
            effects = new List<SoundEffect>();

            specialMoves = new List<SpecialMove>();
            spells = new List<Spell>();
            HiddenNameInfo = new List<HiddenNameInfo>();
            Triggers = new Dictionary<Location, List<DungeonSquareTrigger>>();

            //System-type classes
            combat = new Combat(this);
            movement = new Movement(this);
            weaponUtility = new WeaponUtility(this);

            //Should pull this out as an interface, and get TraumaRL to set it
            MonsterPlacement = new MonsterPlacement();

            PlayerImmortal = false;

            playerBonusTurn = false;
            PlayerHadBonusTurn = false;

            SetupSpecialMoves();

            player = new Player();

            RunMainLoop = true;

            summonedMonsters = new List<Monster>();

            SaveScumming = true;

            Profiling = true;
        }

        public Combat Combat { get { return combat; } }
        public Movement Movement { get { return movement; } }
        public WeaponUtility WeaponUtility { get { return weaponUtility; } }

        public ImmutableDictionary<int, string> LevelReadableNames { get { return MapState.LevelGraph.LevelReadableNames; } }

        /// <summary>
        /// Give the player a bonus turn before the monsters
        /// </summary>
        public bool PlayerBonusTurn
        {
            get
            {
                return playerBonusTurn;
            }
            set
            {
                PlayerHadBonusTurn = false;
                playerBonusTurn = true;
            }
        }

        public Algorithms.IFieldOfView FOV
        {
            get
            {
                return fov;
            }
        }

        /// <summary>
        /// Find the closest creature to the map object
        /// </summary>
        /// <param name="originCreature"></param>
        /// <returns></returns>
        public Creature FindClosestCreature(MapObject origin)
        {
            //Find the closest creature
            Creature closestCreature = null;
            double closestDistance = Double.MaxValue; //a long way

            double distance;

            foreach (Monster creature in monsters)
            {
                distance = Utility.GetDistanceBetween(origin, creature);

                if (distance > 0 && distance < closestDistance && origin != creature)
                {
                    closestDistance = distance;
                    closestCreature = creature;
                }
            }

            //And check for player

            distance = Utility.GetDistanceBetween(origin, Game.Dungeon.Player);

            if (distance > 0 && distance < closestDistance && origin != Game.Dungeon.Player)
            {
                closestDistance = distance;
                closestCreature = Game.Dungeon.Player;
            }

            return closestCreature;
        }

        /// <summary>
        /// Find the hostile creature to the map object
        /// </summary>
        /// <param name="originCreature"></param>
        /// <returns></returns>
        public Creature FindClosestHostileCreature(MapObject origin)
        {
            //Find the closest creature
            Creature closestCreature = null;
            double closestDistance = Double.MaxValue; //a long way

            double distance;

            foreach (Monster creature in monsters)
            {
                if (creature.Charmed || creature.Passive)
                    continue;

                distance = Utility.GetDistanceBetween(origin, creature);

                if (distance > 0 && distance < closestDistance && origin != creature)
                {
                    closestDistance = distance;
                    closestCreature = creature;
                }
            }

            return closestCreature;
        }

        /// <summary>
        /// Public access to path finding function
        /// </summary>
        public Pathing Pathing
        {
            get
            {
                return pathFinding;
            }
        }

        public IEnumerable<Tuple<double, Monster>> GetNearbyCreaturesInOrderOfRange(double range, CreatureFOV currentFOV, int level, Point start)
        {
            var rangeRoundedUp = (int)Math.Ceiling(range);

            var candidates = new List<Tuple<double, Monster>>();

            for (int i = start.x - rangeRoundedUp; i < start.x + rangeRoundedUp; i++)
            {
                for (int j = start.y - rangeRoundedUp; j < start.y + rangeRoundedUp; j++)
                {

                    if (i > 0 && j > 0 && i < Game.Dungeon.Levels[level].width && j < Game.Dungeon.Levels[level].height)
                    {
                        foreach (var c in Game.Dungeon.Monsters)
                        {
                            if (c.LocationLevel != level)
                                continue;

                            if (currentFOV.CheckTileFOV(i, j))
                            {
                                candidates.Add(new Tuple<double, Monster>(Utility.GetDistanceBetween(start, c.LocationMap), c));
                            }
                        }
                    }
                }
            }
            return candidates.OrderBy(c => c.Item1);
        }

        public IEnumerable<Monster> FindClosestCreaturesInPlayerFOV()
        {
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);
            var creatures = GetNearbyCreaturesInOrderOfRange(10.0, currentFOV, player.LocationLevel, player.LocationMap).Select(c => c.Item2);
            return creatures.Where(c => currentFOV.CheckTileFOV(c.LocationMap));
        }

        public bool IsMonsterHostile(Monster monster)
        {
            if (monster.Charmed || monster.Passive)
                return false;

            if (IgnoreHostileMonstersOfType(monster))
                return false;

            return true;
        }

        public IEnumerable<Monster> FindAllHostileCreaturesInFoV(CreatureFOV fov)
        {
            return monsters.Where(m => m.LocationLevel == player.LocationLevel && IsMonsterHostile(m) && fov.CheckTileFOV(m.LocationMap)).ToList();
        }

        /// <summary>
        /// Find the hostile creature to the map object
        /// </summary>
        /// <param name="originCreature"></param>
        /// <returns></returns>
        public Creature FindClosestHostileCreatureInFOV(MapObject origin)
        {
            //Find the closest creature
            Creature closestCreature = null;
            double closestDistance = Double.MaxValue; //a long way

            double distance;

            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            foreach (Monster creature in monsters)
            {
                if (!IsMonsterHostile(creature))
                {
                    continue;
                }

                distance = Utility.GetDistanceBetween(origin, creature);

                if (distance > 0 && distance < closestDistance && origin != creature)
                {
                    //Check in FOV

                    if (currentFOV.CheckTileFOV(creature.LocationMap.x, creature.LocationMap.y))
                    {

                        closestDistance = distance;
                        closestCreature = creature;
                    }
                }
            }

            return closestCreature;
        }

        private bool IgnoreHostileMonstersOfType(Monster monsterType)
        {
            if (monsterType is Creatures.Grenade || monsterType is Creatures.Mine)
                return true;

            return false;
        }


        /// <summary>
        /// Link a potion with a user-provided string
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newName"></param>
        public void AssociateNameWithItem(Item item, string newName)
        {
            HiddenNameInfo thisInfo = HiddenNameInfo.Find(x => x.ActualName == item.SingleItemDescription);

            if (thisInfo == null)
            {
                LogFile.Log.LogEntryDebug("Couldn't find an item to associate with this name", LogDebugLevel.High);
                return;
            }
            LogFile.Log.LogEntryDebug("Renaming " + GetHiddenName(item) + " to " + newName, LogDebugLevel.Medium);
            thisInfo.UserName = newName;
        }

        /// <summary>
        /// Get the hidden name of an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal string GetHiddenName(Item item)
        {
            //Not a hidden item
            if (!item.UseHiddenName)
            {
                LogFile.Log.LogEntryDebug("GetHiddenName called on non-hidden item", LogDebugLevel.High);
                return item.SingleItemDescription;
            }

            HiddenNameInfo hiddenName = HiddenNameInfo.Find(x => x.ActualName == item.SingleItemDescription);

            if (hiddenName == null)
            {
                LogFile.Log.LogEntryDebug("Couldn't find hidden name for item", LogDebugLevel.High);
                return item.SingleItemDescription;
            }

            if (hiddenName.UserName != null)
            {
                return hiddenName.UserName;
            }
            else
                return hiddenName.HiddenName;
        }

        /// <summary>
        /// Add to the special moves list
        /// </summary>
        private void SetupSpecialMoves()
        {
            specialMoves.Add(new SpecialMoves.ChargeAttack());
            //specialMoves.Add(new SpecialMoves.StunBox());
            //specialMoves.Add(new SpecialMoves.WallPush());
            specialMoves.Add(new SpecialMoves.WallLeap());
            specialMoves.Add(new SpecialMoves.WallVault());
            specialMoves.Add(new SpecialMoves.VaultBackstab());
            specialMoves.Add(new SpecialMoves.OpenSpaceAttack());
            specialMoves.Add(new SpecialMoves.Evade());
            specialMoves.Add(new SpecialMoves.MultiAttack());
            specialMoves.Add(new SpecialMoves.BurstOfSpeed());
            specialMoves.Add(new SpecialMoves.CloseQuarters());


            foreach (SpecialMove move in specialMoves)
            {
                move.Known = false;
            }
        }

        /// <summary>
        /// Save the game to disk. Throws exceptions
        /// </summary>
        /// <param name="saveGameName"></param>
        public void SaveGame()
        {
            FileStream stream = null;
            GZipStream compStream = null;

            try
            {

                //Copy across the data we need to save from dungeon

                SaveGameInfo saveGameInfo = new SaveGameInfo();

                saveGameInfo.effects = this.effects;
                saveGameInfo.items = this.items;
                saveGameInfo.monsters = this.monsters;
                saveGameInfo.player = this.player;
                saveGameInfo.specialMoves = this.specialMoves;
                saveGameInfo.spells = this.spells;
                saveGameInfo.hiddenNameInfo = this.HiddenNameInfo;
                saveGameInfo.worldClock = this.worldClock;
                saveGameInfo.difficulty = this.Difficulty;
                saveGameInfo.nextUniqueID = this.nextUniqueID;
                saveGameInfo.nextUniqueSoundID = this.nextUniqueSoundID;
                saveGameInfo.messageLog = Game.MessageQueue.GetMessageHistoryAsList();
                saveGameInfo.effects = this.Effects;

                //Make maps into serializablemaps and store
                List<SerializableMap> serializedLevels = new List<SerializableMap>();
                foreach (Map level in levels)
                {
                    serializedLevels.Add(new SerializableMap(level));
                }

                saveGameInfo.levels = serializedLevels;

                //Construct save game filename
                string filename = player.Name + ".sav";

                XmlSerializer serializer = new XmlSerializer(typeof(SaveGameInfo));
                stream = File.Open(filename, FileMode.Create);
                compStream = new GZipStream(stream, CompressionMode.Compress, true);

                //XmlTextWriter writer = new XmlTextWriter(compStream, System.Text.Encoding.UTF8);
                XmlTextWriter writer = new XmlTextWriter(stream, System.Text.Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, saveGameInfo);

                Game.MessageQueue.AddMessage("Game " + player.Name + " saved successfully.");
                LogFile.Log.LogEntry("Game " + player.Name + " saved successfully: " + filename);
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Save game failed. Name: " + player.Name + ".sav" + " Reason: " + ex.Message);
                throw new ApplicationException("Save game failed. Name: " + player.Name + ".sav" + " Reason: " + ex.Message);
            }
            finally
            {
                if (compStream != null)
                {
                    compStream.Close();
                }

                if (stream != null)
                {
                    stream.Close();
                }
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

            //Note - need to calculate FoV after adding all maps
            return levels.Count - 1;
        }

        /// <summary>
        /// Player learns a random move. Play all movies?.
        /// </summary>
        public void PlayerLearnsRandomMove()
        {
            //OK, this needs to be fixed so you don't keep learning the same moves, but I'm leaving it like this for now for debug

            int moveToLearn = Game.Random.Next(specialMoves.Count);

            specialMoves[moveToLearn].Known = true;

            //Play movie
            foreach (SpecialMove m1 in specialMoves)
            {
                Game.Base.SystemActions.PlayMovie(m1.MovieRoot(), false);
            }
        }

        /// <summary>
        /// Player learns all move. Debug. Movies not played.
        /// </summary>
        public void PlayerLearnsAllMoves()
        {
            //Play movie
            foreach (SpecialMove m1 in specialMoves)
            {
                m1.Known = true;
            }
        }

        /// <summary>
        /// Player learns all spells. Debug. Movies not played.
        /// </summary>
        public void PlayerLearnsAllSpells()
        {
            //Play movie
            foreach (Spell m1 in spells)
            {
                m1.Known = true;
            }
        }

        public bool AddMonster(Monster monster, Location loc)
        {
            return AddMonster(monster, loc.Level, loc.MapCoord);
        }

        public bool AddMonster(Monster monster, int level, Point location)
        {
            //Try to add a creature at the requested location
            //This may fail due to something else being there or being non-walkable
            try
            {
                if (monster == null)
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Tried to add null", LogDebugLevel.High);
                    return false;
                }

                if (monster.UniqueID != 0)
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Tried to add monster which already had ID", LogDebugLevel.High);
                    return false;
                }

                Map creatureLevel = levels[level];

                //Check square is accessable
                if (!MapSquareIsWalkable(level, location))
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Square not enterable", LogDebugLevel.Medium);
                    return false;
                }

                //Check square has nothing else on it
                SquareContents contents = MapSquareContents(level, location);

                if (contents.monster != null)
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Monster at this square", LogDebugLevel.Medium);
                    return false;
                }

                //horrible exception
                if (contents.player != null && !(monster is Creatures.Mine))
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Player at this square", LogDebugLevel.Medium);
                    return false;
                }

                if (DangerousFeatureAtLocation(level, location))
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Dangerous terrain at square", LogDebugLevel.Medium);
                    return false;
                }

                //Otherwise OK
                monster.LocationLevel = level;
                monster.LocationMap = location;

                monster.CalculateSightRadius();

                AddMonsterToList(monster);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddCreature: ") + ex.Message);
                return false;
            }

        }

        /// <summary>
        /// Adds a monster to the monsters list. It gets a unique ID. This is used when saving targets
        /// </summary>
        /// <param name="creature"></param>
        private void AddMonsterToList(Monster monster)
        {
            monster.UniqueID = nextUniqueID;
            nextUniqueID++;

            monsters.Add(monster);
        }

        public static double LevelScalingFactor = 0.5;

        public int LevelScalingCalculation(int input, int level) {
            return (int)Math.Ceiling(input * (1 + LevelScalingFactor * (level - 1)));
        }

        /// <summary>
        /// Return a creature (player or monster) reference from a unique ID. Used to check targets are valid after reload
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Creature GetCreatureByUniqueID(int id)
        {
            if (id == 0)
            {
                return player;
            }

            Creature foundCreature = monsters.Find(x => x.UniqueID == id);

            if (foundCreature == null)
            {
                LogFile.Log.LogEntryDebug("Couldn't find monster from ID " + id + " (may have been reaped)", LogDebugLevel.Medium);
            }

            return foundCreature;
        }

        /// <summary>
        /// A creature does something that creates a new creature, e.g. raising summoning.
        /// Adds to the summoning queue which is processed at the end
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool AddMonsterDynamic(Monster creature, int level, Point location, bool allowPlayer = false)
        {
            //Try to add a creature at the requested location
            //This may fail due to something else being there or being non-walkable
            try
            {
                Map creatureLevel = levels[level];

                //Check square is accessable
                if (!MapSquareIsWalkable(level, location))
                {
                    LogFile.Log.LogEntryDebug("AddMonsterDynamic failure: Square not enterable", LogDebugLevel.Medium);
                    return false;
                }

                //Check square has nothing else on it
                SquareContents contents = MapSquareContents(level, location);

                if (contents.monster != null)
                {
                    LogFile.Log.LogEntryDebug("AddMonsterDynamic failure: Monster at this square", LogDebugLevel.Medium);
                    return false;
                }

                if (contents.player != null && !allowPlayer)
                {
                    LogFile.Log.LogEntryDebug("AddMonsterDynamic failure: Player at this square", LogDebugLevel.Medium);
                    return false;
                }

                if (DangerousFeatureAtLocation(level, location))
                {
                    LogFile.Log.LogEntryDebug("AddMonsterDynamic failure: Dangerous terrain at square", LogDebugLevel.Medium);
                    return false;
                }

                //Otherwise OK
                creature.LocationLevel = level;
                creature.LocationMap = location;

                creature.CalculateSightRadius();

                summonedMonsters.Add(creature);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddMonsterDynamic: ") + ex.Message);
                return false;
            }

        }


        public bool AddItem(Item item, Location loc)
        {
            return AddItem(item, loc.Level, loc.MapCoord);
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
                if (!MapSquareIsWalkable(level, location))
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
        /// Add a feature that blocks (sets the square unwalkable)
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool AddFeatureBlocking(Feature feature, int level, Point location, bool blocksLight)
        {
            var addFeatureSuccess = AddFeature(feature, level, location);
            if (!addFeatureSuccess)
                return false;

            feature.IsBlocking = true;
            var willBlockLight = feature.BlocksLight || blocksLight;

            SetTerrainAtPoint(level, location, willBlockLight ? MapTerrain.NonWalkableFeatureLightBlocking : MapTerrain.NonWalkableFeature);

            return true;
        }

        public bool BlockingFeatureAtLocation(int level, Point location)
        {
            var terrain = GetTerrainAtPoint(level, location);
            if (terrain == MapTerrain.NonWalkableFeature || terrain == MapTerrain.NonWalkableFeatureLightBlocking)
                return true;
            return false;
        }

        /// <summary>
        /// Remove feature from the dungeon
        /// </summary>
        public void RemoveFeature(Feature feature)
        {
            if (!features.ContainsKey(feature.Location)) {
                LogFile.Log.LogEntryDebug("RemoveFeature: feature " + feature.Description + " does not exist at: " + feature.Location, LogDebugLevel.High);
                return;
            }

            var existingFeatureList = features[feature.Location];
            if (!existingFeatureList.Any())
            {
                LogFile.Log.LogEntryDebug("RemoveFeature: feature " + feature.Description + " does not exist at: " + feature.Location, LogDebugLevel.High);
                return;
            }

            existingFeatureList.Remove(feature);

            if (feature.IsBlocking)
            {
                //We don't store what terrain was there before, but it's invariably floor now
                SetTerrainAtPoint(feature.LocationLevel, feature.LocationMap, MapTerrain.Empty);
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
            //Try to add a feature at the requested location
            //This may fail due to something else being there or being non-walkable
            try
            {
                Map featureLevel = levels[level];
                var thisLocation = new Location(level, location);

                //Check square is accessable
                if (!MapSquareIsWalkable(level, location))
                {
                    LogFile.Log.LogEntry("AddFeature: map square can't be entered");
                    return false;
                }

                //Check another feature isn't there
                var featuresAtLocation = GetFeaturesAtLocation(thisLocation);
                if (featuresAtLocation.Count() > 0)
                {
                    LogFile.Log.LogEntry("AddFeature: other feature already there: " + featuresAtLocation.ElementAt(0).Description);
                    return false;
                }

                //DON'T PLACE UNDER MONSTERS OR ITEMS - may make the square unroutable

                //Check square has nothing else on it
                SquareContents contents = MapSquareContents(level, location);

                if (contents.monster != null)
                {
                    LogFile.Log.LogEntryDebug("AddFeature failure: Monster at this square", LogDebugLevel.Low);
                    return false;
                }

                if (contents.items.Count() != 0)
                {
                    LogFile.Log.LogEntryDebug("AddFeature failure: Item at this square", LogDebugLevel.Low);
                    return false;
                }

                //Otherwise OK
                feature.LocationLevel = level;
                feature.LocationMap = location;

                AddFeatureAtLocation(thisLocation, feature);

                //Update routing
                if (feature is DangerousActiveFeature)
                {
                    Pathing.PathFindingInternal.updateMapWithDangerousTerrain(level, location, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddFeature: ") + ex.Message);
                return false;
            }

        }

        private void AddFeatureAtLocation(Location loc, Feature feature)
        {
            feature.LocationLevel = loc.Level;
            feature.LocationMap = loc.MapCoord;

            List<Feature> featureListAtLocation;
            features.TryGetValue(loc, out featureListAtLocation);

            if (featureListAtLocation == null)
            {
                features[loc] = new List<Feature>();
                features[loc].Add(feature);
            }
            else
            {
                featureListAtLocation.Add(feature);
            }
        }

        /// <summary>
        /// Returns features at location, or empty list if none
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public IEnumerable<Feature> GetFeaturesAtLocation(Location loc)
        {
            List<Feature> featureListAtLocation;
            features.TryGetValue(loc, out featureListAtLocation);

            if (featureListAtLocation == null)
            {
                return new List<Feature>();
            }

            return featureListAtLocation;
        }

        /// <summary>
        /// Add a lock to the dungeon. Lock must have level & location specified
        /// </summary>
        public bool AddLock(Lock newLock)
        {
            try
            {
                int level = newLock.LocationLevel;
                Point location = newLock.LocationMap;

                SetTerrainAtPoint(level, location, MapTerrain.ClosedLock);

                if (!locks.ContainsKey(newLock.Location))
                    locks[newLock.Location] = new List<Lock>();

                locks[newLock.Location].Add(newLock);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddLock: ") + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Add decoration feature to the dungeon. Make sure we don't cover up useful non-decoration features
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool AddDecorationFeature(Feature feature, int level, Point location)
        {
            //Try to add a feature at the requested location
            try
            {
                Map featureLevel = levels[level];
                Location thisLocation = new Location(level, location);

                //Check square is accessable
                if (!MapSquareIsWalkable(level, location))
                {
                    LogFile.Log.LogEntry("AddDecorationFeature: map square can't be entered");
                    return false;
                }

                //Don't obscure dangerous terrain
                if (DangerousFeatureAtLocation(level, location))
                {
                    LogFile.Log.LogEntry("AddDecorationFeature: dangerous terrain, not adding");
                    return false;
                }

                feature.LocationLevel = level;
                feature.LocationMap = location;

                AddFeatureAtLocation(thisLocation, feature);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddDecorationFeature: ") + ex.Message);
                return false;
            }

        }

        public SquareContents MapSquareContents(Location location)
        {
            return MapSquareContents(location.Level, location.MapCoord);
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

            //Check if we're off the map
            if (!IsValidLocationInWorld(new Location(level, location)))
            {
                contents.offMap = true;
                return contents;
            }

            contents.monster = MonsterAtSpace(level, location);

            //Items
            contents.items = ItemsAtLocation(new Location(level, location));

            //Check features
            contents.features = GetFeaturesAtLocation(new Location(level, location));

            //Check for PC blocking
            //Allow this to work before having the player placed (at beginning of game)
            if (player != null && player.LocationMap != null)
            {
                if (player.LocationLevel == level && player.LocationMap.x == location.x && player.LocationMap.y == location.y)
                {
                    contents.player = player;
                }
            }

            if (contents.monster == null && contents.player == null)
                contents.empty = true;

            return contents;
        }

        public MapTerrain GetTerrainAtLocation(Location location)
        {
            return GetTerrainAtPoint(location.Level, location.MapCoord);
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

        public bool IsValidLocationInWorld(Location location)
        {
            if (location.Level < 0 || location.Level >= levels.Count)
            {
                return false;
            }

            if (location.MapCoord.x < 0 || location.MapCoord.x >= levels[location.Level].width)
            {
                return false;
            }

            if (location.MapCoord.y < 0 || location.MapCoord.y >= levels[location.Level].height)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Is the target either walkable or something we can interact with to make it potentially walkable (e.g. a door)?
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool MapSquareIsWalkableOrInteractable(Location location)
        {
            if (!IsValidLocationInWorld(location))
            {
                return false;
            }

            if (IsOpenableTerrain(location))
            {
                return true;
            }

            return MapSquareIsWalkable(location);
        }

        public bool IsOpenableTerrain(Location location)
        {
            if (!IsValidLocationInWorld(location))
            {
                return false;
            }

            if (GetTerrainAtLocation(location) == MapTerrain.ClosedDoor ||
                GetTerrainAtLocation(location) == MapTerrain.ClosedLock)
            {
                return true;
            }

            return false;
        }

        public bool MapSquareIsWalkable(Location location)
        {
            return MapSquareIsWalkable(location.Level, location.MapCoord);
        }
        /// <summary>
        /// Is the requested square moveable into? Only deals with terrain, not creatures or items
        /// </summary>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool MapSquareIsWalkable(int level, Point location)
        {
            if (level < 0 || level >= levels.Count)
            {
                return false;
            }

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
                //LogFile.Log.LogEntryDebug("MapSquareCanBeEntered failure: Not Walkable", LogDebugLevel.Low);
                return false;
            }

            //These are duplicates that use different code, so should be obsoleted

            //A wall - should be caught above
            if (!MapUtils.IsTerrainWalkable(levels[level].mapSquares[location.x, location.y].Terrain))
            {
                //LogFile.Log.LogEntryDebug("MapSquareCanBeEntered failure: not walkable by terrain type", LogDebugLevel.High);
                return false;
            }

            //Void (outside of map) - should be caught above
            if (levels[level].mapSquares[location.x, location.y].Terrain == MapTerrain.Void)
            {
                //LogFile.Log.LogEntryDebug("MapSquareCanBeEntered failure: Void", LogDebugLevel.High);
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


        /// <summary>
        /// For serialization only
        /// </summary>
        public List<SoundEffect> Effects
        {
            get
            {
                return effects;
            }
            set
            {
                effects = value;
            }
        }

        /// <summary>
        /// For serialization only
        /// </summary>
        public List<SpecialMove> SpecialMoves
        {
            get
            {
                return specialMoves;
            }
            set
            {
                specialMoves = value;
            }
        }

        //Get the list of creatures
        public List<Monster> Monsters
        {
            get
            {
                return monsters;
            }
            //For serialization
            set
            {
                monsters = value;
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
            //For serialization
            set
            {
                items = value;
            }
        }

        /// <summary>
        /// List of all the features in the game - doesn't use index, so avoid use where possible
        /// </summary>
        public List<Feature> Features
        {
            get
            {
                return GetAllFeatures().ToList();
            }
        }

        /// <summary>
        /// List of all the locks in the game
        /// </summary>
        public Dictionary<Location, List<Lock>> Locks
        {
            get
            {
                return locks;
            }
            //For serialization
            set
            {
                locks = value;
            }
        }


        public Player Player
        {
            get
            {
                return player;
            }
            //For serialization
            set
            {
                player = value;
            }
        }

        /// <summary>
        /// Return the instance of the special move class
        /// </summary>
        /// <param name="specialMove"></param>
        /// <returns></returns>
        SpecialMove FindSpecialMove(Type specialMove)
        {
            return specialMoves.Find(x => x.GetType() == specialMove);
        }

        public IEnumerable<UseableFeature> UseableFeaturesAtLocation(Location location)
        {
            var featuresAtSpace = GetFeaturesAtLocation(location);

            return featuresAtSpace.Where(f => f as UseableFeature != null).Select(f => f as UseableFeature);
        }

        public bool InteractWithUseableFeatures(Location location)
        {
            var featuresAtSpace = UseableFeaturesAtLocation(location);

            //Interact with feature - these will normally put success / failure messages in queue
            var successes = featuresAtSpace.Select(f => f.PlayerInteraction(Player)).ToList();

            //Watch out for the short circuit
            return successes.Any();
        }

        public IEnumerable<ActiveFeature> ActiveFeaturesAtLocation(Location location)
        {
            var featuresAtSpace = GetFeaturesAtLocation(location);

            return featuresAtSpace.Where(f => f as ActiveFeature != null).Select(f => f as ActiveFeature);
        }

        /// <summary>
        /// Active features are typically triggered automatically
        /// </summary>
        /// <returns></returns>
        public bool InteractWithActiveFeatures(Location location)
        {
            var useableFeatures = ActiveFeaturesAtLocation(location);

            //Interact with feature - these will normally put success / failure messages in queue
            var successes = useableFeatures.Select(f => f.PlayerInteraction(player)).ToList();

            //Watch out for the short circuit
            return successes.Any();
        }

        public bool MonsterInteractWithActiveFeature(Monster monster, int level, Point mapLocation)
        {
            var useableFeatures = ActiveFeaturesAtLocation(new Location(level, mapLocation));

            //Interact with feature - these will normally put success / failure messages in queue
            var successes = useableFeatures.Select(f => (f as ActiveFeature).MonsterInteraction(monster)).ToList();

            //Watch out for the short circuit
            return successes.Any();
        }

        public void CheckForNewMonstersInFoV()
        {
            var newMonstersInFoV = player.NewMonstersinFoV();

            foreach (var m in newMonstersInFoV)
            {
                player.NotifyMonsterEvent(new MonsterEvent(MonsterEvent.MonsterEventType.MonsterSeenByPlayer, m));
            }
        }


        public List<Point> GetNeighbourPointsToDelta(Point delta)
        {
            if (delta == new Point(0, -1))
            {
                return new List<Point> { new Point(-1, -1), new Point(1, -1) };
            }
            if (delta == new Point(1, -1))
            {
                return new List<Point> { new Point(0, -1), new Point(1, 0) };
            }
            if (delta == new Point(1, 0))
            {
                return new List<Point> { new Point(1, -1), new Point(1, 1) };
            }
            if (delta == new Point(1, 1))
            {
                return new List<Point> { new Point(1, 0), new Point(0, 1) };
            }
            if (delta == new Point(0, 1))
            {
                return new List<Point> { new Point(1, 1), new Point(-1, 1) };
            }
            if (delta == new Point(-1, 1))
            {
                return new List<Point> { new Point(0, 1), new Point(-1, 0) };
            }
            if (delta == new Point(-1, 0))
            {
                return new List<Point> { new Point(-1, 1), new Point(1, 1) };
            }
            else// (delta == new Point(-1, -1))
            {
                return new List<Point> { new Point(-1, 0), new Point(0, 1) };
            }

        }

        private void ResetPCTurnCountersOnActionStatonary(bool attackAction)
        {
            throw new NotImplementedException();
        }


        public void ExplodeAllMonsters()
        {
            List<Monster> livingMonstersOnLevel = monsters.FindAll(x => x.Alive && x.LocationLevel == player.LocationLevel);

            foreach (Monster m in livingMonstersOnLevel)
            {
                List<Point> grenadeAffects = Game.Dungeon.GetPointsForGrenadeTemplate(m.LocationMap, Game.Dungeon.Player.LocationLevel, 4 + Game.Random.Next(3));

                System.Drawing.Color randColor = System.Drawing.Color.Red;
                int randInt = Game.Random.Next(5);

                switch (randInt)
                {
                    case 0:
                        randColor = System.Drawing.Color.Red;
                        break;
                    case 1:
                        randColor = System.Drawing.Color.Orange;
                        break;
                    case 2:
                        randColor = System.Drawing.Color.Yellow;
                        break;
                    case 3:
                        randColor = System.Drawing.Color.OrangeRed;
                        break;
                    case 4:
                        randColor = System.Drawing.Color.DarkRed;
                        break;
                }

                KillMonster(m, false);

                Screen.Instance.DrawAreaAttackAnimation(grenadeAffects, Screen.AttackType.Explosion);
            }
        }


        /// <summary>
        /// For debug :) purposes
        /// </summary>
        /// <param name="level"></param>
        public void KillAllMonstersOnLevel(int level)
        {
            foreach (var monster in Monsters)
            {
                if (monster.LocationLevel == level)
                {
                    KillMonster(monster, true);
                }
            }
        }

        /// <summary>
        /// Kill a monster. This monster won't get any further turns.
        /// If autokill is set to true, this is a dungeon respawn or similar. Don't count towards achievements
        /// </summary>
        /// <param name="monster"></param>
        public void KillMonster(Monster monster, bool autoKill)
        {
            //We can't take the monster out of the collection directly since we might still be iterating through them
            //Instead set a flag on the monster and remove it after all turns are complete
            monster.Alive = false;

            //Remove all existing effects
            monster.RemoveAllEffects();

            //Notify the monster that it has been killed
            monster.NotifyMonsterDeath();

            //Drop its inventory (including plot items we gave it)
            monster.DropAllItems(monster.LocationLevel);

            //Drop any insta-create treasure
            //Not used at present
            if (!autoKill)
                monster.InventoryDrop();

            //If the creature was charmed, delete 1 charmed creature from the player total
            if (monster.Charmed)
                Game.Dungeon.Player.RemoveCharmedCreature();

            if (!autoKill)
            {
                //Leave a corpse
                AddMonsterCorpse(monster);

                //Add experience
                AddXPForMonster(monster);

                monster.OnKilledSpecialEffects();

                SoundPlayer.Instance().EnqueueSound("death");
            }
        }

        private void AddMonsterCorpse(Monster monster)
        {
            var corpseToAdd = monster.GenerateCorpse();

            if (corpseToAdd != null)
            {
                AddDecorationFeature(corpseToAdd, monster.LocationLevel, monster.LocationMap);
            }
        }


        private void AddXPForMonster(Monster monster)
        {

            var baseXP = monster.GetCombatXP();
            int modifiedXP = baseXP;
            if (player.Level < monster.Level)
            {
                modifiedXP = (int)Math.Floor((monster.Level - player.Level) * 0.5 * baseXP);
            }
            else if (player.Level > monster.Level)
            {
                modifiedXP = (int)Math.Ceiling(Math.Pow(2.0, (monster.Level - player.Level)) * baseXP);
            }

            player.CombatXP += modifiedXP;

            LogFile.Log.LogEntryDebug("Awarding XP: " + modifiedXP + " (from base: " + baseXP + ")", LogDebugLevel.Medium);
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

        public bool PickUpAllItemsInSpace(Location location)
        {
            var itemToPickUp = ItemsAtLocation(location).ToList();

            if (!itemToPickUp.Any())
                return false;

            foreach (var item in itemToPickUp)
            {
                Game.MessageQueue.AddMessage(item.SingleItemDescription + " picked up.");

                item.OnPickup(player);

                if (item.DestroyedOnPickup())
                {
                    Game.Dungeon.RemoveItemFromDungeon(item);
                }
                else
                {
                    player.PickUpItem(item);
                }

            }
            return true;
        }

        /// <summary>
        /// This is used in two scenarios - actually destroying an item (e.g. a use-once)
        /// Moving an item from dungeon tracking into an inventory where it is tracked on the creature
        /// (yeah, that's kinda yuck)
        /// </summary>
        /// <param name="item"></param>
        public bool RemoveItemFromDungeon(Item item)
        {
            if (Game.Dungeon.Items.Contains(item))
            {
                Game.Dungeon.RemoveItem(item);
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Set all items on the floor in this level to visible
        /// </summary>
        /// <param name="level"></param>
        public void RevealItemsOnLevel(int level)
        {
            //Check level
            if (level < 0 || level > levels.Count)
            {
                LogFile.Log.LogEntryDebug("RevealItemsOnLevel: Asked for non-existant level: " + level, LogDebugLevel.High);
                return;
            }

            //Find all items on level on floor

            List<Item> itemsOnLevel = items.FindAll(x => x.LocationLevel == level);
            List<Item> itemsOnFloor = itemsOnLevel.FindAll(x => !x.InInventory);

            Map map = levels[level];

            foreach (Item item in itemsOnFloor)
            {
                map.mapSquares[item.LocationMap.x, item.LocationMap.y].SeenByPlayerThisRun = true;
            }
        }

        /// <summary>
        /// Refresh the all TCOD maps used for FOV and pathfinding
        /// </summary>
        public void RefreshAllLevelPathingAndFOV()
        {
            for (int i = 0; i < levels.Count; i++)
            {
                RefreshLevelPathingAndFOV(i);
            }
        }

        /// <summary>
        /// Refresh the TCOD maps used for FOV and pathfinding
        /// Unoptimised at present
        /// </summary>
        public void RefreshLevelPathingAndFOV(int levelNo)
        {
            //Set the walkable flag based on the terrain
            levels[levelNo].RecalculateWalkable();

            //Set the light blocking flag based on the terrain
            levels[levelNo].RecalculateLightBlocking();

            //Update pathfinding library
            pathFinding.PathFindingInternal.updateMap(levelNo, levels[levelNo].PathRepresentation);

            //Update FoV library
            fov.updateFovMap(levelNo, levels[levelNo].FovRepresentaton);
        }

        public void ResetCreatureFOVOnMap()
        {
            Map level = levels[Player.LocationLevel];

            foreach (MapSquare sq in level.mapSquares)
            {
                sq.InMonsterFOV = false;
                sq.InMonsterStealthRadius = false;
            }
        }

        public void ResetSoundOnMap()
        {
            Map level = levels[Player.LocationLevel];

            foreach (MapSquare sq in level.mapSquares)
            {
                sq.SoundMag = 0;
            }
        }

        /// <summary>
        /// Calculates the FOV for a creature
        /// </summary>
        /// <param name="creature"></param>
        public CreatureFOV CalculateCreatureFOV(Creature creature)
        {
            //Update FOV
            fov.CalculateFOV(creature.LocationLevel, creature.LocationMap, creature.SightRadius);

            //Wrapper with game-specific FOV layer
            CreatureFOV wrappedFOV = new CreatureFOV(creature, new WrappedFOV(fov), creature.FOVType());

            return wrappedFOV;
        }

        /// <summary>
        /// Calculates the FOV for a creature
        /// </summary>
        /// <param name="creature"></param>
        public CreatureFOV CalculateNoRangeCreatureFOV(Creature creature)
        {
            Map currentMap = levels[creature.LocationLevel];

            //Update FOV
            fov.CalculateFOV(creature.LocationLevel, creature.LocationMap, 0);

            //Wrapper with game-specific FOV layer
            CreatureFOV wrappedFOV = new CreatureFOV(creature, new WrappedFOV(fov), creature.FOVType());

            return wrappedFOV;
        }

        /// <summary>
        /// Project a line until the maximum extent of the level
        /// </summary>
        /// <param name="start"></param>
        /// <param name="midPoint"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public Point GetEndOfLine(Point start, Point midPoint, int level)
        {
            int deltaX = midPoint.x - start.x;
            int deltaY = midPoint.y - start.y;

            Vector3 dirVector = new Vector3(deltaX, deltaY, 0);
            dirVector.Normalize();

            bool endNow = false;

            Vector3 startVector = new Vector3(start.x, start.y, 0);

            Vector3 lastVector = startVector;

            do
            {
                startVector += dirVector;

                if (startVector.X >= levels[level].width || startVector.X < 0 ||
                    startVector.Y >= levels[level].height || startVector.Y < 0)
                    endNow = true;
                else
                    lastVector = startVector;

            } while (!endNow);

            return new Point((int)Math.Floor(lastVector.X), (int)Math.Floor(lastVector.Y));

        }

        /// <summary>
        /// Get points on a line in order.
        /// /// Only returns points within FOV. Moral: If you can see it, you can shoot it.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<Point> GetPathLinePointsInFOV(int level, Point start, Point end, WrappedFOV fov)
        {
            List<Point> pointsToRet = new List<Point>();

            foreach (Point p in Utility.GetPointsOnLine(start, end))
            {
                if (fov.CheckTileFOV(level, p))
                {
                    pointsToRet.Add(p);
                }
            }

            return pointsToRet;
        }

        /// <summary>
        /// Calculates the FOV for a creature if it was in the location
        public CreatureFOV CalculateCreatureFOV(Creature creature, Point location)
        {
            Map currentMap = levels[creature.LocationLevel];

            //Update FOV
            fov.CalculateFOV(creature.LocationLevel, location, creature.SightRadius);

            //Wrapper with game-specific FOV layer
            CreatureFOV wrappedFOV = new CreatureFOV(creature, new WrappedFOV(fov), creature.FOVType(), location);

            return wrappedFOV;

        }

        /// <summary>
        /// Calculates the FOV for an abstract point. Uses the old TCODFov without modification
        public WrappedFOV CalculateAbstractFOV(int level, Point mapLocation, int sightRadius)
        {
            Map currentMap = levels[level];

            //Update FOV
            fov.CalculateFOV(level, mapLocation, sightRadius);

            return new WrappedFOV(fov);
        }


        /// <summary>
        /// Show all sounds on map for debug purposes
        /// </summary>
        public void ShowSoundsOnMap()
        {
            //Debug: show all sounds on the map

            foreach (SoundEffect effectPair in Game.Dungeon.Effects)
            {
                SoundEffect sEffect = effectPair;

                Map currentMap = levels[sEffect.LevelLocation];

                //Check if the sound is too old to draw
                double soundDecay = sEffect.DecayedMagnitude(WorldClock);

                if (soundDecay < 0.0001)
                    continue;

                int soundRadius = (int)Math.Ceiling(sEffect.SoundRadius());

                //Draw circle around sound

                int xl = sEffect.MapLocation.x - soundRadius;
                int xr = sEffect.MapLocation.x + soundRadius;

                int yt = sEffect.MapLocation.y - soundRadius;
                int yb = sEffect.MapLocation.y + soundRadius;

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
                        thisSquare.SoundMag = sEffect.DecayedMagnitude(WorldClock, sEffect.LevelLocation, new Point(i, j));
                    }
                }

            }

        }

        /// <summary>
        /// Displays the creature FOV on the map. Note that this clobbers the FOV map
        /// </summary>
        /// <param name="monster"></param>
        public void ShowCreatureFOVOnMap(Monster monster)
        {

            //Only do this if the creature is on a visible level
            if (monster.LocationLevel != Player.LocationLevel)
                return;

            Map currentMap = levels[monster.LocationLevel];

            //Calculate FOV
            CreatureFOV creatureFov = Game.Dungeon.CalculateCreatureFOV(monster);

            //Only check sightRadius around the creature

            int sightRangeMax = 20;
            int xl = Math.Max(0, monster.LocationMap.x - sightRangeMax);
            int xr = Math.Min(monster.LocationMap.x + sightRangeMax, currentMap.width - 1);

            int yt = Math.Max(0, monster.LocationMap.y - sightRangeMax);
            int yb = Math.Min(monster.LocationMap.y + sightRangeMax, currentMap.height - 1);

            //If sight is infinite, check all the map
            //if (creature.SightRadius == 0)
            //{
            //}

            //Always check the whole map (now we have strange FOVs)
            // (may not be necessary) [and is certainly slow]

            //According to profiling this is *BY FAR* the slowest thing in the game

            /*
            if (xl < 0)
                xl = 0;
            if (xr >= currentMap.width)
                xr = currentMap.width - 1;
            if (yt < 0)
                yt = 0;
            if (yb >= currentMap.height)
                yb = currentMap.height - 1;
             * */

            for (int i = xl; i <= xr; i++)
            {
                for (int j = yt; j <= yb; j++)
                {
                    MapSquare thisSquare = currentMap.mapSquares[i, j];
                    bool inFOV = creatureFov.CheckTileFOV(i, j);
                    if (inFOV)
                        thisSquare.InMonsterFOV = true;

                    //Show stealth radii too
                    bool inMonsterStealthRadius = monster.InStealthRadius(new Point(i, j));
                    if (inMonsterStealthRadius)
                    {
                        thisSquare.InMonsterStealthRadius = true;
                    }
                }
            }
        }

        public bool IsSquareInPlayerFOV(Location target)
        {
            if (target.Level != player.LocationLevel)
            {
                return false;
            }

            var targetCoord = target.MapCoord;

            if (targetCoord.x >= 0 && targetCoord.y >= 0 && targetCoord.x < Levels[target.Level].width && targetCoord.y < Levels[target.Level].height)
            {
                MapSquare targetSquare = Levels[target.Level].mapSquares[targetCoord.x, targetCoord.y];
                if (targetSquare.InPlayerFOV)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsSquareSeenByPlayer(Location target)
        {
            var targetCoord = target.MapCoord;

            if (targetCoord.x >= 0 && targetCoord.y >= 0 && targetCoord.x < Levels[target.Level].width && targetCoord.y < Levels[target.Level].height)
            {
                MapSquare targetSquare = Levels[target.Level].mapSquares[targetCoord.x, targetCoord.y];
                if (targetSquare.SeenByPlayer)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Recalculate the players FOV. Subsequent accesses to the TCODMap of the player's level will have his FOV
        /// Note that the maps may get hijacked by other creatures
        /// </summary>
        internal CreatureFOV CalculatePlayerFOV()
        {
            //Get TCOD to calculate the player's FOV
            Map currentMap = levels[Player.LocationLevel];

            CreatureFOV tcodFOV = Game.Dungeon.CalculateCreatureFOV(player);

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

            return tcodFOV;
        }



        public long WorldClock
        {
            get
            {
                return worldClock;
            }
            //For serialization
            set
            {
                worldClock = value;
            }
        }

        /// <summary>
        /// Add a dungeon-wide sound effect, occurring now on the WorldClock
        /// mapLevel - dungeonLevel
        /// soundMagnitude - 0 -> 1
        /// mapLocation - mapLocation
        /// </summary>
        internal SoundEffect AddSoundEffect(double soundMagnitude, int mapLevel, Point mapLocation)
        {
            SoundEffect newEffect = new SoundEffect(nextUniqueSoundID, this, WorldClock, soundMagnitude, mapLevel, mapLocation);

            effects.Add(newEffect);
            nextUniqueSoundID++;
            LogFile.Log.LogEntryDebug("Adding new sound mag: " + soundMagnitude.ToString() + " at level: " + mapLevel.ToString() + " loc: " + mapLocation.ToString(), LogDebugLevel.Medium);

            return newEffect;
        }

        /// <summary>
        /// Get Sound Effect by ID. Sound effects are stored by ID in creatures for easier serialization
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal SoundEffect GetSoundByID(int id)
        {
            //Data structure really needs indexing on id
            foreach (SoundEffect ef in effects)
            {
                if (ef.ID == id)
                    return ef;
            }
            string msg = "Can't find sound ID: " + id;
            LogFile.Log.LogEntryDebug(msg, LogDebugLevel.High);
            throw new ApplicationException(msg);
        }

        /// <summary>
        /// Return sounds after (not including) a particular tick. Used to check for new sounds since last decision
        /// </summary>
        /// <param name="soundAfterThisTime"></param>
        /// <returns></returns>
        internal List<SoundEffect> GetSoundsAfterTime(long soundAfterThisTime)
        {
            List<SoundEffect> newSounds = new List<SoundEffect>();

            //Should do a binary search here

            //Could cache the result

            //SortedList doesn't let us do duplicate keys, so I need a filtering solution instead (inefficient)

            foreach (SoundEffect soundPair in effects)
            {
                // >= in case the monster and player go on the same tick
                if (soundPair.SoundTime >= soundAfterThisTime)
                    newSounds.Add(soundPair);
            }

            return newSounds;
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

        public IEnumerable<Item> ItemsAtLocation(Location loc)
        {
            return items.Where(i => i.IsLocatedAt(loc.Level, loc.MapCoord) && !i.InInventory);
        }

        internal List<Lock> LocksAtLocation(Location location)
        {
            List<Lock> locksAtLocation;
            locks.TryGetValue(location, out locksAtLocation);
            if (locksAtLocation == null)
                return new List<Lock>();
            return locksAtLocation;

        }

        internal bool NonOpenLocksAtLocation(Location location)
        {
            return LocksAtLocation(location).Select(l => !l.IsOpen()).Any();
        }

        /// <summary>
        /// Are there multiple items here?
        /// </summary>
        /// <param name="locationLevel"></param>
        /// <param name="locationMap"></param>
        /// <returns></returns>
        internal bool MultipleItemAtSpace(int locationLevel, Point locationMap)
        {
            int itemCount = 0;

            foreach (Item item in items)
            {
                if (item.IsLocatedAt(locationLevel, locationMap) &&
                    !item.InInventory)
                {
                    itemCount++;
                }
            }

            if (itemCount < 2)
                return false;
            return true;
        }

        public Creature CreatureAtSpaceIncludingPlayer(int locationLevel, Point locationMap)
        {
            var monsterAtSpace = MonsterAtSpace(locationLevel, locationMap);
            bool playerAtSpace = (Player.LocationLevel == locationLevel) && (Player.LocationMap == locationMap);

            if (playerAtSpace)
                return Player;
            return monsterAtSpace;
        }

        /// <summary>
        /// Return an creature if there is one at the requested square, or return null if not
        /// </summary>
        public Monster MonsterAtSpace(int locationLevel, Point locationMap)
        {
            List<Monster> monsters = Monsters;

            foreach (Monster monster in monsters)
            {
                if (monster.LocationLevel == locationLevel && monster.LocationMap == locationMap)
                {
                    return monster;
                }
            }

            return null;
        }

        /// <summary>
        /// Removes an item from the game entirely.
        /// </summary>
        /// <param name="itemToUse"></param>
        private void RemoveItem(Item itemToUse)
        {
            items.Remove(itemToUse);
        }

        /// <summary>
        /// Hides an item so it can't be interacted with
        /// </summary>
        /// <param name="itemToUse"></param>
        public void HideItem(Item itemToUse)
        {
            itemToUse.InInventory = true;
        }

        /// <summary>
        /// Open the door at the requested location. Returns true if the door was successfully opened
        /// </summary>
        /// <param name="p"></param>
        /// <param name="doorLocation"></param>
        /// <returns></returns>
        internal bool OpenDoor(Location target)
        {
            try
            {
                //Check there is a door here                
                MapTerrain doorTerrain = GetTerrainAtLocation(target);

                if (doorTerrain != MapTerrain.ClosedDoor)
                {
                    return false;
                }

                SetTerrainAtPoint(target, MapTerrain.OpenDoor);

                return true;
            }
            catch (ApplicationException)
            {
                //Not a valid location - should not occur
                LogFile.Log.LogEntry("Non-valid location for door requested");
                return false;
            }
        }

        public void SetTerrainAtPoint(Location location, MapTerrain newTerrain)
        {
            SetTerrainAtPoint(location.Level, location.MapCoord, newTerrain);
        }
        /// <summary>
        /// Set a point to a new type of terrain and update pathing and fov
        /// </summary>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <param name="newTerrain"></param>
        public void SetTerrainAtPoint(int level, Point location, MapTerrain newTerrain)
        {
            //Update map

            levels[level].mapSquares[location.x, location.y].Terrain = newTerrain;

            levels[level].mapSquares[location.x, location.y].Walkable = MapUtils.IsTerrainWalkable(newTerrain) ? true : false;
            levels[level].mapSquares[location.x, location.y].BlocksLight = MapUtils.IsTerrainLightBlocking(newTerrain) ? true : false;

            //Update pathing and fov
            fov.updateFovMap(level, location, MapUtils.IsTerrainLightBlocking(newTerrain) ? FOVTerrain.Blocking : FOVTerrain.NonBlocking);

            PathingTerrain pathingTerrain;
            if (newTerrain == MapTerrain.ClosedDoor)
                pathingTerrain = PathingTerrain.ClosedDoor;
            else if (newTerrain == MapTerrain.ClosedLock)
                pathingTerrain = PathingTerrain.ClosedLock;
            else
                pathingTerrain = MapUtils.IsTerrainWalkable(newTerrain) ? PathingTerrain.Walkable : PathingTerrain.Unwalkable;
            Pathing.PathFindingInternal.updateMap(level, location, pathingTerrain);
        }

        /// <summary>
        /// Close the door at the requested location. Returns true if the door was successfully opened
        /// </summary>
        /// <param name="p"></param>
        /// <param name="doorLocation"></param>
        /// <returns></returns>
        internal bool CloseDoor(int level, Point doorLocation)
        {
            try
            {
                //Check there is a door here                
                MapTerrain doorTerrain = GetTerrainAtPoint(player.LocationLevel, doorLocation);

                if (doorTerrain != MapTerrain.OpenDoor)
                {
                    return false;
                }

                SetTerrainAtPoint(level, doorLocation, MapTerrain.ClosedDoor);

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
        /// Can't kill the player immediately now have to wait until end of monster loop
        /// </summary>
        /// <param name="deathString"></param>
        internal void SetPlayerDeath(string deathString)
        {
            PlayerDeathOccured = true;
            PlayerDeathString = deathString;
        }

        /// <summary>
        /// It's all gone wrong!
        /// </summary>
        internal void PlayerDeath(string verb)
        {
            if (PlayerImmortal && !verb.Contains("quit"))
            {
                Game.Dungeon.Player.HealCompletely();
                PlayerDeathOccured = false;
                return;
            }

            if (FunMode && !verb.Contains("quit"))
            {
                //Restart the level with 0 fame
                PlayerDeathOccured = false;
                return;
            }

            if (!verb.Contains("quit"))
            {
                //Reset vars
                PlayerDeathString = "";
                PlayerDeathOccured = false;

                LogFile.Log.LogEntryDebug("Player killed", LogDebugLevel.Medium);

                Game.Base.SystemActions.DoEndOfGame(false, false, false);
                //EndOfGame(false, false);
            }
            else
            {
                Game.Base.SystemActions.DoEndOfGame(false, false, true);
            }
        }


        public struct KillRecord
        {
            public int killCount;
            public List<string> killStrings;
            public int killScore;
        }
        /// <summary>
        /// Generate the grouped kill record for the player
        /// </summary>
        /// <returns></returns>
        public KillRecord GetKillRecord()
        {
            //fake
            /*
            KillRecord fakeKillRec = new KillRecord();
            fakeKillRec.killStrings = new List<string>();
            fakeKillRec.killCount = Game.Dungeon.player.KillCount;

            return fakeKillRec;*/

            //Make killCount list

            List<Monster> kills = player.Kills;
            List<KillCount> killCount = new List<KillCount>();

            int totalKills = 0;
            int totalScore = 0;

            foreach (Monster kill in kills)
            {
                totalKills++;
                totalScore += kill.CreatureCost();

                Type monsterType = kill.GetType();
                bool foundGroup = false;

                foreach (KillCount record in killCount)
                {
                    if (record.type.GetType() == monsterType)
                    {
                        record.count++;
                        foundGroup = true;
                        break;
                    }
                }

                //If there is no group, create a new one
                if (!foundGroup)
                {
                    KillCount newGroup = new KillCount();
                    newGroup.type = kill;
                    newGroup.count = 1;
                    killCount.Add(newGroup);
                }
            }

            List<string> killRecord = new List<string>();

            //Turn list into strings to be displayed
            foreach (KillCount record in killCount)
            {

                string killStr = "";

                if (record.count == 1)
                {
                    killStr += "1 " + record.type.SingleDescription;
                }
                else
                {
                    killStr += record.count.ToString() + " " + record.type.GroupDescription;
                }

                //Add to string list
                killRecord.Add(killStr);
            }

            KillRecord recordS = new KillRecord();
            recordS.killStrings = killRecord;
            recordS.killCount = totalKills;
            recordS.killScore = totalScore;
            return recordS;
        }

        /// <summary>
        /// The player learns a new move. Right now doesn't use the parameter (except as a reference) and just updates the Known parameter
        /// </summary>
        internal void LearnMove(SpecialMove moveToLearn)
        {
            LogFile.Log.LogEntryDebug("Player learnt move: " + moveToLearn.MoveName(), LogDebugLevel.Medium);

            foreach (SpecialMove move in specialMoves)
            {
                if (move.GetType() == moveToLearn.GetType())
                {
                    move.Known = true;
                }
            }
        }

        /// <summary>
        /// Returns points in grenade template. Includes walls and everything.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public List<Point> GetPointsForGrenadeTemplate(Point location, int level, double size)
        {
            List<Point> splashSquares = new List<Point>();

            int sizeI = (int)Math.Ceiling(size) + 1;

            for (int i = location.x - sizeI; i < location.x + sizeI; i++)
            {
                for (int j = location.y - sizeI; j < location.y + sizeI; j++)
                {
                    if (i >= 0 && i < levels[level].width && j >= 0 && j < levels[level].height)
                    {
                        if (Math.Pow(i - location.x, 2) + Math.Pow(j - location.y, 2) < Math.Pow(size, 2) + 0.1)
                        {
                            splashSquares.Add(new Point(i, j));
                        }
                    }
                }
            }

            return splashSquares;
        }

        public void RunDungeonTriggers(int level, Point mapLocation)
        {
            var location = new Location(level, mapLocation);

            if (!Triggers.ContainsKey(location))
                return;

            var triggersAtPoint = Triggers[location];

            foreach (DungeonSquareTrigger trigger in triggersAtPoint)
            {
                trigger.CheckTrigger(level, mapLocation);
            }
        }

        public bool AddTrigger(DungeonSquareTrigger trigger, Location location)
        {
            return AddTrigger(location.Level, location.MapCoord, trigger);
        }

        public bool AddTrigger(int level, Point point, DungeonSquareTrigger trigger)
        {
            //Set the trigger position
            trigger.Level = level;
            trigger.mapPosition = point;

            Location loc = new Location(level, point);

            List<DungeonSquareTrigger> triggerListAtLocation;
            Triggers.TryGetValue(loc, out triggerListAtLocation);

            if (triggerListAtLocation == null)
            {
                Triggers[loc] = new List<DungeonSquareTrigger>();
                Triggers[loc].Add(trigger);
            }
            else
            {
                triggerListAtLocation.Add(trigger);
            }

            return true;
        }



        /// <summary>
        /// Delete save file on player death
        /// </summary>
        private void DeleteSaveFile()
        {
            try
            {
                string filename = player.Name + ".sav";

                File.Delete(filename);
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Couldn't delete save file: " + ex.Message);
            }
        }

        public void SaveObituary(List<string> deathPreamble, List<string> killRecord)
        {
            try
            {

                //Date stamp
                DateTime dateTime = DateTime.Now;
                string timeStamp = dateTime.Year.ToString("0000") + "-" + dateTime.Month.ToString("00") + "-" + dateTime.Day.ToString("00") + "_" + dateTime.Hour.ToString("00") + "-" + dateTime.Minute.ToString("00") + "-" + dateTime.Second.ToString("00");


                Directory.CreateDirectory("obituary");
                string obFilename = "obituary/" + Game.Dungeon.player.Name + " epilogue " + timeStamp + ".txt";

                StreamWriter obFile = new StreamWriter(obFilename);

                foreach (string s in deathPreamble)
                {
                    obFile.WriteLine(s);
                }

                foreach (string s in killRecord)
                {
                    obFile.WriteLine(s);
                }
                obFile.Close();
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Couldn't write obituary file " + ex.Message);
            }

        }

        /// <summary>
        /// Add monsters from the summoning queue to the actual dungeon. Clear at the end. Some monsters may not add if things have moved
        /// </summary>
        internal void AddDynamicMonsters()
        {
            foreach (Monster monster in summonedMonsters)
            {
                Game.Dungeon.AddMonster(monster, monster.LocationLevel, monster.LocationMap);
            }

            summonedMonsters.Clear();
        }


        /// <summary>
        /// Attempt to charm a monster in a target direction.
        /// Returns whether time passes (not if there is a successful charm)
        /// </summary>
        /// <param name="direction"></param>
        internal bool AttemptCharmMonsterByPlayer(Point direction)
        {
            //Work out the monster's location

            Point targetLocation = new Point(Game.Dungeon.Player.LocationMap.x + direction.x, Game.Dungeon.Player.LocationMap.y + direction.y);

            Player player = Game.Dungeon.Player;

            //Is there a monster here?

            if (!Game.Dungeon.MapSquareIsWalkable(player.LocationLevel, targetLocation))
            {
                //No monster
                Game.MessageQueue.AddMessage("No target.");
                return false;
            }
            else
            {
                //Check for monsters in the square
                SquareContents contents = MapSquareContents(player.LocationLevel, targetLocation);

                //Monster - try to charm it
                if (contents.monster != null)
                {
                    Monster monster = contents.monster;

                    //Is the creature already charmed?
                    if (monster.Charmed)
                    {
                        Game.MessageQueue.AddMessage("The creature is already charmed.");
                        return false;
                    }

                    //Check if this class of creature can be charmed or passified
                    if (!monster.CanBeCharmed() || monster.GetCharmRes() >= player.CharmPoints)// && !monster.CanBePassified())
                    {
                        Game.MessageQueue.AddMessage("The " + monster.SingleDescription + " laughs at your feeble attempt.");
                        return true;
                    }

                    //   bool canCharm = true;
                    /*
                    if (!monster.CanBeCharmed())
                    {
                        //On for passify only
                        canCharm = false;
                    }*/

                    //Try to charm, may fail if the player has no more charms


                    bool playerOK = false;
                    //  if (canCharm)
                    //  {
                    //Check if the player has any more charms
                    playerOK = player.MoreCharmedCreaturesPossible();
                    //}

                    if (!playerOK)
                    {
                        //canCharm = false;
                        Game.MessageQueue.AddMessage("Too many charmed creatures.");
                        return true;
                        //return true;
                    }


                    //Test against statistic here
                    int monsterRes = monster.GetCharmRes();
                    int charmRoll = Game.Random.Next(player.CharmPoints);

                    LogFile.Log.LogEntryDebug("Charm attempt. Res: " + monsterRes + " Roll: " + charmRoll, LogDebugLevel.Medium);

                    if (charmRoll < monsterRes)
                    {
                        //Charm not successful
                        string msg;

                        int chance = Game.Random.Next(100);

                        if (chance < 50)
                            msg = "The " + monster.SingleDescription + " does not look convinced by your overtures.";
                        else
                            msg = "The " + monster.SingleDescription + " ignores you and attacks.";

                        Game.MessageQueue.AddMessage(msg);
                        return true;
                    }


                    //All OK do the charm
                    //   if (canCharm)
                    //    {
                    //Should be possible at this point
                    player.AddCharmCreatureIfPossible();

                    int chance2 = Game.Random.Next(100);

                    string msg2;
                    if (chance2 < 30)
                        msg2 = "The " + monster.SingleDescription + " looks at you lovingly.";
                    else if (chance2 < 65)
                        msg2 = "The " + monster.SingleDescription + " can't resist your charms.";
                    else
                        msg2 = "The " + monster.SingleDescription + " is all over you.";
                    Game.MessageQueue.AddMessage(msg2);
                    contents.monster.CharmCreature();

                    //Add XP
                    double diffDelta = (player.CharmStat - monsterRes) / (double)player.CharmStat;
                    if (diffDelta < 0)
                        diffDelta = 0;

                    double xpUpChance = 1 - diffDelta;
                    int xpUpRoll = (int)Math.Floor(xpUpChance * 100.0);
                    int xpUpRollActual = Game.Random.Next(100);
                    LogFile.Log.LogEntryDebug("CharmXP up. Chance: " + xpUpRoll + " roll: " + xpUpRollActual, LogDebugLevel.Medium);

                    if (xpUpRollActual < xpUpRoll)
                    {
                        player.CharmXP++;
                        Game.MessageQueue.AddMessage("You feel more charming.");
                        LogFile.Log.LogEntryDebug("CharmXP up!", LogDebugLevel.Medium);
                    }

                    //Set intrinsic on the player
                    player.CharmUse = true;

                    return true;
                    //    }

                    /*
                //Only a passify
                else
                {
                    //Test against statistic here
                        
                    string msg = "The " + monster.SingleDescription + " sighs and turns away.";

                    Game.MessageQueue.AddMessage(msg);
                    contents.monster.PassifyCreature();

                    return true;
                }*/

                }
                else
                {
                    //No monster
                    Game.MessageQueue.AddMessage("No target.");
                    return false;
                }
            }
            //return false;
        }

        /// <summary>
        /// Remove all active effects on monsters. Used when we leave a dungeon
        /// to ensure events are cancelled before we meet the monster again
        /// </summary>
        void RemoveAllMonsterEffects()
        {
            //Increment time on events and remove finished ones
            List<PlayerEffect> finishedEffects = new List<PlayerEffect>();

            foreach (Monster monster in monsters)
            {
                monster.RemoveAllEffects();
            }
        }

        /// <summary>
        /// Attempt to uncharm a monster in a target direction.
        /// Returns whether time passes (not if there is a successful charm)
        /// </summary>
        /// <param name="direction"></param>
        internal bool UnCharmMonsterByPlayer(Point direction)
        {
            //Work out the monster's location

            Point targetLocation = new Point(Game.Dungeon.Player.LocationMap.x + direction.x, Game.Dungeon.Player.LocationMap.y + direction.y);

            Player player = Game.Dungeon.Player;

            //Is there a monster here?

            if (!Game.Dungeon.MapSquareIsWalkable(player.LocationLevel, targetLocation))
            {
                //No monster
                Game.MessageQueue.AddMessage("No target.");
                return false;
            }
            else
            {
                //Check for monsters in the square
                SquareContents contents = MapSquareContents(player.LocationLevel, targetLocation);

                //Monster - is it already charmed
                if (contents.monster != null)
                {
                    Monster monster = contents.monster;

                    //Is the creature already charmed?
                    if (monster.Charmed)
                    {
                        Game.MessageQueue.AddMessage("The creature looks wistful and then goes about its business.");
                        monster.UncharmCreature();
                        monster.PassifyCreature();

                        player.RemoveCharmedCreature();

                        return true;
                    }
                    else
                    {
                        //Not charmed

                        Game.MessageQueue.AddMessage("The creature is not charmed.");
                        return false;
                    }
                }
                else
                {
                    //No monster
                    Game.MessageQueue.AddMessage("No target.");
                    return false;
                }
            }
        }

        /// <summary>
        /// Remove all temporary items
        /// </summary>
        private void RemoveInventoryItems()
        {
            Inventory inv = player.Inventory;

            List<Item> itemsToRemove = new List<Item>();

            foreach (Item item in inv.Items)
            {
                IEquippableItem itemE = item as IEquippableItem;

                if (itemE == null)
                {
                    itemsToRemove.Add(item);
                }

            }

            LogFile.Log.LogEntryDebug("Removing " + itemsToRemove.Count + " items from inventory.", LogDebugLevel.Medium);
            if (itemsToRemove.Count > 0)
            {
                Game.MessageQueue.AddMessage("The berries you found fade away.");
            }


            foreach (Item item in itemsToRemove)
            {
                inv.RemoveItem(item);
            }
        }

        /// <summary>
        /// Recharge all items in the game
        /// </summary>
        private void RechargeEquippableItems()
        {
            Inventory inv = player.Inventory;

            foreach (Item item in inv.Items)
            {
                IUseableItem itemU = item as IUseableItem;
                if (itemU != null)
                    itemU.UsedUp = false;
            }
        }

        private Tuple<int, List<string>> ItemsUsedSummary()
        {
            var shieldsUsed = Player.Inventory.GetItemsOfType<Items.ShieldPack>().Count();
            var ammoUsed = Player.Inventory.GetItemsOfType<Items.AmmoPack>().Count();

            var itemText = new List<string>();
            itemText.Add(shieldsUsed + " " + new Items.ShieldPack().GroupItemDescription + " used.");

            itemText.Add(ammoUsed + " " + new Items.AmmoPack().GroupItemDescription + " used.");

            return new Tuple<int, List<string>>(0, itemText);
        }

        private IEnumerable<Feature> GetAllFeatures()
        {
            return features.SelectMany(f => f.Value);
        }

        /// <summary>
        /// Move player to next mission
        /// </summary>
        public void MoveToLevel(int levelNo)
        {
            if (levelNo < 0 || levelNo >= levels.Count)
                return;

            //Find any elevator that goes here
            var elevator = GetAllFeatures().Where(f => f.GetType() == typeof(Features.Elevator) && (f as Features.Elevator).DestLevel == levelNo);
            if (elevator.Count() == 0)
            {
                LogFile.Log.LogEntryDebug("Failed to find elevator to get to on level " + levelNo, LogDebugLevel.Medium);
                return;
            }

            var preferredElevator = elevator.Where(f => f.GetType() == typeof(Features.Elevator) && (f as Features.Elevator).LocationLevel == player.LocationLevel);

            if (preferredElevator.Count() > 0)
            {
                var elevatorToUse = preferredElevator.ElementAt(0) as Features.Elevator;
                elevatorToUse.PlayerInteraction(player);
            }
            else
            {
                var elevatorToUse = elevator.ElementAt(0) as Features.Elevator;
                elevatorToUse.PlayerInteraction(player);
            }
        }

        /// <summary>
        /// Bit of a hack, override the tileset per level
        /// </summary>
        private void SelectTilesetForMission(int level)
        {
            //Tutorial levels

            if (level < 6)
            {
                StringEquivalent.OverrideTerrainChar(MapTerrain.Wall, '#');
            }

            else if (level < 11)
            {
                StringEquivalent.OverrideTerrainChar(MapTerrain.Wall, '\xb0');
            }

            else if (level == 14)
            {
                StringEquivalent.OverrideTerrainChar(MapTerrain.Wall, '\xdb');
            }

            else
            {
                StringEquivalent.OverrideTerrainChar(MapTerrain.Wall, '\xb2');
            }


        }

        private void ResetSounds()
        {
            effects.Clear();
            nextUniqueSoundID = 0;

        }

        public List<Point> GetWalkableAdjacentSquaresFreeOfCreatures(int locationLevel, Point locationMap)
        {
            Map levelMap = levels[locationLevel];

            List<Point> adjacentSqFree = new List<Point>();
            List<Point> adjacentSq = MapUtils.GetAdjacentCoords(locationMap);

            foreach (Point p in adjacentSq)
            {
                if (p.x >= 0 && p.x < levelMap.width
                    && p.y >= 0 && p.y < levelMap.height)
                {
                    if (!MapSquareIsWalkable(locationLevel, p))
                    {
                        continue;
                    }

                    //Check square has nothing else on it
                    SquareContents contents = MapSquareContents(locationLevel, p);

                    if (contents.monster != null)
                    {
                        continue;
                    }

                    if (contents.player != null)
                        continue;

                    //Empty and walkable
                    adjacentSqFree.Add(p);
                }
            }

            return adjacentSqFree;
        }

        public List<Point> GetValidMapSquaresWithinRange(int locationLevel, Point locationMap, int range)
        {
            List<Point> pointsToReturn = new List<Point>();
            Map currentMap = levels[locationLevel];

            int xl = locationMap.x - range;
            int xr = locationMap.x + range;

            int yt = locationMap.y - range;
            int yb = locationMap.y + range;

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
                    pointsToReturn.Add(new Point(i, j));
                }
            }

            return pointsToReturn;
        }

        public List<Point> GetWalkableSquaresFreeOfCreaturesWithinRange(int locationLevel, Point locationMap, int minRange, int maxRange)
        {
            Map levelMap = levels[locationLevel];

            List<Point> adjacentSqFree = new List<Point>();
            List<Point> maxRangeSq = GetValidMapSquaresWithinRange(locationLevel, locationMap, maxRange);
            List<Point> minRangeSq = GetValidMapSquaresWithinRange(locationLevel, locationMap, minRange);

            var adjacentSq = maxRangeSq.Except(minRangeSq);
            
            foreach (Point p in adjacentSq)
            {

                if (!MapSquareIsWalkable(locationLevel, p))
                {
                    continue;
                }

                //Check square has nothing else on it
                SquareContents contents = MapSquareContents(locationLevel, p);

                if (contents.monster != null)
                {
                    continue;
                }

                if (contents.player != null)
                    continue;

                //Empty and walkable
                adjacentSqFree.Add(p);

            }

            return adjacentSqFree;
        }

        public List<Point> GetWalkableAdjacentSquares(int locationLevel, Point locationMap)
        {
            Map levelMap = levels[locationLevel];

            List<Point> adjacentSqFree = new List<Point>();
            List<Point> adjacentSq = MapUtils.GetAdjacentCoords(locationMap);

            foreach (Point p in adjacentSq)
            {
                if (p.x >= 0 && p.x < levelMap.width
                    && p.y >= 0 && p.y < levelMap.height)
                {
                    if (!MapSquareIsWalkable(locationLevel, p))
                    {
                        continue;
                    }
                    
                    //Empty and walkable
                    adjacentSqFree.Add(p);
                }
            }

            return adjacentSqFree;
        }

        public List<Point> GetWalkableAdjacentSquaresFreeOfCreaturesAndDangerousTerrain(int locationLevel, Point locationMap)
        {
            Map levelMap = levels[locationLevel];

            List<Point> adjacentSqFree = new List<Point>();
            List<Point> adjacentSq = MapUtils.GetAdjacentCoords(locationMap);

            foreach (Point p in adjacentSq)
            {
                if (p.x >= 0 && p.x < levelMap.width
                    && p.y >= 0 && p.y < levelMap.height)
                {
                    if (!MapSquareIsWalkable(locationLevel, p))
                    {
                        continue;
                    }

                    //Check square has nothing else on it
                    SquareContents contents = MapSquareContents(locationLevel, p);

                    if (contents.monster != null)
                    {
                        continue;
                    }

                    if (contents.player != null)
                        continue;

                    //Check for dangerous features

                    var dangeousTerrainAtPoint = Game.Dungeon.GetFeaturesAtLocation(new Location(locationLevel, p)).Where(f => f is DangerousActiveFeature);
                    if (dangeousTerrainAtPoint.Count() > 0)
                        continue;

                    //Empty and walkable
                    adjacentSqFree.Add(p);
                }
            }

            return adjacentSqFree;
        }

        public List<Point> GetWalkableAdjacentSquaresFreeOfCreaturesAndItems(int locationLevel, Point locationMap)
        {
            Map levelMap = levels[locationLevel];

            List<Point> adjacentSqFree = new List<Point>();
            List<Point> adjacentSq = MapUtils.GetAdjacentCoords(locationMap);

            foreach (Point p in adjacentSq)
            {
                if (p.x >= 0 && p.x < levelMap.width
                    && p.y >= 0 && p.y < levelMap.height)
                {
                    if (!MapSquareIsWalkable(locationLevel, p))
                    {
                        continue;
                    }

                    //Check square has nothing else on it
                    SquareContents contents = MapSquareContents(locationLevel, p);

                    if (contents.monster != null)
                    {
                        continue;
                    }

                    if (contents.player != null)
                        continue;

                    if (contents.items.Any())
                        continue;

                    //Empty and walkable
                    adjacentSqFree.Add(p);
                }
            }

            return adjacentSqFree;
        }

        public IEnumerable<Point> GetWalkablePointsFromSet(int level, IEnumerable<Point> allPossiblePoints)
        {
            return allPossiblePoints.SelectMany(p => MapSquareIsWalkable(level, p) ? new List<Point> { p } : new List<Point>());
        }

        /// <summary>
        /// Source is the person firing
        /// </summary>
        /// <param name="sourceLevel"></param>
        /// <param name="source"></param>
        /// <param name="destLevel"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        internal Tuple<int, int> GetNumberOfCoverItemsBetweenPoints(int sourceLevel, Point source, int destLevel, Point dest)
        {
            if (sourceLevel != destLevel)
                return new Tuple<int, int>(0, 0);

            //Any intervening cover. Stuff we're sitting on doesn't count
            var pointsOnLine = Utility.GetPointsOnLine(source, dest).Except(new List<Point> { source, dest });

            var hardCover = 0;
            var softCover = 0;

            foreach (Point p in pointsOnLine)
            {
                var terrain = GetTerrainAtPoint(sourceLevel, p);

                if (!MapUtils.IsTerrainWalkable(terrain))
                    hardCover++;
            }

            //Blocking features affect the terrain flags and are counted above

            //Each non-blocking feature counts as soft cover
            softCover += pointsOnLine.Select(p => GetFeaturesAtLocation(new Location(sourceLevel, p)).Where(f => !f.IsBlocking).Count()).Sum();
            
            return new Tuple<int, int>(hardCover, softCover);
        }


        internal bool DangerousFeatureAtLocation(int LocationLevel, Point newLocation)
        {
            var dangeousTerrainAtPoint = Game.Dungeon.GetFeaturesAtLocation(new Location(LocationLevel, newLocation)).Where(f => f is DangerousActiveFeature);
            if (dangeousTerrainAtPoint.Count() > 0)
                return true;
            return false;
        }


        internal void ShutDoor(int level)
        {
            DoorStatus[level] = true;
        }

        internal bool CheckDoor(int level)
        {
            bool status = false;
            DoorStatus.TryGetValue(level, out status);

            return status;
        }

    }
}
