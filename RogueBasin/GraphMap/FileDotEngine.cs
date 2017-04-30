using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
using System.IO;

namespace GraphMap
{
    /// <summary>
    /// Default dot engine implementation, writes dot code to disk
    /// </summary>
    public sealed class FileDotEngine : IDotEngine
    {
        public string Run(GraphvizImageType imageType, string dot, string outputFileName)
        {
            string output = outputFileName + ".dot";
            File.WriteAllText(output, dot);
            return output;
        }
    }
}
