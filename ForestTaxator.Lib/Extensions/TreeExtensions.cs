using ForestTaxator.Lib.Data;
using ForestTaxator.Lib.Model;

namespace ForestTaxator.Lib.Extensions
{
    public static class TreeExtensions
    {
        public static void ExportToStream(this Tree tree, ICloudStreamWriter writer)
        {
            var nodes = tree.GetAllNodesAsVector();
            foreach (var node in nodes)
            {
                node?.Ellipse?.ExportToStream(writer);
            }
        }
    }
}