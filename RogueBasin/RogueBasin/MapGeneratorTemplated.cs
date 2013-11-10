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
    }

    public class MapGeneratorTemplated
    {
        /// <summary>
        /// Mapping from template terrain to real terrain on the map
        /// </summary>
        Dictionary<RoomTemplateTerrain, MapTerrain> terrainMapping;

        public MapGeneratorTemplated()
        {
            terrainMapping = new Dictionary<RoomTemplateTerrain, MapTerrain>();
            terrainMapping[RoomTemplateTerrain.Wall] = MapTerrain.Wall;
            terrainMapping[RoomTemplateTerrain.Floor] = MapTerrain.Empty;
            terrainMapping[RoomTemplateTerrain.Transparent] = MapTerrain.Void;
        }


        /** Build a map using templated rooms */
        public Map GenerateMap()
        {
            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            
            //Load sample template
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("RogueBasin.bin.Debug.vaults.vault1.room", StandardTemplateMapping.terrainMapping);

            //Place room at coords
            TemplatePositioned templatePos1 = new TemplatePositioned(10, 10, 0, room1, TemplateRotation.Deg0);
            mapBuilder.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(0, 0, 10, room1, TemplateRotation.Deg0);
            mapBuilder.AddPositionedTemplate(templatePos2);

            Map masterMap = mapBuilder.MergeTemplatesIntoMap(terrainMapping);

            masterMap.PCStartLocation = new Point(templatePos1.X - mapBuilder.MasterMapTopLeft.x + room1.Width / 2, templatePos1.Y - mapBuilder.MasterMapTopLeft.y + room1.Height / 2);

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

        /// <summary>
        /// Add template at z ordering specified in the pre-built template class
        /// </summary>
        /// <param name="z"></param>
        /// <param name="templateToAdd"></param>
        public bool AddPositionedTemplate(TemplatePositioned templateToAdd) {

            foreach(Point p in templateToAdd.Extent()) {
                //Overlap with existing template
                if (GetMergedTerrainAtPoint(p) != RoomTemplateTerrain.Transparent)
                {
                    LogFile.Log.LogEntryDebug("Overlapping terrain at " + p + " add template failed", LogDebugLevel.Medium);
                    return false;
                }
            }

            templates.Add(templateToAdd.Z, templateToAdd);
            return true;
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

                if (!(ptRelativeToTemplate.x >= 0 && point.x < roomExtent.Width &&
                    point.y >= 0 && point.y < roomExtent.Height))
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
            //Calculate smallest rectangle to enclose all templates in current positions
            CalculateTemplatedMapExtent();
            Map masterMap = new Map(masterMapBottomRight.x - masterMapTopLeft.x + 1, masterMapBottomRight.y - masterMapTopLeft.y + 1);

            //Merge each template onto the map in z-order
            foreach (var templatePlacement in templates)
            {
                RoomTemplate template = templatePlacement.Value.Room;
                TemplateRectangle roomExtent = templatePlacement.Value.Extent();

                //Find masterMap relative coordinates
                int roomMapLeft = roomExtent.Left + masterMapTopLeft.x;
                int roomMapTop = roomExtent.Top + masterMapTopLeft.y;

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

        /** Stretches a corridor template into a full sized corridor of length.
         *  Template must be n x 1 (1 row deep).*/
        private RoomTemplate ExpandCorridorTemplate(bool switchToHorizontal, int length, RoomTemplate corridorTemplate)
        {
            if (corridorTemplate.Height > 1)
                throw new ApplicationException("Only corridor templates of height 1 supported");

            RoomTemplateTerrain[,] newRoom;

            if (switchToHorizontal)
            {
                newRoom = new RoomTemplateTerrain[length, corridorTemplate.Width];
                for (int j = 0; j < length; j++)
                {
                    for (int i = 0; i < corridorTemplate.Width; i++)
                    {
                        newRoom[j, i] = corridorTemplate.terrainMap[i, 0];
                    }
                }
            }
            else
            {
                newRoom = new RoomTemplateTerrain[corridorTemplate.Width, length];
                for (int j = 0; j < length; j++)
                {
                    for (int i = 0; i < corridorTemplate.Width; i++)
                    {
                        newRoom[i, j] = corridorTemplate.terrainMap[i, 0];
                    }
                }
            }

            return new RoomTemplate(newRoom);
        }

        /** Expand the corridor template (vertically aligned) into a suitable room template and add */
        public void AddCorridorBetweenPoints(Point point1, Point point2, int z, RoomTemplate corridorTemplate)
        {
            if (!((point1.x == point2.x) || (point1.y == point2.y)))
            {
                throw new ApplicationException("Corridors must be straight");
            }

            bool horizontalSwitchNeeded = false;
            int length;

            if(point1.y == point2.y) {
                horizontalSwitchNeeded = true;
                length = Math.Abs(point1.x - point2.x);
            }
            else {
                length = Math.Abs(point1.y - point2.y);
            }

            RoomTemplate expandedCorridor = ExpandCorridorTemplate(horizontalSwitchNeeded, length, corridorTemplate);

            int centreOfTemplateShortAxis = corridorTemplate.Width / 2;





            throw new NotImplementedException();
        }
    }
}
