using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supercluster.MTree
{
    public class InternalNodeElement<T>
    {
        public double radius;

        public double parentDistance;

        public T value;

        public IIdentifiableNode ChildNode;
    }
}
