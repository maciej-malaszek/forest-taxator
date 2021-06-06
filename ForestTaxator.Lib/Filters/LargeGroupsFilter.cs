using System;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Model;

namespace ForestTaxator.Filters
{
    public class LargeGroupsFilter : IPointSetFilter
    {
        public Func<double, double> MaxSize { get; }

        public LargeGroupsFilter(Func<double, double> maxSize)
        {
            MaxSize = maxSize;
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

            return Math.Max(pointSet.BoundingBox.Width, pointSet.BoundingBox.Depth) <= MaxSize(pointSet.Center.Z);
        }
    }
}