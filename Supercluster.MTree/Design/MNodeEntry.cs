namespace Supercluster.MTree.Design
{
    /// <summary>
    /// An abstract base class containing the shared properties of both<see cref="InternalNode{T}"/> entries
    /// and <see cref="LeafNode{T}"/> entries.
    /// </summary>
    /// <typeparam name="T">The type of the Values stored in the MTree.</typeparam>
    public abstract class MNodeEntry<T>
    {
        /// <summary>
        /// The value contained in the node entry.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// THe distance from the parent routing object.
        /// </summary>
        public double DistanceFromParent { get; set; }

        /// <summary>
        /// The node which contains this entry. Note this is a reference to a node, not a node-entry.
        /// </summary>
        public MNode<T> ParentNode { get; set; }
    }
}
