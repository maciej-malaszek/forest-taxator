using System;
using ForestTaxator.Model;

namespace ForestTaxator.Data
{
    public interface ICloudStreamWriter : IDisposable
    {
        void WritePoint(CloudPoint point);
        void WritePoint(Point point);
        void WritePointSet(PointSet pointSet);
        void WritePointSetGroup(IPointSetGroup pointSetGroup);
    }
}