using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Lib.Model;

namespace ForestTaxator.Lib.Filters
{
    public class TreeHeightFilter : ITreeFilter
    {
        public double MinimalTreeHeight { get; set; }
        public IList<Tree> Filter(IList<Tree> potentialTrees)
        {
            return potentialTrees.Where(tree => tree.GetHighestNode().Center.Z - tree.Root.Center.Z >= MinimalTreeHeight).ToList();
        }
    }
}