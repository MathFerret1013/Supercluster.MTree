
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supercluster.MTree.Tests
{
    public class Utilities
    {
        public static double[][] GenerateDoubles(int points, double range)
        {
            var data = new List<double[]>();
            var random = new Random();

            for (int i = 0; i < points; i++)
            {
                data.Add(new double[] { (random.NextDouble() * range), (random.NextDouble() * range) });
            }

            return data.ToArray();
        }
    }
}
