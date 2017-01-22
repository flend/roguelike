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
        Player player;
        Point runningDirection;
        IEnumerable<Point> runningPath;

        public Running(RogueBase rogueBase, Player player) {
            this.rogueBase = rogueBase;
            this.player = player;
        }

        public ActionResult StartRunning(int directionX, int directionY)
        {
            runningDirection = new Point(directionX, directionY);
            rogueBase.ActionState = ActionState.Running;

            return RunNextStep();
        }

        public ActionResult StartRunning(IEnumerable<Point> path)
        {
            runningDirection = null;
            runningPath = path;
            rogueBase.ActionState = ActionState.Running;
            player.Running = true;

            return RunNextStep();
        }

        public void StopRunning()
        {
            rogueBase.ActionState = ActionState.Interactive;
            rogueBase.SimulateMouseEventInCurrentPosition();
            player.Running = false;
        }

        public ActionResult RunNextStep()
        {
            Point relativeNextStep = new Point(0, 0);
            var originalRunningPath = runningPath.Select(t => t);

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
                return new ActionResult(false, true);
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
                case MoveResults.OpenedDoor:
                    if (!Screen.Instance.SeeAllMap)
                    {
                        StopRunning();
                    }
                    else
                    {
                        //After a time-free action (opening the door), take the last step again (walking onto the door)
                        //A nicer solution might be to indicate from PCMove if the last step should be retaken
                        runningPath = originalRunningPath;
                    }
                    break;
                case MoveResults.NormalMove:
                    break;
                case MoveResults.StoppedByObstacle:
                    StopRunning();
                    break;
                case MoveResults.SwappedWithMonster:
                    break;
                case MoveResults.StoppedByMonster:
                    StopRunning();
                    break;
            }

            return Utility.TimeAdvancesOnMove(results);
        }
    }
}
