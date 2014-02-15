using libtcodWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin.LibTCOD
{
    public class TCODPathFindingWrapper : Algorithms.IPathFinder
    {

        Dictionary<int, TCODFov> levelTCODMaps;
        Dictionary<int, TCODFov> levelTCODMapsIgnoringClosedDoors;

        public TCODPathFindingWrapper()
        {
            levelTCODMaps = new Dictionary<int, TCODFov>();
            levelTCODMapsIgnoringClosedDoors = new Dictionary<int, TCODFov>();
        }

        public bool arePointsConnected(int level, Point origin, Point dest, bool openAllDoors)
        {
            return pathNodes(level, origin, dest, openAllDoors).Count > 1;
        }

        public void updateMap(int level, PathingMap terrainMap) {

            TCODFov tcodLevel = new TCODFov(terrainMap.Width, terrainMap.Height);

            for (int j = 0; j < terrainMap.Width; j++)
            {
                for (int k = 0; k < terrainMap.Height; k++)
                {
                    tcodLevel.SetCell(j, k, true, terrainMap.getCell(j,k) == PathingTerrain.Walkable);
                }
            }

            levelTCODMaps[level] = tcodLevel;

            //Ignoring closed doors

            TCODFov tcodLevelNoClosedDoors = new TCODFov(terrainMap.Width, terrainMap.Height);
            
            for (int j = 0; j < terrainMap.Width; j++)
            {
                for (int k = 0; k < terrainMap.Height; k++)
                {
                    tcodLevelNoClosedDoors.SetCell(j, k, true, terrainMap.getCell(j,k) == PathingTerrain.Walkable || terrainMap.getCell(j,k) == PathingTerrain.ClosedDoor);
                }
            }

            levelTCODMapsIgnoringClosedDoors[level] = tcodLevelNoClosedDoors;
        }

        public void updateMap(int level, Point point, PathingTerrain newTerrain)
        {
            if (levelTCODMaps.Count() == 0)
            {
                LogFile.Log.LogEntryDebug("updateMap called before pathfinding initially done.", LogDebugLevel.Medium);
                return;
            }

            switch (newTerrain)
            {
                case PathingTerrain.ClosedDoor:
                    levelTCODMaps[level].SetCell(point.x, point.y, true, false);
                    levelTCODMapsIgnoringClosedDoors[level].SetCell(point.x, point.y, true, true);
                    break;
                case PathingTerrain.Unwalkable:
                    levelTCODMaps[level].SetCell(point.x, point.y, true, false);
                    levelTCODMapsIgnoringClosedDoors[level].SetCell(point.x, point.y, true, false);
                    break;
                case PathingTerrain.Walkable:
                    levelTCODMaps[level].SetCell(point.x, point.y, true, true);
                    levelTCODMapsIgnoringClosedDoors[level].SetCell(point.x, point.y, true, true);
                    break;
            }
        }

        public List<Point> pathNodes(int level, Point origin, Point dest, bool openAllDoors) {

            List<Point> returnNodes = new List<Point>();

            TCODFov mapToUse;

            if (openAllDoors)
                mapToUse = levelTCODMapsIgnoringClosedDoors[level];
            else
                mapToUse = levelTCODMaps[level];

            //Try to walk the path
            TCODPathFinding path = new TCODPathFinding(mapToUse, 1.0);
            path.ComputePath(origin.x, origin.y, dest.x, dest.y);

            returnNodes.Add(origin);

            int x = origin.x;
            int y = origin.y;
           
            path.WalkPath(ref x, ref y, false);
            returnNodes.Add(new Point(x, y));

            //If the x and y of the next step it means the path is blocked

            if (x == origin.x && y == origin.y)
            {
                //Return 1 node list
                return returnNodes.GetRange(0, 1);
            }

            do
            {
                path.WalkPath(ref x, ref y, false);
                returnNodes.Add(new Point(x, y));
                if (x == dest.x && y == dest.y)
                    return returnNodes;
            } while (true);
        }
    }
}
