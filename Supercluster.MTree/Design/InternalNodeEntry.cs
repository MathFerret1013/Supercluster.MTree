﻿namespace Supercluster.MTree.Design
{
    public class InternalNodeEntry<T> : MNodeEntry<T>
    {
        public double radius;

        public MNode<T, MNodeEntry<T>> ChildNode;
    }
}