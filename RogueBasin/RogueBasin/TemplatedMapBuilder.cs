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

        public Point Location { get { return new Point(X, Y); } }

        public TemplatePositioned(int x, int y, int z, RoomTemplate room, int roomIndex)
        {
            X = x;
            Y = y;
            Z = z;
            Room = room;
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
        /// Templates as they are placed on the map - not really used
        /// </summary>
        List<TemplatePositioned> templates = new List<TemplatePositioned>();

        /// <summary>
        /// 2d array for terrain
        /// </summary>
        ArrayCache<RoomTemplateTerrain> mapCache;

        /// <summary>
        /// 2d array for room ids
        /// </summary>
        ArrayCache<int> idCache;

        /// <summary>
        /// The X, Y coords of the top left of the output map: all templates will be offset by this when merged
        /// </summary>
        Point masterMapTopLeft;

        /// <summary>
        /// The X, Y coords of the bottom right of the output map
        /// </summary>
        Point masterMapBottomRight;

        /// <summary>
        /// May be null if no templates have been added
        /// </summary>
        public Point MasterMapTopLeft
        {
            get
            {
                return mapCache.TL;
            }
        }

        public TemplatedMapBuilder()
        {
            mapCache = new ArrayCache<RoomTemplateTerrain>(10, 10);
            idCache = new ArrayCache<int>(10, 10);
        }

        public TemplatedMapBuilder(int cacheX, int cacheY)
        {
            mapCache = new ArrayCache<RoomTemplateTerrain>(cacheX, cacheY);
            idCache = new ArrayCache<int>(cacheX, cacheY);
        }

        public bool CanBePlacedWithoutOverlappingOtherTemplates(TemplatePositioned template)
        {
            return mapCache.CheckMergeArea(template.Location, template.Room.terrainMap, MergeTerrain);
        }

        private RoomTemplateTerrain MergeTerrain(RoomTemplateTerrain originTerrain, RoomTemplateTerrain newTerrain)
        {
            if (originTerrain == RoomTemplateTerrain.Transparent)
            {
                return newTerrain;
            }
            else if(newTerrain == RoomTemplateTerrain.Transparent) {
                return originTerrain;
            }
            else {
                throw new ApplicationException("Can't overlap terrain");
            }
        }

        public bool AddPositionedTemplate(TemplatePositioned templateToAdd)
        {
            if (!CanBePlacedWithoutOverlappingOtherTemplates(templateToAdd))
                return false;

            try
            {
                templates.Add(templateToAdd);
                mapCache.MergeArea(templateToAdd.Location, templateToAdd.Room.terrainMap, MergeTerrain);
      
                idCache.MergeArea(templateToAdd.Location, MakeIdArray(templateToAdd.Room.terrainMap.GetLength(0), templateToAdd.Room.terrainMap.GetLength(1),
                    templateToAdd.RoomIndex), Math.Max);
                return true;
            }
            catch (ArgumentException e)
            {
                throw new ApplicationException("Can't place room: " + e.Message);
            }
        }

        private int[,] MakeIdArray(int x, int y, int val)
        {
            var ret = new int[x, y];

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    ret[i, j] = val;
                }

            }

            return ret;
        }

        /// <summary>
        /// Return the template at this z
        /// </summary>
        /// <param name="z"></param>
        /// <returns></returns>
        public TemplatePositioned GetTemplate(int z)
        {
            return templates[z];
        }

        /** Get the current terrain at the required point, calculated by flattening the templates. 
         Any terrain overrides Transparent, but no templates should be placed that having different types of
         terrain on the same point. e.g. 2 walls overlapping is OK but wall and floor overlapping is not.
         Absence of a template at this point returns Transparent */
        private RoomTemplateTerrain GetMergedTerrainAtPoint(Point point)
        {
            return mapCache.GetMergedPoint(point);
        }

        private void CalculateTemplatedMapExtent()
        {
            masterMapTopLeft = mapCache.TL;
            masterMapBottomRight = mapCache.BR;
            LogFile.Log.LogEntryDebug("Map coords: TL: " + masterMapTopLeft.x + ", " + masterMapTopLeft.y + " BR: " + masterMapBottomRight.x + ", " + masterMapBottomRight.y, LogDebugLevel.Medium);
        }

        /// <summary>
        /// Create a playable map by merging the terrain of the templates in z-order and mapping to real terrain types
        /// </summary>
        /// <returns></returns>
        public Map MergeTemplatesIntoMap(Dictionary<RoomTemplateTerrain, MapTerrain> terrainMapping)
        {
            if (templates.Count == 0)
                throw new ApplicationException("No templates in map");

            var mergedArea = mapCache.GetMergedArea();
            var mergedIdArea = idCache.GetMergedArea();

            Map masterMap = new Map(mergedArea.GetLength(0), mergedArea.GetLength(1));

            for (int i = 0; i < mergedArea.GetLength(0); i++)
            {
                for (int j = 0; j < mergedArea.GetLength(1); j++)
                {
                    masterMap.mapSquares[i, j].Terrain = terrainMapping[mergedArea[i, j]];
                    masterMap.roomIdMap[i, j] = mergedIdArea[i, j];
                }
            }

            return masterMap;
        }
    }
}
