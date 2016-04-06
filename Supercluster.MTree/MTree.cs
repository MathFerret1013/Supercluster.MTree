using System;


namespace Supercluster.MTree
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;

    using Supercluster.MTree.Design;

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


        private void Split(MNode<T> node, T newEntry)
        {

            if (this.Root != node)
            {
                var entries = node.Entries.ToList();

                // if internal node
                // entries.Add();




                var parentEntry = node.ParentEntry;
                var parentNode = node.ParentEntry.ParentNode;
            }
        }


        // Chooses two nodes according to the mM_RAD split policy with balanced partitions.
        public Tuple<int, int> Promote<TNodeEntry>(TNodeEntry[] entries, bool isSplitNodeLeaf) where TNodeEntry : MNodeEntry<T>
        {
            // There is a one-to-one correspondence between uniquePairs and uniqueDistances
            var uniquePairs = Utilities.UniquePairs(entries.Length);
            var uniqueDistances = uniquePairs.Select(p => this.Metric(entries[p.Item1].Value, entries[p.Item2].Value)).ToArray();


            var minPair = new Tuple<int, int>(-1, -1);
            var minMaxRadius = double.MaxValue;

            foreach (var pair in uniquePairs)
            {
                var pointsNotInPair = Enumerable.Range(1, entries.Length).Except(new[] { pair.Item1, pair.Item2 }).ToList();//TODO: Optimize
                var firstPartDist = double.MinValue;
                var secondPartDist = double.MinValue;

                int k = 0;
                while (pointsNotInPair.Count > 0)
                {
                    if (k % 2 == 0)
                    {
                        // TODO: We are computing these distance instead of looking them up. Optimize!
                        var dist = pointsNotInPair.Select(p => this.Metric(entries[p].Value, entries[pair.Item1].Value)).Min();
                        firstPartDist = Math.Max(firstPartDist, dist);
                    }
                    else
                    {
                        // TODO: We are computing these distance instead of looking them up. Optimize!
                        var dist = pointsNotInPair.Select(p => this.Metric(entries[p].Value, entries[pair.Item2].Value)).Min();
                        secondPartDist = Math.Max(secondPartDist, dist);
                    }

                    pointsNotInPair.RemoveAt(pointsNotInPair.Count); ;
                }

                var localMinMaxRadius = Math.Max(firstPartDist, secondPartDist);
                if (localMinMaxRadius < minMaxRadius)
                {
                    minMaxRadius = localMinMaxRadius;
                    minPair = pair;
                }

            }

            return minPair;

        }

        private void BalancedPartitioning(
            IList<MNodeEntry<T>> entries,
            MNodeEntry<T> object1,
            MNodeEntry<T> object2,
            List<MNodeEntry<T>> partition1, // may need to be ref
            List<MNodeEntry<T>> partition2) // may need to be ref
        {
            // TODO: Optimize this code
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

