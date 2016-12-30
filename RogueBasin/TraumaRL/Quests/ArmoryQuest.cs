using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL.Quests
{
    enum Ware
    {
        Boost,
        Shield,
        Aim,
        Stealth
    }

    class ArmoryQuest : Quest
    {
        int medicalLevel = 0;

        Dictionary<int, int> goodyRooms;
        Dictionary<int, string> goodyRoomKeyNames;
        Dictionary<int, List<Item>> itemsInArmory = new Dictionary<int, List<Item>>();

        public ArmoryQuest(QuestMapBuilder builder, LogGenerator logGen)
            : base(builder, logGen)
        {

        }

        public override void SetupQuest(MapState mapState)
        {
            SetupGoodyRooms(mapState);
            PlaceLootInArmory(mapState);
            AddGoodyQuestLogClues(mapState);
        }

        private void SetupGoodyRooms(MapState mapState)
        {

            //Ensure that we have a goody room on every level that will support it
            var levelInfo = mapState.LevelInfo;
            var replaceableVaultsForLevels = levelInfo.ToDictionary(kv => kv.Key, kv => kv.Value.ReplaceableVaultConnections.Except(kv.Value.ReplaceableVaultConnectionsUsed));
            goodyRooms = new Dictionary<int, int>();
            goodyRoomKeyNames = new Dictionary<int, string>();

            var manager = mapState.DoorAndClueManager;

            foreach (var kv in replaceableVaultsForLevels)
            {
                if (kv.Value.Count() == 0)
                {
                    LogFile.Log.LogEntryDebug("No vaults left for armory on level " + kv.Key, LogDebugLevel.High);
                    continue;
                }

                var thisLevel = kv.Key;
                var thisConnection = kv.Value.RandomElement();
                var thisRoom = thisConnection.Target;

                LogFile.Log.LogEntryDebug("Placing goody room at: level: " + thisLevel + " room: " + thisRoom, LogDebugLevel.Medium);

                //Place door
                var doorReadableId = mapState.LevelNames[thisLevel] + " armory";
                var doorId = doorReadableId;

                var unusedColor = Builder.GetUnusedColor();
                var clueName = unusedColor.Item2 + " key card";

                Builder.PlaceLockedDoorOnMap(mapState, doorId, clueName, 1, unusedColor.Item1, thisConnection);

                goodyRooms[thisLevel] = thisRoom;

                //Clue
                var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(doorId);

                //Assume a critical path from the lower level elevator
                var lowerLevelFloor = levelInfo[thisLevel].ConnectionsToOtherLevels.Min(level => level.Key);
                var elevatorFromLowerLevel = levelInfo[thisLevel].ConnectionsToOtherLevels[lowerLevelFloor].Target;
                var criticalPath = mapState.MapInfo.Model.GetPathBetweenVerticesInReducedMap(elevatorFromLowerLevel, thisRoom);

                var filteredRooms = Builder.FilterRoomsByPath(mapState, allowedRoomsForClues, criticalPath, true, QuestMapBuilder.CluePath.NotOnCriticalPath, true);
                var roomsToPlaceMonsters = new List<int>();

                var roomsForMonsters = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, 1, filteredRooms);
                var clues = manager.AddCluesToExistingDoor(doorId, roomsForMonsters);


                goodyRoomKeyNames[thisLevel] = clueName;
                var cluesAndColors = clues.Select(c => new Tuple<Clue, System.Drawing.Color, string>(c, unusedColor.Item1, clueName));
                Builder.PlaceSimpleClueItems(mapState, cluesAndColors, true, false);

                //Vault is used
                levelInfo[thisLevel].ReplaceableVaultConnectionsUsed.Add(thisConnection);
            }

        }

        private void AddGoodyQuestLogClues(MapState mapState)
        {
            //Ensure that we have a goody room on every level that will support it
            var manager = mapState.DoorAndClueManager;
            var mapInfo = mapState.MapInfo;
            var levelInfo = mapState.LevelInfo;

            foreach (var kv in goodyRooms)
            {
                var thisLevel = kv.Key;
                var thisRoom = kv.Value;

                var doorId = mapState.LevelNames[thisLevel] + " armory";

                //Clue
                var allowedRoomsForClues = manager.GetValidRoomsToPlaceClueForDoor(doorId);

                //Assume a critical path from the lower level elevator
                var lowerLevelFloor = levelInfo[thisLevel].ConnectionsToOtherLevels.Min(level => level.Key);
                var elevatorFromLowerLevel = levelInfo[thisLevel].ConnectionsToOtherLevels[lowerLevelFloor].Target;
                var criticalPath = mapInfo.Model.GetPathBetweenVerticesInReducedMap(elevatorFromLowerLevel, thisRoom);

                //Logs - try placing them on the critical path from the start of the game!

                var criticalPathFromStart = mapInfo.Model.GetPathBetweenVerticesInReducedMap(0, thisRoom);
                var preferredRoomsForLogsNonCritical = Builder.FilterRoomsByPath(mapState, allowedRoomsForClues, criticalPath, false, QuestMapBuilder.CluePath.OnCriticalPath, true);

                var roomsForLogsNonCritical = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(mapState, 1, preferredRoomsForLogsNonCritical);

                var logClues = manager.AddCluesToExistingDoor(doorId, roomsForLogsNonCritical);
                var clueName = goodyRoomKeyNames[thisLevel];
                var log1 = new Tuple<LogEntry, Clue>(LogGen.GenerateGoodyRoomLogEntry(mapState, clueName, thisLevel, itemsInArmory[thisLevel]), logClues[0]);
                Builder.PlaceLogClues(mapState, new List<Tuple<LogEntry, Clue>> { log1 }, true, true);
            }

        }

        private void PlaceLootInArmory(MapState mapState)
        {
            var levelDifficulty = mapState.LevelDifficulty;

            //Add standard loot
            AddStandardLootToArmory(mapState, Builder);

            //Weapons and wetware

            itemsInArmory = new Dictionary<int, List<Item>>();

            foreach (var l in mapState.GameLevels)
            {
                itemsInArmory[l] = new List<Item>();
            }

            var level1Ware = new List<Item>();

            var lootLevels = new Dictionary<int, List<Item>>();

            lootLevels[0] = new List<Item> { new RogueBasin.Items.Shotgun(), new RogueBasin.Items.Vibroblade() };
            lootLevels[0].AddRange(level1Ware);

            lootLevels[1] = new List<Item> { new RogueBasin.Items.Laser(), new RogueBasin.Items.HeavyPistol() };

            lootLevels[2] = new List<Item>();
            // {   };
            lootLevels[3] = new List<Item> { new RogueBasin.Items.HeavyLaser() };
            //new RogueBasin.Items.BoostWare(2),  new RogueBasin.Items.StealthWare()

            lootLevels[4] = new List<Item> { new RogueBasin.Items.AssaultRifle(), new RogueBasin.Items.HeavyShotgun(), };
            //new RogueBasin.Items.BoostWare(3), new RogueBasin.Items.AimWare(3), new RogueBasin.Items.ShieldWare(3)

            var itemsPlaced = new List<Item>();

            var wareInGame = new List<Ware> { Ware.Aim, Ware.Shield, Ware.Boost, Ware.Stealth }.RandomElements(3);

            foreach (var ware in Enum.GetValues(typeof(Ware)).Cast<Ware>())
            {
                if (!wareInGame.Contains(ware))
                    continue;

                if (ware == Ware.Aim)
                {
                    level1Ware.Add(new RogueBasin.Items.AimWare(1));
                    lootLevels[2].AddRange(new List<Item> { new RogueBasin.Items.AimWare(2) });
                    lootLevels[3].AddRange(new List<Item> { new RogueBasin.Items.AimWare(3) });
                }
                if (ware == Ware.Shield)
                {
                    level1Ware.Add(new RogueBasin.Items.ShieldWare(1));
                    lootLevels[2].AddRange(new List<Item> { new RogueBasin.Items.ShieldWare(2) });
                    lootLevels[3].AddRange(new List<Item> { new RogueBasin.Items.ShieldWare(3) });
                }
                if (ware == Ware.Boost)
                {
                    level1Ware.Add(new RogueBasin.Items.BoostWare(1));
                    lootLevels[2].AddRange(new List<Item> { new RogueBasin.Items.BoostWare(2) });
                    lootLevels[3].AddRange(new List<Item> { new RogueBasin.Items.BoostWare(3) });
                }
                if (ware == Ware.Stealth)
                {
                    lootLevels[2].AddRange(new List<Item> { new RogueBasin.Items.StealthWare() });
                }
            }

            //Give 1 ware
            var itemsGivenToPlayer = PlayerInitialItems(level1Ware);

            itemsPlaced.AddRange(itemsGivenToPlayer);

            lootLevels[0].AddRange(level1Ware.Except(itemsGivenToPlayer));

            //Guarantee on medical, at least 1 ware and a pistol or vibroblade
            var randomWare = level1Ware.Except(itemsPlaced).RandomElement();
            Builder.PlaceItems(mapState, new List<Item> { randomWare }, new List<int> { goodyRooms[medicalLevel] }, false);
            itemsPlaced.Add(randomWare);
            itemsInArmory[0].Add(randomWare);

            Builder.PlaceItems(mapState, new List<Item> { lootLevels[0][0] }, new List<int> { goodyRooms[medicalLevel] }, false);
            itemsPlaced.Add(lootLevels[0][0]);
            itemsInArmory[0].Add(lootLevels[0][0]);
            Builder.PlaceItems(mapState, new List<Item> { lootLevels[0][1] }, new List<int> { goodyRooms[medicalLevel] }, false);
            itemsPlaced.Add(lootLevels[0][1]);
            itemsInArmory[0].Add(lootLevels[0][0]);

            var levelsToHandleSeparately = new List<int> { medicalLevel };

            var totalLoot = lootLevels.SelectMany(kv => kv.Value).Except(itemsPlaced).Count();
            var totalRooms = goodyRooms.Select(kv => kv.Key).Except(levelsToHandleSeparately).Count();

            double lootPerRoom = totalLoot / (double)totalRooms;
            int lootPerRoomInt = (int)Math.Floor(lootPerRoom);

            int lootPlaced = 0;
            int roomsDone = 0;

            foreach (var kv in goodyRooms.OrderBy(k => k.Key))
            {
                var level = kv.Key;
                var room = kv.Value;

                if (levelsToHandleSeparately.Contains(level))
                    continue;

                var possibleLoot = lootLevels.Where(l => l.Key <= levelDifficulty[level]).SelectMany(l => l.Value).Except(itemsPlaced);

                var lootInRoom = 0;
                while (lootInRoom < lootPerRoomInt)
                {
                    if (!possibleLoot.Any())
                        break;

                    var lootToPlace = possibleLoot.RandomElement();

                    Builder.PlaceItems(mapState, new List<Item> { lootToPlace }, new List<int> { room }, false);
                    LogFile.Log.LogEntryDebug("Placing item: " + lootToPlace.SingleItemDescription + " on level " + mapState.LevelNames[level], LogDebugLevel.Medium);

                    itemsPlaced.Add(lootToPlace);
                    itemsInArmory[level].Add(lootToPlace);
                    lootInRoom++;
                    lootPlaced++;
                }

                roomsDone++;

                //If we are below our quota
                var behindLoot = (int)Math.Floor(roomsDone * lootPerRoom - lootPlaced);

                var behindLootPlaced = 0;
                while (behindLootPlaced < behindLoot)
                {
                    if (!possibleLoot.Any())
                        break;

                    var lootToPlace = possibleLoot.RandomElement();

                    Builder.PlaceItems(mapState, new List<Item> { lootToPlace }, new List<int> { room }, false);
                    LogFile.Log.LogEntryDebug("Placing item (catchup): " + lootToPlace.SingleItemDescription + " on level " + mapState.LevelNames[level], LogDebugLevel.Medium);

                    itemsPlaced.Add(lootToPlace);
                    itemsInArmory[level].Add(lootToPlace);
                    lootPlaced++;
                    behindLootPlaced++;
                }
            }


            //If we have loot remaining
            if (lootPlaced < totalLoot)
            {
                var possibleLoot = lootLevels.SelectMany(l => l.Value).Except(itemsPlaced);

                //Place at random
                foreach (var i in possibleLoot)
                {
                    var randomRoom = goodyRooms.RandomElement();
                    Builder.PlaceItems(mapState, new List<Item> { i }, new List<int> { randomRoom.Value }, false);
                    itemsPlaced.Add(i);
                    itemsInArmory[randomRoom.Key].Add(i);
                    lootPlaced++;
                    LogFile.Log.LogEntryDebug("Placing item (final): " + i.SingleItemDescription + " on level " + mapState.LevelNames[randomRoom.Key], LogDebugLevel.Medium);
                }
            }

            LogFile.Log.LogEntryDebug("Total items placed  " + itemsPlaced.Count() + " of " + lootLevels.SelectMany(kv => kv.Value).Count(), LogDebugLevel.Medium);


        }

        private void AddStandardLootToArmory(MapState mapState, QuestMapBuilder builder)
        {
            var levelDifficulty = mapState.LevelDifficulty;

            foreach (var kv in goodyRooms.OrderBy(k => k.Key))
            {
                var level = kv.Key;
                var room = kv.Value;

                var randomMedKits = ProduceMultipleItems<RogueBasin.Items.NanoRepair>(1);
                builder.PlaceItems(mapState, randomMedKits, new List<int> { room }, false);

                var totalGrenades = Game.Random.Next(1, 1 + 2 * levelDifficulty[level]);

                var totalExposiveGrenades = totalGrenades / 2;
                var totalStunGrenades = Game.Random.Next(totalGrenades - totalExposiveGrenades);
                var totalSoundGrenades = totalGrenades - totalExposiveGrenades - totalStunGrenades;

                var maxNadesOfType = Math.Max(1, (int)Math.Ceiling(levelDifficulty[level] / 2.0));
                var fragGrenades = ProduceMultipleItems<RogueBasin.Items.FragGrenade>(Game.Random.Next(1, maxNadesOfType));
                var stunGrenades = ProduceMultipleItems<RogueBasin.Items.StunGrenade>(Game.Random.Next(1, maxNadesOfType));
                var soundGrenades = ProduceMultipleItems<RogueBasin.Items.SoundGrenade>(Game.Random.Next(1, maxNadesOfType));

                builder.PlaceItems(mapState, fragGrenades, new List<int> { room }, false);
                builder.PlaceItems(mapState, stunGrenades, new List<int> { room }, false);
                builder.PlaceItems(mapState, soundGrenades, new List<int> { room }, false);
            }

        }

        private List<Item> ProduceMultipleItems<T>(int count) where T : Item, new()
        {

            List<Item> toReturn = new List<Item>();
            for (int i = 0; i < count; i++)
            {
                toReturn.Add(new T());
            }

            return toReturn;
        }
        
        /// <summary>
        /// This shouldn't be in the quest, really
        /// </summary>
        /// <param name="level1Ware"></param>
        /// <returns></returns>
        private IEnumerable<Item> PlayerInitialItems(List<Item> level1Ware)
        {
            var itemsGiven = new List<Item>();

            var player = Game.Dungeon.Player;
            player.GiveItemNotFromDungeon(new RogueBasin.Items.Fists());
            player.GiveItemNotFromDungeon(new RogueBasin.Items.Pistol());

            var level1WareToGive = level1Ware.RandomElement();

            itemsGiven.Add(level1WareToGive);

            player.GiveItemNotFromDungeon(level1WareToGive);

            return itemsGiven;
        }
        
    }
}
