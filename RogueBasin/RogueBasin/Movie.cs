using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    public class Movie
    {
        private List<MovieFrame> frames = new List<MovieFrame>();

        public List<MovieFrame> Frames
        {
            get
            {
                return frames;
            }
            set
            {
                frames = value;
            }
        }

        public Movie(List<MovieFrame> frames)
        {
            Frames = frames;
        }
    }
}
