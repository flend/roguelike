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


        public bool ShownMovie = false;

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

            if(dungeon.IsWeekday()) {

                //Adventure weekend
                Game.MessageQueue.AddMessage("You're expected in classes during the week.");
            }
            else if(dungeon.IsNormalWeekend()) {

                //Learn combat moves

                List <SpecialMove> notLearntMoves = dungeon.SpecialMoves.FindAll(x => !x.Known);

                //Have we learnt all the moves
                if (notLearntMoves.Count == 0)
                {
                    Game.MessageQueue.AddMessage("Gumball has no more moves to teach you.");
                    return false;
                }

                //Random check sequence
                
                int[] checkSequence = new int[notLearntMoves.Count];
                /* //doesn't work too late to fix
                for (int i = 0; i < checkSequence.Length; i++)
                {
                    do {
                        int rand = Game.Random.Next(checkSequence.Length);

                        for (int j = 0; j < i; j++)
                        {
                            if (checkSequence[j] == rand)
                                continue;
                        }

                        checkSequence[i] = rand;
                        break;
                     }  while(true);
                }*/

                

                
                for (int i = 0; i < checkSequence.Length; i++)
                {
                    checkSequence[i] = i;
                }
                //Actually do the check

                int playerCombatStat = dungeon.Player.AttackStat;

                for (int i = 0; i < checkSequence.Length; i++)
                {
                    SpecialMove thisMove = notLearntMoves[checkSequence[i]];

                    int combatDiff = thisMove.GetRequiredCombat() - playerCombatStat;
                    if (combatDiff < 0)
                        combatDiff = 0;

                    int chanceToLearn = 20 - combatDiff;
                    
                    int roll = Game.Random.Next(20);

                    LogFile.Log.LogEntryDebug("Chance to learn : " + thisMove.MoveName() + " roll: " + roll.ToString() + " " + chanceToLearn.ToString() + "/20", LogDebugLevel.Medium);

                    if (roll < chanceToLearn)
                    {
                        //Learn move
                        //prob won't work cos of type checking
                        //Game.Dungeon.LearnMove(thisMove);


                        thisMove.Known = true;
                        LogFile.Log.LogEntryDebug("Player learnt move: " + thisMove.MoveName(), LogDebugLevel.Medium);

                        //Play movie
                        Screen.Instance.PlayMovie("succeededToLearnMove", false);
                        Screen.Instance.PlayMovie(thisMove.MovieRoot(), false);

                        Game.MessageQueue.AddMessage("You learn a new combat move: " + thisMove.MoveName() + "!");

                        //Hack for wallvalt
                        SpecialMoves.VaultBackstab backStab = thisMove as SpecialMoves.VaultBackstab;
                        if (backStab != null)
                        {
                            dungeon.LearnMove(new SpecialMoves.WallVault());
                        }

                        if (!ShownMovie && dungeon.Player.PlayItemMovies)
                        {
                            ShownMovie = true;
                            Screen.Instance.PlayMovie("helpspecialmoves", false);
                        }

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
            Screen.Instance.UpdateNoMsgQueue();

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
                        Screen.Instance.UpdateNoMsgQueue();
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
