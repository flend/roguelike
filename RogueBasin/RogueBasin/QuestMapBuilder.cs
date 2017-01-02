using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// TODO: Use relative to room co-ordinates for all placing and utility functions
    /// </summary>
    public class QuestMapBuilder
    {
        private static List<Tuple<System.Drawing.Color, string>> availableColors = new List<Tuple<System.Drawing.Color, string>> {
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Red, "red"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Coral, "coral"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Blue, "blue"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Orange, "orange"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Yellow, "yellow"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Khaki, "khaki"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Chartreuse, "chartreuse"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.HotPink, "hot pink"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Cyan, "cyan"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Lime, "lime"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Navy, "navy"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Tan, "tan"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Fuchsia, "fuchsia"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.GhostWhite, "ghost"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Teal, "teal"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Plum, "plum"),
                new Tuple<System.Drawing.Color, string>(System.Drawing.Color.Plum, "wheat") };

        private List<Tuple<System.Drawing.Color, string>> usedColors = new List<Tuple<System.Drawing.Color, string>>();

        public RoomPoint PlaceFeatureInRoom(MapState mapState, Feature objectiveFeature, IEnumerable<int> candidateRooms, bool preferBoundaries)
        {
            var mapInfo = mapState.MapInfo;

            var roomPoints = mapInfo.GetAllUnoccupiedRoomPoints(candidateRooms, preferBoundaries);

            if (!roomPoints.Any())
            {
                throw new ApplicationException("Unable to place feature " + objectiveFeature.Description);
            }

            var roomPoint = roomPoints.First();

            bool success = mapInfo.Populator.AddFeatureToRoom(mapInfo, roomPoint.roomId, roomPoint.ToRelativePoint(mapInfo), objectiveFeature);

            if (!success)
            {
                throw new ApplicationException("Unable to place feature " + objectiveFeature.Description + " in room " + roomPoint.roomId);
            }

            return roomPoint;
        }

        public void AddStandardDecorativeFeaturesToRoom(MapState mapState, int roomId, int featuresToPlace, IEnumerable<Tuple<int, DecorationFeatureDetails.Decoration>> decorationDetails, bool useBoundary)
        {
            var mapInfo = mapState.MapInfo;
            var floorPoints = new List<RogueBasin.Point>();
            var thisRoom = mapInfo.Room(roomId);

            if (!useBoundary)
                floorPoints = RoomTemplateUtilities.GetPointsInRoomWithTerrain(thisRoom.Room, RoomTemplateTerrain.Floor);
            else
                floorPoints = RoomTemplateUtilities.GetBoundaryFloorPointsInRoom(thisRoom.Room);

            var floorPointsToUse = floorPoints.Shuffle().Take(featuresToPlace);

            AddStandardDecorativeFeaturesToRoom(mapInfo, roomId, floorPointsToUse, decorationDetails);
        }

        public void AddStandardDecorativeFeaturesToRoomUsingGrid(MapState mapState, int roomId, int featuresToPlace, IEnumerable<Tuple<int, DecorationFeatureDetails.Decoration>> decorationDetails)
        {
            var mapInfo = mapState.MapInfo;

            var thisRoom = mapInfo.Room(roomId);
            var floorPoints = RoomTemplateUtilities.GetGridFromRoom(thisRoom.Room, 2, 1, 0.5);
            var floorPointsToUse = floorPoints.Shuffle().Take(featuresToPlace);
            AddStandardDecorativeFeaturesToRoom(mapInfo, roomId, floorPointsToUse, decorationDetails);
        }

        private void AddStandardDecorativeFeaturesToRoom(MapInfo mapInfo, int roomId, IEnumerable<RogueBasin.Point> points, IEnumerable<Tuple<int, DecorationFeatureDetails.Decoration>> decorationDetails)
        {
            if (points.Count() == 0)
                return;

            var featuresObjectsDetails = points.Select(p => new Tuple<RogueBasin.Point, DecorationFeatureDetails.Decoration>
                (p, Utility.ChooseItemFromWeights<DecorationFeatureDetails.Decoration>(decorationDetails)));
            var featureObjectsToPlace = featuresObjectsDetails.Select(dt => new Tuple<RogueBasin.Point, Feature>
                (dt.Item1, new RogueBasin.Features.StandardDecorativeFeature(dt.Item2.representation, dt.Item2.colour, dt.Item2.isBlocking)));

            var featuresPlaced = mapInfo.Populator.AddFeaturesToRoom(mapInfo, roomId, featureObjectsToPlace);
            LogFile.Log.LogEntryDebug("Placed " + featuresPlaced + " standard decorative features in room " + roomId, LogDebugLevel.Medium);
        }


        public Door PlaceMovieDoorOnMap(MapState mapState, string doorId, string doorName, int numberOfCluesForDoor, System.Drawing.Color colorToUse, string openMovie, string cantOpenMovie, Connection criticalConnectionForDoor)
        {
            var door = PlaceLockedDoorInManager(mapState, doorId, numberOfCluesForDoor, criticalConnectionForDoor);

            var lockedDoor = new RogueBasin.Locks.SimpleLockedDoorWithMovie(door, openMovie, cantOpenMovie, doorName, colorToUse);

            PlaceLockedDoorOnMap(mapState, lockedDoor, door);

            return door;
        }

        private Door PlaceLockedDoorInManager(MapState mapState, string doorId, int numberOfCluesForDoor, Connection criticalConnectionForDoor)
        {
            var manager = mapState.DoorAndClueManager;

            manager.PlaceDoor(new DoorRequirements(criticalConnectionForDoor, doorId, numberOfCluesForDoor));
            var door = manager.GetDoorById(doorId);
            return door;
        }

        public void PlaceLockedDoorOnMap(MapState mapState, Lock lockedDoor, Door door)
        {
            var doorInfo = mapState.MapInfo.GetDoorForConnection(door.DoorConnectionFullMap);
            lockedDoor.LocationLevel = doorInfo.LevelNo;
            lockedDoor.LocationMap = doorInfo.MapLocation;

            mapState.MapInfo.Populator.GetDoorInfo(door.Id).AddLock(lockedDoor);
        }


        public Tuple<System.Drawing.Color, string> GetUnusedColor()
        {
            var unusedColor = availableColors.Except(usedColors);
            var colorToReturn = availableColors.RandomElement();

            if (unusedColor.Count() > 0)
                colorToReturn = unusedColor.RandomElement();

            usedColors.Add(colorToReturn);

            return colorToReturn;
        }

        public RoomPoint PlaceObjective(MapState mapState, Objective obj, Feature objectiveFeature, bool avoidCorridors, bool includeVaults, bool preferBoundaries)
        {
            var candidateRooms = GetPossibleRoomsForObjective(mapState, obj, avoidCorridors, includeVaults);
            return PlaceFeatureInRoom(mapState, objectiveFeature, candidateRooms, preferBoundaries);
        }

        private IEnumerable<int> GetPossibleRoomsForObjective(MapState mapState, Objective clue, bool filterCorridors, bool includeVaults)
        {
            var possibleRooms = clue.PossibleClueRoomsInFullMap;
            IEnumerable<int> initialRooms = possibleRooms;
            if (!includeVaults)
                initialRooms = possibleRooms.Except(mapState.AllReplaceableVaults);
            var candidateRooms = initialRooms;
            if (filterCorridors)
                candidateRooms = mapState.MapInfo.FilterOutCorridors(initialRooms);
            if (candidateRooms.Count() == 0)
                candidateRooms = initialRooms;
            return candidateRooms;
        }

        public void UseVault(MapState mapState, Connection vaultConnection)
        {
            var levelInfo = mapState.LevelGraph.LevelInfo;
            var levelForVault = mapState.LevelGraph.GameLevels.Where(l => levelInfo[l].ReplaceableVaultConnections.Contains(vaultConnection)).First();

            levelInfo[levelForVault].ReplaceableVaultConnectionsUsed.Add(vaultConnection);
        }

        private IEnumerable<Connection> GetAllUsedVaults(MapState mapState)
        {
            return mapState.LevelGraph.GameLevels.SelectMany(l => mapState.LevelGraph.LevelInfo[l].ReplaceableVaultConnectionsUsed);
        }

        public IEnumerable<Connection> GetAllAvailableVaults(MapState mapState)
        {
            return GetAllVaults(mapState).Except(GetAllUsedVaults(mapState));
        }

        public IEnumerable<Connection> GetAllVaults(MapState mapState)
        {
            return mapState.LevelGraph.GameLevels.SelectMany(l => mapState.LevelGraph.LevelInfo[l].ReplaceableVaultConnections);
        }

        public enum CluePath
        {
            OnCriticalPath, NotOnCriticalPath, Any
        }

        public IEnumerable<int> FilterRoomsByPath(MapState mapState, IEnumerable<int> allCandidateRooms, IEnumerable<Connection> criticalPath, bool enforceClueOnDestLevel, CluePath clueCriticalPath, bool clueNotInCorridors, bool excludeVaults = true)
        {
            var mapInfo = mapState.MapInfo;
            var candidateRooms = allCandidateRooms;
            if(excludeVaults) 
                candidateRooms = candidateRooms.Except(mapState.AllReplaceableVaults);

            if (enforceClueOnDestLevel)
                candidateRooms = candidateRooms.Intersect(mapInfo.GetRoomIndicesForLevel(mapInfo.GetLevelForRoomIndex(criticalPath.Last().Target)));

            var preferredRooms = candidateRooms;
            if (clueCriticalPath == CluePath.NotOnCriticalPath)
            {
                preferredRooms = candidateRooms.Except(criticalPath.SelectMany(c => new List<int> { c.Source, c.Target }));
                if (preferredRooms.Count() == 0)
                    preferredRooms = candidateRooms;
            }
            else if (clueCriticalPath == CluePath.OnCriticalPath)
            {
                preferredRooms = candidateRooms.Intersect(criticalPath.SelectMany(c => new List<int> { c.Source, c.Target }));
                if (preferredRooms.Count() == 0)
                    preferredRooms = candidateRooms;
            }

            var preferredRoomsIncludingType = preferredRooms;
            if (clueNotInCorridors)
            {
                preferredRoomsIncludingType = mapInfo.FilterOutCorridors(preferredRooms);
                if (preferredRoomsIncludingType.Count() == 0)
                    preferredRoomsIncludingType = preferredRooms;
            }

            return preferredRoomsIncludingType;
        }

        public List<int> PickClueRoomsFromReducedRoomsListUsingFullMapWeighting(MapState mapState, int cluesToPlace, IEnumerable<int> allowedRoomsForClues)
        {
            if (allowedRoomsForClues.Count() == 0)
                throw new ApplicationException("Not enough rooms to place clues");

            //To get an even distribution we need to take into account how many nodes are in each group node
            var expandedAllowedRoomForClues = mapState.MapInfo.RepeatRoomNodesByNumberOfRoomsInCollapsedCycles(allowedRoomsForClues.Except(mapState.AllReplaceableVaults));

            if (expandedAllowedRoomForClues.Count() == 0)
                throw new ApplicationException("No allowed rooms for clues.");

            var roomsToPlaceClues = new List<int>();

            //Can reuse rooms if number of rooms < cluesToPlace
            while (roomsToPlaceClues.Count() < cluesToPlace)
            {
                roomsToPlaceClues.AddRange(expandedAllowedRoomForClues.Shuffle());
            }

            return roomsToPlaceClues.GetRange(0, cluesToPlace);
        }


        public void PlaceCreatureClues<T>(MapState mapState, List<Clue> monsterCluesToPlace, bool autoPickup, bool includeVaults, bool preferBoundaries) where T : Monster, new()
        {
            var mapInfo = mapState.MapInfo;

            foreach (var clue in monsterCluesToPlace)
            {
                var candidateRooms = GetPossibleRoomsForClues(mapState, clue, true, includeVaults);
                var pointsForClues = mapInfo.GetAllUnoccupiedRoomPoints(candidateRooms, preferBoundaries);

                var newMonster = new T();
                Item clueItem;
                if (autoPickup)
                    clueItem = new RogueBasin.Items.ClueAutoPickup(clue);
                else
                    clueItem = new RogueBasin.Items.Clue(clue);

                newMonster.PickUpItem(clueItem);

                if (!pointsForClues.Any())
                {
                    throw new ApplicationException("Nowhere to place clue monster " + newMonster.SingleDescription);
                }

                var pointToPlaceClue = pointsForClues.Shuffle().First();

                mapInfo.Populator.AddMonsterToRoom(newMonster, pointToPlaceClue.roomId, pointToPlaceClue.ToRelativePoint(mapState.MapInfo));
            }
        }

        public void PlaceCreaturesInRoom(MapState mapState, int level, int roomId, List<Monster> monstersToPlace, bool preferBoundaries)
        {
            var mapInfo = mapState.MapInfo;
            var pointsForClues = mapInfo.GetAllUnoccupiedRoomPoints(roomId, preferBoundaries);

            var shuffledPoints = pointsForClues.Shuffle();

            if (shuffledPoints.Count() < monstersToPlace.Count())
            {
                throw new ApplicationException("Nowhere to place clue monsters including: " + monstersToPlace.ElementAt(0).SingleDescription);
            }

            var monstersAndPoints = shuffledPoints.Zip(monstersToPlace, Tuple.Create);
            foreach (var m in monstersAndPoints)
            {
                mapInfo.Populator.AddMonsterToRoom(m.Item2, m.Item1.roomId, m.Item1.ToRelativePoint(mapState.MapInfo));
            }
        }

        public void PlaceLogClues(MapState mapState, List<Tuple<LogEntry, Clue>> logCluesToPlace, bool boundariesPreferred, bool cluesNotInCorridors)
        {
            var clueItems = logCluesToPlace.Select(t => new Tuple<Clue, Item>(t.Item2, new RogueBasin.Items.Log(t.Item1, t.Item2.GetLockedItemId())));
            PlaceClueItems(mapState, clueItems, boundariesPreferred, cluesNotInCorridors, false);
        }


        public IEnumerable<RoomPoint> PlaceClueItems(MapState mapState, IEnumerable<Tuple<Clue, Item>> clueItems, bool boundariesPreferred, bool avoidCorridors, bool includeVaults)
        {
            var mapInfo = mapState.MapInfo;
            List<RoomPoint> pointsPlaced = new List<RoomPoint>();

            foreach (var clueItem in clueItems)
            {
                var clue = clueItem.Item1;
                var itemToPlace = clueItem.Item2;

                var candidateRooms = GetPossibleRoomsForClues(mapState, clue, true, includeVaults);
                var pointsForClues = mapInfo.GetAllUnoccupiedRoomPoints(candidateRooms, boundariesPreferred);

                if (!pointsForClues.Any())
                {
                    throw new ApplicationException("Nowhere to place clue item: " + itemToPlace.SingleItemDescription);
                }

                var pointToPlaceClue = pointsForClues.Shuffle().First();
                mapInfo.Populator.AddItemToRoom(itemToPlace, pointToPlaceClue.roomId, pointToPlaceClue.ToRelativePoint(mapState.MapInfo));
                pointsPlaced.Add(pointToPlaceClue);
            }

            return pointsPlaced;
        }


        private IEnumerable<int> GetPossibleRoomsForClues(MapState mapState, Clue clue, bool filterCorridors, bool includeVaults)
        {
            var possibleRooms = clue.PossibleClueRoomsInFullMap;
            IEnumerable<int> initialRooms = possibleRooms;
            if (!includeVaults)
                initialRooms = possibleRooms.Except(mapState.AllReplaceableVaults);
            var candidateRooms = initialRooms;
            if (filterCorridors)
                candidateRooms = mapState.MapInfo.FilterOutCorridors(initialRooms);
            if (candidateRooms.Count() == 0)
                candidateRooms = initialRooms;
            return candidateRooms;
        }

        public IEnumerable<RoomPoint> PlaceSimpleClueItem(MapState mapState, Tuple<Clue, System.Drawing.Color, string> clues, bool avoidCorridors, bool includeVaults)
        {
            return PlaceSimpleClueItems(mapState, new List<Tuple<Clue, System.Drawing.Color, string>> { clues }, avoidCorridors, includeVaults);
        }

        public IEnumerable<RoomPoint> PlaceSimpleClueItems(MapState mapState, IEnumerable<Tuple<Clue, System.Drawing.Color, string>> clues, bool avoidCorridors, bool includeVaults)
        {
            var simpleClueItems = clues.Select(c => new Tuple<Clue, Item>(c.Item1, new RogueBasin.Items.Clue(c.Item1, c.Item2, c.Item3)));
            return PlaceClueItems(mapState, simpleClueItems, false, avoidCorridors, includeVaults);
        }


        public Door PlaceLockedDoorOnMap(MapState mapState, string doorId, string doorName, int numberOfCluesForDoor, System.Drawing.Color colorToUse, Connection criticalConnectionForDoor)
        {
            var door = PlaceLockedDoorInManager(mapState, doorId, numberOfCluesForDoor, criticalConnectionForDoor);

            var lockedDoor = new RogueBasin.Locks.SimpleLockedDoor(door, doorName, colorToUse);

            PlaceLockedDoorOnMap(mapState, lockedDoor, door);

            return door;
        }


        public int PlaceClueForDoorInVault(MapState mapState, Dictionary<int, LevelInfo> levelInfo, string doorId, System.Drawing.Color clueColour, string clueName, IEnumerable<int> idealLevelsForClue)
        {
            var manager = mapState.DoorAndClueManager;
            var mapInfo = mapState.MapInfo;

            var possibleRoomsForClue = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            var possibleVaultsForClue = possibleRoomsForClue.Intersect(GetAllAvailableVaults(mapState).Select(c => c.Target));

            var roomsOnRequestedLevels = mapInfo.FilterRoomsByLevel(possibleVaultsForClue, idealLevelsForClue);

            if (!roomsOnRequestedLevels.Any())
                roomsOnRequestedLevels = possibleVaultsForClue;

            var clueRoom = roomsOnRequestedLevels.RandomElement();

            var clueRoomConnection = GetAllVaults(mapState).Where(c => c.Target == clueRoom).First();
            var clueRoomLevel = mapInfo.GetLevelForRoomIndex(clueRoom);

            UseVault(mapState, clueRoomConnection);

            var newClue = mapState.DoorAndClueManager.AddCluesToExistingDoor(doorId, new List<int> { clueRoom }).First();
            PlaceSimpleClueItem(mapState, new Tuple<Clue, System.Drawing.Color, string>(newClue, clueColour, clueName), true, true);

            LogFile.Log.LogEntryDebug("Placing " + clueName + " on level " + clueRoomLevel + " in vault " + clueRoom, LogDebugLevel.Medium);

            return clueRoom;
        }

        //Refactor with above method?
        public int PlaceClueItemForDoorInVault(MapState mapState, Dictionary<int, LevelInfo> levelInfo, string doorId, Item itemToPlace, string clueName, IEnumerable<int> idealLevelsForClue)
        {
            var manager = mapState.DoorAndClueManager;
            var mapInfo = mapState.MapInfo;

            var possibleRoomsForCaptainsId = manager.GetValidRoomsToPlaceClueForDoor(doorId);
            var possibleVaultsForCaptainsId = possibleRoomsForCaptainsId.Intersect(GetAllAvailableVaults(mapState).Select(c => c.Target));

            var roomsOnRequestedLevels = mapInfo.FilterRoomsByLevel(possibleVaultsForCaptainsId, idealLevelsForClue);

            if (!roomsOnRequestedLevels.Any())
                roomsOnRequestedLevels = possibleVaultsForCaptainsId;

            // var captainIdRoomsInDistanceOrderFromStart = RoomsInDescendingDistanceFromSource(mapInfo, mapInfo.StartRoom, roomsOnRequestedLevels);
            // var captainIdRoom = captainIdRoomsInDistanceOrderFromStart.ElementAt(0);
            //above is not performing, since it always sticks everything in level 8 as far away from everything as it can
            var captainIdRoom = roomsOnRequestedLevels.RandomElement();

            var captainsIdConnection = GetAllVaults(mapState).Where(c => c.Target == captainIdRoom).First();
            var captainsIdLevel = mapInfo.GetLevelForRoomIndex(captainIdRoom);

            UseVault(mapState, captainsIdConnection);

            var captainIdClue = mapState.DoorAndClueManager.AddCluesToExistingDoor(doorId, new List<int> { captainIdRoom }).First();

            PlaceItems(mapState, new List<Item> { itemToPlace }, new List<int> { captainIdRoom }, true);

            LogFile.Log.LogEntryDebug("Placing " + clueName + " on level " + captainsIdLevel + " in vault " + captainIdRoom, LogDebugLevel.Medium);

            return captainIdRoom;
        }

        //Refactor with above method
        public int PlaceMovieClueForObjectiveInVault(MapState mapState, string objectiveId, char representation, string pickupMovie, string description, IEnumerable<int> idealLevelsForClue)
        {
            var manager = mapState.DoorAndClueManager;
            var levelInfo = mapState.LevelGraph.LevelInfo;
            var mapInfo = mapState.MapInfo;

            var possibleRoomsForCaptainsId = manager.GetValidRoomsToPlaceClueForObjective(objectiveId);
            var possibleVaultsForCaptainsId = possibleRoomsForCaptainsId.Intersect(GetAllAvailableVaults(mapState).Select(c => c.Target));

            var roomsOnRequestedLevels = mapInfo.FilterRoomsByLevel(possibleVaultsForCaptainsId, idealLevelsForClue);

            if (!roomsOnRequestedLevels.Any())
                roomsOnRequestedLevels = possibleVaultsForCaptainsId;

            // var captainIdRoomsInDistanceOrderFromStart = RoomsInDescendingDistanceFromSource(mapInfo, mapInfo.StartRoom, roomsOnRequestedLevels);
            // var captainIdRoom = captainIdRoomsInDistanceOrderFromStart.ElementAt(0);            //above is not performing, since it always sticks everything in level 8 as far away from everything as it can
            var captainIdRoom = roomsOnRequestedLevels.RandomElement();

            var captainsIdConnection = GetAllVaults(mapState).Where(c => c.Target == captainIdRoom).First();
            var captainsIdLevel = mapInfo.GetLevelForRoomIndex(captainIdRoom);

            UseVault(mapState, captainsIdConnection);

            var captainIdClue = mapState.DoorAndClueManager.AddCluesToExistingObjective(objectiveId, new List<int> { captainIdRoom }).First();
            Item clueItemToPlace = new RogueBasin.Items.MovieClue(captainIdClue, representation, pickupMovie, description);
            PlaceClueItems(mapState, new List<Tuple<Clue, Item>> { new Tuple<Clue, Item>(captainIdClue, clueItemToPlace) }, false, true, true);

            LogFile.Log.LogEntryDebug("Placing " + clueItemToPlace.SingleItemDescription + " on level " + captainsIdLevel + " in vault " + captainIdRoom, LogDebugLevel.Medium);

            return captainIdRoom;
        }

        public void PlaceItems(MapState mapState, Item item, int room, bool boundariesPreferred)
        {
            PlaceItems(mapState, Enumerable.Repeat(item, 1), Enumerable.Repeat(room, 1), boundariesPreferred);
        }

        public void PlaceItems(MapState mapState, IEnumerable<Item> items, IEnumerable<int> rooms, bool boundariesPreferred)
        {
            var mapInfo = mapState.MapInfo;
            IEnumerable<RoomPoint> pointsToPlace = mapInfo.GetAllUnoccupiedRoomPoints(rooms, boundariesPreferred);

            if (!pointsToPlace.Any())
            {
                throw new ApplicationException("Nowhere to place item");
            }

            var pointsForItems = pointsToPlace.RepeatToLength(items.Count());
            var pointsAndItems = pointsForItems.Zip(items, (p, i) => new Tuple<Item, RoomPoint>(i, p));

            foreach (var pi in pointsAndItems)
            {
                mapInfo.Populator.AddItemToRoom(pi.Item1, pi.Item2.roomId, pi.Item2.ToRelativePoint(mapState.MapInfo));
            }
        }

    }
}
