using System;
using System.Collections.Generic;
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
        /// <summary>
        /// -1 signifies empty
        /// </summary>
        int tileID = -1;
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

        public TileCell()
        {
            //-1 default id
        }

        public TileCell(int tileID)
        {
            this.tileID = tileID;
        }
    }
}
