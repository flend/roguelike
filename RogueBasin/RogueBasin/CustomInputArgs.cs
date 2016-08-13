using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    public enum CustomInputArgsActions
    {
        MouseMoveToCurrentLocation
    }

    class CustomInputArgs
    {
        public readonly CustomInputArgsActions action;

        public CustomInputArgs(CustomInputArgsActions action)
        {
            this.action = action;
        }
    }
}
