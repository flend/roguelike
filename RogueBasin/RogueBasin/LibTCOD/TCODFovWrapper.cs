
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.LibTCOD
{
    public class TCODFovWrapper : Algorithms.IFieldOfView
    {
        public class WidthHeight {
            public readonly int width;
            public readonly int height;

            public WidthHeight(int w, int h) {
                this.width = w;
                this.height = h;
            }
        }

        Dictionary<int, TCODFov> levelTCODMaps;
        Dictionary<int, WidthHeight> levelTCODMapSizes;

        public TCODFovWrapper()
        {
            levelTCODMaps = new Dictionary<int, TCODFov>();
            levelTCODMapSizes = new Dictionary<int, WidthHeight>();
        }

        public void updateFovMap(int level, FovMap fovMap)
        {
            TCODFov tcodLevel = new TCODFov(fovMap.Width, fovMap.Height);

            for (int j = 0; j < fovMap.Width; j++)
            {
                for (int k = 0; k < fovMap.Height; k++)
                {
                    if (fovMap.getCell(j, k) != FOVTerrain.Blocking)
                        tcodLevel.SetCell(j, k, true, false);
                    else
                        tcodLevel.SetCell(j, k, false, false);
                }
            }

            levelTCODMaps[level] = tcodLevel;
            levelTCODMapSizes[level] = new WidthHeight(fovMap.Width, fovMap.Height);
        }

        public void updateFovMap(int level, Point point, FOVTerrain newTerrain)
        {
            if (point.x < 0 || point.x >= levelTCODMapSizes[level].width ||
                point.y < 0 || point.y >= levelTCODMapSizes[level].height)
                return;

            levelTCODMaps[level].SetCell(point.x, point.y, newTerrain != FOVTerrain.Blocking, false);
        }

        public bool CheckTileFOV(int level, Point pointToCheck)
        {
            if (pointToCheck.x < 0 || pointToCheck.x >= levelTCODMapSizes[level].width ||
                pointToCheck.y < 0 || pointToCheck.y >= levelTCODMapSizes[level].height)
                return false;

            return levelTCODMaps[level].CheckTileFOV(pointToCheck.x, pointToCheck.y);
        }

        public void CalculateFOV(int level, Point origin, int sightRange)
        {
            levelTCODMaps[level].CalculateFOV(origin.x, origin.y, sightRange);
        }

    }
}
