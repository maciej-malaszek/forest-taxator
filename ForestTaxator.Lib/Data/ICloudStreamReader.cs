using System;
using System.Collections.Generic;
using ForestTaxator.Lib.Model;

namespace ForestTaxator.Lib.Data
{
    public interface ICloudStreamReader : IDisposable
    {
        CloudPoint ReadPoint();
        PointSet ReadPointSet();
        IEnumerable<PointSlice> ReadPointSlices(float sliceHeight = 0.1f);
        Cloud ReadCloud();
        long Size { get; }
    }
}