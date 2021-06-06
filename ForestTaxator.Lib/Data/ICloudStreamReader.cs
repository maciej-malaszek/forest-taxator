using ForestTaxator.Model;

namespace ForestTaxator.Data
{
    public interface ICloudStreamReader
    {
        CloudPoint ReadPoint();
        PointSet ReadPointSet();
        Cloud ReadCloud();
        long Size { get; }
    }
}