using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;

namespace RogueBasin.LibRogueSharp
{
    class MapSize
    {
        public readonly int width;
        public readonly int height;

        public MapSize(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    }

    public class RogueSharpPathFindingWrapper : Algorithms.IPathFinder
    {
        Dictionary<int, RogueSharp.Map> sharpMaps = new Dictionary<int, RogueSharp.Map>();
        Dictionary<int, RogueSharp.Map> sharpMapsIgnoringTerrain = new Dictionary<int, RogueSharp.Map>();
        Dictionary<int, RogueSharp.Map> sharpMapsIgnoringClosedDoors = new Dictionary<int, RogueSharp.Map>();
        Dictionary<int, RogueSharp.Map> sharpMapsIgnoringClosedDoorsAndTerrain = new Dictionary<int, RogueSharp.Map>();
        Dictionary<int, RogueSharp.Map> sharpMapsIgnoringClosedDoorsAndLocks = new Dictionary<int, RogueSharp.Map>();

        Dictionary<int, MapSize> levelTCODMapsSize = new Dictionary<int, MapSize>();

        public RogueSharpPathFindingWrapper()
        {
        }

        public bool arePointsConnected(int level, Point origin, Point dest, Pathing.PathingPermission permission)
        {
            return pathNodes(level, origin, dest, permission, true).Count > 1;
        }

        public void updateMap(int level, PathingMap terrainMap)
        {
            RogueSharp.Map sharpLevel = new RogueSharp.Map(terrainMap.Width, terrainMap.Height);

            for (int j = 0; j < terrainMap.Width; j++)
            {
                for (int k = 0; k < terrainMap.Height; k++)
                {
                    sharpLevel.SetCellProperties(j, k, true, terrainMap.getCell(j, k) == PathingTerrain.Walkable);
                }
            }

            sharpMaps[level] = sharpLevel;
            levelTCODMapsSize[level] = new MapSize(terrainMap.Width, terrainMap.Height);

            //Taking into account dangerous terrain (will be done in separate calls)
            RogueSharp.Map sharpLevelIgnoringDangerousTerrain = new RogueSharp.Map(terrainMap.Width, terrainMap.Height);

            for (int j = 0; j < terrainMap.Width; j++)
            {
                for (int k = 0; k < terrainMap.Height; k++)
                {
                    sharpLevelIgnoringDangerousTerrain.SetCellProperties(j, k, true, terrainMap.getCell(j, k) == PathingTerrain.Walkable);
                }
            }

            sharpMapsIgnoringTerrain[level] = sharpLevelIgnoringDangerousTerrain;

            //Ignoring closed doors

            RogueSharp.Map sharpLevelNoClosedDoors = new RogueSharp.Map(terrainMap.Width, terrainMap.Height);

            for (int j = 0; j < terrainMap.Width; j++)
            {
                for (int k = 0; k < terrainMap.Height; k++)
                {
                    sharpLevelNoClosedDoors.SetCellProperties(j, k, true, terrainMap.getCell(j, k) == PathingTerrain.Walkable || terrainMap.getCell(j, k) == PathingTerrain.ClosedDoor);
                }
            }

            sharpMapsIgnoringClosedDoors[level] = sharpLevelNoClosedDoors;

            //Ignoring closed doors and dangerous terrain

            RogueSharp.Map sharpLevelNoClosedDoorsWithTerrain = new RogueSharp.Map(terrainMap.Width, terrainMap.Height);

            for (int j = 0; j < terrainMap.Width; j++)
            {
                for (int k = 0; k < terrainMap.Height; k++)
                {
                    sharpLevelNoClosedDoorsWithTerrain.SetCellProperties(j, k, true, terrainMap.getCell(j, k) == PathingTerrain.Walkable || terrainMap.getCell(j, k) == PathingTerrain.ClosedDoor);
                }
            }

            sharpMapsIgnoringClosedDoorsAndTerrain[level] = sharpLevelNoClosedDoorsWithTerrain;

            //Ignoring closed doors and locks
            RogueSharp.Map sharpLevelNoClosedDoorsAndLocks = new RogueSharp.Map(terrainMap.Width, terrainMap.Height);

            for (int j = 0; j < terrainMap.Width; j++)
            {
                for (int k = 0; k < terrainMap.Height; k++)
                {
                    sharpLevelNoClosedDoorsAndLocks.SetCellProperties(j, k, true, terrainMap.getCell(j, k) == PathingTerrain.Walkable ||
                        terrainMap.getCell(j, k) == PathingTerrain.ClosedDoor ||
                        terrainMap.getCell(j, k) == PathingTerrain.ClosedLock);
                }
            }

            sharpMapsIgnoringClosedDoorsAndLocks[level] = sharpLevelNoClosedDoorsAndLocks;
        }

