using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraumaRL
{
    public partial class TraumaWorldGenerator
    {
        
        Dictionary<int, int> levelDifficulty;

        private void CalculateLevelDifficulty()
        {
            var levelsToHandleSeparately = new List<int> { medicalLevel, arcologyLevel, computerCoreLevel, bridgeLevel };

            levelDifficulty = new Dictionary<int, int>(levelDepths);
            levelDifficulty[reactorLevel] = 4;
            levelDifficulty[arcologyLevel] = 4;
            levelDifficulty[computerCoreLevel] = 5;
            levelDifficulty[bridgeLevel] = 5;

        }

        Dictionary<int, List<Item>> itemsInArmory;

        enum Ware
        {
            Boost,
            Shield,
            Aim,
            Stealth
        }

        private void PlaceLootInArmory(MapInfo mapInfo, Dictionary<int, LevelInfo> levelInfo)
        {
            //Add standard loot
            AddStandardLootToArmory(mapInfo);

            //Weapons and wetware

            itemsInArmory = new Dictionary<int, List<Item>>();

            foreach(var l in mapState.GameLevels) {
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
                if(!wareInGame.Contains(ware))
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
            PlaceItems(mapInfo, new List<Item> { randomWare }, new List<int> { goodyRooms[medicalLevel] }, false);
            itemsPlaced.Add(randomWare);
            itemsInArmory[0].Add(randomWare);

            PlaceItems(mapInfo, new List<Item> { lootLevels[0][0] }, new List<int> { goodyRooms[medicalLevel] }, false);
            itemsPlaced.Add(lootLevels[0][0]);
            itemsInArmory[0].Add(lootLevels[0][0]);
            PlaceItems(mapInfo, new List<Item> { lootLevels[0][1] }, new List<int> { goodyRooms[medicalLevel] }, false);
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

                    PlaceItems(mapInfo, new List<Item> { lootToPlace }, new List<int> { room }, false);
                    LogFile.Log.LogEntryDebug("Placing item: " + lootToPlace.SingleItemDescription + " on level " + Game.Dungeon.DungeonInfo.LevelNaming[level], LogDebugLevel.Medium);

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

                    PlaceItems(mapInfo, new List<Item> { lootToPlace }, new List<int> { room }, false);
                    LogFile.Log.LogEntryDebug("Placing item (catchup): " + lootToPlace.SingleItemDescription + " on level " + Game.Dungeon.DungeonInfo.LevelNaming[level], LogDebugLevel.Medium);

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
                    PlaceItems(mapInfo, new List<Item> { i }, new List<int> { randomRoom.Value }, false);
                    itemsPlaced.Add(i);
                    itemsInArmory[randomRoom.Key].Add(i);
                    lootPlaced++;
                    LogFile.Log.LogEntryDebug("Placing item (final): " + i.SingleItemDescription + " on level " + Game.Dungeon.DungeonInfo.LevelNaming[randomRoom.Key], LogDebugLevel.Medium);
                }
            }

            LogFile.Log.LogEntryDebug("Total items placed  " + itemsPlaced.Count() + " of " + lootLevels.SelectMany(kv => kv.Value).Count(), LogDebugLevel.Medium);

            
        }

        private void AddStandardLootToArmory(MapInfo mapInfo)
        {
            foreach (var kv in goodyRooms.OrderBy(k => k.Key))
            {
                var level = kv.Key;
                var room = kv.Value;

                var randomMedKits = ProduceMultipleItems<RogueBasin.Items.NanoRepair>(1);
                PlaceItems(mapInfo, randomMedKits, new List<int> { room }, false);

                var totalGrenades = Game.Random.Next(1, 1 + 2 * levelDifficulty[level]);

                var totalExposiveGrenades = totalGrenades / 2;
                var totalStunGrenades = Game.Random.Next(totalGrenades - totalExposiveGrenades);
                var totalSoundGrenades = totalGrenades - totalExposiveGrenades - totalStunGrenades;

                var maxNadesOfType = Math.Max(1, (int)Math.Ceiling(levelDifficulty[level] / 2.0));
                var fragGrenades = ProduceMultipleItems<RogueBasin.Items.FragGrenade>(Game.Random.Next(1, maxNadesOfType));
                var stunGrenades = ProduceMultipleItems<RogueBasin.Items.StunGrenade>(Game.Random.Next(1, maxNadesOfType));
                var soundGrenades = ProduceMultipleItems<RogueBasin.Items.SoundGrenade>(Game.Random.Next(1, maxNadesOfType));

                PlaceItems(mapInfo, fragGrenades, new List<int> { room }, false);
                PlaceItems(mapInfo, stunGrenades, new List<int> { room }, false);
                PlaceItems(mapInfo, soundGrenades, new List<int> { room }, false);
            }

        }

        private List<Item> ProduceMultipleItems<T>(int count) where T : Item, new() {

            List<Item> toReturn = new List<Item>();
            for(int i=0;i<count;i++) {
                toReturn.Add(new T());
            }

            return toReturn;
        }

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
