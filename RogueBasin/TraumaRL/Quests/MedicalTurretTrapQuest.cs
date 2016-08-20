using GraphMap;
using RogueBasin;
using RogueBasin.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL.Quests
{
    class MedicalTurretTrapQuest : Quest
    {
        public MedicalTurretTrapQuest(MapState mapState, QuestMapBuilder builder, LogGenerator logGen)
            : base(mapState, builder, logGen)
        {

        }

        public override void SetupQuest()
        {
            var medicalLevel = MapState.LevelIds["medical"];
            var lowerAtriumLevel = MapState.LevelIds["lowerAtrium"];

            //LOCKED DOOR

            //Lock the door to the next level elevator with a key card which is stashed in a trap room full of turrets
            //Make sure a stun grenade is accessible before the door
        
            //Lock the door off the level
            var elevatorConnection = MapState.LevelInfo[medicalLevel].ConnectionsToOtherLevels.First().Value;
            var doorId = "medical-turret-security";

            var numCluesRequiredToOpen = 1;
            Builder.PlaceMovieDoorOnMap(MapState, doorId, doorId, numCluesRequiredToOpen, System.Drawing.Color.Red, "t_medicalturretsecurityunlocked", "t_medicalturretsecuritylocked", elevatorConnection);

            //TRAP/KEY ROOM

            //Find a room where the key is allowed to exist, from which we can grow off the turret room

            var allowedRoomsForCluesReducedMap = MapState.DoorAndClueManager.GetValidRoomsToPlaceClueForDoor(doorId);
            var allowedRoomsForCluesFullMap = MapState.MapInfo.RoomsInFullMapFromRoomsInCollapsedCycles(allowedRoomsForCluesReducedMap);

            allowedRoomsForCluesFullMap = MapState.MapInfo.FilterOutCorridors(allowedRoomsForCluesFullMap);

            var turretRoomConnection = AddTurretRoomToMap(medicalLevel, allowedRoomsForCluesFullMap);
            var turretRoom = turretRoomConnection.Target;

            //Using medicalLevel would be fine, but this lookup is slightly more general, if we refactor out this code in future
            var turretLevel = MapState.MapInfo.GetLevelForRoomIndex(turretRoom);

            //Note mapState is rebuilt after adding new room
            var manager = MapState.DoorAndClueManager;
            var mapInfo = MapState.MapInfo;

            //Turret room is guaranteed not to be in a cycle (at this stage), so turretRoom is guaranteed to be in the reduced map
            
            var turrentRoomKeycardClue = manager.AddCluesToExistingDoor(doorId, Enumerable.Repeat(turretRoom, 1)).ElementAt(0);
          
            //Add Keycard

            var colorForKeycard = Builder.GetUnusedColor();
            var nameForKeycard = colorForKeycard.Item2 + " key card";
            var keycardItem = new RogueBasin.Items.Clue(turrentRoomKeycardClue, colorForKeycard.Item1, nameForKeycard);
            var keycardLocation = MapState.MapInfo.Room(turretRoom).FeatureMarkerPoints("key").First();

            mapInfo.Populator.AddItemToRoom(keycardItem, turretRoom, new Location(turretLevel, keycardLocation));
            
            //Add features (including concealed turrets)
            var turretOrDecorationLocations = MapState.MapInfo.Room(turretRoom).FeatureMarkerPoints("turret");
            var concealedTurretFeatures = new List<Feature>();
            var turrets = new Dictionary<Location, Monster>();
            foreach (var turretLoc in turretOrDecorationLocations)
            {
                var sqPC = DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.SquarePC].NewFeature();
                mapInfo.Populator.AddFeatureToRoom(mapInfo, turretRoom, turretLoc, sqPC);

                if (Game.Random.Next(100) < 50)
                {
                    concealedTurretFeatures.Add(sqPC);
                    var turret = new RogueBasin.Creatures.RotatingTurret();
                    turrets.Add(new Location(turretLevel, turretLoc), turret);
                }
            }

            //Add trigger
            var keycardTrigger = new FeaturesToCreaturesTrigger(concealedTurretFeatures, turrets);
            mapInfo.Populator.AddTriggerToRoom(keycardTrigger, turretRoom, new Location(turretLevel, keycardLocation));

            //GRENADE ROOM

            //Add grenade (helper item) elsewhere on the level (in an existing room)

            var grenadeClueRoom = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(MapState, 1, allowedRoomsForCluesReducedMap).ElementAt(0);
            var grenadeClue = manager.AddCluesToExistingDoor(doorId, Enumerable.Repeat(grenadeClueRoom, 1)).ElementAt(0);
            var grenadeItem = new RogueBasin.Items.StunGrenade();
            //since this is not a simple clue item, we need to manually set it as associated with this quest to appear in the drawn graph
            grenadeItem.QuestId = "grenade " + "(" + doorId + ")";
            var grenade = new Tuple<Clue, Item>(grenadeClue, grenadeItem);

            Builder.PlaceClueItems(MapState, Enumerable.Repeat(grenade, 1), false, true, false);

            //Place log entry warning about the turret room
            var criticalPath = MapState.MapInfo.Model.GetPathBetweenVerticesInFullMap(MapState.MapInfo.StartRoom, turretRoom);
            var roomsOnLevel = MapState.MapInfo.GetRoomIndicesForLevel(medicalLevel);

            //All rooms will be on the medical level, so we could make the list of rooms from the connections
            var roomsOnCriticalPath = Builder.FilterRoomsByPath(MapState, roomsOnLevel, criticalPath, true, QuestMapBuilder.CluePath.OnCriticalPath, true);

            //Note that roomsOnCriticalPath doesn't seem to take into account loops, so you can miss the critical path (i.e. a loop room will be in the critical path
            //and then the clue is placed in a random room within this)
            var roomToPlaceLog = roomsOnCriticalPath.Skip(1).Take(roomsOnCriticalPath.Count() / 2).RandomElement();
            var logClue = manager.AddCluesToExistingDoor(doorId, Enumerable.Repeat(roomToPlaceLog, 1)).ElementAt(0);
            var turretLog = new Tuple<LogEntry, Clue>(LogGen.GenerateArbitaryLogEntry("qe_medicalturretsecurity"), logClue);
            
            //Place log entry foreshadowing the next level
            var roomForLevelLog = Builder.PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(MapState, 1, allowedRoomsForCluesReducedMap).ElementAt(0);
            var levelLogClue = manager.AddCluesToExistingDoor(doorId, Enumerable.Repeat(roomForLevelLog, 1)).ElementAt(0);
            var levelLog = new Tuple<LogEntry, Clue>(LogGen.GenerateElevatorLogEntry(MapState, medicalLevel, lowerAtriumLevel), levelLogClue);
            
            Builder.PlaceLogClues(MapState, new List<Tuple<LogEntry, Clue>> { levelLog, turretLog }, true, true);
        }
                
        private Connection AddTurretRoomToMap(int level, IEnumerable<int> allowedRoomsFullMap)
        {
            RoomTemplate roomTemplate = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.turret_trap1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            var doorIndex = 0; //align the top door of the template

            var levelGenerator = MapState.LevelInfo[level].LevelGenerator;
            var sourceDoorsInAllowedRooms = levelGenerator.PotentialDoors.Where(d => allowedRoomsFullMap.Contains(d.OwnerRoomIndex)).Shuffle();

            Connection placedRoomConnection = null;

            foreach (var door in sourceDoorsInAllowedRooms)
            {
                try
                {
                    placedRoomConnection = levelGenerator.PlaceRoomTemplateAlignedWithExistingDoor(roomTemplate, door, doorIndex);
                    break;
                }
                catch (ApplicationException) { }
            }

            if (placedRoomConnection == null)
            {
                throw new ApplicationException("Unable to place a room for MedicalTurretTrapQuest.");
            }

            MapState.RefreshLevelMaps();

            return placedRoomConnection;
        }
    }
}
