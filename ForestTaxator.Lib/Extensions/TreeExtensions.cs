using ForestTaxator.Data;
using ForestTaxator.Model;

namespace ForestTaxator.Extensions
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