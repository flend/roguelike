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

            int roomsToPlace = 50;
            int maxRoomDistance = 10;

            int roomsPlaced = 0;

            //Terminate when all rooms placed or no more potential door sites
            do
            {
                if (roomsPlaced == 0)
                {
                    //Place a random room at a location near the origin
                    templatedGenerator.PlaceRoomTemplateAtPosition(RandomRoom(), new Point(Game.Random.Next(maxRoomDistance), Game.Random.Next(maxRoomDistance)));
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

            public DoorInfo(TemplatePositioned ownerRoom, int ownerRoomIndex, int doorIndex)
            {
                OwnerRoom = ownerRoom;
                DoorIndexInRoom = doorIndex;
                OwnerRoomIndex = ownerRoomIndex;
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

            //Store a reference to each potential door in the room
            int noDoors = positionedRoom.PotentialDoors.Count();
            for (int i = 0; i < noDoors; i++)
            {
                potentialDoors.Add(new DoorInfo(positionedRoom, roomIndex, i));
            }

            return true;
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
                var corridorTermini = RoomTemplateUtilities.CorridorTerminalPointsBetweenDoors(existingDoorLoc, alignedDoorLocation);
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
            int noDoors = alignedNewRoom.PotentialDoors.Count();
            for (int i = 0; i < noDoors; i++)
            {
                potentialDoors.Add(new DoorInfo(alignedNewRoom, newRoomIndex, i));
            }

            //If successful, remove the candidate door from the list
            potentialDoors.Remove(existingDoor);

            return true;
        }
    }

}
