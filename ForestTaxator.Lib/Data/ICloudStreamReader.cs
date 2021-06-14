using System.Collections;
using System.Collections.Generic;
using ForestTaxator.Model;

namespace ForestTaxator.Data
{
    public interface ICloudStreamReader
    {
        CloudPoint ReadPoint();
        PointSet ReadPointSet();
        IEnumerable<PointSlice> ReadPointSlices(float sliceHeight = 0.1f);
        Cloud ReadCloud();
        long Size { get; }
    }
}