        public void updateMapWithDangerousTerrain(int level, Point point, bool terrainPresent)
        {
            sharpMaps[level].SetCellProperties(point.x, point.y, true, !terrainPresent);
            sharpMapsIgnoringClosedDoors[level].SetCellProperties(point.x, point.y, true, !terrainPresent);
            sharpMapsIgnoringClosedDoorsAndLocks[level].SetCellProperties(point.x, point.y, true, !terrainPresent);
        }
        
        public bool getPathable(int level, Point point, Pathing.PathingPermission permission, bool ignoreDangerousTerrain)
        {
            RogueSharp.Map mapToUse;

            switch (permission)
            {
                case Pathing.PathingPermission.Normal:
                    if (ignoreDangerousTerrain)
                        mapToUse = sharpMapsIgnoringTerrain[level];
                    else
                        mapToUse = sharpMaps[level];
                    break;
                case Pathing.PathingPermission.IgnoreDoors:
                    if (ignoreDangerousTerrain)
                        mapToUse = sharpMapsIgnoringClosedDoorsAndTerrain[level];
                    else
                        mapToUse = sharpMapsIgnoringClosedDoors[level];
                    break;
                case Pathing.PathingPermission.IgnoreDoorsAndLocks:
                    mapToUse = sharpMapsIgnoringClosedDoorsAndLocks[level];
                    break;
                default:
                    mapToUse = sharpMaps[level];
                    break;
            }

            return mapToUse.GetCell(point.x, point.y).IsWalkable;
        }

        //Should this be deprecated in favour of the dangerous terrain version?
        public void updateMap(int level, Point point, PathingTerrain newTerrain)
        {
            if (sharpMaps.Count() == 0)
            {
                LogFile.Log.LogEntryDebug("updateMap called before pathfinding initially done.", LogDebugLevel.Medium);
                return;
            }

            switch (newTerrain)
            {
                case PathingTerrain.ClosedLock:
                    sharpMaps[level].SetCellProperties(point.x, point.y, true, false);
                    sharpMapsIgnoringTerrain[level].SetCellProperties(point.x, point.y, true, false);
                    sharpMapsIgnoringClosedDoors[level].SetCellProperties(point.x, point.y, true, false);
                    sharpMapsIgnoringClosedDoorsAndTerrain[level].SetCellProperties(point.x, point.y, true, false);
                    sharpMapsIgnoringClosedDoorsAndLocks[level].SetCellProperties(point.x, point.y, true, true);
                    break;
                case PathingTerrain.ClosedDoor:
                    sharpMaps[level].SetCellProperties(point.x, point.y, true, false);
                    sharpMapsIgnoringTerrain[level].SetCellProperties(point.x, point.y, true, false);
                    sharpMapsIgnoringClosedDoors[level].SetCellProperties(point.x, point.y, true, true);
                    sharpMapsIgnoringClosedDoorsAndTerrain[level].SetCellProperties(point.x, point.y, true, true);
                    sharpMapsIgnoringClosedDoorsAndLocks[level].SetCellProperties(point.x, point.y, true, true);
                    break;
                case PathingTerrain.Unwalkable:
                    sharpMaps[level].SetCellProperties(point.x, point.y, true, false);
                    sharpMapsIgnoringTerrain[level].SetCellProperties(point.x, point.y, true, false);
                    sharpMapsIgnoringClosedDoors[level].SetCellProperties(point.x, point.y, true, false);
                    sharpMapsIgnoringClosedDoorsAndTerrain[level].SetCellProperties(point.x, point.y, true, false);
                    sharpMapsIgnoringClosedDoorsAndLocks[level].SetCellProperties(point.x, point.y, true, false);
                    break;
                case PathingTerrain.Walkable:
                    sharpMaps[level].SetCellProperties(point.x, point.y, true, true);
                    sharpMapsIgnoringTerrain[level].SetCellProperties(point.x, point.y, true, true);
                    sharpMapsIgnoringClosedDoors[level].SetCellProperties(point.x, point.y, true, true);
                    sharpMapsIgnoringClosedDoorsAndTerrain[level].SetCellProperties(point.x, point.y, true, true);
                    sharpMapsIgnoringClosedDoorsAndLocks[level].SetCellProperties(point.x, point.y, true, true);
                    break;
            }
        }

