using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Items
{
    class Potion : Item
    {
        public Potion()
        {

        }

        public override int GetWeight()
        {
            return 10;
        }

        public override string SingleItemDescription
        {
            get { return "potion"; }
        }

        public override string GroupItemDescription
        {
            get { return "potions"; }
        }
    }
}
