
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.LibTCOD
{
    public class TCODFovWrapper : Algorithms.IFieldOfView
    {
        Dictionary<int, TCODFov> levelTCODMaps;

        public TCODFovWrapper()
        {
            levelTCODMaps = new Dictionary<int, TCODFov>();
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

        public void updateFovMap(int level, Point point, FOVTerrain newTerrain)
        {
            levelTCODMaps[level].SetCell(point.x, point.y, newTerrain != FOVTerrain.Blocking, false);
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
