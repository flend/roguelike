using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphMap;
using RogueBasin;

namespace GraphMapStressTester
{
    public class GraphGenerator
    {
        private Random rand;

        public GraphGenerator(Random rand)
        {
            this.rand = rand;
        }

        public ConnectivityMap GenerateConnectivityMapNoCycles(int numberNodes, double branchingRatio)
        {
            var map = new ConnectivityMap();

            var terminalNodes = new HashSet<int> { 0, 1 };
            var nextNodeIndex = 2;

            map.AddRoomConnection(0, 1);

            while(nextNodeIndex < numberNodes) {
                var randomChance = rand.NextDouble();
                
                int sourceVertex;

                if(randomChance < branchingRatio) {
                    sourceVertex = rand.Next(nextNodeIndex);
                }
                else {
                    sourceVertex = terminalNodes.RandomElementUsing(rand);
                }

                map.AddRoomConnection(sourceVertex, nextNodeIndex);
                terminalNodes.Remove(sourceVertex);
                terminalNodes.Add(nextNodeIndex);
                nextNodeIndex++;
            }

            return map;
        }

    }


}
