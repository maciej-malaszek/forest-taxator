using System;
using ForestTaxator.Lib.Model;

namespace ForestTaxator.Lib.Data
{
    public interface ICloudStreamWriter : IDisposable
    {
        void WritePoint(CloudPoint point);
        void WritePoint(Point point);
        void WritePointSet(PointSet pointSet);
        void WritePointSetGroup(IPointSetGroup pointSetGroup);
    }
}