using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    static class ItemMapping
    {
        public static readonly Dictionary<int, Type> WeaponMapping = new Dictionary<int, Type> {
            
            { 1, typeof(Items.Fists)},
            { 2, typeof(Items.Pistol) },
            { 3, typeof(Items.Shotgun) },
            { 4, typeof(Items.Laser) },
            { 5, typeof(Items.AssaultRifle) },
            { 6, typeof(Items.FragGrenade) },
            { 7, typeof(Items.StunGrenade) },
            { 8, typeof(Items.SoundGrenade) },
            { 9, typeof(Items.NanoRepair) },
        };

        public static readonly Dictionary<char, Type> WetwareMapping = new Dictionary<char, Type> {
            
            { 'D', typeof(Items.StealthWare) },
            { 'S', typeof(Items.ShieldWare) },
            { 'A', typeof(Items.AimWare) },
            { 'B', typeof(Items.BoostWare) }
        };
    }
}
