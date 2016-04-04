namespace Supercluster.MTree
{
    using System.Collections.Generic;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Policy;

    public class InternalNode<T> : IIdentifiableNode
    {
        // May need a point data type as we did in the KDTree
        // May also need to define a TDimension type

        public T Node;


        // NOTE: elements contain references to other internal nodes
        public List<InternalNodeElement<T>> elements;

        public double CoveringRadius;

        public double DistanceFromParent;

        public bool IsInternalNode => true;
    }
}
