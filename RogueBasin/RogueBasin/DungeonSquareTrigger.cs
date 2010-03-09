using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Function that triggers when the PC moves into a particular square
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.DungeonEntranceTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.HelpFriend))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.LichBattle))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.SeeCorpses))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.SpotFriend))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TreasureRoom))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainAthleticsTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainCharmTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainCombatTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainRestTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainMagicTrigger))]
    public abstract class DungeonSquareTrigger
    {
        public int Level { get; set; }
        public Point mapPosition { get; set; }

        
        public DungeonSquareTrigger()
        {

        }

        /// <summary>
        /// Run the trigger. Really responsible for its own stage and retriggering.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="mapLocation"></param>
        /// <returns></returns>
        public abstract bool CheckTrigger(int level, Point mapLocation);

        protected bool CheckLocation(int level, Point mapLocation)
        {

            //Just check location
            if (Level != level || mapLocation != mapPosition)
                return false;

            return true;
        }
    }
}
