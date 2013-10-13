using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin.Algorithms
{
    interface IPathFinder
    {
        /// <summary>
        /// Return a list of nodes in the path, or null if no path is available.
        /// Path should include origin and destination
        /// On no path, should return origin only
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        List<Point> pathNodes(int level, Point origin, Point dest, bool openAllDoors);

        /// <summary>
        /// Update internal map representation, gets enum map for pathfinding
        /// </summary>
        /// <param name="byteMap"></param>
        void updateMap(int level, PathingMap byteMap);

        /// <summary>
        /// Update internal map representation, point by point
        /// </summary>
        void updateMap(int level, Point point, PathingTerrain newTerrain);
    }
}
