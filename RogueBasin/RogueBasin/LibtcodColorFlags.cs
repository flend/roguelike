using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    class LibtcodColorFlags : TileEngine.TileFlags
    {
        Color thisColor;

        public LibtcodColorFlags(Color color)
        {
            thisColor = color;
        }

        public Color Color {
           get { return thisColor; }
        }

    }
}
