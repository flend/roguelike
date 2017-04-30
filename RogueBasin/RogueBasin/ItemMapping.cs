using System;
using System.Collections.Generic;

namespace RogueBasin
{
    static class ItemMapping
    {
        public static readonly Dictionary<int, Type> WeaponMapping = new Dictionary<int, Type> {
            
            { 1, typeof(Items.Pistol) },
            { 2, typeof(Items.Shotgun) },
            { 3, typeof(Items.Laser) },
            { 4, typeof(Items.AssaultRifle) },
            { 5, typeof(Items.FragGrenade) },
            { 6, typeof(Items.StunGrenade) },
            { 7, typeof(Items.SoundGrenade) },
            { 8, typeof(Items.NanoRepair) },
        };

        public static readonly Dictionary<char, Type> WetwareMapping = new Dictionary<char, Type> {
            
            { 'd', typeof(Items.StealthWare) },
            { 'v', typeof(Items.ShieldWare) },
            { 'x', typeof(Items.AimWare) },
            { 'z', typeof(Items.BoostWare) },
            { 'c', typeof(Items.BioWare) }
        };
    }
}
