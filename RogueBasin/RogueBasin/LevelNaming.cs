using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    /// <summary>
    /// Whilst we have the assumption that each level type is only represented once,
    /// this gives the link between LevelTypes and names
    /// </summary>
    public static class LevelNaming
    {
        static public ImmutableDictionary<LevelType, string> LevelNames { get; private set; }

        static LevelNaming()
        {
            var levelNamingDict = new Dictionary<LevelType, string>() {
                { LevelType.ArcologyLevel, "arcology" },
                { LevelType.BridgeLevel, "bridge" },
                { LevelType.CommercialLevel, "commercial" },
                { LevelType.ComputerCoreLevel, "computerCore" },
                { LevelType.FlightDeck, "flightDeck" },
                { LevelType.LowerAtriumLevel, "lowerAtrium" },
                { LevelType.MedicalLevel, "medical" },
                { LevelType.ReactorLevel, "reactor" },
                { LevelType.ScienceLevel, "science" },
                { LevelType.StorageLevel, "storage" }
            };

            LevelNames = levelNamingDict.ToImmutableDictionary(i => i.Key, i => i.Value);
        }
    }
}

