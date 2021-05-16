using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForestTaxator.Model;

namespace ForestTaxator.Filters
{
    public class SmallGroupsFilter : IPointSetFilter
    {
        public float MinimalSize { get; set; }
        public SmallGroupsFilter(float minimalSize = 0.05f)
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

            return Math.Max(pointSet.BoundingBox.Width, pointSet.BoundingBox.Depth) >= MinimalSize;
        }
    }
}