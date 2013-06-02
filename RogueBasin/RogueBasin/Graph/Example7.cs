using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;
using System.Windows.Forms;
using QuickGraph.Graphviz;
using QuickGraph.Algorithms.Search;

namespace graphtestc
{
    class Example7
    {
        private UndirectedGraph<int, TaggedEdge<int, string>> g;

        public Example7()
        {

        }

        public void OutputGraph()
        {
            BuildMap();
            ProcessMap();
        }

        private void vertex(int vertex)
        {

            Console.WriteLine(vertex.ToString());
        }

        private void ProcessMap()
        {
            try
            {
                Console.WriteLine("===EXAMPLE 7===");

                //Build acyclic version of map
                MapModel mapModel = new MapModel(g);
                mapModel.StartVertex = 1;

 
                /* 
                var doorDependencyGraph = new AdjacencyGraph<int, TaggedEdge<int,string>>();

                doorDependencyGraph.AddVerticesAndEdge(new TaggedEdge<int, string>(0, 1, ""));
                doorDependencyGraph.AddVerticesAndEdge(new TaggedEdge<int, string>(1, 2, ""));
                doorDependencyGraph.AddVerticesAndEdge(new TaggedEdge<int, string>(2, 3, ""));

                var doorDependencyGraph2 = new AdjacencyGraph<int, Edge<int>>();

                doorDependencyGraph2.AddVerticesAndEdge(new Edge<int>(0, 1));
                doorDependencyGraph2.AddVerticesAndEdge(new Edge<int>(1, 2));
                doorDependencyGraph2.AddVerticesAndEdge(new Edge<int>(2, 3));

                 var dfs = new DepthFirstSearchAlgorithm<int, Edge<int>>(doorDependencyGraph2);

                dfs.DiscoverVertex += vertex;

                dfs.Compute(2);
                */

                mapModel.EliminateCyclesInMap();


                //Example that generates interesting dependency trees

                //Place a red door that blocks off half the map

                mapModel.LockEdgeRandomClue(mapModel.GetReducedMapEdge(10, 11), "red");

                //Green blocks off the other half, likely to have red clue behind it
                //(so red clue depends on green clue)

                mapModel.LockEdgeRandomClue(mapModel.GetReducedMapEdge(2, 3), "green");

                //Blue door is behind green door
                //Blue will depends on green clue
                //If blue also locks the red clue, then the red clue will depend on the blue clue

                mapModel.LockEdgeRandomClue(mapModel.GetReducedMapEdge(6, 16), "blue");
                

                /*
                //Example that requires the dependency tree
                //TODO: refactor placedoor&clue to updatedependencygraph
                //Place a red door that blocks off half the map, and the clue at the end of the other branch

                mapModel.PlaceDoorAndClue(mapModel.GetReducedMapEdge(10, 11), "red", 18);

                //Lock the the red clue with a green door, and put the green clue in a nearby branch

                mapModel.PlaceDoorAndClue(mapModel.GetReducedMapEdge(16, 17), "green", 20);

                //Lock the green clue branch with a blue door. We should not be able to place the blue clue behind the red door

                mapModel.LockEdgeRandomClue(mapModel.GetReducedMapEdge(19, 20), "blue");
                */

                //Before and after reduction graphs

                GraphvizExport.OutputUndirectedGraph(g, "example7-base");
                GraphvizExport.OutputUndirectedGraph(mapModel.GraphNoCycles, "example7-reduced");

                //Graphs with doors and clues

                DoorClueGraphvizExport visualiser = new DoorClueGraphvizExport(mapModel);
                visualiser.OutputUndirectedGraph("example7-door");
                visualiser.OutputDoorDependencyGraph("example7-dep");

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }


        private void BuildMap()
        {
            //Build a graph with one nested cycle

            g = new UndirectedGraph<int, TaggedEdge<int, string>>();
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(1, 2, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(2, 3, ""));

            //cycle
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(3, 4, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(3, 5, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(4, 5, ""));

            g.AddVerticesAndEdge(new TaggedEdge<int, string>(5, 6, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(6, 16, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(16, 17, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(17, 18, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(6, 19, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(19, 20, ""));

            //cycle
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(2, 8, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(8, 9, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(7, 8, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(2, 7, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(7, 9, ""));

            g.AddVerticesAndEdge(new TaggedEdge<int, string>(9, 10, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(10, 11, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(11, 12, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(11, 13, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(13, 14, ""));
            g.AddVerticesAndEdge(new TaggedEdge<int, string>(13, 15, ""));
        }

        private void graphviz_FormatEdge(object sender, FormatEdgeEventArgs<int, TaggedEdge<int, string>> e)
        {
            //Take tag from TaggedEdge and add as label on edge in serialized view

            TaggedEdge<int, string> edge = e.Edge;
            var edgeFormattor = e.EdgeFormatter;

            string edgeTag = edge.Tag;
            edgeFormattor.Label = new QuickGraph.Graphviz.Dot.GraphvizEdgeLabel();
            edgeFormattor.Label.Value = edgeTag;

        }

        private void graphviz_FormatVertex(object sender, FormatVertexEventArgs<int> e)
        {
            //(If nothing is included here, having this here allows default behaviour for serializer (add labels))

            //Use formattor explicitally
            var vertexFormattor = e.VertexFormatter;
            int vertexNo = e.Vertex;

            vertexFormattor.Label = String.Format("{0}", vertexNo);
        }


    }
}
