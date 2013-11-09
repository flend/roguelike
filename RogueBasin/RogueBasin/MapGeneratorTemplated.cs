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

        /// <summary>
        /// Templates as they are placed on the map (after positions, rotations etc.).
        /// Sorted by z-ordering
        /// </summary>
        SortedDictionary<int, TemplatePositioned> templates = new SortedDictionary<int,TemplatePositioned>();

        /// <summary>
        /// The X, Y coords of the top of the output map: all templates will be offset by this when merged
        /// </summary>
        Point masterMapTopLeft;

        public MapGeneratorTemplated()
        {
            terrainMapping = new Dictionary<RoomTemplateTerrain, MapTerrain>();
            terrainMapping[RoomTemplateTerrain.Wall] = MapTerrain.Wall;
            terrainMapping[RoomTemplateTerrain.Floor] = MapTerrain.Empty;
            terrainMapping[RoomTemplateTerrain.Transparent] = MapTerrain.Void;
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
        
        /** Build a map using templated rooms */
        public Map GenerateMap()
        {

            //Load sample template
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile("vaults.vault1.room", StandardTemplateMapping.terrainMapping);

            //Place room at coords
            TemplatePositioned templatePos1 = new TemplatePositioned(10, 10, 0, room1, TemplateRotation.Deg0);
            AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(0, 0, 10, room1, TemplateRotation.Deg0);
            AddPositionedTemplate(templatePos2);

            //should fail
            TemplatePositioned templatePos3 = new TemplatePositioned(4, 4, 10, room1, TemplateRotation.Deg0);
            AddPositionedTemplate(templatePos3);

            Map masterMap = MergeTemplatesIntoMap();

            masterMap.PCStartLocation = new Point(templatePos1.X - masterMapTopLeft.x + room1.Width / 2, templatePos1.Y - masterMapTopLeft.y + room1.Height / 2);

            return masterMap;
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


        /// <summary>
        /// Create a playable map by merging the terrain of the templates in z-order
        /// </summary>
        /// <returns></returns>
        private Map MergeTemplatesIntoMap()
        {
            //Calculate smallest rectangle to enclose all templates in current positions

            int mapLeft = templates.Min(x => x.Value.Extent().Left);
            int mapRight = templates.Max(x => x.Value.Extent().Right);
            int mapTop = templates.Min(x => x.Value.Extent().Top);
            int mapBottom = templates.Max(x => x.Value.Extent().Bottom);

            masterMapTopLeft = new Point(mapLeft, mapTop);
            LogFile.Log.LogEntryDebug("Map coords: TL: " + masterMapTopLeft.x + ", " + masterMapTopLeft.y + " BR: " + mapBottom + ", " + mapRight, LogDebugLevel.Medium);

            Map masterMap = new Map(mapRight - mapLeft + 1, mapBottom - mapTop + 1);

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
    }
}
