
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.LibTCOD
{
    class TCODFovWrapper
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
    }
}
