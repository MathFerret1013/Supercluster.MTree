namespace Workbench
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Supercluster.MTree;
    using Supercluster.MTree.Design;

    class Program
    {
        public static Func<double[], double[], double> L2Norm_Squared_Double = (x, y) =>
        {
            double dist = 0f;
            for (int i = 0; i < x.Length; i++)
            {
                dist += (x[i] - y[i]) * (x[i] - y[i]);
            }

            return dist;
        };

        static void Main(string[] args)
        {
            var mtree = new MTree<double[]>();
            var uniquePairs = Utilities.UniquePairs(4);

            var lns = new List<LeafNodeEntry<double[]>>
                          {
                              new LeafNodeEntry<double[]> { Value = new double[] { 1, 3 } },
                              new LeafNodeEntry<double[]> { Value = new double[] { 2, 1 } },
                              new LeafNodeEntry<double[]> { Value = new double[] { 4, 2 } },
                              new LeafNodeEntry<double[]> { Value = new double[] { 4, 1 } },
                              new LeafNodeEntry<double[]> { Value = new double[] { 3, 4 } }
                          };

            mtree.Metric = L2Norm_Squared_Double;

            mtree.Promote(lns.ToArray(), false);

        }
    }
}

