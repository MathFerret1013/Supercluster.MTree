namespace Supercluster.MTree.Tests
{
    using System;

    public static class Metrics
    {
        public static Func<double[], double[], double> L2Norm_Double = (x, y) =>
        {
            double dist = 0;
            for (int i = 0; i < x.Length; i++)
            {
                dist += (x[i] - y[i]) * (x[i] - y[i]);
            }

            return Math.Sqrt(dist);
        };

    }
}
