using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphMap
{
    public class GraphSolver
    {
        List<Clue> cluesFound;

        MapModel model;

        public GraphSolver(MapModel model)
        {
            this.model = model;
        }

        public bool MapCanBeSolved()
        {
            //Do while locked doors exist
            
            //Find the part of the map that can be accessed using the clues we have
            return false;

            //Collect all new clues in that area


        }
    }
}
