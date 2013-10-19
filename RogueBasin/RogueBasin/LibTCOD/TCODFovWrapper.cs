
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.LibTCOD
{
    public class TCODFovWrapper
    {
        Dictionary<int, TCODFov> levelTCODMaps;

        public TCODFovWrapper()
        {
            levelTCODMaps = new Dictionary<int, TCODFov>();
        }

        public void addMap(int level, int width, int height)
        {
            levelTCODMaps[level] = new TCODFov(width, height);
        }

        public TCODFov getMap(int level)
        {
            return levelTCODMaps[level];
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
        }

        public bool CheckTileFOV(int level, Point pointToCheck)
        {
            return levelTCODMaps[level].CheckTileFOV(pointToCheck.x, pointToCheck.y);
        }

        public void CalculateFOV(int level, Point origin, int sightRange)
        {
            levelTCODMaps[level].CalculateFOV(origin.x, origin.y, sightRange);
        }

    }
}
