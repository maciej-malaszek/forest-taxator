using System.Collections.Generic;
using ForestTaxator.Lib.Model;
using ForestTaxator.Lib.Utils;

namespace ForestTaxator.Lib.Extensions
{
    public static class PointSetExtensions
    {
        public static Distribution GetDistribution(this PointSet pointSet, int steps, MathUtils.EDimension dimension)
        {
            var distribution = new int[steps];
            var setSize = pointSet.P2[(int) dimension] - pointSet.P1[(int) dimension];
            var stepSize = setSize / (steps - 1);

            foreach (var cloudPoint in pointSet)
            {
                var index = (int) ((cloudPoint[(int) dimension] + setSize / 2) / stepSize);
                distribution[index]++;
            }

            return new Distribution(distribution);
        }

        public static IList<Distribution> GetDistribution(this PointSet pointSet, int steps, params MathUtils.EDimension[] dimensions)
        {
            var distributions = new Distribution[dimensions.Length];
            for (var i = 0; i < dimensions.Length; i++)
            {
                distributions[i] = pointSet.GetDistribution(steps, dimensions[i]);
            }

            return distributions;
        }
        
    }
}