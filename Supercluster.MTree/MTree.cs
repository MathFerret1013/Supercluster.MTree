using System;


namespace Supercluster.MTree
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;

    using Supercluster.MTree.Design;

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// 
    /// References:
    /// [1] P. Ciaccia, M. Patella, and P. Zezula. M-tree: an efficient access method for similarity search in metric spaces. 
    /// In Proceedings of the 23rd International Conference on Very Large Data Bases (VLDB), pages 426–435, Athens, Greece, August 1997
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class MTree<T>
    {
        public InternalNode<T> Root = new InternalNode<T>();
        public Func<T, T, double> Metric;

        public void Add(MNode<T> node, T newEntry)
        {
            /*
                Note for my first attempt I will implement that algorithm exactly as described in the paper
                P. Ciaccia, M. Patella, and P. Zezula. M-tree: an efficient access method for similarity search in metric spaces.
                Insert Algorithm
            */

            if (node.IsInternalNode)
            {
                var internalNode = (InternalNode<T>)node;
                var entries = internalNode.Entries;
                // let N_in = entries such that d(O_r, O_n) <= r(O_r)

                // these are the balls in which our newEntry already resides in
                // TODO: Don't double compute the distances
                var entries_in = entries.Where(n => this.Metric(n.Value, newEntry) <= n.radius).ToArray();
                InternalNodeEntry<T> closestEntry;
                if (entries_in.Length > 0) // new entry is currently in the region of a ball
                {
                    // TODO: This is the second computation
                    var elementDistances = entries_in.Select(e => this.Metric(e.Value, newEntry)).ToList();
                    closestEntry = entries_in[elementDistances.IndexOf(elementDistances.Min())];
                }
                else // the new element does not currently reside in any of the current regions balls
                {
                    // since we are not in any of the balls we find which ball we are closest to and extend that ball
                    // we choose the ball whose radius we must increase the least
                    var elementDistances = entries_in.Select(e => this.Metric(e.Value, newEntry) - e.radius).ToList();
                    closestEntry = entries_in[elementDistances.IndexOf(elementDistances.Min())];
                    closestEntry.radius = this.Metric(closestEntry.Value, newEntry);
                }

                // recurse into the closes elements subtree
                this.Add(closestEntry.ChildNode, newEntry);
            }
            else // node is a leaf node
            {
                var leafNode = (LeafNode<T>)node;
                if (!leafNode.IsFull)
                {
                    var dist = this.Metric(leafNode.ParentEntry.Value, newEntry);
                    leafNode.Add(newEntry, dist);
                }
                else
                {
                    this.Split(leafNode, newEntry);
                }
            }

        }


        private void Split<TNode>(TNode node, T newEntryValue) where TNode : MNode<T>
        {
            if (!node.IsInternalNode)
            {
                // add new entry to the entries list
                var entries = new List<LeafNodeEntry<T>>();
                entries.AddRange(node.Entries.Select(e => (LeafNodeEntry<T>)e));
                var newEntry = new LeafNodeEntry<T> { Value = newEntryValue };
                entries.Add(newEntry);

                //if we are not the root, the get the parent of the current node.
                MNodeEntry<T> parentEntry;
                MNode<T> parentNode;
                if (node != this.Root)
                {
                    parentEntry = node.ParentEntry;
                    parentNode = node.ParentEntry.ParentNode;
                }


                var newNode = new InternalNode<T>();
                var promotionIndexes = this.Promote(entries.ToArray()); // TODO: Does not need to be an array
                var promotionObjects = entries.SubsetByIndex(promotionIndexes);
                var partition1 = new List<LeafNodeEntry<T>>();
                var partition2 = new List<LeafNodeEntry<T>>();
                BalancedPartitioning(entries, promotionObjects[0], promotionObjects[1], partition1, partition2);

                node.Entries = partition1;
                newNode.Entries = new List<InternalNodeEntry<T>>();

            }





        }


        /// <summary>
        /// Chooses two <see cref="MNodeEntry{T}"/>s to be promoted up the tree. The two nodes are chosen 
        /// according to the mM_RAD split policy with balanced partitions defined in reference [1] pg. 431.
        /// </summary>
        /// <typeparam name="TNodeEntry">The type pf the node entry.</typeparam>
        /// <param name="entries">The entries for which two node will be choose from.</param>
        /// <param name="isSplitNodeLeaf"></param>
        /// <returns>The indexes of the element pairs which are the two objects to promote</returns>
        public int[] Promote<TNodeEntry>(TNodeEntry[] entries) where TNodeEntry : MNodeEntry<T>
        {
            // Note: We calculate all possible unique pair-wise distance between the points to avoid
            // and further distance calculations.
            // There is a one-to-one correspondence between uniquePairs and uniqueDistances
            var uniquePairs = Utilities.UniquePairs(entries.Length); // we only store the indexes of the pairs
            var uniqueDistances = uniquePairs.Select(p => this.Metric(entries[p.Item1].Value, entries[p.Item2].Value)).Reverse().ToArray();

            // The pair which has the current minimum maximum radius
            var minPair = new Tuple<int, int>(-1, -1);
            var minMaxRadius = double.MaxValue;

            // We iterate through each pair performing a balanced partition
            // of the remaining points.
            foreach (var pair in uniquePairs)
            {
                // Get the indexes of the points not in the current pair
                var pointsNotInPair = Enumerable.Range(0, entries.Length - 1).Except(new[] { pair.Item1, pair.Item2 }).ToList();//TODO: Optimize
                var firstPartDist = double.MinValue;
                var secondPartDist = double.MinValue;

                int k = 0;
                var len = entries.Length - 1;
                while (pointsNotInPair.Count > 0)
                {
                    int minIndex = -1;
                    if (k % 2 == 0)
                    {
                        var dist = double.MaxValue;

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
                    else
                    {
                        var dist = double.MaxValue;

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

                        //var dist = pointsNotInPair.Select(p => this.Metric(entries[p].Value, entries[pair.Item2].Value)).Min();
                        secondPartDist = Math.Max(secondPartDist, dist);
                    }

                    pointsNotInPair.RemoveAt(minIndex);
                    k++;
                }

                var localMinMaxRadius = Math.Max(firstPartDist, secondPartDist);
                if (localMinMaxRadius < minMaxRadius)
                {
                    minMaxRadius = localMinMaxRadius;
                    minPair = pair;
                }

            }

            // TODO: This method is called from the split method. In the split method we call both promote an partition in one method

            return new int[] { minPair.Item1, minPair.Item2 };

        }

        private void BalancedPartitioning<TNodeEntry>(
            IList<TNodeEntry> entries,
            TNodeEntry object1,
            TNodeEntry object2,
            List<TNodeEntry> partition1,
            List<TNodeEntry> partition2) where TNodeEntry : MNodeEntry<T>
        {
            // TODO: This method is called from the split method. In the split method we call both promote an partition in one method
            // TOD): Get rid of this method
            while (entries.Count > 0)
            {
                var firstNearestIndex = entries.Select(e => this.Metric(e.Value, object1.Value)).MinIndex();
                partition1.Add(entries[firstNearestIndex]);
                entries.RemoveAt(firstNearestIndex);

                var secondNearestIndex = entries.Select(e => this.Metric(e.Value, object2.Value)).MinIndex();
                partition2.Add(entries[secondNearestIndex]);
                entries.RemoveAt(secondNearestIndex);
            }
        }
    }
}

