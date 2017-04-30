using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    static class RoomTemplateTerrainWalkable
    {
        public static readonly Dictionary<RoomTemplateTerrain, bool> terrainWalkable;

        static RoomTemplateTerrainWalkable()
        {
            terrainWalkable = new Dictionary<RoomTemplateTerrain, bool>();

            terrainWalkable.Add(RoomTemplateTerrain.Floor, true);
            terrainWalkable.Add(RoomTemplateTerrain.OpenWithPossibleDoor, true);
            terrainWalkable.Add(RoomTemplateTerrain.Transparent, false);
            terrainWalkable.Add(RoomTemplateTerrain.Wall, false);
            terrainWalkable.Add(RoomTemplateTerrain.WallWithPossibleDoor, true);
        }
    }


    public class RoomRouting
    {
        private RoomTemplate template;
        private Algorithms.IPathFinder pathFinding;
        private PathingMap pathingMap;

        public RoomRouting(RoomTemplate template)
        {
            this.template = template;

            BuildPathableMap();
        }

        private void BuildPathableMap() {

            pathFinding = new LibRogueSharp.RogueSharpPathAndFoVWrapper();

            pathingMap = new PathingMap(template.Width, template.Height);

            for (int i = 0; i < template.Width; i++)
            {
                for (int j = 0; j < template.Height; j++)
                {
                    pathingMap.setCell(i, j,
                        RoomTemplateTerrainWalkable.terrainWalkable[template.TerrainMap[i, j]] ? PathingTerrain.Walkable : PathingTerrain.Unwalkable);
                }
            }

            pathFinding.updateMap(0, pathingMap);
        }

        public bool SetSquareUnwalkableIfDoorPathingIsPreserved(Point p)
        {
            if (p.x < 0 || p.y < 0 || p.x >= template.Width || p.y >= template.Height)
                return false;

            //Set square unwalkable to test
            var originalTerrain = pathingMap.getCell(p.x, p.y);
            pathFinding.updateMap(0, p, PathingTerrain.Unwalkable);

            //Check all doors are pathable to each other
            bool pathable = true;

            var doors = template.PotentialDoors;

            for (int i = 0; i < doors.Count; i++)
            {
                for (int j = 0; j < doors.Count; j++)
                {
                    if (i == j)
                        continue;

                    if (!pathFinding.arePointsConnected(0, doors[i].Location, doors[j].Location, Pathing.PathingPermission.Normal))
                    {
                        pathable = false;
                        break;
                    }
                }
            }

            if (!pathable)
            {
                pathFinding.updateMap(0, p, originalTerrain);
                return false;
            }
            return true;
        }
    }
}
