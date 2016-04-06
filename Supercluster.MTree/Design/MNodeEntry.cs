namespace Supercluster.MTree.Design
{
    public abstract class MNodeEntry<T>
    {
        public T Value { get; set; }

        public double DistanceFromParent { get; set; }

        /// <summary>
        /// THe node which contains this entry.
        /// </summary>
        public MNode<T> ParentNode { get; set; }
    }
}
