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

    class Animation
    {
        public Animation(int durationMS) {
            DurationMS = durationMS;
        }

        public int DurationMS { get; set; }
        public int CurrentFrame { get; set; }
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

        public Animation Animation
        {
            get;
            set;
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
