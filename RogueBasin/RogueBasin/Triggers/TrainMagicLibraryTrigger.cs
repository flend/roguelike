using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Magic library
    /// </summary>
    public class TrainMagicLibraryTrigger : DungeonSquareTrigger
    {
       
        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public static bool Triggered { get; set; }

        public TrainMagicLibraryTrigger()
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
                string movieName = "trainmagiclibrary";

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

                //To do

                //Learn spells

                List<Spell> notLearntSpells = dungeon.Spells.FindAll(x => !x.Known);

                //Have we learnt all the moves
                if (notLearntSpells.Count == 0)
                {
                    Game.MessageQueue.AddMessage("You search around the library for several hours but you realise you have learnt all you can from here.");
                    return false;
                }

                //Random check sequence
                int[] checkSequence = new int[notLearntSpells.Count];

                for (int i = 0; i < checkSequence.Length; i++)
                {
                    checkSequence[i] = Game.Random.Next(checkSequence.Length);
                }

                //Actually do the check

                int playerMagicStat = dungeon.Player.MagicStat;

                for (int i = 0; i < checkSequence.Length; i++)
                {
                    Spell thisSpell = notLearntSpells[checkSequence[i]];

                    int magicDiff = thisSpell.GetRequiredMagic() - playerMagicStat;
                    if (magicDiff < 0)
                        magicDiff = 0;

                    int chanceToLearn = 20 - magicDiff;

                    int roll = Game.Random.Next(20);

                    LogFile.Log.LogEntryDebug("Chance to learn : " + thisSpell.SpellName() + " roll: " + roll.ToString() + " " + chanceToLearn.ToString() + "/20", LogDebugLevel.Medium);

                    if (roll < chanceToLearn)
                    {
                        //Learn move
                        //prob won't work cos of type checking
                        //Game.Dungeon.LearnMove(thisMove);


                        thisSpell.Known = true;
                        LogFile.Log.LogEntryDebug("Player learnt move: " + thisSpell.SpellName(), LogDebugLevel.Medium);

                        //Play movie
                        Screen.Instance.PlayMovie("succeededToLearnSpell", false);
                        Screen.Instance.PlayMovie(thisSpell.MovieRoot(), false);

                        Game.MessageQueue.AddMessage("You learn a new spell: " + thisSpell.SpellName() + "!");
                        dungeon.MoveToNextDate();
                        //Teleport the user back to the start location
                        Game.Dungeon.PlayerBackToTown();

                        return true;
                    }

                }

                //If we get here we haven't learnt a move

                Screen.Instance.PlayMovie("failedToLearnSpell", false);
                dungeon.MoveToNextDate();
                //Teleport the user back to the start location
                Game.Dungeon.PlayerBackToTown();

                return true;
            }
            else {

                //Adventure weekend
                Game.MessageQueue.AddMessage("Surely there's something better to do this weekend!");
            }

            if (doesTraining)
            {
                RunTrainingUI();
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
