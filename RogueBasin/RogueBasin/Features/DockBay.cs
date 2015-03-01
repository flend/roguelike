using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Features
{
    public class DockBay : DecorationFeature
    {
        public DockBay()
        {
        }

        protected override char GetRepresentation()
        {
            return '\xe8';
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.Cyan;
        }
    }
}
