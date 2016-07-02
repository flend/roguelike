using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    public class MovieFrame
    {
        private List<string> scanLines;

        public List<string> ScanLines
        {
            get
            {
                return scanLines;
            }
            set
            {
                scanLines = value;
            }
        }

        public MovieFrame()
        {
            scanLines = new List<string>();
        }

        public void AddLine(string line)
        {
            scanLines.Add(line);
        }
    }
}
