using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiObjectiveHS
{
    class Program
    {
        static void Main(string[] args)
        {
            CostOptimizer co = new CostOptimizer();
            co.Solve();
            Console.WriteLine("Press any key to finish ...");
            Console.ReadKey();

        }
    }
}
