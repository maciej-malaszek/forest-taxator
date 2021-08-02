using System;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Lib.Model;

namespace ForestTaxator.Lib.Filters
{
    public class SmallGroupsFilter : IPointSetFilter
    {
        public Func<double, double> MinimalSize { get; }
        public SmallGroupsFilter(Func<double, double> minimalSize)
        {
            MinimalSize = minimalSize;
        }
        public IList<PointSet> Filter(IList<PointSet> pointSets)
        {
            return pointSets?.Where(ConditionFulfilled).ToList() ?? new List<PointSet>();
        }

        private bool ConditionFulfilled(PointSet pointSet)
        {
            if ((pointSet?.Count ?? 0) <= 0)
            {
                return false;
            }

            return Math.Max(pointSet.BoundingBox.Width, pointSet.BoundingBox.Depth) >= MinimalSize(pointSet.Center.Z);
        }
    }
}