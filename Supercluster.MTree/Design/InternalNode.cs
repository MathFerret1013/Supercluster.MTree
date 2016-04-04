using System.Collections.Generic;

namespace Supercluster.MTree.Design
{
    public class InternalNode<T> : MNode<T, InternalNodeEntry<T>>
    {
        public override bool IsInternalNode => true;
    }
}
