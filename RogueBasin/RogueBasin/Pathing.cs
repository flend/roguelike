using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{

    /** Handles all pathing queries. A friend/extension of Dungeon */
    public class Pathing
    {
        public enum PathingType
        {
            Normal, CreaturePass
        }

        public enum PathingPermission
        {
            Normal, IgnoreDoors, IgnoreDoorsAndLocks
        }

        Algorithms.IPathFinder pathFinding;
        Dungeon dungeon;

        public Pathing(Dungeon dungeon, Algorithms.IPathFinder pathFinding)
        {
            this.pathFinding = pathFinding;
            this.dungeon = dungeon;
        }

        public Algorithms.IPathFinder PathFindingInternal
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
        public bool ArePointsConnected(int level, Point firstPoint, Point secondPoint, PathingPermission permission)
        {
            if (firstPoint == secondPoint)
                return true;

            return pathFinding.arePointsConnected(level, firstPoint, secondPoint, permission);
        }

        /// <summary>
        /// General pathing finding function.
        /// allDoorsAsOpen - assume all doors are open (for creatures who can open doors)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="origin"></param>
        /// <param name="dest"></param>
        /// <param name="allDoorsAsOpen"></param>
        /// <param name="attackDestination"></param>
        /// <returns></returns>
        private PathingResult GetPathToPoint(int level, Point origin, Point dest, PathingPermission permission, bool ignoreDangerousTerrain)
        {
            //Try to walk the path
            //If we fail, check if this square occupied by a creature
            //If so, make that square temporarily unwalkable and try to re-route

            List<Point> blockedSquares = new List<Point>();
            bool goodPath = false;
            bool pathBlockedByCreatureOrLock = false;
            Point nextStep = new Point(-1, -1);
            bool interaction = false;
            bool tryBackupRoute = false;

            //Check for pathing to own square - return blocked but not terminally
            if (origin == dest)
            {
                LogFile.Log.LogEntryDebug("Monster trying to path to monster on same square", LogDebugLevel.High);
                return new PathingResult(origin, false, false);
            }

            do
            {
                //Generate path object
                //We actually only need the next point here
                List<Point> pathNodes = pathFinding.pathNodes(level, origin, dest, permission, ignoreDangerousTerrain);

                //No path
                if (pathNodes.Count == 1)
                {
                    //If there was no blocking creature then there is no possible route, this may be due to dangerous terrain
                    if (!pathBlockedByCreatureOrLock)
                    {
                        LogFile.Log.LogEntryDebug("Path blocked by dangerous terrain!", LogDebugLevel.High);
                        return new PathingResult(origin, false, false, true);
                    }
                    else
                    {
                        //Wait for blocking creatures
                        nextStep = origin;
                        //Exits loop and allows cleanup
                        break;
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

                    if (creature.LocationMap == theNextStep)
                    {
                        blockingCreature = creature;

                        //Is it at the destination? If so, that's the target creature and it is our goal.
                        if (theNextStep == dest)
                        {
                            interaction = true;
                            goodPath = true;
                            nextStep = origin;
                            break;
                        }
                    }
                }

                //(above break doesn't break the do, just the foreach)
                if (goodPath)
                    break;

                //Do the same for the player (if the creature is chasing another creature around the player)
                //Ignore if the player is the target - that's a valid move

                if (dungeon.Player.LocationLevel == level && dungeon.Player.LocationMap == theNextStep)
                {
                    //Is it at the destination? If so, that's the target creature and it is our goal.
                    if (dungeon.Player.LocationMap == theNextStep && theNextStep == dest)
                    {
                        //All OK, continue, we will return these coords
                        interaction = true;
                        nextStep = origin;
                        break;
                    }

                    blockingCreature = dungeon.Player;
                }

                //If no blocking creature, the path is good
                if (blockingCreature == null)
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

            return new PathingResult(nextStep, interaction, false, tryBackupRoute);
        }

        public class PathingResult {
            
            public Point MonsterFinalLocation { get; private set; }
            public bool MoveIsInteractionWithTarget { get; private set; }
            public bool TerminallyBlocked { get; private set; }
            public bool TryBackupRoute { get; private set; }

            public PathingResult(Point monsterFinalLocation, bool moveIsInteractionWithMonsterAtDest, bool terminallyBlocked) {
                MonsterFinalLocation = monsterFinalLocation;
                MoveIsInteractionWithTarget = moveIsInteractionWithMonsterAtDest;
                TerminallyBlocked = terminallyBlocked;
            }

            public PathingResult(Point monsterFinalLocation, bool moveIsInteractionWithMonsterAtDest, bool terminallyBlocked, bool tryBackupRoute)
            {
                MonsterFinalLocation = monsterFinalLocation;
                MoveIsInteractionWithTarget = moveIsInteractionWithMonsterAtDest;
                TerminallyBlocked = terminallyBlocked;
                TryBackupRoute = tryBackupRoute;
            }
        }

        private PathingResult GetPathToPointPassThroughMonsters(int level, Point origin, Point dest, PathingPermission permission, bool ignoreDangerousTerrain)
        {
            //Check for pathing to own square
            if (origin == dest)
            {
                LogFile.Log.LogEntryDebug("Monster trying to path to monster on same square", LogDebugLevel.High);
                return new PathingResult(origin, false, false);
            }

            //Generate path object
            List<Point> pathNodes = pathFinding.pathNodes(level, origin, dest, permission, ignoreDangerousTerrain);
            //Remove last node if repeated (happens sometimes)
            if (pathNodes.Count > 1)
            {
                if (pathNodes[pathNodes.Count - 1] == pathNodes[pathNodes.Count - 2])
                    pathNodes.RemoveAt(pathNodes.Count - 1);
            }

            //No possible path
            if (pathNodes.Count == 1)
            {
                LogFile.Log.LogEntryDebug("Monster Path Passing blocked by terrain!", LogDebugLevel.High);
                return new PathingResult(origin, false, true, true);
            }

            //Run through the path.
            //We stop at the following conditions:

            //First non-occupied square - place ourselves here

            //Target square: Was last square occupied? If not, go here

            //If so, look for free adjacent squares to the target. Route to the first one of these.
            //Place ourselves on the first free empty square enroute. We are guaranteed that the target square is empty

            int pathPoint = 1;
            do
            {
                Point theNextStep = pathNodes[pathPoint];

                //Check if that square is occupied
                Creature blockingCreature = Game.Dungeon.CreatureAtSpaceIncludingPlayer(level, theNextStep);
                
                if (blockingCreature == null)
                {
                    //Free space on the path, stop here
                    return new PathingResult(theNextStep, false, false);
                }

                pathPoint++;

            } while (pathPoint < pathNodes.Count);

            //Path only consists of origin and destination - this can be an attack
            if (pathNodes.Count() == 2)
            {
                return new PathingResult(origin, true, false);
            }

            //We have a route from the start to our target destination
            //Find an unoccupied square where we can rest

            //Best idea would be to find all squares adjacent to the monster sea then order by distance from destination
            //(flood fill style)

            //For now, just look for a free adjacent location next to each of our path nodes (excluding origin)
            //Case we want is to surround the player

            pathPoint = pathNodes.Count() - 1;
            do
            {
                Point thisPathPoint = pathNodes[pathPoint];

                List<Point> possibleRestSquares;
                if(ignoreDangerousTerrain)
                    possibleRestSquares = Game.Dungeon.GetWalkableAdjacentSquaresFreeOfCreatures(level, thisPathPoint);
                else
                    possibleRestSquares = Game.Dungeon.GetWalkableAdjacentSquaresFreeOfCreaturesAndDangerousTerrain(level, thisPathPoint);

                if (possibleRestSquares.Any())
                {
                    var destSquare = possibleRestSquares.RandomElement();
                    return new PathingResult(destSquare, false, false);
                }

                pathPoint--;
            } while (pathPoint >= 1);

            //We haven't found a node so far, we're temporarily blocked
            return new PathingResult(origin, false, false);
        }

        internal PathingResult GetPathToCreature(Creature originCreature, Creature destCreature, PathingType type, PathingPermission permission, bool ignoreDangerousTerrain)
        {
            //If on different levels it's an error
            if (originCreature.LocationLevel != destCreature.LocationLevel)
            {
                string msg = originCreature.Representation + " not on the same level as " + destCreature.Representation;
                LogFile.Log.LogEntry(msg);
                throw new ApplicationException(msg);
            }

            return GetPathToPoint(originCreature.LocationLevel, originCreature.LocationMap, destCreature.LocationMap, type, permission, ignoreDangerousTerrain);
        }

        public PathingResult GetPathToPoint(int level, Point origin, Point dest, PathingType pathingType, PathingPermission permission, bool ignoreDangerousTerrain)
        {
            switch (pathingType)
            {
                case PathingType.CreaturePass:

                    PathingResult result = GetPathToPointPassThroughMonsters(level, origin, dest, permission, ignoreDangerousTerrain);
                    if (result.TryBackupRoute)
                    {
                        LogFile.Log.LogEntryDebug("GetPathToPoint: CreaturePass falling back to normal routing", LogDebugLevel.Medium);
                        return GetPathToPointDefault(level, origin, dest, permission, ignoreDangerousTerrain);
                    }
                    return result;

                default:
                    return GetPathToPointDefault(level, origin, dest, permission, ignoreDangerousTerrain);
            }
        }

        private PathingResult GetPathToPointDefault(int level, Point origin, Point dest, PathingPermission permission, bool ignoreDangerousTerrain)
        {
            var result = GetPathToPoint(level, origin, dest, permission, ignoreDangerousTerrain);

            if (result.TryBackupRoute)
            {
                LogFile.Log.LogEntryDebug("GetPathToPoint: DangerousTerrain falling back to normal routing", LogDebugLevel.Medium);
                //Just ignore dangerous terrain if it didn't work out being careful
                return GetPathToPoint(level, origin, dest, permission, true);
            }

            return result;
        }

    }
}
