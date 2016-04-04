using System;


namespace Supercluster.MTree
{
    using System.Linq;
    using System.Runtime.Remoting.Channels;

    public class MTree<T>
    {
        public InternalNode<T> Root = new InternalNode<T>();


        public Func<T, T, double> Metric;


        public void Add(IIdentifiableNode node, T newEntry)
        {

            /*
                Note for my first attempt I will implement that algorithm exactly as described in the paper
                P. Ciaccia, M. Patella, and P. Zezula. M-tree: an efficient access method for similarity search in metric spaces.
                Insert Algorithm
            */

            if (node.IsInternalNode)
            {

                var internalNode = (InternalNode<T>)node;
                var entries = internalNode.elements;
                // let N_in = entries such that d(O_r, O_n) <= r(O_r)

                // these are the balls in which our newEntry already resides in
                // TODO: Don't double compute the distances
                var entries_in = entries.Where(n => this.Metric(n.value, newEntry) <= n.radius).ToArray();
                InternalNodeElement<T> closestElement;
                if (entries_in.Length > 0) // new entry is currently in the region of a ball
                {
                    // TODO: This is the second computation
                    var elementDistances = entries_in.Select(e => this.Metric(e.value, newEntry)).ToList();
                    closestElement = entries_in[elementDistances.IndexOf(elementDistances.Min())];
                }
                else // the new element does not currently reside in any of the current regions balls
                {
                    // since we are not in any of the balls we find which ball we are closest to and extend that ball
                    // we choose the ball whose radius we must increase the least
                    var elementDistances = entries_in.Select(e => this.Metric(e.value, newEntry) - e.radius).ToList();
                    closestElement = entries_in[elementDistances.IndexOf(elementDistances.Min())];
                    closestElement.radius = this.Metric(closestElement.value, newEntry);
                }

                // recurse into the closes elements subtree
                this.Add(closestElement.ChildNode, newEntry);
            }
            else // node is a leaf node
            {
                var leafNode = (LeafNode<T>)node;
                if (!leafNode.IsFull)
                {
                    leafNode.Add(newEntry);
                }
                else
                {
                    this.Split(leafNode, newEntry);
                }
            }

        }

        private void Split(IIdentifiableNode node, T newEntry)
        {

            if (this.Root != node)
            {

            }
        }
    }
}
