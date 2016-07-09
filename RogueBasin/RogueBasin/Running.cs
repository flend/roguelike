using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    class Running
    {
        RogueBase rogueBase;
        Point runningDirection;
        IEnumerable<Point> runningPath;

        public Running(RogueBase rogueBase) {
            this.rogueBase = rogueBase;
        }

        public bool StartRunning(int directionX, int directionY)
        {
            runningDirection = new Point(directionX, directionY);
            rogueBase.ActionState = ActionState.Running;

            return RunNextStep();
        }

        public bool StartRunning(IEnumerable<Point> path)
        {
            runningDirection = null;
            runningPath = path;
            rogueBase.ActionState = ActionState.Running;

            return RunNextStep();
        }

        public void StopRunning()
        {
            rogueBase.ActionState = ActionState.Interactive;
        }

        public bool RunNextStep()
        {
            Point relativeNextStep = new Point(0, 0);

            if (runningPath != null && runningPath.Any())
            {
                relativeNextStep = runningPath.ElementAt(0) - Game.Dungeon.Player.LocationMap;
                runningPath = runningPath.Skip(1);

                if (!runningPath.Any())
                {
                    //Reached end of path
                    StopRunning();
                }
            }
            else if (runningPath != null && !runningPath.Any())
            {
                //Empty path
                StopRunning();
                return false;
            }
            else if (runningPath == null)
            {
                relativeNextStep = new Point(runningDirection.x, runningDirection.y);
            }

            if (relativeNextStep == new Point(0, 0))
            {
                LogFile.Log.LogEntryDebug("Can't run onto yourself", LogDebugLevel.High);
                StopRunning();
            }

            MoveResults results = Game.Dungeon.PCMove(relativeNextStep.x, relativeNextStep.y);

            switch (results)
            {
                case MoveResults.AttackedMonster:
                    StopRunning();
                    break;
                case MoveResults.InteractedWithFeature:
                    StopRunning();
                    break;
                case MoveResults.InteractedWithObstacle:
                    StopRunning();
                    break;
                case MoveResults.NormalMove:
                    break;
                case MoveResults.StoppedByObstacle:
                    StopRunning();
                    break;
                case MoveResults.SwappedWithMonster:
                    break;
            }

            return Utility.TimeAdvancesOnMove(results);
        }
    }
}
