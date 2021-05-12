using System;
using ForestTaxator.Model;

namespace ForestTaxator.Filters
{
    public class SmallGroupsFilter
    {
        public float MinimalSize { get; set; }
        public SmallGroupsFilter(float minimalSize = 0.05f)
        {
            MinimalSize = minimalSize;
        }
        public void Filter(PointSet[] groups)
        {
            if(groups == null)
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

                if (Math.Min(groups[j].BoundingBox.Width, groups[j].BoundingBox.Depth) >= MinimalSize)
                {
                    continue;
                }

                Console.WriteLine($"Small Group Nullify {groups[j].BoundingBox.Width} x {groups[j].BoundingBox.Depth}");
                groups[j] = null;
            }
        }
    }
}