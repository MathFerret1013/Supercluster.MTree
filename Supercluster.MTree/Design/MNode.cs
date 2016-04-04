namespace Supercluster.MTree
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using Supercluster.MTree.Design;

    public abstract class MNode<TValue, TNodeEntry> where TNodeEntry : MNodeEntry<TValue>
    {
        public abstract bool IsInternalNode { get; }

        public MNode<TValue, TNodeEntry> Parent;

        public List<TNodeEntry> Entries;

        public bool IsFull => this.Entries.Count == this.Capacity;

        public int Capacity;
    }
}
