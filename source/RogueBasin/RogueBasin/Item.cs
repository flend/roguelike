using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Base class for all types of pickup-able items
    /// </summary>
    class Item
    {
        public Item() { }

        /// <summary>
        /// ASCII character
        /// </summary>
        char representation;

        public char Representation
        {
            get
            {
                return representation;
            }
            set
            {
                representation = value;
            }
        }
    }
}
