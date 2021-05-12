using System;
using ForestTaxator.Model;

namespace ForestTaxator.Filters
{
    public class LargeGroupsFilter
    {
        public float MaxSize { get; set; }

        public LargeGroupsFilter(float maxSize = 0.5f)
        {
            MaxSize = maxSize;
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

                if (Math.Max(groups[j].BoundingBox.Width, groups[j].BoundingBox.Depth) <= MaxSize)
                {
                    continue;
                }

                Console.WriteLine($"Large Group nullify {groups[j].BoundingBox.Width} x {groups[j].BoundingBox.Depth}");
                groups[j] = null;
            }
        }
    }
}