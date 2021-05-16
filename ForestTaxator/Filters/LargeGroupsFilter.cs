using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForestTaxator.Model;

namespace ForestTaxator.Filters
{
    public class LargeGroupsFilter : IPointSetFilter
    {
        public float MaxSize { get; set; }

        public LargeGroupsFilter(float maxSize = 0.5f)
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

            return Math.Max(pointSet.BoundingBox.Width, pointSet.BoundingBox.Depth) <= MaxSize;
        }
    }
}