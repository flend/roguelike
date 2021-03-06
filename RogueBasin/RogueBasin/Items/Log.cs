﻿using libtcodWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin.Items
{
    public class Log : Item, IUseableItem
    {
        LogEntry logEntry;

        public Log(LogEntry entry)
        {
            this.logEntry = entry;
        }

        public bool UsedUp
        {
            set { }
            get { return false; }
        }

        public bool Use(Creature user)
        {
            if(Game.Dungeon.Player.PlayItemMovies)
                Screen.Instance.PlayLog(logEntry);
            return true;
        }

        public LogEntry LogEntry
        {
            get { return logEntry; }
        }

        public override int GetWeight()
        {
            return 10;
        }

        public override string SingleItemDescription
        {
            get { return "Log"; }
        }

        public override string GroupItemDescription
        {
            get { return "Logs"; }
        }

        public override System.Drawing.Color GetColour()
        {
            return System.Drawing.Color.LimeGreen;
        }

        protected override char GetRepresentation()
        {
            return (char)381;
        }

        public override bool UseHiddenName { get { return false; } }

        public override string HiddenSuffix
        {
            get
            {
                return "log";
            }
        }

        public override bool OnPickup(Creature pickupCreature)
        {
            return Use(pickupCreature);
        }
    }
}
