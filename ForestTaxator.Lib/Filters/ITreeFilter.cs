using System.Collections.Generic;
using ForestTaxator.Model;

namespace ForestTaxator.Filters
{
    public interface ITreeFilter
    {
        IList<Tree> Filter(IList<Tree> potentialTrees);
    }
}