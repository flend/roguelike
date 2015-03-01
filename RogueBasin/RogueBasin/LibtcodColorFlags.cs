using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    class LibtcodColorFlags : TileEngine.TileFlags
    {
        System.Drawing.Color foregroundColor;
        System.Drawing.Color backgroundColor;

        /// <summary>
        /// Foreground color only
        /// </summary>
        /// <param name="foregroundColor"></param>
        public LibtcodColorFlags(System.Drawing.Color foregroundColor)
        {
            this.foregroundColor = foregroundColor;
        }

        /// <summary>
        /// Background color only
        /// </summary>
        public LibtcodColorFlags(System.Drawing.Color foregroundColor, System.Drawing.Color backgroundColor)
        {
            this.foregroundColor = foregroundColor;
            this.backgroundColor = backgroundColor;
        }

        public System.Drawing.Color ForegroundColor {
            get { return foregroundColor; }
        }

        public System.Drawing.Color BackgroundColor
        {
            get { return backgroundColor; }
        }

    }
}
