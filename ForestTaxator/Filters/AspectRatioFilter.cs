using System;
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

        public void Filter(PointSet[] groups)
        {
            if (groups == null)
            {
                return;
            }

            for (var j = 0; j < groups.Length; j++)
            {
                if (groups[j] == null || groups[j].Count == 0)
                {
                    groups[j] = null;
                    continue;
                }

                var aspectRatio = groups[j].BoundingBox.Width / groups[j].BoundingBox.Depth;
                if (aspectRatio >= LowerRange && aspectRatio <= UpperRange)
                {
                    continue;
                }

                groups[j] = null;
                Console.WriteLine($"Aspect Ration Nullify {aspectRatio}");
            }
        }
    }
}