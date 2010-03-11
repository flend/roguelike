using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Magic library
    /// </summary>
    public class TrainMasterTrigger : DungeonSquareTrigger
    {
        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public static bool Triggered { get; set; }

        public TrainMasterTrigger()
        {
            Triggered = false;
        }

        public override bool CheckTrigger(int level, Point mapLocation)
        {
            //Check we are in the right place
            if (CheckLocation(level, mapLocation) == false)
            {
                return false;
            }
            
            //Otherwise in the right place

            //If this is the first time, give some flavour text - to do
            if (!Triggered)
            {
                string movieName = "trainmaster";

                if (movieName != "")
                    Screen.Instance.PlayMovie(movieName, false);
            }
            Triggered = true;

            Dungeon dungeon = Game.Dungeon;

            bool doesTraining = false;

            if(dungeon.IsWeekday()) {

                //Adventure weekend
                Game.MessageQueue.AddMessage("You're expected in classes during the week.");
            }
            else if(dungeon.IsNormalWeekend()) {

                //Learn combat moves

                


                int[] moveRequiredCombat = { 20, 30, 40, 60, 80, 100 };
                SpecialMove[] movesToLearn = { new SpecialMoves.CloseQuarters(), new SpecialMoves.ChargeAttack(), new SpecialMoves.VaultBackstab(), new SpecialMoves.OpenSpaceAttack(), new SpecialMoves.BurstOfSpeed(), new SpecialMoves.MultiAttack() };

                //Try to learn all unlearn moves. Stop if we succeed

                int playerCombatStat = dungeon.Player.AttackStat;

                for (int i = 0; i < moveRequiredCombat.Length; i++)
                {
                    bool moveKnown = false;

                    //Check if we know this special move already
                    
                    foreach(SpecialMove move in dungeon.SpecialMoves) {
                        //Same move
                        if(Object.ReferenceEquals(move.GetType(), movesToLearn[i].GetType())) {
                            if (move.Known)
                            {
                                moveKnown = true;
                                break;
                            }
                        }
                    }

                    if (moveKnown)
                        continue;

                    int combatDiff = moveRequiredCombat[i] - playerCombatStat;
                    if (combatDiff < 0)
                        combatDiff = 0;

                    int chanceToLearn = 20 - combatDiff;
                    
                    int roll = Game.Random.Next(20);

                    LogFile.Log.LogEntryDebug("Chance to learn : " + movesToLearn[i].MoveName() + " roll: " + roll.ToString() + " " + chanceToLearn.ToString() + "/20" , LogDebugLevel.Medium);

                    if (roll < chanceToLearn)
                    {
                        //Learn move
                        Game.Dungeon.LearnMove(movesToLearn[i]);

                        //Play movie
                        Screen.Instance.PlayMovie("succeededToLearnMove", false);
                        Screen.Instance.PlayMovie(movesToLearn[i].MovieRoot(), false);

                        Game.MessageQueue.AddMessage("You learn a new combat move: " + movesToLearn[i].MoveName());
                        dungeon.MoveToNextDate();
                        //Teleport the user back to the start location
                        Game.Dungeon.PlayerBackToTown();

                        return true;
                    }

                }

                //If we get here we haven't learnt a move

                Screen.Instance.PlayMovie("failedToLearnMove", false);
                dungeon.MoveToNextDate();
                //Teleport the user back to the start location
                Game.Dungeon.PlayerBackToTown();

                return true;
            }
            else {

                //Adventure weekend
                Game.MessageQueue.AddMessage("Surely there's something better to do this weekend!");
            }
            return true;
        }

        protected void RunTrainingUI()
        {
            //Show training UI
            Screen.Instance.DisplayTrainingUI = true;
            Screen.Instance.DrawAndFlush();

            bool continueLooking = true;

            while (continueLooking)
            {
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {
                    char keyCode = (char)userKey.Character;

                    //Exit out of inventory
                    if (keyCode == 'x')
                    {
                        Screen.Instance.DisplayTrainingUI = false;
                        Screen.Instance.DrawAndFlush();
                        continueLooking = false;
                    }
                }
            }

            //Increment calendar time
            Game.Dungeon.MoveToNextDate();

            //Teleport the user back to the start location
            Game.Dungeon.PlayerBackToTown();
        }
    }
}
