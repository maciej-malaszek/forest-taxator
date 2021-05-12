using ForestTaxator.Model;

namespace ForestTaxator.Data
{
    public interface ICloudStreamReader
    {
        CloudPoint ReadPoint();
        PointSet ReadPointSet();
        long Size { get; }
    }
}