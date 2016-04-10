namespace Supercluster.MTree
{
    using System.Collections.Generic;

    using Supercluster.MTree.Design;

    public class MNode<TValue>
    {
        public bool IsInternalNode { get; }

        public MNodeEntry<TValue> ParentEntry;

        public List<MNodeEntry<TValue>> Entries { get; }

        public 7bool IsFull { get; }

        public int Capacity;
    }
}
