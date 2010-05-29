using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Message queue that will be presented to the user before (creatures etc.) and after (their actions) their turn
    /// </summary>
    public class MessageQueue
    {
        List<string> messages;

        /// <summary>
        /// Require a keypress at the end of the message display
        /// </summary>
        public bool RequireKeypress { get; set; }

        public MessageQueue()
        {
            messages = new List<string>();
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
    }
}
