using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    class LibtcodColorFlags : TileEngine.TileFlags
    {
        Color foregroundColor;
        Color backgroundColor;

        /// <summary>
        /// Foreground color only
        /// </summary>
        /// <param name="foregroundColor"></param>
        public LibtcodColorFlags(Color foregroundColor)
        {
            this.foregroundColor = foregroundColor;
        }

        /// <summary>
        /// Background color only
        /// </summary>
        public LibtcodColorFlags(Color foregroundColor, Color backgroundColor)
        {
            this.foregroundColor = foregroundColor;
            this.backgroundColor = backgroundColor;
        }

        public Color ForegroundColor {
            get { return foregroundColor; }
        }

        public Color BackgroundColor
        {
            get { return backgroundColor; }
        }

    }
}
