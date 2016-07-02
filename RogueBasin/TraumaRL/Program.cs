using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL
{
    class Program
    {
        static void Main(string[] args)
        {
            var traumaRunner = new TraumaRunner();

            traumaRunner.TemplatedMapTest();
        }
    }
}
