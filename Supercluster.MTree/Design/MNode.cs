namespace Supercluster.MTree
{
    using System.Collections.Generic;

    using Supercluster.MTree.Design;

    public abstract class MNode<TValue>
    {
        public abstract bool IsInternalNode { get; }

        public MNodeEntry<TValue> ParentEntry;

        public List<MNodeEntry<TValue>> Entries { get; }

        public abstract bool IsFull { get; }

        public int Capacity;
    }
}
