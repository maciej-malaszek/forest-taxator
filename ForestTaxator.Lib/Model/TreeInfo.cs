using System.Collections.Generic;

namespace ForestTaxator.Lib.Model
{
    public class TreeInfo
    {
        public List<TreeSliceInfo> SliceInfos { get; set; } = new List<TreeSliceInfo>();
        public double Height { get; set; }
    }

    public class TreeSliceInfo
    {
        public Point Center { get; set; }
        public Point[] EllipseFocis { get; set; }
        public double? EllipseMajorSemiAxis { get; set; }
        public double? EllipseEccentricity { get; set; }
        public string Id { get; set; }
        public string ParentId { get; set; }
    }
}