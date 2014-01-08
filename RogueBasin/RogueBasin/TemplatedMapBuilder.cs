using System;
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
        Deg0 = 0, Deg90 = 1, Deg180 = 2, Deg270 = 3
    }


    public class TemplatePositioned
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int Z { get; set; }

        public int RoomIndex { get; private set; }

        public RoomTemplate Room { get; private set; }

        public TemplateRotation Rotation { get; set; }

        public Point Location { get { return new Point(X, Y); } }

        public TemplatePositioned(int x, int y, int z, RoomTemplate room, TemplateRotation rotation, int roomIndex)
        {
            X = x;
            Y = y;
            Z = z;
            Room = room;
            Rotation = rotation;
            RoomIndex = roomIndex;
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
        public List<Point> PotentialDoors
        {
            get
            {
                return Room.PotentialDoors.Select(x => x.Location + new Point(X, Y)).ToList();
            }
        }

        public RoomTemplateTerrain TerrainAtPoint(Point p)
        {
            return Room.terrainMap[p.x - X, p.y - Y];
        }
    }

    
    /** Allows a map to be built up by placing templates in z-ordering.
     *  Z-ordering is currently meaningless because no overlap is allowed**/
    public class TemplatedMapBuilder
    {
        /// <summary>
        /// Templates as they are placed on the map (after positions, rotations etc.).
        /// Sorted by z-ordering
        /// </summary>
        SortedDictionary<int, TemplatePositioned> templates = new SortedDictionary<int, TemplatePositioned>();

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

        public bool CanBePlacedWithoutOverlappingOtherTemplates(TemplatePositioned template)
        {
            foreach (Point p in template.Extent())
            {
                //Overlap with existing template
                var existingMergedTerrain = GetMergedTerrainAtPoint(p);
                if (existingMergedTerrain != RoomTemplateTerrain.Transparent && template.TerrainAtPoint(p) != RoomTemplateTerrain.Transparent)
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
        public bool AddPositionedTemplate(TemplatePositioned templateToAdd)
        {
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

            if (templates.Count > 0)
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
        public TemplatePositioned GetTemplateAtZ(int z)
        {
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
                        masterMap.roomIdMap[roomMapLeft + i, roomMapTop + j] = templatePlacement.Value.RoomIndex;

                        //For transparent areas, the terrain below is kept
                        if (terrainToMerge != RoomTemplateTerrain.Transparent)
                        {
                            masterMap.mapSquares[roomMapLeft + i, roomMapTop + j].Terrain = terrainMapping[terrainToMerge];
                        }
                    }
                }
            }

            return masterMap;
        }
    }
}
