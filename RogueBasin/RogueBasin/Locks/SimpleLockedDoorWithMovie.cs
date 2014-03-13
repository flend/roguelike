﻿using libtcodWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin.Locks
{
    public class SimpleLockedDoorWithMovie : SimpleLockedDoor
    {
        private string openMovie;
        private string cantOpenMovie;

        public SimpleLockedDoorWithMovie(GraphMap.Door door, string openMovie, string cantOpenMovie) : base(door)
        {
            this.openMovie = openMovie;
            this.cantOpenMovie = cantOpenMovie;
        }

        public override bool OpenLock(Player player)
        {
            bool canDoorBeOpened = CanDoorBeOpenedWithClues(player);

            if (!canDoorBeOpened)
            {
                Screen.Instance.PlayMovie(cantOpenMovie, true);
                return false;
            }
            else
            {
                Screen.Instance.PlayMovie(openMovie, true);
                isOpen = true;
                return true;
            }
        }
    }
}