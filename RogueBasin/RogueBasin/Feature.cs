using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Non-pickupable objects in the dungeon
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(Features.StaircaseDown))]
    [System.Xml.Serialization.XmlInclude(typeof(Features.StaircaseUp))]
    [System.Xml.Serialization.XmlInclude(typeof(Features.Corpse))]
    [System.Xml.Serialization.XmlInclude(typeof(Features.DockBay))]
    [System.Xml.Serialization.XmlInclude(typeof(Features.StaircaseEntry))]
    [System.Xml.Serialization.XmlInclude(typeof(Features.StaircaseExit))]
    public abstract class Feature : MapObject
    {

        public Feature()
        {
            IsBlocking = false;
        }

        public bool IsBlocking { get; set; }

        public virtual string Description
        {
            get
            {
                return "Feature";
            }
        }
    }
}
