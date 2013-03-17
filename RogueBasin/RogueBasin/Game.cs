using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    static class Game
    {
        static Dungeon dungeon = null;
        static MessageQueue messages = null;
        static Random rand;

        static Game()
        {
            rand = new Random();
        }
        /// <summary>
        /// Access to the current dungeon
        /// </summary>
        public static Dungeon Dungeon {
            get
            {
                if (dungeon == null)
                {
                    throw new ApplicationException("Dungeon accessed in Game before being created");
                }
                return dungeon;
            }
            set
            {
                dungeon = value;
            }
        }

        public static MessageQueue MessageQueue
        {
            get
            {
                if (messages == null)
                {
                    throw new ApplicationException("MessageQueue accessed in Game before being created");
                }
                return messages;
            }
            set
            {
                messages = value;
            }
        }

        public static Random Random
        {
            get
            {
                return rand;
            }

            set
            {
                rand = value;
            }
        }
    }
}
