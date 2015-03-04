using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace RogueBasin
{
    class LibtcodColorFlags : TileEngine.TileFlags
    {
        Color foregroundColor;
        Color backgroundColor;
        Color transparentColor = Color.FromArgb(255, 0, 255);

        /// <summary>
        /// Foreground color only
        /// </summary>
        /// <param name="foregroundColor"></param>
        public LibtcodColorFlags(System.Drawing.Color foregroundColor)
        {
            this.foregroundColor = foregroundColor;
            this.backgroundColor = transparentColor;
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
