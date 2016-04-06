using System.Collections.Generic;

namespace Supercluster.MTree.Design
{
    public class InternalNode<T> : MNode<T>
    {
        public override bool IsInternalNode => true;

        public new List<InternalNodeEntry<T>> Entries;

        public override bool IsFull => this.Entries.Count == this.Capacity;
    }
}
