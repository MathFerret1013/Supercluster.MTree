namespace Supercluster.MTree.Design
{
    public class LeafNode<T> : MNode<T, LeafNodeEntry<T>>
    {
        public override bool IsInternalNode => false;

        public new MNode<T, InternalNodeEntry<T>> Parent;
    }
}
