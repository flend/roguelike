using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    public class MovieFrame
    {
        public List<string> scanLines;

        public int width;
        public int height;

        public MovieFrame()
        {
            width = 60;
            height = 25;
        }

        public void AddLine(string line)
        {
            scanLines.Add(line);
        }
    }
}
