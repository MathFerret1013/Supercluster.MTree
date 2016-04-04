namespace Supercluster.MTree.Design
{
    public abstract class MNodeEntry<T>
    {
        public T Value;

        /// <summary>
        /// Note: This is not the node containing the entry. Is the parent of the node containing this entry.
        /// </summary>
        public MNode<T, InternalNodeEntry<T>> ParentNode;

        public double DistanceFromParent;

    }
}
