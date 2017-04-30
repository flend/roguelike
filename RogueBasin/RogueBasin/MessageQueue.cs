using System.Collections.Generic;

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
        public LinkedList<string> messageHistory { get; set; }

        const int messageHistorySize = 1000;
                
        List<string> messages;

        const int displayCachedMsgTurns = 10;

        public int CachedMsgTurnCount{ get; set; }
        bool showCachedMsg = false;
        string cachedMsg = "";
        System.Drawing.Color cachedMsgColor = System.Drawing.Color.SkyBlue;
        
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

        public List<string> GetMessageHistoryAsList()
        {
            List<string> retList = new List<string>();
            foreach (string s in messageHistory)
                retList.Add(s);

            return retList;
        }

        public void TakeMessageHistoryFromList(List<string> newMsgHistory)
        {
            messageHistory.Clear();

            foreach (string s in newMsgHistory)
                messageHistory.AddLast(s);
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

            //Need to re-wrap to message history screen width
            int wrapToWidth = Screen.Instance.MsgLogWrapWidth;

            //Wrap the lines to the console width
            List<string> wrappedMsgs = new List<string>();
            string workStr = msgStringToStore;
            do
            {
                string trimmedMsg = Utility.SubstringWordCutAndNormalise(workStr, "", (uint)wrapToWidth);
                int charsUsed = trimmedMsg.Length;
                wrappedMsgs.Add(trimmedMsg.Trim());

                //make our allMsgs smaller
                workStr = workStr.Substring(charsUsed);
            } while (workStr.Length > 0);
            
            //Make sure the msg is broken up into one string per line
            //string [] separateStrings = msgStringToStore.Split('\n');

            //foreach (string s in separateStrings)
            //{
            //    if(s.Length > 0)
            //        messageHistory.AddLast(s.Trim());
            //}

            //while(messageHistory.Count > messageHistorySize) {          
            //    messageHistory.RemoveFirst();
            //}

            foreach (string s in wrappedMsgs)
            {
                if (s.Length > 0)
                    messageHistory.AddLast(s.Trim());
            }

            while (messageHistory.Count > messageHistorySize)
            {
                messageHistory.RemoveFirst();
            }
        }

        /// <summary>
        /// Run through the messages for the user and require a key press if long string
        /// </summary>
        public void RunMessageQueue()
        {
            List<string> messages = Game.MessageQueue.GetMessages();

            //Increment no of turns we have shown cached msg. Turn it off if too many
            IncrementCachedMsgCounter();
            if (CachedMsgTurnCount > displayCachedMsgTurns)
                showCachedMsg = false;

            if (messages.Count == 0)
            {
                //If we have a cached msg, show it
                if (showCachedMsg)
                {
                    //The different colors don't work so well now since with the mouse we sometimes seem to take an extra frame
                    Screen.Instance.ClearMessageLine();
                    Screen.Instance.ShowMessageLine(cachedMsg, cachedMsgColor);
                }

                //Don't clear if we don't have anything to display (for prompts etc.)

                return;
            }

            if (messages.Count == 1)
            {
                //Single message just print it
                Screen.Instance.ShowMessageLine(messages[0]);

                //Add to history
                AddToHistory(messages[0]);

                //Set cache
                SetCache(messages[0]);

                Game.MessageQueue.ClearList();

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
                string trimmedMsg = Utility.SubstringWordCut(allMsgs, "", Screen.Instance.MessageQueueWidth);
                int charsUsed = trimmedMsg.Length;
                wrappedMsgs.Add(trimmedMsg.Trim());

                //make our allMsgs smaller
                allMsgs = allMsgs.Substring(charsUsed);
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

                    Screen.Instance.ShowMessageLine(outputMsg + " <more>");

                    //Block for this keypress - may want to listen for exit too
                    //KeyPress userKey;
                    //userKey = Keyboard.WaitForKeyPress(true);
                    //This doesn't work right now
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

                    //Set cache
                    SetCache(outputMsg);

                    //Show on screen

                    Screen.Instance.ShowMessageLine(outputMsg);
                   
                }
            } while (i < wrappedMsgs.Count);


            /*
            //Require a keypress if requested
            if (Game.MessageQueue.RequireKeypress)
            {
                Keyboard.WaitForKeyPress(true);
            }*/

            Game.MessageQueue.ClearList();

        }

        /// <summary>
        /// Set this msg as the last cached msg
        /// </summary>
        /// <param name="p"></param>
        private void SetCache(string p)
        {
            cachedMsg = p;
            CachedMsgTurnCount = 0;
            showCachedMsg = true;
        }


        public void IncrementCachedMsgCounter() {
            CachedMsgTurnCount++;
        }
    }
}
