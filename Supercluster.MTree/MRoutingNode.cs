namespace Supercluster.MTree
{
    public class MRoutingNode<T>
    {
        // May need a point data type as we did in the KDTree
        // May also need to define a TDimension type

        public T Node;

        public MRoutingNode<T> CoveringTreeRoot;

        public double CoveringRadius;

        public double DistanceFromParent;
    }


    public class MLeafNode<T>
    {
        // May need a point data type as we did in the KDTree
        // May also need to define a TDimension type

        public T Node;

        public double DistanceFromParent;
    }
}
