using System.Linq;

namespace RogueBasin
{
    public static class RoutabilityUtilities
    {
        public static bool CheckItemRouteability()
        {
            var items = Game.Dungeon.Items;
            var features = Game.Dungeon.Features;

            var elevators = features.Where(f => f.GetType() == typeof(RogueBasin.Features.Elevator)).Select(f => f as RogueBasin.Features.Elevator).ToList();

            foreach (var item in items)
            {
                if (item.InInventory)
                    continue;

                var destElevator = elevators.Where(e => e.DestLevel == item.LocationLevel);
                var destElevatorLocations = destElevator.Select(e => (e as RogueBasin.Features.Elevator).DestLocation);

                var srcElevator = features.Where(e => e.LocationLevel == item.LocationLevel);
                var srcElevatorLocations = srcElevator.Select(e => (e as RogueBasin.Features.Elevator).LocationMap);

                var elevatorLocations = destElevatorLocations.Union(srcElevatorLocations);

                if (!elevatorLocations.Any())
                {
                    LogFile.Log.LogEntryDebug("Item " + item.SingleItemDescription + " has no elevator on the same level, map cannot be solved!", LogDebugLevel.High);
                    return false;
                }

                var elevatorToRouteTo = elevatorLocations.First();

                //Check routing between elevator and item
                if (!Game.Dungeon.Pathing.ArePointsConnected(item.LocationLevel, item.LocationMap, elevatorToRouteTo, Pathing.PathingPermission.IgnoreDoorsAndLocks))
                {
                    LogFile.Log.LogEntryDebug("Item " + item.SingleItemDescription + " at " + item.LocationMap + "(" + item.LocationLevel + ")" + "is not connected to elevator at " + elevatorToRouteTo + ", map cannot be solved!", LogDebugLevel.High);
                    return false;
                }
            }

            return true;
        }

        public static bool CheckFeatureRouteability()
        {
            var features = Game.Dungeon.Features;

            var elevators = features.Where(f => f.GetType() == typeof(RogueBasin.Features.Elevator)).Select(f => f as RogueBasin.Features.Elevator).ToList();
            var nonDecorationFeature = features.Where(f => f.GetType() != typeof(RogueBasin.Features.StandardDecorativeFeature));

            foreach (var feature in nonDecorationFeature)
            {
                var destElevator = elevators.Where(e => e.DestLevel == feature.LocationLevel);
                var destElevatorLocations = destElevator.Select(e => (e as RogueBasin.Features.Elevator).DestLocation);

                var srcElevator = features.Where(e => e.LocationLevel == feature.LocationLevel);
                var srcElevatorLocations = srcElevator.Select(e => (e as RogueBasin.Features.Elevator).LocationMap);

                var elevatorLocations = destElevatorLocations.Union(srcElevatorLocations);

                if (!elevatorLocations.Any())
                {
                    LogFile.Log.LogEntryDebug("Feature " + feature.Description + " has no elevator on the same level, map cannot be solved!", LogDebugLevel.High);
                    return false;
                }

                var elevatorToRouteTo = elevatorLocations.First();

                //Check routing between elevator and item
                if (!Game.Dungeon.Pathing.ArePointsConnected(feature.LocationLevel, feature.LocationMap, elevatorToRouteTo, Pathing.PathingPermission.IgnoreDoorsAndLocks))
                {
                    LogFile.Log.LogEntryDebug("Feature " + feature.Description + " at " + feature.LocationMap + "(" + feature.LocationLevel + ")" + "is not connected to elevator at " + elevatorToRouteTo + ", map cannot be solved!", LogDebugLevel.High);
                    return false;
                }
            }

            return true;
        }

    }
}
