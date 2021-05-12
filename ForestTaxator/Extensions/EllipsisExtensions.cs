using ForestTaxator.Data;
using ForestTaxator.Model;

namespace ForestTaxator.Extensions
{
    public static class EllipsisExtensions
    {
        public static void ExportToStream(this Ellipsis ellipsis, ICloudStreamWriter writer, int size = 50, float resolution = 0.01f)
        {
            for (var x = -size; x < size; x++)
            for (var y = -size; y < size; y++)
            {
                var p = new CloudPoint(ellipsis.Center.X + x * resolution, ellipsis.Center.Y + y * resolution, ellipsis.Center.Z)
                {
                    Intensity = 1000
                };

                if (ellipsis.Contains(p))
                {
                    writer.WritePoint(p);
                }
            }
        }
    }
}