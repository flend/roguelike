using GraphMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
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

    /** Builds up a template map and connectivity graph, exposing methods for room aligned placement */
    public class TemplatedMapGenerator
    {
    
        List<DoorInfo> potentialDoors = new List<DoorInfo>();
        Dictionary<Connection, DoorInfo> connectionDoors = new Dictionary<Connection, DoorInfo>();
        int nextRoomIndex = 0;

        TemplatedMapBuilder mapBuilder;
        ConnectivityMap connectivityMap;

        Dictionary<int, TemplatePositioned> templates = new Dictionary<int, TemplatePositioned>();

        public TemplatedMapGenerator(TemplatedMapBuilder builder)
        {
            this.mapBuilder = builder;
            this.connectivityMap = new ConnectivityMap();
        }

        public TemplatedMapGenerator(TemplatedMapBuilder builder, int roomIndexToStart)
        {
            this.mapBuilder = builder;
            this.connectivityMap = new ConnectivityMap();
            this.nextRoomIndex = roomIndexToStart;
        }

        public int NextRoomIndex()
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

        public Dictionary<Connection, DoorInfo> ConnectionDoors
        {
            get
            {
                return connectionDoors;
            }
        }

        public int PlaceRoomTemplateAtPosition(RoomTemplate roomTemplate, Point point)
        {
            var roomIndex = NextRoomIndex();
            var positionedRoom = new TemplatePositioned(point.x, point.y, 0, roomTemplate, roomIndex);
            templates[roomIndex] = positionedRoom;
            bool placementSuccess = mapBuilder.AddPositionedTemplate(positionedRoom);

            if (!placementSuccess)
                throw new ApplicationException("Can't place template at position");

            IncreaseNextRoomIndex();
            AddNewDoorsToPotentialDoors(positionedRoom, roomIndex);

            return roomIndex;
        }

        private void AddNewDoorsToPotentialDoors(TemplatePositioned positionedRoom, int roomIndex)
        {
            //Store a reference to each potential door in the room
            int noDoors = positionedRoom.PotentialDoors.Count();
            //var currentDoorLocations = potentialDoors.Select(d => d.MapCoords);

            for (int i = 0; i < noDoors; i++)
            {
                //if (!currentDoorLocations.Contains(positionedRoom.PotentialDoors[i]))
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
                if (connectionDoors.ContainsKey(new Connection(firstDoor.OwnerRoomIndex, secondDoor.OwnerRoomIndex).Ordered))
                {
                    LogFile.Log.LogEntryDebug("No allowing 2nd connection between rooms for now - revisit past 7DRL", LogDebugLevel.High);
                    return false;
                }

                var firstDoorLoc = RoomTemplateUtilities.GetDoorLocation(firstDoor.OwnerRoom.Room, firstDoor.DoorIndexInRoom);
                var secondDoorLoc = RoomTemplateUtilities.GetDoorLocation(secondDoor.OwnerRoom.Room, secondDoor.DoorIndexInRoom);

                var firstDoorCoord = firstDoor.MapCoords;
                var secondDoorCoord = secondDoor.MapCoords;

                var corridorTermini = RoomTemplateUtilities.CorridorTerminalPointsBetweenDoors(firstDoor.MapCoords, firstDoor.DoorLocation, secondDoor.MapCoords, secondDoor.DoorLocation);

                bool canDoLSharedCorridor = RoomTemplateUtilities.CanBeConnectedWithLShapedCorridor(firstDoorCoord, firstDoorLoc, secondDoorCoord, secondDoorLoc);
                bool canDoBendCorridor = RoomTemplateUtilities.CanBeConnectedWithBendCorridor(firstDoorCoord, firstDoorLoc, secondDoorCoord, secondDoorLoc);
                bool canDoStraightCorridor = RoomTemplateUtilities.CanBeConnectedWithStraightCorridor(firstDoorCoord, firstDoorLoc, secondDoorCoord, secondDoorLoc);
                bool areAdjacent = corridorTermini.Item1 == secondDoorCoord && corridorTermini.Item2 == firstDoorCoord;
                bool areOverlapping = firstDoorCoord == secondDoorCoord;

                if (!canDoLSharedCorridor && !canDoBendCorridor && !canDoStraightCorridor && !areAdjacent && !areOverlapping)
                    throw new ApplicationException("No corridor available to connect this type of door");

                if (areAdjacent || areOverlapping)
                {
                    //Add a direct connection in the connectivity graph
                    connectivityMap.AddRoomConnection(firstDoor.OwnerRoomIndex, secondDoor.OwnerRoomIndex);
                    connectionDoors.Add(new Connection(firstDoor.OwnerRoomIndex, secondDoor.OwnerRoomIndex).Ordered, firstDoor);
                }
                else
                {
                    //Create template

                    var horizontal = false;
                    if (firstDoorLoc == RoomTemplate.DoorLocation.Left || firstDoorLoc == RoomTemplate.DoorLocation.Right)
                    {
                        horizontal = true;
                    }

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
                    else if (canDoLSharedCorridor)
                    {
                        var expandedCorridorAndPoint = RoomTemplateUtilities.ExpandCorridorTemplateLShaped(xOffset, yOffset, horizontal, corridorTemplate);
                        expandedCorridor = expandedCorridorAndPoint.Item1;
                        corridorTerminus1InTemplate = expandedCorridorAndPoint.Item2;
                    }
                    else
                    {
                        var offsetToUse = horizontal ? xOffset : yOffset;
                        var expandedCorridorAndPoint = RoomTemplateUtilities.ExpandCorridorTemplateStraight(offsetToUse, horizontal, corridorTemplate);
                        expandedCorridor = expandedCorridorAndPoint.Item1;
                        corridorTerminus1InTemplate = expandedCorridorAndPoint.Item2;
                    }

                    //Place corridor

                    //Match corridor tile to location of door
                    Point topLeftCorridor = corridorTermini.Item1 - corridorTerminus1InTemplate;

                    var corridorRoomIndex = NextRoomIndex();
                    var positionedCorridor = new TemplatePositioned(topLeftCorridor.x, topLeftCorridor.y, 0, expandedCorridor, corridorRoomIndex);

                    if (!mapBuilder.CanBePlacedWithoutOverlappingOtherTemplates(positionedCorridor))
                        return false;

                    //Place the corridor
                    mapBuilder.AddPositionedTemplate(positionedCorridor);
                    templates[corridorRoomIndex] = positionedCorridor;
                    IncreaseNextRoomIndex();

                    //Add connections to the old and new rooms
                    connectivityMap.AddRoomConnection(firstDoor.OwnerRoomIndex, corridorRoomIndex);
                    connectivityMap.AddRoomConnection(corridorRoomIndex, secondDoor.OwnerRoomIndex);

                    connectionDoors.Add(new Connection(firstDoor.OwnerRoomIndex, corridorRoomIndex).Ordered, firstDoor);
                    connectionDoors.Add(new Connection(corridorRoomIndex, secondDoor.OwnerRoomIndex).Ordered, secondDoor);
                }
                
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
        
        /// <summary>
        /// Replace a room template with a smaller one. Aligns new template with old door.
        /// This should only be called on dead-ends
        /// Enforces that replacement rooms can only override floor areas of old rooms (helping to avoid some breakage)
        /// </summary>
        public bool ReplaceRoomTemplate(int roomToReplaceIndex, Connection existingRoomConnection, RoomTemplate replacementRoom, int replaceRoomDoorIndex)
        {
            var templateToReplace = templates[roomToReplaceIndex];

            if (templateToReplace.PotentialDoors.Count() > 1)
                throw new ApplicationException("Can't replace rooms with >1 doors");

            var doorToConnection = GetDoorForConnection(existingRoomConnection);

            Tuple<TemplatePositioned, Point> replacementRoomTuple = RoomTemplateUtilities.AlignRoomOverlapping(replacementRoom, roomToReplaceIndex, templateToReplace,
                replaceRoomDoorIndex, doorToConnection.DoorIndexInRoom);

            var replacementRoomTemplate = replacementRoomTuple.Item1;

            //Check if the overlapping room can be placed
            if (!mapBuilder.CanBePlacedOverlappingOtherTemplates(replacementRoomTemplate))
                return false;

            //Blank the old room area
            var voidedOldRoomTemplate = RoomTemplateUtilities.TransparentTemplate(templateToReplace);
            mapBuilder.UnconditionallyOverridePositionedTemplate(voidedOldRoomTemplate);

            //Override with new template
            mapBuilder.OverridePositionedTemplate(replacementRoomTemplate);

            //Ensure that the room is replaced in the index
            templates[roomToReplaceIndex] = replacementRoomTemplate;

            //We don't change the connectivity or doors

            return true;
        }

        /// <summary>
        /// Place a room template aligned with an existing door.
        /// Returns Connection(Source = existing room or corridor to new room, Target = new room))
        /// </summary>
        public Connection PlaceRoomTemplateAlignedWithExistingDoor(RoomTemplate roomTemplateToPlace, RoomTemplate corridorTemplate, DoorInfo existingDoor, int newRoomDoorIndex, int distanceApart)
        {
            var newRoomIndex = NextRoomIndex();
            
            Point existingDoorLoc = existingDoor.MapCoords;

            Tuple<TemplatePositioned, Point> newRoomTuple = RoomTemplateUtilities.AlignRoomFacing(roomTemplateToPlace, newRoomIndex, existingDoor.OwnerRoom,
                newRoomDoorIndex, existingDoor.DoorIndexInRoom, distanceApart);

            var alignedNewRoom = newRoomTuple.Item1;
            var alignedDoorLocation = newRoomTuple.Item2;

            var alignedDoorIndex = alignedNewRoom.PotentialDoors.IndexOf(alignedDoorLocation);
            var alignedDoor = new DoorInfo(alignedNewRoom, newRoomIndex, alignedDoorIndex, RoomTemplateUtilities.GetDoorLocation(alignedNewRoom.Room, alignedDoorIndex));

            //In order to place this successfully, we need to be able to both place the room and a connecting corridor

            if (!mapBuilder.CanBePlacedWithoutOverlappingOtherTemplates(alignedNewRoom))
                throw new ApplicationException("Room failed to place because overlaps existing room");

            //Increase next room for any corridor we may add
            IncreaseNextRoomIndex();

            TemplatePositioned corridorTemplateConnectingRooms = null;
            Connection connectionToNewRoom = null;

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
                    throw new ApplicationException("Room failed to place because corridor overlaps existing room");

                //Place the corridor
                mapBuilder.AddPositionedTemplate(corridorTemplateConnectingRooms);
                templates[corridorIndex] = corridorTemplateConnectingRooms;
                IncreaseNextRoomIndex();

                //Add connections to the old and new rooms
                connectionToNewRoom = new Connection(corridorIndex, newRoomIndex);

                connectivityMap.AddRoomConnection(existingDoor.OwnerRoomIndex, corridorIndex);
                LogFile.Log.LogEntryDebug("Adding connection: " + existingDoor.OwnerRoomIndex + " to " + corridorIndex, LogDebugLevel.Medium);
                connectivityMap.AddRoomConnection(corridorIndex, newRoomIndex);
                LogFile.Log.LogEntryDebug("Adding connection: " + corridorIndex + " to " + newRoomIndex, LogDebugLevel.Medium);

                connectionDoors.Add(new Connection(existingDoor.OwnerRoomIndex, corridorIndex).Ordered, existingDoor);
                connectionDoors.Add(connectionToNewRoom.Ordered, alignedDoor);
            }
            else
            {
                //No corridor - a direct connection between the rooms
                connectionToNewRoom = new Connection(existingDoor.OwnerRoomIndex, newRoomIndex);

                connectivityMap.AddRoomConnection(existingDoor.OwnerRoomIndex, newRoomIndex);
                connectionDoors.Add(connectionToNewRoom.Ordered, alignedDoor);
                LogFile.Log.LogEntryDebug("Adding connection: " + existingDoor.OwnerRoomIndex + " to " + newRoomIndex, LogDebugLevel.Medium);
            }

            //Place the room
            bool successfulPlacement = mapBuilder.AddPositionedTemplate(alignedNewRoom);
            if (!successfulPlacement)
            {
                LogFile.Log.LogEntryDebug("Room failed to place because overlaps own corridor - bug", LogDebugLevel.High);
                throw new ApplicationException("Room failed to place because overlaps own corridor - bug");
            }
            templates[newRoomIndex] = alignedNewRoom;

            //Add the new potential doors (excluding the one we are linked on)
            //Can't find a nice linq alternative
            int noDoors = alignedNewRoom.PotentialDoors.Count();
            for (int i = 0; i < noDoors; i++)
            {
                if (alignedNewRoom.PotentialDoors[i] == alignedDoorLocation)
                    continue;
                potentialDoors.Add(new DoorInfo(alignedNewRoom, newRoomIndex, i, RoomTemplateUtilities.GetDoorLocation(alignedNewRoom.Room, i)));
            }

            //If successful, remove the candidate door from the list
            potentialDoors.Remove(existingDoor);

            return connectionToNewRoom;
        }

        public void ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain roomTemplateTerrain)
        {
            foreach (var door in PotentialDoors)
            {
                mapBuilder.AddOverrideTerrain(door.MapCoords, roomTemplateTerrain);
            }

            PotentialDoors.Clear();
        }

        public void ReplaceConnectedDoorsWithTerrain(RoomTemplateTerrain roomTemplateTerrain)
        {
            foreach (var door in ConnectionDoors)
            {
                mapBuilder.AddOverrideTerrain(door.Value.MapCoords, roomTemplateTerrain);
            }
        }

        /// <summary>
        /// Get all the room templates in world / map coords. These can then be passed onto another class
        /// </summary>
        /// <returns></returns>
        public List<TemplatePositioned> GetRoomTemplatesInWorldCoords()
        {
            var templatesList = templates.Values.Select(v => v).ToList();
            return templatesList.Select(t => 
                new TemplatePositioned(t.X - mapBuilder.MasterMapTopLeft.x, t.Y - mapBuilder.MasterMapTopLeft.y, t.Z, t.Room, t.RoomIndex)).ToList();
        }

        /// <summary>
        /// Get all the doors in world / map coords. These can then be passed onto another class
        /// </summary>
        /// <returns></returns>
        public Dictionary<Connection, Point> GetDoorsInMapCoords()
        {
            var dictToReturn = new Dictionary<Connection, Point>();

            foreach(var door in connectionDoors) {
                var doorLocMap = door.Value.MapCoords;
                var doorLocWorld = new Point(doorLocMap.x - mapBuilder.MasterMapTopLeft.x, doorLocMap.y - mapBuilder.MasterMapTopLeft.y);

                dictToReturn.Add(door.Key, doorLocWorld);
            }

            return dictToReturn;
        }

        /// <summary>
        /// Get the door on the map for the connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public DoorInfo GetDoorForConnection(Connection connection)
        {
            return connectionDoors[connection.Ordered];
        }


        public TemplatePositioned GetRoomTemplateById(int roomId)
        {
            return templates[roomId];
        }
    }
}
