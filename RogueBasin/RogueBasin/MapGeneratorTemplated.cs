using GraphMap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{

    public class MapGeneratorTemplated
    {


        /// <summary>
        /// Mapping from template terrain to real terrain on the map
        /// </summary>
        Dictionary<RoomTemplateTerrain, MapTerrain> terrainMapping;

        List<RoomTemplate> roomTemplates = new List<RoomTemplate>();
        List<RoomTemplate> corridorTemplates = new List<RoomTemplate>();

        TemplatedMapGenerator templatedGenerator;

        public MapGeneratorTemplated()
        {
            terrainMapping = new Dictionary<RoomTemplateTerrain, MapTerrain>();
            terrainMapping[RoomTemplateTerrain.Wall] = MapTerrain.Wall;
            terrainMapping[RoomTemplateTerrain.Floor] = MapTerrain.Empty;
            terrainMapping[RoomTemplateTerrain.Transparent] = MapTerrain.Void;
            terrainMapping[RoomTemplateTerrain.WallWithPossibleDoor] = MapTerrain.ClosedDoor;
        }

        private RoomTemplate RandomRoom()
        {
            return roomTemplates[Game.Random.Next(roomTemplates.Count)];
        }

        private RoomTemplate RandomCorridor()
        {
            return corridorTemplates[Game.Random.Next(corridorTemplates.Count)];
        }

        private TemplatedMapGenerator.DoorInfo RandomDoor(TemplatedMapGenerator generator)
        {
            return generator.PotentialDoors[Game.Random.Next(generator.PotentialDoors.Count())];
        }

        public ConnectivityMap ConnectivityMap
        {
            get
            {
                return templatedGenerator.ConnectivityMap;
            }
        }

        /** Build a map using templated rooms */
        public Map GenerateMap()
        {
            //Load sample templates
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            //Add to stores
            roomTemplates.Add(room1);
            corridorTemplates.Add(corridor1);

            //Create generator
            var mapBuilder = new TemplatedMapBuilder();
            templatedGenerator = new TemplatedMapGenerator(mapBuilder);

            int roomsToPlace = 20;
            int maxRoomDistance = 10;

            int roomsPlaced = 0;

            //Terminate when all rooms placed or no more potential door sites
            do
            {
                if (roomsPlaced == 0)
                {
                    //Place a random room at a location near the origin
                    bool success = templatedGenerator.PlaceRoomTemplateAtPosition(RandomRoom(), new Point(Game.Random.Next(maxRoomDistance), Game.Random.Next(maxRoomDistance)));
                    if (success)
                        roomsPlaced++;
                }
                else
                {
                    //Find a random potential door and try to grow a random room off this
                    templatedGenerator.PlaceRoomTemplateAlignedWithExistingDoor(RandomRoom(), corridorTemplates[0], RandomDoor(templatedGenerator),
                        Game.Random.Next(maxRoomDistance));

                    roomsPlaced++;
                }
            } while (roomsPlaced < roomsToPlace && templatedGenerator.HaveRemainingPotentialDoors());

            //Add some extra connections, if doors are available
            var totalExtraConnections = 500;
            var extraConnections = 0;

            var allDoors = templatedGenerator.PotentialDoors;

            //Find all possible doors matches that aren't in the same room
            var allMatchingDoorPossibilities = from d1 in allDoors
                                               from d2 in allDoors
                                               where d1.DoorLocation == RoomTemplateUtilities.GetOppositeDoorLocation(d2.DoorLocation)
                                                     && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                               select new { origin = d1, target = d2 };

            while (allMatchingDoorPossibilities.Any() && extraConnections < totalExtraConnections)
            {
                //Try a random combination to see if it works
                var doorsToTry = allMatchingDoorPossibilities.ElementAt(Game.Random.Next(allMatchingDoorPossibilities.Count()));

                LogFile.Log.LogEntryDebug("Trying door " + doorsToTry.origin.MapCoords + " to " + doorsToTry.target.MapCoords, LogDebugLevel.Medium);

                bool success = templatedGenerator.JoinDoorsWithCorridor(doorsToTry.origin, doorsToTry.target, corridor1);
                if (success)
                    extraConnections++;

                //In any case, remove this attempt
                allMatchingDoorPossibilities = allMatchingDoorPossibilities.Except(Enumerable.Repeat(doorsToTry, 1));
            }

            Map masterMap = mapBuilder.MergeTemplatesIntoMap(terrainMapping);

            var firstRoom = mapBuilder.GetTemplateAtZ(0);
            masterMap.PCStartLocation = new Point(firstRoom.X - mapBuilder.MasterMapTopLeft.x + firstRoom.Room.Width / 2, firstRoom.Y - mapBuilder.MasterMapTopLeft.y + firstRoom.Room.Height / 2);

            return masterMap;
        }

    }

    /** Builds up a template map and connectivity graph, exposing methods for room aligned placement */
    public class TemplatedMapGenerator
    {
        public class DoorInfo
        {
            public TemplatePositioned OwnerRoom { get; private set; }
            public int DoorIndexInRoom { get; private set; }
            public int OwnerRoomIndex { get; private set; }
            public RoomTemplate.DoorLocation DoorLocation { get; private set; }

            public DoorInfo(TemplatePositioned ownerRoom, int ownerRoomIndex, int doorIndex, RoomTemplate.DoorLocation doorLocation)
            {
                OwnerRoom = ownerRoom;
                DoorIndexInRoom = doorIndex;
                OwnerRoomIndex = ownerRoomIndex;
                DoorLocation = doorLocation;
            }

            public Point MapCoords
            {
                get
                {
                    return OwnerRoom.PotentialDoors[DoorIndexInRoom];
                }
            }
        }

        List<DoorInfo> potentialDoors = new List<DoorInfo>();
        int nextRoomIndex = 0;

        TemplatedMapBuilder mapBuilder;
        ConnectivityMap connectivityMap;

        public TemplatedMapGenerator(TemplatedMapBuilder builder)
        {
            this.mapBuilder = builder;
            this.connectivityMap = new ConnectivityMap();
        }

        private int NextRoomIndex()
        {
            return nextRoomIndex;
        }

        private void IncreaseNextRoomIndex()
        {
            nextRoomIndex++;
        }

        public bool HaveRemainingPotentialDoors()
        {
            return potentialDoors.Count > 0;
        }

        public List<DoorInfo> PotentialDoors
        {
            get
            {
                return potentialDoors;
            }
        }

        public ConnectivityMap ConnectivityMap
        {
            get
            {
                return connectivityMap;
            }
        }

        public bool PlaceRoomTemplateAtPosition(RoomTemplate roomTemplate, Point point)
        {
            var roomIndex = NextRoomIndex();
            var positionedRoom = new TemplatePositioned(point.x, point.y, 0, roomTemplate, TemplateRotation.Deg0, roomIndex);
            bool placementSuccess = mapBuilder.AddPositionedTemplateOnTop(positionedRoom);

            if (!placementSuccess)
                return false;

            IncreaseNextRoomIndex();
            AddNewRoomsToPotentialRooms(positionedRoom, roomIndex);

            return true;
        }

        private void AddNewRoomsToPotentialRooms(TemplatePositioned positionedRoom, int roomIndex)
        {
            //Store a reference to each potential door in the room
            int noDoors = positionedRoom.PotentialDoors.Count();
            for (int i = 0; i < noDoors; i++)
            {
                potentialDoors.Add(new DoorInfo(positionedRoom, roomIndex, i, RoomTemplateUtilities.GetDoorLocation(positionedRoom.Room, i)));
            }
        }

        /// <summary>
        /// Join 2 doors with a corridor. They must be on the opposite sides of their parent rooms (for now)
        /// </summary>
        public bool JoinDoorsWithCorridor(DoorInfo firstDoor, DoorInfo secondDoor, RoomTemplate corridorTemplate)
        {
            try
            {
                var firstDoorLoc = RoomTemplateUtilities.GetDoorLocation(firstDoor.OwnerRoom.Room, firstDoor.DoorIndexInRoom);
                var secondDoorLoc = RoomTemplateUtilities.GetDoorLocation(secondDoor.OwnerRoom.Room, secondDoor.DoorIndexInRoom);

                var firstDoorCoord = firstDoor.MapCoords;
                var secondDoorCoord = secondDoor.MapCoords;

                bool canDoLSharedCorridor = RoomTemplateUtilities.CanBeConnectedWithLShapedCorridor(firstDoorCoord, firstDoorLoc, secondDoorCoord, secondDoorLoc);
                bool canDoBendCorridor = RoomTemplateUtilities.CanBeConnectedWithBendCorridor(firstDoorCoord, firstDoorLoc, secondDoorCoord, secondDoorLoc);

                if (!canDoLSharedCorridor && !canDoBendCorridor)
                    throw new ApplicationException("No corridor available to connect this type of door");

                //Create template

                var horizontal = false;
                if (firstDoorLoc == RoomTemplate.DoorLocation.Left || firstDoorLoc == RoomTemplate.DoorLocation.Right)
                {
                    horizontal = true;
                }

                var corridorTermini = RoomTemplateUtilities.CorridorTerminalPointsBetweenDoors(firstDoor.MapCoords, firstDoor.DoorLocation, secondDoor.MapCoords, secondDoor.DoorLocation);

                int xOffset = corridorTermini.Item2.x - corridorTermini.Item1.x;
                int yOffset = corridorTermini.Item2.y - corridorTermini.Item1.y;

                RoomTemplate expandedCorridor;
                Point corridorTerminus1InTemplate;

                if (canDoBendCorridor)
                {
                    int transition = (int)Math.Floor(yOffset / 2.0);
                    if (horizontal == true)
                    {
                        transition = (int)Math.Floor(xOffset / 2.0);
                    }
                    var expandedCorridorAndPoint = RoomTemplateUtilities.ExpandCorridorTemplateBend(xOffset, yOffset, transition, horizontal, corridorTemplate);
                    expandedCorridor = expandedCorridorAndPoint.Item1;
                    corridorTerminus1InTemplate = expandedCorridorAndPoint.Item2;
                }
                else
                {
                    var expandedCorridorAndPoint = RoomTemplateUtilities.ExpandCorridorTemplateLShaped(xOffset, yOffset, horizontal, corridorTemplate);
                    expandedCorridor = expandedCorridorAndPoint.Item1;
                    corridorTerminus1InTemplate = expandedCorridorAndPoint.Item2;
                }

                //Place corridor

                //Match corridor tile to location of door
                Point topLeftCorridor = corridorTermini.Item1 - corridorTerminus1InTemplate;

                var corridorRoomIndex = NextRoomIndex();
                var positionedCorridor = new TemplatePositioned(topLeftCorridor.x, topLeftCorridor.y, 0, expandedCorridor, TemplateRotation.Deg0, corridorRoomIndex);

                if (!mapBuilder.CanBePlacedWithoutOverlappingOtherTemplates(positionedCorridor))
                    return false;

                //Place the corridor
                mapBuilder.AddPositionedTemplateOnTop(positionedCorridor);
                IncreaseNextRoomIndex();

                //Add connections to the old and new rooms
                connectivityMap.AddRoomConnection(firstDoor.OwnerRoomIndex, corridorRoomIndex);
                connectivityMap.AddRoomConnection(corridorRoomIndex, secondDoor.OwnerRoomIndex);

                //Remove both doors from the potential list
                potentialDoors.Remove(firstDoor);
                potentialDoors.Remove(secondDoor);

                return true;
            }
            catch (ApplicationException ex)
            {
                LogFile.Log.LogEntryDebug("Failed to join doors: " + ex.Message, LogDebugLevel.Medium);
                return false;
            }
        }

        public bool PlaceRoomTemplateAlignedWithExistingDoor(RoomTemplate roomTemplateToPlace, RoomTemplate corridorTemplate, DoorInfo existingDoor, int distanceApart)
        {
            var newRoomIndex = NextRoomIndex();
            int newRoomDoorIndex = Game.Random.Next(roomTemplateToPlace.PotentialDoors.Count);

            Point existingDoorLoc = existingDoor.MapCoords;

            Tuple<TemplatePositioned, Point> newRoomTuple = RoomTemplateUtilities.AlignRoomOnDoor(roomTemplateToPlace, newRoomIndex, existingDoor.OwnerRoom,
                newRoomDoorIndex, existingDoor.DoorIndexInRoom, distanceApart);

            var alignedNewRoom = newRoomTuple.Item1;
            var alignedDoorLocation = newRoomTuple.Item2;

            //In order to place this successfully, we need to be able to both place the room and a connecting corridor

            if (!mapBuilder.CanBePlacedWithoutOverlappingOtherTemplates(alignedNewRoom))
                return false;

            //Increase next room for any corridor we may add
            IncreaseNextRoomIndex();

            TemplatePositioned corridorTemplateConnectingRooms = null;

            if (distanceApart > 1)
            {
                //Need points that are '1-in' from the doors
                var doorOrientation = RoomTemplateUtilities.GetDoorLocation(existingDoor.OwnerRoom.Room, existingDoor.DoorIndexInRoom);
                bool isHorizontal = doorOrientation == RoomTemplate.DoorLocation.Left || doorOrientation == RoomTemplate.DoorLocation.Right;

                var corridorTermini = RoomTemplateUtilities.CorridorTerminalPointsBetweenDoors(existingDoorLoc, existingDoor.DoorLocation, alignedDoorLocation, RoomTemplateUtilities.GetOppositeDoorLocation(existingDoor.DoorLocation));
                var corridorIndex = NextRoomIndex();

                if (corridorTermini.Item1 == corridorTermini.Item2)
                {
                    corridorTemplateConnectingRooms =
                        RoomTemplateUtilities.GetTemplateForSingleSpaceCorridor(corridorTermini.Item1,
                        RoomTemplateUtilities.ArePointsOnVerticalLine(corridorTermini.Item1, corridorTermini.Item2), 0, corridorTemplate, corridorIndex);
                }
                else
                {
                    corridorTemplateConnectingRooms =
                        RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(corridorTermini.Item1, corridorTermini.Item2, 0, corridorTemplate, corridorIndex);
                }

                //Implicit guarantee that the corridor won't overlap with the new room we're about to place
                //(but it may overlap other previously placed rooms or corridors)
                if (!mapBuilder.CanBePlacedWithoutOverlappingOtherTemplates(corridorTemplateConnectingRooms))
                    return false;

                //Place the corridor
                mapBuilder.AddPositionedTemplateOnTop(corridorTemplateConnectingRooms);
                IncreaseNextRoomIndex();

                //Add connections to the old and new rooms
                connectivityMap.AddRoomConnection(existingDoor.OwnerRoomIndex, corridorIndex);
                connectivityMap.AddRoomConnection(corridorIndex, newRoomIndex);
            }
            else
            {
                //No corridor - a direct connection between the rooms
                connectivityMap.AddRoomConnection(existingDoor.OwnerRoomIndex, newRoomIndex);
            }

            //Place the room
            bool successfulPlacement = mapBuilder.AddPositionedTemplateOnTop(alignedNewRoom);
            if (!successfulPlacement)
            {
                LogFile.Log.LogEntryDebug("Room failed to place because overlaps own corridor - bug", LogDebugLevel.High);
            }

            //Add the new potential doors

            AddNewRoomsToPotentialRooms(alignedNewRoom, newRoomIndex);

            //If successful, remove the candidate door from the list
            potentialDoors.Remove(existingDoor);

            return true;
        }
    }

}