        public void updateMap(int level, Point point, PathingTerrain newTerrain, Pathing.PathingPermission permission, bool ignoreDangerousTerrain)
        {

            if (sharpMaps.Count() == 0)
            {
                LogFile.Log.LogEntryDebug("updateMap called before pathfinding initially done.", LogDebugLevel.Medium);
                return;
            }

            RogueSharp.Map mapToUse;

            switch (permission)
            {
                case Pathing.PathingPermission.Normal:
                    if (ignoreDangerousTerrain)
                        mapToUse = sharpMapsIgnoringTerrain[level];
                    else
                        mapToUse = sharpMaps[level];
                    break;
                case Pathing.PathingPermission.IgnoreDoors:
                    if (ignoreDangerousTerrain)
                        mapToUse = sharpMapsIgnoringClosedDoorsAndTerrain[level];
                    else
                        mapToUse = sharpMapsIgnoringClosedDoors[level];
                    break;
                case Pathing.PathingPermission.IgnoreDoorsAndLocks:
                    mapToUse = sharpMapsIgnoringClosedDoorsAndLocks[level];
                    break;
                default:
                    mapToUse = sharpMaps[level];
                    break;
            }

            switch (newTerrain)
            {
                case PathingTerrain.ClosedLock:
                    if (mapToUse != sharpMapsIgnoringClosedDoorsAndLocks[level])
                        mapToUse.SetCellProperties(point.x, point.y, true, true);
                    break;
                case PathingTerrain.ClosedDoor:
                    if (mapToUse == sharpMaps[level] || mapToUse == sharpMapsIgnoringTerrain[level])
                    {
                        mapToUse.SetCellProperties(point.x, point.y, true, false);
                    }
                    break;
                case PathingTerrain.Unwalkable:
                    mapToUse.SetCellProperties(point.x, point.y, true, false);
                    break;
                case PathingTerrain.Walkable:
                    mapToUse.SetCellProperties(point.x, point.y, true, true);
                    break;
            }
        }

        /// <summary>
        /// Return path from origin to dest.
        /// Calling functions currently assume that the path list will be a 1 member list containing the origin if there is no path
        /// </summary>
        public List<Point> pathNodes(int level, Point origin, Point dest, Pathing.PathingPermission permission, bool ignoreDangerousTerrain)
        {
            List<Point> returnNodes = new List<Point>();
            returnNodes.Add(origin);

            RogueSharp.Map mapToUse;

            if (origin.x < 0 || origin.y < 0 ||
                origin.x >= levelTCODMapsSize[level].width || origin.y >= levelTCODMapsSize[level].height ||
                dest.x < 0 || dest.y < 0 ||
                dest.x >= levelTCODMapsSize[level].width || dest.y >= levelTCODMapsSize[level].height)
            {
                return returnNodes;
            }

            if (origin.x == dest.x && origin.y == dest.y)
            {
                return returnNodes;
            }

            switch (permission)
            {
                case Pathing.PathingPermission.Normal:
                    if (ignoreDangerousTerrain)
                        mapToUse = sharpMapsIgnoringTerrain[level];
                    else
                        mapToUse = sharpMaps[level];
                    break;
                case Pathing.PathingPermission.IgnoreDoors:
                    if (ignoreDangerousTerrain)
                        mapToUse = sharpMapsIgnoringClosedDoorsAndTerrain[level];
                    else
                        mapToUse = sharpMapsIgnoringClosedDoors[level];
                    break;
                case Pathing.PathingPermission.IgnoreDoorsAndLocks:
                    mapToUse = sharpMapsIgnoringClosedDoorsAndLocks[level];
                    break;
                default:
                    mapToUse = sharpMaps[level];
                    break;
            }

            try
            {
                PathFinder finder = new PathFinder(mapToUse, 1.02);
                Path path = finder.ShortestPath(mapToUse.GetCell(origin.x, origin.y), mapToUse.GetCell(dest.x, dest.y));

                //Exclude start cell
                foreach (Cell step in path.Steps.Skip(1))
                {
                    returnNodes.Add(new Point(step.X, step.Y));
                }

                return returnNodes;
            }
            catch (PathNotFoundException)
            {
                return returnNodes;
            }
        }
    }


}
