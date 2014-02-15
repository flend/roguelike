using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{

    /** Handles all pathing queries. A friend/extension of Dungeon */
    public class Pathing
    {
        LibTCOD.TCODPathFindingWrapper pathFinding;
        Dungeon dungeon;

        public Pathing(Dungeon dungeon, LibTCOD.TCODPathFindingWrapper pathFinding)
        {
            this.pathFinding = pathFinding;
            this.dungeon = dungeon;
        }

        public LibTCOD.TCODPathFindingWrapper PathFindingInternal
        {
            get
            {
                return pathFinding;
            }
        }

        /// <summary>
        /// Are these points connected, considering only terrain (not monsters)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        /// <returns></returns>
        public bool ArePointsConnected(int level, Point firstPoint, Point secondPoint)
        {
            if (firstPoint == secondPoint)
                return true;

            return pathFinding.arePointsConnected(level, firstPoint, secondPoint, false);
        }

        /// <summary>
        /// Master path finding. Finds a route from origin to dest. Will reroute around Creatures.
        /// Returns the direction to go in (+-xy) for the next step towards the target
        /// If there's no route at all, return -1, -1
        /// If there's a route but its blocked by a creature return the originCreature's coords
        /// 
        /// Needs a lot of state from Dungeon, so users will call dungeon and get passed through
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="dest"></param>
        /// <param name="allDoorsAsOpen">Assume doors are walkable</param>
        /// <returns></returns>
        internal Point GetPathToPoint(int level, Point origin, Point dest, bool allDoorsAsOpen)
        {

            //Try to walk the path
            //If we fail, check if this square occupied by a creature
            //If so, make that square temporarily unwalkable and try to re-route

            List<Point> blockedSquares = new List<Point>();
            bool goodPath = false;
            bool pathBlockedByCreatureOrLock = false;
            Point nextStep = new Point(-1, -1);

            //Check for pathing to own square - return blocked but not terminally
            if (origin == dest)
            {
                LogFile.Log.LogEntryDebug("Monster trying to path to monster on same square", LogDebugLevel.High);
                return origin;
            }

            do
            {

                //Generate path object
                //We actually only need the next point here
                List<Point> pathNodes = pathFinding.pathNodes(level, origin, dest, allDoorsAsOpen);

                //No path
                if (pathNodes.Count == 1)
                {
                    //If there was no blocking creature then there is no possible route (hopefully impossible in a fully connected dungeon)
                    if (!pathBlockedByCreatureOrLock)
                    {
                        //This gets thrown a lot mainly when you cheat
                        LogFile.Log.LogEntryDebug("Path blocked by terrain!", LogDebugLevel.High);
                        return new Point(-1, -1);
                    }
                    else
                    {
                        //All paths are blocked by creatures, we will return the origin creature's location
                        nextStep = origin;
                        goodPath = true;
                        //Exits loop and allows cleanup
                        continue;
                    }
                }

                //Non-null, find next step (0th index is origin)
                Point theNextStep = pathNodes[1];

                //Check if that square is occupied
                Creature blockingCreature = null;

                foreach (Monster creature in dungeon.Monsters)
                {
                    if (creature.LocationLevel != level)
                        continue;

                    //Is it at the destination? If so, that's the target creature and it is our goal.
                    if (creature.LocationMap == dest)
                        continue;

                    //Another creature is blocking
                    if (creature.LocationMap == theNextStep)
                    {
                        blockingCreature = creature;
                    }
                }

                //Do the same for the player (if the creature is chasing another creature around the player)
                //Ignore if the player is the target - that's a valid move

                if (!(dungeon.Player.LocationMap == dest))
                {
                    if (dungeon.Player.LocationLevel == level && dungeon.Player.LocationMap == theNextStep)
                    {
                        blockingCreature = dungeon.Player;
                    }
                }

                //Check if there is a blocking lock
                var locksInSquare = dungeon.NonOpenLocksAtLocation(level, theNextStep);

                //If no blocking creature or lock, the path is good
                if (blockingCreature == null && !locksInSquare)
                {
                    goodPath = true;
                    nextStep = theNextStep;
                }
                else
                {
                    //Otherwise, there's a blocking creature. Make his square unwalkable temporarily and try to reroute
                    pathBlockedByCreatureOrLock = true;

                    pathFinding.updateMap(level, theNextStep, PathingTerrain.Unwalkable);

                    //Add this square to a list of squares to put back
                    blockedSquares.Add(theNextStep);

                    //We will try again
                }
            } while (!goodPath);

            //Put back any squares we made unwalkable
            foreach (Point sq in blockedSquares)
            {
                pathFinding.updateMap(level, sq, PathingTerrain.Walkable);
            }

            return nextStep;
        }

        /// <summary>
        /// Checks if location is in the connected part of the dungeon. Checked by routing a path from the up or entry stairs
        /// </summary>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool CheckInConnectedPartOfMap(int level, Point location)
        {
            //Level nature
            //if (levels[level].GuaranteedConnected)
            //    return true;

            //Find downstairs
            Features.StaircaseUp upStairs = null;
            Features.StaircaseExit entryStairs = null;
            Point upStairlocation = new Point(0, 0);
            Point entryStairlocation = new Point(0, 0);


            foreach (Feature feature in dungeon.Features)
            {
                if (feature.LocationLevel == level &&
                    feature is Features.StaircaseUp)
                {
                    upStairs = feature as Features.StaircaseUp;
                    upStairlocation = feature.LocationMap;
                }

                if (feature.LocationLevel == level &&
                    feature is Features.StaircaseExit)
                {
                    entryStairs = feature as Features.StaircaseExit;
                    entryStairlocation = feature.LocationMap;
                }
            }

            //We don't have downstairs, warn but return true
            if (upStairs == null && entryStairs == null)
            {
                LogFile.Log.LogEntryDebug("CheckInConnectedPartOfMap called on level with no downstairs", LogDebugLevel.Medium);
                return true;
            }

            bool toUp = ArePointsConnected(level, location, upStairlocation);
            bool toEntry = ArePointsConnected(level, location, entryStairlocation);

            if (toUp || toEntry)
                return true;

            return false;
        }

        /// <summary>
        /// Returns the direction to go in (+-xy) for the next step towards the target
        /// If there's no route at all, return -1, -1. Right now we throw an exception for this, since it shouldn't happen in a connected dungeon
        /// If there's a route but its blocked by a creature return the originCreature's coords
        /// 
        /// NB: This is rather inefficient as it recalculates the route each time. Probably the creature should continue on the same route unless something changes
        /// </summary>
        /// <param name="originCreature"></param>
        /// <param name="destCreature"></param>
        /// <returns></returns>
        internal Point GetPathToCreature(Creature originCreature, Creature destCreature)
        {
            //If on different levels it's an error
            if (originCreature.LocationLevel != destCreature.LocationLevel)
            {
                string msg = originCreature.Representation + " not on the same level as " + destCreature.Representation;
                LogFile.Log.LogEntry(msg);
                throw new ApplicationException(msg);
            }

            return GetPathToPoint(originCreature.LocationLevel, originCreature.LocationMap, destCreature.LocationMap, false);
        }

        /// <summary>
        /// Returns the direction to go in (+-xy) for the next step towards the target
        /// If there's no route at all, return -1, -1
        /// If there's a route but its blocked by a creature return the originCreature's coords
        /// Use the map which assumes doors are all open
        /// </summary>
        /// <param name="originCreature"></param>
        /// <param name="destCreature"></param>
        /// <returns></returns>
        internal Point GetPathToPointIgnoreClosedDoors(int level, Monster originCreature, Point dest)
        {
            //If on different levels it's an error
            if (originCreature.LocationLevel != level)
            {
                string msg = originCreature.Representation + " not on the same level as level " + level;
                LogFile.Log.LogEntry(msg);
                throw new ApplicationException(msg);
            }

            return GetPathToPoint(originCreature.LocationLevel, originCreature.LocationMap, dest, true);
        }

        /// <summary>
        /// Returns the direction to go in (+-xy) for the next step towards the target
        /// If there's no route at all, return -1, -1. Right now we throw an exception for this, since it shouldn't happen in a connected dungeon
        /// If there's a route but its blocked by a creature return the originCreature's coords
        /// </summary>
        /// <param name="originCreature"></param>
        /// <param name="destCreature"></param>
        /// <returns></returns>
        internal Point GetPathToCreatureIgnoreClosedDoors(Creature originCreature, Creature destCreature)
        {
            //If on different levels it's an error
            if (originCreature.LocationLevel != destCreature.LocationLevel)
            {
                string msg = originCreature.Representation + " not on the same level as " + destCreature.Representation;
                LogFile.Log.LogEntry(msg);
                throw new ApplicationException(msg);
            }

            return GetPathToPoint(originCreature.LocationLevel, originCreature.LocationMap, destCreature.LocationMap, true);
        }

        /// <summary>
        /// Returns the direction to go in (+-xy) for the next step towards the target
        /// If there's no route at all, return -1, -1
        /// If there's a route but its blocked by a creature return the originCreature's coords
        /// Use the map which assumes doors are all open
        /// </summary>
        /// <param name="originCreature"></param>
        /// <param name="destCreature"></param>
        /// <returns></returns>
        internal Point GetPathFromCreatureToPoint(int level, Monster originCreature, Point dest)
        {
            //If on different levels it's an error
            if (originCreature.LocationLevel != level)
            {
                string msg = originCreature.Representation + " not on the same level as level " + level;
                LogFile.Log.LogEntry(msg);
                throw new ApplicationException(msg);
            }

            return GetPathToPoint(originCreature.LocationLevel, originCreature.LocationMap, dest, false);
        }

    }
}
