using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
 

    public class MapGeneratorTemplated
    {

        class DoorInfo
        {
            public TemplatePositioned OwnerRoom { get; private set; }
            public int DoorIndexInRoom { get; private set;  }

            public DoorInfo(TemplatePositioned ownerRoom, int doorIndex)
            {
                OwnerRoom = ownerRoom;
                DoorIndexInRoom = doorIndex;
            }

            public Point MapCoords {
                get
                {
                    return OwnerRoom.PotentialDoors[DoorIndexInRoom];
                }
            }
        }

        /// <summary>
        /// Mapping from template terrain to real terrain on the map
        /// </summary>
        Dictionary<RoomTemplateTerrain, MapTerrain> terrainMapping;

        List<RoomTemplate> roomTemplates = new List<RoomTemplate>();
        List<RoomTemplate> corridorTemplates = new List<RoomTemplate>();
        List<DoorInfo> potentialDoors = new List<DoorInfo>();


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

        private DoorInfo RandomDoor()
        {
            return potentialDoors[Game.Random.Next(potentialDoors.Count)];
        }

        /** Build a map using templated rooms */
        public Map GenerateMap()
        {
            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            
            //Load sample templates
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);
            RoomTemplate corridor1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping);

            //Add to stores
            roomTemplates.Add(room1);
            corridorTemplates.Add(corridor1);

            int roomsToPlace = 40;
            int maxRoomDistance = 10;

            int roomsPlaced = 0;

            //Terminate when all rooms placed or no more potential door sites
            do
            {
                if (roomsPlaced == 0)
                {
                    //Place a random room at a location near the origin
                    var positionedRoom = new TemplatePositioned(Game.Random.Next(maxRoomDistance), Game.Random.Next(maxRoomDistance), 0, RandomRoom(), TemplateRotation.Deg0);
                    //Will always pass
                    mapBuilder.AddPositionedTemplateOnTop(positionedRoom);

                    //Store a reference to each potential door in the room
                    int noDoors = positionedRoom.PotentialDoors.Count();
                    for (int i = 0; i < noDoors; i++)
                    {
                        potentialDoors.Add(new DoorInfo(positionedRoom, i));
                    }

                    roomsPlaced++;
                }
                else
                {
                    //Find a random potential door and try to grow a random room off this

                    DoorInfo existingDoor = RandomDoor();
                    RoomTemplate roomTemplateToPlace = RandomRoom();

                    int newRoomDoorIndex = Game.Random.Next(roomTemplateToPlace.PotentialDoors.Count);
                    int distanceApart = Game.Random.Next(maxRoomDistance);

                    Point existingDoorLoc = existingDoor.MapCoords;

                    Tuple<TemplatePositioned, Point> newRoomTuple = RoomTemplateUtilities.AlignRoomOnDoor(roomTemplateToPlace, existingDoor.OwnerRoom,
                        newRoomDoorIndex, existingDoor.DoorIndexInRoom, distanceApart);

                    var alignedNewRoom = newRoomTuple.Item1;
                    var alignedDoorLocation = newRoomTuple.Item2;

                    //In order to place this successfully, we need to be able to both place the room and a connecting corridor

                    if (!mapBuilder.CanBePlacedWithoutOverlappingOtherTemplates(alignedNewRoom))
                        continue;

                    TemplatePositioned corridorTemplateConnectingRooms = null;

                    if (distanceApart > 0)
                    {
                        //Need points that are '1-in' from the doors
                        var corridorTermini = RoomTemplateUtilities.CorridorTerminalPointsBetweenDoors(existingDoorLoc, alignedDoorLocation);

                        corridorTemplateConnectingRooms =
                            RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(corridorTermini.Item1, corridorTermini.Item2, 0, corridorTemplates[0]);

                        //Implicit guarantee that the corridor won't overlap with the new room we're about to place
                        if (!mapBuilder.CanBePlacedWithoutOverlappingOtherTemplates(corridorTemplateConnectingRooms))
                            continue;

                        //Place the corridor
                        mapBuilder.AddPositionedTemplateOnTop(corridorTemplateConnectingRooms);
                    }

                    //Place the room
                    bool successfulPlacement = mapBuilder.AddPositionedTemplateOnTop(alignedNewRoom);
                    if (!successfulPlacement)
                    {
                        LogFile.Log.LogEntryDebug("Room failed to place because overlaps own corridor - bug", LogDebugLevel.High);
                    }

                    roomsPlaced++;

                    //Add the new potential doors
                    int noDoors = alignedNewRoom.PotentialDoors.Count();
                    for (int i = 0; i < noDoors; i++)
                    {
                        potentialDoors.Add(new DoorInfo(alignedNewRoom, i));
                    }

                    //If successful, remove the candidate door from the list
                    potentialDoors.Remove(existingDoor);

                }
            } while (roomsPlaced < roomsToPlace && potentialDoors.Count > 0);

            //Place room at coords
            //TemplatePositioned templatePos1 = new TemplatePositioned(10, 10, 0, room1, TemplateRotation.Deg0);
            //mapBuilder.AddPositionedTemplate(templatePos1);
            /*
            TemplatePositioned templatePos2 = new TemplatePositioned(0, 0, 10, room1, TemplateRotation.Deg0);
            mapBuilder.AddPositionedTemplate(templatePos2);

            TemplatePositioned corridorToPlace = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(5, 8), new Point(8, 8), 1, corridor1);
            mapBuilder.AddPositionedTemplate(corridorToPlace);

            TemplatePositioned corridorToPlaceVertical = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 10), new Point(0, 12), 2, corridor1);
            mapBuilder.AddPositionedTemplate(corridorToPlaceVertical);

            TemplatePositioned templatePos3 = new TemplatePositioned(20, 20, 11, room1, TemplateRotation.Deg0);
            mapBuilder.AddPositionedTemplate(templatePos3);

            TemplatePositioned templatePos4 = RoomTemplateUtilities.AlignRoomOnDoor(room1, templatePos3, 3, 0, 5);
            mapBuilder.AddPositionedTemplate(templatePos4);
            */
            Map masterMap = mapBuilder.MergeTemplatesIntoMap(terrainMapping);

            var firstRoom = mapBuilder.GetTemplateAtZ(0);
            masterMap.PCStartLocation = new Point(firstRoom.X - mapBuilder.MasterMapTopLeft.x + firstRoom.Room.Width / 2, firstRoom.Y - mapBuilder.MasterMapTopLeft.y + firstRoom.Room.Height / 2);

            return masterMap;
        }

    }
}
