using System;

namespace Supercluster.MTree.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;
    using Supercluster.MTree.NewDesign;

    [TestFixture]
    public class MTreeUnitTests
    {
        /// <summary>
        /// Test a specific build of a two dimensional M-Tree with real valued coordinates.
        /// Test distance, properties, and placements of nodes.
        /// </summary>
        [Test]
        public void MtreeBuildTest()
        {
            var points = new double[][]
                  {
                      new double[] { 1, 3 },
                      new double[] { 2, 1 },
                      new double[] { 4, 2 },
                      new double[] { 10, 10 },
                      new double[] { 9, 9 },
                      new double[] { 8, 9 },
                      new double[] { 9.5, 9.5 },
                      new double[] { 9.75, 9.75 }
                  };

            var mtree = new MTree<double[]> { Capacity = 3, Metric = Norms.L2Norm_Squared_Double };
            foreach (var point in points)
            {
                mtree.Add(point);
            }

            /*
                1. Ensure all node values are in the right place
            */

            /*

            The M-Tree node entry values should look like the following:

                                        +-------------+
                                        |(1,3),(10,10)|
                                        +--+------+---+
                                           |      |
                           +---------------+      +---------------+
                           |                                      |
                     +-----+-----+                        +-------+-----+
                     |(1,3),(4,2)|                        |(10,10),(9,9)|
                     +--+-----+--+                        +---+------+--+
                        |     |                               |      |
                 +------+     +---+                  +--------+      +-----+
                 |                |                  |                     |
            +----+------+    +----+------+   +-------+-----------+   +-----+---------+
            |(1,3),(2,1)|    |(4,2),(8,9)|   |(10,10),(9.75,9.75)|   |(9,9),(9.5,9.5)|
            +-----------+    ------------+   +-------------------+   +---------------+

            */

            var rootEntries = mtree.Root.Entries;
            var middleEntries = rootEntries[0].ChildNode.Entries.Concat(rootEntries[1].ChildNode.Entries).ToArray();
            var leafEntries = new List<MNodeEntry<double[]>>();
            foreach (var entry in middleEntries)
            {
                leafEntries.AddRange(entry.ChildNode.Entries);
            }

            // Check node entry values

            // Test Root entries
            Assert.That(rootEntries[0].Value, Is.EqualTo(points[0]));
            Assert.That(rootEntries[1].Value, Is.EqualTo(points[3]));

            // Test mid entries
            Assert.That(middleEntries[0].Value, Is.EqualTo(points[0]));
            Assert.That(middleEntries[1].Value, Is.EqualTo(points[2]));
            Assert.That(middleEntries[2].Value, Is.EqualTo(points[3]));
            Assert.That(middleEntries[3].Value, Is.EqualTo(points[4]));

            // Test leaf entries
            Assert.That(leafEntries[0].Value, Is.EqualTo(points[0]));
            Assert.That(leafEntries[1].Value, Is.EqualTo(points[1]));
            Assert.That(leafEntries[2].Value, Is.EqualTo(points[2]));
            Assert.That(leafEntries[3].Value, Is.EqualTo(points[5]));
            Assert.That(leafEntries[4].Value, Is.EqualTo(points[3]));
            Assert.That(leafEntries[5].Value, Is.EqualTo(points[7]));
            Assert.That(leafEntries[6].Value, Is.EqualTo(points[4]));
            Assert.That(leafEntries[7].Value, Is.EqualTo(points[6]));

            /*
                2. Ensure all node properties are correct at each level
            */

            // Ensure that root node has no parent entry
            Assert.That(mtree.Root.ParentEntry, Is.Null);

            // Check node entry properties
            foreach (var entry in rootEntries)
            {
                Assert.That(entry.ParentNode, Is.EqualTo(mtree.Root));
                Assert.That(entry.ChildNode, Is.Not.Null);
                Assert.That(entry.DistanceFromParent, Is.EqualTo(-1));
                Assert.That(entry.CoveringRadius, Is.GreaterThan(0));
                Assert.That(entry.ParentNode.IsInternalNode, Is.True);
            }

            foreach (var entry in middleEntries)
            {
                Assert.That(entry.ParentNode.ParentEntry.ParentNode, Is.EqualTo(mtree.Root));
                Assert.That(entry.ChildNode, Is.Not.Null);
                Assert.That(entry.DistanceFromParent, Is.GreaterThanOrEqualTo(0));
                Assert.That(entry.CoveringRadius, Is.GreaterThan(0));
                Assert.That(entry.ParentNode.IsInternalNode, Is.True);
            }

            foreach (var entry in leafEntries)
            {
                Assert.That(entry.ParentNode.ParentEntry.ParentNode.ParentEntry.ParentNode, Is.EqualTo(mtree.Root));
                Assert.That(entry.ChildNode, Is.Null);
                Assert.That(entry.DistanceFromParent, Is.GreaterThanOrEqualTo(0));
                Assert.That(entry.CoveringRadius, Is.EqualTo(-1));
                Assert.That(entry.ParentNode.IsInternalNode, Is.False);
            }

            /*
                3. Ensure all Distance from parents are correct
                Root entries have a distance from parent of -1, so they are not checked here
            */
            var distanceMatrix = new DistanceMatrix<double[]>(points, Norms.L2Norm_Squared_Double);
            Assert.That(middleEntries[0].DistanceFromParent, Is.EqualTo(distanceMatrix[0, 0]));
            Assert.That(middleEntries[1].DistanceFromParent, Is.EqualTo(distanceMatrix[0, 2]));
            Assert.That(middleEntries[2].DistanceFromParent, Is.EqualTo(distanceMatrix[3, 3]));
            Assert.That(middleEntries[3].DistanceFromParent, Is.EqualTo(distanceMatrix[3, 4]));

            Assert.That(leafEntries[0].DistanceFromParent, Is.EqualTo(distanceMatrix[0, 0]));
            Assert.That(leafEntries[1].DistanceFromParent, Is.EqualTo(distanceMatrix[0, 1]));
            Assert.That(leafEntries[2].DistanceFromParent, Is.EqualTo(distanceMatrix[2, 2]));
            Assert.That(leafEntries[3].DistanceFromParent, Is.EqualTo(distanceMatrix[2, 5]));
            Assert.That(leafEntries[4].DistanceFromParent, Is.EqualTo(distanceMatrix[3, 3]));
            Assert.That(leafEntries[5].DistanceFromParent, Is.EqualTo(distanceMatrix[3, 7]));
            Assert.That(leafEntries[6].DistanceFromParent, Is.EqualTo(distanceMatrix[4, 4]));
            Assert.That(leafEntries[7].DistanceFromParent, Is.EqualTo(distanceMatrix[4, 6]));
        }


        /// <summary>
        /// Unit test to ensure the correct distances between points of the computed by the distance matrix.
        /// </summary>
        [Test]
        public void DistanceMatrixTest()
        {
            var points = new double[][]
                             {
                                 new double[] { 1, 2 },
                                 new double[] { 4.8363, 2.93732 },
                                 new double[] { 2.974 , 38.098 },
                                 new double[] { 1, 2 },
                                 new double[] { 10, 3.5 },
                                 new double[] { Math.PI, Math.E }
                             };

            var distMatrix = new DistanceMatrix<double[]>(points, Norms.L2Norm_Squared_Double);

            for (int i = 0; i < points.Length; i++)
            {
                for (int j = 0; j < points.Length; j++)
                {
                    Assert.That(Norms.L2Norm_Squared_Double(points[i], points[j]), Is.EqualTo(distMatrix[i, j]));
                }
            }
        }
    }
}
