namespace Supercluster.MTree.Design
{
    using System.Collections.Generic;

    public class LeafNode<T> : MNode<T>
    {
        public override bool IsInternalNode => false;

        public override bool IsFull => this.Entries.Count == this.Capacity;

        public new List<LeafNodeEntry<T>> Entries;

        public void Add(T entry, double distance)
        {
            var leafEntry = new LeafNodeEntry<T> { Value = entry, DistanceFromParent = distance };
            this.Entries.Add(leafEntry);
        }
    }
}
