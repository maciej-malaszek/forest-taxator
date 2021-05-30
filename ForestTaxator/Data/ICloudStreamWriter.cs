using ForestTaxator.Model;

namespace ForestTaxator.Data
{
    public interface ICloudStreamWriter
    {
        void WritePoint(CloudPoint point);
        void WritePoint(Point point);
        void WritePointSet(PointSet pointSet);
        void WritePointSetGroup(PointSetGroup pointSetGroup);
    }
}