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
            return Room.TerrainMap[p.x - X, p.y - Y];
        }
    }

    
    /** Allows a map to be built up by placing templates in z-ordering.
     *  Z-ordering is currently meaningless because no overlap is allowed**/
    public class TemplatedMapBuilder
    {
        /// <summary>
        /// 2d array for terrain
        /// </summary>
        ArrayCache<RoomTemplateTerrain> mapCache;

        /// <summary>
        /// 2d array for room ids
        /// </summary>
        ArrayCache<int> idCache;

        /// <summary>
        /// Default value for id map. Must not be a possible id
        /// </summary>
        readonly public static int defaultId = -1;

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
            InitialiseCaches(10, 10);
        }

        public TemplatedMapBuilder(int cacheX, int cacheY)
        {
            InitialiseCaches(cacheX, cacheY);
        }

        private void InitialiseCaches(int cacheX, int cacheY) {
            mapCache = new ArrayCache<RoomTemplateTerrain>(cacheX, cacheY, RoomTemplateTerrain.Transparent);
            idCache = new ArrayCache<int>(cacheX, cacheY, defaultId);
        }

        public bool CanBePlacedWithoutOverlappingOtherTemplates(TemplatePositioned template)
        {
            return mapCache.CheckMergeArea(template.Location, template.Room.TerrainMap, MergeTerrain, CheckNotCompletelyOverlapping);
        }

        /// <summary>
        /// Check placement using the rules for replacing a room
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public bool CanBePlacedOverlappingOtherTemplates(TemplatePositioned template)
        {
            return mapCache.CheckMergeArea(template.Location, template.Room.TerrainMap, OverrideFloorTerrain);
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
                //Can overlap indentical terrain
                if (newTerrain == originTerrain)
                    return originTerrain;

                //Mismatched terrain throws exception
                throw new ApplicationException("Can't overlap terrain");
            }
        }

        private RoomTemplateTerrain OverrideTerrain(RoomTemplateTerrain originTerrain, RoomTemplateTerrain newTerrain)
        {
            return newTerrain;
        }

        private RoomTemplateTerrain OverrideFloorTerrain(RoomTemplateTerrain originTerrain, RoomTemplateTerrain newTerrain)
        {
            if (originTerrain == RoomTemplateTerrain.Transparent)
            {
                return newTerrain;
            }
            else if (newTerrain == RoomTemplateTerrain.Transparent)
            {
                return originTerrain;
            }
            else
            {
                //Can overlap identical terrain
                if (newTerrain == originTerrain)
                    return originTerrain;

                //Can replace floor with new terrain
                if (originTerrain == RoomTemplateTerrain.Floor)
                    return newTerrain;

                //Mismatched terrain throws exception
                throw new ApplicationException("Can't overlap terrain");
            }
        }

        public bool AddPositionedTemplate(TemplatePositioned templateToAdd)
        {
            if (!CanBePlacedWithoutOverlappingOtherTemplates(templateToAdd))
                return false;

            try
            {
                mapCache.MergeArea(templateToAdd.Location, templateToAdd.Room.TerrainMap, MergeTerrain, CheckNotCompletelyOverlapping);
      
                idCache.MergeArea(templateToAdd.Location, MakeIdArray(templateToAdd), MergeIds);
                return true;
            }
            catch (ArgumentException e)
            {
                throw new ApplicationException("Can't place room: " + e.Message);
            }
        }

        /// <summary>
        /// Add the terrain to the map, overriding what was there before.
        /// Use with caution since may break connectivity
        /// </summary>
        /// <param name="templateToAdd"></param>
        /// <returns></returns>
        public bool OverridePositionedTemplate(TemplatePositioned templateToAdd)
        {
            if (!CanBePlacedOverlappingOtherTemplates(templateToAdd))
                return false;

            try
            {
                mapCache.MergeArea(templateToAdd.Location, templateToAdd.Room.TerrainMap, OverrideFloorTerrain);

                idCache.MergeArea(templateToAdd.Location, MakeIdArray(templateToAdd), OverrideIds);

                return true;
            }
            catch (ArgumentException e)
            {
                throw new ApplicationException("Can't place room: " + e.Message);
            }
        }

        /// <summary>
        /// Add the terrain to the map, overriding what was there before.
        /// Use with caution since may break connectivity
        /// Performs no checking and allows any override
        /// </summary>
        /// <param name="templateToAdd"></param>
        /// <returns></returns>
        public void UnconditionallyOverridePositionedTemplate(TemplatePositioned templateToAdd)
        {
            try
            {
                mapCache.MergeArea(templateToAdd.Location, templateToAdd.Room.TerrainMap, OverrideTerrain);

                idCache.MergeArea(templateToAdd.Location, MakeIdArray(templateToAdd), OverrideIds);
            }
            catch (ArgumentException e)
            {
                throw new ApplicationException("Can't place room: " + e.Message);
            }
        }


        /// <summary>
        /// Ensure that rooms can't completely overlap (which passes - is any terrain different - test) 
        /// by requiring some different terrain (which implicitally must be transparent)
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        private bool CheckNotCompletelyOverlapping(RoomTemplateTerrain arg1, RoomTemplateTerrain arg2)
        {
            if (arg1 != arg2)
                return true;
            return false;
        }

        /// <summary>
        /// Merge in terrain at location, ignoring using transparent requirement
        /// </summary>
        /// <param name="location"></param>
        /// <param name="terrain"></param>
        public void AddOverrideTerrain(Point location, RoomTemplateTerrain terrain)
        {
            RoomTemplateTerrain[,] terrainToAdd = new RoomTemplateTerrain[1,1];
            terrainToAdd[0, 0] = terrain;

            mapCache.MergeArea(location, terrainToAdd, OverrideTerrain);
        }

        private int MergeIds(int existingId, int newId)
        {
            if (existingId == defaultId)
                return newId;
            else
                return existingId;
        }

        private int OverrideIds(int existingId, int newId)
        {
            return newId;
        }

        private int[,] MakeIdArray(TemplatePositioned template)
        {
            var ret = new int[template.Room.TerrainMap.GetLength(0), template.Room.TerrainMap.GetLength(1)];

            for (int i = 0; i < template.Room.TerrainMap.GetLength(0); i++)
            {
                for (int j = 0; j < template.Room.TerrainMap.GetLength(1); j++)
                {
                    if (template.Room.TerrainMap[i, j] != RoomTemplateTerrain.Transparent)
                        ret[i, j] = template.RoomIndex;
                    else
                        ret[i, j] = defaultId;
                }

            }

            return ret;
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
