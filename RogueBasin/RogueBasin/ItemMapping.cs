using System;
using System.Collections.Generic;

namespace RogueBasin
{
    static class ItemMapping
    {
        public static readonly Dictionary<int, Type> WeaponMapping = new Dictionary<int, Type> {
            
            { new Items.Pistol().Index(), typeof(Items.Pistol) },
            { new Items.Shotgun().Index(), typeof(Items.Shotgun) },
            { new Items.Laser().Index(), typeof(Items.Laser) },
            { new Items.AssaultRifle().Index(), typeof(Items.AssaultRifle) },
            { new Items.RocketLauncher().Index(), typeof(Items.RocketLauncher) },
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
