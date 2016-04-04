namespace Supercluster.MTree

{
    using System.Collections.Generic;

    public class LeafNode<T> : IIdentifiableNode
    {
        public List<T> Nodes;
        public List<double> Distances;

        public bool IsInternalNode => false;

        public int Capacity;

        public bool IsFull => this.Nodes.Count == this.Capacity;

        // TODO: Distance needs to be added
        public void Add(T entry)//, double distance)
        {
            this.Nodes.Add(entry);
            // this.Distances.Add(distance);
        }
    }
}