﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class ClueAutoPickup : Clue
    {

        public ClueAutoPickup(GraphMap.Clue mapClue) : base(mapClue)
        {
           
        }

        public override bool OnDrop(Creature droppingCreature)
        {
            return Game.Dungeon.Player.PickUpItem(this);
        }
    }
}
