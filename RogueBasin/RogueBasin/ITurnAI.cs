using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    interface ITurnAI
    {
        /// <summary>
        /// Carry out a turn using the AI
        /// </summary>
        void ProcessTurn();
    }
}
