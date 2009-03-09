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

        public MessageQueue()
        {
            messages = new List<string>();
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
   
            return returnedList;
        }

        public void ClearList()
        {
            messages.Clear();
        }
    }
}
