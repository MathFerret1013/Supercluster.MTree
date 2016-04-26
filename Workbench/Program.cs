namespace Workbench
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using Supercluster.MTree;
    using Supercluster.MTree.NewDesign;
    using Supercluster.MTree.Tests;

    using Utilities = Supercluster.MTree.Tests.Utilities;

    class Program
    {
        static void FindInvalidDataSet()
        {
            var testPoint = new double[] { 4, 4 };
            // var radius = 5;

            for (int i = 0; i < 10000; i++)
            {
                var dataSize = 10;
                var testDataSize = 1;
                var range = 10;
                var radius = 5;

                var treeData = Supercluster.MTree.Tests.Utilities.GenerateDoubles(dataSize, range).Select(d => new[] { Math.Floor(d[0]), Math.Floor(d[1]) }).ToArray();
                var testData = Supercluster.MTree.Tests.Utilities.GenerateDoubles(testDataSize, range).Select(d => new[] { Math.Floor(d[0]), Math.Floor(d[1]) }).ToArray();
                var tree = new MTree<double[]>();
                tree.Metric = Metrics.L2Norm_Double;

                // build tree
                foreach (var point in treeData)
                {
                    tree.Add(point);
                }

                // perform searches
                var resultsList = new List<double[]>();
                tree.RangeSearch(tree.Root, testData[0], radius, resultsList);

                var linearResults = new List<double[]>();
                foreach (var point in treeData)
                {
                    if (Metrics.L2Norm_Double(point, testData[0]) <= radius)
                    {
                        linearResults.Add(point);
                    }
                }

                // sort results
                var sortedTreeResults = resultsList.OrderBy(r => r[0]).ThenBy(r => r[1]).ToArray();
                var sortedLinearResults = linearResults.OrderBy(r => r[0]).ThenBy(r => r[1]).ToArray();

                if (sortedTreeResults.Length != sortedLinearResults.Length)
                {
                    Console.WriteLine("Gotcha!");
                }
            }
        }


        static void Main(string[] args)
        {


        }

    }
}

