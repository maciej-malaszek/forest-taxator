using System.Collections.Generic;
using ForestTaxator.Data;

namespace ForestTaxator.Model
{
    public class Cloud
    {
        public PointSet PointSet { get; }

        public Cloud(ICloudStreamReader streamReader)
        {
            PointSet = streamReader.ReadPointSet();
            PointSet.RecalculateBoundingBox();
        }
        public Cloud(PointSet pointSet)
        {
            PointSet = pointSet;
            PointSet.RecalculateBoundingBox();
        }
        public IList<PointSlice> Slice(float height = 0.1f)
        {
            return PointSet.SplitByHeight(PointSet.BoundingBox, height);
        }
    }
}