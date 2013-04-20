using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TileEngine
{
    class TileFlags
    {
        public TileFlags()
        {

        }
    }

    class TileCell
    {
        int tileID;
        TileFlags flags;

        public int TileID
        {
            get { return tileID; }
            set
            {
                tileID = value;
            }
        }

        public TileFlags TileFlag
        {
            get { return flags; }
            set
            {
                flags = value;
            }
        }


        public TileCell(int tileID)
        {
            this.tileID = tileID;
        }
    }
}
