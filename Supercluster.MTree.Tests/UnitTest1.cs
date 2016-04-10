using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Supercluster.MTree.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using Supercluster.MTree.Design;

    [TestFixture]
    public class MTreeUnitTests
    {
        [Test]
        public void DistanceMatrixSelectionAlgorithm()
        {

            var entries =
                new List<LeafNodeEntry<double[]>>
                    {
                        new LeafNodeEntry<double[]> { Value = new double[] { 1, 3 } },
                        new LeafNodeEntry<double[]> { Value = new double[] { 20, 134 } },
                        new LeafNodeEntry<double[]> { Value = new double[] { 99, 90 } },
                        new LeafNodeEntry<double[]> { Value = new double[] { 12, -13 } },
                        new LeafNodeEntry<double[]> { Value = new double[] { -23, -45 } }
                    }
                    .ToArray();


            Func<double[], double[], double> Metric = (x, y) =>
            {
                double dist = 0f;
                for (int i = 0; i < x.Length; i++)
                {
                    dist += (x[i] - y[i]) * (x[i] - y[i]);
                }

                return dist;
            };

            // Note: We calculate all possible unique pair-wise distance between the points to avoid
            // and further distance calculations.
            // There is a one-to-one correspondence between uniquePairs and uniqueDistances
            var uniquePairs = Utilities.UniquePairs(entries.Length); // we only store the indexes of the pairs
            var uniqueDistances = uniquePairs.Select(p => Metric(entries[p.Item1].Value, entries[p.Item2].Value)).Reverse().ToArray();

            // The pair which has the current minimum maximum radius
            var minPair = new Tuple<int, int>(-1, -1);
            var minMaxRadius = double.MaxValue;

            var dist = double.MaxValue;
            int minIndex
            // Here we calculate the index of the pair in the unique distances array
            // this calculation depends on the unique distances array being reversed
            for (int i = 0; i < pointsNotInPair.Count; i++)
            {
                var max = Math.Max(pointsNotInPair[i], pair.Item1);
                var min = Math.Min(pointsNotInPair[i], pair.Item1);
                var x = len - min;
                var index = (x * (x + 1) / 2) - (max - min);

                if (uniqueDistances[index] < dist)
                {
                    dist = uniqueDistances[index];
                    minIndex = i;
                }
            }
            // var dist = pointsNotInPair.Select(p => this.Metric(entries[p].Value, entries[pair.Item1].Value)).Min();
            firstPartDist = Math.Max(firstPartDist, dist);
        }
    }
}
