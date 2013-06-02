using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace graphtestc
{
    public sealed class FileDotEngineUndirected : IDotEngine
    {
        public string Run(GraphvizImageType imageType, string dot, string outputFileName)
        {
            string output = outputFileName + ".dot";

            //Find and replace "->" in string with "--"
            //(assuming input is made from an undirected graphs)

            // IMPORTANT NB:

            // Graphviz expects edges to be written as:

            // '0 -- 1' for undirected graphs and will syntax error on the '->' produced by the library
            
            string modifiedOutputStr = dot.Replace("->", "--");

            File.WriteAllText(output, modifiedOutputStr);
            return output;
        }
    }
}
