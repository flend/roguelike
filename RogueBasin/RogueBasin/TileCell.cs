using RogueBasin;

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
            Displayed = true;
        }

        public Animation(int durationMS, int delayMS)
        {
            DurationMS = durationMS;
            DelayMS = delayMS;
            Displayed = false;
        }

        public bool Displayed { get; set; }
        public int DelayMS { get; set; }
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
        string tileSprite = null;

        public string TileSprite
        {
            get { return tileSprite; }
            set { tileSprite = value; }
        }

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

        public double Transparency { get; set; }

        public Animation Animation
        {
            get;
            set;
        }

        public RecurringAnimation RecurringAnimation
        {
            get;
            set;
        }

        public bool IsPresent()
        {
            return tileID != -1 || tileSprite != null;
        }

        public void Reset()
        {
            tileID = -1;
            tileSprite = null;
            Animation = null;
            TileFlag = null;
            Transparency = 0.0;
        }

        public TileCell()
        {
            //-1 default id
            Transparency = 0.0;
        }

        public TileCell(int tileID)
        {
            this.tileID = tileID;
            Transparency = 0.0;
        }

        public TileCell(string tileSprite)
        {
            TileSprite = tileSprite;
            Transparency = 0.0;
        }

        public override string ToString()
        {
            if (tileSprite != null)
            {
                return tileSprite;
            }
            else
            {
                return "tileid:" + tileID;
            }
        }
    }
}
