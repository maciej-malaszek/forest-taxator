using System.Collections.Generic;
using ForestTaxator.Lib.Model;

namespace ForestTaxator.Lib.Filters
{
    public interface IPointSetFilter
    {
        IList<PointSet> Filter(IList<PointSet> pointSets);
    }
}