using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    static class MapUtils
    {
        /// <summary>
        /// Master is terrain walkable from MapTerrain type (not universally used yet) - defaults false
        /// </summary>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static bool IsTerrainWalkable(MapTerrain terrain)
        {
            if (terrain == MapTerrain.Empty ||
                terrain == MapTerrain.Flooded ||
                terrain == MapTerrain.OpenDoor ||
                terrain == MapTerrain.Corridor ||
                terrain == MapTerrain.Grass ||
                terrain == MapTerrain.Road ||
                terrain == MapTerrain.Gravestone ||
                terrain == MapTerrain.Trees ||
                terrain == MapTerrain.Rubble ||
                terrain == MapTerrain.OpenLock)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Master is terrain light blocking from MapTerrain type (not universally used yet) - defaults true
        /// </summary>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static bool IsTerrainLightBlocking(MapTerrain terrain)
        {
            if (terrain == MapTerrain.Empty ||
                terrain == MapTerrain.Flooded ||
                terrain == MapTerrain.OpenDoor ||
                terrain == MapTerrain.Corridor ||
                terrain == MapTerrain.Grass ||
                terrain == MapTerrain.Road ||
                terrain == MapTerrain.Gravestone ||
                terrain == MapTerrain.Trees ||
                terrain == MapTerrain.Rubble ||
                terrain == MapTerrain.OpenLock ||
                terrain == MapTerrain.NonWalkableFeature)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Get all adjacent coords
        /// </summary>
        /// <param name="locationMap"></param>
        /// <returns></returns>
        public static List<Point> GetAdjacentCoords(Point locationMap)
        {
            List<Point> adjacentSq = new List<Point>();

            adjacentSq.Add(new Point(locationMap.x + 1, locationMap.y - 1));
            adjacentSq.Add(new Point(locationMap.x + 1, locationMap.y));
            adjacentSq.Add(new Point(locationMap.x + 1, locationMap.y + 1));
            adjacentSq.Add(new Point(locationMap.x - 1, locationMap.y - 1));
            adjacentSq.Add(new Point(locationMap.x - 1, locationMap.y));
            adjacentSq.Add(new Point(locationMap.x - 1, locationMap.y + 1));
            adjacentSq.Add(new Point(locationMap.x, locationMap.y + 1));
            adjacentSq.Add(new Point(locationMap.x, locationMap.y - 1));
            return adjacentSq;
        }

    }
}
