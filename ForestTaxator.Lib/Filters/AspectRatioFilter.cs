using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Model;

namespace ForestTaxator.Filters
{
    public class AspectRatioFilter : IPointSetFilter
    {
        public float LowerRange { get; set; }
        public float UpperRange { get; set; }

        public AspectRatioFilter(float lowerRange = 0.8f, float upperRange = 1.2f)
        {
            LowerRange = lowerRange;
            UpperRange = upperRange;
        }

        private bool AspectRatioWithRange(PointSet pointSet)
        {
            if (pointSet == null || pointSet.Count == 0)
            {
                return false;
            }

            var aspectRatio = pointSet.BoundingBox.Width / pointSet.BoundingBox.Depth;
            return aspectRatio >= LowerRange && aspectRatio <= UpperRange;
        }

        public IList<PointSet> Filter(IList<PointSet> pointSets)
        {
            return pointSets?.Where(AspectRatioWithRange).ToList() ?? new List<PointSet>();
        }
    }
}