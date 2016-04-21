namespace Workbench 
{
    using Supercluster.MTree;
    using Supercluster.MTree.NewDesign;
    using Supercluster.MTree.Tests;

    class Program
    {

        static void Main(string[] args)
        {
            var points = new double[][]
                             {
                                 new double[] { 1, 3 }, new double[] { 2, 1 }, new double[] { 4, 2 },
                                 new double[] { 10, 10 }, new double[] { 9, 9 }, new double[] {8,9},   new double[] {9.5,9.5},  new double[] {9.75,9.75},
                             };

            /*
            var points = new double[][]
                 {
                                 new double[] { 1, 1 }, new double[] { 2, 2 }, new double[] { 3, 2 },
                                 new double[] { 4, 1 }
                 };*/


            var distMat = new DistanceMatrix<double[]>(points, Norms.L2Norm_Squared_Double).ToRectangularArray();

            var mtree = new MTree<double[]>();
            mtree.Metric = Norms.L2Norm_Squared_Double;

            foreach (double[] t in points)
            {
                mtree.Add(t);
            }

            // var uniquePairs = Utilities.UniquePairs(4);
            /* var lns =
                 new List<MNodeEntry<double[]>>
                     {
                         new MNodeEntry<double[]> { Value = new double[] { 1, 3 } },
                         new MNodeEntry<double[]> { Value = new double[] { 2, 1 } },
                         new MNodeEntry<double[]> { Value = new double[] { 4, 2 } },
                         new MNodeEntry<double[]> { Value = new double[] { 4, 1 } },
                         new MNodeEntry<double[]> { Value = new double[] { 3, 4 } }
                     }.ToArray();




             mtree.Metric = L2Norm_Squared_Double;
             mtree.Promote(lns);*/

        }
    }
}

