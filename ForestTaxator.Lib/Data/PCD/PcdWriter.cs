using System;
using ForestTaxator.Model;

namespace ForestTaxator.Data.PCD
{
    public class PcdWriter : ICloudStreamWriter
    {
        public PcdWriter(string path)
        {
            
        }
        public void WritePoint(CloudPoint point)
        {
            throw new NotImplementedException();
        }

        public void WritePoint(Point point)
        {
            throw new NotImplementedException();
        }

        public void WritePointSet(PointSet pointSet)
        {
            throw new NotImplementedException();
        }

        public void WritePointSetGroup(IPointSetGroup pointSetGroup)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}