using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    /// <summary>
    /// Message queue that will be presented to the user before (creatures etc.) and after (their actions) their turn
    /// </summary>
    public class MessageQueue
    {
        /// <summary>
        /// Contains a list of wrapped strings for the history. Public so serializable
        /// </summary>
        public LinkedList<string> messageHistory;

        const int messageHistorySize = 1000;
                
        List<string> messages;
        
        /// <summary>
        /// Require a keypress at the end of the message display
        /// </summary>
        public bool RequireKeypress { get; set; }

        public MessageQueue()
        {
            messages = new List<string>();
            messageHistory = new LinkedList<string>();
            RequireKeypress = false;
        }

        public void AddMessage(string newMessage) {
            messages.Add(newMessage);
        }

        /// <summary>
        /// Return a new list containing the messages. The list in the class can be cleared any time after this
        /// </summary>
        /// <returns></returns>
        public List<string> GetMessages()
        {
            List<string> returnedList = new List<string>(messages);

            if (RequireKeypress)
                returnedList.Add("<any key>");
   
            return returnedList;
        }

        public void ClearList()
        {
            messages.Clear();
            RequireKeypress = false;
        }

        /// <summary>
        /// Add a string (maybe be multiline) to the history
        /// </summary>
        /// <param name="msgStringToStore"></param>
        private void AddToHistory(string msgStringToStore) {

            //Make sure the msg is broken up into one string per line
            string [] separateStrings = msgStringToStore.Split('\n');

            foreach (string s in separateStrings)
            {
                messageHistory.AddLast(s.Trim());
            }

            while(messageHistory.Count > messageHistorySize) {          
                messageHistory.RemoveFirst();
            }
        }

        /// <summary>
        /// Run through the messages for the user and require a key press after each one
        /// </summary>
        public void RunMessageQueue()
        {
            List<string> messages = Game.MessageQueue.GetMessages();

            Screen.Instance.ClearMessageLine();

            if (messages.Count == 0)
            {
                Screen.Instance.FlushConsole();
                return;
            }

            if (messages.Count == 1)
            {
                //Single message just print it
                Screen.Instance.PrintMessage(messages[0]);

                //Add to history
                AddToHistory(messages[0]);

                Game.MessageQueue.ClearList();

                Screen.Instance.FlushConsole();
                return;
            }

            //Stick all the messages together in one long string
            string allMsgs = "";
            foreach (string message in messages)
            {
                allMsgs += message + " ";
            }

            //Strip off the last piece of white space
            allMsgs = allMsgs.Trim();

            //Wrap the lines to the console width
            List<string> wrappedMsgs = new List<string>();
            do
            {
                string trimmedMsg = Utility.SubstringWordCut(allMsgs, "", 83);
                wrappedMsgs.Add(trimmedMsg);

                //make our allMsgs smaller
                allMsgs = allMsgs.Substring(trimmedMsg.Length);
            } while (allMsgs.Length > 0);

            int noLines = Screen.Instance.msgDisplayNumLines;

            int i = 0;
            do
            {
                //Require moreing
                if (i < wrappedMsgs.Count - noLines)
                {
                    //Add the messages together for PrintMessage
                    string outputMsg = "";

                    for (int j = 0; j < noLines; j++)
                    {
                        outputMsg += wrappedMsgs[i + j].Trim();

                        if (j != noLines - 1)
                            outputMsg += "\n";
                    }

                    //Update line counter
                    i += noLines;

                    outputMsg.Trim();

                    //Add to history
                    AddToHistory(outputMsg);

                    //Show on screen

                    Screen.Instance.PrintMessage(outputMsg + " <more>");
                    Screen.Instance.FlushConsole();

                    //Block for this keypress - may want to listen for exit too
                    KeyPress userKey;
                    userKey = Keyboard.WaitForKeyPress(true);
                }
                else
                {
                    //Add the messages together for PrintMessage
                    string outputMsg = "";

                    for (int j = 0; j < noLines; j++)
                    {
                        if (i + j >= wrappedMsgs.Count)
                            break;

                        outputMsg += wrappedMsgs[i + j].Trim();

                        if (j != noLines - 1)
                            outputMsg += "\n";
                    }

                    outputMsg.Trim();

                    //Update line counter
                    i += noLines;

                    //Add to history
                    AddToHistory(outputMsg);

                    //Show on screen

                    Screen.Instance.PrintMessage(outputMsg);
                    Screen.Instance.FlushConsole();
                }
            } while (i < wrappedMsgs.Count);

            //Require a keypress if requested
            if (Game.MessageQueue.RequireKeypress)
            {
                Keyboard.WaitForKeyPress(true);
            }

            Game.MessageQueue.ClearList();

        }
    }
}
