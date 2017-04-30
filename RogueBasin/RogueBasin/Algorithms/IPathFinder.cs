﻿using System.Collections.Generic;

namespace RogueBasin.Algorithms
{
    public interface IPathFinder
    {
        /// <summary>
        /// Return a list of nodes in the path, or null if no path is available.
        /// Path should include origin and destination
        /// On no path, should return origin only
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        List<Point> pathNodes(int level, Point origin, Point dest, Pathing.PathingPermission permission, bool ignoreDangerousTerrain);

        /// <summary>
        /// Update internal map representation, gets enum map for pathfinding
        /// </summary>
        /// <param name="byteMap"></param>
        void updateMap(int level, PathingMap byteMap);

        /// <summary>
        /// Update internal map representation, point by point
        /// </summary>
        void updateMap(int level, Point point, PathingTerrain newTerrain);

        /// <summary>
        /// Update internal map representation, point by point
        /// </summary>
        void updateMap(int level, Point point, PathingTerrain newTerrain, Pathing.PathingPermission permission, bool ignoreDangerousTerrain);

        
        /// <summary>
        /// Is this square pathable?
        /// </summary>
        /// <param name="level"></param>
        /// <param name="point"></param>
        /// <param name="permission"></param>
        /// <param name="ignoreDangerousTerrain"></param>
        /// <returns></returns>
        bool getPathable(int level, Point point, Pathing.PathingPermission permission, bool ignoreDangerousTerrain);

        /// <summary>
        /// Update internal map representation, with the addition or removal of dangerous terrain
        /// </summary>
        /// <param name="level"></param>
        /// <param name="point"></param>
        /// <param name="terrainPresent"></param>
        void updateMapWithDangerousTerrain(int level, Point point, bool terrainPresent);

        /// <summary>
        /// Are these points connected?
        /// </summary>
        /// <param name="level"></param>
        /// <param name="origin"></param>
        /// <param name="dest"></param>
        /// <param name="openAllDoors"></param>
        bool arePointsConnected(int level, Point origin, Point dest, Pathing.PathingPermission permission);
    }
}
