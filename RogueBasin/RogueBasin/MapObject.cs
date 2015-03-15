using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    /// <summary>
    /// Base class for any object that can be represented on the map by level & position
    /// </summary>
    public class MapObject
    {
        /// <summary>
        /// ASCII character
        /// </summary>
        char representation = '\0';

        /// <summary>
        /// Level the object is on
        /// </summary>
        int locationLevel;

        /// <summary>
        /// Point on the map on this level that the object is on
        /// </summary>
        Point locationMap;

        public MapObject()
        {
            SetupAnimationForObject();
        }

        /// <summary>
        /// Level the object is on
        /// </summary>
        public int LocationLevel
        {
            get
            {
                return locationLevel;
            }
            set
            {
                locationLevel = value;
            }
        }

        /// <summary>
        /// Point within the level the object is at
        /// </summary>
        virtual public Point LocationMap
        {
            get
            {
                return locationMap;
            }
            set
            {
               locationMap = value;
            }
        }

        public Location Location
        {
            get
            {
                return new Location(locationLevel, locationMap);
            }
        }

        /// <summary>
        /// Map char. Stored in derived classes but can also be overridden by setting with this
        /// </summary>
        public char Representation
        {
            get
            {
                if (representation == '\0')
                {
                    return GetRepresentation();
                }
                else
                {
                    return representation;
                }
            }
            set
            {
                representation = value;
            }
        }

        public String UISprite
        {
            get
            {
                if (GetUISprite() == null)
                {
                    return null; //better to return a default
                }
                return GetUISprite();
            }
        }

        private String gameSprite = null;

        public String GameSprite
        {
            get
            {
                if (gameSprite != null)
                    return gameSprite;
                else return GetGameSprite();
            }
            protected set { gameSprite = value;  }
        }

        public String GameOverlaySprite
        {
            get
            {
                if (GetGameOverlaySprite() == null)
                {
                    return null; //better to return a default
                }
                return GetGameOverlaySprite();
            }
        }

        /// <summary>
        /// Character for the monster's heading. Can be overriden in derived classes.
        /// </summary>
        public char HeadingRepresentation
        {
            get
            {
                return '*';
            }
        }

        /// <summary>
        /// Colour for representation. Override in derived classes.
        /// </summary>
        /// <returns></returns>
        virtual public System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.White;
        }

        /// <summary>
        /// Colour for representation. Override in derived classes.
        /// </summary>
        /// <returns></returns>
        virtual public System.Drawing.Color RepresentationBackgroundColor()
        {
            return System.Drawing.Color.Black;
        }

        /// <summary>
        /// Get the representation from the derived class
        /// </summary>
        /// <returns></returns>
        protected virtual char GetRepresentation()
        {
            return 'X';
        }

        /// <summary>
        /// Name of the UI sprite, without path or .png
        /// </summary>
        /// <returns></returns>
        protected virtual string GetUISprite()
        {
            return null;
        }

        /// <summary>
        /// Name of the game sprite, without path or .png
        /// </summary>
        /// <returns></returns>
        protected virtual string GetGameSprite()
        {
            return null;
        }

        /// <summary>
        /// Name of the game sprite, without path or .png
        /// </summary>
        /// <returns></returns>
        protected virtual string GetGameOverlaySprite()
        {
            return null;
        }

        /// <summary>
        /// Return true if this object and other are in the same place (level and square)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool InSameSpace(MapObject other)
        {
            if (this.LocationLevel == other.LocationLevel &&
                this.LocationMap == other.LocationMap)
            {
                return true;
            }
            else 
                return false;
        }
        
        /// <summary>
        /// Return true if the object is at the position specified
        /// </summary>
        /// <param name="level"></param>
        /// <param name="locationMap"></param>
        /// <returns></returns>
        public bool IsLocatedAt(int level, Point locationMap)
        {
            if (this.LocationLevel == level && this.LocationMap == locationMap)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool HasAnimation { get; protected set; }
        public int AnimationDelayMS { get; protected set; }
        public int NumberOfFrames { get; protected set; }
        public int CurrentFrame { get; protected set; }
        public int CurrentTick { get; protected set; }

        protected virtual void SetupAnimationForObject() { HasAnimation = false; }
        
        public bool IncrementAnimation(int inc)
        {
            if (!HasAnimation)
                return false;

            CurrentTick += inc;
            if (CurrentTick > AnimationDelayMS)
            {
                CurrentTick -= AnimationDelayMS;
                CurrentFrame++;
                if (CurrentFrame >= NumberOfFrames)
                    CurrentFrame = 0;

                return true;
            }

            return false;
        }

        public RecurringAnimation GetAnimation()
        {
            var anim = new RecurringAnimation();
            anim.FrameNo = CurrentFrame;
            return anim;
        }

    }
}
