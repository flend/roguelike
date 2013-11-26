using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Anticlockwise rotation of a template onto the map
    /// </summary>
    public enum TemplateRotation
    {
        Deg0, Deg90, Deg180, Deg270
    }


    public class TemplatePositioned
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int Z { get; set; }

        public RoomTemplate Room { get; private set; }

        public TemplateRotation Rotation { get; set; }

        public TemplatePositioned(int x, int y, int z, RoomTemplate room, TemplateRotation rotation)
        {
            X = x;
            Y = y;
            Z = z;
            Room = room;
            Rotation = rotation;
        }

        /// <summary>
        /// Return a rectangle representing the extent of the room in positioned (X, Y) coordinates
        /// </summary>
        /// <returns></returns>
        public TemplateRectangle Extent()
        {
            return new TemplateRectangle(X, Y, Room.Width, Room.Height);
        }

        /// <summary>
        /// Return potential doors in post-positioned coordinates
        /// </summary>
        public IEnumerable<Point> PotentialDoors {
            get
            {
                return Room.PotentialDoors.Select(x => x.Location + new Point(X, Y));
            }
        }
    }

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

            int roomsToPlace = 20;
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

                    DoorInfo randomDoor = RandomDoor();
                    RoomTemplate roomTemplateToPlace = RandomRoom();
                    TemplatePositioned alignedNewRoom = RoomTemplateUtilities.AlignRoomOnDoor(roomTemplateToPlace, randomDoor.OwnerRoom,
                        Game.Random.Next(roomTemplateToPlace.PotentialDoors.Count), randomDoor.DoorIndexInRoom,
                        Game.Random.Next(maxRoomDistance));

                    if (mapBuilder.AddPositionedTemplateOnTop(alignedNewRoom))
                    {
                        roomsPlaced++;
                        
                        //Add the new potential doors
                        int noDoors = alignedNewRoom.PotentialDoors.Count();
                        for (int i = 0; i < noDoors; i++)
                        {
                            potentialDoors.Add(new DoorInfo(alignedNewRoom, i));
                        }

                        //If successful, remove the candidate door from the list
                        potentialDoors.Remove(randomDoor);
                    }
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

    /** Allows a map to be built up by placing templates in z-ordering **/
    public class TemplatedMapBuilder
    {
        /// <summary>
        /// Templates as they are placed on the map (after positions, rotations etc.).
        /// Sorted by z-ordering
        /// </summary>
        SortedDictionary<int, TemplatePositioned> templates = new SortedDictionary<int,TemplatePositioned>();

        /// <summary>
        /// The X, Y coords of the top left of the output map: all templates will be offset by this when merged
        /// </summary>
        Point masterMapTopLeft;

        /// <summary>
        /// The X, Y coords of the bottom right of the output map
        /// </summary>
        Point masterMapBottomRight;

        /// <summary>
        /// Only available after MergeTemplatesIntoMap() is called
        /// </summary>
        public Point MasterMapTopLeft
        {
            get
            {
                //May not have been calculated yet
                CalculateTemplatedMapExtent();
                return masterMapTopLeft;
            }
        }

        private bool CanBePlacedWithoutOverlappingOtherTemplates(TemplatePositioned template)
        {
            foreach (Point p in template.Extent())
            {
                //Overlap with existing template
                if (GetMergedTerrainAtPoint(p) != RoomTemplateTerrain.Transparent)
                {
                    LogFile.Log.LogEntryDebug("Overlapping terrain at " + p + " add template failed", LogDebugLevel.Medium);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Add template at z ordering specified in the pre-built template class. Z must be unique
        /// </summary>
        /// <param name="z"></param>
        /// <param name="templateToAdd"></param>
        public bool AddPositionedTemplate(TemplatePositioned templateToAdd) {

            return AddPositionedTemplate(templateToAdd, templateToAdd.Z);
        }

        /// <summary>
        /// Add template on top of current templates. Overwrites Z value in templateToAdd
        /// </summary>
        /// <param name="z"></param>
        /// <param name="templateToAdd"></param>
        public bool AddPositionedTemplateOnTop(TemplatePositioned templateToAdd)
        {
            int maxZ = 0;

            if(templates.Count > 0)
                maxZ = templates.Keys.Max(x => x) + 1;

            return AddPositionedTemplate(templateToAdd, maxZ);
        }

        private bool AddPositionedTemplate(TemplatePositioned templateToAdd, int zToPlace)
        {
            if (!CanBePlacedWithoutOverlappingOtherTemplates(templateToAdd))
                return false;

            try
            {
                templates.Add(zToPlace, templateToAdd);
                return true;
            }
            catch (ArgumentException e)
            {
                throw new ApplicationException("Can't place room at z: " + zToPlace + e.Message);
            }
        }

        /// <summary>
        /// Return the template at this z
        /// </summary>
        /// <param name="z"></param>
        /// <returns></returns>
        public TemplatePositioned GetTemplateAtZ(int z) {
            return templates[z];
        }

        /** Get the current terrain at the required point, calculated by flattening the templates. 
         Any terrain overrides Transparent, but no templates should be placed that having different types of
         terrain on the same point. e.g. 2 walls overlapping is OK but wall and floor overlapping is not.
         Absence of a template at this point returns Transparent */
        private RoomTemplateTerrain GetMergedTerrainAtPoint(Point point)
        {
            foreach (var templatePlacement in templates)
            {
                TemplateRectangle roomExtent = templatePlacement.Value.Extent();

                Point ptRelativeToTemplate = new Point(point.x - roomExtent.Left, point.y - roomExtent.Top);

                //Check for point outside of template

                if (!(ptRelativeToTemplate.x >= 0 && ptRelativeToTemplate.x < roomExtent.Width &&
                    ptRelativeToTemplate.y >= 0 && ptRelativeToTemplate.y < roomExtent.Height))
                    continue;

                RoomTemplateTerrain thisTerrain = templatePlacement.Value.Room.terrainMap[ptRelativeToTemplate.x, ptRelativeToTemplate.y];

                if (thisTerrain != RoomTemplateTerrain.Transparent)
                    return thisTerrain;
            }

            return RoomTemplateTerrain.Transparent;
        }


        private void CalculateTemplatedMapExtent()
        {
            int mapLeft = templates.Min(x => x.Value.Extent().Left);
            int mapRight = templates.Max(x => x.Value.Extent().Right);
            int mapTop = templates.Min(x => x.Value.Extent().Top);
            int mapBottom = templates.Max(x => x.Value.Extent().Bottom);

            masterMapTopLeft = new Point(mapLeft, mapTop);
            masterMapBottomRight = new Point(mapRight, mapBottom);
            LogFile.Log.LogEntryDebug("Map coords: TL: " + masterMapTopLeft.x + ", " + masterMapTopLeft.y + " BR: " + mapBottom + ", " + mapRight, LogDebugLevel.Medium);
        }

        /// <summary>
        /// Create a playable map by merging the terrain of the templates in z-order and mapping to real terrain types
        /// </summary>
        /// <returns></returns>
        public Map MergeTemplatesIntoMap(Dictionary<RoomTemplateTerrain, MapTerrain> terrainMapping)
        {

            if (templates.Count == 0)
                throw new ApplicationException("No templates in map");

            //Calculate smallest rectangle to enclose all templates in current positions
            CalculateTemplatedMapExtent();
            Map masterMap = new Map(masterMapBottomRight.x - masterMapTopLeft.x + 1, masterMapBottomRight.y - masterMapTopLeft.y + 1);

            //Merge each template onto the map in z-order
            foreach (var templatePlacement in templates)
            {
                RoomTemplate template = templatePlacement.Value.Room;
                TemplateRectangle roomExtent = templatePlacement.Value.Extent();

                //Find masterMap relative coordinates
                int roomMapLeft = roomExtent.Left - masterMapTopLeft.x; 
                int roomMapTop = roomExtent.Top - masterMapTopLeft.y;

                for (int i = 0; i < roomExtent.Width; i++)
                {
                    for (int j = 0; j < roomExtent.Height; j++)
                    {
                        RoomTemplateTerrain terrainToMerge = template.terrainMap[i, j];

                        //For transparent areas, the terrain below is kept
                        if(terrainToMerge != RoomTemplateTerrain.Transparent) {
                            masterMap.mapSquares[roomMapLeft + i, roomMapTop + j].Terrain = terrainMapping[terrainToMerge];
                        }
                    }
                }
            }

            return masterMap;
        }
    }
}
