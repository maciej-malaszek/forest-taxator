using System.Collections.Generic;
using ForestTaxator.Lib.Model;

namespace ForestTaxator.Lib.Filters
{
    public interface ITreeFilter
    {
        IList<Tree> Filter(IList<Tree> potentialTrees);
    }
}